using System;
using System.IO;
using System.Net;

namespace Nice.Network.Web
{
    public class HttpServer
    {
        private bool isAlive = false;
        private readonly string physicalPath = null;
        private readonly HttpListener httpListener = null;
        private readonly HttpListenerHandler handler = null;
        public HttpServer(string physicalPath, string[] listenerPrefixs)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("不支持当前操作系统");
            if (listenerPrefixs == null || listenerPrefixs.Length == 0)
                throw new ArgumentNullException("listenerPrefixs不能为空");
            if (!Directory.Exists(physicalPath))
                throw new ArgumentException("physicalPath指定路径不存在," + physicalPath);

            httpListener = new HttpListener();
            handler = new HttpListenerHandler(physicalPath);
            foreach (string listenerPrefix in listenerPrefixs)
            {
                httpListener.Prefixes.Add(listenerPrefix);
            }
            this.physicalPath = physicalPath;
            this.isAlive = true;
        }
        public async void Start()
        {
            httpListener.Start();
            while (isAlive)
            {
                HttpListenerContext content = null;
                try
                {
                    content = await httpListener.GetContextAsync();
                    handler.ProcessRequest(content);
                }
                catch (ObjectDisposedException ex)
                {
                    if (!isAlive) return;
                    Console.WriteLine(ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        public void Stop()
        {
            isAlive = false;
            handler.Close();
            httpListener.Stop();
            httpListener.Close();
        }
    }
}
