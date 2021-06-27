namespace Nice.Network.Web
{
    public class NiceWebSettings
    {
        public static string DefaultPage = "index.html";

        public static string ViewsPath = "Views";

        public static string WebControllerAssembly = null;

        public static string WebControllerNamespace = null;

        public static int SessionTimeout = 1200;

    }
    
    public class WebConfig
    {
        private string _DefaultPage = "index.html";
        private string _ViewsPath = "Views";
        private int _SessionTimeout = 1200;
       /// <summary>
       /// 默认页，如index.html
       /// </summary>
        public string DefaultPage { get => _DefaultPage; set => _DefaultPage = value; }
        /// <summary>
        /// HTML目录
        /// </summary>
        public string ViewsPath { get => _ViewsPath; set => _ViewsPath = value; }
        /// <summary>
        /// 会话超时时间
        /// </summary>
        public int SessionTimeout { get => _SessionTimeout; set => _SessionTimeout = value; }
    }
}
