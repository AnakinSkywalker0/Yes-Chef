using UnityEngine;

namespace YesChef.UI
{
    /// <summary>A slow bob and tilt for the title so the menu never looks frozen.</summary>
    [DisallowMultipleComponent]
    public class MenuTitleFloat : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _bobDistance = 6f;
        [SerializeField, Min(0f)] private float _tiltDegrees = 1.2f;
        [SerializeField, Min(0.01f)] private float _period = 3.4f;

        private RectTransform _rect;
        private Vector2 _restPosition;

        private void Awake()
        {
            _rect = transform as RectTransform;
            _restPosition = _rect != null ? _rect.anchoredPosition : Vector2.zero;
        }

        private void OnDisable()
        {
            if (_rect != null)
            {
                _rect.anchoredPosition = _restPosition;
                _rect.localRotation = Quaternion.identity;
            }
        }

        private void Update()
        {
            if (_rect == null)
            {
                return;
            }

            float phase = Time.unscaledTime / _period * Mathf.PI * 2f;
            _rect.anchoredPosition = _restPosition + Vector2.up * (Mathf.Sin(phase) * _bobDistance);
            _rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase * 0.5f) * _tiltDegrees);
        }
    }
}
