using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Characters;
using YesChef.Ingredients;

namespace YesChef.UI
{
    /// <summary>
    /// Small HUD pill showing what the chef is carrying and whether it is ready to serve.
    /// Reads <see cref="PlayerController.OnHeldIngredientChanged"/>; the player never
    /// knows this exists.
    /// </summary>
    [DisallowMultipleComponent]
    public class HeldIngredientHudUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _stateText;

        [Header("State colours")]
        [SerializeField] private Color _readyColor = new(0.55f, 0.95f, 0.6f);
        [SerializeField] private Color _rawColor = new(1f, 0.72f, 0.4f);

        private PlayerController _player;
        private Ingredient _observed;

        private void Start()
        {
            _player = PlayerController.Instance;
            if (_player != null)
            {
                _player.OnHeldIngredientChanged += HandleHeldChanged;
                HandleHeldChanged(_player.HeldIngredient);
            }
            else
            {
                Refresh(null);
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnHeldIngredientChanged -= HandleHeldChanged;
            }

            Observe(null);
        }

        private void HandleHeldChanged(Ingredient ingredient)
        {
            Observe(ingredient);
            Refresh(ingredient);
        }

        private void HandleIngredientChanged() => Refresh(_observed);

        private void Observe(Ingredient ingredient)
        {
            if (_observed != null)
            {
                _observed.OnChanged -= HandleIngredientChanged;
            }

            _observed = ingredient;

            if (_observed != null)
            {
                _observed.OnChanged += HandleIngredientChanged;
            }
        }

        private void Refresh(Ingredient ingredient)
        {
            bool holding = ingredient != null && ingredient.Definition != null;

            if (_root != null)
            {
                _root.SetActive(holding);
            }

            if (!holding)
            {
                return;
            }

            IngredientSO definition = ingredient.Definition;

            if (_icon != null)
            {
                _icon.sprite = definition.Icon;
                _icon.enabled = definition.Icon != null;
            }

            if (_nameText != null)
            {
                _nameText.text = definition.DisplayName;
            }

            if (_stateText != null)
            {
                _stateText.text = definition.GetStateLabel(ingredient.State);
                _stateText.color = ingredient.IsReadyForDelivery ? _readyColor : _rawColor;
            }
        }
    }
}
