using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;

namespace YesChef.UI
{
    /// <summary>Pause overlay. Works while the timescale is frozen because UI is timescale independent.</summary>
    [DisallowMultipleComponent]
    public class PauseScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject _screenRoot;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(HandleResumeClicked);
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(HandleRestartClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(HandleQuitClicked);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            RefreshVisibility();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState state) => RefreshVisibility();

        private void RefreshVisibility()
        {
            if (_screenRoot == null || GameManager.Instance == null)
            {
                return;
            }

            _screenRoot.SetActive(GameManager.Instance.State == GameState.Paused);
        }

        private void HandleResumeClicked() => GameManager.Instance?.Resume();

        private void HandleRestartClicked() => GameManager.Instance?.Restart();

        private void HandleQuitClicked() => GameManager.Instance?.QuitGame();
    }
}
