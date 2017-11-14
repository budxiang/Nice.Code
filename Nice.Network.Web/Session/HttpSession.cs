using System;
using System.Collections.Generic;

namespace Nice.Network.Web.Session
{
    [Serializable]
    public class HttpSession
    {
        private readonly IDictionary<string, object> _vars = new Dictionary<string, object>();

        [field: NonSerialized]
        public event Action<HttpSession> OnChanged = null;
        public HttpSession(string id)
        {
            this.sessionId = id;
        }

        private string sessionId;
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        private DateTime expires;
        public DateTime Expires
        {
            get { return expires; }
            set { expires = value; }
        }

        public object this[string key]
        {
            get
            {
                lock (_vars)
                {
                    if (_vars.ContainsKey(key))
                    {
                        return _vars[key];
                    }
                    return null;
                }
            }
            set
            {
                lock (_vars)
                {
                    if (_vars.ContainsKey(key))
                    {
                        _vars[key] = value;
                    }
                    else
                    {
                        _vars.Add(key, value);
                    }
                    if (OnChanged != null)
                        OnChanged(this);
                }
            }
        }

    }
}
