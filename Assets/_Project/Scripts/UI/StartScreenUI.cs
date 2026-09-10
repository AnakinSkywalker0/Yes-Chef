using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Controls;
using YesChef.Core;

namespace YesChef.UI
{
    /// <summary>
    /// The opening window. Lists the controls - read straight from the bound Input System
    /// actions, so rebinding the asset updates this screen automatically - and starts the
    /// round on click.
    /// </summary>
    [DisallowMultipleComponent]
    public class StartScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject _screenRoot;
        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _controlsText;

        private void Start()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(HandlePlayClicked);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            BuildControlsText();
            RefreshVisibility();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void BuildControlsText()
        {
            if (_controlsText == null || GameInput.Instance == null)
            {
                return;
            }

            GameInput input = GameInput.Instance;

            _controlsText.text =
                $"<b>{input.GetMoveDisplayString()}</b>    Move around the kitchen\n" +
                $"<b>{input.GetInteractDisplayString()}</b>    Use the station you are facing\n" +
                $"<b>{input.GetPauseDisplayString()}</b>    Pause";
        }

        private void HandleStateChanged(GameState state) => RefreshVisibility();

        private void RefreshVisibility()
        {
            if (_screenRoot == null || GameManager.Instance == null)
            {
                return;
            }

            _screenRoot.SetActive(GameManager.Instance.State == GameState.WaitingToStart);
        }

        private void HandlePlayClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartRound();
            }
        }
    }
}
