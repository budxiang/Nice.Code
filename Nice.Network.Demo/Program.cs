using System;
using System.Net.Sockets;
using System.Threading;

namespace Nice.Network.Demo
{
    class Program
    {
        //static UdpClient client = null;
        private static UdpListenerAsync udpListenerAsync = null;
        static void Main(string[] args)
        {
            // client = new UdpClient(25000, "192.168.25.255");
            //Thread.Sleep(1000);
            //client.ReceiveEvent += Client_ReceiveEvent;
            udpListenerAsync = new UdpListenerAsync(8, 2048, 25000);
            udpListenerAsync.Received += UdpListenerAsync_Received;
            for (int i = 0; i < 100; i++)
            {
                Boadcast(1, new byte[1] { 0 }, null, null);
                //Thread.Sleep(400);
                Send("192.168.25.173", 1, null, null, null);
                //Thread.Sleep(400);
                Send("192.168.25.118", 1, null, null, null);
            }
            Console.ReadLine();
        }

        private static void UdpListenerAsync_Received(SocketAsyncEventArgs receiveEventArgs)
        {
            if (receiveEventArgs.BytesTransferred != 32) return;
            byte[] recvData = new byte[32];
            Buffer.BlockCopy(receiveEventArgs.Buffer, receiveEventArgs.Offset, recvData, 0, receiveEventArgs.BytesTransferred);
            Console.WriteLine("{0}", ByteConverter.ByteArrayToHexStr(recvData));
        }

        private static void Send(string targetIp, byte protocal, byte[] data1, byte[] data2, byte[] data)
        {
            try
            {
                //client.Send(Package(protocal, data1, data2, data), targetIp);
                udpListenerAsync.SendAsync(Package(protocal, data1, data2, data), targetIp);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Boadcast(byte protocal, byte[] data1, byte[] data2, byte[] data)
        {
            try
            {
                //client.Boadcast(Package(protocal, data1, data2, data));
                udpListenerAsync.SendAsync(Package(protocal, data1, data2, data), "192.168.25.255");
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static short pckgNo = 0;

        private static short GetNo()
        {
            pckgNo++;
            if (pckgNo == short.MaxValue)
            {
                pckgNo = 1;
            }
            return pckgNo;
        }
        private static byte[] Package(byte protocal, byte[] data1, byte[] data2, byte[] data)
        {
            byte[] senddata = new byte[32];
            senddata[0] = 0x7e;
            senddata[1] = 0x01;
            byte[] btNo1 = BitConverter.GetBytes(GetNo());
            //Array.Copy(btNo1, 0, senddata, 2, 2);
            senddata[2] = btNo1[1];//高地位
            senddata[3] = btNo1[0];
            senddata[4] = protocal;
            int currentIndex = 5;
            if (data1 != null)
            {
                Array.Copy(data1, 0, senddata, currentIndex, data1.Length);
                currentIndex = 5 + data1.Length;
            }
            else
            {
                senddata[currentIndex] = 0;
                currentIndex++;
            }
            if (data2 != null)
            {
                Array.Copy(data2, 0, senddata, currentIndex, data2.Length);
                currentIndex += data2.Length;
            }
            else
            {
                senddata[currentIndex] = 0;
                currentIndex++;
            }
            if (data != null)
            {
                Array.Copy(data, 0, senddata, 4, data.Length);
            }
            senddata[30] = 0x55;
            senddata[31] = 0xaa;
            return senddata;
        }
    }
}
