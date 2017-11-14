
using Nice.Network.Base.Infrastructure;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nice.Network.Base.Saea
{
    public delegate void SocketEventHandler(SocketAsyncEventArgs receiveEventArgs);
    public delegate void SessionHandler(SocketAsyncUserToken userToken);
    public delegate void InnerExceptionHandler(Exception ex);
    public class SocketAsyncServer
    {
        private int m_numConnections;   // 最大连接数
        private int m_receiveBufferSize;// 缓冲区大小
        BufferManager m_bufferManager;  //Socket操作的缓冲区
        const int opsToPreAlloc = 2;    // 每个SocketAsyncEventArgs的操作数（读、写）
        Socket listenSocket;
        SocketAsyncUserTokenPool socketAsyncUserTokenPool;
        int m_numConnectedSockets;      // 连接到服务器的总数
        int acceptErrorCount = 0; //当前的错误数
        int acceptErrorMaxCount = 0;    //接收连接错误数
        SemaphoreSlim m_maxNumberAcceptedClients;//限制访问接收连接的线程数，用来控制最大并发数
        private byte[] keepAliveValues = null;//服务端连接检测机制数值
        private bool IsNetExKeepAlive = true;//是否开启服务器心跳检测机制
        public event SessionHandler OnConnected = null;
        public event SocketEventHandler OnReceived = null;
        public event SocketEventHandler OnSendCompleted = null;
        public event SessionHandler OnClosed = null;
        public event InnerExceptionHandler InnerException = null;
        public event DangerErrorHandler DangerError = null;
        public SocketAsyncServer(int numConnections, int receiveBufferSize, int acceptErrorCount)
        {
            m_numConnectedSockets = 0;
            m_numConnections = numConnections;
            m_receiveBufferSize = receiveBufferSize;
            acceptErrorMaxCount = acceptErrorCount;
            // 分配缓冲区，使得最大数量的套接字可以同时具有一个未完成的读写
            m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc, receiveBufferSize);
            socketAsyncUserTokenPool = new SocketAsyncUserTokenPool(numConnections);
            m_maxNumberAcceptedClients = new SemaphoreSlim(numConnections, numConnections);
        }

        public void Init(uint netExKeepaliveTime, uint netExKeepaliveInterval)
        {
            // 分配一个大字节缓冲区,所有的I/O操作都使用一个.这样可以防止内存碎片化
            m_bufferManager.InitBuffer();

            SocketAsyncUserToken userToken;
            for (int i = 0; i < m_numConnections; i++)
            {
                userToken = new SocketAsyncUserToken();
                userToken.ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                userToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                m_bufferManager.SetBuffer(userToken.ReceiveEventArgs);
                m_bufferManager.SetBuffer(userToken.SendEventArgs);
                socketAsyncUserTokenPool.Push(userToken);
            }
            SetKeepAliveValue(netExKeepaliveTime, netExKeepaliveInterval);
        }

        // 启动服务器，使其正在侦听传入的连接请求.  
        public void Start(IPEndPoint localEndPoint)
        {
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(m_numConnections);
            StartAccept(null);
        }

        // 开始接受来自的连接请求的操作
        public void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                // 必须清除套接字，因为上下文对象被重用
                acceptEventArgs.AcceptSocket = null;
            }

            m_maxNumberAcceptedClients.Wait();
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArgs);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }

        // 此方法是与Socket.AcceptAsync操作相关联的回调方法，并在接受操作完成时调用
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch (SocketException ex)
            {
                ProcessAcceptError(acceptEventArgs, ex, true);
            }
            catch (ObjectDisposedException ex)
            {
                ProcessAcceptError(acceptEventArgs, ex, true);
            }
            catch (NullReferenceException ex)
            {
                ProcessAcceptError(acceptEventArgs, ex, false);
            }
            catch (InvalidOperationException ex)
            {
                ProcessAcceptError(acceptEventArgs, ex, false);
            }
            catch (Exception ex)
            {
                ProcessAcceptError(acceptEventArgs, ex, false);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            Interlocked.Increment(ref m_numConnectedSockets);
            SocketAsyncUserToken userToken = socketAsyncUserTokenPool.Pop();
            userToken.EnterActive();
            userToken.ConnectSocket = acceptEventArgs.AcceptSocket;
            if (IsNetExKeepAlive)
            {
                userToken.ConnectSocket.IOControl(IOControlCode.KeepAliveValues, keepAliveValues, null);
            }
            userToken.SessionID = SessionIdGenerator.Get();
            userToken.ClientIP = ((IPEndPoint)acceptEventArgs.AcceptSocket.RemoteEndPoint).Address.ToString();
            if (OnConnected != null)
                OnConnected(userToken);
            // 连接后，将接收发送到连接
            bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(userToken.ReceiveEventArgs);
            }

            // 接受下一个连接请求
            StartAccept(acceptEventArgs);
        }

        //特殊处理连接请求时的异常
        void ProcessAcceptError(SocketAsyncEventArgs acceptEventArgs, Exception ex, bool acceptNew)
        {
            if (InnerException != null)
                InnerException(ex);
            acceptErrorCount++;
            if (acceptErrorCount > acceptErrorMaxCount && DangerError != null)
            {
                DangerError(string.Format("接收新的连接时发生错误,错误数{0}", acceptErrorCount));
                return;
            }
            if (acceptNew)
            {
                try
                {
                    StartAccept(acceptEventArgs);
                }
                catch (SocketException e)
                {
                    ProcessAcceptError(acceptEventArgs, e, true);
                }
                catch (Exception e)
                {
                    ProcessAcceptError(acceptEventArgs, e, true);
                }
            }
            else
            {
                AcceptEventArg_Completed(null, acceptEventArgs);
            }
        }

        // 无论何时在套接字上完成接收或发送操作，都会调用此方法
        void IO_Completed(object sender, SocketAsyncEventArgs args)
        {
            // 确定刚刚完成的操作类型并调用相关联的处理程序

            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(args);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(args);
                    break;
                default:
                    if (InnerException != null)
                    {
                        InnerException(new ArgumentException("无法处理套接字完成的最后一个操作"));
                    }
                    break;
            }

        }
        // 异步接收操作完成时调用此方法。
        //如果远程主机关闭了连接，则该套接字被关闭。 
        private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            // 检查远程主机是否关闭连接
            SocketAsyncUserToken userToken = (SocketAsyncUserToken)receiveEventArgs.UserToken;
            if (receiveEventArgs.BytesTransferred > 0 && receiveEventArgs.SocketError == SocketError.Success)
            {
                if (OnReceived != null)
                    OnReceived(receiveEventArgs);
                bool willRaiseEvent = false;
                try
                {
                    willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs);
                }
                catch (SocketException ex)
                {
                    if (InnerException != null)
                        InnerException(ex);
                    CloseClientSocket(receiveEventArgs);
                    return;
                }
                catch (ObjectDisposedException ex)
                {
                    if (InnerException != null)
                        InnerException(ex);
                    CloseClientSocket(receiveEventArgs);
                    return;
                }
                catch (Exception ex)
                {
                    if (InnerException != null)
                        InnerException(ex);
                    CloseClientSocket(receiveEventArgs);
                    return;
                }
                if (!willRaiseEvent)
                {
                    ProcessReceive(userToken.ReceiveEventArgs);
                }
            }
            else
            {
                if (receiveEventArgs.BytesTransferred > 0 && InnerException != null)
                {
                    InnerException(new SocketErrorException(string.Format("异步接收失败:{0}", receiveEventArgs.SocketError.ToString())));
                }
                CloseClientSocket(receiveEventArgs);
            }
        }

        public void SendAsync(SocketAsyncUserToken userToken, SocketAsyncEventArgs sendEventArgs, int offset, int count)
        {
            sendEventArgs.SetBuffer(offset, count);
            bool willRaiseEvent = userToken.ConnectSocket.SendAsync(sendEventArgs);
            if (!willRaiseEvent)
            {
                ProcessSend(userToken.SendEventArgs);
            }
        }
        // 异步发送操作完成时调用此方法
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                if (OnSendCompleted != null)
                    OnSendCompleted(sendEventArgs);
            }
            else
            {
                if (InnerException != null)
                {
                    InnerException(new SocketErrorException(string.Format("异步发送失败:{0}", sendEventArgs.SocketError.ToString())));
                }
                CloseClientSocket(sendEventArgs);
            }
        }

        public void CloseClientSocket(SocketAsyncEventArgs args)
        {
            SocketAsyncUserToken token = args.UserToken as SocketAsyncUserToken;
            try
            {
                if (!token.ExitActive()) return;
            }
            catch (NullReferenceException ex)
            {
                InnerException(ex);
            }
            if (OnClosed != null)
                OnClosed(token);
            // 关闭与关联的套接字
            try
            {
                token.ConnectSocket.Shutdown(SocketShutdown.Both);
            }
            // 如果进程已经关闭，则抛出
            catch (SocketException ex)
            {
                if (InnerException != null)
                    InnerException(ex);
            }
            catch (ObjectDisposedException ex)
            {
                if (InnerException != null)
                    InnerException(ex);
            }
            try
            {
                token.ConnectSocket.Close();
                // 递减计数器跟踪连接到服务器的总数
                Interlocked.Decrement(ref m_numConnectedSockets);
                m_maxNumberAcceptedClients.Release();
                // 释放SocketAsyncEventArg，以便它们可以被另一个重用

                token.Clear();
                socketAsyncUserTokenPool.Push(token);
            }
            catch (Exception ex)
            {
                if (InnerException != null)
                    InnerException(ex);
            }
        }

        //设置连接检测数值(处理出现网络链路异常[如:网线拔出\交换机掉电\客户端机器掉电]时,服务端不会出现任何异常的问题)
        /// 客户端关闭或出现异常时,服务端可知晓,不通过此处理
        /// <param name="keepaliveTime">多长时间后开始第一次探测（单位：毫秒）</param>
        /// <param name="keepaliveInterval">探测时间间隔（单位：毫秒）</param>
        private void SetKeepAliveValue(uint keepaliveTime, uint keepaliveInterval)
        {
            if (keepaliveTime == 0)
            {
                IsNetExKeepAlive = false;
                return;
            }
            uint onoff = 1;
            IsNetExKeepAlive = true;
            int sizeOfUInt = Marshal.SizeOf(onoff);
            keepAliveValues = new byte[sizeOfUInt * 3];
            BitConverter.GetBytes(onoff).CopyTo(keepAliveValues, 0);
            BitConverter.GetBytes(keepaliveTime).CopyTo(keepAliveValues, sizeOfUInt);
            BitConverter.GetBytes(keepaliveInterval).CopyTo(keepAliveValues, sizeOfUInt * 2);
        }
    }
}
