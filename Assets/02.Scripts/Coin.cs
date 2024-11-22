using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private int _coinUID;
    public int CoinUID => _coinUID;
    public Vector3 spawnPoint;

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

        Player player = other.GetComponent<Player>();

        // 플레이어가 아닌 다른 객체에 충돌하면 총알을 제거함
        if (player == null)
        {
            //RemoveCoin();
            return;
        }

        // 상대 플레이어에게 데미지를 전송
        PacketEntityPlayerCollision packet = new PacketEntityPlayerCollision();
        packet.entityUID = CoinUID;
        packet.type = EEntity.Point;
        packet.playerUID = player.UID;
        GameManager.Instance.Client.Send(packet);
        RemoveCoin();
    }

    private void RemoveCoin()
    {
        if (!GameManager.Instance.IsHost)
            return;

        // 총알이 파괴되었음을 서버에 알림
        PacketEntityDestroy packetBombDistroy = new PacketEntityDestroy();
        packetBombDistroy.entityUID = CoinUID;
        packetBombDistroy.type = EEntity.Point;
        GameManager.Instance.Client.Send(packetBombDistroy);

        // 호스트에서 총알 제거
        GameManager.Instance.RemoveCoin(_coinUID);
    }

    public void Init(int coinUID)
    {
        _coinUID = coinUID;

        GameManager.Instance.AddCoin(this);
    }
    void OnEnable() 
    {
        this.transform.position = spawnPoint;
    }
}
