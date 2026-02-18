using System;
using UnityEngine;

namespace Miyo.Core.Utilities
{
    public class Timer
    {
        public float Duration { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public float RemainingSeconds => Mathf.Max(0, Duration - ElapsedSeconds);
        public float NormalizedProgress => Duration > 0 ? Mathf.Clamp01(ElapsedSeconds / Duration) : 0;
        public bool IsRunning { get; private set; }
        public bool IsCompleted => ElapsedSeconds >= Duration && Duration > 0;

        public event Action OnCompleted;
        public event Action<float> OnTick;

        public Timer(float duration = 0f)
        {
            Duration = duration;
        }

        public void Start(float duration = -1f)
        {
            if (duration >= 0)
                Duration = duration;

            ElapsedSeconds = 0f;
            IsRunning = true;
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public void Resume()
        {
            if (!IsCompleted)
                IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
            ElapsedSeconds = 0f;
        }

        public void Update(float deltaTime)
        {
            if (!IsRunning)
                return;

            ElapsedSeconds += deltaTime;
            OnTick?.Invoke(ElapsedSeconds);

            if (Duration > 0 && ElapsedSeconds >= Duration)
            {
                IsRunning = false;
                OnCompleted?.Invoke();
            }
        }
    }
}
