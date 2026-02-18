using System.Collections.Generic;

namespace Miyo.Services.Analytics
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventName);
        void LogEvent(string eventName, Dictionary<string, object> parameters);
        void LogGameStarted(string gameId);
        void LogGameCompleted(string gameId, int stars, float score, float duration);
        void LogScreenView(string screenName);
        void SetUserId(string userId);
    }
}
