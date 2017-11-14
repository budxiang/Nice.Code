using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Nice.Network.Demo
{
    public class SocketAsyncUserTokenCache
    {
        private readonly Dictionary<string, SocketAsyncUserToken> dictionary = null;
        private readonly SocketAsyncUserTokenPool userTokenPool = null;
        private readonly int port = 0;
        public SocketAsyncUserTokenCache(int capacity,int port, SocketAsyncUserTokenPool userTokenPool)
        {
            dictionary = new Dictionary<string, SocketAsyncUserToken>(capacity);
            this.userTokenPool = userTokenPool;
            this.port = port;
        }

        public SocketAsyncUserToken Get(string ipaddr)
        {
            if (dictionary.ContainsKey(ipaddr))
            {
                return dictionary[ipaddr];
            }
            else
            {
                SocketAsyncUserToken userToken = userTokenPool.Pop();
                userToken.SendEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ipaddr), port);
                dictionary.Add(ipaddr, userToken);
                return userToken;
            }
        }
    }
}
