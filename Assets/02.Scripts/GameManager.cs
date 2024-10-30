using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임을 관리하는 매니저 클래스
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject container = new GameObject("GameManager");
                _instance = container.AddComponent<GameManager>();
            }

            return _instance;
        }
    }

    private UIMain _ui;
    private Client _client;
    private Dictionary<int, Player> _playerDic =
        new Dictionary<int, Player>();             // UID, 플레이어 캐릭터
    private Player _localPlayer;                   // 로컬 플레이어 캐릭터
    private Dictionary<int, Bullet> _bulletDic = new Dictionary<int, Bullet>(); // UID, 총알
    private bool _startGame;                                // 게임이 시작되었는지 여부
    private float _playTime = 0f;

    //! 나중에 입력 받아서 플레이타임 결정하기
    public float InputPlayTime = 270f;

    
    public int redPoint;
    public int bluePoint;

    public int UserUID { get; set; }    // 클라이언트 자신의 UID
    public string UserID { get; set; }  // 클라이언트 자신의 ID
    public bool IsHost { get; set; }
    public bool IsGameStarted { get; set; }
    public Client client => _client;

    private void Start()
    {
        _ui = FindObjectOfType<UIMain>();
        _client = FindObjectOfType<Client>();
    }

    private void Update()
    {
        //UpdateInput();
        UpdateCheckGameEnd();
    }

    // private void UpdateInput()
    // {
    //     if (_localPlayer == null || !_localPlayer.IsAlive)
    //         return;

    //     if (Input.GetKey(KeyCode.W))
    //     {
    //         _localPlayer.Move(Vector3.forward);
    //     }
    //     if (Input.GetKey(KeyCode.S))
    //     {
    //         _localPlayer.Move(Vector3.back);
    //     }
    //     if (Input.GetKey(KeyCode.A))
    //     {
    //         _localPlayer.Move(Vector3.left);
    //     }
    //     if (Input.GetKey(KeyCode.D))
    //     {
    //         _localPlayer.Move(Vector3.right);
    //     }

    //     // 마우스 방향으로 캐릭터를 회전
    //     Vector3 screenPos = Camera.main.WorldToScreenPoint(_localPlayer.transform.position);
    //     Vector3 dir = Input.mousePosition - screenPos;
    //     dir = new Vector3(dir.x, 0f, dir.y);
    //     _localPlayer.transform.forward = dir.normalized;

    //     // 총알 발사
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         _localPlayer.FireBullet();
    //     }

    // }

    private void UpdateCheckGameEnd()
    {
        if (!_startGame || !IsHost)
            return;
        if (_playerDic.Count <= 1)
            return;

        if (_playTime >= InputPlayTime)
        {
            // 게임 종료 처리
            Host host = FindObjectOfType<Host>();
            if (host != null)
            {
                PacketGameEnd packet = new PacketGameEnd();

                foreach (var player in _playerDic)
                {
                    if (player.Value.Team == ETeam.Red)
                        redPoint += player.Value._currentValue.point;
                    else if (player.Value.Team == ETeam.Blue)
                        bluePoint += player.Value._currentValue.point;
                }
                if (redPoint < bluePoint)
                {
                    packet.winTeam = ETeam.Blue;
                }
                else if (redPoint > bluePoint)
                {
                    packet.winTeam = ETeam.Red;
                }
                else
                {
                    packet.winTeam = ETeam.None;
                }
                host.SendAll(packet);

            }
            _startGame = false;
        }
        else
            _playTime += Time.deltaTime;
        // int blueAlive = 0;
        // int redAlive = 0;
        // foreach (var playerKeyValue in _playerDic)
        // {
        //     if (playerKeyValue.Value.Team == ETeam.Blue)
        //     {
        //         if (playerKeyValue.Value.IsAlive)
        //         {
        //             blueAlive++;
        //         }
        //     }
        //     else
        //     {
        //         if (playerKeyValue.Value.IsAlive)
        //         {
        //             redAlive++;
        //         }
        //     }
        // }
        // if (blueAlive == 0 || redAlive == 0)
        // {
        //     // 게임 종료 처리
        //     Host host = FindObjectOfType<Host>();
        //     if (host != null)
        //     {
        //         PacketGameEnd packet = new PacketGameEnd();
        //         if (blueAlive > 0)
        //         {
        //             packet.winTeam = ETeam.Blue;
        //         }
        //         else
        //         {
        //             packet.winTeam = ETeam.Red;
        //         }
        //         host.SendAll(packet);

        //     }
        //     _startGame = false;
        // }

    }

    public void GameReady()
    {
        // 게임 시작 준비.
        _ui.SetUIState(UIMain.EUIState.Game);

        PacketGameReadyOk packet = new PacketGameReadyOk();
        _client.Send(packet);
    }

    public void GameStart(PacketGameStart packet)
    {
        for (int i = 0; i < packet.userNum; i++)
        {
            // Resources 폴더에서 캐릭터를 불러온다.
            var resource = Resources.Load("Player_Runner");
            // 캐릭터를 인스턴스화 한다.
            var inst = Instantiate(resource) as GameObject;
            // GameObject에 있는 PlayerCharacter 컴포넌트를 가져온다.
            //var player = inst.GetComponent<PlayerCharacter>();
            var player = inst.GetComponent<Player>();
            player.name = $"Player {packet.startInfos[i].uid}";

            //player.Init(packet.startInfos[i].uid, packet.startInfos[i].id, packet.startInfos[i].team, packet.startInfos[i].position);
            player.Init(packet.startInfos[i].uid, packet.startInfos[i].id, packet.startInfos[i].team, packet.startInfos[i].position);
            _playerDic.Add(packet.startInfos[i].uid, player);

            if (UserUID == packet.startInfos[i].uid)
            {
                _localPlayer = player;
            }
        }
        _startGame = true;
        StartCoroutine(SendPlayerPosition());
    }

    private IEnumerator SendPlayerPosition()
    {
        float interval = 1f / 20f;
        while (_localPlayer != null)
        {
            PacketPlayerPosition packet = new PacketPlayerPosition();
            packet.uid = UserUID;
            packet.position = _localPlayer.transform.position;
            packet.rotation = _localPlayer.transform.eulerAngles.y;
            _client.Send(packet);

            yield return new WaitForSeconds(interval);
        }
    }

    public Player GetPlayer(int uid)
    {
        // 키가 존재하는지 확인
        if (!_playerDic.ContainsKey(uid))
            return null;

        return _playerDic[uid];
    }

    public void AddBullet(Bullet bullet)
    {
        _bulletDic.Add(bullet.BulletUID, bullet);
    }

    public void RemoveBullet(int uid)
    {
        if (!_bulletDic.ContainsKey(uid)) 
            return;

        Destroy(_bulletDic[uid].gameObject);
        _bulletDic.Remove(uid);

    }
}