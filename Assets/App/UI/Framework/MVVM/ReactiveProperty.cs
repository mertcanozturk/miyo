using System;
using System.Collections.Generic;

namespace Miyo.UI.MVVM
{
    public class ReactiveProperty<T> : IReadOnlyReactiveProperty<T>, IDisposable
    {
        private T _value;
        private readonly List<Action<T>> _subscribers = new();
        private readonly EqualityComparer<T> _comparer = EqualityComparer<T>.Default;
        private bool _disposed;

        public ReactiveProperty() { }

        public ReactiveProperty(T initialValue)
        {
            _value = initialValue;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (_comparer.Equals(_value, value)) return;
                _value = value;
                NotifySubscribers();
            }
        }

        public void SetValueWithoutNotify(T value)
        {
            _value = value;
        }

        public void ForceNotify()
        {
            NotifySubscribers();
        }

        public IDisposable Subscribe(Action<T> callback, bool invokeImmediately = true)
        {
            if (_disposed) return Disposable.Empty;

            _subscribers.Add(callback);

            if (invokeImmediately)
                callback(_value);

            return new Disposable(() => _subscribers.Remove(callback));
        }

        private void NotifySubscribers()
        {
            for (int i = _subscribers.Count - 1; i >= 0; i--)
            {
                if (i < _subscribers.Count)
                    _subscribers[i]?.Invoke(_value);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _subscribers.Clear();
        }

        public static implicit operator T(ReactiveProperty<T> property) => property.Value;

        public override string ToString() => _value?.ToString() ?? "null";
    }

    public sealed class Disposable : IDisposable
    {
        public static readonly IDisposable Empty = new Disposable(null);

        private Action _disposeAction;

        public Disposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction?.Invoke();
            _disposeAction = null;
        }
    }
}
