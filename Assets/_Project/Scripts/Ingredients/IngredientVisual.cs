using UnityEngine;

namespace YesChef.Ingredients
{
    /// <summary>
    /// Presentation half of an ingredient. Reads state from <see cref="Ingredient"/> and
    /// applies the colour and scale authored on the <see cref="IngredientSO"/> - this is
    /// what makes chopped vegetables and cooked meat readable at a glance.
    /// <para>
    /// Colour goes through a <see cref="MaterialPropertyBlock"/> so every ingredient can
    /// share one material without leaking a material instance per object.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class IngredientVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Ingredient _ingredient;
        [SerializeField] private Transform _meshRoot;
        [SerializeField] private Renderer _meshRenderer;

        private MaterialPropertyBlock _propertyBlock;

        private void OnEnable()
        {
            if (_ingredient == null)
            {
                Debug.LogError($"{nameof(IngredientVisual)} is missing its {nameof(Ingredient)} reference.", this);
                return;
            }

            _ingredient.OnChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_ingredient == null)
            {
                return;
            }

            _ingredient.OnChanged -= Refresh;
        }

        private void Refresh()
        {
            IngredientSO definition = _ingredient != null ? _ingredient.Definition : null;
            if (definition == null || _meshRenderer == null)
            {
                return;
            }

            IngredientState state = _ingredient.State;

            if (_meshRoot != null)
            {
                _meshRoot.localScale = definition.GetScale(state);
            }

            _propertyBlock ??= new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, definition.GetColor(state));
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
