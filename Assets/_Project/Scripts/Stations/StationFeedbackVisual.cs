using System.Collections;
using UnityEngine;

namespace YesChef.Stations
{
    /// <summary>
    /// Physical feedback for a station: a small scale punch when an interaction lands, a
    /// red flash and shake when it is refused. Nothing here touches gameplay - it only
    /// listens to the events <see cref="BaseStation"/> already raises.
    /// </summary>
    [DisallowMultipleComponent]
    public class StationFeedbackVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private BaseStation _station;
        [SerializeField] private Transform _body;
        [SerializeField] private Renderer _bodyRenderer;

        [Header("Accepted")]
        [SerializeField, Min(0f)] private float _punchScale = 0.07f;
        [SerializeField, Min(0.01f)] private float _punchDuration = 0.18f;

        [Header("Rejected")]
        [SerializeField] private Color _rejectTint = new(1f, 0.32f, 0.28f);
        [SerializeField, Min(0.01f)] private float _rejectDuration = 0.32f;
        [SerializeField, Min(0f)] private float _shakeDistance = 0.05f;

        private Vector3 _baseScale;
        private Vector3 _basePosition;
        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _routine;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            if (_body != null)
            {
                _baseScale = _body.localScale;
                _basePosition = _body.localPosition;
            }
        }

        private void OnEnable()
        {
            if (_station == null)
            {
                return;
            }

            _station.OnInteracted += HandleInteracted;
            _station.OnInteractionRejected += HandleRejected;
        }

        private void OnDisable()
        {
            if (_station != null)
            {
                _station.OnInteracted -= HandleInteracted;
                _station.OnInteractionRejected -= HandleRejected;
            }

            StopAndReset();
        }

        private void HandleInteracted() => Play(PunchRoutine());

        private void HandleRejected() => Play(RejectRoutine());

        private void Play(IEnumerator routine)
        {
            StopAndReset();
            _routine = StartCoroutine(routine);
        }

        private void StopAndReset()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            if (_body != null)
            {
                _body.localScale = _baseScale;
                _body.localPosition = _basePosition;
            }

            if (_bodyRenderer != null)
            {
                _propertyBlock.Clear();
                _bodyRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private IEnumerator PunchRoutine()
        {
            float elapsed = 0f;

            while (elapsed < _punchDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _punchDuration);
                float bump = Mathf.Sin(t * Mathf.PI) * _punchScale;

                if (_body != null)
                {
                    _body.localScale = _baseScale * (1f + bump);
                }

                yield return null;
            }

            StopAndReset();
        }

        private IEnumerator RejectRoutine()
        {
            float elapsed = 0f;

            while (elapsed < _rejectDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _rejectDuration);
                float falloff = 1f - t;

                if (_body != null)
                {
                    float shake = Mathf.Sin(t * Mathf.PI * 6f) * _shakeDistance * falloff;
                    _body.localPosition = _basePosition + Vector3.right * shake;
                }

                if (_bodyRenderer != null)
                {
                    Color tint = _rejectTint;
                    tint.a = 1f;
                    _propertyBlock.SetColor(BaseColorId, Color.Lerp(_rejectTint, GetBaseColor(), t));
                    _bodyRenderer.SetPropertyBlock(_propertyBlock);
                }

                yield return null;
            }

            StopAndReset();
        }

        private Color GetBaseColor()
        {
            Material material = _bodyRenderer != null ? _bodyRenderer.sharedMaterial : null;
            return material != null && material.HasProperty(BaseColorId) ? material.GetColor(BaseColorId) : Color.white;
        }
    }
}
