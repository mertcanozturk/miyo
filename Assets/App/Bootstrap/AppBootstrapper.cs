using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Services.Auth;
using Miyo.Services.ChildProfile;
using Miyo.UI;
using Miyo.UI.MVVM;
using Miyo.UI.Screens;
using UnityEngine;

namespace Miyo.Bootstrap
{
    public class AppBootstrapper : MonoBehaviour
    {
        private async void Start()
        {
            // Wait until ServiceInstaller's async Awake finishes registering the LAST service
            await UniTask.WaitUntil(() => ServiceLocator.Contains<IChildProfileService>());

            var authService = ServiceLocator.Get<IAuthService>();
            var nav = ServiceLocator.Get<INavigationService>();

            if (authService.IsLoggedIn)
            {
                await AuthNavigationHelper.NavigateAfterAuth(authService.PlayerId, nav);
            }
            else
            {
                await nav.ClearAndNavigateTo<ParentRegisterViewModel>();
            }
        }
    }
}
