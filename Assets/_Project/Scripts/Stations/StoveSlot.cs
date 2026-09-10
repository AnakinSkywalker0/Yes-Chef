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

        private float _elapsed;

        public event Action OnProgressChanged;

        public bool IsCooking { get; private set; }
        public bool IsInProgress => IsCooking;
        public float Progress { get; private set; }

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
            CancelCooking();
        }

        public void BeginCooking()
        {
            if (!HasIngredient)
            {
                Debug.LogError($"{name} was told to cook with an empty burner.", this);
                return;
            }

            _elapsed = 0f;
            IsCooking = true;
            SetProgress(0f, forceNotify: true);
        }

        private void Update()
        {
            if (!IsCooking)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            SetProgress(Mathf.Clamp01(_elapsed / _cookDuration));

            if (_elapsed < _cookDuration)
            {
                return;
            }

            CompleteCooking();
        }

        private void CompleteCooking()
        {
            IsCooking = false;
            HeldIngredient.MarkPrepared();
            SetProgress(0f, forceNotify: true);
        }

        private void CancelCooking()
        {
            if (!IsCooking)
            {
                return;
            }

            IsCooking = false;
            SetProgress(0f, forceNotify: true);
        }

        private void SetProgress(float value, bool forceNotify = false)
        {
            if (!forceNotify && Mathf.Approximately(Progress, value))
            {
                return;
            }

            Progress = value;
            OnProgressChanged?.Invoke();
        }
    }
}
