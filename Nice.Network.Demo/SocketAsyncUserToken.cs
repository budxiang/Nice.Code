using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Nice.Network.Demo
{
    public class SocketAsyncUserToken
    {
        private SocketAsyncEventArgs sendEventArgs;

        public SocketAsyncEventArgs SendEventArgs { get => sendEventArgs; set => sendEventArgs = value; }


    }
}
