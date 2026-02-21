using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Miyo.UI.MVVM
{
    public enum ViewAnimationMode
    {
        Code,
        Animator
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ViewBase<TViewModel> : MonoBehaviour where TViewModel : ViewModelBase
    {
        [Header("Animation")]
        [SerializeField] private ViewAnimationMode _animationMode = ViewAnimationMode.Code;
        [SerializeField] private Animator _animator;

        private static readonly int OpenHash = Animator.StringToHash("Open");
        private static readonly int CloseHash = Animator.StringToHash("Close");

        private CanvasGroup _canvasGroup;
        private TViewModel _viewModel;
        private CompositeDisposable _disposables;
        private bool _bound;

        public TViewModel ViewModel => _viewModel;
        protected CanvasGroup CanvasGroup => _canvasGroup;
        protected CompositeDisposable Disposables => _disposables;
        protected bool HasAnimator => _animationMode == ViewAnimationMode.Animator && _animator != null;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_animator == null)
                _animator = GetComponent<Animator>();
        }

        public void Bind(TViewModel viewModel)
        {
            if (_bound) Unbind();

            _viewModel = viewModel;
            _disposables = new CompositeDisposable();
            _bound = true;

            _viewModel.InitializeInternal();
            OnBind(_viewModel);
        }

        public void Unbind()
        {
            if (!_bound) return;
            _bound = false;

            OnUnbind();
            _disposables?.Dispose();
            _disposables = null;
            _viewModel?.Dispose();
            _viewModel = default;
        }

        protected abstract void OnBind(TViewModel vm);

        protected virtual void OnUnbind() { }

        public virtual async UniTask AnimateIn()
        {
            _canvasGroup.interactable = false;
            _viewModel?.OnAppearing();

            if (_animationMode == ViewAnimationMode.Animator && _animator != null)
            {
                await PlayAnimatorState(OpenHash);
            }
            else
            {
                _canvasGroup.alpha = 0f;
                await UniTask.Delay(500);
                await ViewAnimator.FadeIn(_canvasGroup);
            }

            _canvasGroup.interactable = true;
            _viewModel?.OnAppeared();
        }

        public virtual async UniTask AnimateOut()
        {
            _canvasGroup.interactable = false;
            _viewModel?.OnDisappearing();

            if (_animationMode == ViewAnimationMode.Animator && _animator != null)
            {
                await PlayAnimatorState(CloseHash);
            }
            else
            {
                await ViewAnimator.FadeOut(_canvasGroup);
            }

            _viewModel?.OnDisappeared();
        }

        private async UniTask PlayAnimatorState(int stateHash)
        {
            _animator.Play(stateHash, 0, 0f);
            // Wait one frame for the animator to start
            await UniTask.Yield();

            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == stateHash)
            {
                await UniTask.WaitWhile(() =>
                {
                    if (_animator == null) return false;
                    var info = _animator.GetCurrentAnimatorStateInfo(0);
                    return info.shortNameHash == stateHash && info.normalizedTime < 1f;
                });
            }
        }

        protected virtual void OnDestroy()
        {
            Unbind();
        }
    }
}
