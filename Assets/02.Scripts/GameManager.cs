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

    public Client Client { get; private set; }
    public Host Host { get; private set; }

    public LobbyController LobbyController;
    public SecondSceneUIController UIPlayers2;
    public ThirdSceneUIController UIPlayers3;

    public PoolManager pool;
    public PlayerSheetData sheetData;

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
    private PacketGameStart _gameStartPacket;
    private PacketGameEnd _scorePacket;

    //! 나중에 입력 받아서 플레이타임 결정하기
    private float InputPlayTime = 180f;

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
    public float PlayTime => InputPlayTime;

    public Canvas canvas;

    public PlayerSheetData playerSheetData;
    public int[] points = new int[4];

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        Client = GetComponent<Client>();
        Host = GetComponent<Host>();
        sheetData.GetComponent<PlayerSheetData>();

        canvas = FindObjectOfType<Canvas>();
        playerSheetData = FindObjectOfType<PlayerSheetData>();

        SceneSetting();
        SceneManager.sceneLoaded += GameSceneLoaded;
    }

    private void Update()
    {
        UpdateInput();
        UpdateCheckGameEnd();

        //if (UIPlayers2 != null) UIPlayers2.SetPointUI(UserUID);
    }

    private void SceneSetting()
    {
        if (SceneManager.GetActiveScene().name == "GameServer"
            || SceneManager.GetActiveScene().name == "GameReady"
            || SceneManager.GetActiveScene().name == "Game")
        {
            LobbyController = FindObjectOfType<LobbyController>(true);
            if (LobbyController != null) LobbyController.gameObject.SetActive(false);
        }
        else if (SceneManager.GetActiveScene().name == "GamePlay")
        {
            UIPlayers2 = FindObjectOfType<SecondSceneUIController>(true);
            pool = FindObjectOfType<PoolManager>(true);
        }
        else if (SceneManager.GetActiveScene().name == "GameResult")
        {
            UIPlayers3 = FindObjectOfType<ThirdSceneUIController>(true);
            //if (UIPlayers3 != null) UIPlayers3.gameObject.SetActive(false);
        }
        
        canvas = FindObjectOfType<Canvas>();
    }

    private void UpdateInput()
    {
        if (_localPlayer == null || UIPlayers3 != null)
            return;

        if (Input.GetKey(KeyCode.W))
        {
            _localPlayer.Move(KeyCode.W);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            _localPlayer.Move(KeyCode.S);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            _localPlayer.Move(KeyCode.A);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _localPlayer.Move(KeyCode.D);
        }
        else _localPlayer._playerComponents.animator.SetBool("isRunning", false);

        _localPlayer.Rotate();

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
                LobbyController.SetReadyProgress(UserUID, readyTime / Define.READY_TIME);
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) && readyTime > 0)
        {
            readyTime = Define.READY_TIME;
            LobbyController.SetReadyProgress(UserUID, readyTime / Define.READY_TIME);
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
                LobbyController.SetReadyProgress(UserUID, readyTime / Define.READY_TIME);
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space) && readyTime < Define.READY_TIME)
        {
            readyTime = 0;
            LobbyController.SetReadyProgress(UserUID, readyTime / Define.READY_TIME);
        }
        
        //*============================ GamePlay Scene===================

        if (UIPlayers2 == null)
        {
            return;
        }

        // 총알 발사
        if (Input.GetMouseButtonDown(0) && _localPlayer.Role == ERole.Gunner)
        {
            //Debug.Log("[bullet] Input Mouse");
            _localPlayer.FireBullet();
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
            if (Host != null)
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
                Host.SendAll(packet);

            }
            _startGame = false;
        }
        else
            _playTime += Time.deltaTime;

    }

    public void GameStart(PacketGameStart packet)
    {
        SceneManager.LoadScene("GamePlay");

        _gameStartPacket = packet;
    }
    public void GameEnd(PacketGameEnd packet)
    {
        //Debug.Log("[GameEnd] GameEnd()");
        SceneManager.LoadScene("GameResult");

        _scorePacket = packet;
    }

    private void OnGamePlaySceneLoaded(PacketGameStart packet)
    {
        _playerDic.Clear();
        Debug.Log(SceneManager.GetActiveScene().name);
        Debug.Log("게임 시작! 유저 수: " + packet.userNum);
        //_ui.SetUIState(UIMain.EUIState.Game);
        for (int i = 0; i < packet.userNum; i++)
        {
            // Resources 폴더에서 캐릭터를 불러온다.
            var resource = Resources.Load("Player");
            // 캐릭터를 인스턴스화 한다.
            var inst = Instantiate(resource) as GameObject;
            // GameObject에 있는 Player 컴포넌트를 가져온다.
            var player = inst.GetComponent<Player>();
            player.name = $"Player {packet.startInfos[i].uid}";

            Debug.Log(player);
            player.Init(packet.startInfos[i].uid, packet.startInfos[i].id, packet.startInfos[i].team, packet.startInfos[i].position, packet.startInfos[i].role);
            _playerDic.Add(packet.startInfos[i].uid, player);
            //Debug.Log("GameStart() _playerDic.Add()");

            if (UserUID == packet.startInfos[i].uid)
            {
                _localPlayer = player;
            }
            if (packet.startInfos[i].team == ETeam.Red)
            {
                player.transform.rotation = Quaternion.Euler(new Vector3(0, 90f, 0));
            }
            else
            {
                player.transform.rotation = Quaternion.Euler(new Vector3(0, -90f, 0));
            }
        }

        _startGame = true;

        if (UIPlayers2)
        {
            for (int i = 0; i < _playerDic.Count; i++)
            {
                UIPlayers2.SetIDUI(i);
                UIPlayers2.SetPointUI(i);
                _localPlayer.ChangeRole(_localPlayer.Role);
            }
        }
        StartCoroutine(SendPlayerPosition());
    }

    private void OnGameResultSceneLoaded(PacketGameStart packet, PacketGameEnd scorePacket)
    {
        _playerDic.Clear();
        Vector3 position;
        int resultPoint = 0;
        for (int i = 0; i < packet.userNum; i++)
        {
            // Resources 폴더에서 캐릭터를 불러온다.
            var resource = Resources.Load("Player");
            // 캐릭터를 인스턴스화 한다.
            var inst = Instantiate(resource) as GameObject;
            // GameObject에 있는 Player 컴포넌트를 가져온다.
            var player = inst.GetComponent<Player>();
            player.name = $"Player {packet.startInfos[i].uid}";

            switch (packet.startInfos[i].uid + 1)
            {
                case 1:
                {
                    position = sheetData.Red1WinCheckStartPos;
                    resultPoint = scorePacket.redTeamScores[0];
                    //Debug.Log($"[GameEnd] case 1 position: {position}");
                }
                break;
                case 3:
                {
                    position = sheetData.Red2WinCheckStartPos;
                    resultPoint = scorePacket.redTeamScores[1];
                    //Debug.Log($"[GameEnd] case 2 position: {position}");
                }
                break;
                case 2:
                {
                    position = sheetData.Blue1WinCheckStartPos;
                    resultPoint = scorePacket.blueTeamScores[0];
                    //Debug.Log($"[GameEnd] case 3 position: {position}");
                }
                break;
                case 4:
                {
                    position = sheetData.Blue2WinCheckStartPos;
                    resultPoint = scorePacket.blueTeamScores[1];
                    //Debug.Log($"[GameEnd] case 4 position: {position}");
                }
                break;
                default:
                    position = packet.startInfos[i].position;
                    //Debug.Log($"[GameEnd] position: {position}");
                break;
            }
            points[packet.startInfos[i].uid] = resultPoint;

            player.Init(packet.startInfos[i].uid, packet.startInfos[i].id, packet.startInfos[i].team, position, packet.startInfos[i].role, points[packet.startInfos[i].uid]);
            _playerDic.Add(packet.startInfos[i].uid, player);
            //Debug.Log("GameStart() _playerDic.Add()");

            if (UserUID == packet.startInfos[i].uid)
            {
                _localPlayer = player;
            }
        SetPlayersPoint();
        }
    }

    public void OnPlayerListUpdated(PacketAnsUserList packet)
    {
        for (int i = 0; i < packet.userNum; i++)
        {
            var userInfo = packet.userInfos[i];
            int uid = userInfo.uid;
            Player player;

            if (_playerDic.ContainsKey(uid))
            {
                player = GetPlayer(uid);
            }
            else
            {
                // 만약 플레이어가 존재하지 않으면 새 플레이어 스폰
                var resource = Resources.Load("Player");
                var inst = Instantiate(resource) as GameObject;
                player = inst.GetComponent<Player>();
                var position = LobbyController.GetSpawnPoint(uid);

                player.name = $"Player {uid}";
                player.Init(uid, userInfo.id, userInfo.team, position, userInfo.role);

                if (UserUID == uid)
                {
                    _localPlayer = player;
                    StartCoroutine(SendPlayerPosition());
                }
                _playerDic.Add(uid, player);
            }

            player.ChangeRole(userInfo.role);

            LobbyController.SetRole(uid, player.Role);
            LobbyController.SetReadyState(uid, false);
        }
    }


    public void UpdatePoint(int uid, int point)// EEntity type)
    {
        // if (type == EEntity.Point)
        //     UIPlayers2.SetPointUI(uid, false, 1);
        // else
        //     UIPlayers2.SetPointUI(uid, false, -1);
        UIPlayers2.SetPointUI(uid, point);
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
            Client.Send(packet);

            //Debug.Log("Position 패킷 보냄!");
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

       //Destroy(_bulletDic[uid].gameObject);
        _bulletDic[uid].gameObject.SetActive(false);

        //_bulletDic.Remove(uid);
    }
    public void RemoveBomb(int uid)
    {
        if (!_bombs.ContainsKey(uid))
            return;

        //Destroy(_bombs[uid].gameObject);
        _bombs[uid].gameObject.SetActive(false);

        //_bombs.Remove(uid);
    }
    public void RemoveCoin(int uid)
    {
        if (!_coins.ContainsKey(uid))
            return;

        //Destroy(_coins[uid].gameObject);
        _coins[uid].gameObject.SetActive(false);

        //_coins.Remove(uid);
    }
    #endregion

    public int GetEntityCount(EEntity entity)
    {
        switch (entity)
        {
            case EEntity.Point:
                return _coins.Count;
            case EEntity.Bomb:
                return _bombs.Count;
            default:
                return 0;
        }
    }

    public void AddEntity(PacketEntitySpawn packet)
    {
        switch (packet.type)
        {
            case EEntity.Point:
                {
                    //GameObject Coin = Instantiate(Resources.Load("Coin"), packet.position, Quaternion.identity) as GameObject;
                    //Coin.GetComponent<Coin>().Init(packet.entityUID);
                    Coin coin = pool.Get(0, packet.position).GetComponent<Coin>();
                    coin.Init(packet.entityUID);
                    coin.spawnPoint = packet.position;
                    coin.gameObject.SetActive(true);
                }
                break;
            case EEntity.Bomb:
                {
                    //GameObject Bomb = Instantiate(Resources.Load("Bomb"), packet.position, Quaternion.identity) as GameObject;
                    //Bomb.GetComponent<Bomb>().Init(packet.entityUID);
                    Bomb bomb = pool.Get(1, packet.position).GetComponent<Bomb>();
                    bomb.Init(packet.entityUID);
                    bomb.spawnPoint = packet.position;
                    bomb.gameObject.SetActive(true);
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
        SceneSetting();
        //Debug.Log($"[GameEnd] scene.name: {scene.name}");

        if (scene.name == "GamePlay")
        {
            OnGamePlaySceneLoaded(_gameStartPacket);
        }
        if (scene.name == "GameResult")
        {
            OnGameResultSceneLoaded(_gameStartPacket,_scorePacket);
        }
    }
    public void GameSceneNext()
    {
        //if (SceneManager.GetActiveScene().name == "Game")
        SceneManager.LoadScene("GamePlay");
    }

    public void SetPlayersPoint()
    {
        foreach (KeyValuePair<int,Player> p in _playerDic)
        {
            p.Value.SetPlayerPoint();
        }
    }
}