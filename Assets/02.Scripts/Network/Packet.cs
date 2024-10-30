using System.Runtime.InteropServices;
using System;
using MessagePack;

/*
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
    SC_GAME_END
*/

[Union(0, typeof(PacketReqUserInfo))]
[Union(1, typeof(PacketAnsUserInfo))]
[Union(2, typeof(PacketAnsUserList))]
[Union(3, typeof(PacketReqChangeTeam))]
[Union(4, typeof(PacketGameReady))]
[Union(5, typeof(PacketGameReadyOk))]
[Union(6, typeof(PacketGameStart))]
[Union(7, typeof(PacketPlayerPosition))]
[Union(8, typeof(PacketPlayerFire))]
[Union(9, typeof(PacketPlayerDamage))]
[Union(10, typeof(PacketBulletDestroy))]
[Union(11, typeof(PacketGameEnd))]
[MessagePackObject]
public abstract class Packet
{
}