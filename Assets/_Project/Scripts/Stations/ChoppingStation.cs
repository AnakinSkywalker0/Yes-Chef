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

        private PreparationTimer _timer;

        // Created lazily so a progress bar subscribing in its OnEnable never races our Awake.
        private PreparationTimer Timer => _timer ??= new PreparationTimer(_chopDuration);

        public event Action OnProgressChanged
        {
            add => Timer.OnChanged += value;
            remove => Timer.OnChanged -= value;
        }

        public bool IsChopping => Timer.IsRunning;
        public bool IsInProgress => Timer.IsRunning;
        public float Progress => Timer.Progress;

        public Transform IngredientAnchor => _ingredientAnchor;
        public Ingredient HeldIngredient { get; private set; }
        public bool HasIngredient => HeldIngredient != null;

        void IIngredientHolder.SetIngredient(Ingredient ingredient) => HeldIngredient = ingredient;

        void IIngredientHolder.ClearIngredient()
        {
            HeldIngredient = null;
            Timer.Cancel();
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

        // Time.deltaTime is zero while the game is paused, so this freezes for free.
        private void Update() => Advance(Time.deltaTime);

        /// <summary>Advances the chop. Exposed to tests so the rules can be checked without frames.</summary>
        internal void Advance(float deltaTime)
        {
            if (Timer.Tick(deltaTime) && HeldIngredient != null)
            {
                HeldIngredient.MarkPrepared();
            }
        }

        private void TryPlaceIngredient(PlayerController player)
        {
            if (HasIngredient || !CanChop(player.HeldIngredient))
            {
                RaiseRejected();
                return;
            }

            player.HeldIngredient.SetHolder(this);
            Timer.Start();
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
    }
}
