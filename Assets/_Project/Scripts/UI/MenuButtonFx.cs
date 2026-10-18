using UnityEngine;
using UnityEngine.EventSystems;

namespace YesChef.UI
{
    /// <summary>
    /// Springy hover / press feedback for a menu button: it grows and tilts under the
    /// pointer or keyboard focus and squashes when pressed. Runs on unscaled time so it
    /// still animates while the game is waiting to start.
    /// </summary>
    [DisallowMultipleComponent]
    public class MenuButtonFx : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        [SerializeField, Min(1f)] private float _hoverScale = 1.06f;
        [SerializeField, Range(0.5f, 1f)] private float _pressScale = 0.95f;
        [SerializeField] private float _hoverTilt = -1.5f;
        [SerializeField, Min(0f)] private float _sharpness = 16f;
        [SerializeField, Min(0f)] private float _idlePulse;

        private bool _hovered;
        private bool _selected;
        private bool _pressed;
        private float _scale = 1f;
        private float _tilt;
        private float _pulsePhase;

        /// <summary>Extra multiplier owned by the intro animation (0 = hidden, 1 = settled).</summary>
        public float IntroScale { get; set; } = 1f;

        /// <summary>Gentle breathing scale while idle; 0 disables it.</summary>
        public void SetIdlePulse(float amount) => _idlePulse = Mathf.Max(0f, amount);

        private void OnEnable()
        {
            _hovered = _selected = _pressed = false;
            _scale = 1f;
            _tilt = 0f;
            Apply();
        }

        private void Update()
        {
            float target = _pressed ? _pressScale : (_hovered || _selected) ? _hoverScale : 1f;
            float tiltTarget = (_hovered || _selected) && !_pressed ? _hoverTilt : 0f;
            float t = 1f - Mathf.Exp(-_sharpness * Time.unscaledDeltaTime);
            _scale = Mathf.Lerp(_scale, target, t);
            _tilt = Mathf.Lerp(_tilt, tiltTarget, t);

            if (_idlePulse > 0f)
            {
                _pulsePhase += Time.unscaledDeltaTime * 2.2f;
            }

            Apply();
        }

        private void Apply()
        {
            float pulse = _idlePulse > 0f && !_hovered && !_selected ? 1f + Mathf.Sin(_pulsePhase) * _idlePulse : 1f;
            transform.localScale = Vector3.one * (_scale * pulse * IntroScale);
            transform.localRotation = Quaternion.Euler(0f, 0f, _tilt);
        }

        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; _pressed = false; }
        public void OnPointerDown(PointerEventData eventData) => _pressed = true;
        public void OnPointerUp(PointerEventData eventData) => _pressed = false;
        public void OnSelect(BaseEventData eventData) => _selected = true;
        public void OnDeselect(BaseEventData eventData) => _selected = false;
    }
}
