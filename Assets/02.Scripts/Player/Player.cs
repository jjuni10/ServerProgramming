using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public PlayerInfo _playerInfos = new PlayerInfo();
    public PlayerComponents _playerComponents = new PlayerComponents();
    public PlayerInput _input = new PlayerInput();
    public CheckOption _checkOption = new CheckOption();
    public CurrentState _currentState = new CurrentState();
    public CurrentValue _currentValue = new CurrentValue();

    protected PlayerInfo P_Info => _playerInfos;
    protected PlayerComponents P_Com => _playerComponents;
    protected PlayerInput P_Input => _input;
    protected CheckOption P_COption => _checkOption;
    protected CurrentState P_States => _currentState;
    protected CurrentValue P_Value => _currentValue;

    private PlayerGunner _gunner;
    private PlayerRunner _runner;

    public GameObject RedGunner;
    public GameObject RedRunner;
    public GameObject BlueGunner;
    public GameObject BlueRunner;

    public TMP_Text nickname;
    public GameObject name_;

    public bool IsReady = false;
    public int LosePoint = -1;
    public int GetPoint = 1;
    public int UID => _playerInfos.UID;
    public string ID => _playerInfos.ID;
    public ETeam Team => _playerInfos.TEAM;
    public ERole Role => _playerInfos.ROLE;
    public bool IsLocalPlayer => _playerInfos._localPlayer;
    protected Vector3 _destPosition;          // 비로컬 캐릭터의 목표 위치 (서버에서 받는 위치)
    private float _curFireCoolTime;         // 현재 공격 쿨타임

    public void Init(int uid, string id, ETeam team, Vector3 position, ERole role, int point = 0)
    {
        P_Com.animator = this.GetComponent<Animator>();
        P_Com.rigidbody = this.GetComponent<Rigidbody>();

        P_Info.UID = uid;
        P_Info.ID = id;
        P_Info.TEAM = team;
        P_Info.ROLE = role;
        P_Value.point = point;

        if (nickname == null) 
        {
            // Resources 폴더에서 불러온다.
            var resource = Resources.Load("nickname");
            // 인스턴스화 한다.
            var inst = Instantiate(resource) as GameObject;
            // GameObject에 있는 컴포넌트를 가져온다.
            nickname = inst.GetComponent<TMP_Text>();
            nickname.transform.parent = GameManager.Instance.canvas.transform;
            nickname.name = $"nickname {id}";
        }
        nickname.text = id;
        if (GameManager.Instance.UserUID == UID)
            _playerInfos._localPlayer = true;
        P_Com.cameraObj = Camera.main;

        _gunner = GetComponent<PlayerGunner>();
        _runner = GetComponent<PlayerRunner>();

        ChangeRole(role);

        _destPosition = position;
        transform.position = position;
        transform.Rotate(new Vector3(0, 180, 0));
    }

    void Update()
    {
        //Vector3.Distance(transform.position,_destPosition);
        if (!IsLocalPlayer)
        {
            // 위치 보정
            transform.position = Vector3.Lerp(transform.position, _destPosition, Time.deltaTime * P_COption.runningSpeed);
            if (Vector3.Distance(transform.position,_destPosition) <= 0.1f)
                P_Com.animator.SetBool("isRunning", false);
            else 
                P_Com.animator.SetBool("isRunning", true);
        }
        nickname.transform.position = SetNicknamePos();
        _curFireCoolTime += Time.deltaTime;
    }

    public void Move(KeyCode keyCode)
    {
        P_Com.animator.SetBool("isRunning", true);
        if (Role == ERole.Runner || GameManager.Instance.LobbyController != null)
        {
            _runner.Move(keyCode);
        }
        else
        {
            //Debug.Log($"In {keyCode}");
            _gunner.Move(keyCode);
        }
    }

    public void Rotate()
    {
        if (Role == ERole.Runner || GameManager.Instance.LobbyController != null)
        {
            _runner.Rotate();
        }
    }

    public void NotLocalDodge()
    { 
        if (IsLocalPlayer) return;

        P_Com.animator.SetTrigger("isDodge");
    }

    public void SetReady(bool isReady)
    {
        //if (!P_Info._localPlayer) return;
        PacketGameReady packet = new PacketGameReady();
        packet.uid = UID;
        packet.IsReady = isReady;
        GameManager.Instance.Client.Send(packet);
        IsReady = isReady;
        //GameManager.Instance.UIPlayers.SetReadyUI(packet.uid, packet.IsReady);
    }
    public void ReadyUISetting(int uid, bool ready)
    {
        _curFireCoolTime = 0;
        GameManager.Instance.LobbyController.SetReadyState(uid, ready);
    }

    public void SetPositionRotation(Vector3 position, float rotation)
    {
        _destPosition = position;
        transform.eulerAngles = new Vector3(0f, rotation, 0f);
    }
    public void FireBullet()
    {
        if (_curFireCoolTime < GameManager.Instance.playerSheetData.GunnerFireCoolTime)
        {
            return;
        }
        PacketPlayerFire packet = new PacketPlayerFire();
        packet.ownerUID = UID;
        packet.position = transform.position + new Vector3(0f, 1.5f, 0f);
        packet.direction = transform.forward;
        GameManager.Instance.Client.Send(packet);
        _curFireCoolTime = 0;
    }
    public void CreateBullet(Vector3 position, Vector3 direction, int ownerUID, int bulletUID)
    {
        GameObject bulletResource = null;
        Bullet bullet;
        if (Team == ETeam.Red)
        {
            bulletResource = Resources.Load("RedBullet") as GameObject;
            bullet = GameManager.Instance.pool.Get(2, position).GetComponent<Bullet>();
        }
        else
        {
            bulletResource = Resources.Load("BlueBullet") as GameObject;
            bullet = GameManager.Instance.pool.Get(3, position).GetComponent<Bullet>();
        }

        bullet.Init(ownerUID, bulletUID);
        bullet.spawnPoint = position;
        bullet.transform.forward = direction.normalized;
        bullet.gameObject.SetActive(true);
        //Debug.Log("[bullet] CreateBullet()");
    }
    public void RecivePoint(int point)
    {
        //if (!IsLocalPlayer) return;
        P_Value.point += point;
        PacketTeamScoreUpdate packet = new PacketTeamScoreUpdate();
        packet.uid = UID;
        packet.score = P_Value.point;
        //Debug.Log($"player {packet.uid}, {packet.score}");

        GameManager.Instance.Client.Send(packet);
    }

    // 역할 바꾸는 함수
    public void ChangeRole(ERole role)
    {
        //Debug.Log($"ChangeRole {role}");
        P_Info.ROLE = role;
        if (P_Info.ROLE == ERole.Runner || GameManager.Instance.LobbyController != null)
        {
            _gunner.enabled = false;
            _runner.enabled = true;
            ChangeLayerRecursively(this.gameObject, 6); //Runner
        }
        else if (P_Info.ROLE == ERole.Gunner)
        {
            _runner.enabled = false;
            _gunner.enabled = true;
            ChangeLayerRecursively(this.gameObject, 7); //Gunner
        }
    }

    // 자식 객체도 레이어 변경
    private void ChangeLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach(Transform child in obj.transform)
        {
            ChangeLayerRecursively(child.gameObject, layer);
        }
        ChangeModeling();
    }
    
    // modeling change
    public void ChangeModeling()
    {
        if (P_Info.TEAM == ETeam.Red)
        {
            if (P_Info.ROLE == ERole.Runner)
            {
                RedGunner.SetActive(false);
                RedRunner.SetActive(true);
                BlueGunner.SetActive(false);
                BlueRunner.SetActive(false);
                P_Com.animator = RedRunner.GetComponent<Animator>();
            }
            else if (P_Info.ROLE == ERole.Gunner)
            {
                RedGunner.SetActive(true);  
                RedRunner.SetActive(false);
                BlueGunner.SetActive(false);
                BlueRunner.SetActive(false);
                P_Com.animator = RedGunner.GetComponent<Animator>();
            }
        }
        else    // blue
        {
            if(P_Info.ROLE == ERole.Runner)
            {
                RedGunner.SetActive(false);
                RedRunner.SetActive(false);
                BlueGunner.SetActive(false);
                BlueRunner.SetActive(true);
                P_Com.animator = BlueRunner.GetComponent<Animator>();
            }
            else if (P_Info.ROLE == ERole.Gunner)
            {
                RedGunner.SetActive(false);
                RedRunner.SetActive(false);
                BlueGunner.SetActive(true);
                BlueRunner.SetActive(false);
                P_Com.animator = BlueGunner.GetComponent<Animator>();
            }

        }
    }

    public void SetPlayerPoint()
    {
        nickname.text = "<" + ID + ">\n" + P_Value.point + "Point";
    }
    public Vector3 SetNicknamePos()
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(name_.transform.position);
        //nickname.transform.position = screenPoint + new Vector3(0, 2, 0);
        return screenPoint;// + new Vector3(0, 270, 0);
    }
}
