using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{   
    public enum EState // 캐릭터의 상태
    {
        Idle,
        Run,
        Fire,
        Die
    }
    private int _uid;
    private string _id;
    private ETeam _team;
    private bool _localPlayer;              // 로컬 플레이어 캐릭터 여부

    private int _hp;
    private int _damage;
    private float _speed;
    private Vector3 _destPosition;          // 비로컬 캐릭터의 목표 위치 (서버에서 받는 위치)

    private EState _curState;
    private float _fireTime;                // 발사 후 공격 애니메이션 유지 시간
    private float _curFireCoolTime;         // 현재 공격 쿨타임

    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;

    public int UID => _uid;
    public ETeam Team => _team;
    public bool IsLocalPlayer => _localPlayer;
    public int Damage => _damage;
    public bool IsAlive => _hp > 0;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    public void Init(int uid, string id, ETeam team, Vector3 position)
    {
        _uid = uid; 
        _id = id; 
        _team = team;
        if (GameManager.Instance.UserUID == _uid)
            _localPlayer = true;

        //스프레드 시트나 json으로 연동
        _hp = 100;
        _damage = 5;
        _speed = 30f;

        _destPosition = position;
        transform.position = position;
    }

    private void Update()
    {
        if (!_localPlayer)
        {
            // 위치 보정
            transform.position = Vector3.Lerp(transform.position, _destPosition, Time.deltaTime * _speed);
        }

        // 입력에 따른 움직임 처리
        if (_moveDirection != Vector3.zero)
        {
            _rigidbody.MovePosition(transform.position + _moveDirection.normalized * Time.deltaTime * _speed);
            _moveDirection = Vector3.zero;
        }

        // 발사 쿨타임 처리
        if (_curFireCoolTime > 0f)
        {
            _curFireCoolTime -= Time.deltaTime;
        }
    }

    public void Move(Vector3 direction)
    {
        _moveDirection += direction;
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
        packet.ownerUID = _uid;
        packet.position = transform.position + new Vector3(0f, 0.5f, 0f);
        packet.direction = transform.forward;
        GameManager.Instance.Client.Send(packet);

        _curFireCoolTime = Define.FIRE_COOL_TIME;
    }
    public void CreateBullet(Vector3 position, Vector3 direction, int ownerUID, int bulletUID)
    {
        GameObject bulletResource = null;
        if(_team == ETeam.Red)
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
        bullet.GetComponent<Bullet>().Init(ownerUID, bulletUID, position);
    }
    

    public void ReciveDamage(int damage)
    {
        _hp -= damage;
    }
}