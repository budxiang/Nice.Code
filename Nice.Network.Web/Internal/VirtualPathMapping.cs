using System.Collections.Generic;

namespace Nice.Network.Web
{
    internal class VirtualPathMapping
    {
        private readonly Dictionary<string, string> dic = new Dictionary<string, string>();

        internal void Add(IDictionary<string, string> keyValues)
        {
            foreach (KeyValuePair<string, string> item in keyValues)
            {
                if (dic.ContainsKey(item.Key))
                {
                    dic[item.Key] = item.Value;
                }
                else
                {
                    dic.Add(item.Key, item.Value);
                }
            }
        }

        internal void Add(string path, string target)
        {
            if (dic.ContainsKey(path))
            {
                dic[path] = target;
            }
            else
            {
                dic.Add(path, target);
            }
        }

        internal bool Validate(string path)
        {
            return dic.ContainsKey(path);
        }

        internal string Get(string path)
        {
            return dic[path];
        }

    }
}
