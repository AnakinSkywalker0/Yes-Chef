using UnityEngine;
using UnityEngine.UI;
using YesChef.Stations;

namespace YesChef.UI
{
    /// <summary>
    /// Shows the remaining time on a timed station job. Binds to <see cref="IHasProgress"/>,
    /// so it works for chopping tables and stove burners without knowing about either.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldProgressBarUI : MonoBehaviour
    {
        [Tooltip("Any component implementing IHasProgress (chopping table, stove burner...).")]
        [SerializeField] private MonoBehaviour _progressSourceBehaviour;

        [Tooltip("Child object toggled with the bar. Must NOT be this GameObject.")]
        [SerializeField] private GameObject _barRoot;

        [SerializeField] private Image _fillImage;

        private IHasProgress _progressSource;

        private void Awake()
        {
            _progressSource = _progressSourceBehaviour as IHasProgress;

            if (_progressSource == null)
            {
                Debug.LogError(
                    $"{nameof(WorldProgressBarUI)} needs a source implementing {nameof(IHasProgress)}.", this);
            }
        }

        private void OnEnable()
        {
            if (_progressSource == null)
            {
                return;
            }

            _progressSource.OnProgressChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_progressSource == null)
            {
                return;
            }

            _progressSource.OnProgressChanged -= Refresh;
        }

        private void Refresh()
        {
            bool active = _progressSource.IsInProgress;

            if (_barRoot != null)
            {
                _barRoot.SetActive(active);
            }

            if (active && _fillImage != null)
            {
                _fillImage.fillAmount = _progressSource.Progress;
            }
        }
    }
}
