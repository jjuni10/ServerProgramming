using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using MessagePack;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;

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
    private ConcurrentQueue<byte[]> _sendQueue = new ConcurrentQueue<byte[]>();

    private CancellationTokenSource cts;
    private byte[] _buffer;
    private MemoryStream _memoryStream;

    public Socket Socket => _socket;

    public bool IsConnected => _curState == EState.Connected;
    public IPeer Peer => _peer;

    public event Action<UserToken> onSessionClosed;

    public UserToken(Socket socket)
    {
        _socket = socket;

        _memoryStream = new MemoryStream();
        _buffer = new byte[NetDefine.BUFFER_SIZE];
    }

    public void OnConnected()
    {
        _curState = EState.Connected;
    }

    public void SetPeer(IPeer peer)
    {
        _peer = peer;
    }


    public void StartReceiveAndSend()
    {
        cts = new CancellationTokenSource();
        Task.Run(ReceiveLoopAsync, cts.Token);
        Task.Run(SendLoopAsync, cts.Token);
    }

    private async Task ReceiveLoopAsync()
    {
        while (true)
        {
            Debug.Log("ReceiveLoopAsync.while");

            if (cts.IsCancellationRequested)
            {
                Debug.Log("[ReceiveLoopAsync] Cancellation Requested.");
                break;
            }

            try
            {
                int numReceive = await _socket.ReceiveAsync(new ArraySegment<byte>(_buffer), SocketFlags.None);

                byte[] messageBuffer = _buffer.AsSpan().Slice(0, numReceive).ToArray();             
                PacketMessageDispatcher.Instance.OnMessage(this, messageBuffer);
            }
            catch (SocketException)
            {
                Close();
                break;
            }
            catch (Exception ex)
            {
                MainThread.Instance.Add(() => Debug.LogError(ex.ToString()));
            }
        }
        Debug.Log("[ReceiveLoopAsync] Break");
    }
   
    public void OnMessage(byte[] messageBuffer)
    {
        _peer.ProcessMessage(messageBuffer, messageBuffer.Length);
    }

    private async Task SendLoopAsync()
    {
        while (true)
        {
            Debug.Log("SendLoopAsync.while");

            if (cts.IsCancellationRequested)
            {
                Debug.Log("[SendLoopAsync] Cancellation Requested.");
                break;
            }
            try
            {
                while (_sendQueue.Count > 0)
                {
                    if (_sendQueue.TryDequeue(out byte[] buffer))
                    {
                        await _socket.SendAsync(buffer, SocketFlags.None);
                    }
                }

                if (_curState == EState.ReserveClosing)
                {
                    _socket.Shutdown(SocketShutdown.Send);
                    break;
                }
            }
            catch (Exception ex)
            {
                Close();
                Debug.LogError("send error!! close socket. " + ex.Message);
                break;
            }
            finally
            {
                await Task.Delay(1);
            }
        }

        Debug.Log("[SendLoopAsync] Break");
    }


    public void Close()
    {
        if (_curState == EState.Closed)
        {
            return;
        }

        _curState = EState.Closed;
        _sendQueue.Clear();
        _socket.Close();

        cts.Cancel();
        cts.Dispose();

        MainThread.Instance.Add(() =>
        {
            onSessionClosed?.Invoke(this);
        });

        _peer.Remove();
        Debug.Log("[UserToken] Close");
    }

    public void Send(byte[] data)
    {
        _sendQueue.Enqueue(data);
    }

    public void Send(Packet packet)
    {
        try
        {
            Send(MessagePackSerializer.Serialize(packet));
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