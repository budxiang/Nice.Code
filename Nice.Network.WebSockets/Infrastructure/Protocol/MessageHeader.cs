namespace Nice.Network.WebSockets.Protocol
{
    public class MessageHeader
    {
        public char FIN;// 1位，用来表明这是一个消息的最后的消息片断，当然第一个消息片断也可能是最后的一个消息片断
        public char RSV1;//保留位 0 分别都是1位，如果双方之间没有约定自定义协议，那么这几位的值都必须为0,否则必须断掉WebSocket连接
        public char RSV2;//保留位 0
        public char RSV3;//保留位 0
        public OpCode OpCode;//操作码
        public char MASK;//是否有加掩码,如果设置为1,掩码键必须放在masking-key区域,客户端发送给服务端的所有消息,此位的值都是1
        public ulong Payloadlen;//传输数据的长度，以字节的形式表示：7位、7+16位、或者7+64位
        public byte[] Maskey;// 0或4个字节,客户端发送给服务端的数据
        public int PayloadDataStartIndex;
    }
}
