using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YesChef.Orders;
using YesChef.Stations;

namespace YesChef.UI
{
    /// <summary>
    /// The ticket floating above a customer window: what the order needs, how long it has
    /// been waiting, and what it is currently worth.
    /// <para>
    /// Showing the live value alongside the age makes the "score decays with time" rule
    /// legible while playing, so the player can triage which window to serve first.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class OrderTicketUI : MonoBehaviour
    {
        [SerializeField] private CustomerWindowStation _window;

        [Tooltip("Child object toggled with the ticket. Must NOT be this GameObject.")]
        [SerializeField] private GameObject _ticketRoot;

        [SerializeField] private List<OrderIngredientIconUI> _icons = new();
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private ScorePopupUI _scorePopup;

        [Header("Value colours")]
        [SerializeField] private Color _positiveColor = new(0.85f, 0.95f, 0.6f);
        [SerializeField] private Color _negativeColor = new(1f, 0.45f, 0.42f);

        private int _lastShownSeconds = -1;

        private void Start()
        {
            if (_window != null)
            {
                _window.OnOrderChanged += HandleOrderChanged;
                _window.OnOrderCompleted += HandleOrderCompleted;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (_window == null)
            {
                return;
            }

            _window.OnOrderChanged -= HandleOrderChanged;
            _window.OnOrderCompleted -= HandleOrderCompleted;
        }

        private void Update()
        {
            if (_window == null || !_window.HasOrder)
            {
                return;
            }

            Order order = _window.CurrentOrder;
            int seconds = Mathf.FloorToInt(order.GetElapsedSeconds(Time.time));

            // Text is only rebuilt when the displayed second actually changes.
            if (seconds == _lastShownSeconds)
            {
                return;
            }

            _lastShownSeconds = seconds;

            if (_timerText != null)
            {
                _timerText.text = $"{seconds}s";
            }

            if (_valueText == null)
            {
                return;
            }

            int value = order.CalculateScore(Time.time);
            _valueText.text = value.ToString();
            _valueText.color = value >= 0 ? _positiveColor : _negativeColor;
        }

        private void HandleOrderChanged(CustomerWindowStation window) => Refresh();

        private void HandleOrderCompleted(CustomerWindowStation window, Order order, int awardedPoints)
        {
            if (_scorePopup != null)
            {
                _scorePopup.Show(awardedPoints);
            }
        }

        private void Refresh()
        {
            bool hasOrder = _window != null && _window.HasOrder;
            _lastShownSeconds = -1;

            if (_ticketRoot != null)
            {
                _ticketRoot.SetActive(hasOrder);
            }

            if (!hasOrder)
            {
                HideAllIcons();
                return;
            }

            IReadOnlyList<Ingredients.IngredientSO> requirements = _window.CurrentOrder.Requirements;

            for (int i = 0; i < _icons.Count; i++)
            {
                if (_icons[i] == null)
                {
                    continue;
                }

                if (i < requirements.Count)
                {
                    _icons[i].Show(requirements[i], _window.CurrentOrder.IsDelivered(i));
                }
                else
                {
                    _icons[i].Hide();
                }
            }
        }

        private void HideAllIcons()
        {
            for (int i = 0; i < _icons.Count; i++)
            {
                if (_icons[i] != null)
                {
                    _icons[i].Hide();
                }
            }
        }
    }
}
