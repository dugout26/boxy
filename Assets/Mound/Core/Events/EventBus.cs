using System;
using System.Collections.Generic;

namespace Mound.Core.Events
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : struct;
        void Unsubscribe<T>(Action<T> handler) where T : struct;
        void Publish<T>(T evt) where T : struct;
    }

    public sealed class EventBus : IEventBus
    {
        readonly Dictionary<Type, Delegate> handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var type = typeof(T);
            if (handlers.TryGetValue(type, out var existing))
            {
                handlers[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                handlers[type] = handler;
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;

            var type = typeof(T);
            if (!handlers.TryGetValue(type, out var existing)) return;

            var remaining = Delegate.Remove(existing, handler);
            if (remaining == null)
            {
                handlers.Remove(type);
            }
            else
            {
                handlers[type] = remaining;
            }
        }

        public void Publish<T>(T evt) where T : struct
        {
            if (!handlers.TryGetValue(typeof(T), out var del)) return;

            // 핸들러 실행 중 Subscribe/Unsubscribe가 일어나도 안전하도록 호출 시점 스냅샷 사용
            if (del is Action<T> typed) typed.Invoke(evt);
        }
    }
}
