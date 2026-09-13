using UnityEngine;

namespace YesChef.Ingredients
{
    /// <summary>
    /// Authored definition of an ingredient type. Adding a fourth ingredient to the game is
    /// a matter of creating one of these assets and dropping it into the refrigerator and
    /// the order pool - no code changes required.
    /// </summary>
    [CreateAssetMenu(fileName = "Ingredient", menuName = "Yes Chef/Ingredient")]
    public class IngredientSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "Ingredient";
        [Tooltip("Fallback glyph for UI when no icon is assigned.")]
        [SerializeField] private string _shortLabel = "?";
        [SerializeField] private Sprite _icon;

        [Header("Rules")]
        [SerializeField, Min(0)] private int _scoreValue = 10;
        [SerializeField] private IngredientProcess _requiredProcess = IngredientProcess.None;
        [SerializeField] private Ingredient _prefab;

        [Header("Visuals")]
        [SerializeField] private Color _rawColor = Color.white;
        [SerializeField] private Color _preparedColor = Color.white;
        [SerializeField] private Vector3 _rawScale = Vector3.one;
        [SerializeField] private Vector3 _preparedScale = Vector3.one;

        public string DisplayName => _displayName;
        public string ShortLabel => _shortLabel;
        public Sprite Icon => _icon;
        public int ScoreValue => _scoreValue;
        public IngredientProcess RequiredProcess => _requiredProcess;
        public Ingredient Prefab => _prefab;

        /// <summary>True when the ingredient has to visit a station before delivery.</summary>
        public bool NeedsPreparation => _requiredProcess != IngredientProcess.None;

        /// <summary>Colour used on order tickets and other flat UI.</summary>
        public Color TicketColor => _preparedColor;

        public Color GetColor(IngredientState state) =>
            state == IngredientState.Prepared ? _preparedColor : _rawColor;

        public Vector3 GetScale(IngredientState state) =>
            state == IngredientState.Prepared ? _preparedScale : _rawScale;

        /// <summary>Human-readable state, e.g. "Raw" or "Chopped".</summary>
        public string GetStateLabel(IngredientState state)
        {
            if (!NeedsPreparation)
            {
                return "Ready";
            }

            if (state == IngredientState.Raw)
            {
                return "Raw";
            }

            return _requiredProcess == IngredientProcess.Chop ? "Chopped" : "Cooked";
        }
    }
}
