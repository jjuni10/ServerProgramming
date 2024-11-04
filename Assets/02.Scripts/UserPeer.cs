using MessagePack;
using UnityEngine;

/// <summary>
/// 유저와 관련된 동작 처리
/// </summary>
public class UserPeer : IPeer
{
    private static int _bulletUID;  // 총알 고유번호 카운터, 유일 식별자
    private Host _host;

    private UserToken _userToken;
    private int _uid;               // 유저 고유 식별 번호
    private string _id;
    private ETeam _team;
    private ERole _role;
    private bool _isHost;
    private bool _gameReady = false;        // 게임 준비 완료 여부

    public int UID => _uid;
    public string ID => _id;
    public bool IsHost
    {
        get => _isHost;
        set => _isHost = value;
    }
    public bool GameReady => _gameReady;

    public ETeam Team
    {
        get => _team;
        set => _team = value;
    }

    public ERole Role
    {
        get => _role;
        set => _role = value;
    }

    public UserPeer(UserToken userToken, int uid, Host host)
    {
        _userToken = userToken;
        _uid = uid;
        _host = host;

        _userToken.SetPeer(this);
    }

    public void ProcessMessage(short protocolID, byte[] buffer)
    {
        Packet receivedPacket = MessagePackSerializer.Deserialize<Packet>(buffer);
        switch (receivedPacket)
        {
            case PacketAnsUserInfo packet:
                {
                    _id = packet.id;
                    _isHost = packet.host;
                    Debug.Log("CS_ANS_USERINFO " + packet.id + " " + _isHost);

                    _host.SendUserList();
                }
                break;
            case PacketReqChangeTeam packet:
                {
                    _team = packet.team;
                    _host.SendUserList();
                }
                break;
            case PacketReqChangeRole packet:
                {
                    _role = packet.role;
                    _host.SendUserList();
                }
                break;
            case PacketGameReady packet:
                {
                    _gameReady = true;
                    _host.SendAll(packet);
                }
                break;
            case PacketGameReadyOk packet:
                {
                    _host.CheckGameReady();
                }
                break;
            case PacketPlayerPosition packet:
                {
                    packet.uid = _uid;
                    _host.SendAll(packet, this);
                }
                break;
            case PacketPlayerFire packet:
                {
                    packet.ownerUID = _uid;
                    packet.bulletUID = _bulletUID;
                    _host.SendAll(packet);
                    _bulletUID++;
                }
                break;
            case PacketPlayerDamage packet:
                {
                    _host.SendAll(packet);
                }
                break;
            case PacketBulletDestroy packet:
                {
                    _host.SendAll(packet, this);
                }
                break;
        }
    }

    public void Remove()
    {
        _host.SendUserList();
    }

    public void Send(Packet packet)
    {
        _userToken.Send(packet);
    }
}