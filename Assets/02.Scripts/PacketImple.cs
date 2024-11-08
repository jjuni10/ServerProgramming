// SC : 서버->클라, CS : 클라->서버, REL : 중계형
using UnityEngine;
using MessagePack;

/*public enum EProtocolID
{
    SC_REQ_USERINFO,
    CS_ANS_USERINFO,
    SC_ANS_USERLIST,
    CS_REQ_CHANGE_TEAM,
    REL_GAME_READY,
    CS_GAME_READY_OK,
    SC_GAME_START,
    REL_PLAYER_POSITION,
    REL_PLAYER_FIRE,
    REL_PLAYER_DAMAG,
    REL_BULLET_DISTROY,
    SC_GAME_END,
}*/

#region 게임 시작 전
[MessagePackObject]
public struct UserInfo
{
    [Key(0)]
    public int uid;

    [Key(1)]
    public string id;

    [Key(2)]
    public ETeam team;

    [Key(3)]
    public bool host;

    [Key(4)]
    public ERole role;
}

[MessagePackObject]
public class PacketUserJoin : Packet
{
    [Key(10)]
    public int uid;
}

[MessagePackObject]
public class PacketUserLeave : Packet
{
    [Key(10)]
    public int uid;
}

[MessagePackObject]
public class PacketReqUserInfo : Packet
{
    [Key(10)]
    public int uid;

    [Key(11)]
    public ETeam team;

    [Key(12)]
    public ERole role;
}

[MessagePackObject]
public class PacketAnsUserInfo : Packet
{
    [Key(10)]
    public string id;

    [Key(11)]
    public bool host;
}


[MessagePackObject]
public class PacketAnsUserList : Packet
{
    [Key(10)]
    public int userNum;

    [Key(11)]
    public UserInfo[] userInfos = new UserInfo[20];
}

[MessagePackObject]
public class PacketReqChangeTeam : Packet
{
    [Key(10)]
    public ETeam team;
}

[MessagePackObject]
public class PacketReqChangeRole : Packet
{
    [Key(10)]
    public ERole role;
}

[MessagePackObject]
public class PacketGameReady : Packet
{
    [Key(10)]
    public bool IsReady;
}

[MessagePackObject]
public class PacketGameReadyOk : Packet
{
}

#endregion

#region 게임 시작
[MessagePackObject]
public struct GameStartInfo
{
    [Key(0)]
    public int uid;

    [Key(1)]
    public string id;

    [Key(2)]
    public ETeam team;

    [Key(3)]
    public Vector3 position;

    [Key(4)]
    public ERole role;
}

[MessagePackObject]
public class PacketGameStart : Packet
{
    [Key(10)]
    public int userNum;

    [Key(11)]
    public GameStartInfo[] startInfos = new GameStartInfo[20];
}
#endregion

#region 게임 플레이 중
[MessagePackObject]
public class PacketFeverStart : Packet
{
}

[MessagePackObject]
public class PacketTimerUpdate : Packet
{
    [Key(10)]
    public int timeSeconds;
}

[MessagePackObject]
public class PacketTeamScoreUpdate : Packet
{
    [Key(10)]
    public int redTeamScore;

    [Key(11)]
    public int blueTeamScore;
}


[MessagePackObject]
public class PacketPlayerPosition : Packet
{
    [Key(10)]
    public int uid;         // 유저의 uid를 확인하고 처리한다.

    [Key(11)]
    public Vector3 position;

    [Key(12)]
    public float rotation;
}

[MessagePackObject]
public class PacketPlayerFire : Packet
{
    [Key(10)]
    public int ownerUID;

    [Key(11)]
    public int bulletUID;

    [Key(12)]
    public Vector3 position;

    [Key(13)]
    public Vector3 direction;
}

[MessagePackObject]
public class PacketEntitySpawn : Packet
{
    [Key(10)]
    public EEntity type;

    [Key(11)]
    public int ownerUID;

    [Key(12)]
    public int entityUID;

    [Key(13)]
    public Vector3 position;

    [Key(14)]
    public Vector3 velocity;

    [Key(15)]
    public float rotation;
}

[MessagePackObject]
public class PacketEntityDestroy : Packet
{
    [Key(10)]
    public int entityUID;
}

[MessagePackObject]
public class PacketEntityPlayerCollision : Packet
{
    [Key(10)]
    public int playerUID;

    [Key(11)]
    public int entityUID;
}

[MessagePackObject]
public class PacketPlayerDamage : Packet
{
    [Key(10)]
    public int attackUID; // 공격한 플레이어

    [Key(11)]
    public int targetUID; // 공격받은 플레이어
}

[MessagePackObject]
public class PacketBulletDestroy : Packet
{
    [Key(10)]
    public int bulletUID;
}
#endregion

#region 게임 끝
[MessagePackObject]
public class PacketGameEnd : Packet
{
    [Key(10)]
    public ETeam winTeam;

    [Key(11)]
    public int[] redTeamScores = new int[2];

    [Key(12)]
    public int[] blueTeamScores = new int[2];
}
#endregion