using System.Data;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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
    private string _serverIP;

    public void Connect(string ip)
    {
        _serverIP = ip;
        Task.Run(ConnectAsync);
        Debug.Log("[NetClient] 클라이언트 연결 시도");
    }

    private async Task ConnectAsync()
    {
        _connectionState = SocketConnectionState.Connecting;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _userToken = null;

        bool success = true;
        try
        {
            await _socket.ConnectAsync(IPAddress.Parse(_serverIP), NetDefine.PORT);
            _connectionState = SocketConnectionState.Connected;

            _userToken = new UserToken(_socket);
            _userToken.OnConnected();
            _userToken.onSessionClosed += OnSessionClosed;
            _userToken.StartReceiveAndSend();
        }
        catch (Exception ex)
        {
            _connectionState = SocketConnectionState.Disconnected;
            success = false;

            Debug.LogException(ex);
        }
        finally
        {
            MainThread.Instance.Add(() =>
            {
                Connected?.Invoke(success, _userToken);
            });
        }
    }

    public void Close()
    {
        _connectionState = SocketConnectionState.Disconnected;
        if (_userToken != null)
        {
            _userToken.onSessionClosed -= OnSessionClosed;
            _userToken.Close();
        }
    }

    private void OnSessionClosed(UserToken token)
    {
        _connectionState = SocketConnectionState.Disconnected;
    }
}