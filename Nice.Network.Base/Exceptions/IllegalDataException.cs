using System;

namespace Nice.Network.Base
{
    public class IllegalDataException : Exception
    {
        public IllegalDataException() : base() { }
        public IllegalDataException(string message) : base(message) { }
        public IllegalDataException(string message, Exception innerException) : base(message, innerException) { }

        //public override string Message{ get{ }}
    }
}
