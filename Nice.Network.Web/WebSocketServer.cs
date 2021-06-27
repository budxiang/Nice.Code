using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace Nice.Network.Web
{
    public class WebSocketServer
    {
        private bool isAlive = false;
        private string subProtocol = null;
        private readonly HttpListener httpListener = null;
        private TimeSpan keepAliveInterval = new TimeSpan();

        public WebSocketServer(string listenerPrefix)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("不支持当前操作系统");
            if (string.IsNullOrEmpty(listenerPrefix))
                throw new ArgumentException("listenerPrefix");
            httpListener = new HttpListener();
            this.isAlive = true;
        }

        public async void Start()
        {
            httpListener.Start();
            while (isAlive)
            {
                HttpListenerContext httpListenerContext = await httpListener.GetContextAsync();
                if (httpListenerContext.Request.IsWebSocketRequest)
                {
                    ProcessRequest(httpListenerContext);
                }
                else
                {
                    httpListenerContext.Response.StatusCode = 400;
                    httpListenerContext.Response.Close();
                }
            }
        }

        private async void ProcessRequest(HttpListenerContext httpListenerContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await httpListenerContext.AcceptWebSocketAsync(subProtocol, keepAliveInterval);
                string ipaddr = httpListenerContext.Request.RemoteEndPoint.Address.ToString();
                Console.WriteLine(string.Format("Connected:{0}", ipaddr));
            }
            catch (Exception ex)
            {
                httpListenerContext.Response.StatusCode = 500;
                httpListenerContext.Response.Close();
                Console.WriteLine(ex);
                return;
            }
            WebSocket webSocket = webSocketContext.WebSocket;

            try
            {
                byte[] receiveBuffer = new byte[1024];
                if (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:{0}", ex);
            }
        }

        public void Stop()
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }
}
