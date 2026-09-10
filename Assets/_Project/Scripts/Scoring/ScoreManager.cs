using System;
using UnityEngine;
using YesChef.Core;
using YesChef.Orders;
using YesChef.Stations;

namespace YesChef.Scoring
{
    /// <summary>
    /// Tallies the round score and owns the persisted high score.
    /// <para>
    /// The high score is committed when the round ends rather than during play, so a run
    /// that peaks mid-round and then loses points to slow orders is recorded honestly.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [SerializeField] private OrderBoard _orderBoard;

        private IHighScoreRepository _repository;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        public int Score { get; private set; }
        public int HighScore { get; private set; }

        /// <summary>True when the round that just ended beat the stored high score.</summary>
        public bool HasNewHighScore { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _repository = new PlayerPrefsHighScoreRepository();
            HighScore = _repository.Load();
        }

        private void Start()
        {
            if (_orderBoard != null)
            {
                _orderBoard.OnOrderScored += HandleOrderScored;
            }

            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.OnRoundStarted += HandleRoundStarted;
            GameManager.Instance.OnRoundEnded += HandleRoundEnded;
        }

        private void OnDestroy()
        {
            if (_orderBoard != null)
            {
                _orderBoard.OnOrderScored -= HandleOrderScored;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
                GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void HandleOrderScored(CustomerWindowStation window, int points) => AddScore(points);

        private void HandleRoundStarted()
        {
            HasNewHighScore = false;
            Score = 0;
            OnScoreChanged?.Invoke(Score);
        }

        private void HandleRoundEnded()
        {
            if (Score <= HighScore)
            {
                return;
            }

            HighScore = Score;
            HasNewHighScore = true;
            _repository.Save(HighScore);
            OnHighScoreChanged?.Invoke(HighScore);
        }

        private void AddScore(int points)
        {
            if (points == 0)
            {
                return;
            }

            Score += points;
            OnScoreChanged?.Invoke(Score);
        }
    }
}
