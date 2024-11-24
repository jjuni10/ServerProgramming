using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System;

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

    private SocketAsyncEventArgs _receiveEventArgs;
    private SocketAsyncEventArgs _sendEventArgs;
    private MessageResolver _messageResolver = new MessageResolver(NetDefine.BUFFER_SIZE * 3);
    private IPeer _peer;
    private List<byte[]> _sendingList = new List<byte[]>();
    private IMessage _dispatcher;

    public Socket Socket => _socket;
    public SocketAsyncEventArgs ReceiveEventArgs => _receiveEventArgs;
    public SocketAsyncEventArgs SendEventArgs => _sendEventArgs;
    public bool IsConnected => _curState == EState.Connected;
    public IPeer Peer => _peer;

    public event Action<UserToken> onSessionClosed;

    public UserToken(Socket socket, IMessage dispatcher)
    {
        _socket = socket;
        _dispatcher = dispatcher;

        _receiveEventArgs = new SocketAsyncEventArgs();
        _receiveEventArgs.UserToken = this;
        _receiveEventArgs.Completed += OnReceiveCompleted;
        _receiveEventArgs.SetBuffer(new byte[NetDefine.BUFFER_SIZE], 0, NetDefine.BUFFER_SIZE);

        _sendEventArgs = new SocketAsyncEventArgs();
        _sendEventArgs.UserToken = this;
        _sendEventArgs.Completed += OnSendComplteted;
    }

    public void OnConnected()
    {
        _curState = EState.Connected;
    }

    public void SetPeer(IPeer peer)
    {
        _peer = peer;
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
        if (e.LastOperation == SocketAsyncOperation.Receive)
        {
            _messageResolver.OnReceive(e.Buffer, e.Offset, e.BytesTransferred, OnMessageComplete);
        }
        else
        {
            _socket.Close();
        }

        StartReceive();
    }

    private void OnMessageComplete(byte[] buffer)
    {
        _dispatcher.OnMessage(this, buffer);
    }

    public void OnMessage(byte[] buffer)
    {
        if (_peer == null)
            return;

        short protocolID = BitConverter.ToInt16(buffer, 2);
        try
        {
            _peer.ProcessMessage(protocolID, buffer);
        }
        catch (Exception ex)
        {
            MainThread.Instance.Add(() => Debug.LogError(ex.ToString()));
        }
    }

    public void Close()
    {
        if (_curState == EState.Closed)
        {
            return;
        }

        _curState = EState.Closed;
        _socket.Close();

        _socket = null;

        _sendingList.Clear();

        MainThread.Instance.Add(() =>
        {
            onSessionClosed?.Invoke(this);
        });
       
        _peer.Remove();

        Debug.Log("Close");
    }

    public void Send(byte[] data)
    {
        lock (_sendingList)
        {
            _sendingList.Add(data);

            if (_sendingList.Count > 1)
            {
                return;
            }

            StartSend();
        }
    }

    public void Send(Packet packet)
    {
        Send(packet.ToByte());
    }

    void StartSend()
    {
        try
        {
            _sendEventArgs.SetBuffer(_sendingList[0], 0, _sendingList[0].Length);

            bool pending = _socket.SendAsync(_sendEventArgs);
            if (!pending)
            {
                OnSendComplteted(null, _sendEventArgs);
            }
        }
        catch (Exception e)
        {
            if (_socket == null)
            {
                Close();
                return;
            }

            Debug.LogError("send error!! close socket. " + e.Message);
        }
    }

    public void OnSendComplteted(object sender, SocketAsyncEventArgs e)
    {
        if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
        {
            return;
        }

        lock (_sendingList)
        {
            _sendingList.RemoveAt(0);
            if (_sendingList.Count > 0)
            {
                StartSend();
                return;
            }

            if (_curState == EState.ReserveClosing)
            {
                _socket.Shutdown(SocketShutdown.Send);
            }
        }
    }

    public void Disconnect()
    {
        try
        {
            if (_sendingList.Count <= 0)
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