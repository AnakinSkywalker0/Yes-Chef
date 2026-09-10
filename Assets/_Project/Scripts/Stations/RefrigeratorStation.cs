using UnityEngine;
using YesChef.Characters;
using YesChef.Ingredients;

namespace YesChef.Stations
{
    /// <summary>
    /// A refrigerator shelf dispensing one raw ingredient type, forever.
    /// <para>
    /// The design asks for refrigerators that hold every raw ingredient. Modelling each
    /// shelf as its own station keeps the interaction unambiguous - the player takes what
    /// they are standing in front of - and avoids a selection menu mid-service.
    /// </para>
    /// </summary>
    public class RefrigeratorStation : BaseStation
    {
        [SerializeField] private IngredientSO _dispensedIngredient;

        public IngredientSO DispensedIngredient => _dispensedIngredient;

        public override void Interact(PlayerController player)
        {
            if (player.HasIngredient)
            {
                RaiseRejected();
                return;
            }

            if (_dispensedIngredient == null)
            {
                Debug.LogError($"{name} has no ingredient assigned.", this);
                RaiseRejected();
                return;
            }

            Ingredient.Spawn(_dispensedIngredient, player);
            RaiseInteracted();
        }
    }
}
