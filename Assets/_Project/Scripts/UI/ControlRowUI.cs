using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace YesChef.UI
{
    /// <summary>
    /// One line on the controls list: a run of key-cap chips followed by what they do.
    /// <para>
    /// Single-key composites such as "W/A/S/D" become one cap per key. Composites made of
    /// words ("Up/Left/Down/Right") would sprawl into a dozen caps, so they collapse to a
    /// single readable cap - the arrow keys get a friendly name, anything else is joined.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class ControlRowUI : MonoBehaviour
    {
        private const string ArrowKeysLabel = "Arrow Keys";

        [SerializeField] private RectTransform _chipContainer;
        [SerializeField] private GameObject _chipTemplate;
        [SerializeField] private GameObject _separatorTemplate;
        [SerializeField] private TMP_Text _actionLabel;

        public void Bind(IReadOnlyList<string> bindingLabels, string action)
        {
            if (_actionLabel != null)
            {
                _actionLabel.text = action;
            }

            for (int i = 0; i < bindingLabels.Count; i++)
            {
                if (i > 0)
                {
                    Clone(_separatorTemplate);
                }

                foreach (string key in SplitIntoCaps(bindingLabels[i]))
                {
                    GameObject chip = Clone(_chipTemplate);
                    TMP_Text text = chip != null ? chip.GetComponentInChildren<TMP_Text>(true) : null;
                    if (text != null)
                    {
                        text.text = key;
                    }
                }
            }
        }

        private static IEnumerable<string> SplitIntoCaps(string label)
        {
            string[] parts = label.Split('/');

            if (parts.Length == 1)
            {
                yield return label.Trim();
                yield break;
            }

            bool allSingleKeys = true;
            bool allArrows = true;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                allSingleKeys &= part.Length == 1;
                allArrows &= part is "Up" or "Down" or "Left" or "Right";
            }

            if (allSingleKeys)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    yield return parts[i].Trim();
                }

                yield break;
            }

            yield return allArrows ? ArrowKeysLabel : string.Join(" / ", parts).Trim();
        }

        private GameObject Clone(GameObject template)
        {
            if (template == null || _chipContainer == null)
            {
                return null;
            }

            GameObject clone = Instantiate(template, _chipContainer);
            clone.SetActive(true);
            return clone;
        }
    }
}
