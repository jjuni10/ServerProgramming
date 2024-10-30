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
            case EProtocolID.SC_REQ_USERINFO:
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
            case EProtocolID.SC_ANS_USERLIST:
                {
                    // 서버로부터 유저 리스트 정보를 받았을 때 처리
                    PacketAnsUserList packet = new PacketAnsUserList();
                    packet.ToPacket(buffer);

                    string strRed = string.Empty;
                    string strBlue = string.Empty;

                    for (int i = 0; i < packet.userNum; i++)
                    {
                        string strHost = string.Empty;
                        if (packet.userInfos[i].host)
                            strHost = "HOST";

                        if (packet.userInfos[i].team == ETeam.Red)
                        {
                            strRed += $"ID:{packet.userInfos[i].id} UID:{packet.userInfos[i].uid} {strHost} 팀:{packet.userInfos[i].team}\n";
                        }
                        else
                        {
                            strBlue += $"ID:{packet.userInfos[i].id} UID:{packet.userInfos[i].uid} {strHost} 팀:{packet.userInfos[i].team}\n";
                        }
                    }

                    _ui.SetUIState(UIMain.EUIState.Lobby);
                    _ui.SetLobbyText(strRed, strBlue);
                }
                break;
            case EProtocolID.REL_GAME_READY:
                {
                    GameManager.Instance.GameReady();
                }
                break;
            case EProtocolID.SC_GAME_START:
                {
                    GameManager.Instance.IsGameStarted = true;

                    PacketGameStart packet = new PacketGameStart();
                    packet.ToPacket(buffer);

                    GameManager.Instance.GameStart(packet);
                }
                break;
            case EProtocolID.REL_PLAYER_POSITION:
                {
                    PacketPlayerPosition packet = new PacketPlayerPosition();
                    packet.ToPacket(buffer);

                    Player player = GameManager.Instance.GetPlayer(packet.uid);
                    if (player == null)
                        return;

                    player.SetPositionRotation(packet.position, packet.rotation);
                }
                break;
            case EProtocolID.REL_PLAYER_FIRE:
                {
                    PacketPlayerFire packet = new PacketPlayerFire();
                    packet.ToPacket(buffer);

                    Player player = GameManager.Instance.GetPlayer(packet.ownerUID);
                    if (player == null)
                        return;

                    player.CreateBullet(packet.position, packet.direction, packet.ownerUID,  packet.bulletUID);
                }
                break;
            case EProtocolID.REL_PLAYER_POINT:
                {
                    PacketPlayerDamage packet = new PacketPlayerDamage();
                    packet.ToPacket(buffer);
                    Player attackPlayer = GameManager.Instance.GetPlayer(packet.attackUID);
                    Player targetPlayer = GameManager.Instance.GetPlayer(packet.targetUID);
                    if (attackPlayer == null || targetPlayer == null)
                        return;

                    targetPlayer.RecivePoint(attackPlayer.LosePoint);
                }
                break;
            case EProtocolID.REL_BULLET_DISTROY:
                {
                    PacketBulletDistroy packet = new PacketBulletDistroy();
                    packet.ToPacket(buffer);
                    GameManager.Instance.RemoveBullet(packet.bulletUID);

                }
                break;
            case EProtocolID.SC_GAME_END:
                {
                    PacketGameEnd packet = new PacketGameEnd();
                    packet.ToPacket(buffer);
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