using System.Net.Sockets;
using UnityEngine;

public class NetServer
{
    private bool _run;
    private Listener _listener = new Listener();

    public event System.Action<UserToken> onClientConnected;

    public bool GetRun() { return _run; }
    public void Start(int backlog)
    {
        if (_run)
        {
            return;
        }

        _listener.ClientConnected += OnClientConnected;
        _listener.Start(NetDefine.PORT, backlog);

        Debug.Log("[NetServer] 서버 시작");
        _run = true;
    }

    public void End()
    {
        if (!_run)
        {
            return;
        }

        _listener.Stop();
        _listener.ClientConnected -= OnClientConnected;

        Debug.Log("[NetServer] 서버 종료");
        _run = false;
    }

    private void OnClientConnected(Socket socket)
    {
        UserToken userToken = new UserToken(socket);
        userToken.SessionClosed += OnSessionClosed;
        userToken.OnConnected();
        userToken.StartReceive();

        MainThread.Instance.Add(() =>
        {
            onClientConnected?.Invoke(userToken);
        });

        Debug.Log("[NetServer] 클라이언트 접속");
    }

    private void OnSessionClosed(UserToken token)
    {
        Debug.Log("OnSessionClosed");
    }
}