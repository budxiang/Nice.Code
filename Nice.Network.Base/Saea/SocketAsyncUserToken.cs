using Nice.Network.Base.Protocol;
using System.Net.Sockets;
using System.Threading;

namespace Nice.Network.Base.Saea
{
    /// <summary>
    /// SocketAsyncEventArgs 用户令牌
    /// </summary>
    public class SocketAsyncUserToken
    {
        public SocketAsyncUserToken()
        {
            receiveEventArgs = new SocketAsyncEventArgs();
            sendEventArgs = new SocketAsyncEventArgs();
            receiveEventArgs.UserToken = this;
            sendEventArgs.UserToken = this;
            receiveHeader = new Header();
            sendHeader = new Header();
        }
        private SocketAsyncEventArgs receiveEventArgs;
        /// <summary>
        ///用于Socket接收数据的 SocketAsyncEventArgs
        /// </summary>
        public SocketAsyncEventArgs ReceiveEventArgs
        {
            get { return receiveEventArgs; }
            set { receiveEventArgs = value; }
        }

        private Header receiveHeader;
        public Header ReceiveHeader
        {
            get { return receiveHeader; }
            set { receiveHeader = value; }
        }

        private byte[] bytesReceiveOutOfSize;
        public byte[] BytesReceiveOutOfSize
        {
            get { return bytesReceiveOutOfSize; }
            set { bytesReceiveOutOfSize = value; }
        }
        private SocketAsyncEventArgs sendEventArgs;
        /// <summary>
        ///用于Socket发送数据的 SocketAsyncEventArgs
        /// </summary>
        public SocketAsyncEventArgs SendEventArgs
        {
            get { return sendEventArgs; }
            set { sendEventArgs = value; }
        }

        private Header sendHeader;
        public Header SendHeader
        {
            get { return sendHeader; }
            set { sendHeader = value; }
        }

        private int isSending;//0 未占用,可以发送，1-正在发送
        /// <summary>
        /// 是否进入发送
        /// </summary>
        public bool EnterSend()
        {
            return Interlocked.CompareExchange(ref this.isSending, 1, 0) == 0;
        }
        public void ExitSend()
        {
             Interlocked.CompareExchange(ref this.isSending, 0, 1);
        }

        private byte[] bytesSendOutOfSize;
        public byte[] BytesSendOutOfSize
        {
            get { return bytesSendOutOfSize; }
            set { bytesSendOutOfSize = value; }
        }

        private Socket connectSocket;
        /// <summary>
        /// 使用的Socket对象
        /// </summary>
        public Socket ConnectSocket
        {
            get
            {
                return connectSocket;
            }
            set
            {
                connectSocket = value;
                receiveEventArgs.AcceptSocket = connectSocket;
                sendEventArgs.AcceptSocket = connectSocket;
            }
        }
        private string sessionID;
        /// <summary>
        /// 会话ID,客户端ID
        /// </summary>
        public string SessionID
        {
            get { return sessionID; }
            set { sessionID = value; }
        }
        private string clientIP;
        /// <summary>
        /// 客户端IP
        /// </summary>
        public string ClientIP
        {
            get { return clientIP; }
            set { clientIP = value; }
        }

        private int isActive;//0 未激活，1-激活
        /// <summary>
        /// 是否进入激活状态
        /// </summary>
        /// <returns></returns>
        public bool EnterActive()
        {
            return Interlocked.CompareExchange(ref this.isActive, 1, 0) == 0;
        }
        //是否退出激活状态
        public bool ExitActive()
        {
            return Interlocked.CompareExchange(ref this.isActive, 0, 1) == 1;
        }
        // 是否进入激活状态
        public bool IsActive()
        {
            return Interlocked.CompareExchange(ref this.isActive, 2, 2) == 1;
        }

        private int bizState;//0-警告可用,1-警告繁忙
        /// <summary>
        /// 进入业务状态
        /// </summary>
        /// <returns></returns>
        public bool EnterBizState()
        {
            return Interlocked.CompareExchange(ref this.bizState, 1, 0) == 0;
        }
        /// <summary>
        /// 业务状态空闲
        /// </summary>
        /// <returns></returns>
        public bool FreeBizState()
        {
            return Interlocked.CompareExchange(ref this.bizState, 2, 2) == 0;
        }
        /// <summary>
        /// 退出业务状态
        /// </summary>
        /// <param name="bizState">0-退出警告状态</param>
        public bool ExitBizState()
        {
            return Interlocked.CompareExchange(ref this.bizState, 0, 1) == 1;
        }

        private int userId;
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        public void ResetReceive()
        {
            this.receiveHeader.DataLength = 0;
            this.receiveHeader.Transferred = 0;
            this.receiveHeader.Start = false;
            this.receiveHeader.End = false;
            if (this.bytesReceiveOutOfSize != null)
                this.bytesReceiveOutOfSize = null;
        }

        public void ResetSend()
        {
            this.SendHeader.DataLength = 0;
            this.SendHeader.Transferred = 0;
            this.SendHeader.Start = false;
            this.SendHeader.End = false;
            ExitSend();
            if (this.bytesSendOutOfSize != null)
                this.bytesSendOutOfSize = null;
        }

        public void Clear()
        {
            this.sessionID = null;
            this.clientIP = null;
            ExitBizState();
            ResetReceive();
            ResetSend();
        }
    }
}
