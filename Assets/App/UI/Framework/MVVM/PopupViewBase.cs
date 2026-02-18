using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Miyo.UI.MVVM
{
    public abstract class PopupViewBase<TViewModel> : ViewBase<TViewModel> where TViewModel : ViewModelBase
    {
        public event Action OnCloseRequested;

        protected void RequestClose()
        {
            OnCloseRequested?.Invoke();
        }

        public override async UniTask AnimateIn()
        {
            CanvasGroup.interactable = false;
            ViewModel?.OnAppearing();

            // Animator mode is handled by base class
            if (HasAnimator)
            {
                await base.AnimateIn();
                return;
            }

            CanvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * 0.8f;
            await ViewAnimator.ScaleIn(transform, CanvasGroup);

            CanvasGroup.interactable = true;
            ViewModel?.OnAppeared();
        }

        public override async UniTask AnimateOut()
        {
            CanvasGroup.interactable = false;
            ViewModel?.OnDisappearing();

            if (HasAnimator)
            {
                await base.AnimateOut();
                return;
            }

            await ViewAnimator.ScaleOut(transform, CanvasGroup);

            ViewModel?.OnDisappeared();
        }
    }
}
