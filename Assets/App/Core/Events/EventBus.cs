using System;
using System.Collections.Generic;

namespace Miyo.Core.Events
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var handlers))
                return;

            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                if (handlers[i] is Action<T> handler)
                {
                    try
                    {
                        handler.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                }
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var handlers))
            {
                handlers = new List<Delegate>();
                _handlers[type] = handlers;
            }

            handlers.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
