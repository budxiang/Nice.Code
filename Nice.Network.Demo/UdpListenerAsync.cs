using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Nice.Network.Demo
{
    public delegate void SocketEventHandler(SocketAsyncEventArgs receiveEventArgs);
    public class UdpListenerAsync
    {
        private readonly int port = 0;
        private readonly int bufferSize = 0;
        private readonly Socket listener = null;
        private readonly SocketAsyncUserTokenPool userTokenPool = null;
        private readonly SocketAsyncUserTokenCache userTokenCache= null;
        private readonly BufferManager m_bufferManager=null;  //Socket操作的缓冲区
        public event SocketEventHandler Received = null;
        public event SocketEventHandler SendCompleted = null;
        public UdpListenerAsync(int numConnections, int bufferSize, int port)
        {
            this.port = port;
            this.bufferSize = bufferSize;
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipaddr = new IPEndPoint(IPAddress.Any, this.port);
            listener.Bind(ipaddr);
            m_bufferManager = new BufferManager(bufferSize * numConnections, bufferSize);
            // 分配一个大字节缓冲区,所有的I/O操作都使用一个.这样可以防止内存碎片化
            m_bufferManager.InitBuffer();
            userTokenPool = new SocketAsyncUserTokenPool(numConnections);
            userTokenCache = new SocketAsyncUserTokenCache(numConnections, port, userTokenPool);
            SocketAsyncUserToken userToken = null;
            for (int i = 0; i < numConnections; i++)
            {
                userToken = new SocketAsyncUserToken();
                userToken.SendEventArgs = new SocketAsyncEventArgs();
                userToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                m_bufferManager.SetBuffer(userToken.SendEventArgs);
                userTokenPool.Push(userToken);
            }
            SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
            StartReceive(receiveEventArgs);
        }

        private void StartReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            byte[] recvBuffer = new byte[bufferSize];
            receiveEventArgs.SetBuffer(recvBuffer, 0, recvBuffer.Length);
            receiveEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            bool willRaiseEvent = listener.ReceiveFromAsync(receiveEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(receiveEventArgs);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            if (receiveEventArgs.BytesTransferred > 0 && receiveEventArgs.SocketError == SocketError.Success)
            {
                if (Received != null)
                    Received(receiveEventArgs);
                bool willRaiseEvent = listener.ReceiveFromAsync(receiveEventArgs);
                if (!willRaiseEvent)
                    IO_Completed(this, receiveEventArgs);
            }
            else
            {
                //Close
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    ProcessReceive(args);
                    break;
                case SocketAsyncOperation.SendTo:
                    ProcessSend(args);
                    break;
                default:
                    break;
            }
        }

        public void SendAsync(byte[] data, string targetIp)
        {
            SocketAsyncUserToken userToken = userTokenCache.Get(targetIp);
            Buffer.BlockCopy(data, 0, userToken.SendEventArgs.Buffer, userToken.SendEventArgs.Offset, data.Length);
            userToken.SendEventArgs.SetBuffer(userToken.SendEventArgs.Offset, data.Length);
            listener.SendToAsync(userToken.SendEventArgs);
        }

        private void ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                SendCompleted(sendEventArgs);
            }
            else
            {
            }
        }

    }
}
