using System;
using System.Security.Cryptography;
using System.Text;

namespace Nice.Network.WebSockets.Protocol
{
    public class ProtocolDraft10 : IProtocol
    {
        private const string MagicKey = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private const char charOne = '1';
        private const char charZero = '0';

        #region Handshake
        /// <summary>
        /// 握手
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public byte[] Handshake(WebSocketRequest request)
        {
            StringBuilder header = new StringBuilder();
            header.Append(WebSocketConstant.ResponseHeadLine10);
            header.AppendFormat(WebSocketConstant.ResponseUpgradeLine, request.Upgrade);
            header.AppendFormat(WebSocketConstant.ResponseConnectionLine, request.Connection);
            header.AppendFormat(WebSocketConstant.ResponseAcceptLine, produceAcceptKey(request.SecWebSocketKey));
            header.AppendFormat(WebSocketConstant.ResponseOriginLine, request.SecWebSocketOrigin);
            header.AppendFormat(WebSocketConstant.ResponseLocationLine, request.Host);
            header.Append(Environment.NewLine);
            return Encoding.UTF8.GetBytes(header.ToString());
        }

        private string produceAcceptKey(string webSocketKey)
        {
            byte[] acceptKey = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(webSocketKey + MagicKey));
            return Convert.ToBase64String(acceptKey);
        }
        #endregion

        #region  包装数据

        public void Package(OpCode opCode, byte[] src,  byte[] dst, int dstOffset, out int count)
        {
            int length = src.Length;
            int offset = 0;
            if (length < 126)
            {
                offset = 2;
                dst[dstOffset + 1] = (byte)length;
            }
            else if (length < 65536)
            {
                offset = 4;
                dst[dstOffset + 1] = 126;
                dst[dstOffset + 2] = (byte)(length / 256);
                dst[dstOffset + 3] = (byte)(length % 256);
            }
            else
            {
                offset = 10;
                dst[dstOffset + 1] = 127;

                int left = length;
                int unit = 256;

                for (int i = 9; i > 1; i--)
                {
                    dst[dstOffset + i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
            }
            dst[dstOffset] = (byte)((byte)opCode | 0x80);
            if (length > 0)
            {
                Buffer.BlockCopy(src, 0, dst, dstOffset + offset, length);
            }
            count = length + offset;
        }

        #endregion

        #region 拆包数据
        public Message UnPackage(byte[] data, int offset, int count)
        {
            byte[] buffer = new byte[14];
            if (count >= 14)
                Buffer.BlockCopy(data, offset, buffer, 0, 14);
            else
                Buffer.BlockCopy(data, offset, buffer, 0, count);
            MessageHeader header = analyseHead(data, offset);
            Message msg = new Message();
            msg.Header = header;

            byte[] payload;
            if (header != null)
            {
                payload = new byte[count - header.PayloadDataStartIndex];
                Buffer.BlockCopy(data, offset + header.PayloadDataStartIndex, payload, 0, payload.Length);
                if (header.MASK == charOne)
                {
                    for (int i = 0; i < payload.Length; i++)
                    {
                        payload[i] = (byte)(payload[i] ^ header.Maskey[i % 4]);
                    }
                }
            }
            else
            {
                msg.Data = Encoding.UTF8.GetString(data, offset, count);
                return msg;
            }

            if (header.OpCode == OpCode.Text)
                msg.Data = Encoding.UTF8.GetString(payload, 0, payload.Length);

            return msg;
        }

        private MessageHeader analyseHead(byte[] buffer, int offset)
        {
            MessageHeader header = new MessageHeader();
            header.FIN = (buffer[offset] & 0x80) == 0x80 ? charOne : charZero;
            header.RSV1 = (buffer[offset] & 0x40) == 0x40 ? charOne : charZero;
            header.RSV2 = (buffer[offset] & 0x20) == 0x20 ? charOne : charZero;
            header.RSV3 = (buffer[offset] & 0x10) == 0x10 ? charOne : charZero;

            if ((buffer[offset] & 0xA) == 0xA)
                header.OpCode = OpCode.Pong;
            else if ((buffer[offset] & 0x9) == 0x9)
                header.OpCode = OpCode.Ping;
            else if ((buffer[offset] & 0x8) == 0x8)
                header.OpCode = OpCode.Close;
            else if ((buffer[offset] & 0x2) == 0x2)
                header.OpCode = OpCode.Binary;
            else if ((buffer[offset] & 0x1) == 0x1)
                header.OpCode = OpCode.Text;
            else if ((buffer[offset] & 0x0) == 0x0)
                header.OpCode = OpCode.Continuation;

            header.MASK = (buffer[offset + 1] & 0x80) == 0x80 ? charOne : charZero;
            int len = buffer[1] & 0x7F;
            if (len == 126)
            {
                header.Payloadlen = (ushort)(buffer[offset + 2] << 8 | buffer[offset + 3]);
                if (header.MASK == charOne)
                {
                    header.Maskey = new byte[4];
                    Buffer.BlockCopy(buffer, offset + 4, header.Maskey, 0, 4);
                    header.PayloadDataStartIndex = 8;
                }
                else
                    header.PayloadDataStartIndex = 4;
            }
            else if (len == 127)
            {
                byte[] byteLen = new byte[8];
                Buffer.BlockCopy(buffer, offset + 4, byteLen, 0, 8);
                header.Payloadlen = BitConverter.ToUInt64(byteLen, 0);
                if (header.MASK == charOne)
                {
                    header.Maskey = new byte[4];
                    Buffer.BlockCopy(buffer, offset + 10, header.Maskey, 0, 4);
                    header.PayloadDataStartIndex = 14;
                }
                else
                    header.PayloadDataStartIndex = 10;
            }
            else
            {
                if (header.MASK == charOne)
                {
                    header.Maskey = new byte[4];
                    Buffer.BlockCopy(buffer, offset + 2, header.Maskey, 0, 4);
                    header.PayloadDataStartIndex = 6;
                }
                else
                    header.PayloadDataStartIndex = 2;
            }
            return header;
        }
        #endregion

    }
}
