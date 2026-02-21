using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Data;
using Miyo.Services.Analytics;
using Miyo.Services.Auth;
using Miyo.Services.ChildProfile;
using Miyo.Services.DateTimePicker;
using Miyo.Services.Save;
using Miyo.UI.MVVM;
using UnityEngine;

namespace Miyo.Bootstrap
{
    public class ServiceInstaller : MonoBehaviour
    {
        [SerializeField] private GameDatabase _gameDatabase;
        [SerializeField] private NavigationService _navigationService;

        private async void Awake()
        {
            RegisterGameDatabase();
            RegisterNavigationService();
            RegisterAnalyticsService();
            RegisterDateTimePicker();

            // UGS Auth — Register immediately so other scripts can Get it, then initialize safely
            var authService = new AuthService();
            ServiceLocator.Register<IAuthService>(authService);
            
            await authService.InitializeAsync();

            // UGS Cloud Save — requires auth to be initialized
            var saveService = new SaveService();
            ServiceLocator.Register<ISaveService>(saveService);

            // ChildProfile depends on SaveService
            ServiceLocator.Register<IChildProfileService>(new ChildProfileService(saveService));

            Debug.Log("[ServiceInstaller] All services registered.");
        }

        private void RegisterNavigationService()
        {
            ServiceLocator.Register<INavigationService>(_navigationService);
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

        private void RegisterAnalyticsService()
        {
            ServiceLocator.Register<IAnalyticsService>(new AnalyticsService());
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
