using UnityEngine;

public class Client : MonoBehaviour, IPeer
{
    private NetClient _client = new NetClient();
    private UserToken _userToken;
    private UIMain _ui;

    public void StartClient(string ip)
    {
        MainThread.Instance.Init();
        PacketMessageDispatcher.Instance.Init();
        _client.onConnected += OnConnected;
        _client.Start(ip);

        _ui = FindObjectOfType<UIMain>();
    }

    private void OnConnected(bool connected, UserToken token)
    {
        if (connected)
        {
            Debug.Log("서버에 연결 완료");
            _userToken = token;
            _userToken.SetPeer(this);
        }
    }

    public void ProcessMessage(short protocolID, byte[] buffer)
    {
        switch ((EProtocolID)protocolID)
        {
            case EProtocolID.PacketReqUserInfo:
                {
                    PacketReqUserInfo packet = new PacketReqUserInfo();
                    packet.ToPacket(buffer);
                    GameManager.Instance.UserUID = packet.uid;

                    PacketAnsUserInfo sendPacket = new PacketAnsUserInfo();
                    sendPacket.id = GameManager.Instance.UserID;
                    sendPacket.host = GameManager.Instance.IsHost;
                    Send(sendPacket);
                }
                break;
            case EProtocolID.PacketAnsUserList:
                {
                    PacketAnsUserList packet = new PacketAnsUserList();
                    packet.ToPacket(buffer);
                    string strRed = string.Empty;
                    string strBlue = string.Empty;

                    for (int i = 0; i < packet.userNum; i++)
                    {
                        var userInfo = packet.userInfos[i];

                        string strHost = string.Empty;
                        if (userInfo.host)
                            strHost = "HOST";

                        if (userInfo.team == ETeam.Red)
                        {
                            strRed += $"ID:{userInfo.id} UID:{userInfo.uid} {strHost} 팀:{userInfo.team} 역할:{userInfo.role}\n";
                        }
                        else
                        {
                            strBlue += $"ID:{userInfo.id} UID:{userInfo.uid} {strHost} 팀:{userInfo.team} 역할:{userInfo.role}\n";
                        }


                    }
                    _ui.SetUIState(UIMain.EUIState.Lobby);
                    _ui.SetLobbyText(strRed, strBlue);
                    GameManager.Instance.OnPlayerListUpdated(packet);
                }
                break;
            case EProtocolID.PacketGameReady:
                {
                    PacketGameReady packet = new PacketGameReady();
                    packet.ToPacket(buffer);
                    Player player = GameManager.Instance.GetPlayer(packet.uid);
                    if (player == null)
                        return;

                    player.ReadyUISetting(packet.uid, packet.IsReady);
                }
                break;
            case EProtocolID.PacketGameStart:
                {
                    PacketGameStart packet = new PacketGameStart();
                    packet.ToPacket(buffer);
                    GameManager.Instance.IsGameStarted = true;
                    GameManager.Instance.GameStart(packet);
                }
                break;
            case EProtocolID.PacketPlayerPosition:
                {
                    PacketPlayerPosition packet = new PacketPlayerPosition();
                    packet.ToPacket(buffer);
                    Player player = GameManager.Instance.GetPlayer(packet.uid);
                    if (player == null)
                        return;

                    player.SetPositionRotation(packet.position, packet.rotation);
                }
                break;
            case EProtocolID.PacketDashStart:
                {
                    PacketDashStart packet = new PacketDashStart();
                    packet.ToPacket(buffer);
                    Player player = GameManager.Instance.GetPlayer(packet.uid);
                    if (player == null)
                        return;

                    player.NotLocalDodge();
                }
                break;
            case EProtocolID.PacketEntitySpawn:
                {
                    PacketEntitySpawn packet = new PacketEntitySpawn();
                    packet.ToPacket(buffer);
                    GameManager.Instance.AddEntity(packet);
                }
                break;
            case EProtocolID.PacketPlayerFire:
                {
                    PacketPlayerFire packet = new PacketPlayerFire();
                    packet.ToPacket(buffer);
                    Player player = GameManager.Instance.GetPlayer(packet.ownerUID);
                    if (player == null)
                        return;

                    player.CreateBullet(packet.position, packet.direction, packet.ownerUID, packet.bulletUID);
                }
                break;
            case EProtocolID.PacketPlayerDamage:
                {
                    PacketPlayerDamage packet = new PacketPlayerDamage();
                    packet.ToPacket(buffer);
                    Player attackPlayer = GameManager.Instance.GetPlayer(packet.attackUID);
                    Player targetPlayer = GameManager.Instance.GetPlayer(packet.targetUID);
                    if (attackPlayer == null || targetPlayer == null)
                        return;

                    if (attackPlayer.Team == targetPlayer.Team) //같은 팀이면
                        attackPlayer.RecivePoint(attackPlayer.LosePoint);   //실점
                    else                                        //다른 팀이면
                        attackPlayer.RecivePoint(attackPlayer.GetPoint);    //득점
                }
                break;
            case EProtocolID.PacketEntityPlayerCollision:
                {
                    PacketEntityPlayerCollision packet = new PacketEntityPlayerCollision();
                    packet.ToPacket(buffer);
                    Player player = GameManager.Instance.GetPlayer(packet.playerUID);
                    if (player == null)
                        return;

                    if (packet.type == EEntity.Point)
                        player.RecivePoint(player.GetPoint);
                    else
                        player.RecivePoint(player.LosePoint);
                }
                break;
            case EProtocolID.PacketTeamScoreUpdate:
                {
                    PacketTeamScoreUpdate packet = new PacketTeamScoreUpdate();
                    packet.ToPacket(buffer);
                    GameManager.Instance.UpdatePoint(packet.uid, packet.score);
                }
                break;
            case EProtocolID.PacketBulletDestroy:
                {
                    PacketBulletDestroy packet = new PacketBulletDestroy();
                    packet.ToPacket(buffer);
                    GameManager.Instance.RemoveBullet(packet.bulletUID);
                }
                break;
            case EProtocolID.PacketEntityDestroy:
                {
                    PacketEntityDestroy packet = new PacketEntityDestroy();
                    packet.ToPacket(buffer);
                    if (packet.type == EEntity.Point)
                        GameManager.Instance.RemoveCoin(packet.entityUID);
                    else
                        GameManager.Instance.RemoveBomb(packet.entityUID);
                }
                break;
            case EProtocolID.PacketGameEnd:
                {
                    PacketGameEnd packet = new PacketGameEnd();
                    packet.ToPacket(buffer);
                    GameManager.Instance.IsGameEnd = true;
                    GameManager.Instance.WinTeam = packet.winTeam;
                    Debug.Log($"승리팀은 {packet.winTeam}");
                }
                break;
        }
    }
    public void Remove()
    {
    }

    public void Send(Packet packet)
    {
        _userToken.Send(packet);
    }
}