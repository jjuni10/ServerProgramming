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
    private bool _isHost;
    private bool _gameReady;        // 게임 준비 완료 여부

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

    public UserPeer(UserToken userToken, int uid, Host host)
    {
        _userToken = userToken;
        _uid = uid;
        _host = host;

        _userToken.SetPeer(this);
    }

    public void ProcessMessage(short protocolID, byte[] buffer)
    {
        switch ((EProtocolID)protocolID)
        {
            case EProtocolID.CS_ANS_USERINFO:
                {
                    PacketAnsUserInfo packet = new PacketAnsUserInfo();
                    packet.ToPacket(buffer);
                    _id = packet.id;
                    _isHost = packet.host;
                    Debug.Log("CS_ANS_USERINFO " + packet.id + " " + _isHost);

                    _host.SendUserList();
                }
                break;
            case EProtocolID.CS_REQ_CHANGE_TEAM:
                {
                    PacketReqChangeTeam packet = new PacketReqChangeTeam();
                    packet.ToPacket(buffer);
                    _team = packet.team;

                    _host.SendUserList();
                }
                break;
            case EProtocolID.REL_GAME_READY:
                {
                    PacketGameReady packet = new PacketGameReady();
                    packet.ToPacket(buffer);
                    _host.SendAll(packet);
                }
                break;
            case EProtocolID.CS_GAME_READY_OK:
                {
                    _gameReady = true;
                    _host.CheckGameReady();
                }
                break;
            case EProtocolID.REL_PLAYER_POSITION:
                {
                    PacketPlayerPosition packet = new PacketPlayerPosition();
                    packet.ToPacket(buffer);
                    packet.uid = _uid;
                    _host.SendAll(packet , this);
                }
                break;
            case EProtocolID.REL_PLAYER_FIRE:
                {
                    PacketPlayerFire packet = new PacketPlayerFire();
                    packet.ToPacket(buffer);
                    packet.ownerUID = _uid;
                    packet.bulletUID = _bulletUID;
                    _host.SendAll(packet);
                    _bulletUID++;
                }
                break;
            case EProtocolID.REL_PLAYER_DAMAG:
                {
                    PacketPlayerDamage packet = new PacketPlayerDamage();
                    packet.ToPacket(buffer);
                    _host.SendAll(packet);
                }
                break;
            case EProtocolID.REL_BULLET_DISTROY:
                {
                    PacketBulletDistroy packet = new PacketBulletDistroy();
                    packet.ToPacket(buffer);
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