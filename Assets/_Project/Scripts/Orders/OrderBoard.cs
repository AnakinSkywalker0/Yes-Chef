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
    /// rules about when a new one appears and turns completions into a score signal.
    /// </summary>
    public class OrderBoard : MonoBehaviour
    {
        [Tooltip("Ingredients orders may ask for.")]
        [SerializeField] private List<IngredientSO> _ingredientPool = new();

        [SerializeField] private List<CustomerWindowStation> _windows = new();

        [Tooltip("Seconds an empty window waits before a new order appears.")]
        [SerializeField, Min(0f)] private float _respawnDelay = 5f;

        private readonly List<PendingOrder> _pendingOrders = new();
        private OrderGenerator _generator;

        /// <summary>Raised when an order is completed, carrying the points it was worth.</summary>
        public event Action<CustomerWindowStation, int> OnOrderScored;

        public IReadOnlyList<CustomerWindowStation> Windows => _windows;

        private struct PendingOrder
        {
            public CustomerWindowStation Window;
            public float TimeRemaining;
        }

        private void Awake() => _generator = new OrderGenerator(_ingredientPool);

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

        private void HandleOrderCompleted(CustomerWindowStation window, Order order, int awardedPoints)
        {
            OnOrderScored?.Invoke(window, awardedPoints);

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
