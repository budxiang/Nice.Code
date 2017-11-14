using System;

namespace Nice.Network.Base
{
    public delegate void DangerErrorHandler(string msg);
    public class SocketErrorException : Exception
    {
        public SocketErrorException() : base() { }
        public SocketErrorException(string message) : base(message) { }
        public SocketErrorException(string message, Exception innerException) : base(message, innerException) { }

        //public override string Message{ get{ }}
    }
}
