using System.Collections.Generic;

namespace Nice.Network.Base
{
    public class DataHolder
    {
        private Queue<byte[]> queues = null;

        public DataHolder()
        {
            queues = new Queue<byte[]>(4);
        }
        public void Push(byte[] buffer)
        {
            queues.Enqueue(buffer);
        }

        public byte[] Pop()
        {
            if (queues.Count > 0)
                return queues.Dequeue();
            return null;
        }

        public int GetCount()
        {
            return queues.Count;
        }
    }

    public class BufferSegment
    {
        private int offset;
        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        private int transferred;
        public int Transferred
        {
            get { return transferred; }
            set { transferred = value; }
        }

        private int count;
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public BufferSegment(int offset, int count)
        {
            this.offset = offset;
            this.count = count;
        }
    }
   
}
