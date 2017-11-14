using System;
using System.Collections;
using System.Collections.Generic;

namespace Nice.Network.WebSockets
{
    public class Mapping<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private int nextIndex = 0;
        private const int Incremental = 4;

        public int Size
        {
            get
            {
                return nextIndex == 0 ? 0 : nextIndex - 1;
            }
        }

        private int capacity;
        public int Capacity
        {
            get
            {
                return capacity;
            }

            set
            {
                capacity = value;
            }
        }

        private KeyValuePair<TKey, TValue>[] items;
        public KeyValuePair<TKey, TValue>[] Items
        {
            get
            {
                return items;
            }

            set
            {
                items = value;
            }
        }

        public Mapping()
        {
            items = new KeyValuePair<TKey, TValue>[Incremental];
            this.capacity = Incremental;
        }
        public Mapping(int capacity)
        {
            items = new KeyValuePair<TKey, TValue>[capacity];
            this.capacity = capacity;
        }

        public TValue this[TKey key]
        {
            get
            {
                foreach (var item in items)
                {
                    if (item.Key == null) break;
                    if (item.Key.Equals(key))
                    {
                        return item.Value;
                    }
                }
                return default(TValue);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (nextIndex < capacity)
            {
                items[nextIndex] = new KeyValuePair<TKey, TValue>(key, value);
                nextIndex++;
            }
            else
            {
                this.capacity = (items.Length == 0) ? Incremental : (items.Length * 2);
                //this.capacity += Incremental;
                KeyValuePair<TKey, TValue>[] newitems = new KeyValuePair<TKey, TValue>[this.capacity];
                Array.Copy(items, 0, newitems, 0, items.Length);
                items = newitems;
                items[nextIndex] = new KeyValuePair<TKey, TValue>(key, value);
                nextIndex++;
            }
        }

        public TValue Get(TKey key, TValue defaultValue)
        {
            if (items.Length == 0) return defaultValue;
            foreach (var item in items)
            {
                if (item.Key == null) break;
                if (item.Key.Equals(key))
                {
                    defaultValue = item.Value;
                }
            }
            return defaultValue;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < this.Size; i++)
            {
                yield return items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
