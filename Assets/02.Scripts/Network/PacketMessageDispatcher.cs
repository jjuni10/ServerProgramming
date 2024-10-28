using System.Collections.Concurrent;
using UnityEngine;

public class PacketMessageDispatcher : MonoBehaviour, IMessage
{
    private static PacketMessageDispatcher _instance;
    public static PacketMessageDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject container = new GameObject("PacketMessageDispatcher");
                container.name = "PacketMessageDispatcher";
                _instance = container.AddComponent<PacketMessageDispatcher>();
            }

            return _instance;
        }
    }

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