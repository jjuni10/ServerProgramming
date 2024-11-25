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
        Debug.Log("UserToken ���� / socket: " + _socket.GetHashCode());
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
                 * .NET Standard 2.1 ���� Socket.ReceiveAsync���� CancellationToken�� ������� �ʴ� ������ ����
                 * ���� Socket.Close()�� �� ReceiveAsync���� �߻��ϴ� ObjectDisposedException�� ĳġ�ؼ�
                 * Socket.ReceiveAsync �۾��� ��ҽ�Ű�� ������ ��ȸ��
                 * 
                 * ���� ��ũ
                 * https://discussions.unity.com/t/unable-to-cancel-socket-receiveasync-with-cancellationtokensource-in-unity-unity-version-2022-3-26/947022/4
                 */

                // 1. ��Ŷ ������ �б�
                int totalRead = 0;
                while (totalRead < 2)
                {
                    var segment = new ArraySegment<byte>(lengthBuffer, totalRead, lengthBuffer.Length - totalRead);
                    totalRead += await _socket.ReceiveAsync(segment, SocketFlags.None);
                }

                int messageLength = BitConverter.ToUInt16(lengthBuffer);

                // 2. ��Ŷ �����ŭ �ʿ��� ����Ʈ �о����
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

                // (�߿�!) ArrayPool���� ���� byte[] �迭�� �ݳ��ؾ���
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
            // ��Ʈ���� ��ġ, ���̸� ��� 0���� �ϰ�
            // ���� �ʵ带 �ӽ÷� �ۼ� (���� 0����)
            const int HeaderSize = 2;

            _sendStream.SetLength(0);
            _sendStream.WriteByte(0);
            _sendStream.WriteByte(0);

            // ��Ŷ�� ��Ʈ���� ����ȭ �� ��Ŷ ���� ���
            MessagePackSerializer.Serialize(_sendStream, packet);
            int totalSize = (int)_sendStream.Length;
            int packetSize = totalSize - HeaderSize;

            // ��Ʈ���� ó�� 2����Ʈ�� ��Ŷ ���̸� ����Ʈ�� ��ȯ�� ����� �����
            byte[] sendStreamBuffer = _sendStream.GetBuffer();
            BitConverter.TryWriteBytes(sendStreamBuffer.AsSpan(), (ushort)packetSize);

            // ArrayPool���� ��üũ�� (����+��Ŷ)��ŭ �ش��ϴ� byte[] �迭�� �����ͼ� ��Ʈ���� ������ �迭�� ����
            byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(totalSize);
            Array.Copy(sendStreamBuffer, sendBuffer, totalSize);

            // ť�� ���� ��Ŷ�� �ش��ϴ� �κи� �߰� (MemoryStream�� buffer ũ�Ⱑ ���� �þ �� ����)
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