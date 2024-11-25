using UnityEngine;
using System.Net.Sockets;
using System;
using MessagePack;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;
using System.Buffers;

public class UserToken
{
    private enum EState
    {
        Idle,
        Connected,
        ReserveClosing,
        Closed
    }

    private Socket _socket;
    private EState _curState = EState.Idle;

    private IPeer _peer;

    private BlockingCollection<ArraySegment<byte>> _sendQueue = new BlockingCollection<ArraySegment<byte>>();

    private CancellationTokenSource cts;
    private byte[] _buffer;
    private MemoryStream _sendStream;

    public Socket Socket => _socket;

    public bool IsConnected => _curState == EState.Connected;
    public IPeer Peer => _peer;
    private string peerTag = string.Empty;

    public event Action<UserToken> onSessionClosed;

    public UserToken(Socket socket)
    {
        _socket = socket;

        _sendStream = new MemoryStream(NetDefine.BUFFER_SIZE);
        _buffer = new byte[NetDefine.BUFFER_SIZE];
        Debug.Log("UserToken 생성 / socket: " + _socket.GetHashCode());
    }

    public void OnConnected()
    {
        _curState = EState.Connected;
    }

    public void SetPeer(IPeer peer)
    {
        _peer = peer;
        peerTag = peer.GetType().Name + "#" + peer.GetHashCode();
    }


    public void StartReceiveAndSend()
    {
        cts = new CancellationTokenSource();
        Task.Run(ReceiveLoopAsync);
        Task.Run(SendLoopAsync);
    }

    private async Task ReceiveLoopAsync()
    {
        var cancellationToken = cts.Token;
        byte[] lengthBuffer = new byte[2];
        while (true)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("[ReceiveLoopAsync] Cancellation Requested. " + peerTag);
                    break;
                }

                /*
                 * .NET Standard 2.1 기준 Socket.ReceiveAsync에는 CancellationToken이 적용되지 않는 것으로 보임
                 * 따라서 Socket.Close()할 때 ReceiveAsync에서 발생하는 ObjectDisposedException을 캐치해서
                 * Socket.ReceiveAsync 작업을 취소시키는 것으로 우회함
                 * 
                 * 관련 링크
                 * https://discussions.unity.com/t/unable-to-cancel-socket-receiveasync-with-cancellationtokensource-in-unity-unity-version-2022-3-26/947022/4
                 */

                // 1. 패킷 사이즈 읽기
                int totalRead = 0;
                while (totalRead < 2)
                {
                    var segment = new ArraySegment<byte>(lengthBuffer, totalRead, lengthBuffer.Length - totalRead);
                    totalRead += await _socket.ReceiveAsync(segment, SocketFlags.None);
                }

                int messageLength = BitConverter.ToUInt16(lengthBuffer);

                // 2. 패킷 사이즈만큼 필요한 바이트 읽어오기
                totalRead = 0;
                while (totalRead < messageLength)
                {
                    var segment = new ArraySegment<byte>(_buffer, totalRead, messageLength - totalRead);
                    totalRead += await _socket.ReceiveAsync(segment, SocketFlags.None);
                }

                var packetSegment = new ArraySegment<byte>(_buffer, 0, messageLength);
                Packet packet = MessagePackSerializer.Deserialize<Packet>(packetSegment, out _);
                PacketMessageDispatcher.Instance.OnMessage(this, packet);
            }
            catch (SocketException e)
            {
                Debug.LogWarning(peerTag + " " + e.ToString());
                Close();
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        Debug.Log("[ReceiveLoopAsync] Terminated " + peerTag);
    }

    public void OnMessage(Packet packet)
    {
        _peer.OnReceive(packet);
    }

    private async Task SendLoopAsync()
    {
        var cancellationToken = cts.Token;
        while (true)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("[SendLoopAsync] Cancellation Requested. " + peerTag);
                    break;
                }

                ArraySegment<byte> segment = _sendQueue.Take(cancellationToken);
                await _socket.SendAsync(segment, SocketFlags.None, cancellationToken);

                // (중요!) ArrayPool에서 빌린 byte[] 배열을 반납해야함
                ArrayPool<byte>.Shared.Return(segment.Array);

                if (_curState == EState.ReserveClosing)
                {
                    _socket.Shutdown(SocketShutdown.Send);
                    break;
                }
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Close();
                Debug.LogError("send error!! close socket. " + ex.ToString());
            }
        }

        _sendQueue.Dispose();
        Debug.Log("[SendLoopAsync] Terminated " + peerTag);
    }


    public void Close()
    {
        if (_curState == EState.Closed)
        {
            return;
        }

        _curState = EState.Closed;

        _socket.Close();
        cts.Cancel();

        MainThread.Instance.Add(() =>
        {
            onSessionClosed?.Invoke(this);
        });

        _peer.Remove();
        Debug.Log("[UserToken] Close");
    }

    public void Send(byte[] data)
    {
        if (_curState == EState.Connected)
        {
            _sendQueue.Add(data);
        }
    }

    public void Send(Packet packet)
    {
        if (_curState != EState.Connected)
        {
            return;
        }

        try
        {
            // 스트림의 위치, 길이를 모두 0으로 하고
            // 길이 필드를 임시로 작성 (값을 0으로)
            const int HeaderSize = 2;

            _sendStream.SetLength(0);
            _sendStream.WriteByte(0);
            _sendStream.WriteByte(0);

            // 패킷을 스트림에 직렬화 및 패킷 길이 계산
            MessagePackSerializer.Serialize(_sendStream, packet);
            int totalSize = (int)_sendStream.Length;
            int packetSize = totalSize - HeaderSize;

            // 스트림의 처음 2바이트를 패킷 길이를 바이트로 변환한 결과로 덮어씌움
            byte[] sendStreamBuffer = _sendStream.GetBuffer();
            BitConverter.TryWriteBytes(sendStreamBuffer.AsSpan(), (ushort)packetSize);

            // ArrayPool에서 전체크기 (길이+패킷)만큼 해당하는 byte[] 배열을 빌려와서 스트림의 내용을 배열에 복사
            byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(totalSize);
            Array.Copy(sendStreamBuffer, sendBuffer, totalSize);

            // 큐에 보낼 패킷에 해당하는 부분만 추가 (MemoryStream의 buffer 크기가 도중 늘어날 수 있음)
            _sendQueue.Add(new ArraySegment<byte>(sendBuffer, 0, totalSize));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void Disconnect()
    {
        try
        {
            if (_sendQueue.Count <= 0)
            {
                _socket.Shutdown(SocketShutdown.Send);
                return;
            }

            _curState = EState.ReserveClosing;
        }
        catch (Exception)
        {
            Close();
        }
    }
}