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
            lastAccessedTime = DateTime.Now;
        }

        private string sessionId;
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        private bool expired;

        public bool Expired
        {
            get { return expired; }
        }

        private DateTime expires;
        public DateTime Expires
        {
            get { return expires; }
            set { expires = value; }
        }

        private DateTime lastAccessedTime;
        public DateTime LastAccessedTime
        {
            get { return lastAccessedTime; }
            set { lastAccessedTime = value; }
        }

        public void Clear()
        {
            lock (_vars)
            {
                expired = true;
                _vars.Clear();
                if (OnChanged != null)
                    OnChanged(this);
            }
        }
        public object this[string key]
        {
            get
            {
                lock (_vars)
                {
                    if (_vars.ContainsKey(key))
                    {
                        lastAccessedTime = DateTime.Now;
                        if (OnChanged != null)
                            OnChanged(this);
                        return _vars[key];
                    }
                    return null;
                }
            }
            set
            {
                lock (_vars)
                {
                    lastAccessedTime = DateTime.Now;
                    if (_vars.ContainsKey(key))
                    {
                        _vars[key] = value;
                    }
                    else
                    {
                        if (expired) expired = false;
                        _vars.Add(key, value);
                    }
                    if (OnChanged != null)
                        OnChanged(this);
                }
            }
        }

    }
}
