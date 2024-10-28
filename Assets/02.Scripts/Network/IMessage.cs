public interface IMessage
{
    void OnMessage(UserToken user, byte[] buffer);
}