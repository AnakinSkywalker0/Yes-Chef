using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Scoring;

namespace YesChef.UI
{
    /// <summary>
    /// End-of-round summary: what the player scored, the high score, and an acknowledgement
    /// when they beat it. The restart button resets the game.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameOverScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject _screenRoot;
        [SerializeField] private TMP_Text _finalScoreText;
        [SerializeField] private TMP_Text _highScoreText;
        [SerializeField] private GameObject _newHighScoreBanner;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(HandleRestartClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(HandleQuitClicked);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            RefreshVisibility();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState state) => RefreshVisibility();

        private void RefreshVisibility()
        {
            if (_screenRoot == null || GameManager.Instance == null)
            {
                return;
            }

            bool isGameOver = GameManager.Instance.State == GameState.GameOver;
            _screenRoot.SetActive(isGameOver);

            if (isGameOver)
            {
                RefreshResults();
            }
        }

        /// <summary>
        /// Safe to read here: the score commits its high score on the round-ended event,
        /// which <see cref="GameManager"/> raises before the state change reaches the UI.
        /// </summary>
        private void RefreshResults()
        {
            ScoreManager score = ScoreManager.Instance;
            if (score == null)
            {
                return;
            }

            if (_finalScoreText != null)
            {
                _finalScoreText.text = score.Score.ToString();
            }

            if (_highScoreText != null)
            {
                _highScoreText.text = score.HighScore.ToString();
            }

            if (_newHighScoreBanner != null)
            {
                _newHighScoreBanner.SetActive(score.HasNewHighScore);
            }
        }

        private void HandleRestartClicked() => GameManager.Instance?.Restart();

        private void HandleQuitClicked() => GameManager.Instance?.QuitGame();
    }
}
