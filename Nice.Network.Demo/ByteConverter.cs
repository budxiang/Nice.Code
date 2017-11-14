using System;
using System.Collections.Generic;
using System.Text;

namespace Nice.Network.Demo
{
    public class ByteConverter
    {
        /// <summary>
        /// 字节数组转换成十六进制字符串
        /// </summary>
        /// <param name="bytes">要转换的字节数组</param>
        /// <returns></returns>
        public static string ByteArrayToHexStr(byte[] byteArray)
        {
            int capacity = byteArray.Length * 2;
            StringBuilder sb = new StringBuilder(capacity);

            if (byteArray != null)
            {
                for (int i = 0; i < byteArray.Length; i++)
                {
                    sb.Append(byteArray[i].ToString("X2"));
                }
            }
            return sb.ToString();
        }
    }
}
