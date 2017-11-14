using System;
using System.Text;
using Nice.Network.Base.Protocol;

namespace Nice.Network.Base
{
    public class PackageHandler
    {
        public static void SinglePackage(byte[] bytesCmd, byte[] src, byte[] target, int dstOffset)
        {
            byte byteCode = (1 | 2);
            byte[] bytesSize = BitConverter.GetBytes(src.Length);
            Package(byteCode, bytesCmd, bytesSize, src, 0, src.Length, target, dstOffset);
        }
        //包装数据
        public static void Package(byte byteCode, byte[] bytesCmd, byte[] lenOrSn, byte[] src, int srcOffset, int srcCount, byte[] target, int dstOffset)
        {
            target[dstOffset + 0] = ProtocolConst.Prefix;
            target[dstOffset + 1] = byteCode;
            Buffer.BlockCopy(bytesCmd, 0, target, dstOffset + ProtocolConst.CommandStart, bytesCmd.Length);
            Buffer.BlockCopy(lenOrSn, 0, target, dstOffset + ProtocolConst.DataSizeStart, ProtocolConst.DataSizeLength);
            Buffer.BlockCopy(src, srcOffset, target, dstOffset + ProtocolConst.DataStart, srcCount);
            target[dstOffset + ProtocolConst.DataStart + srcCount] = ProtocolConst.Suffix;
        }

        public static void Package(byte byteCode, byte[] bytesCmd, byte[] lenOrSn, byte[] segment, byte[] src, int srcOffset, int srcCount, byte[] buffer, int dstOffset)
        {
            buffer[dstOffset + 0] = ProtocolConst.Prefix;
            buffer[dstOffset + 1] = byteCode;
            Buffer.BlockCopy(bytesCmd, 0, buffer, dstOffset + ProtocolConst.CommandStart, bytesCmd.Length);
            Buffer.BlockCopy(lenOrSn, 0, buffer, dstOffset + ProtocolConst.DataSizeStart, ProtocolConst.DataSizeLength);
            int segmentLen = 0;
            if (segment != null)
            {
                segmentLen = segment.Length;
                Buffer.BlockCopy(segment, 0, buffer, dstOffset + ProtocolConst.DataStart, segmentLen);
            }
            Buffer.BlockCopy(src, srcOffset, buffer, dstOffset + ProtocolConst.DataStart + segmentLen, srcCount);
            buffer[dstOffset + ProtocolConst.DataStart + srcCount + segmentLen] = ProtocolConst.Suffix;
        }
        //8bit操作码
        public static byte GetByteCode(bool blStart, bool blEnd)
        {
            byte byteCode = 0;
            if (blStart) byteCode += 1;
            if (blEnd) byteCode += 2;
            return byteCode;
        }

        public static void GetByteCode(byte byteCode, out bool blStart, out bool blEnd)
        {
            blStart = (byteCode & 1) == 1;
            blEnd = (byteCode & 2) == 2;
        }

        public static void UnPackage(Header header, byte[] buffer, int offset, int transferred)
        {
            if (buffer[offset] != ProtocolConst.Prefix || buffer[offset + transferred - 1] != ProtocolConst.Suffix)
            {
                throw new IllegalDataException("非法数据");
            }
            byte byteCode = buffer[offset + ProtocolConst.PrefixLength];
            header.Start = (byteCode & 1) == 1;
            header.End = (byteCode & 2) == 2;
            short ncmd = BitConverter.ToInt16(buffer, offset + ProtocolConst.CommandStart);
            header.Cmd = (SocketCommand)Enum.ToObject(typeof(SocketCommand), ncmd);
            if (header.Start)
            {
                header.DataLength = BitConverter.ToInt32(buffer, offset + ProtocolConst.DataSizeStart);
            }
            header.Transferred += transferred;
        }
    }
}
