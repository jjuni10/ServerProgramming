using UnityEngine;
using System.Net.Sockets;
using System;
using MessagePack;
using System.IO;
using System.Collections.Concurrent;
using System.Buffers;
using System.Threading;
using System.Collections.Generic;

public class UserToken
{
    private enum State
    {
        Idle,
        Connected,
        ReserveClosing,
        Closed
    }

    private enum ReceiveState
    {
        RecvHeader,
        RecvMessage,
    }

    private Socket _socket;
    private State _curState = State.Idle;
    private bool _isReceivingHeader = false;

    private IPeer _peer;

    private byte[] _recvBuffer;
    private byte[] _dataBuffer;
    private byte[] _sendBuffer;

    private int _curPosition = 0;
    private int _packetSize = 0;

    private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    private MemoryStream _sendStream;

    public Socket Socket => _socket;
    private SocketAsyncEventArgs _receiveEventArgs;
    private SocketAsyncEventArgs _sendEventArgs;

    public bool IsConnected => _curState == State.Connected;
    public IPeer Peer => _peer;
    private string peerTag = string.Empty;

    public event Action<UserToken> SessionClosed;

    public UserToken(Socket socket)
    {
        _socket = socket;

        _sendStream = new MemoryStream(NetDefine.BUFFER_SIZE);

        _sendBuffer = new byte[NetDefine.BUFFER_SIZE];
        _recvBuffer = new byte[NetDefine.BUFFER_SIZE];
        _dataBuffer = new byte[NetDefine.BUFFER_SIZE];

        _receiveEventArgs = new SocketAsyncEventArgs();
        _receiveEventArgs.UserToken = this;
        _receiveEventArgs.Completed += OnReceiveCompleted;
        _receiveEventArgs.SetBuffer(_recvBuffer, 0, _recvBuffer.Length);

        _sendEventArgs = new SocketAsyncEventArgs();
        _sendEventArgs.UserToken = this;
        _sendEventArgs.Completed += OnSendCompleted;

        Debug.Log("UserToken 생성 / socket: " + _socket.GetHashCode());
    }


    public void OnConnected()
    {
        _curState = State.Connected;
        _isReceivingHeader = true;
        _curPosition = 0;
    }

    public void SetPeer(IPeer peer)
    {
        _peer = peer;
        peerTag = peer.GetType().Name + "#" + peer.GetHashCode();
    }


    public void StartReceive()
    {
        bool pending = false;
        try
        {
            pending = _socket.ReceiveAsync(_receiveEventArgs);
        }
        catch
        {

        }

        if (!pending)
        {
            OnReceiveCompleted(null, _receiveEventArgs);
        }
    }

