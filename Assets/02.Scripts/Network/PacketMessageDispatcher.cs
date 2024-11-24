using System.Collections.Concurrent;
using UnityEngine;

public class PacketMessageDispatcher : MonoBehaviour, IMessage
{
    public static PacketMessageDispatcher Instance => _instance;
    private static PacketMessageDispatcher _instance;

    public struct PacketMessage
    {
        public UserToken token;
        public byte[] buffer;

        public PacketMessage(UserToken token, byte[] buffer)
        {
            this.token = token;
            this.buffer = buffer;
        }
    }

    private ConcurrentQueue<PacketMessage> _messageQueue;
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            Init();
        }
        else
        {
            Destroy(this);
        }
    }

    public void Init()
    {
        _messageQueue = new ConcurrentQueue<PacketMessage>();
    }


    private void Update()
    {
        while (_messageQueue.TryDequeue(out PacketMessage msg))
        {
            if (!msg.token.IsConnected)
                continue;

            msg.token.OnMessage(msg.buffer);
        }
    }

    public void OnMessage(UserToken token, byte[] buffer)
    {
        _messageQueue.Enqueue(new PacketMessage(token, buffer));
    }
}