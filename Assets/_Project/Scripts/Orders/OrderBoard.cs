using System;
using System.Collections.Generic;
using UnityEngine;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Stations;

namespace YesChef.Orders
{
    /// <summary>
    /// Keeps the customer windows stocked. Windows own their order; the board owns the
    /// rules about when a new one appears and how patient the customers are, and funnels
    /// completions from every window into a single signal for the scorer.
    /// </summary>
    public class OrderBoard : MonoBehaviour
    {
        [Tooltip("Ingredients orders may ask for.")]
        [SerializeField] private List<IngredientSO> _ingredientPool = new();

        [SerializeField] private List<CustomerWindowStation> _windows = new();

        [Tooltip("Seconds an empty window waits before a new order appears.")]
        [SerializeField, Min(0f)] private float _respawnDelay = 5f;

        [Tooltip("Seconds a customer waits at full value before the order starts losing points. " +
                 "Delivering inside this window keeps the combo streak alive.")]
        [SerializeField, Min(0f)] private float _gracePeriod = 25f;

        private readonly List<PendingOrder> _pendingOrders = new();
        private OrderGenerator _generator;

        /// <summary>
        /// Raised when any window completes its order, carrying the order and the points it
        /// was worth on its own. Combo multipliers are applied downstream by the scorer.
        /// </summary>
        public event Action<CustomerWindowStation, Order, int> OnOrderCompleted;

        public IReadOnlyList<CustomerWindowStation> Windows => _windows;
        public float GracePeriod => _gracePeriod;

        private struct PendingOrder
        {
            public CustomerWindowStation Window;
            public float TimeRemaining;
        }

        private void Awake() => _generator = new OrderGenerator(_ingredientPool, _gracePeriod);

        private void Start()
        {
            for (int i = 0; i < _windows.Count; i++)
            {
                if (_windows[i] != null)
                {
                    _windows[i].OnOrderCompleted += HandleOrderCompleted;
                }
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
            for (int i = 0; i < _windows.Count; i++)
            {
                if (_windows[i] != null)
                {
                    _windows[i].OnOrderCompleted -= HandleOrderCompleted;
                }
            }

            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
            GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                return;
            }

            TickPendingOrders();
        }

        private void TickPendingOrders()
        {
            for (int i = _pendingOrders.Count - 1; i >= 0; i--)
            {
                PendingOrder pending = _pendingOrders[i];
                pending.TimeRemaining -= Time.deltaTime;

                if (pending.TimeRemaining > 0f)
                {
                    _pendingOrders[i] = pending;
                    continue;
                }

                _pendingOrders.RemoveAt(i);
                AssignNewOrder(pending.Window);
            }
        }

        /// <summary>The kitchen opens with every window already showing an order.</summary>
        private void HandleRoundStarted()
        {
            _pendingOrders.Clear();

            for (int i = 0; i < _windows.Count; i++)
            {
                AssignNewOrder(_windows[i]);
            }
        }

        private void HandleRoundEnded()
        {
            _pendingOrders.Clear();

            for (int i = 0; i < _windows.Count; i++)
            {
                if (_windows[i] != null)
                {
                    _windows[i].ClearOrder();
                }
            }
        }

        private void HandleOrderCompleted(CustomerWindowStation window, Order order, int rawPoints)
        {
            OnOrderCompleted?.Invoke(window, order, rawPoints);

            _pendingOrders.Add(new PendingOrder
            {
                Window = window,
                TimeRemaining = _respawnDelay
            });
        }

        private void AssignNewOrder(CustomerWindowStation window)
        {
            if (window == null)
            {
                return;
            }

            window.AssignOrder(_generator.Create(Time.time));
        }
    }
}
