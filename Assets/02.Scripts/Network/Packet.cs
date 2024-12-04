using System.Runtime.InteropServices;
using System;

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
[Union(10, typeof(PacketGameOn))]
[Union(11, typeof(PacketFeverStart))]
[Union(12, typeof(PacketTimerUpdate))]
[Union(13, typeof(PacketTeamScoreUpdate))]
[Union(14, typeof(PacketPlayerPosition))]
[Union(15, typeof(PacketPlayerFire))]
[Union(16, typeof(PacketEntitySpawn))]
[Union(17, typeof(PacketEntityDestroy))]
[Union(18, typeof(PacketEntityPlayerCollision))]
[Union(19, typeof(PacketPlayerDamage))]
[Union(20, typeof(PacketBulletDestroy))]
[Union(21, typeof(PacketDashStart))]
[Union(22, typeof(PacketLatencyTest))]
[Union(23, typeof(PacketGameEnd))]

[MessagePackObject]
public abstract partial class Packet
{
    private short _size;
    private short _protocolID;

    public short Size => _size;
    public short ProtocolID => _protocolID;

    public Packet(short protocolID)
    {
        _size = (short)Marshal.SizeOf(this);
        _protocolID = protocolID;
    }

    public byte[] ToByte()
    {
        // Send Packet;
        byte[] buffer = new byte[Marshal.SizeOf(this)];

        // unsafe 포인터 사용하기 위해
        // 빌드 설정 allow unsafe code, Unity : Player - OtherSetting
        // fixed unsafe 내에서만 사용가능 메모리를 안전하게 사용할수 있다
        unsafe
        {
            fixed (byte* fixed_buffer = buffer)
            {
                Marshal.StructureToPtr(this, (IntPtr)fixed_buffer, false);
            }
        }

        return buffer;
    }

    public void ToPacket(byte[] buffer)
    {
        unsafe
        {
            fixed (byte* fixed_buffer = buffer)
            {
                Marshal.PtrToStructure((IntPtr)fixed_buffer, this);
            }
        }
    }
}