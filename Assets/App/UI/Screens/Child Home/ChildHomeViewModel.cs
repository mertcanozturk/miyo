using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Data;
using Miyo.Services.ChildProfile;
using Miyo.UI.MVVM;
using UnityEngine;

namespace Miyo.UI
{
    public class ChildHomeViewModel : ViewModelBase
    {
        public ReactiveProperty<ChildProfile> CurrentChild { get; } = new();

        protected override async void Initialize()
        {
            var profileService = ServiceLocator.Get<IChildProfileService>();
            CurrentChild.Value = await profileService.GetCurrentChildAsync();
        }
    }
}
