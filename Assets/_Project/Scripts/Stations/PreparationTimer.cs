using System;
using UnityEngine;

namespace YesChef.Stations
{
    /// <summary>
    /// A single timed preparation job: chopping on a table, cooking on a burner.
    /// <para>
    /// Plain C# with no Unity lifecycle, so the two stations share one implementation
    /// instead of each carrying their own copy of the same countdown, and the rules
    /// (progress is 0..1 while running, 0 when idle, completion fires exactly once) are
    /// covered by unit tests rather than by inspection.
    /// </para>
    /// </summary>
    public sealed class PreparationTimer
    {
        private float _elapsed;

        public PreparationTimer(float duration)
        {
            if (duration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "A preparation job must take some time.");
            }

            Duration = duration;
        }

        /// <summary>Raised whenever <see cref="IsRunning"/> or <see cref="Progress"/> changes.</summary>
        public event Action OnChanged;

        public float Duration { get; }

        public bool IsRunning { get; private set; }

        /// <summary>Normalised completion in [0, 1]. Always 0 while idle.</summary>
        public float Progress { get; private set; }

        public void Start()
        {
            _elapsed = 0f;
            IsRunning = true;
            Progress = 0f;
            OnChanged?.Invoke();
        }

        public void Cancel()
        {
            if (!IsRunning)
            {
                return;
            }

            IsRunning = false;
            Progress = 0f;
            OnChanged?.Invoke();
        }

        /// <summary>Advances the job. Returns true on the tick that completes it.</summary>
        public bool Tick(float deltaTime)
        {
            if (!IsRunning)
            {
                return false;
            }

            _elapsed += Mathf.Max(0f, deltaTime);

            if (_elapsed < Duration)
            {
                Progress = Mathf.Clamp01(_elapsed / Duration);
                OnChanged?.Invoke();
                return false;
            }

            IsRunning = false;
            Progress = 0f;
            OnChanged?.Invoke();
            return true;
        }
    }
}
