using System;
using UnityEngine;
using YesChef.Core;
using YesChef.Orders;
using YesChef.Stations;

namespace YesChef.Scoring
{
    /// <summary>
    /// Tallies the round score, runs the combo multiplier and owns the persisted high score.
    /// <para>
    /// Windows report the raw value of a completed order; this is the one place that turns it
    /// into points, so the multiplier is applied exactly once and every listener (HUD, popup,
    /// camera nudge) sees the same settled <see cref="ScoreAward"/>.
    /// </para>
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

        [Header("Combo")]
        [Tooltip("Highest multiplier a streak of on-time deliveries can reach.")]
        [SerializeField, Min(ComboTracker.MinMultiplier)] private int _maxMultiplier = 3;

        private IHighScoreRepository _repository;
        private ComboTracker _combo;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        /// <summary>Raised whenever the multiplier steps up or the streak breaks.</summary>
        public event Action<int> OnMultiplierChanged;

        /// <summary>Raised once per completed order with the points that actually landed.</summary>
        public event Action<CustomerWindowStation, ScoreAward> OnOrderAwarded;

        public int Score { get; private set; }
        public int HighScore { get; private set; }

        /// <summary>Multiplier the next on-time delivery will receive.</summary>
        public int Multiplier => Combo.Multiplier;

        /// <summary>Consecutive on-time deliveries so far.</summary>
        public int Streak => Combo.Streak;

        public int MaxMultiplier => Combo.MaxMultiplier;

        /// <summary>True when the round that just ended beat the stored high score.</summary>
        public bool HasNewHighScore { get; private set; }

        private ComboTracker Combo => _combo ??= new ComboTracker(_maxMultiplier);

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
                _orderBoard.OnOrderCompleted += HandleOrderCompleted;
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
                _orderBoard.OnOrderCompleted -= HandleOrderCompleted;
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

        private void HandleOrderCompleted(CustomerWindowStation window, Order order, int rawPoints)
        {
            int multiplierBefore = Combo.Multiplier;
            ScoreAward award = Combo.Award(rawPoints, order.IsWithinGrace(Time.time));

            AddScore(award.Points);

            if (Combo.Multiplier != multiplierBefore)
            {
                OnMultiplierChanged?.Invoke(Combo.Multiplier);
            }

            OnOrderAwarded?.Invoke(window, award);
        }

        private void HandleRoundStarted()
        {
            HasNewHighScore = false;
            Score = 0;
            Combo.Reset();
            OnScoreChanged?.Invoke(Score);
            OnMultiplierChanged?.Invoke(Combo.Multiplier);
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
