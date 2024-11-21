using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

class Listener
{
    private SocketAsyncEventArgs _acceptArgs;
    private Socket _listenSocket;
    private bool isListening;

    public event Action<Socket> onClientConnected;

    public bool Start(int port, int backlog)
    {
        _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);

        try
        {
            _listenSocket.Bind(endpoint);
            _listenSocket.Listen(backlog);
            isListening = true;

            _acceptArgs = new SocketAsyncEventArgs();
            _acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            StartAccept();
        }
        catch (Exception e)
        {
            Debug.LogError($"Start Error {e.Message}");
            return false;
        }

        return true;
    }

    public void Stop()
    {
        isListening = false;
        _listenSocket.Close();
    }

    private void StartAccept()
    {
        if (!isListening) return;

        _acceptArgs.AcceptSocket = null;
        bool pending = false;
        try
        {
            pending = _listenSocket.AcceptAsync(_acceptArgs);
        }
        catch
        {
        }

        if (!pending)
        {
            OnAcceptCompleted(null, _acceptArgs);
        }
    }
    
    void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            Debug.Log("Accept Success");
            onClientConnected?.Invoke(e.AcceptSocket);
        }
        else
        {
            Debug.Log("Accept Fail");
        }

        StartAccept();
    }
}