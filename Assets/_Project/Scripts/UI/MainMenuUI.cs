using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Scoring;

namespace YesChef.UI
{
    /// <summary>
    /// The front door. Two panels share one screen: the menu (play, how to play, quit,
    /// best score) and the briefing that always precedes service - how the kitchen works
    /// plus the live control bindings - so the player never starts a round without having
    /// seen the controls.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _screenRoot;

        [Header("Menu")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _howToPlayButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private TMP_Text _bestScoreText;

        [Header("Briefing")]
        [SerializeField] private GameObject _briefingPanel;
        [SerializeField] private Button _startServiceButton;
        [SerializeField] private Button _backButton;

        private void Start()
        {
            Bind(_playButton, ShowBriefing);
            Bind(_howToPlayButton, ShowBriefing);
            Bind(_quitButton, HandleQuitClicked);
            Bind(_startServiceButton, HandleStartServiceClicked);
            Bind(_backButton, ShowMenu);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnHighScoreChanged += HandleHighScoreChanged;
                HandleHighScoreChanged(ScoreManager.Instance.HighScore);
            }

            ShowMenu();
            RefreshVisibility();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnHighScoreChanged -= HandleHighScoreChanged;
            }
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private void ShowMenu() => SetPanels(menu: true);

        private void ShowBriefing() => SetPanels(menu: false);

        private void SetPanels(bool menu)
        {
            if (_menuPanel != null)
            {
                _menuPanel.SetActive(menu);
            }

            if (_briefingPanel != null)
            {
                _briefingPanel.SetActive(!menu);
            }
        }

        private void HandleStateChanged(GameState state) => RefreshVisibility();

        private void HandleHighScoreChanged(int highScore)
        {
            if (_bestScoreText != null)
            {
                _bestScoreText.text = highScore.ToString();
            }
        }

        private void RefreshVisibility()
        {
            if (_screenRoot == null || GameManager.Instance == null)
            {
                return;
            }

            _screenRoot.SetActive(GameManager.Instance.State == GameState.WaitingToStart);
        }

        private void HandleStartServiceClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartRound();
            }
        }

        private void HandleQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
    }
}
