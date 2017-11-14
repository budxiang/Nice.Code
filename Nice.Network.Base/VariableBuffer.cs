using System;

namespace Nice.Network.Base
{
    public class VariableBuffer
    {
        private byte[] bytes;
        public byte[] Bytes
        {
            get { return bytes; }
            set { bytes = value; }
        }

        private int length;

        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        private int capacity;


        public VariableBuffer(int capacity)
        {
            this.capacity = capacity;
            bytes = new byte[capacity];
        }

        public void Write(byte[] data, int srcOffset, int count)
        {
            if (count > capacity - length)
            {
                byte[] temp = new byte[length];
                Buffer.BlockCopy(bytes, 0, temp, 0, length);
                capacity *= 2;
                bytes = new byte[capacity];
                Buffer.BlockCopy(temp, 0, bytes, 0, length);
                Buffer.BlockCopy(data, srcOffset, bytes, length, count);
                length += count;
            }
            else
            {
                Buffer.BlockCopy(data, srcOffset, bytes, length, count);
                length += count;
            }
        }

        
    }
}
