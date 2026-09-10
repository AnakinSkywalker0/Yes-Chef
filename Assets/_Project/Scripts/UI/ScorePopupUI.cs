using System.Collections;
using TMPro;
using UnityEngine;

namespace YesChef.UI
{
    /// <summary>
    /// The "+17" that floats up and fades away beside a window when an order is settled.
    /// Negative awards are shown in red so a slow delivery reads as a punishment.
    /// </summary>
    [DisallowMultipleComponent]
    public class ScorePopupUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _text;

        [Header("Animation")]
        [SerializeField, Min(0.1f)] private float _duration = 2.5f;
        [SerializeField] private float _riseDistance = 40f;
        [SerializeField] private Color _positiveColor = new(0.5f, 1f, 0.55f);
        [SerializeField] private Color _negativeColor = new(1f, 0.45f, 0.42f);

        private Vector3 _restLocalPosition;
        private Coroutine _routine;

        private void Awake()
        {
            _restLocalPosition = transform.localPosition;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        public void Show(int points)
        {
            if (_text == null || _canvasGroup == null)
            {
                return;
            }

            _text.text = points >= 0 ? $"+{points}" : points.ToString();
            _text.color = points >= 0 ? _positiveColor : _negativeColor;

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);

                transform.localPosition = _restLocalPosition + Vector3.up * (_riseDistance * t);
                _canvasGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, t);

                yield return null;
            }

            _canvasGroup.alpha = 0f;
            transform.localPosition = _restLocalPosition;
            _routine = null;
        }
    }
}
