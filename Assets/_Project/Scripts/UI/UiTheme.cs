using TMPro;
using UnityEngine;

namespace YesChef.UI
{
    /// <summary>
    /// The shared look of every screen: fonts, sprites and the colour palette. Screens are
    /// built from this one asset so a restyle is a change here, not a hunt through scenes.
    /// </summary>
    [CreateAssetMenu(fileName = "UiTheme", menuName = "Yes Chef/UI Theme")]
    public class UiTheme : ScriptableObject
    {
        [Header("Fonts")]
        public TMP_FontAsset DisplayFont;
        public TMP_FontAsset BodyFont;

        [Header("Shapes")]
        [Tooltip("9-sliced rounded rectangle.")] public Sprite Rounded;
        public Sprite Circle;
        public Sprite Gradient;

        [Header("Icons")]
        public Sprite Vegetable;
        public Sprite Cheese;
        public Sprite Meat;
        public Sprite Bell;
        public Sprite Knife;
        public Sprite Fridge;
        public Sprite Flame;
        public Sprite Star;
        public Sprite Play;
        public Sprite Check;

        // Palette. Kept in code so it is searchable and identical across screens.
        public static readonly Color Ink = new Color32(0x1A, 0x18, 0x30, 255);
        public static readonly Color InkSoft = new Color32(0x26, 0x23, 0x4A, 255);
        public static readonly Color Tomato = new Color32(0xFF, 0x5A, 0x3C, 255);
        public static readonly Color TomatoDeep = new Color32(0xA5, 0x2D, 0x16, 255);
        public static readonly Color Butter = new Color32(0xFF, 0xC9, 0x3C, 255);
        public static readonly Color ButterDeep = new Color32(0xC9, 0x8A, 0x12, 255);
        public static readonly Color Leaf = new Color32(0x6B, 0xD3, 0x6A, 255);
        public static readonly Color Cream = new Color32(0xFF, 0xF3, 0xD9, 255);
        public static readonly Color Paper = new Color32(0xFD, 0xF6, 0xE3, 255);
        public static readonly Color PaperInk = new Color32(0x3A, 0x2F, 0x2A, 255);
        public static readonly Color PaperMuted = new Color32(0x8A, 0x7A, 0x6A, 255);
        public static readonly Color Muted = new Color32(0x9A, 0x97, 0xC2, 255);
        public static readonly Color KeyCap = new Color32(0x44, 0x40, 0x72, 255);
        public static readonly Color KeyCapEdge = new Color32(0x1C, 0x1A, 0x38, 255);
        public static readonly Color StarOff = new Color32(0xCF, 0xC4, 0xAD, 255);
    }
}
