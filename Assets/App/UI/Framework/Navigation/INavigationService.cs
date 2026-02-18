using System;
using Cysharp.Threading.Tasks;

namespace Miyo.UI.MVVM
{
    public interface INavigationService
    {
        string CurrentScreenId { get; }
        bool CanGoBack { get; }
        bool IsTransitioning { get; }

        // ViewModel tipinden ID otomatik türetilir: ParentLoginViewModel → "parent-login"
        UniTask NavigateTo<TViewModel>(Action<TViewModel> configure = null)
            where TViewModel : ViewModelBase, new();

        UniTask GoBack();

        UniTask ClearAndNavigateTo<TViewModel>(Action<TViewModel> configure = null)
            where TViewModel : ViewModelBase, new();
    }
}
