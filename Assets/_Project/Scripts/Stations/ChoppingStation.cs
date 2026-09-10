using System;
using UnityEngine;
using YesChef.Characters;
using YesChef.Ingredients;

namespace YesChef.Stations
{
    /// <summary>
    /// A table that chops one vegetable at a time.
    /// <para>
    /// Placing a raw vegetable starts the timer automatically and the player is free to
    /// walk away, which keeps the kitchen flowing. The table stays occupied for the whole
    /// two seconds, which is what enforces "one vegetable at a time".
    /// </para>
    /// </summary>
    public class ChoppingStation : BaseStation, IIngredientHolder, IHasProgress
    {
        [SerializeField] private Transform _ingredientAnchor;
        [SerializeField, Min(0.05f)] private float _chopDuration = 2f;

        private float _elapsed;

        public event Action OnProgressChanged;

        public bool IsChopping { get; private set; }
        public bool IsInProgress => IsChopping;
        public float Progress { get; private set; }

        public Transform IngredientAnchor => _ingredientAnchor;
        public Ingredient HeldIngredient { get; private set; }
        public bool HasIngredient => HeldIngredient != null;

        void IIngredientHolder.SetIngredient(Ingredient ingredient) => HeldIngredient = ingredient;

        void IIngredientHolder.ClearIngredient()
        {
            HeldIngredient = null;
            CancelChopping();
        }

        public override void Interact(PlayerController player)
        {
            // Busy tables refuse everything; the knife is in the way.
            if (IsChopping)
            {
                RaiseRejected();
                return;
            }

            if (player.HasIngredient)
            {
                TryPlaceIngredient(player);
                return;
            }

            TryCollectIngredient(player);
        }

        private void Update()
        {
            if (!IsChopping)
            {
                return;
            }

            // Time.deltaTime is zero while the game is paused, so this freezes for free.
            _elapsed += Time.deltaTime;
            SetProgress(Mathf.Clamp01(_elapsed / _chopDuration));

            if (_elapsed < _chopDuration)
            {
                return;
            }

            CompleteChopping();
        }

        private void TryPlaceIngredient(PlayerController player)
        {
            if (HasIngredient)
            {
                RaiseRejected();
                return;
            }

            Ingredient candidate = player.HeldIngredient;
            if (!CanChop(candidate))
            {
                RaiseRejected();
                return;
            }

            candidate.SetHolder(this);
            BeginChopping();
            RaiseInteracted();
        }

        private void TryCollectIngredient(PlayerController player)
        {
            if (!HasIngredient)
            {
                RaiseRejected();
                return;
            }

            HeldIngredient.SetHolder(player);
            RaiseInteracted();
        }

        private static bool CanChop(Ingredient ingredient) =>
            ingredient != null
            && ingredient.Definition != null
            && ingredient.Definition.RequiredProcess == IngredientProcess.Chop
            && ingredient.State == IngredientState.Raw;

        private void BeginChopping()
        {
            _elapsed = 0f;
            IsChopping = true;
            SetProgress(0f, forceNotify: true);
        }

        private void CompleteChopping()
        {
            IsChopping = false;
            HeldIngredient.MarkPrepared();
            SetProgress(0f, forceNotify: true);
        }

        private void CancelChopping()
        {
            if (!IsChopping)
            {
                return;
            }

            IsChopping = false;
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
