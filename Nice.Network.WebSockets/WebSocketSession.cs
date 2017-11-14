using Nice.Network.Base.Saea;

namespace Nice.Network.WebSockets
{
    public class WebSocketSession
    {
        private SocketAsyncUserToken userToken;

        public SocketAsyncUserToken UserToken
        {
            get { return userToken; }
            set { userToken = value; }
        }

        private string sessionID;

        public string SessionID
        {
            get { return sessionID; }
            set { sessionID = value; }
        }

        private string actionName;
        /// <summary>
        /// 操作名称
        /// </summary>
        public string ActionName
        {
            get { return actionName; }
            set { actionName = value; }
        }

        private string parmkey;
        /// <summary>
        /// 主参数
        /// </summary>
        public string Parmkey
        {
            get { return parmkey; }
            set { parmkey = value; }
        }

        private WebSocketRequest requestContent;

        public WebSocketRequest RequestContent
        {
            get
            {
                return requestContent;
            }
        }

        public WebSocketSession(SocketAsyncUserToken userToken, string request)
        {
            this.userToken = userToken;
            this.sessionID = userToken.SessionID;
            this.requestContent = new WebSocketRequest(request);
            this.actionName = GetParamValue(requestContent.RowUrl, "action");
            this.parmkey = GetParamValue(requestContent.RowUrl, "parmkey");
        }

        public string GetParamValue(string url, string paramName)
        {
            string actionName = string.Empty;
            if (string.IsNullOrWhiteSpace(url)) return actionName;
            string[] rawUrl = url.Split('?');
            if (rawUrl.Length > 1)
            {
                string[] parms = rawUrl[1].Split('&');
                for (int i = 0; i < parms.Length; i++)
                {
                    string[] parm = parms[i].Split('=');
                    if (parm.Length > 1)
                    {
                        if (parm[0] == paramName)
                        {
                            actionName = parm[1];
                            break;
                        }
                    }
                }
            }
            return actionName;
        }
    }
}
