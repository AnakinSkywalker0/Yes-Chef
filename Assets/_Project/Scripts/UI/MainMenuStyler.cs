using UnityEngine;

namespace YesChef.UI
{
    /// <summary>
    /// Motion for the main menu: the logo sways, the buttons pop in one after another, the
    /// best-score ticket flutters in and the ingredient icons bob. Purely presentational;
    /// <see cref="MainMenuUI"/> keeps all behaviour. Lives on the screen root so the intro
    /// replays every time the menu is shown.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuStyler : MonoBehaviour
    {
        [SerializeField] private RectTransform _logo;
        [SerializeField] private RectTransform _bestTicket;
        [SerializeField] private RectTransform _buttonColumn;
        [SerializeField] private MenuButtonFx[] _buttons;
        [SerializeField] private RectTransform[] _floaters;

        [Header("Logo")]
        [SerializeField] private float _logoTilt = -4f;
        [SerializeField] private float _logoSway = 1.2f;

        [Header("Intro")]
        [SerializeField, Min(0f)] private float _stagger = 0.09f;
        [SerializeField, Min(0.05f)] private float _duration = 0.5f;

        private Vector2[] _floaterHome;
        private Vector2 _columnHome;
        private float _time;

        private void Awake()
        {
            _floaterHome = new Vector2[_floaters != null ? _floaters.Length : 0];
            for (int i = 0; i < _floaterHome.Length; i++)
            {
                _floaterHome[i] = _floaters[i].anchoredPosition;
            }

            if (_buttonColumn != null)
            {
                _columnHome = _buttonColumn.anchoredPosition;
            }
        }

        private void OnEnable() => _time = 0f;

        private void Update()
        {
            _time += Time.unscaledDeltaTime;
            AnimateLogo();
            AnimateButtons();
            AnimateTicket();
            AnimateFloaters();
        }

        private void AnimateLogo()
        {
            if (_logo == null)
            {
                return;
            }

            float pop = EaseOutBack(Mathf.Clamp01(_time / _duration));
            float sway = Mathf.Sin(Time.unscaledTime * 1.6f) * _logoSway;
            _logo.localRotation = Quaternion.Euler(0f, 0f, _logoTilt + sway);
            _logo.localScale = Vector3.one * (pop * (1f + Mathf.Sin(Time.unscaledTime * 2.4f) * 0.012f));
        }

        private void AnimateButtons()
        {
            if (_buttons != null)
            {
                for (int i = 0; i < _buttons.Length; i++)
                {
                    if (_buttons[i] != null)
                    {
                        float t = Mathf.Clamp01((_time - (i + 1) * _stagger) / _duration);
                        _buttons[i].IntroScale = EaseOutBack(t);
                    }
                }
            }

            if (_buttonColumn != null)
            {
                float slide = (1f - EaseOutCubic(Mathf.Clamp01(_time / (_duration * 1.2f)))) * -90f;
                _buttonColumn.anchoredPosition = _columnHome + new Vector2(slide, 0f);
            }
        }

        private void AnimateTicket()
        {
            if (_bestTicket == null)
            {
                return;
            }

            float t = EaseOutBack(Mathf.Clamp01((_time - 0.3f) / 0.5f));
            _bestTicket.localScale = Vector3.one * t;
            _bestTicket.localRotation = Quaternion.Euler(0f, 0f, 3f + Mathf.Sin(Time.unscaledTime * 1.3f) * 0.8f);
        }

        private void AnimateFloaters()
        {
            if (_floaters == null)
            {
                return;
            }

            float intro = EaseOutBack(Mathf.Clamp01((_time - 0.25f) / 0.6f));
            for (int i = 0; i < _floaters.Length; i++)
            {
                if (_floaters[i] == null)
                {
                    continue;
                }

                float t = Time.unscaledTime * 1.3f + i * 1.7f;
                _floaters[i].anchoredPosition = _floaterHome[i] + new Vector2(Mathf.Sin(t * 0.7f) * 12f, Mathf.Sin(t) * 20f);
                _floaters[i].localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.8f) * 12f);
                _floaters[i].localScale = Vector3.one * intro;
            }
        }

        private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }
    }
}
