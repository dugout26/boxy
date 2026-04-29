using System;
using System.Collections.Generic;

namespace Mound.Core.Pool
{
    public sealed class ObjectPool<T> where T : class
    {
        readonly Stack<T> available;
        readonly Func<T> factory;
        readonly Action<T> onGet;
        readonly Action<T> onRelease;
        readonly int maxSize;

        public int CountInactive => available.Count;

        public ObjectPool(Func<T> factory, int initialCapacity = 0, int maxSize = int.MaxValue,
                          Action<T> onGet = null, Action<T> onRelease = null)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.maxSize = maxSize > 0 ? maxSize : throw new ArgumentOutOfRangeException(nameof(maxSize));
            this.onGet = onGet;
            this.onRelease = onRelease;

            available = new Stack<T>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
            {
                available.Push(factory());
            }
        }

        public T Get()
        {
            var item = available.Count > 0 ? available.Pop() : factory();
            onGet?.Invoke(item);
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            onRelease?.Invoke(item);

            // maxSize 초과 시 GC가 회수하도록 풀에 안 넣음 — 메모리 무한 팽창 방지
            if (available.Count < maxSize) available.Push(item);
        }

        public void Clear()
        {
            available.Clear();
        }
    }
}
