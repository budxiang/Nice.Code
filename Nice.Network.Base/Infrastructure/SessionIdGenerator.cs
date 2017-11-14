using System;
using System.Collections.Generic;
using System.Text;

namespace Nice.Network.Base.Infrastructure
{
    public class SessionIdGenerator
    {
        private static readonly Queue<string> ids = new Queue<string>();
        private static int StartNum = 1;
        private static string prefixDate = DateTime.Now.ToString("yyyyMMdd");
        public static void SetStart(int start)
        {
            StartNum = start;
        }
        public static string Get()
        {
            string id = null;
            lock (ids)
            {
                id = GetId();
                if (id == null)
                {
                    string strNow = DateTime.Now.ToString("yyyyMMdd");
                    if (prefixDate != strNow)
                    {
                        StartNum = 1;
                        prefixDate = strNow;
                    }
                    id = strNow + ZeroPrefix(StartNum, 8);
                    StartNum++;
                    CreateId(strNow, StartNum);
                }
            }
            return id;
        }

        private static void CreateId(string strNow, int id)
        {
            int maxid = id + 50;
            for (int i = id; i < maxid; i++)
            {
                ids.Enqueue(strNow + ZeroPrefix(i, 8));
            }
            StartNum = maxid;
        }
        private static string GetId()
        {
            if (ids.Count > 0)
                return ids.Dequeue();
            return null;
        }

        private static string ZeroPrefix(int num, int len)
        {
            StringBuilder sb = new StringBuilder(len);
            int length = len - num.ToString().Length;
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    sb.Append("0");
                }
            }
            sb.Append(num);
            return sb.ToString();
        }
    }
}
