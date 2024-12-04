using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Listener
{
    private SocketAsyncEventArgs _eventArgs;
    private Socket _listenSocket;

    public event Action<Socket> ClientConnected;

    private bool _isListening = false;
    private object lockObj = new object();

    public bool Start(int port, int backlog)
    {
        _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

        try
        {
            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(backlog);
            _isListening = true;

            _eventArgs = new SocketAsyncEventArgs();
            _eventArgs.Completed += OnAcceptCompleted;
            StartAccept();

            Debug.Log("[Listener] Start");
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("[Listener] Start Error: {0}", e.Message);
            _listenSocket.Close();
            return false;
        }

        return true;
    }

    public void Stop()
    {
        _listenSocket.Close();

        lock (lockObj)
        {
            _isListening = false;
        }
    }

    private void StartAccept()
    {
        _eventArgs.AcceptSocket = null;
        bool pending = false;
        try
        {
            pending = _listenSocket.AcceptAsync(_eventArgs);
        }
        catch { }

        if (!pending)
        {
            OnAcceptCompleted(null, _eventArgs);
        }
    }

    private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            Debug.Log("[Listener] Accept Success");
            ClientConnected?.Invoke(e.AcceptSocket);
        }
        else
        {
            Debug.Log("[Listener] Accept Fail");
        }

        lock (lockObj)
        {
            if (!_isListening)
            {
                return;
            }
        }
        StartAccept();
    }
}