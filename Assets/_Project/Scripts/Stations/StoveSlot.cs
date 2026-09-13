using System;
using UnityEngine;
using YesChef.Ingredients;

namespace YesChef.Stations
{
    /// <summary>
    /// One burner on a stove. Owns its own cooking timer and progress bar, which is what
    /// lets a single stove run two independent pieces of meat.
    /// <para>
    /// Slots are driven by <see cref="StoveStation"/> rather than being interactable
    /// themselves, so the player interacts with "the stove" and the stove picks a slot.
    /// </para>
    /// </summary>
    public class StoveSlot : MonoBehaviour, IIngredientHolder, IHasProgress
    {
        [SerializeField] private Transform _ingredientAnchor;
        [SerializeField, Min(0.05f)] private float _cookDuration = 6f;

        private PreparationTimer _timer;

        private PreparationTimer Timer => _timer ??= new PreparationTimer(_cookDuration);

        public event Action OnProgressChanged
        {
            add => Timer.OnChanged += value;
            remove => Timer.OnChanged -= value;
        }

        public bool IsCooking => Timer.IsRunning;
        public bool IsInProgress => Timer.IsRunning;
        public float Progress => Timer.Progress;

        public Transform IngredientAnchor => _ingredientAnchor;
        public Ingredient HeldIngredient { get; private set; }
        public bool HasIngredient => HeldIngredient != null;

        public bool IsEmpty => !HasIngredient;

        /// <summary>True when this burner holds meat that has finished cooking.</summary>
        public bool HoldsCookedIngredient =>
            HasIngredient && !IsCooking && HeldIngredient.State == IngredientState.Prepared;

        void IIngredientHolder.SetIngredient(Ingredient ingredient) => HeldIngredient = ingredient;

        void IIngredientHolder.ClearIngredient()
        {
            HeldIngredient = null;
            Timer.Cancel();
        }

        public void BeginCooking()
        {
            if (!HasIngredient)
            {
                Debug.LogError($"{name} was told to cook with an empty burner.", this);
                return;
            }

            Timer.Start();
        }

        private void Update() => Advance(Time.deltaTime);

        /// <summary>Advances the cook. Exposed to tests so the rules can be checked without frames.</summary>
        internal void Advance(float deltaTime)
        {
            if (Timer.Tick(deltaTime) && HeldIngredient != null)
            {
                HeldIngredient.MarkPrepared();
            }
        }
    }
}
