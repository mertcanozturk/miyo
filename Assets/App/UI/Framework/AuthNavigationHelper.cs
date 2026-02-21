using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Services.ChildProfile;
using Miyo.UI.MVVM;
using Miyo.UI.Screens;

namespace Miyo.UI
{
    public static class AuthNavigationHelper
    {
        public static async UniTask NavigateAfterAuth(string playerId, INavigationService nav)
        {
            var profileService = ServiceLocator.Get<IChildProfileService>();
            var children = await profileService.GetChildrenForParentAsync(playerId);

            if (children.Count > 0)
                await nav.ClearAndNavigateTo<ChildProfileSelectViewModel>();
            else
                await nav.ClearAndNavigateTo<CreateChildViewModel>();
        }
    }
}
