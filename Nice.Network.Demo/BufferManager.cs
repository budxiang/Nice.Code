using System.Collections.Generic;
using System.Net.Sockets;

namespace Nice.Network.Demo
{
    /// <summary>
    /// 分配给SocketAsyncEventArgs对象的单个大型缓冲区,用于每个Socket I/O操作,这样可以轻松地重新使用缓冲区，并防止破坏堆内存
    /// </summary>
    public class BufferManager
    {
        int m_numBytes;  // 由缓冲池控制的总字节数
        byte[] m_buffer; // 由缓冲区管理器维护的底层字节数组
        Stack<int> m_freeIndexPool;     
        int m_currentIndex;
        int bufferSize;

        public BufferManager(int totalBytes, int bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            this.bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        // 分配缓冲池使用的缓冲区 
        public void InitBuffer()
        {
            // 创建一个大的大缓冲区，并将其分隔给每个SocketAsyncEventArg对象
            m_buffer = new byte[m_numBytes];
        }


        ///  为SocketAsyncEventArgs分配缓冲区
        // <returns>如果缓冲区成功设置，则为true，否则为false</returns>  
        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), bufferSize);
            }
            else
            {
                if ((m_numBytes - bufferSize) < m_currentIndex)
                {
                    return false;
                }
                args.SetBuffer(m_buffer, m_currentIndex, bufferSize);
                m_currentIndex += bufferSize;
            }
            return true;
        }

        // 从SocketAsyncEventArg对象中删除缓冲区。
        //这将缓冲区释放回缓冲池
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
