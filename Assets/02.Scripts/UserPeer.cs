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
                    _gameReady = packet.IsReady;
                    //packet.uid = _uid;
                    //packet.IsReady = true;
                    //GameManager.Instance.GameReady(packet);
                    //GameManager.Instance.UIPlayers.SetReadyUI(packet.uid, packet.IsReady);
                    //Debug.Log("UserPeer PacketGameReady packet UID: " + packet.uid);
                    //GameManager.Instance.client.Send(packet);
                    _host.SendAll(packet);
                }
                break;
            //case PacketGameReadyOk packet:
            //    {
            //        _host.CheckGameReady();
            //    }
            //    break;
            case PacketPlayerPosition packet:
                {
                    packet.uid = _uid;
                    _host.SendAll(packet, this);
                }
                break;
            case PacketDashStart packet:
                {
                    _host.SendAll(packet);
                }
                break;
            case PacketEntitySpawn packet:
                {
                    //_host.SendAll(packet);
                }
                break;
            case PacketEntityPlayerCollision packet:
                {
                    _host.SendAll(packet);
                }
                break;
            case PacketTeamScoreUpdate packet:
                {
                    _host.SendAll(packet);
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
            case PacketEntityDestroy packet:
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
            case PacketGameEnd packet:
                {
                    _host.SendAll(packet);
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