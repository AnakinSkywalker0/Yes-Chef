using UnityEngine;
using UnityEngine.UI;
using YesChef.Scoring;

namespace YesChef.UI
{
    /// <summary>
    /// Maps a score to one to three stars and lights a row of star images. Used for the best
    /// score on the menu and the round score on the results screen.
    /// </summary>
    [DisallowMultipleComponent]
    public class StarRatingUI : MonoBehaviour
    {
        public enum Source
        {
            HighScore,
            CurrentScore,
        }

        [SerializeField] private Source _source = Source.HighScore;
        [SerializeField] private Image[] _stars;
        [SerializeField, Tooltip("Score needed for the first, second and third star.")]
        private int[] _thresholds = { 100, 250, 400 };
        [SerializeField] private Color _lit = UiTheme.Butter;
        [SerializeField] private Color _unlit = UiTheme.StarOff;

        /// <summary>Stars earned (0-3) for <paramref name="score"/> given the thresholds.</summary>
        public static int StarsFor(int score, int[] thresholds)
        {
            int stars = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (score >= thresholds[i])
                {
                    stars = i + 1;
                }
            }

            return stars;
        }

        public int Stars { get; private set; }

        private bool _subscribed;

        private void OnEnable() => Subscribe();

        // The score manager may not exist yet when a screen first enables, so retry here.
        private void Start() => Subscribe();

        private void OnDisable()
        {
            if (_subscribed && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnHighScoreChanged -= HandleHighScoreChanged;
            }

            _subscribed = false;
        }

        private void Subscribe()
        {
            if (!_subscribed && ScoreManager.Instance != null && _source == Source.HighScore)
            {
                ScoreManager.Instance.OnHighScoreChanged += HandleHighScoreChanged;
                _subscribed = true;
            }

            Refresh();
        }

        private void HandleHighScoreChanged(int _) => Refresh();

        public void Refresh()
        {
            ScoreManager score = ScoreManager.Instance;
            int value = score == null ? 0 : _source == Source.HighScore ? score.HighScore : score.Score;
            Stars = StarsFor(value, _thresholds);

            for (int i = 0; _stars != null && i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    _stars[i].color = i < Stars ? _lit : _unlit;
                }
            }
        }
    }
}
