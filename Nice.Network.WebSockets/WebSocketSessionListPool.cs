using System.Collections.Generic;

namespace Nice.Network.WebSockets
{
    /// <summary>
    /// 发送Session集合池,减少每次重新创建新的集合
    /// </summary>
    public class WebSocketSessionListPool
    {
        private readonly Stack<IList<WebSocketSession>> m_pool;

        public WebSocketSessionListPool(int capacity)
        {
            m_pool = new Stack<IList<WebSocketSession>>(capacity);
        }

        public void Push(IList<WebSocketSession> item)
        {
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        public IList<WebSocketSession> Pop()
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
