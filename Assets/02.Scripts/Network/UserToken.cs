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

        _sendStream = new MemoryStream(NetDefine.SOCKET_BUFFER_SIZE);

        _sendBuffer = new byte[NetDefine.SOCKET_BUFFER_SIZE];
        _recvBuffer = new byte[NetDefine.SOCKET_BUFFER_SIZE];
        _dataBuffer = new byte[NetDefine.DATA_BUFFER_SIZE];

        _receiveEventArgs = new SocketAsyncEventArgs();
        _receiveEventArgs.UserToken = this;
        _receiveEventArgs.Completed += OnReceiveCompleted;
        _receiveEventArgs.SetBuffer(_recvBuffer, 0, _recvBuffer.Length);

        _sendEventArgs = new SocketAsyncEventArgs();
        _sendEventArgs.UserToken = this;
        _sendEventArgs.Completed += OnSendCompleted;

        Debug.Log("UserToken ���� / socket: " + _socket.GetHashCode());
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

        try
        {
            ResolveMessage(e.Buffer, e.Offset, e.BytesTransferred);
            StartReceive();
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Error on OnReceiveCompleted!\n{0}", ex.ToString());
        }
    }


    // ���Ͽ��� �޾ƿ� ��Ŷ �����͸� ó���Ѵ�
    private void ResolveMessage(byte[] buffer, int offset, int numTransferred)
    {
        int combinedCount = 0;
        while (true)
        {
            bool shouldReceiveMore = false;
            int newOffset = 0;

            if (_isReceivingHeader)
            {
                Array.Copy(buffer, offset, _dataBuffer, _curPosition, numTransferred);
                _curPosition += numTransferred;

                if (_curPosition >= NetDefine.HEADER_SIZE)
                {
                    // ����� ��� �� �޾ƿ°�� �޽����� �޴� ���·� ��ȯ�Ѵ�. (-> RecvMessage)
                    _packetSize = BitConverter.ToUInt16(_dataBuffer, 0);
                    _curPosition -= NetDefine.HEADER_SIZE;
                    _isReceivingHeader = false;
                    if (_curPosition > 0)
                    {
                        // ���� ����Ʈ�� �� �����ִٸ� �޽����� ���ۿ��� �ٷ� �д´�.
                        shouldReceiveMore = true;
                        newOffset = NetDefine.HEADER_SIZE;
                    }
                }
            }
            else
            {
                Array.Copy(buffer, offset, _dataBuffer, _curPosition, numTransferred);
                _curPosition += numTransferred;
                if (_curPosition >= _packetSize)
                {
                    // �޽����� ��� �� �޾ƿ°�� ����� �޴� ���·� ��ȯ�Ѵ�. (-> RecvHeader)
                    var segment = new ArraySegment<byte>(_dataBuffer, 0, _packetSize);
                    OnMessageCompleted(segment);

                    _curPosition -= _packetSize;
                    _isReceivingHeader = true;
                    if (_curPosition > 0)
                    {
                        // ���� ����Ʈ�� �� �����ִٸ� �޽����� ���ۿ��� �ٷ� �д´�.
                        shouldReceiveMore = true;
                        combinedCount++;
                        newOffset = _packetSize;
                    }
                }
            }

            if (shouldReceiveMore)
            {
                int remaining = _curPosition;
                _curPosition = 0;

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
            Debug.LogWarning($"{combinedCount}ȸ �������� ����!");
        }
    }

    private void OnMessageCompleted(ArraySegment<byte> segment)
    {
        try
        {
            Packet packet = MessagePackSerializer.Deserialize<Packet>(segment, out _);
            if (packet is not PacketPlayerPosition)
            {
                Debug.LogFormat("[UserToken] {0} received: {1}", peerTag, packet);
            }
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

    private void StartSend(ArraySegment<byte> segment)
    {
        try
        {
            segment.CopyTo(_sendBuffer, 0);
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
        finally
        {
            ArrayPool<byte>.Shared.Return(segment.Array);
        }
    }

    private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            Debug.LogError("Socket Error: " + e.SocketError);
            return;
        }


        bool shouldStartSend = false;
        lock (_sendQueue)
        {
            _sendQueue.Dequeue();
            shouldStartSend = (_sendQueue.Count > 0);
        }

        if (shouldStartSend)
        {
            StartSend(_sendQueue.Peek());
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
            bool shouldStartSend = false;
            lock (_sendQueue)
            {
                _sendQueue.Enqueue(AssemblePacket(packet));
                shouldStartSend = (_sendQueue.Count == 1);
            }

            if (shouldStartSend)
            {
                StartSend(_sendQueue.Peek());
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

        // ��Ŷ�� ��Ʈ���� ����ȭ �� ��Ŷ ���� ���
        MessagePackSerializer.Serialize(_sendStream, packet);
        int totalSize = (int)_sendStream.Length;
        int packetSize = totalSize - NetDefine.HEADER_SIZE;

        // ��Ʈ���� ó�� 2����Ʈ�� ��Ŷ ���̸� ����Ʈ�� ��ȯ�� ����� �����
        byte[] sendStreamBuffer = _sendStream.GetBuffer();
        BitConverter.TryWriteBytes(sendStreamBuffer.AsSpan(), (ushort)packetSize);

        byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(totalSize);
        Array.Copy(sendStreamBuffer, sendBuffer, totalSize);

        // ť�� ���� ��Ŷ�� �ش��ϴ� �κи� �߰� (MemoryStream�� buffer ũ�Ⱑ ���� �þ �� ����)
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