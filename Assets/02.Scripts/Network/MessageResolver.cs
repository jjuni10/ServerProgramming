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
        UnityEngine.Debug.Log($"Received data: {transferred} bytes");
        // 패킷구조
        // [body길이:2byte][protocolID:2byte][body:보낼만큼]

        // 받은 바이트배열을 메시지버퍼 현재위치 뒤쪽에 복사 한다.
        // (복사할바이트 배열, 복사할바이트 배열 위치, 대상바이트 배열, 대상바이트위치, 길이)
        Array.Copy(buffer, offset, _messageBuffer, _curPosition, transferred);
        // 받은만큼 위치를 옮긴다.
        _curPosition += transferred;

        while (_curPosition > 0)
        {
            // 현재 받은 크기가 헤더보다 작다면 더받아야된다.
            if (_curPosition < NetDefine.HEADER_SIZE)
            {
                UnityEngine.Debug.Log("Incomplete header received, waiting for more data...");
                return;
            }

            // 헤더 정보를 가져온다.
            // BitConverter : 바이트 배열을 읽어서 특정 자료형으로 변환.
            short size = BitConverter.ToInt16(_messageBuffer, 0);
            short protocolID = BitConverter.ToInt16(_messageBuffer, 2);

            // 패킷이 완성되지 않았다면 더받아야된다.
            if (_curPosition < size)
            {
                UnityEngine.Debug.Log("Incomplete packet received, waiting for more data...");
                return;
            }

            // 하나의 패킷이 완성됨, 배열에 복사해서 넘김
            byte[] clone = new byte[size];
            Array.Copy(_messageBuffer, clone, size);

            UnityEngine.Debug.Log($"Complete packet received. Size: {size}, ProtocolID: {protocolID}");

            // 처리함수 호출
            onComplete?.Invoke(clone);

            // 하나의 패킷을 처리했으므로 _curPosition 위치를 옮김, _messageBuffer를 처리한만큼의 앞쪽으로 옮김
            _curPosition -= size;
            Array.Copy(_messageBuffer, size, _messageBuffer, 0, _curPosition);
        }
    }
}