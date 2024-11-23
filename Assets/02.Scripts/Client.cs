using MessagePack;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour, IPeer
{
    private NetClient _client = new NetClient();
    private UserToken _userToken;
    private UIMain _ui;

    private ConcurrentQueue<Packet> queue = new ConcurrentQueue<Packet>();


    public void StartClient(string ip)
    {
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
        try
        {
            Packet receivedPacket = MessagePackSerializer.Deserialize<Packet>(buffer);
            queue.Enqueue(receivedPacket);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void ProcessPacket(Packet receivedPacket)
    {
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
            case PacketGameReady packet:
                {
                    Player player = GameManager.Instance.GetPlayer(packet.uid);
                    if (player == null)
                        return;

                    player.IsReady = packet.IsReady;
                    player.ReadyUISetting(packet.uid, packet.IsReady);
                }
                break;
            case PacketGameStart packet:
                {
                    StartCoroutine(WaitAndStartGame(packet));
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
            case PacketDashStart packet:
                {
                    Player player = GameManager.Instance.GetPlayer(packet.uid);
                    if (player == null)
                        return;

                    player.NotLocalDodge();
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

                    if (attackPlayer.Team == targetPlayer.Team) //같은 팀이면
                        attackPlayer.RecivePoint(attackPlayer.LosePoint);   //실점
                    else                                        //다른 팀이면
                        attackPlayer.RecivePoint(attackPlayer.GetPoint);    //득점
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
                }
                break;
            case PacketTeamScoreUpdate packet:
                {
                    GameManager.Instance.UpdatePoint(packet.uid, packet.score);
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

        if (receivedPacket is not PacketPlayerPosition)
        {
            Debug.LogFormat("패킷 받음! Type:{0}", receivedPacket.GetType().Name);
        }
    }

    private IEnumerator WaitAndStartGame(PacketGameStart packet)
    {
        yield return new WaitForSeconds(1.0f);

        GameManager.Instance.IsGameStarted = true;
        GameManager.Instance.GameStart(packet);
    }

    void Update()
    {
        while (queue.Count > 0)
        {
            if (queue.TryDequeue(out Packet packet))
            {
                ProcessPacket(packet);
            }
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