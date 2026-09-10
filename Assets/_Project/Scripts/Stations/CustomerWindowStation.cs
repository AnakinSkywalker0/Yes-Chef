using System;
using YesChef.Characters;
using YesChef.Ingredients;
using YesChef.Orders;

namespace YesChef.Stations
{
    /// <summary>
    /// A hatch where the player hands finished ingredients to a customer.
    /// <para>
    /// The window owns its order and settles the score the moment the last ingredient
    /// lands, so the awarded points are computed exactly once, at exactly the right time.
    /// </para>
    /// </summary>
    public class CustomerWindowStation : BaseStation
    {
        /// <summary>Raised whenever the displayed order changes: assigned, ticked off or cleared.</summary>
        public event Action<CustomerWindowStation> OnOrderChanged;

        /// <summary>Raised when the final ingredient lands, carrying the points awarded.</summary>
        public event Action<CustomerWindowStation, Order, int> OnOrderCompleted;

        public Order CurrentOrder { get; private set; }
        public bool HasOrder => CurrentOrder != null;

        public void AssignOrder(Order order)
        {
            CurrentOrder = order;
            OnOrderChanged?.Invoke(this);
        }

        public void ClearOrder()
        {
            if (CurrentOrder == null)
            {
                return;
            }

            CurrentOrder = null;
            OnOrderChanged?.Invoke(this);
        }

        public override void Interact(PlayerController player)
        {
            if (!player.HasIngredient || !HasOrder)
            {
                RaiseRejected();
                return;
            }

            Ingredient candidate = player.HeldIngredient;

            // Unprepared food and ingredients this order does not want stay in hand.
            if (!candidate.IsReadyForDelivery || !CurrentOrder.TryDeliver(candidate.Definition))
            {
                RaiseRejected();
                return;
            }

            candidate.DestroySelf();
            RaiseInteracted();

            if (!CurrentOrder.IsComplete)
            {
                OnOrderChanged?.Invoke(this);
                return;
            }

            Order completed = CurrentOrder;
            int awarded = completed.CalculateScore(UnityEngine.Time.time);

            CurrentOrder = null;
            OnOrderChanged?.Invoke(this);
            OnOrderCompleted?.Invoke(this, completed, awarded);
        }
    }
}
