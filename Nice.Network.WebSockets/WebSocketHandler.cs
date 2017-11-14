using Nice.Core.Log;
using Nice.Network.Base.Protocol;
using Nice.Network.Base.Saea;
using Nice.Network.WebSockets.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nice.Network.WebSockets
{
    public delegate void WebSocketEventHandler(string content);
    public class WebSocketHandler
    {
        private SocketAsyncServer server = null;
        private ProtocolProcessor processor = null;
        private WebSocketSessionList webSocketSessionList = null;
        private WebSocketSessionListPool webSocketSessionListPool = null;
        public event WebSocketEventHandler OnReceived = null;
        public void Start(string srvip, int port)
        {
            server = new SocketAsyncServer(WebSocketConstant.MaxConnection, WebSocketConstant.BufferSize, SettingsConfig.MaxAcceptErrorCount);
            server.Init(SettingsConfig.NetExKeepaliveInterval, SettingsConfig.NetExKeepaliveTime);
            server.Start(new IPEndPoint(IPAddress.Parse(srvip), port));
            server.OnConnected += Server_OnConnected;
            server.OnReceived += Server_OnReceived;
            server.OnSendCompleted += Server_OnSendCompleted;
            server.OnClosed += Server_OnClosed;
            server.InnerException += Server_InnerException;
            processor = new ProtocolProcessor();
            webSocketSessionList = new WebSocketSessionList(WebSocketConstant.MaxConnection);
            InitSessionListPool();
        }

        private void InitSessionListPool()
        {
            //发送Session集合池,减少每次重新创建新的集合
            webSocketSessionListPool = new WebSocketSessionListPool(WebSocketConstant.MaxSyncSend);
            for (int i = 0; i < WebSocketConstant.MaxSyncSend; i++)
            {
                webSocketSessionListPool.Push(new List<WebSocketSession>());
            }
        }
        #region SocketAsyncServer Event
        //有新的连接
        private void Server_OnConnected(SocketAsyncUserToken userToken)
        {
            Logging.Info(string.Format("消息推送连接成功{0},{1}", userToken.ClientIP, userToken.SessionID));
        }

        //接收数据处理
        private void Server_OnReceived(SocketAsyncEventArgs receiveEventArgs)
        {
            byte[] buffer = receiveEventArgs.Buffer;
            int offset = receiveEventArgs.Offset;
            int count = receiveEventArgs.BytesTransferred;
            SocketAsyncUserToken userToken = (SocketAsyncUserToken)receiveEventArgs.UserToken;
            try
            {
                if (webSocketSessionList.IsExist(userToken))
                {
                    Message message = processor.UnPackage(buffer, offset, count);
                    if (message.Header.MASK == '0')//接收到客户端非掩码数据，服务端断开
                    {
                        //server.CloseClientSocket(receiveEventArgs);
                        Logging.Info("接收到客户端非掩码数据");
                        return;
                    }
                    if (message.Header.OpCode == OpCode.Close)
                    {
                        //server.CloseClientSocket(receiveEventArgs);
                        //webSocketSessionList.Remove(userToken);
                    }
                    else if (message.Header.OpCode == OpCode.Text)
                    {
                        if (OnReceived != null)
                            OnReceived(message.Data.ToString());
                    }
                }
                else
                {
                    string request = Encoding.UTF8.GetString(buffer, offset, count);
                    WebSocketSession session = new WebSocketSession(userToken, request);
                    byte[] response = processor.GetResponse(session.RequestContent);

                    SocketAsyncEventArgs sendEventArgs = userToken.SendEventArgs;
                    Buffer.BlockCopy(response, 0, sendEventArgs.Buffer, sendEventArgs.Offset, response.Length);
                    server.SendAsync(userToken, sendEventArgs, sendEventArgs.Offset, response.Length);
                    webSocketSessionList.Add(session);
                    Logging.Info(string.Format("当前消息推送连接数量{0}", webSocketSessionList.GetCount()));
                }
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
            }
        }
        private void Server_OnSendCompleted(SocketAsyncEventArgs sendEventArgs)
        {
            try
            {
                ((SocketAsyncUserToken)sendEventArgs.UserToken).ExitSend();
            }
            catch (ArgumentNullException ex)
            {
                Logging.Error(ex);
            }
        }

        private void SendAsync(SocketAsyncUserToken userToken, SocketAsyncEventArgs sendEventArgs, byte[] buffer)
        {
            int count = 0;
            processor.Package(OpCode.Text, buffer, sendEventArgs.Buffer, sendEventArgs.Offset, out count);
            if (count > ProtocolConst.BufferSize)
            {
                Logging.Info(string.Format("数据消息推送被截断{0},{1}", count, userToken.ClientIP));
            }
            server.SendAsync(userToken, sendEventArgs, sendEventArgs.Offset, count);
        }

        private void Send(SocketAsyncUserToken userToken, SocketAsyncEventArgs sendEventArgs, byte[] buffer)
        {
            if (userToken.EnterSend())
            {
                SendAsync(userToken, sendEventArgs, buffer);
                return;
            }
            else
            {
                WaitSend(userToken, sendEventArgs, buffer);
            }
        }
        private void WaitSend(SocketAsyncUserToken userToken, SocketAsyncEventArgs sendEventArgs, byte[] buffer)
        {
            int i = 0;
            do
            {
                Thread.Sleep(100);
                if (userToken.EnterSend())
                {
                    SendAsync(userToken, sendEventArgs, buffer);
                    Logging.Info(string.Format("消息延时发送{0}", userToken.ClientIP));
                    return;
                }
            }
            while (i < SettingsConfig.WebSocketSendTimeOut);
            Logging.Error(string.Format("消息推送繁忙,{0}", userToken.ClientIP));
        }
        //连接断开
        private void Server_OnClosed(SocketAsyncUserToken userToken)
        {
            try
            {
                webSocketSessionList.Remove(userToken);
                Logging.Info(string.Format("消息推送连接关闭{0},{1},当前连接数量{2}", userToken.ClientIP, userToken.SessionID, webSocketSessionList.GetCount()));
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
            }
        }

        private void Server_InnerException(Exception ex)
        {
            Logging.Error(ex);
        }
        #endregion

        #region Public
        public void SendTextAsync(byte[] buffer, string actionName, string parmkey)
        {
            Task.Run(() =>
            {
                IList<WebSocketSession> sessionList = null;//返回新的集合，防止并发时，原有集合被修改
                try
                {
                    SocketAsyncUserToken userToken = null;
                    sessionList = webSocketSessionListPool.Pop();
                    webSocketSessionList.GetList(sessionList, actionName, parmkey);
                    if (sessionList.Count > 0)
                    {
                        foreach (var session in sessionList)
                        {
                            userToken = session.UserToken;
                            if (userToken.SessionID == null || userToken.ClientIP == null) continue;
                            Send(userToken, userToken.SendEventArgs, buffer);
                        }
                        sessionList.Clear();
                    }
                    else
                    {
                        Logging.Info(string.Format("未发现可用的WebSocket连接{0},{1}", actionName, parmkey));
                    }
                    webSocketSessionListPool.Push(sessionList);
                }
                catch (SocketException ex)
                {
                    if (sessionList != null)
                    {
                        sessionList.Clear();
                        webSocketSessionListPool.Push(sessionList);
                    }
                    Logging.Error(ex);
                }
                catch (Exception ex)
                {
                    if (sessionList != null)
                    {
                        sessionList.Clear();
                        webSocketSessionListPool.Push(sessionList);
                    }
                    Logging.Error(ex);
                }
            });
        }

        public void Close()
        {
            IList<WebSocketSession> list = webSocketSessionList.ToArray();
            foreach (WebSocketSession item in list)
            {
                if (item.UserToken != null && (item.UserToken.SessionID != null || item.UserToken.ClientIP != null))
                {
                    server.CloseClientSocket(item.UserToken.ReceiveEventArgs);
                }
            }
        }
        #endregion

    }
}
