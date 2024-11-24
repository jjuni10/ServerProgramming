public interface IPeer
{
    void ProcessMessage(byte[] buffer, int length);
    void Remove();
    void Send(Packet packet);
}