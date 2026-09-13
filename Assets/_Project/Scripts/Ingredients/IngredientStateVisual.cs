using UnityEngine;

namespace YesChef.Ingredients
{
    /// <summary>
    /// Presentation half of an ingredient: swaps between an authored "raw" look and an
    /// authored "prepared" look as <see cref="Ingredient.State"/> changes. A whole
    /// broccoli becomes a pile of pieces, a pink patty becomes a seared one.
    /// <para>
    /// Ingredients that need no preparation (cheese) simply leave the prepared look empty.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class IngredientStateVisual : MonoBehaviour
    {
        [SerializeField] private Ingredient _ingredient;
        [SerializeField] private GameObject _rawLook;
        [SerializeField] private GameObject _preparedLook;

        private void OnEnable()
        {
            if (_ingredient == null)
            {
                Debug.LogError($"{nameof(IngredientStateVisual)} is missing its {nameof(Ingredient)} reference.", this);
                return;
            }

            _ingredient.OnChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_ingredient != null)
            {
                _ingredient.OnChanged -= Refresh;
            }
        }

        private void Refresh()
        {
            bool prepared = _ingredient.State == IngredientState.Prepared && _preparedLook != null;

            if (_rawLook != null)
            {
                _rawLook.SetActive(!prepared);
            }

            if (_preparedLook != null)
            {
                _preparedLook.SetActive(prepared);
            }
        }
    }
}
