using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Data;
using Miyo.Services.Analytics;
using Miyo.Services.Auth;
using Miyo.Services.ChildProfile;
using Miyo.Services.Save;
using Miyo.UI.MVVM;

namespace Miyo.UI.Screens
{
    public class ChildProfileSelectViewModel : ViewModelBase
    {
        public ReactiveProperty<List<ChildProfile>> Children { get; } = new();
        public ReactiveProperty<ChildProfile> SelectedChild { get; } = new();

        protected override async void Initialize()
        {
            var authService = ServiceLocator.Get<IAuthService>();
            var profileService = ServiceLocator.Get<IChildProfileService>();

            var children = await profileService.GetChildrenForParentAsync(authService.PlayerId);
            Children.Value = children;
        }
        
        public async void SelectChild(string childId)
        {
            var child = Children.Value?.FirstOrDefault(c => c.Id == childId);
            if (child == null) return;

            SelectedChild.Value = child;
            // Navigate to child home screen
            // child session start
            var profileService = ServiceLocator.Get<IChildProfileService>();
            profileService.SetCurrentChild(childId);

            var nav = ServiceLocator.Get<INavigationService>();
            nav.NavigateTo<ChildHomeViewModel>().Forget();
        }

        public void OnParentPanelClicked(PinEntryConfig pinConfig)
        {
            var nav = ServiceLocator.Get<INavigationService>();
            nav.NavigateTo<PinEntryViewModel>(vm =>
                vm.Configure(pinConfig, () => nav.ClearAndNavigateTo<ParentHomeViewModel>())
            ).Forget();
        }
    }
}
