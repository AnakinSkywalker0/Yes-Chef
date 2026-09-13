using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Ingredients;

namespace YesChef.UI
{
    /// <summary>
    /// One requirement on an order ticket: the ingredient's icon on a tinted tile, dimmed
    /// with a tick once delivered. Falls back to the short label if an icon is missing.
    /// </summary>
    [DisallowMultipleComponent]
    public class OrderIngredientIconUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _tile;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _fallbackLabel;
        [SerializeField] private GameObject _deliveredTick;

        [Header("Delivered look")]
        [SerializeField, Range(0f, 1f)] private float _deliveredTileAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _deliveredIconAlpha = 0.35f;

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

            if (_tile != null)
            {
                Color tint = ingredient.TicketColor;
                tint.a = delivered ? _deliveredTileAlpha : 0.9f;
                _tile.color = tint;
            }

            bool hasIcon = ingredient.Icon != null;

            if (_icon != null)
            {
                _icon.gameObject.SetActive(hasIcon);
                _icon.sprite = ingredient.Icon;
                _icon.color = new Color(1f, 1f, 1f, delivered ? _deliveredIconAlpha : 1f);
            }

            if (_fallbackLabel != null)
            {
                _fallbackLabel.gameObject.SetActive(!hasIcon);
                _fallbackLabel.text = ingredient.ShortLabel;
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
