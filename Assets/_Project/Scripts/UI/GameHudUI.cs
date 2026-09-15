using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Scoring;

namespace YesChef.UI
{
    /// <summary>Always-on readouts: score, combo multiplier, high score, remaining time, pause and quit.</summary>
    [DisallowMultipleComponent]
    public class GameHudUI : MonoBehaviour
    {
        [SerializeField] private GameObject _hudRoot;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _highScoreText;
        [SerializeField] private TMP_Text _multiplierText;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _quitButton;

        [Header("Combo")]
        [SerializeField] private Color _idleMultiplierColor = new(1f, 1f, 1f, 0.45f);
        [SerializeField] private Color _activeMultiplierColor = new(1f, 0.85f, 0.35f);
        [SerializeField] private Color _maxMultiplierColor = new(1f, 0.55f, 0.25f);

        [Header("Timer urgency")]
        [Tooltip("Seconds remaining at which the clock turns urgent.")]
        [SerializeField, Min(0f)] private float _urgentThreshold = 30f;
        [SerializeField] private Color _timerColor = Color.white;
        [SerializeField] private Color _urgentTimerColor = new(1f, 0.42f, 0.38f);

        private int _lastShownSeconds = -1;

        private void Start()
        {
            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(HandlePauseClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(HandleQuitClicked);
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
                ScoreManager.Instance.OnHighScoreChanged += HandleHighScoreChanged;
                ScoreManager.Instance.OnMultiplierChanged += HandleMultiplierChanged;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            RefreshScoreTexts();
            RefreshMultiplierText();
            RefreshVisibility();
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
                ScoreManager.Instance.OnHighScoreChanged -= HandleHighScoreChanged;
                ScoreManager.Instance.OnMultiplierChanged -= HandleMultiplierChanged;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || _timerText == null)
            {
                return;
            }

            float remaining = GameManager.Instance.TimeRemaining;
            int seconds = Mathf.CeilToInt(remaining);
            if (seconds == _lastShownSeconds)
            {
                return;
            }

            _lastShownSeconds = seconds;
            _timerText.text = $"{seconds / 60:0}:{seconds % 60:00}";
            _timerText.color = remaining <= _urgentThreshold ? _urgentTimerColor : _timerColor;
        }

        private void HandleScoreChanged(int score) => RefreshScoreTexts();

        private void HandleHighScoreChanged(int highScore) => RefreshScoreTexts();

        private void HandleMultiplierChanged(int multiplier) => RefreshMultiplierText();

        private void HandleStateChanged(GameState state) => RefreshVisibility();

        private void RefreshScoreTexts()
        {
            if (ScoreManager.Instance == null)
            {
                return;
            }

            if (_scoreText != null)
            {
                _scoreText.text = ScoreManager.Instance.Score.ToString();
            }

            if (_highScoreText != null)
            {
                _highScoreText.text = ScoreManager.Instance.HighScore.ToString();
            }
        }

        private void RefreshMultiplierText()
        {
            if (_multiplierText == null || ScoreManager.Instance == null)
            {
                return;
            }

            int multiplier = ScoreManager.Instance.Multiplier;
            _multiplierText.text = $"x{multiplier}";

            if (multiplier <= ComboTracker.MinMultiplier)
            {
                _multiplierText.color = _idleMultiplierColor;
            }
            else if (multiplier >= ScoreManager.Instance.MaxMultiplier)
            {
                _multiplierText.color = _maxMultiplierColor;
            }
            else
            {
                _multiplierText.color = _activeMultiplierColor;
            }
        }

        private void RefreshVisibility()
        {
            if (_hudRoot == null || GameManager.Instance == null)
            {
                return;
            }

            GameState state = GameManager.Instance.State;
            _hudRoot.SetActive(state is GameState.Playing or GameState.Paused);
        }

        private void HandlePauseClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Pause();
            }
        }

        private void HandleQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
    }
}
