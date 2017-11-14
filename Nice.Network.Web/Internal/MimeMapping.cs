using System.Collections.Generic;

namespace Nice.Network.Web
{
    public enum MimeType
    {
         OctetStream = 1,
         Text = 2,
         Json = 3,
         Html = 4,
    }
    public class MimeMapping
    {
        public const string OctetStream = "application/octet-stream";
        public const string Text = "text/plain";
        public const string Json = "application/Json";
        public const string Html = "text/html";

        private static Dictionary<string, string> dic = new Dictionary<string, string>() {

            {".htm","text/html" },{".html","text/html" },
            {".js","text/javascript" },
            {".css","text/css" },
            {".txt","text/plain" },
            {".jpg","image/jpeg" },{".png","image/png" }, {".ico","image/x-icon" }, {".gif","image/gif" },
            { ".woff","application/font-woff" }, { ".woff2","application/font-woff2" },
        };

        public static string Get(string key)
        {
            if (dic.ContainsKey(key))
                return dic[key];
            return null;
        }
    }
}
