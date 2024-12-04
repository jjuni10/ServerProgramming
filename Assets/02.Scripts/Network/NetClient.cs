using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetClient
{
    public enum SocketConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    public SocketConnectionState ConnectionState => _connectionState;

    public event Action<bool, UserToken> Connected;

    private Socket _socket;
    private UserToken _userToken;
    private SocketConnectionState _connectionState;

    public void Connect(string ip)
    {
        _connectionState = SocketConnectionState.Connecting;

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), NetDefine.PORT);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += OnConnected;
        args.RemoteEndPoint = endPoint;
        bool pending = _socket.ConnectAsync(args);
        if (!pending)
        {
            OnConnected(null, args);
        }

        Debug.Log("[NetClient]클라이언트 연결 시도");
    }


    public void Close()
    {
        _connectionState = SocketConnectionState.Disconnected;
        if (_userToken != null)
        {
            _userToken.SessionClosed -= OnSessionClosed;
            _userToken.Close();
        }
    }

    private void OnSessionClosed(UserToken token)
    {
        _connectionState = SocketConnectionState.Disconnected;
    }

    private void OnConnected(object sender, SocketAsyncEventArgs e)
    {
        bool success = false;
        _userToken = null;

        if (e.SocketError == SocketError.Success)
        {
            success = true;
            _connectionState = SocketConnectionState.Connected;
            _userToken = new UserToken(_socket);
            _userToken.OnConnected();
            _userToken.SessionClosed += OnSessionClosed;
            _userToken.StartReceive();
        }
        else
        {
            _connectionState = SocketConnectionState.Disconnected;
        }

        MainThread.Instance.Add(() =>
        {
            Connected?.Invoke(success, _userToken);
        });
    }
}