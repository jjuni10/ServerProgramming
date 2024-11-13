using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Host : MonoBehaviour
{
    private NetServer _server = new NetServer();
    private List<UserPeer> _userList = new List<UserPeer>();
    private int _curUID;
    private int _curCoinUID = 0;
    private int _curBombUID = 0;

    public void StartHost()
    {
        MainThread.Instance.Init();
        PacketMessageDispatcher.Instance.Init();
        _server.onClientConnected += OnClientConnected;
        _server.Start(10);
        GameManager.Instance.IsHost = true;
        StartCoroutine(Entity_Co());

        FindObjectOfType<Client>().StartClient("127.0.0.1");
    }

    private void OnClientConnected(UserToken token)
    {
        Debug.Log("클라이언트 접속");

        // 게임이 시작되었거나 유저가 4명 이상이라면 접속을 거부한다.
        if (GameManager.Instance.IsGameStarted || _userList.Count >= 4)
        {
            token.Close();
            return;
        }

        UserPeer user = new UserPeer(token, _curUID, this);
        // 팀 배정
        var redList = _userList.FindAll(item => item.Team == ETeam.Red);
        var blueList = _userList.FindAll(item => item.Team == ETeam.Blue);
        if (redList.Count > blueList.Count)
        {
            user.Team = ETeam.Blue;
            user.Role = ERole.Gunner;
        }
        else
        {
            user.Team = ETeam.Red;
            user.Role = ERole.Runner;
        }

        _userList.Add(user);
        token.onSessionClosed += OnClosed;

        PacketReqUserInfo packet = new PacketReqUserInfo();
        packet.uid = _curUID;
        packet.team = user.Team;

        user.Send(packet);

        _curUID++;
    }

    private void OnClosed(UserToken token)
    {
        _userList.Remove(token.Peer as UserPeer);
    }

    public void SendAll(Packet packet)
    {
        foreach (UserPeer user in _userList)
        {
            user.Send(packet);
        }
    }

    public void SendAll(Packet packet, UserPeer except)
    {
        foreach (UserPeer user in _userList)
        {
            if (user == except)
                continue;
            user.Send(packet);
        }
    }

    public void SendUserList()
    {
        PacketAnsUserList sendPacket = new PacketAnsUserList();
        sendPacket.userNum = _userList.Count;
        for (int i = 0; i < _userList.Count; i++)
        {
            sendPacket.userInfos[i] = new UserInfo();
            sendPacket.userInfos[i].id = _userList[i].ID;
            sendPacket.userInfos[i].uid = _userList[i].UID;
            sendPacket.userInfos[i].team = _userList[i].Team;
            sendPacket.userInfos[i].role = _userList[i].Role;
            sendPacket.userInfos[i].host = _userList[i].IsHost;
        }
        SendAll(sendPacket);
    }

    public void CheckGameReady()
    {
        // for (int i = 0; i < _userList.Count; i++)
        // {
        //     if (!_userList[i].GameReady)
        //         return;
        // }
        GameManager.Instance.IsGameStarted = true;

        Vector3 redPosition = new Vector3(5, 3, 0F);
        Vector3 bluePosition = new Vector3(55, 3, 0F);
        int redCount = 0;
        int blueCount = 0;

        // 게임 시작 정보를 전송한다.
        PacketGameStart packet = new PacketGameStart();
        packet.userNum = _userList.Count;
        for (int i = 0; i < _userList.Count; i++)
        {
            packet.startInfos[i] = new GameStartInfo();
            packet.startInfos[i].uid = _userList[i].UID;
            packet.startInfos[i].id = _userList[i].ID;
            packet.startInfos[i].team = _userList[i].Team;
            packet.startInfos[i].role = _userList[i].Role;
            if (_userList[i].Team == ETeam.Red)
            {
                packet.startInfos[i].position = redPosition;
                if (redCount % 2 == 0)
                {
                    redPosition = new Vector3(redPosition.x + Define.START_DISTANCE_OFFSET, redPosition.y, redPosition.z);
                }
                else
                {
                    redPosition = new Vector3(-redPosition.x, redPosition.y, redPosition.z);
                }
                redCount++;
            }
            else
            {
                packet.startInfos[i].position = bluePosition;
                if (blueCount % 2 == 0)
                {
                    bluePosition = new Vector3(bluePosition.x + Define.START_DISTANCE_OFFSET, bluePosition.y, bluePosition.z);
                }
                else
                {
                    bluePosition = new Vector3(-bluePosition.x, bluePosition.y, bluePosition.z);
                }
                blueCount++;
            }
        }

        SendAll(packet);
    }

    public void ReadyCheckGameStart()
    {
        for (int i = 0; i < _userList.Count; i++)
        {
            if (!_userList[i].GameReady)
                return;
        }
        GameManager.Instance.IsGameStarted = true;

        Vector3 gunnerPosition = new Vector3(Define.GAME_GUNNER_POSITION_OFFSET, 3, 0);
        Vector3 runnerPosition = new Vector3(Define.GAME_RUNNER_POSITION_OFFSET, 3, 0);

        // 게임 시작 정보를 전송한다.
        Debug.Log("Host PacketGameOn ReadyCheckGameStart() PacketGameStart packet");
        PacketGameStart packet = new PacketGameStart();
        packet.userNum = _userList.Count;
        for (int i = 0; i < _userList.Count; i++)
        {
            packet.startInfos[i] = new GameStartInfo();
            packet.startInfos[i].uid = _userList[i].UID;
            packet.startInfos[i].id = _userList[i].ID;
            packet.startInfos[i].team = _userList[i].Team;
            packet.startInfos[i].role = _userList[i].Role;
            if (_userList[i].Role == ERole.Gunner)
            {
                packet.startInfos[i].position = gunnerPosition;
                if (_userList[i].Team == ETeam.Red)
                {
                    gunnerPosition = new Vector3(gunnerPosition.x * -1, gunnerPosition.y, gunnerPosition.z);
                }
                else
                {
                    gunnerPosition = new Vector3(gunnerPosition.x, gunnerPosition.y, gunnerPosition.z);
                }
            }
            else
            {
                packet.startInfos[i].position = runnerPosition;
                if (_userList[i].Team == ETeam.Red)
                {
                    runnerPosition = new Vector3(runnerPosition.x * -1, runnerPosition.y, runnerPosition.z);
                }
                else
                {
                    runnerPosition = new Vector3(runnerPosition.x, runnerPosition.y, runnerPosition.z);
                }
            }
        }

        SendAll(packet);
    }

    IEnumerator Entity_Co()
    {
        float spawnTime = 0f;
        float createTime = 5f;
        while (_server.GetRun())
        {
            yield return new WaitUntil(() => GameManager.Instance.IsGamePlayOn == true);
            if (spawnTime >= createTime)
            {
                PacketEntitySpawn packetPoint = new PacketEntitySpawn();
                packetPoint.type = EEntity.Point;
                packetPoint.entityUID = _curCoinUID++;
                packetPoint.position = SpreadEntity();
                SendAll(packetPoint);
                
                PacketEntitySpawn packetBomb = new PacketEntitySpawn();
                packetBomb.type = EEntity.Bomb;
                packetBomb.entityUID = _curBombUID++;
                packetBomb.position = SpreadEntity();
                SendAll(packetBomb);

                spawnTime = 0f;
            }
            spawnTime += Time.deltaTime;

            yield return null;
        }
    }

    public Vector3 SpreadEntity()
    {
        int ranPosX;
        int ranPosZ;

        while (true)
        {
            ranPosX = UnityEngine.Random.Range(-60, 60);
            ranPosZ = UnityEngine.Random.Range(-60, 60);
            if (System.Math.Abs(ranPosX) + System.Math.Abs(ranPosZ) <= 60) break;
        }

        return new Vector3(ranPosX, 3, ranPosZ);
    }

    // (진짜) 게임 시작
    public void GameOn()
    {
        PacketGameOn packet = new PacketGameOn();
        SendAll(packet);
    }
}