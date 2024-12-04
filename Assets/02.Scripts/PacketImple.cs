using UnityEngine;
using System.Runtime.InteropServices;
public enum EProtocolID
{
    PacketUserJoin,
    PacketUserLeave,
    PacketReqUserInfo,
    PacketAnsUserInfo,
    PacketAnsUserList,
    PacketReqChangeTeam,
    PacketReqChangeRole,
    PacketGameReady,
    PacketGameReadyOk,
    GameStartInfo,
    PacketGameStart,
    PacketGameOn,
    PacketFeverStart,
    PacketTimerUpdate,
    PacketTeamScoreUpdate,
    PacketPlayerPosition,
    PacketPlayerFire,
    PacketEntitySpawn,
    PacketEntityDestroy,
    PacketEntityPlayerCollision,
    PacketPlayerDamage,
    PacketBulletDestroy,
    PacketDashStart,
    PacketGameEnd
}


#region 게임 시작 전
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UserInfo
{
    public int uid;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string id;
    public ETeam team;
    public bool host;
    public ERole role;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketUserJoin : Packet
{
    public int uid;
    public PacketUserJoin():base((short)EProtocolID.PacketUserJoin){}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketUserLeave : Packet
{
    public int uid;
    
    public PacketUserLeave():base((short)EProtocolID.PacketUserLeave){}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketReqUserInfo : Packet
{
    public int uid;
    public ETeam team;
    public ERole role;
    
    public PacketReqUserInfo():base((short)EProtocolID.PacketReqUserInfo){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketAnsUserInfo : Packet
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string id;
    public bool host;
    
    public PacketAnsUserInfo():base((short)EProtocolID.PacketAnsUserInfo){}
}



[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketAnsUserList : Packet
{
    public int userNum;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public UserInfo[] userInfos = new UserInfo[20];
    
    public PacketAnsUserList():base((short)EProtocolID.PacketAnsUserList){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketReqChangeTeam : Packet
{
    public ETeam team;
    
    public PacketReqChangeTeam():base((short)EProtocolID.PacketReqChangeTeam){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketReqChangeRole : Packet
{
    public ERole role;
    
    public PacketReqChangeRole():base((short)EProtocolID.PacketReqChangeRole){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketGameReady : Packet
{
    public int uid;
    public bool IsReady;
    
    public PacketGameReady():base((short)EProtocolID.PacketGameReady){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketGameReadyOk : Packet
{
    public PacketGameReadyOk():base((short)EProtocolID.PacketGameReadyOk){}
}


#endregion

#region 게임 시작

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GameStartInfo
{
    public int uid;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string id;

    public ETeam team;

    public Vector3 position;

    public ERole role;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketGameStart : Packet
{
    public int userNum;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public GameStartInfo[] startInfos = new GameStartInfo[20];
    
    public PacketGameStart():base((short)EProtocolID.PacketGameStart){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketGameOn : Packet
{
    public PacketGameOn():base((short)EProtocolID.PacketGameOn){}
}
#endregion

#region 게임 플레이 중

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketFeverStart : Packet
{
    public PacketFeverStart():base((short)EProtocolID.PacketFeverStart){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketTimerUpdate : Packet
{
    public int timeSeconds;
    
    public PacketTimerUpdate():base((short)EProtocolID.PacketTimerUpdate){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketTeamScoreUpdate : Packet
{
    public int uid;
    public int score;
    
    public PacketTeamScoreUpdate():base((short)EProtocolID.PacketTeamScoreUpdate){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketPlayerPosition : Packet
{
    public int uid;         // 유저의 uid를 확인하고 처리한다.
    public Vector3 position;
    public float rotation;
    
    public PacketPlayerPosition():base((short)EProtocolID.PacketPlayerPosition){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketPlayerFire : Packet
{
    public int ownerUID;
    public int bulletUID;
    public Vector3 position;
    public Vector3 direction;
    
    public PacketPlayerFire():base((short)EProtocolID.PacketPlayerFire){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketEntitySpawn : Packet
{
    public EEntity type;
    public int ownerUID;
    public int entityUID;
    public Vector3 position;
    public Vector3 velocity;
    public float rotation;
    
    public PacketEntitySpawn():base((short)EProtocolID.PacketEntitySpawn){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketEntityDestroy : Packet
{
    public EEntity type;
    public int entityUID;
    
    public PacketEntityDestroy():base((short)EProtocolID.PacketEntityDestroy){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketEntityPlayerCollision : Packet
{
    public int playerUID;

    public EEntity type;
    public int entityUID;
    
    public PacketEntityPlayerCollision():base((short)EProtocolID.PacketEntityPlayerCollision){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketPlayerDamage : Packet
{
    public int attackUID; // 공격한 플레이어
    public int targetUID; // 공격받은 플레이어
    
    public PacketPlayerDamage():base((short)EProtocolID.PacketPlayerDamage){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketBulletDestroy : Packet
{
    public int bulletUID;
    
    public PacketBulletDestroy():base((short)EProtocolID.PacketBulletDestroy){}
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketDashStart : Packet
{
    public int uid;
    
    public PacketDashStart():base((short)EProtocolID.PacketDashStart){}
}
#endregion

#region 게임 끝

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketGameEnd : Packet
{
    public ETeam winTeam;
    public int[] redTeamScores = new int[2];
    public int[] blueTeamScores = new int[2];
    
    public PacketGameEnd():base((short)EProtocolID.PacketGameEnd){}
}
#endregion