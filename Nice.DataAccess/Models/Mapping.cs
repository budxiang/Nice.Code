using System;
using System.Collections;
using System.Collections.Generic;

namespace Nice.DataAccess.TypeEx
{
    /// <summary>
    /// 自定义轻量级Mapping
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class Mapping<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private int length;
        private int capacity;
        private int currIndex;
        private const int Incremental = 64;

        public int Length
        {
            get
            {
                return length;
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
            this.capacity = Incremental;
            items = new KeyValuePair<TKey, TValue>[capacity];
            this.length = capacity;
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
                KeyValuePair<TKey, TValue> item;
                for (int i = 0; i < length; i++)
                {
                    item = items[i];
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
            if (currIndex < length)
            {
                items[currIndex] = new KeyValuePair<TKey, TValue>(key, value);
                currIndex++;
            }
            else
            {
                this.length += Incremental;
                KeyValuePair<TKey, TValue>[] newitems = new KeyValuePair<TKey, TValue>[this.length];
                Array.Copy(items, newitems, items.Length);
                items = newitems;
                items[currIndex] = new KeyValuePair<TKey, TValue>(key, value);
                currIndex++;
            }

        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < length; i++)
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
