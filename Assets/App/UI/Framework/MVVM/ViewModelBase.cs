using System;
using Miyo.Core;
using Miyo.Core.Events;

namespace Miyo.UI.MVVM
{
    public abstract class ViewModelBase : IDisposable
    {
        protected CompositeDisposable Disposables { get; } = new();

        private IEventBus _eventBus;
        protected IEventBus EventBus => _eventBus ??= ServiceLocator.Get<IEventBus>();

        private bool _initialized;
        private bool _disposed;

        public void InitializeInternal()
        {
            if (_initialized) return;
            _initialized = true;
            Initialize();
        }

        protected virtual void Initialize() { }

        public virtual void OnAppearing() { }

        public virtual void OnAppeared() { }

        public virtual void OnDisappearing() { }

        public virtual void OnDisappeared() { }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            OnDispose();
            Disposables.Dispose();
        }

        protected virtual void OnDispose() { }
    }
}
