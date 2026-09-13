using System;
using UnityEngine;
using YesChef.Controls;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Stations;

namespace YesChef.Characters
{
    /// <summary>
    /// The chef. Handles movement, decides which station is being aimed at, and acts as the
    /// player's pair of hands (one ingredient at a time).
    /// <para>
    /// Station targeting uses a facing-weighted overlap rather than a single raycast: in a
    /// top-down kitchen where counters sit shoulder to shoulder, picking the best-aligned
    /// station in range feels far more forgiving than a ray that must land exactly.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, IIngredientHolder
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _moveSpeed = 7f;
        [SerializeField, Min(0f)] private float _turnSharpness = 16f;
        [SerializeField] private float _gravity = -25f;

        [Header("Interaction")]
        [SerializeField, Min(0f)] private float _interactRadius = 2f;
        [Tooltip("1 = station must be dead ahead, 0 = anywhere in the forward hemisphere.")]
        [SerializeField, Range(-1f, 1f)] private float _minFacingAlignment = 0.2f;
        [Tooltip("Bias towards nearer stations when several sit in front of the player.")]
        [SerializeField, Min(0f)] private float _distanceWeight = 0.35f;
        [SerializeField] private LayerMask _stationMask = ~0;

        [Header("Hands")]
        [SerializeField] private Transform _ingredientAnchor;

        private readonly Collider[] _overlapResults = new Collider[24];
        private CharacterController _characterController;
        private Vector3 _facing = Vector3.forward;
        private float _verticalVelocity;

        /// <summary>Raised when the aimed-at station changes, including to null.</summary>
        public event Action<BaseStation> OnSelectedStationChanged;

        /// <summary>Raised when the chef picks something up or their hands empty (null).</summary>
        public event Action<Ingredient> OnHeldIngredientChanged;

        public BaseStation SelectedStation { get; private set; }
        public bool IsMoving { get; private set; }

        public Transform IngredientAnchor => _ingredientAnchor;
        public Ingredient HeldIngredient { get; private set; }
        public bool HasIngredient => HeldIngredient != null;

        void IIngredientHolder.SetIngredient(Ingredient ingredient)
        {
            HeldIngredient = ingredient;
            OnHeldIngredientChanged?.Invoke(ingredient);
        }

        void IIngredientHolder.ClearIngredient()
        {
            HeldIngredient = null;
            OnHeldIngredientChanged?.Invoke(null);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (GameInput.Instance != null)
            {
                GameInput.Instance.OnInteractPerformed += HandleInteractPerformed;
            }
        }

        private void OnDestroy()
        {
            if (GameInput.Instance != null)
            {
                GameInput.Instance.OnInteractPerformed -= HandleInteractPerformed;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (!CanAct())
            {
                IsMoving = false;
                SetSelectedStation(null);
                return;
            }

            HandleMovement();
            UpdateSelectedStation();
        }

        private static bool CanAct() => GameManager.Instance != null && GameManager.Instance.IsPlaying;

        private void HandleMovement()
        {
            Vector2 input = GameInput.Instance != null ? GameInput.Instance.MoveInput : Vector2.zero;
            Vector3 direction = new Vector3(input.x, 0f, input.y);

            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            IsMoving = direction.sqrMagnitude > 0.0001f;

            if (IsMoving)
            {
                // Framerate-independent smoothing towards the input direction.
                float t = 1f - Mathf.Exp(-_turnSharpness * Time.deltaTime);
                _facing = Vector3.Slerp(_facing, direction.normalized, t);
                transform.rotation = Quaternion.LookRotation(_facing, Vector3.up);
            }

            _verticalVelocity = _characterController.isGrounded
                ? -1f
                : _verticalVelocity + _gravity * Time.deltaTime;

            Vector3 velocity = direction * _moveSpeed + Vector3.up * _verticalVelocity;
            _characterController.Move(velocity * Time.deltaTime);
        }

        private void UpdateSelectedStation()
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            int hitCount = Physics.OverlapSphereNonAlloc(
                origin, _interactRadius, _overlapResults, _stationMask, QueryTriggerInteraction.Collide);

            BaseStation best = null;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _overlapResults[i];
                if (hit == null)
                {
                    continue;
                }

                BaseStation station = hit.GetComponentInParent<BaseStation>();
                if (station == null)
                {
                    continue;
                }

                Vector3 toStation = station.InteractionPoint.position - transform.position;
                toStation.y = 0f;

                float distance = toStation.magnitude;
                if (distance > _interactRadius)
                {
                    continue;
                }

                Vector3 direction = distance > 0.01f ? toStation / distance : _facing;
                float alignment = Vector3.Dot(_facing, direction);
                if (alignment < _minFacingAlignment)
                {
                    continue;
                }

                float score = alignment - distance * _distanceWeight;
                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                best = station;
            }

            SetSelectedStation(best);
        }

        private void SetSelectedStation(BaseStation station)
        {
            if (SelectedStation == station)
            {
                return;
            }

            SelectedStation = station;
            OnSelectedStationChanged?.Invoke(station);
        }

        private void HandleInteractPerformed()
        {
            if (!CanAct() || SelectedStation == null)
            {
                return;
            }

            SelectedStation.Interact(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.35f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, _interactRadius);
        }
    }
}
