using System;
using System.Collections.Generic;

namespace Nice.Network.Base.Saea
{
    /// <summary>
    /// SocketAsyncEventArgs用户令牌池
    /// </summary>
    public class SocketAsyncUserTokenPool
    {
        Stack<SocketAsyncUserToken> m_pool;
        public SocketAsyncUserTokenPool(int capacity)
        {
            m_pool = new Stack<SocketAsyncUserToken>(capacity);
        }

        public void Push(SocketAsyncUserToken item)
        {
            if (item == null) { throw new ArgumentNullException("添加的SocketAsynUserToken不能为空"); }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        public SocketAsyncUserToken Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }
        public int Count
        {
            get { return m_pool.Count; }
        }
    }
}
