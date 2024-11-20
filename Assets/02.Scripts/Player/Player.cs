using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Init(int uid, string id, ETeam team, Vector3 position, ERole role)
    {
        P_Com.animator = this.GetComponent<Animator>();
        P_Com.rigidbody = this.GetComponent<Rigidbody>();

        P_Info.UID = uid;
        P_Info.ID = id;
        P_Info.TEAM = team;
        P_Info.ROLE = role;
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
        if (!IsLocalPlayer)
        {
            // 위치 보정
            transform.position = Vector3.Lerp(transform.position, _destPosition, Time.deltaTime * P_COption.runningSpeed);
        }
        _curFireCoolTime += Time.deltaTime;
    }

    public void Move(KeyCode keyCode)
    {
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
        if (_curFireCoolTime < Define.FIRE_COOL_TIME)
        {
            //Debug.Log($"[bullet] _curFireCoolTime {_curFireCoolTime}");
            //_curFireCoolTime += Time.deltaTime;
            return;
        }
        //Debug.Log("[bullet] FireBullet()");
        PacketPlayerFire packet = new PacketPlayerFire();
        packet.ownerUID = UID;
        packet.position = transform.position + new Vector3(0f, 1.5f, 0f);
        packet.direction = transform.forward;
        GameManager.Instance.Client.Send(packet);
        _curFireCoolTime = 0;

         //Define.FIRE_COOL_TIME;
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
    }
}
