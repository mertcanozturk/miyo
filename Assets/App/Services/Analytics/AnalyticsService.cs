using System;
using System.Collections.Generic;
using Miyo.Data;

namespace Miyo.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        public GameStatistic[] GetDummyGameStatistics(string childId, GameDefinition[] availableGames = null)
        {
            var seed = childId != null ? childId.GetHashCode() : 42;
            var rng = new Random(seed);

            var today = DateTime.Today;
            var results = new List<GameStatistic>();

            for (int dayOffset = 0; dayOffset < 30; dayOffset++)
            {
                var date = today.AddDays(-dayOffset);
                int sessions = rng.Next(1, 5);

                for (int i = 0; i < sessions; i++)
                {
                    GameDefinition game = null;
                    if (availableGames != null && availableGames.Length > 0)
                        game = availableGames[rng.Next(availableGames.Length)];

                    results.Add(new GameStatistic
                    {
                        game = game,
                        playTimeMinutes = rng.Next(5, 45),
                        starsCount = rng.Next(0, 4),
                        date = date.AddHours(rng.Next(8, 21))
                    });
                }
            }

            return results.ToArray();
        }
    }
}
