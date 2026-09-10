using UnityEngine;

namespace YesChef.Characters
{
    /// <summary>
    /// Presentation half of the chef: a walk bob and a little squash while moving.
    /// Reads <see cref="PlayerController.IsMoving"/> and never influences gameplay, so the
    /// controller stays a pure movement/interaction machine.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private Transform _bodyRoot;

        [Header("Bob")]
        [SerializeField, Min(0f)] private float _bobFrequency = 9f;
        [SerializeField, Min(0f)] private float _bobHeight = 0.07f;
        [SerializeField, Min(0f)] private float _squashAmount = 0.07f;
        [SerializeField, Min(0f)] private float _settleSharpness = 12f;

        private Vector3 _baseLocalPosition;
        private Vector3 _baseLocalScale;
        private float _phase;

        private void Awake()
        {
            if (_bodyRoot == null)
            {
                return;
            }

            _baseLocalPosition = _bodyRoot.localPosition;
            _baseLocalScale = _bodyRoot.localScale;
        }

        private void LateUpdate()
        {
            if (_player == null || _bodyRoot == null)
            {
                return;
            }

            if (_player.IsMoving)
            {
                _phase += Time.deltaTime * _bobFrequency;

                float bob = Mathf.Sin(_phase);
                _bodyRoot.localPosition = _baseLocalPosition + Vector3.up * (Mathf.Abs(bob) * _bobHeight);

                float squash = bob * _squashAmount;
                _bodyRoot.localScale = new Vector3(
                    _baseLocalScale.x * (1f + squash * 0.5f),
                    _baseLocalScale.y * (1f - squash),
                    _baseLocalScale.z * (1f + squash * 0.5f));

                return;
            }

            // Ease back to the rest pose so stopping never snaps.
            float t = 1f - Mathf.Exp(-_settleSharpness * Time.deltaTime);
            _bodyRoot.localPosition = Vector3.Lerp(_bodyRoot.localPosition, _baseLocalPosition, t);
            _bodyRoot.localScale = Vector3.Lerp(_bodyRoot.localScale, _baseLocalScale, t);
        }
    }
}
