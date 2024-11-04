using MessagePack;

[Union(0, typeof(PacketUserJoin))]
[Union(1, typeof(PacketUserLeave))]
[Union(2, typeof(PacketReqUserInfo))]
[Union(3, typeof(PacketAnsUserInfo))]
[Union(4, typeof(PacketAnsUserList))]
[Union(5, typeof(PacketReqChangeTeam))]
[Union(6, typeof(PacketReqChangeRole))]
[Union(7, typeof(PacketGameReady))]
[Union(8, typeof(PacketGameReadyOk))]
[Union(9, typeof(PacketGameStart))]
[Union(10, typeof(PacketFeverStart))]
[Union(11, typeof(PacketTimerUpdate))]
[Union(12, typeof(PacketTeamScoreUpdate))]
[Union(13, typeof(PacketPlayerPosition))]
[Union(14, typeof(PacketPlayerFire))]
[Union(15, typeof(PacketEntitySpawn))]
[Union(16, typeof(PacketEntityDestroy))]
[Union(17, typeof(PacketEntityPlayerCollision))]
[Union(18, typeof(PacketPlayerDamage))]
[Union(19, typeof(PacketBulletDestroy))]
[Union(20, typeof(PacketGameEnd))]

[MessagePackObject]
public abstract partial class Packet
{
}
