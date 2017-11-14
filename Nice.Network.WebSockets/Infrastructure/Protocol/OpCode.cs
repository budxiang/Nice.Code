namespace Nice.Network.WebSockets.Protocol
{
    /// <summary>
    /// 帧类型 RFC 6455
    /// </summary>
    public enum OpCode
    {
        Continuation = 0,//0x0 连续帧消息  
        Text = 1,//0x0 表示文本消息  
        Binary = 2,//0x0 表示二进制消息  
        Close = 8,//0x0 表示客户端发起的关闭  
        Ping = 9,//0x0 ping（用于心跳）
        Pong = 0xa //0x0 pong（用于心跳）
    }
}
