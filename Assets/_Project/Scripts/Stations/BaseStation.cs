using System;
using UnityEngine;
using YesChef.Characters;

namespace YesChef.Stations
{
    /// <summary>
    /// Base class for every interactable in the kitchen. A station decides for itself what
    /// an interaction means; the player only knows how to say "interact with that".
    /// <para>
    /// Stations expose success/rejection events so feedback (highlights, flashes) can be
    /// bolted on by separate visual components without touching gameplay rules.
    /// </para>
    /// </summary>
    public abstract class BaseStation : MonoBehaviour
    {
        [Tooltip("Point the player aims at. Falls back to this transform when empty.")]
        [SerializeField] private Transform _interactionPoint;

        /// <summary>Raised when an interaction actually changed something.</summary>
        public event Action OnInteracted;

        /// <summary>Raised when an interaction was refused (wrong ingredient, station busy...).</summary>
        public event Action OnInteractionRejected;

        public Transform InteractionPoint => _interactionPoint != null ? _interactionPoint : transform;

        public abstract void Interact(PlayerController player);

        protected void RaiseInteracted() => OnInteracted?.Invoke();

        protected void RaiseRejected() => OnInteractionRejected?.Invoke();
    }
}
