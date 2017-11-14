using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Nice.Network.Demo
{
    //public delegate void ReceiveHandler(byte[] data, EndPoint Remote);
    public class UdpClient
    {
        private int port = 0;
        private bool IsRunning = false;
        private string broadcastaddr = null;
        private readonly Socket listener = null;
        private readonly Socket socbrdc = null;
        //public event ReceiveHandler ReceiveEvent = null;
        public UdpClient(int port)
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.port = port;
            IPEndPoint ipaddr = new IPEndPoint(IPAddress.Any, this.port);
            listener.Bind(ipaddr);
            this.IsRunning = true;
            Task.Run(() =>
            {
                Receive();
            });
        }

        public UdpClient(int port, string broadcastaddr) : this(port)
        {
            this.broadcastaddr = broadcastaddr;
            socbrdc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socbrdc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        }

        public void Send(byte[] data, string targetIp)
        {
            IPEndPoint iped = new IPEndPoint(IPAddress.Parse(targetIp), port);
            EndPoint Remote = (EndPoint)(iped);
            listener.SendTo(data, data.Length, SocketFlags.None, Remote);
        }

        public void Boadcast(byte[] data)
        {
            IPEndPoint iped = new IPEndPoint(IPAddress.Parse(broadcastaddr), this.port);
            socbrdc.SendTo(data, data.Length, SocketFlags.None, iped);
        }

        private void Receive()
        {
            IPEndPoint ipaddr = new IPEndPoint(IPAddress.Any, this.port);
            EndPoint Remote = (EndPoint)ipaddr;
            byte[] data = new byte[32];
            int length = 0;
            while (IsRunning)
            {
                length = listener.Receive(data);
                //if (length > 0 && ReceiveEvent != null)
                //    ReceiveEvent(data, Remote);
            }
        }
    }
}
