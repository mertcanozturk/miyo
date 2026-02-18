using System;
using Cysharp.Threading.Tasks;

namespace Miyo.UI.MVVM
{
    public interface IPopupService
    {
        bool HasActivePopup { get; }

        UniTask ShowPopup<TViewModel>(Action<TViewModel> configure = null)
            where TViewModel : ViewModelBase, new();

        UniTask<TResult> ShowPopup<TViewModel, TResult>(Action<TViewModel> configure = null)
            where TViewModel : PopupViewModel<TResult>, new();

        void CloseTop();
        void CloseAll();
    }
}
