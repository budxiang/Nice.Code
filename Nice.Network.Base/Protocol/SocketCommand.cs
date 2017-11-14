using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nice.Network.Base.Protocol
{
    public enum SocketCommand : short
    {
        /// <summary>
        /// 登录验证
        /// </summary>
        Login = 1001,
        /// <summary>
        /// 心跳
        /// </summary>
        Active = 1002,
        /// <summary>
        /// 退出\关闭连接
        /// </summary>
        Close = 1003,
        /// <summary>
        /// 注册客户端
        /// </summary>
        Register = 1004,
    }


}
