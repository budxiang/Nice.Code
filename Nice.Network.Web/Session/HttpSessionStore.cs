using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Nice.Network.Web.Session
{
    [Serializable]
    public class HttpSessionStore
    {
        private BinaryFormatter _formatter = new BinaryFormatter();
        private string dirSessionStore = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session");
        private readonly IDictionary<string, HttpSession> sessions = new Dictionary<string, HttpSession>();
        private readonly object locker = new object();
        private readonly IList<HttpSession> sessionsChanged = new List<HttpSession>();
        private bool isActive = false;
        private readonly int sessionFileExpirationDays = 7;
        public HttpSessionStore(int sessionFileExpirationDays)
        {
            isActive = true;
            this.sessionFileExpirationDays = sessionFileExpirationDays;
            StoreRead();
            Task.Run(() =>
            {
                TimingDetect();
            });
        }
        // 添加session
        public void Add(Cookie cookie)
        {
            lock (sessions)
            {
                HttpSession session = null;
                if (sessions.ContainsKey(cookie.Value))
                {
                    session = sessions[cookie.Value];
                    session.Expires = cookie.Expires;
                    session.SessionId = cookie.Name;
                }
                else
                {
                    session = new HttpSession(cookie.Value);
                    session.Expires = cookie.Expires;
                    session.OnChanged += Session_OnChanged;
                    sessions.Add(cookie.Value, session);
                }
            }
        }
        public void ValidateCookie(Cookie ssCookie)
        {
            Task.Run(() =>
            {
                lock (sessions)
                {
                    if (!sessions.ContainsKey(ssCookie.Value))
                    {
                        HttpSession session = new HttpSession(ssCookie.Value);
                        session.Expires = DateTime.Now.AddYears(2);
                        session.OnChanged += Session_OnChanged;
                        sessions.Add(ssCookie.Value, session);
                    }
                }
            });
        }

        //获取session
        public HttpSession Get(string sessionId)
        {
            lock (sessions)
            {
                if (sessions.ContainsKey(sessionId))
                {
                    return sessions[sessionId];
                }
                return null;
            }
        }
        //获取session文件名称
        private string GetSessionFileName(string sessionId)
        {
            return Path.Combine(dirSessionStore, sessionId + ".session");
        }

        //session发送变化事件
        private void Session_OnChanged(HttpSession session)
        {
            Persistence(session);
        }

        //删除存储
        private void StoreDelete(string id)
        {
            try
            {
                string filename = GetSessionFileName(id);
                lock (locker)
                {
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        //读取session
        private void StoreRead()
        {
            try
            {
                if (!Directory.Exists(dirSessionStore))
                {
                    Directory.CreateDirectory(dirSessionStore);
                    return;
                }
                string[] files = Directory.GetFiles(dirSessionStore, "*.session");
                if (files == null || files.Length == 0) return;
                DateTime dtNow = DateTime.Now;
                HttpSession session = null;
                bool errorread = false;
                foreach (string file in files)
                {
                    errorread = false;
                    using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        try
                        {
                            session = (HttpSession)_formatter.Deserialize(stream);
                        }
                        catch (SerializationException ex)
                        {
                            errorread = true;
                            Console.WriteLine(ex);
                        }
                        catch (Exception ex)
                        {
                            errorread = true;
                            Console.WriteLine(ex);
                        }
                    }
                    if (errorread)
                    {
                        File.Delete(file);
                        continue;
                    }
                    if (session != null)
                    {
                        if (session.Expires < dtNow)
                        {
                            File.Delete(file);
                            continue;
                        }
                        session.OnChanged += Session_OnChanged;
                        sessions.Add(session.SessionId, session);
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        //定时检测
        private void TimingDetect()
        {
            while (isActive)
            {
                DateTime dtNow = DateTime.Now;
                DateTime limitLastAccessedTime = DateTime.Now.AddDays(-sessionFileExpirationDays);
                IList<string> delList = new List<string>(4);
                lock (sessions)
                {
                    try
                    {
                        foreach (KeyValuePair<string, HttpSession> item in sessions)
                        {
                            if (item.Value.Expires < dtNow)
                            {
                                delList.Add(item.Key);
                            }
                            if (item.Value.LastAccessedTime < limitLastAccessedTime)
                            {
                                delList.Add(item.Key);
                            }
                            else if (!item.Value.Expired && item.Value.LastAccessedTime.AddSeconds(NiceWebSettings.SessionTimeout) < dtNow)
                            {
                                item.Value.Clear();
                            }
                        }
                        if (delList.Count > 0)
                        {
                            foreach (string key in delList)
                            {
                                sessions.Remove(key);
                                StoreDelete(key);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                Thread.Sleep(8000);
            }
        }
        //写入文件
        private void Persistence(HttpSession session)
        {
            if (!isActive) return;
            UpdateSessionsChanged(session);
            Task.Run(async () =>
            {
                await Task.Delay(1500);
                HttpSession httpSession = GetSession(session.SessionId);
                if (httpSession != null)
                {
                    Save(httpSession);
                }
            });
        }

        private void UpdateSessionsChanged(HttpSession session)
        {
            lock (sessionsChanged)
            {
                IList<int> removeIndexs = new List<int>();
                for (int i = 0; i < sessionsChanged.Count; i++)
                {
                    if (sessionsChanged[i].SessionId == session.SessionId)
                    {
                        removeIndexs.Add(i);
                    }
                }
                for (int i = removeIndexs.Count - 1; i >= 0; i--)
                {
                    sessionsChanged.RemoveAt(removeIndexs[i]);
                }
                sessionsChanged.Add(session);
            }
        }

        private HttpSession GetSession(string sessionId)
        {
            lock (sessionsChanged)
            {
                int index = -1;
                for (int i = 0; i < sessionsChanged.Count; i++)
                {
                    if (sessionsChanged[i].SessionId == sessionId)
                    {
                        index = i;
                    }
                }
                HttpSession session = null;
                if (index >= 0)
                {
                    session = sessionsChanged[index];
                    sessionsChanged.RemoveAt(index);
                }
                return session;
            }
        }

        //保存
        public void Save(HttpSession session)
        {
            lock (locker)
            {
                UnsafeSave(session);
            }
        }

        private void UnsafeSave(HttpSession session)
        {
            try
            {
                using (FileStream stream = new FileStream(GetSessionFileName(session.SessionId), FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    _formatter.Serialize(stream, session);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SaveSessionsChanged()
        {
            lock (sessionsChanged)
            {
                foreach (HttpSession item in sessionsChanged)
                {
                    UnsafeSave(item);
                }
            }
        }

        public void Close()
        {
            isActive = false;
            SaveSessionsChanged();
        }
    }
}
