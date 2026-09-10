using UnityEngine;

namespace YesChef.Ingredients
{
    /// <summary>
    /// Anything that can physically carry a single ingredient: the player's hands, a
    /// chopping table, a stove slot.
    /// <para>
    /// Implementations only track the reference - moving the ingredient (including
    /// detaching it from its previous holder and re-parenting the transform) is handled in
    /// one place, <see cref="Ingredient.SetHolder"/>, so the two sides can never disagree.
    /// </para>
    /// </summary>
    public interface IIngredientHolder
    {
        /// <summary>Transform the ingredient snaps to while held.</summary>
        Transform IngredientAnchor { get; }

        Ingredient HeldIngredient { get; }

        bool HasIngredient { get; }

        /// <summary>Called by <see cref="Ingredient.SetHolder"/>. Do not call directly.</summary>
        void SetIngredient(Ingredient ingredient);

        /// <summary>Called by <see cref="Ingredient.SetHolder"/>. Do not call directly.</summary>
        void ClearIngredient();
    }
}
