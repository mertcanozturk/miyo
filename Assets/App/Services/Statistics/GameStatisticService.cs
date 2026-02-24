using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Miyo.Core;
using Miyo.Core.Events;
using Miyo.Data;
using Miyo.Services.ChildProfile;
using Miyo.Services.Save;
using UnityEngine;

namespace Miyo.Services.Statistics
{
    public class GameStatisticService : IGameStatisticService
    {
        private readonly ISaveService _saveService;
        private readonly IChildProfileService _childProfileService;
        private readonly GameDatabase _gameDatabase;

        private int _sessionStars;

        public GameStatisticService(
            ISaveService saveService,
            IChildProfileService childProfileService,
            GameDatabase gameDatabase,
            IEventBus eventBus)
        {
            _saveService = saveService;
            _childProfileService = childProfileService;
            _gameDatabase = gameDatabase;

            eventBus.Subscribe<GameSessionStartedEvent>(OnSessionStarted);
            eventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            eventBus.Subscribe<GameSessionEndedEvent>(OnSessionEnded);
        }

        private void OnSessionStarted(GameSessionStartedEvent e)
        {
            _sessionStars = 0;
        }

        private void OnLevelCompleted(LevelCompletedEvent e)
        {
            _sessionStars += e.Stars;
        }

        private async void OnSessionEnded(GameSessionEndedEvent e)
        {
            try
            {
                var child = await _childProfileService.GetCurrentChildAsync();
                if (child == null) return;

                var key = GetSaveKey(child.Id);
                var data = await _saveService.LoadAsync(key, new StatisticsSaveData());

                data.entries.Add(new StatisticEntry
                {
                    gameId = e.GameId,
                    playTimeMinutes = Mathf.RoundToInt(e.SessionDurationSeconds / 60f),
                    starsCount = _sessionStars,
                    dateTicks = DateTime.UtcNow.Ticks
                });

                _sessionStars = 0;

                await _saveService.SaveAsync(key, data);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public async UniTask<GameStatistic[]> GetStatisticsAsync()
        {
            var child = await _childProfileService.GetCurrentChildAsync();
            if (child == null) return Array.Empty<GameStatistic>();

            return await GetStatisticsForChildAsync(child.Id);
        }

        public async UniTask<GameStatistic[]> GetStatisticsForChildAsync(string childId)
        {
            var key = GetSaveKey(childId);
            var data = await _saveService.LoadAsync(key, new StatisticsSaveData());

            var result = new GameStatistic[data.entries.Count];
            for (int i = 0; i < data.entries.Count; i++)
            {
                var entry = data.entries[i];
                result[i] = new GameStatistic
                {
                    game = _gameDatabase.GetGame(entry.gameId),
                    playTimeMinutes = entry.playTimeMinutes,
                    starsCount = entry.starsCount,
                    date = new DateTime(entry.dateTicks, DateTimeKind.Utc)
                };
            }

            return result;
        }

        private static string GetSaveKey(string childId) => $"statistics_{childId}";

        // ── Serializable Save Data ──────────────────────────────────

        [Serializable]
        private class StatisticsSaveData
        {
            public List<StatisticEntry> entries = new();
        }

        [Serializable]
        private class StatisticEntry
        {
            public string gameId;
            public int playTimeMinutes;
            public int starsCount;
            public long dateTicks;
        }
    }
}
