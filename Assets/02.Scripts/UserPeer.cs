using MessagePack;
using System;
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

    public void Close()
    {
        _userToken.Close();
    }

    public void OnReceive(Packet receivedPacket)
    {
        switch (receivedPacket)
        {
            case EProtocolID.PacketAnsUserInfo:
                {
                    PacketAnsUserInfo packet = new PacketAnsUserInfo();
                    packet.ToPacket(buffer);
                    _id = packet.id;
                    _isHost = packet.host;
                    Debug.Log("CS_ANS_USERINFO " + packet.id + " " + _isHost);

                    _host.SendUserList();
                }
                break;
            case EProtocolID.PacketReqChangeTeam:
                {
                    PacketReqChangeTeam packet= new PacketReqChangeTeam();
                    packet.ToPacket(buffer);
                    _team = packet.team;
                    _host.SendUserList();
                }
                break;
            case EProtocolID.PacketReqChangeRole:
                {
                    PacketReqChangeRole packet = new PacketReqChangeRole();
                    packet.ToPacket(buffer);
                    _role = packet.role;
                    _host.SendUserList();
                }
                break;
            case EProtocolID.PacketGameReady:
                {
                    PacketGameReady packet = new PacketGameReady();
                    packet.ToPacket(buffer);
                    _gameReady = packet.IsReady;
                    _host.SendAll(packet);
                }
                break;
            case EProtocolID.PacketPlayerPosition:
                {
                    PacketPlayerPosition packet = new PacketPlayerPosition();
                    packet.ToPacket(buffer);
                    packet.uid = _uid;
                    _host.SendAll(packet, this);
                }
                break;
            case EProtocolID.PacketDashStart:
                {
                    PacketDashStart packet = new PacketDashStart();
                    packet.ToPacket(buffer);
                    _host.SendAll(packet);
                }
                break;
            case EProtocolID.PacketEntitySpawn:
                {
                }
                break;
            case EProtocolID.PacketEntityPlayerCollision:
                {
                    PacketEntityPlayerCollision packet = new PacketEntityPlayerCollision();
                    packet.ToPacket(buffer);
                    _host.SendAll(packet);
                }
                break;
            case EProtocolID.PacketTeamScoreUpdate:
                {
                    PacketTeamScoreUpdate packet = new PacketTeamScoreUpdate();
                    packet.ToPacket(buffer);
                    _host.SendAll(packet);
                }
                break;
            case EProtocolID.PacketPlayerFire:
                {
                    PacketPlayerFire packet = new PacketPlayerFire();
                    packet.ToPacket(buffer);
                    packet.ownerUID = _uid;
                    packet.bulletUID = _bulletUID;
                    _host.SendAll(packet);
                    _bulletUID++;
                }
                break;
            case EProtocolID.PacketPlayerDamage:
                {
                    PacketPlayerDamage packet = new PacketPlayerDamage();
                    packet.ToPacket(buffer);
                    _host.SendAll(packet);
                }
                break;
            case EProtocolID.PacketBulletDestroy:
                {
                    PacketBulletDestroy packet = new PacketBulletDestroy();
                    packet.ToPacket(buffer);
                    _host.SendAll(packet, this);
                }
                break;
            case EProtocolID.PacketEntityDestroy:
                {
                    PacketEntityDestroy packet = new PacketEntityDestroy();
                    packet.ToPacket(buffer);
                    _host.SendAll(packet);
                }
                break;
            case PacketGameEnd packet : 
                {
                    _host.SendAll(packet);
                }
                break;
            case PacketLatencyTest packet:
                {
                    TimeSpan diff = (DateTime.Now - new DateTime(packet.DateTimeTicks));
                    Debug.LogFormat("Latency: {0}ms", diff.TotalMilliseconds);
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