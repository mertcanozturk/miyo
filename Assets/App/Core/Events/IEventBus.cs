using System;

namespace Miyo.Core.Events
{
    public interface IEventBus
    {
        void Publish<T>(T eventData) where T : struct;
        void Subscribe<T>(Action<T> handler) where T : struct;
        void Unsubscribe<T>(Action<T> handler) where T : struct;
        void Clear();
    }
}
