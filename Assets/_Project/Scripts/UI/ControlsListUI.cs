using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YesChef.Controls;

namespace YesChef.UI
{
    /// <summary>
    /// Builds the controls summary from the live Input System bindings, so rebinding the
    /// asset (or adding a gamepad scheme) updates the start screen with no UI work.
    /// <para>
    /// One column per action: its name, the primary keyboard keys as caps, and any other
    /// ways to do it (arrow keys, gamepad) as a single quiet line underneath.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class ControlsListUI : MonoBehaviour
    {
        [SerializeField] private UiTheme _theme;

        [Header("Action names")]
        [SerializeField] private string _moveName = "MOVE";
        [SerializeField] private string _interactName = "USE STATION";
        [SerializeField] private string _pauseName = "PAUSE";

        private readonly List<GameObject> _columns = new();

        private void Start() => Rebuild();

        public void Rebuild()
        {
            Clear();

            GameInput input = GameInput.Instance;
            if (input == null || _theme == null)
            {
                return;
            }

            AddColumn(_moveName, input.GetMoveBindings());
            AddColumn(_interactName, input.GetInteractBindings());
            AddColumn(_pauseName, input.GetPauseBindings());
        }

        private void AddColumn(string actionName, IReadOnlyList<GameInput.BindingLabel> bindings)
        {
            RectTransform column = UiKit.Node(actionName, transform);
            UiKit.Column(column, 14f, TextAnchor.MiddleCenter, expandWidth: false);
            UiKit.Size(column, -1f, -1f, flexWidth: 1f);
            _columns.Add(column.gameObject);

            TMP_Text title = UiKit.Text(_theme, column, "Name", actionName, false, 22f, UiTheme.Muted, spacing: 8f, bold: true);
            UiKit.Size(title, -1f, 28f);

            // Primary keys: the first keyboard binding, or the first binding of any kind.
            int primary = IndexOfPrimary(bindings);
            RectTransform caps = UiKit.Node("Keys", column);
            UiKit.Row(caps, 10f, TextAnchor.MiddleCenter);
            UiKit.Size(caps, -1f, 56f);
            if (primary >= 0)
            {
                foreach (string key in KeyLabels.SplitIntoCaps(bindings[primary].Text))
                {
                    UiKit.KeyCap(_theme, caps, key);
                }
            }

            // Everything else, kept to one muted line.
            var others = new List<string>();
            for (int i = 0; i < bindings.Count; i++)
            {
                if (i != primary)
                {
                    others.Add(string.Join(" ", KeyLabels.SplitIntoCaps(bindings[i].Text)));
                }
            }

            string alt = others.Count == 0 ? string.Empty : "or " + string.Join("  ·  ", others);
            TMP_Text altText = UiKit.Text(_theme, column, "Alternatives", alt, false, 26f, UiTheme.Muted, bold: true);
            UiKit.Size(altText, -1f, 34f);
        }

        private static int IndexOfPrimary(IReadOnlyList<GameInput.BindingLabel> bindings)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (!bindings[i].IsGamepad)
                {
                    return i;
                }
            }

            return bindings.Count > 0 ? 0 : -1;
        }

        private void Clear()
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                if (_columns[i] != null)
                {
                    Destroy(_columns[i]);
                }
            }

            _columns.Clear();
        }
    }
}
