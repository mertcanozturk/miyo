using System.Collections.Generic;
using UnityEngine;

namespace Miyo.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName)
        {
            Debug.Log($"[Analytics] {eventName}");
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            Debug.Log($"[Analytics] {eventName}: {string.Join(", ", parameters)}");
        }

        public void LogGameStarted(string gameId)
        {
            LogEvent("game_started", new Dictionary<string, object>
            {
                { "game_id", gameId },
            });
        }

        public void LogGameCompleted(string gameId, int stars, float score, float duration)
        {
            LogEvent("game_completed", new Dictionary<string, object>
            {
                { "game_id", gameId },
                { "stars", stars },
                { "score", score },
                { "duration", duration }
            });
        }

        public void LogScreenView(string screenName)
        {
            LogEvent("screen_view", new Dictionary<string, object>
            {
                { "screen_name", screenName }
            });
        }

        public void SetUserId(string userId)
        {
            Debug.Log($"[Analytics] SetUserId: {userId}");
        }
    }
}
