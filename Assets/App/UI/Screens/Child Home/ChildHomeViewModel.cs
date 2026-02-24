using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Core.Events;
using Miyo.Data;
using Miyo.Services.ChildProfile;
using Miyo.UI.MVVM;
using UnityEngine;

namespace Miyo.UI
{
    public class ChildHomeViewModel : ViewModelBase
    {
        public ReactiveProperty<ChildProfile> CurrentChild { get; } = new();
        public ReactiveProperty<GameDefinition[]> Games { get; } = new();

        protected override async void Initialize()
        {
            var profileService = ServiceLocator.Get<IChildProfileService>();
            CurrentChild.Value = await profileService.GetCurrentChildAsync();
            Games.Value = ServiceLocator.Get<GameDatabase>().Games;
        }

        public void OnGamePlayButtonClicked(GameDefinition game)
        {
            var eventbus = ServiceLocator.Get<IEventBus>();
            eventbus.Publish(new GameSessionStartedEvent { GameId = game.GameName });
        }
    }
}
