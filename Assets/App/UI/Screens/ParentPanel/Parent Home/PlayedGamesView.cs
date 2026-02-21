using UnityEngine;
using Miyo.Data;
using System.Collections.Generic;
using System;

namespace Miyo.UI
{
    public class PlayedGamesView : MonoBehaviour
    {
        [SerializeField] private UICollection<GameInfoUI> _valueElements;

        
        public enum Mode {
            Daily,
            Weekly,
            Monthly
        }
        
        public void Prepare(List<GameStatistic> gameStatistics, Mode mode)
        {
            var statistics = mode switch
            {
                Mode.Daily => StatisticExtensions.GetDailyPlayTimeByGame(gameStatistics.ToArray(), DateTime.Today),
                Mode.Weekly => StatisticExtensions.GetWeeklyPlayTimeByGame(gameStatistics.ToArray(), DateTime.Today),
                Mode.Monthly => StatisticExtensions.GetMonthlyPlayTimeByGame(gameStatistics.ToArray(), DateTime.Today),
                _ => throw new ArgumentException("Invalid mode"),
            };

            _valueElements.Count = statistics.Length;
            for (int i = 0; i < statistics.Length; i++)
            {
                var minutes = statistics[i].Item1;
                var game = statistics[i].Item2;
                //TODO: Calculate star count
                int starCount = 3;
                _valueElements[i].Prepare(game, minutes, starCount);
            }
        }
    }
}
