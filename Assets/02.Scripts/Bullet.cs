using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int _bulletUID;
    private int _ownerUID;
    private Rigidbody _rigidbody;
    public int BulletUID => _bulletUID;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 총알을 일정 속도로 전방으로 이동시킴
        _rigidbody.MovePosition(transform.position + transform.forward * Time.deltaTime * 100f);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!GameManager.Instance.IsHost)
        {
            return;
        }

        // 다른 총알과 충돌 시 처리하지 않음
        if (other.CompareTag("Bullet"))
        {
            return;
        }

        PlayerCharacter player = other.GetComponent<PlayerCharacter>();
        
        // 플레이어가 아닌 다른 객체에 충돌하면 총알을 제거함
        if (player == null)
        {
            RemoveBullet();
            return;
        }

        // 이미 죽은 플레이어에 충돌하면 처리하지 않음
        if (!player.IsAlive)
            return;

        Player owner = GameManager.Instance.GetPlayer(_ownerUID);
        // 총알을 발사한 플레이어 자신에게 충돌하면 처리하지 않음
        if (_ownerUID == player.UID)
        {
            return;
        }
        
        // 같은 팀 플레이어에게 충돌하면 처리하지 않음
        if (player.Team == owner.Team)
        {
            return;
        }

        // 상대 플레이어에게 데미지를 전송
        PacketPlayerDamage packet = new PacketPlayerDamage();
        packet.attackUID = _ownerUID;
        packet.targetUID = player.UID;
        GameManager.Instance.Client.Send(packet);
        RemoveBullet();
    }

    private void RemoveBullet()
    {
        if (!GameManager.Instance.IsHost)
            return;

        // 총알이 파괴되었음을 서버에 알림
        PacketBulletDestroy packetBulletDistroy = new PacketBulletDestroy();
        packetBulletDistroy.bulletUID = _bulletUID;
        GameManager.Instance.Client.Send(packetBulletDistroy);

        // 호스트에서 총알 제거
        GameManager.Instance.RemoveBullet(_bulletUID);
    }

    public void Init(int ownerUID, int bulletUID)
    {
        _bulletUID = bulletUID;
        _ownerUID = ownerUID;

        GameManager.Instance.AddBullet(this);
    }
}