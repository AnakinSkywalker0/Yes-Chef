using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using YesChef.Controls;

namespace YesChef.Core
{
    /// <summary>
    /// Owns the round lifecycle: the countdown, pausing and game-over.
    /// <para>
    /// This is the only object allowed to touch <see cref="Time.timeScale"/>. Because every
    /// gameplay timer in the project is driven by <see cref="Time.deltaTime"/>, freezing the
    /// timescale pauses cooking, chopping and order timers for free.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Round")]
        [Tooltip("Length of a single round in seconds. The design calls for 3 minutes.")]
        [SerializeField, Min(1f)] private float _roundDuration = 180f;

        private float _timeRemaining;

        /// <summary>Raised on every state transition. Intended for UI.</summary>
        public event Action<GameState> OnStateChanged;

        /// <summary>Raised once when a round begins, before <see cref="OnStateChanged"/>.</summary>
        public event Action OnRoundStarted;

        /// <summary>Raised once when the timer expires, before <see cref="OnStateChanged"/>.</summary>
        public event Action OnRoundEnded;

        public GameState State { get; private set; } = GameState.WaitingToStart;
        public float RoundDuration => _roundDuration;
        public float TimeRemaining => _timeRemaining;
        public bool IsPlaying => State == GameState.Playing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _timeRemaining = _roundDuration;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            if (GameInput.Instance != null)
            {
                GameInput.Instance.OnPausePerformed += HandlePauseInput;
            }

            RaiseStateChanged();
        }

        private void OnDestroy()
        {
            if (GameInput.Instance != null)
            {
                GameInput.Instance.OnPausePerformed -= HandlePauseInput;
            }

            if (Instance == this)
            {
                Instance = null;
            }

            // Never leak a frozen clock into the next scene load.
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining > 0f)
            {
                return;
            }

            _timeRemaining = 0f;
            EndRound();
        }

        public void StartRound()
        {
            if (State != GameState.WaitingToStart)
            {
                return;
            }

            _timeRemaining = _roundDuration;
            Time.timeScale = 1f;
            State = GameState.Playing;

            OnRoundStarted?.Invoke();
            RaiseStateChanged();
        }

        public void TogglePause()
        {
            switch (State)
            {
                case GameState.Playing:
                    Pause();
                    break;
                case GameState.Paused:
                    Resume();
                    break;
            }
        }

        public void Pause()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            State = GameState.Paused;
            Time.timeScale = 0f;
            RaiseStateChanged();
        }

        public void Resume()
        {
            if (State != GameState.Paused)
            {
                return;
            }

            State = GameState.Playing;
            Time.timeScale = 1f;
            RaiseStateChanged();
        }

        /// <summary>Reloads the scene, returning the player to the controls screen.</summary>
        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EndRound()
        {
            State = GameState.GameOver;
            Time.timeScale = 1f;

            // Gameplay systems settle first (the score commits the high score here) so
            // that the UI reads final values when it reacts to the state change.
            OnRoundEnded?.Invoke();
            RaiseStateChanged();
        }

        private void HandlePauseInput() => TogglePause();

        private void RaiseStateChanged() => OnStateChanged?.Invoke(State);
    }
}
