public interface IPeer
{
    void OnReceive(Packet packet);
    void Remove();
    void Send(Packet packet);
}