using Nice.Network.Base.Saea;
using System.Collections.Generic;
using System.Linq;

namespace Nice.Network.WebSockets
{
    public class WebSocketSessionList
    {
        private readonly IList<WebSocketSession> userTokenList = null;

        public WebSocketSessionList(int capacity)
        {
            userTokenList = new List<WebSocketSession>(capacity);
        }

        public bool IsExist(SocketAsyncUserToken userToken)
        {
            lock (userTokenList)
            {
                string sessionId = userToken.SessionID;
                for (int i = 0; i < userTokenList.Count; i++)
                {
                    if (userTokenList[i].SessionID == sessionId)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void Add(WebSocketSession userToken)
        {
            lock (userTokenList)
            {
                userTokenList.Add(userToken);
            }
        }

        public WebSocketSession Get(string sessionId)
        {
            lock (userTokenList)
            {
                WebSocketSession session = null;
                for (int i = 0; i < userTokenList.Count; i++)
                {
                    if (userTokenList[i].SessionID == sessionId)
                    {
                        session = userTokenList[i];
                        break;
                    }
                }
                return session;
            }
        }

        public void Remove(SocketAsyncUserToken userToken)
        {
            string sessionId = userToken.SessionID;
            if (string.IsNullOrEmpty(sessionId)) return;
            lock (userTokenList)
            {
                int idx = -1;
                for (int i = 0; i < userTokenList.Count; i++)
                {
                    if (userTokenList[i].SessionID == sessionId)
                    {
                        idx = i;
                        break;
                    }
                }
                if (idx >= 0)
                {
                    userTokenList.RemoveAt(idx);
                }
            }
        }

        public IList<WebSocketSession> ToArray()
        {
            lock (userTokenList)
            {
                return userTokenList.ToArray();
            }
        }
       
        public IList<WebSocketSession> GetList( IList<WebSocketSession> sessions,string actionName, string parmkey)
        {
            lock (userTokenList)
            {
                WebSocketSession session = null;
                for (int i = 0; i < userTokenList.Count; i++)
                {
                    session = userTokenList[i];
                   if (parmkey == null || parmkey == "")
                    {
                        if (session.ActionName == actionName)
                        {
                            sessions.Add(session);
                        }
                    }
                    else
                    {
                        if (session.ActionName == actionName && session.Parmkey == parmkey)
                        {
                            sessions.Add(session);
                        }
                    }  
                }
                return sessions;
            }
        }
        public int GetCount()
        {
            lock (userTokenList)
            {
                return userTokenList.Count;
            }
        }
    }
}
