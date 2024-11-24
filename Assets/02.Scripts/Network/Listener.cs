using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Listener
{
    public event Action<Socket> ClientConnected;

    private Socket listenSocket;
    private CancellationTokenSource cts;

    public void Start(int port, int backlog)
    {
        cts = new CancellationTokenSource();
        listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        listenSocket.Bind(endPoint);
        listenSocket.Listen(backlog);

        Task.Run(AcceptLoopAsync);
    }

    public void Stop()
    {
        cts.Cancel();
        cts.Dispose();
        listenSocket.Close();
    }

    private async Task AcceptLoopAsync()
    {
        while (!cts.IsCancellationRequested)
        {
            Debug.Log("[Listener] 새 클라이언트를 기다리는 중..");
            try
            {
                Socket newClient = await listenSocket.AcceptAsync();
                ClientConnected?.Invoke(newClient);
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        MainThread.Instance.Add(() => Debug.Log("<b>Listener Task가 중단됨</b>"));
    }
}