using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Ingredients;

namespace YesChef.UI
{
    /// <summary>One requirement on an order ticket: a colour swatch, a label and a tick.</summary>
    [DisallowMultipleComponent]
    public class OrderIngredientIconUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _swatch;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private GameObject _deliveredTick;

        [Tooltip("How far a delivered swatch is dimmed towards black.")]
        [SerializeField, Range(0f, 1f)] private float _deliveredDim = 0.4f;

        public void Show(IngredientSO ingredient, bool delivered)
        {
            if (_root != null)
            {
                _root.SetActive(true);
            }

            if (ingredient == null)
            {
                return;
            }

            if (_swatch != null)
            {
                Color color = ingredient.TicketColor;
                _swatch.color = delivered ? Color.Lerp(Color.black, color, _deliveredDim) : color;
            }

            if (_label != null)
            {
                _label.text = ingredient.ShortLabel;
            }

            if (_deliveredTick != null)
            {
                _deliveredTick.SetActive(delivered);
            }
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }
    }
}
