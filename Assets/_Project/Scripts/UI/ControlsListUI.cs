using System.Collections.Generic;
using UnityEngine;
using YesChef.Controls;

namespace YesChef.UI
{
    /// <summary>
    /// Builds the controls list from the live Input System bindings, so rebinding the
    /// asset (or adding a gamepad scheme) updates the start screen with no UI work.
    /// </summary>
    [DisallowMultipleComponent]
    public class ControlsListUI : MonoBehaviour
    {
        [SerializeField] private ControlRowUI _rowTemplate;

        [Header("Descriptions")]
        [SerializeField] private string _moveDescription = "Move around the kitchen";
        [SerializeField] private string _interactDescription = "Use the station you're facing";
        [SerializeField] private string _pauseDescription = "Pause";

        private readonly List<GameObject> _rows = new();

        private void Start() => Rebuild();

        public void Rebuild()
        {
            Clear();

            GameInput input = GameInput.Instance;
            if (input == null || _rowTemplate == null)
            {
                return;
            }

            AddRow(input.GetMoveBindingLabels(), _moveDescription);
            AddRow(input.GetInteractBindingLabels(), _interactDescription);
            AddRow(input.GetPauseBindingLabels(), _pauseDescription);
        }

        private void AddRow(IReadOnlyList<string> bindingLabels, string description)
        {
            ControlRowUI row = Instantiate(_rowTemplate, _rowTemplate.transform.parent);
            row.gameObject.SetActive(true);
            row.Bind(bindingLabels, description);
            _rows.Add(row.gameObject);
        }

        private void Clear()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null)
                {
                    Destroy(_rows[i]);
                }
            }

            _rows.Clear();
        }
    }
}