    private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.LastOperation != SocketAsyncOperation.Receive)
        {
            _socket.Close();
            return;
        }

        ResolveMessage(e.Buffer, e.Offset, e.BytesTransferred);
        StartReceive();

    }


    // 소켓에서 받아온 패킷 데이터를 처리한다
    private void ResolveMessage(byte[] buffer, int offset, int numTransferred)
    {
        int combinedCount = 0;
        while (true)
        {
            int newOffset = 0;
            if (_isReceivingHeader)
            {
                Array.Copy(buffer, offset, _dataBuffer, _curPosition, numTransferred);
                _curPosition += numTransferred;

                if (_curPosition >= NetDefine.HEADER_SIZE)
                {
                    // 헤더를 모두 다 받아온경우 메시지를 받는 상태로 전환한다. (-> RecvMessage)
                    _packetSize = BitConverter.ToUInt16(_dataBuffer, 0);
                    _curPosition -= NetDefine.HEADER_SIZE;
                    _isReceivingHeader = false;
                    newOffset = NetDefine.HEADER_SIZE;
                }
            }
            else
            {
                Array.Copy(buffer, offset, _dataBuffer, _curPosition, numTransferred);
                _curPosition += numTransferred;
                if (_curPosition >= _packetSize)
                {
                    // 메시지를 모두 다 받아온경우 헤더를 받는 상태로 전환한다. (-> RecvHeader)
                    var segment = new ArraySegment<byte>(_dataBuffer, 0, _packetSize);
                    OnMessageCompleted(segment);

                    _curPosition -= _packetSize;
                    _isReceivingHeader = true;
                    newOffset = _packetSize;
                }
            }

            if (_curPosition > 0)
            {
                // 만약 바이트가 더 남아있다면 메시지를 버퍼에서 바로 읽는다.
                int remaining = _curPosition;
                _curPosition = 0;

                if (newOffset != NetDefine.HEADER_SIZE)
                {
                    combinedCount++;
                }

                // ResolveMessage(_dataBuffer, newOffset, remaining);
                buffer = _dataBuffer;
                offset = newOffset;
                numTransferred = remaining;
            }
            else
            {
                break;
            }
        }

        if (combinedCount > 0)
        {
            Debug.LogWarning($"{combinedCount}회 합쳐져서 들어옴!");
        }
    }


    private void OnMessageCompleted(ArraySegment<byte> segment)
    {
        try
        {
            Packet packet = MessagePackSerializer.Deserialize<Packet>(segment, out _);
            //Debug.LogFormat("[UserToken] {0} received: {1}", peerTag, packet);
            PacketMessageDispatcher.Instance.OnMessage(this, packet);
        }
        catch (Exception ex)
        {
            Debug.LogError("[UserToken] Error (at OnMessageCompleted)! " + ex.ToString());
        }
    }

    public void OnMessage(Packet packet)
    {
        _peer.OnReceive(packet);
    }

    private void StartSend()
    {
        ArraySegment<byte> segment = _sendQueue.Peek();

        try
        {
            segment.CopyTo(_sendBuffer, 0);

            ArrayPool<byte>.Shared.Return(segment.Array);
            _sendEventArgs.SetBuffer(_sendBuffer, 0, segment.Count);

            bool pending = _socket.SendAsync(_sendEventArgs);
            if (!pending)
            {
                OnSendCompleted(null, _sendEventArgs);
            }
        }
        catch (Exception ex)
        {
            if (_socket == null)
            {
                Close();
                return;
            }

            Debug.LogErrorFormat("[UserToken] {0}: send error!! close socket. {1}", peerTag, ex.ToString());
        }
    }

    private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            Debug.LogError("Socket Error: " + e.SocketError);
            return;
        }

        lock (_sendQueue)
        {
            _sendQueue.Dequeue();

            if (_sendQueue.Count > 0)
            {
                StartSend();
                return;
            }
        }

        if (_curState == State.ReserveClosing)
        {
            _socket.Shutdown(SocketShutdown.Send);
        }
    }

    public void Close()
    {
        if (_curState == State.Closed)
        {
            return;
        }

        _curState = State.Closed;
        _socket.Close();

        MainThread.Instance.Add(() =>
        {
            SessionClosed?.Invoke(this);
        });

        _peer.Remove();
        Debug.Log("[UserToken] Close");
    }

    public void Send(Packet packet)
    {
        if (_curState != State.Connected)
        {
            return;
        }

        try
        {
            //Debug.LogFormat("[UserToken] {0} Send: {1}", peerTag, packet);
            lock (_sendQueue)
            {
                _sendQueue.Enqueue(AssemblePacket(packet));
                if (_sendQueue.Count > 1)
                {
                    return;
                }
                StartSend();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[UserToken] Send Error! " + ex.ToString());
        }
    }

    private ArraySegment<byte> AssemblePacket(Packet packet)
    {
        _sendStream.SetLength(0);
        _sendStream.WriteByte(0);
        _sendStream.WriteByte(0);

        // 패킷을 스트림에 직렬화 및 패킷 길이 계산
        MessagePackSerializer.Serialize(_sendStream, packet);
        int totalSize = (int)_sendStream.Length;
        int packetSize = totalSize - NetDefine.HEADER_SIZE;

        // 스트림의 처음 2바이트를 패킷 길이를 바이트로 변환한 결과로 덮어씌움
        byte[] sendStreamBuffer = _sendStream.GetBuffer();
        BitConverter.TryWriteBytes(sendStreamBuffer.AsSpan(), (ushort)packetSize);

        byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(totalSize);
        Array.Copy(sendStreamBuffer, sendBuffer, totalSize);

        // 큐에 보낼 패킷에 해당하는 부분만 추가 (MemoryStream의 buffer 크기가 도중 늘어날 수 있음)
        return new ArraySegment<byte>(sendBuffer, 0, totalSize);
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

            _curState = State.ReserveClosing;
        }
        catch (Exception)
        {
            Close();
        }
    }
}