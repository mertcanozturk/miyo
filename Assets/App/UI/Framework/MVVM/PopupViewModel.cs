using Cysharp.Threading.Tasks;

namespace Miyo.UI.MVVM
{
    public abstract class PopupViewModel<TResult> : ViewModelBase
    {
        private readonly UniTaskCompletionSource<TResult> _completionSource = new();

        public UniTask<TResult> Result => _completionSource.Task;

        protected void SetResult(TResult result)
        {
            _completionSource.TrySetResult(result);
        }

        protected void Cancel()
        {
            _completionSource.TrySetCanceled();
        }

        protected override void OnDispose()
        {
            _completionSource.TrySetCanceled();
        }
    }
}
