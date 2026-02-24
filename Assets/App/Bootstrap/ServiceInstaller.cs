using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Core.Events;
using Miyo.Data;
using Miyo.Games;
using Miyo.Services.Auth;
using Miyo.Services.ChildProfile;
using Miyo.Services.DateTimePicker;
using Miyo.Services.Save;
using Miyo.Services.Statistics;
using Miyo.UI.MVVM;
using UnityEngine;

namespace Miyo.Bootstrap
{
    public class ServiceInstaller : MonoBehaviour
    {
        [SerializeField] private GameDatabase _gameDatabase;
        [SerializeField] private NavigationService _navigationService;
        [SerializeField] private GameLauncher _gameLauncher;

        private async void Awake()
        {
            ServiceLocator.Register<IEventBus>(new EventBus());
            RegisterGameDatabase();
            RegisterGameLauncher();
            RegisterDateTimePicker();

            // UGS Auth — Register immediately so other scripts can Get it, then initialize safely
            var authService = new AuthService();
            ServiceLocator.Register<IAuthService>(authService);
            
            await authService.InitializeAsync();

            // UGS Cloud Save — requires auth to be initialized
            var saveService = new SaveService();
            ServiceLocator.Register<ISaveService>(saveService);

            // ChildProfile depends on SaveService
            var childProfileService = new ChildProfileService(saveService);
            ServiceLocator.Register<IChildProfileService>(childProfileService);

            // Statistics depends on SaveService, ChildProfile, GameDatabase, EventBus
            ServiceLocator.Register<IGameStatisticService>(new GameStatisticService(
                saveService,
                childProfileService,
                _gameDatabase,
                ServiceLocator.Get<IEventBus>()
            ));

            Debug.Log("[ServiceInstaller] All services registered.");
        }

        private void RegisterGameDatabase()
        {
            if (_gameDatabase == null)
            {
                Debug.LogError("[ServiceInstaller] GameDatabase atanmamış! Inspector'dan GameDatabase asset'ini sürükleyin.");
                return;
            }
            ServiceLocator.Register<GameDatabase>(_gameDatabase);
        }

        private void RegisterGameLauncher()
        {
            if (_gameLauncher == null)
            {
                Debug.LogWarning("[ServiceInstaller] GameLauncher atanmamış.");
                return;
            }
            ServiceLocator.Register<IGameLauncher>(_gameLauncher);
        }

        private void RegisterDateTimePicker()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ServiceLocator.Register<INativeDateTimePicker>(
                new Infrastructure.DateTimePicker.AndroidDateTimePicker());
#elif UNITY_IOS && !UNITY_EDITOR
            ServiceLocator.Register<INativeDateTimePicker>(
                new Infrastructure.DateTimePicker.IOSDateTimePicker());
#else
            ServiceLocator.Register<INativeDateTimePicker>(
                new Infrastructure.DateTimePicker.EditorDateTimePicker());
#endif
        }
    }
}
