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

    private void FixedUpdate()
    {
        // 총알을 일정 속도로 전방으로 이동시킴
        _rigidbody.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * GameManager.Instance.playerSheetData.BulletSpeed);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!GameManager.Instance.IsHost)
        {
            return;
        }

        Player player = other.GetComponent<Player>();
        
        // 플레이어가 아닌 다른 객체에 충돌하면 총알을 제거함
        if (player == null)
        {
            Debug.Log("부딪힌 놈: " + other.name);
            RemoveBullet();
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

    public void Init(int ownerUID, int bulletUID, Vector3 spawnPoint)
    {
        _bulletUID = bulletUID;
        _ownerUID = ownerUID;
        transform.position = spawnPoint;

        GameManager.Instance.AddBullet(this);
    }
}