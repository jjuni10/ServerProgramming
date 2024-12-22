using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private int _bombUID;
    public int BombUID => _bombUID;
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
            //RemoveBomb();
            return;
        }

        // 상대 플레이어에게 데미지를 전송
        PacketEntityPlayerCollision packet = new PacketEntityPlayerCollision();
        packet.entityUID = BombUID;
        packet.type = EEntity.Bomb;
        packet.playerUID = player.UID;
        GameManager.Instance.Client.Send(packet);
        RemoveBomb();
    }

    private void RemoveBomb()
    {
        if (!GameManager.Instance.IsHost)
            return;

        // 총알이 파괴되었음을 서버에 알림
        PacketEntityDestroy packetBombDistroy = new PacketEntityDestroy();
        packetBombDistroy.entityUID = BombUID;
        packetBombDistroy.type = EEntity.Bomb;
        GameManager.Instance.Client.Send(packetBombDistroy);

        // 호스트에서 총알 제거
        GameManager.Instance.RemoveBomb(_bombUID);
    }

    public void Init(int bombUID)
    {
        _bombUID = bombUID;

        GameManager.Instance.AddBomb(this);
    }
}
