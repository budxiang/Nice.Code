namespace Nice.Network.WebSockets
{
    public class SettingsConfig
    {
        public static string ServerSrvIP = null;

        public static int ServerSrvPort = 8101;

        public static int ServerSendTimeOut = 2;//单位100毫秒

        public static int WebSocketSrvPort = 8011;

        public static int WebSocketSendTimeOut = 2;//单位100毫秒

        /// <summary>
        /// 接收的最大错误数
        /// </summary>
        public static int MaxAcceptErrorCount = 8;

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
