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

    public int LosePoint = -1;
    public int GetPoint = 1;
    public int UID => _playerInfos.UID;
    public ETeam Team => _playerInfos.TEAM;
    public bool IsLocalPlayer => _playerInfos._localPlayer;
    protected Vector3 _destPosition;          // 비로컬 캐릭터의 목표 위치 (서버에서 받는 위치)
    private float _curFireCoolTime;         // 현재 공격 쿨타임

    public void Init(int uid, string id, ETeam team, Vector3 position)
    {
        P_Info.UID = uid; 
        P_Info.ID = id; 
        P_Info.TEAM = team;
        P_Com.cameraObj = Camera.main;
        
        _destPosition = position;
        transform.position = position;
    }

    // private void Update()
    // {
    //     if (!IsLocalPlayer)
    //     {
    //         // 위치 보정
    //         transform.position = Vector3.Lerp(transform.position, _destPosition, Time.deltaTime * P_COption.runningSpeed);
    //     }

    //     // 입력에 따른 움직임 처리
    //     if (P_Value.moveDirection != Vector3.zero)
    //     {
    //         P_Com.rigidbody.MovePosition(transform.position + P_Value.moveDirection.normalized * Time.deltaTime * P_COption.runningSpeed);
    //         P_Value.moveDirection = Vector3.zero;
    //     }

    //     // 발사 쿨타임 처리
    //     if (_curFireCoolTime > 0f)
    //     {
    //         _curFireCoolTime -= Time.deltaTime;
    //     }
    // }

    public virtual void Move()
    {

    }

    public virtual void Rotate()
    {
        
    }

    public void SetPositionRotation(Vector3 position, float rotation)
    {
        _destPosition = position;
        transform.eulerAngles = new Vector3(0f, rotation, 0f);
    }
    public void FireBullet()
    {
        if (_curFireCoolTime > 0f)
        {
            return;
        }
        PacketPlayerFire packet = new PacketPlayerFire();
        packet.ownerUID = UID;
        packet.position = transform.position + new Vector3(0f, 0.5f, 0f);
        packet.direction = transform.forward;
        GameManager.Instance.client.Send(packet);

        _curFireCoolTime = Define.FIRE_COOL_TIME;
    }
    public void CreateBullet(Vector3 position, Vector3 direction, int ownerUID, int bulletUID)
    {
        GameObject bulletResource = null;
        if(Team == ETeam.Red)
        {
            bulletResource = Resources.Load("RedBullet") as GameObject;
        }
        else
        {
            bulletResource = Resources.Load("BlueBullet") as GameObject;
        }

        GameObject bullet = Instantiate(bulletResource);
        bullet.transform.position = position;
        bullet.transform.forward = direction.normalized;
        bullet.GetComponent<Bullet>().Init(ownerUID, bulletUID);
    }
    public void RecivePoint(int point)
    {
        P_Value.point += point;
    }
}
