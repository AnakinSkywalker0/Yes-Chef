using System.Collections.Generic;
using UnityEngine;
using YesChef.Characters;
using YesChef.Ingredients;

namespace YesChef.Stations
{
    /// <summary>
    /// A stove with several independent burners. The player interacts with the stove as a
    /// whole: dropping meat fills the first free burner, empty-handed interaction collects
    /// the first burner that has finished.
    /// </summary>
    public class StoveStation : BaseStation
    {
        [SerializeField] private List<StoveSlot> _slots = new();

        public IReadOnlyList<StoveSlot> Slots => _slots;

        public override void Interact(PlayerController player)
        {
            if (player.HasIngredient)
            {
                TryStartCooking(player);
                return;
            }

            TryCollectCooked(player);
        }

        private void TryStartCooking(PlayerController player)
        {
            Ingredient candidate = player.HeldIngredient;
            if (!CanCook(candidate))
            {
                RaiseRejected();
                return;
            }

            StoveSlot slot = FindFirstEmptySlot();
            if (slot == null)
            {
                RaiseRejected();
                return;
            }

            candidate.SetHolder(slot);
            slot.BeginCooking();
            RaiseInteracted();
        }

        private void TryCollectCooked(PlayerController player)
        {
            StoveSlot slot = FindFirstCookedSlot();
            if (slot == null)
            {
                RaiseRejected();
                return;
            }

            slot.HeldIngredient.SetHolder(player);
            RaiseInteracted();
        }

        private StoveSlot FindFirstEmptySlot()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null && _slots[i].IsEmpty)
                {
                    return _slots[i];
                }
            }

            return null;
        }

        private StoveSlot FindFirstCookedSlot()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null && _slots[i].HoldsCookedIngredient)
                {
                    return _slots[i];
                }
            }

            return null;
        }

        private static bool CanCook(Ingredient ingredient) =>
            ingredient != null
            && ingredient.Definition != null
            && ingredient.Definition.RequiredProcess == IngredientProcess.Cook
            && ingredient.State == IngredientState.Raw;
    }
}
