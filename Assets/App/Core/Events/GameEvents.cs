namespace Miyo.Core.Events
{
    public struct GameStartedEvent
    {
        public string GameId;
    }

    public struct GameCompletedEvent
    {
        public string GameId;
        public int Stars;
        public float Score;
        public int DifficultyLevel;
        public float DurationSeconds;
        public int CorrectAnswers;
        public int TotalQuestions;
    }

    public struct GamePausedEvent
    {
        public string GameId;
    }

    public struct GameResumedEvent
    {
        public string GameId;
    }

    public struct CorrectAnswerEvent
    {
        public string GameId;
        public int QuestionIndex;
    }

    public struct WrongAnswerEvent
    {
        public string GameId;
        public int QuestionIndex;
    }

    public struct ProfileSelectedEvent
    {
        public string ProfileId;
    }

    public struct ProfileCreatedEvent
    {
        public string ProfileId;
        public string DisplayName;
    }

    public struct ScreenTimeLimitReachedEvent { }

    public struct ScreenTimeWarningEvent
    {
        public float MinutesRemaining;
    }

    public struct ConnectivityChangedEvent
    {
        public bool IsOnline;
    }

    public struct SubscriptionStatusChangedEvent
    {
        public bool IsActive;
        public bool IsTrial;
    }

    public struct SceneTransitionRequestedEvent
    {
        public string TargetScene;
    }
}
