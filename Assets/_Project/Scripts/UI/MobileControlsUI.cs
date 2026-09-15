using UnityEngine;
using YesChef.Controls;
using YesChef.Core;

namespace YesChef.UI
{
    /// <summary>
    /// Shows the on-screen stick and interact button on phones and tablets, and only while
    /// a round is being played so they never sit on top of the menus.
    /// <para>
    /// The controls themselves are Input System on-screen components that emulate a
    /// gamepad, so the existing gamepad bindings drive the chef and no gameplay code knows
    /// the input came from a thumb.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class MobileControlsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _controlsRoot;

        [Tooltip("Show the touch controls in the editor even without a touchscreen, for layout work.")]
        [SerializeField] private bool _forceShowInEditor;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState state) => Refresh();

        private void Refresh()
        {
            if (_controlsRoot == null)
            {
                return;
            }

            bool wanted = GameInput.IsTouchDevice;
#if UNITY_EDITOR
            wanted |= _forceShowInEditor;
#endif
            bool playing = GameManager.Instance != null && GameManager.Instance.IsPlaying;
            _controlsRoot.SetActive(wanted && playing);
        }
    }
}
