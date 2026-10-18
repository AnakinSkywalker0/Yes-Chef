using TMPro;
using UnityEngine;

namespace YesChef.UI
{
    /// <summary>
    /// Applies an SDF outline and a soft drop shadow to a TMP label at runtime. Material
    /// instances are not serialised, so this has to happen when the object wakes up rather
    /// than when the scene is authored.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    public class TextOutline : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _width = 0.2f;
        [SerializeField] private Color _color = new Color32(0x15, 0x12, 0x2C, 255);
        [SerializeField] private bool _dropShadow = true;
        [SerializeField] private Color _shadowColor = new Color32(0x15, 0x12, 0x2C, 255);

        private void OnEnable() => Apply();

        public void Configure(float width, Color color, bool dropShadow, Color shadowColor)
        {
            _width = width;
            _color = color;
            _dropShadow = dropShadow;
            _shadowColor = shadowColor;
        }

        private void Apply()
        {
            if (!Application.isPlaying)
            {
                return; // material instances are runtime-only; touching one at edit time leaks it into the scene
            }

            var text = GetComponent<TMP_Text>();
            Material material = text.fontMaterial;

            text.outlineWidth = _width;
            text.outlineColor = _color;

            if (!_dropShadow)
            {
                return;
            }

            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor(ShaderUtilities.ID_UnderlayColor, _shadowColor);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0f);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.9f);
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.35f);
            material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.1f);
        }
    }
}
