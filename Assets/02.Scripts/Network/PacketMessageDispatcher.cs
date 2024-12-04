using System;
using System.Collections.Concurrent;
using UnityEngine;

public class PacketMessageDispatcher : MonoBehaviour
{
    public static PacketMessageDispatcher Instance => _instance;
    private static PacketMessageDispatcher _instance;

    public struct PacketMessage
    {
        public UserToken token;
        public Packet packet;

        public PacketMessage(UserToken token, Packet packet)
        {
            this.token = token;
            this.packet = packet;
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

            msg.token.OnMessage(msg.packet);
        }
    }

    public void OnMessage(UserToken token, Packet packet)
    {
        _messageQueue.Enqueue(new PacketMessage(token, packet));
    }
}