namespace Nice.Network.WebSockets.Protocol
{
    public class ProtocolProcessor
    {
        private IProtocol protocol;
        public byte[] GetResponse(WebSocketRequest request)
        {
            if (request.SecWebSocketVersion == "13")
                protocol = new ProtocolDraft10();
            return protocol.Handshake(request);
        }

        public Message UnPackage(byte[] buffer, int offset, int count)
        {
            return protocol.UnPackage(buffer, offset, count);
        }

        public void Package(OpCode opCode, byte[] src, byte[] dst, int dstOffset, out int count)
        {
            protocol.Package(opCode, src, dst, dstOffset, out count);
        }

    }
}
