using System;
using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Core.Events;
using Miyo.Services.Save;
using UnityEngine;

namespace Miyo.Games
{
    public abstract class GameBase<TSaveData> : MonoBehaviour, IGame
        where TSaveData : class, new()
    {
        public event Action GameExited;
        public enum GameState
        {
            None,
            Initializing,
            Playing,
            Exiting
        }

        protected ISaveService SaveService { get; private set; }
        protected IEventBus EventBus { get; private set; }
        protected TSaveData SaveData { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.None;
        public bool IsPlaying => CurrentState == GameState.Playing;
        public float ElapsedTime { get; private set; }

        protected abstract string GameId { get; }

        private float _lastTimeRecord;
        private float _sessionStartTime;
        private int _levelsCompleted;
        private bool _startGuard;

        // ── Public API ───────────────────────────────────────────────

        public async void StartGame(string childName)
        {
            if (_startGuard) return;
            _startGuard = true;

            try
            {
                CurrentState = GameState.Initializing;

                SaveService = ServiceLocator.Get<ISaveService>();
                EventBus = ServiceLocator.Get<IEventBus>();

                SaveData = await SaveService.LoadAsync<TSaveData>(GameId, new TSaveData());

                OnInitialize(childName, SaveData);

                CurrentState = GameState.Playing;
                _sessionStartTime = Time.realtimeSinceStartup;
                _levelsCompleted = 0;
                ResetTimer();

                EventBus.Publish(new GameSessionStartedEvent { GameId = GameId });

                OnGameStart();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _startGuard = false;
            }
        }

        public async void ExitGame()
        {
            if (CurrentState == GameState.Exiting) return;
            CurrentState = GameState.Exiting;

            PublishSessionEnded();

            if (SaveData != null && SaveService != null)
                await SaveService.SaveAsync(GameId, SaveData);

            OnCleanup();
            _startGuard = false;
            OnExit();
            GameExited?.Invoke();
        }

        // ── Protected API ────────────────────────────────────────────

        protected async UniTask CompleteLevel(int levelIndex, int stars, float score,
            int correctAnswers, int totalQuestions)
        {
            if (CurrentState == GameState.Exiting) return;

            EventBus.Publish(new LevelCompletedEvent
            {
                GameId = GameId,
                LevelIndex = levelIndex,
                Stars = stars,
                Score = score,
                DurationSeconds = ElapsedTime,
                CorrectAnswers = correctAnswers,
                TotalQuestions = totalQuestions
            });

            _levelsCompleted++;

            await SaveService.SaveAsync(GameId, SaveData);

            ResetTimer();
        }

        private void PublishSessionEnded()
        {
            if (EventBus == null) return;

            EventBus.Publish(new GameSessionEndedEvent
            {
                GameId = GameId,
                SessionDurationSeconds = Time.realtimeSinceStartup - _sessionStartTime,
                LevelsCompleted = _levelsCompleted
            });
        }

        protected UniTask SaveProgress()
        {
            return SaveService.SaveAsync(GameId, SaveData);
        }

        protected void ResetTimer()
        {
            ElapsedTime = 0f;
            _lastTimeRecord = Time.time;
        }

        // ── Template Methods ─────────────────────────────────────────

        protected abstract void OnInitialize(string childName, TSaveData saveData);
        protected abstract void OnGameStart();
        protected virtual void OnCleanup() { }
        protected virtual void OnExit() { }

        // ── Unity Lifecycle ──────────────────────────────────────────

        protected virtual void Update()
        {
            if (CurrentState != GameState.Playing) return;

            float now = Time.time;
            ElapsedTime += now - _lastTimeRecord;
            _lastTimeRecord = now;
        }

        protected virtual void OnDestroy()
        {
            if (CurrentState != GameState.None &&
                CurrentState != GameState.Exiting)
            {
                PublishSessionEnded();
                OnCleanup();
            }
        }
    }
}
