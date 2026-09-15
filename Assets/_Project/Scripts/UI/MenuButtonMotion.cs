using UnityEngine;
using UnityEngine.EventSystems;

namespace YesChef.UI
{
    /// <summary>
    /// Gives a menu button a little life: it grows under the pointer and dips when pressed.
    /// Runs on unscaled time so it keeps working while the game is paused.
    /// </summary>
    [DisallowMultipleComponent]
    public class MenuButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField, Min(1f)] private float _hoverScale = 1.04f;
        [SerializeField, Range(0.5f, 1f)] private float _pressScale = 0.97f;
        [SerializeField, Min(1f)] private float _speed = 14f;

        private bool _hovered;
        private bool _pressed;
        private float _current = 1f;

        private void OnDisable()
        {
            _hovered = false;
            _pressed = false;
            _current = 1f;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            float target = _pressed ? _pressScale : _hovered ? _hoverScale : 1f;
            _current = Mathf.Lerp(_current, target, 1f - Mathf.Exp(-_speed * Time.unscaledDeltaTime));
            transform.localScale = Vector3.one * _current;
        }

        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; _pressed = false; }
        public void OnPointerDown(PointerEventData eventData) => _pressed = true;
        public void OnPointerUp(PointerEventData eventData) => _pressed = false;
    }
}
