using System;
using UnityEngine;

namespace YesChef.Ingredients
{
    /// <summary>
    /// A live ingredient in the kitchen. Holds state only - every visual reaction lives in
    /// <see cref="IngredientVisual"/> and is driven by <see cref="OnChanged"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class Ingredient : MonoBehaviour
    {
        [SerializeField] private IngredientSO _definition;

        private IIngredientHolder _holder;

        /// <summary>Raised whenever the definition or preparation state changes.</summary>
        public event Action OnChanged;

        public IngredientSO Definition => _definition;
        public IngredientState State { get; private set; } = IngredientState.Raw;
        public IIngredientHolder Holder => _holder;

        /// <summary>True when this ingredient may be handed to a customer window.</summary>
        public bool IsReadyForDelivery =>
            _definition != null && (!_definition.NeedsPreparation || State == IngredientState.Prepared);

        /// <summary>Creates an ingredient and places it straight into <paramref name="holder"/>.</summary>
        public static Ingredient Spawn(IngredientSO definition, IIngredientHolder holder)
        {
            if (definition == null)
            {
                Debug.LogError("Cannot spawn an ingredient without a definition.");
                return null;
            }

            if (definition.Prefab == null)
            {
                Debug.LogError($"Ingredient '{definition.DisplayName}' has no prefab assigned.", definition);
                return null;
            }

            Ingredient instance = Instantiate(definition.Prefab);
            instance.Initialise(definition);
            instance.SetHolder(holder);
            return instance;
        }

        public void Initialise(IngredientSO definition)
        {
            _definition = definition;
            State = IngredientState.Raw;
            OnChanged?.Invoke();
        }

        public void MarkPrepared()
        {
            if (State == IngredientState.Prepared)
            {
                return;
            }

            State = IngredientState.Prepared;
            OnChanged?.Invoke();
        }

        /// <summary>
        /// Moves this ingredient between holders. Detaches from the previous holder,
        /// registers with the new one and snaps the transform onto its anchor.
        /// </summary>
        public void SetHolder(IIngredientHolder holder)
        {
            if (_holder == holder)
            {
                return;
            }

            if (holder != null && holder.HasIngredient)
            {
                Debug.LogError($"'{name}' was moved onto a holder that is already occupied.", this);
                return;
            }

            _holder?.ClearIngredient();
            _holder = holder;

            if (holder == null)
            {
                return;
            }

            holder.SetIngredient(this);
            transform.SetParent(holder.IngredientAnchor, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void DestroySelf()
        {
            _holder?.ClearIngredient();
            _holder = null;
            Destroy(gameObject);
        }
    }
}
