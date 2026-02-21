using System;
using System.Collections.Generic;
using UnityEngine;

namespace Miyo.Data
{
    public struct GameStatistic
    {
        public GameDefinition game;
        public int playTimeMinutes;
        public int starsCount;
        public DateTime date;
    }


    public static class StatisticExtensions{

        public static int[] GetWeeklyPlayTime(GameStatistic[] statistics, DateTime today)
        {
            var fromDate = today.Date.AddDays(-7);
            int count = 0;
            for (int i = 0; i < statistics.Length; i++)
            {
                ref var s = ref statistics[i];
                if (s.date.Date >= fromDate) count++;
            }

            var result = new int[count];
            int idx = 0;
            for (int i = 0; i < statistics.Length; i++)
            {
                ref var s = ref statistics[i];
                if (s.date.Date >= fromDate)
                    result[idx++] = s.playTimeMinutes;
            }
            return result;
        }

        public static int[] GetMonthlyPlayTime(GameStatistic[] statistics, DateTime today)
        {
            var fromDate = today.Date.AddDays(-30);
            int count = 0;
            for (int i = 0; i < statistics.Length; i++)
            {
                ref var s = ref statistics[i];
                if (s.date.Date >= fromDate) count++;
            }

            var result = new int[count];
            int idx = 0;
            for (int i = 0; i < statistics.Length; i++)
            {
                ref var s = ref statistics[i];
                if (s.date.Date >= fromDate)
                    result[idx++] = s.playTimeMinutes;
            }
            return result;
        }


        public static (int playTime, GameDefinition game)[] GetDailyPlayTimeByGame(GameStatistic[] statistics, DateTime today)
        {
            return GroupByGame(statistics, today.Date, today.Date.AddDays(1));
        }

        public static (int playTime, GameDefinition game)[] GetWeeklyPlayTimeByGame(GameStatistic[] statistics, DateTime today)
        {
            return GroupByGame(statistics, today.Date.AddDays(-7), DateTime.MaxValue);
        }

        public static (int playTime, GameDefinition game)[] GetMonthlyPlayTimeByGame(GameStatistic[] statistics, DateTime today)
        {
            return GroupByGame(statistics, today.Date.AddDays(-30), DateTime.MaxValue);
        }

        // Groups statistics by game within [fromDate, toDate) and sums play times. O(n) time.
        private static (int playTime, GameDefinition game)[] GroupByGame(GameStatistic[] statistics, DateTime fromDate, DateTime toDate)
        {
            var grouped = new Dictionary<GameDefinition, int>(statistics.Length);
            for (int i = 0; i < statistics.Length; i++)
            {
                ref var s = ref statistics[i];
                var date = s.date.Date;
                if (date < fromDate || date >= toDate) continue;

                grouped.TryGetValue(s.game, out var existing);
                grouped[s.game] = existing + s.playTimeMinutes;
            }

            var result = new (int, GameDefinition)[grouped.Count];
            int idx = 0;
            foreach (var kv in grouped)
                result[idx++] = (kv.Value, kv.Key);
            return result;
        }
    }
}