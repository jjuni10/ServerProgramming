using System;

public class MessageResolver
{
    private byte[] _messageBuffer;
    private int _curPosition;

    public MessageResolver(int bufferSize)
    {
        _messageBuffer = new byte[bufferSize];
    }
    public void OnReceive(byte[] buffer, int offset, int transferred, Action<byte[]> onComplete)
    {
        /*Array.Copy(buffer, offset, _messageBuffer, _curPosition, transferred);
        _curPosition += transferred;

        while (_curPosition > 0)
        {
            if (_curPosition < NetDefine.HEADER_SIZE)
            {
                return;
            }

            byte[] clone = new byte[size];
            Array.Copy(_messageBuffer, clone, size);

            onComplete?.Invoke(clone);

            _curPosition -= size;
            Array.Copy(_messageBuffer, size, _messageBuffer, 0, _curPosition);
        }*/

        byte[] clone = new byte[transferred];
        Array.Copy(buffer, offset, clone, 0, transferred);
        onComplete?.Invoke(clone);
    }
}