using Nice.Network.Base.Infrastructure;

namespace Nice.Network.WebSockets
{
    public class WebSocketConstant
    {
        public const string Host = "Host";
        public const string Connection = "Connection";
        public const string Method = "Method";
        public const string Path = "Path";
        public const string Cookie = "Cookie";
        public const string Upgrade = "Upgrade";
        public const string Origin = "Origin";

        public const string SecWebSocketOrigin = "Sec-WebSocket-Origin";
        public const string SecWebSocketKey = "Sec-WebSocket-Key";
        public const string SecWebSocketVersion = "Sec-WebSocket-Version";
        public const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";
        public const string WebSocketProtocol = "WebSocket-Protocol";
      
        public const string ResponseHeadLine10 = "HTTP/1.1 101 Web Socket Protocol Handshake\r\n";
        public const string ResponseUpgradeLine = "Upgrade: {0}\r\n";
        public const string ResponseConnectionLine = "Connection: {0}\r\n";
        public const string ResponseAcceptLine = "Sec-WebSocket-Accept: {0}\r\n";
        public const string ResponseOriginLine = "WebSocket-Origin: {0}\r\n";
        public const string ResponseLocationLine = "WebSocket-Location: {0}\r\n";

        public const byte StartByte = 0x00;
        public const byte EndByte = 0xFF;
        public static byte[] ClosingHandshake = new byte[] { 0xFF, 0x00 };
        public const string WsSchema = "ws";
        public const string WssSchema = "wss";

        /// <summary>
        /// 同时在线连接数
        /// </summary>
        public static int MaxConnection = 200;
        /// <summary>
        /// 包大小
        /// </summary>
        public static int BufferSize = 20480;

        /// <summary>
        /// 最大同步发送
        /// </summary>
        public static int MaxSyncSend = 24;

    }
}
