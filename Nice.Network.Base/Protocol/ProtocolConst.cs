namespace Nice.Network.Base.Protocol
{
    public class ProtocolConst
    {
        /// <summary>
        /// 同时在线连接数
        /// </summary>
        public static int MaxConnection = 100;
        /// <summary>
        /// 包大小
        /// </summary>
        public static int BufferSize = 20480;

        /// <summary>
        /// 开头
        /// </summary>
        public const byte Prefix = 0x7e;
        /// <summary>
        /// 结尾
        /// </summary>
        public const byte Suffix = 0x9a;
        /// <summary>
        /// 包头长度
        /// </summary>
        public const byte PrefixLength = 1;
        /// <summary>
        /// 包尾长度
        /// </summary>
        public const byte SuffixLength = 1;
        /// <summary>
        /// 操作码字节长度
        /// </summary>
        public const byte CodeLength = 1;
        /// <summary>
        /// 预留字节长度
        /// </summary>
        public const byte RSVLength = 1;
        /// <summary>
        /// 命令行字节长度
        /// </summary>
        public const byte CommandLength = 2;
        /// <summary>
        /// 数据包大小所占字节长度
        /// </summary>
        public const byte DataSizeLength = 4;
        /// <summary>
        /// 命令行字节开始Index
        /// </summary>
        public const int CommandStart = PrefixLength + CodeLength + RSVLength;
        /// <summary>
        /// 包头字节长度开始Index
        /// </summary>
        public const int DataSizeStart = PrefixLength + CodeLength + RSVLength + CommandLength;
        /// <summary>
        ///包头长度
        /// </summary>
        public const int DataStart = PrefixLength + CodeLength + RSVLength + CommandLength + DataSizeLength;
        /// <summary>
        /// 数据字节长度
        /// </summary>
        public static int DataLength = BufferSize - DataStart - SuffixLength;
        /// <summary>
        /// 包头包尾总长
        /// </summary>
        public const int ProtocolLength = DataStart + SuffixLength;
        /// <summary>
        /// 大数据(bytes超过Int.Max)第一包数据长度
        /// </summary>
        public const int BigDataSizeFirst = ProtocolLength + 8;
        /// <summary>
        /// 命令返回状态长度
        /// </summary>
        public const int StatusLength = 2;
        /// <summary>
        /// 登录超时时间(毫秒)
        /// </summary>
        public const int MillSecLoginTimeOut = 30 * 1000;
        /// <summary>
        /// 连接失败重连时间(毫秒)
        /// </summary>
        public const int MillSecReConnTimeOut = 30 * 1000;
        /// <summary>
        /// 定时时间单元(毫秒)
        /// </summary>
        public const int TimeTimingUnit = 4000;
        /// <summary>
        /// 发送等待超时时间(100毫秒)
        /// </summary>
        public const int SendWaitTimeOut = 4;
        /// <summary>
        /// 心跳间隔时间(毫秒)
        /// </summary>
        public const int SecTimingDetectActived = 30000;
        /// <summary>
        /// UID数据长度
        /// </summary>
        public const int UIDLength = 16;

        /// <summary>
        /// 接收的最大错误数
        /// </summary>
        public static int MaxAcceptErrorCount =8;

        /// <summary>
        /// 是否设置服务端进行心跳检测
        /// </summary>
        public static bool IsNetExKeepAlive = true;

        /// <summary>
        /// 服务端多长时间后开始第一次探测
        /// </summary>
        public static uint NetExKeepaliveTime = 30000;

        /// <summary>
        /// 服务端探测时间间隔
        /// </summary>
        public static uint NetExKeepaliveInterval = 10000;
    }
}
