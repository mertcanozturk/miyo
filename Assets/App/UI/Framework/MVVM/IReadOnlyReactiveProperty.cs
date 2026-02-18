using System;

namespace Miyo.UI.MVVM
{
    public interface IReadOnlyReactiveProperty<out T>
    {
        T Value { get; }
        IDisposable Subscribe(Action<T> callback, bool invokeImmediately = true);
    }
}
