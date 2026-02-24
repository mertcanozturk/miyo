using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Core.Events;
using Miyo.Data;
using Miyo.Services.ChildProfile;
using UnityEngine;

namespace Miyo.Games
{
    public class GameLauncher : MonoBehaviour, IGameLauncher
    {
        [SerializeField] private Transform _gameContainer;

        public bool IsGameActive { get; private set; }

        private GameObject _activeInstance;

        void Awake()
        {
            SubscribeToEvents();
        }

        async void SubscribeToEvents()
        {
            await UniTask.WaitUntil(() => ServiceLocator.Contains<IEventBus>());
            var eventbus = ServiceLocator.Get<IEventBus>();
            eventbus.Subscribe<GameSessionStartedEvent>(OnGameSessionStarted);
        }

        void OnGameSessionStarted(GameSessionStartedEvent eventData)
        {
            LaunchAsync(ServiceLocator.Get<GameDatabase>().GetGame(eventData.GameId)).Forget();
        }
        

        public async UniTask LaunchAsync(GameDefinition gameDefinition)
        {
            if (IsGameActive) return;
            IsGameActive = true;

            var childProfileService = ServiceLocator.Get<IChildProfileService>();

            var childProfile = await childProfileService.GetCurrentChildAsync();
            if (childProfile == null)
            {
                Debug.LogError("Child profile not found");
                return;
            }

            _activeInstance = Instantiate(gameDefinition.Game, _gameContainer);
            var game = _activeInstance.GetComponent<IGame>();

            var completionSource = new UniTaskCompletionSource();
            game.GameExited += () => completionSource.TrySetResult();

            game.StartGame(childProfile.Name);

            await completionSource.Task;

            Destroy(_activeInstance);
            _activeInstance = null;
            IsGameActive = false;
        }

    }
}
