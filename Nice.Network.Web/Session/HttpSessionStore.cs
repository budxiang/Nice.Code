using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
        private bool isActive = false;

        public HttpSessionStore()
        {
            isActive = true;
            StoreRead();
            Task.Run(() =>
            {
                TimingDetect();
            });
        }
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
        private string GetSessionFileName(string sessionId)
        {
            return Path.Combine(dirSessionStore, sessionId + ".session");
        }
        public void Save(HttpSession session)
        {
            Task.Run(() =>
            {
                lock (locker)
                {
                    try
                    {
                        using (var stream = new FileStream(GetSessionFileName(session.SessionId), FileMode.Create, FileAccess.Write, FileShare.Read))
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
            });
        }
        private void Session_OnChanged(HttpSession session)
        {
            Save(session);
        }

        private void StoreDelete(string id)
        {
            try
            {
                var filename = GetSessionFileName(id);
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
                foreach (string file in files)
                {
                    using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        session = (HttpSession)_formatter.Deserialize(stream);
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
        private void TimingDetect()
        {
            while (isActive)
            {
                DateTime dtNow = DateTime.Now;
                IList<string> delList = new List<string>(4);
                lock (sessions)
                {
                    try
                    {
                        foreach (var item in sessions)
                        {
                            if (item.Value.Expires < dtNow)
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
                            foreach (var key in delList)
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
                Thread.Sleep(2000);
            }
        }

        public void Close()
        {
            isActive = false;
        }
    }
}
