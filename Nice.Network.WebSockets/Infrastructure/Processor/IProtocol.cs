namespace Nice.Network.WebSockets.Protocol
{
    public interface IProtocol
    {
        byte[] Handshake(WebSocketRequest request);
        void Package(OpCode opCode, byte[] src, byte[] dst, int dstOffset, out int count);
        Message UnPackage(byte[] buffer, int offset, int count);
    }
}
