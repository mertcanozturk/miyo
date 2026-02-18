using System;
using System.Collections.Generic;

namespace Miyo.UI.MVVM
{
    public class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables = new();
        private bool _disposed;

        public int Count => _disposables.Count;

        public void Add(IDisposable disposable)
        {
            if (_disposed)
            {
                disposable?.Dispose();
                return;
            }
            _disposables.Add(disposable);
        }

        public void Remove(IDisposable disposable)
        {
            _disposables.Remove(disposable);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            for (int i = _disposables.Count - 1; i >= 0; i--)
                _disposables[i]?.Dispose();

            _disposables.Clear();
        }

        public void Clear()
        {
            for (int i = _disposables.Count - 1; i >= 0; i--)
                _disposables[i]?.Dispose();

            _disposables.Clear();
        }
    }

    public static class DisposableExtensions
    {
        public static T AddTo<T>(this T disposable, CompositeDisposable composite) where T : IDisposable
        {
            composite.Add(disposable);
            return disposable;
        }
    }
}
