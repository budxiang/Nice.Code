using System.Net;
using Nice.Network.Web.Session;

namespace Nice.Network.Web
{
    public class HttpContext
    {
        private IPEndPoint remoteEndPoint;
        public IPEndPoint RemoteEndPoint
        {
            get { return remoteEndPoint; }
            set { remoteEndPoint = value; }
        }
        private HttpSession session;
        public HttpSession Session
        {
            get { return session; }
            set { session = value; }
        }
    }
}
