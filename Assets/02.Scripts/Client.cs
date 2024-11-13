using MessagePack;
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
        Packet receivedPacket = MessagePackSerializer.Deserialize<Packet>(buffer);
        switch (receivedPacket)
        {
            case PacketReqUserInfo packet:
                {
                    GameManager.Instance.UserUID = packet.uid;

                    PacketAnsUserInfo sendPacket = new PacketAnsUserInfo();
                    sendPacket.id = GameManager.Instance.UserID;
                    sendPacket.host = GameManager.Instance.IsHost;
                    Send(sendPacket);
                }
                break;
            case PacketAnsUserList packet:
                {
                    string strRed = string.Empty;
                    string strBlue = string.Empty;

                    for (int i = 0; i < packet.userNum; i++)
                    {
                        string strHost = string.Empty;
                        if (packet.userInfos[i].host)
                            strHost = "HOST";

                        if (packet.userInfos[i].team == ETeam.Red)
                        {
                            strRed += $"ID:{packet.userInfos[i].id} UID:{packet.userInfos[i].uid} {strHost} 팀:{packet.userInfos[i].team} 역할:{packet.userInfos[i].role}\n";
                        }
                        else
                        {
                            strBlue += $"ID:{packet.userInfos[i].id} UID:{packet.userInfos[i].uid} {strHost} 팀:{packet.userInfos[i].team} 역할:{packet.userInfos[i].role}\n";
                        }
                    }

                    _ui.SetUIState(UIMain.EUIState.Lobby);
                    _ui.SetLobbyText(strRed, strBlue);
                }
                break;
            case PacketGameReady packet:
                {
                    //packet.uid = GameManager.Instance.UserUID;
                    Debug.Log("Client PacketGameReady packet UID: " + packet.uid);
                    Player player = GameManager.Instance.GetPlayer(packet.uid);
                    if (player == null)
                        return;

                    player.ReadyUISetting(packet.uid, packet.IsReady);
                    //GameManager.Instance.GameReady(packet);
                    //GameManager.Instance.UIPlayers.SetReadyUI(packet.uid, packet.IsReady);
                }
                break;
            case PacketGameStart packet:
                {
                    GameManager.Instance.IsGameStarted = true;
                    GameManager.Instance.GameStart(packet);
                }
                break;
            case PacketPlayerPosition packet:
                {
                    Player player = GameManager.Instance.GetPlayer(packet.uid);
                    if (player == null)
                        return;

                    player.SetPositionRotation(packet.position, packet.rotation);
                }
                break;
            case PacketEntitySpawn packet:
                {
                    GameManager.Instance.AddEntity(packet);
                }
                break;
            case PacketPlayerFire packet:
                {
                    Player player = GameManager.Instance.GetPlayer(packet.ownerUID);
                    if (player == null)
                        return;

                    player.CreateBullet(packet.position, packet.direction, packet.ownerUID, packet.bulletUID);
                }
                break;
            case PacketPlayerDamage packet:
                {
                    Player attackPlayer = GameManager.Instance.GetPlayer(packet.attackUID);
                    Player targetPlayer = GameManager.Instance.GetPlayer(packet.targetUID);
                    if (attackPlayer == null || targetPlayer == null)
                        return;

                    targetPlayer.RecivePoint(attackPlayer.LosePoint);
                }
                break;
            case PacketEntityPlayerCollision packet:
                {
                    Player player = GameManager.Instance.GetPlayer(packet.playerUID);
                    if (player == null)
                        return;

                    if (packet.type == EEntity.Point)
                        player.RecivePoint(player.GetPoint);
                    else
                        player.RecivePoint(player.LosePoint);

                    GameManager.Instance.UpdatePoint(packet.playerUID, packet.type);
                }
                break;
            case PacketBulletDestroy packet:
                {
                    GameManager.Instance.RemoveBullet(packet.bulletUID);
                }
                break;
            case PacketEntityDestroy packet:
                {
                    if (packet.type == EEntity.Point)
                        GameManager.Instance.RemoveCoin(packet.entityUID);
                    else
                        GameManager.Instance.RemoveBomb(packet.entityUID);
                }
                break;
            case PacketGameEnd packet:
                {
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