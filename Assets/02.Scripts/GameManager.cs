using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임을 관리하는 매니저 클래스
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private UIMain _ui;
    public FirstSceneUIController UIPlayers;
    public SecondSceneUIController UIPlayers2;
    public ThirdSceneUIController UIPlayers3;
    private Client _client;
    private Dictionary<int, Player> _playerDic =
        new Dictionary<int, Player>();             // UID, 플레이어 캐릭터
    [SerializeField]
    private Player _localPlayer;                   // 로컬 플레이어 캐릭터

    private Dictionary<int, Coin> _coins = new Dictionary<int, Coin>();
    private Dictionary<int, Bomb> _bombs = new Dictionary<int, Bomb>();
    private Dictionary<int, Bullet> _bulletDic = new Dictionary<int, Bullet>(); // UID, 총알
    private bool _startGame;                                // 게임이 시작되었는지 여부
    private float _playTime = 0f;

    //! 나중에 입력 받아서 플레이타임 결정하기
    public float InputPlayTime = 270f;

    private float readyTime;

    public int redPoint;
    public int bluePoint;
    public ETeam WinTeam { get; set; }

    public int UserUID { get; set; }    // 클라이언트 자신의 UID
    public string UserID { get; set; }  // 클라이언트 자신의 ID
    public bool IsHost { get; set; }
    public bool IsGameStarted { get; set; }
    public bool IsGamePlayOn { get; set; }
    public bool IsGameEnd { get; set; }

    public int PlayerCount => _playerDic.Count;
    public Client client => _client;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += GameSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _ui = FindObjectOfType<UIMain>();
        _client = FindObjectOfType<Client>();
    }

    private void Update()
    {
        UpdateInput();
        UpdateCheckGameEnd();

        if (UIPlayers2 != null) UIPlayers2.SetPointUI(UserUID);
    }

    private void SceneSetting()
    {
        //Debug.Log("SceneSetting()");
        if (SceneManager.GetActiveScene().name == "GameServer"
            || SceneManager.GetActiveScene().name == "GameReady"
            || SceneManager.GetActiveScene().name == "Game")
        {
            UIPlayers = FindObjectOfType<FirstSceneUIController>();
            if (UIPlayers != null) UIPlayers.gameObject.SetActive(false);
        }
        else if (SceneManager.GetActiveScene().name == "GamePlay")
        {
            UIPlayers2 = FindObjectOfType<SecondSceneUIController>();
            //todo: players Setting(Position, Rotation, UI(ID, Point = 0))
            //UIPlayers2.SetIDUI(UserUID);
            //UIPlayers2.SetPointUI(UserUID, true);
        }
        else if (SceneManager.GetActiveScene().name == "GameResult")
        {
            UIPlayers3 = FindObjectOfType<ThirdSceneUIController>();
            //if (UIPlayers3 != null) UIPlayers3.gameObject.SetActive(false);
        }
    }

    private void UpdateInput()
    {
        if (_localPlayer == null)// || !_localPlayer.IsAlive)
            return;

        if (Input.GetKey(KeyCode.W))
        {
            _localPlayer.Move(KeyCode.W);
        }
        if (Input.GetKey(KeyCode.S))
        {
            _localPlayer.Move(KeyCode.S);
        }
        if (Input.GetKey(KeyCode.A))
        {
            _localPlayer.Move(KeyCode.A);
        }
        if (Input.GetKey(KeyCode.D))
        {
            _localPlayer.Move(KeyCode.D);
        }

        _localPlayer.Rotate();

        // 총알 발사
        if (Input.GetMouseButtonDown(0))
        {
            _localPlayer.FireBullet();
        }

        // Ready 취소
        if (Input.GetKey(KeyCode.LeftControl))
        {
            readyTime -= Time.deltaTime;
            if (readyTime < 0)
            {
                _localPlayer.SetReady(false);
                readyTime = 0;
            }
            else
            {
                UIPlayers.SetReadyProgress(UserUID, readyTime / Define.READY_TIME);
            }
        }

        // Ready 하기
        if (Input.GetKey(KeyCode.Space))
        {
            readyTime += Time.deltaTime;
            if (readyTime >= Define.READY_TIME)
            {
                _localPlayer.SetReady(true);
                readyTime = Define.READY_TIME;
            }
            else
            {
                UIPlayers.SetReadyProgress(UserUID, readyTime / Define.READY_TIME);
            }
        }
    }

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

    }

    public void GameStart(PacketGameStart packet)
    {
        if (_ui) _ui.SetUIState(UIMain.EUIState.Game);
        for (int i = 0; i < packet.userNum; i++)
        {
            // Resources 폴더에서 캐릭터를 불러온다.
            var resource = Resources.Load("Player");
            // 캐릭터를 인스턴스화 한다.
            var inst = Instantiate(resource) as GameObject;
            // GameObject에 있는 Player 컴포넌트를 가져온다.
            var player = inst.GetComponent<Player>();
            //player.name = $"Player {packet.startInfos[i].uid}";

            player.Init(packet.startInfos[i].uid, packet.startInfos[i].id, packet.startInfos[i].team, packet.startInfos[i].position, packet.startInfos[i].role);
            _playerDic.Add(packet.startInfos[i].uid, player);
            //Debug.Log("GameStart() _playerDic.Add()");

            if (UserUID == packet.startInfos[i].uid)
            {
                _localPlayer = player;
            }
        }
        _startGame = true;
        StartCoroutine(SendPlayerPosition());
    }

    public void UpdatePoint(int uid, EEntity type)
    {
        if (type == EEntity.Point)
            UIPlayers2.SetPointUI(uid, false, 1);
        else
            UIPlayers2.SetPointUI(uid, false, -1);
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
        //Debug.Log($"{_playerDic.Count}");
        // 키가 존재하는지 확인
        if (!_playerDic.ContainsKey(uid))
            return null;

        return _playerDic[uid];
    }

