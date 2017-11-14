using System;

namespace Nice.Network.WebSockets
{
    public class WebSocketRequest
    {
        private string _SecWebSocketKey;

        public string SecWebSocketKey
        {
            get
            {
                if (_SecWebSocketKey == null)
                    _SecWebSocketKey = headers.Get(WebSocketConstant.SecWebSocketKey, string.Empty);
                return _SecWebSocketKey;
            }
        }
        private string _SecWebSocketVersion;
        public string SecWebSocketVersion
        {
            get
            {
                if (_SecWebSocketVersion == null)
                    _SecWebSocketVersion = headers.Get(WebSocketConstant.SecWebSocketVersion, string.Empty);
                return _SecWebSocketVersion;
            }
        }
        private string _SecWebSocketProtocol;
        public string SecWebSocketProtocol
        {
            get
            {
                if (_SecWebSocketProtocol == null)
                    _SecWebSocketProtocol = headers.Get(WebSocketConstant.SecWebSocketProtocol, string.Empty);
                return _SecWebSocketProtocol;
            }
        }
        private string _Method;
        public string Method
        {
            get
            {
                if (_Method == null)
                    _Method = headers.Get(WebSocketConstant.Method, string.Empty);
                return _Method;
            }
        }
        private string _Path;
        public string Path
        {
            get
            {
                if (_Path == null)
                    _Path = headers.Get(WebSocketConstant.Path, string.Empty);
                return _Path;
            }
        }
        private string _Origin;
        public string Origin
        {
            get
            {
                if (_Origin == null)
                    _Origin = headers.Get(WebSocketConstant.Origin, string.Empty);
                return _Origin;
            }
        }
        private string _Host;
        public string Host
        {
            get
            {
                if (_Host == null)
                    _Host = headers.Get(WebSocketConstant.Host, string.Empty);
                return _Host;
            }
        }

        private string _Upgrade;

        public string Upgrade
        {
            get
            {
                if (_Upgrade == null)
                    _Upgrade = headers.Get(WebSocketConstant.Upgrade, string.Empty);
                return _Upgrade;
            }
        }

        private string _Connection;

        public string Connection
        {
            get
            {
                if (_Connection == null)
                    _Connection = headers.Get(WebSocketConstant.Connection, string.Empty);
                return _Connection;
            }
        }

        private string _RowUrl;

        public string RowUrl
        {
            get
            {
                return _RowUrl;
            }
        }

        private string _SecWebSocketOrigin;
        public string SecWebSocketOrigin
        {
            get
            {
                if (_SecWebSocketOrigin == null)
                    _SecWebSocketOrigin = headers.Get(WebSocketConstant.SecWebSocketOrigin, string.Empty);
                return _SecWebSocketOrigin;
            }
        }
        private Mapping<string, string> _Cookies;
        public Mapping<string, string> Cookies
        {
            get
            {
                if (_Cookies == null)
                {
                    _Cookies = new Mapping<string, string>();
                    string match = headers.Get(WebSocketConstant.Cookie, string.Empty);
                    string[] cookies = match.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string cook in cookies)
                    {
                        string[] keyval = cook.Split('=');
                        _Cookies.Add(keyval[0], keyval[1]);
                    }
                }
                return _Cookies;
            }
        }


        private Mapping<string, string> headers;

        public WebSocketRequest(string request)
        {
            headers = new Mapping<string, string>();
            string[] rows = request.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (rows.Length > 0)
            {
                string[] firstRows = rows[0].Split(' ');
                if (firstRows.Length > 1) _RowUrl = firstRows[1];
                foreach (string row in rows)
                {
                    int index = row.IndexOf(':');
                    if (index > 0)
                    {
                        headers.Add(row.Substring(0, index).Trim(), row.Substring(index + 1).Trim());
                    }
                }
            }

        }
    }
}
