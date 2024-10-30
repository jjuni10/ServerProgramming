// SC : 서버->클라, CS : 클라->서버, REL : 중계형
using UnityEngine;
using System.Runtime.InteropServices;

public enum EProtocolID
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
    REL_PLAYER_POINT,
    REL_BULLET_DISTROY,
    SC_GAME_END,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketReqUserInfo : Packet
{
    public int uid;
    public ETeam team;

    public PacketReqUserInfo()
        : base((short)EProtocolID.SC_REQ_USERINFO)
    {
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketAnsUserInfo : Packet
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string id;
    public bool host;

    public PacketAnsUserInfo()
        : base((short)EProtocolID.CS_ANS_USERINFO)
    {
    }
}

// 유저 리스트를 배열로 보내야 하기 때문에 struct로 만들어야 한다.
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UserInfo
{
    public int uid;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string id;
    public ETeam team;
    public bool host;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketAnsUserList : Packet
{
    public int userNum;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public UserInfo[] userInfos = new UserInfo[20];
    public PacketAnsUserList()
        : base ((short)EProtocolID.SC_ANS_USERLIST)
    {
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketReqChangeTeam : Packet
{
    public ETeam team;
    public PacketReqChangeTeam()
        : base ((short)EProtocolID.CS_REQ_CHANGE_TEAM)
    {
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketGameReady : Packet
{
    public PacketGameReady()
        : base ((short)EProtocolID.REL_GAME_READY)
    {
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketGameReadyOk : Packet
{
    public PacketGameReadyOk()
        : base((short)EProtocolID.CS_GAME_READY_OK)
    {
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GameStartInfo
{
    public int uid;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string id;
    public ETeam team;
    public Vector3 position;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketGameStart : Packet
{
    public int userNum;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public GameStartInfo[] startInfos = new GameStartInfo[20];

    public PacketGameStart()
        : base((short)EProtocolID.SC_GAME_START)
    {
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketPlayerPosition : Packet
{
    public int uid;         // 유저의 uid를 확인하고 처리한다.
    public Vector3 position;
    public float rotation;

    public PacketPlayerPosition()
        : base((short)EProtocolID.REL_PLAYER_POSITION)
    {
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketPlayerFire : Packet
{
    public int ownerUID;
    public int bulletUID;
    public Vector3 position;
    public Vector3 direction;

    public PacketPlayerFire()
        : base((short)EProtocolID.REL_PLAYER_FIRE)
    {

    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketPlayerDamage : Packet
{
    public int attackUID; // 공격한 플레이어
    public int targetUID; // 공격받은 플레이어
    public PacketPlayerDamage()
        : base((short)EProtocolID.REL_PLAYER_POINT)
    {

    }

}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketBulletDistroy : Packet
{
    public int bulletUID;
    public PacketBulletDistroy()
        : base((short)EProtocolID.REL_BULLET_DISTROY)
    {

    }

}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PacketGameEnd : Packet
{
    public ETeam winTeam;
    public PacketGameEnd()
        : base((short)EProtocolID.SC_GAME_END)
    {

    }

}