#region Add / Remove Entity
    public void AddBullet(Bullet bullet)
    {
        _bulletDic.Add(bullet.BulletUID, bullet);
    }
    public void AddBomb(Bomb bomb)
    {
        _bombs.Add(bomb.BombUID, bomb);
    }
    public void AddCoin(Coin coin)
    {
        _coins.Add(coin.CoinUID, coin);
    }

    public void RemoveBullet(int uid)
    {
        if (!_bulletDic.ContainsKey(uid))
            return;

        Destroy(_bulletDic[uid].gameObject);
        _bulletDic.Remove(uid);
    }
    public void RemoveBomb(int uid)
    {
        if (!_bombs.ContainsKey(uid))
            return;

        Destroy(_bombs[uid].gameObject);
        _bombs.Remove(uid);
    }
    public void RemoveCoin(int uid)
    {
        if (!_coins.ContainsKey(uid))
            return;

        Destroy(_coins[uid].gameObject);
        _coins.Remove(uid);
    }
#endregion

    public void AddEntity(PacketEntitySpawn packet)
    {
        switch (packet.type)
        {
            case EEntity.Point:
                {
                    //GameObject Coin = 
                    Instantiate(Resources.Load("Coin"), packet.position, Quaternion.identity);// as GameObject;
                    //_coins.Add(packet.entityUID, Coin.GetComponent<Coin>());
                }
                break;
            case EEntity.Bomb:
                {
                    //GameObject Bomb = 
                    Instantiate(Resources.Load("Bomb"), packet.position, Quaternion.identity);// as GameObject;
                    //_bombs.Add(packet.entityUID, Bomb.GetComponent<Bomb>());
                }
                break;
            case EEntity.Bullet:
                {

                }
                break;
            default:
                break;
        }

    }

    private void GameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("GameSceneLoaded()");
        SceneSetting();
        if (UIPlayers2)
        {
            for (int i = 0; i < _playerDic.Count; i++)
            {
                UIPlayers2.SetIDUI(i);
                UIPlayers2.SetPointUI(i);
            }
        }
    }
    public void GameSceneNext()
    {
        //if (SceneManager.GetActiveScene().name == "Game")
           SceneManager.LoadScene("GamePlay");
    }
}