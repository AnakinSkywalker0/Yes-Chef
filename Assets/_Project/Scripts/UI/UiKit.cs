using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YesChef.UI
{
    /// <summary>
    /// Small factory for the pieces the redesigned screens are made of: tickets, keycaps,
    /// chunky buttons, labelled icons. Used by the editor builder to author the scene and by
    /// runtime code (the controls table) to add rows, so both produce identical visuals.
    /// </summary>
    public static class UiKit
    {
        public enum ButtonStyle
        {
            Primary,
            Ghost,
            Paper,
        }

        // ---------- plumbing ----------

        public static RectTransform Node(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static void Stretch(RectTransform rt, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        public static LayoutElement Size(Component c, float width = -1f, float height = -1f, float flexWidth = -1f, float flexHeight = -1f)
        {
            if (!c.TryGetComponent(out LayoutElement le))
            {
                le = c.gameObject.AddComponent<LayoutElement>();
            }

            le.preferredWidth = width;
            le.minWidth = width >= 0f ? width : le.minWidth;
            le.preferredHeight = height;
            le.minHeight = height >= 0f ? height : le.minHeight;
            le.flexibleWidth = flexWidth;
            le.flexibleHeight = flexHeight;
            return le;
        }

        public static HorizontalLayoutGroup Row(Component c, float spacing, TextAnchor align = TextAnchor.MiddleCenter,
            RectOffset padding = null, bool expandWidth = false, bool expandHeight = false)
        {
            var layout = c.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = align;
            layout.padding = padding ?? new RectOffset();
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = expandWidth;
            layout.childForceExpandHeight = expandHeight;
            return layout;
        }

        public static VerticalLayoutGroup Column(Component c, float spacing, TextAnchor align = TextAnchor.UpperCenter,
            RectOffset padding = null, bool expandWidth = true, bool expandHeight = false)
        {
            var layout = c.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = align;
            layout.padding = padding ?? new RectOffset();
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = expandWidth;
            layout.childForceExpandHeight = expandHeight;
            return layout;
        }

        // ---------- surfaces ----------

        /// <summary>A rounded, 9-sliced rectangle. <paramref name="roundness"/> 1 = native corner, 2 = twice as round.</summary>
        public static Image Panel(UiTheme theme, Transform parent, string name, Color color, float roundness = 1f, bool raycast = false)
        {
            RectTransform rt = Node(name, parent);
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = theme.Rounded;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f / Mathf.Max(0.1f, roundness);
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static Shadow Edge(Graphic graphic, Color color, float depth)
        {
            var shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = new Vector2(0f, -depth);
            shadow.useGraphicAlpha = false;
            return shadow;
        }

        /// <summary>Paper order ticket: cream card with a hard offset edge.</summary>
        public static Image Ticket(UiTheme theme, Transform parent, string name, float roundness = 1.4f)
        {
            Image paper = Panel(theme, parent, name, UiTheme.Paper, roundness);
            Edge(paper, new Color(0f, 0f, 0f, 0.28f), 8f);
            return paper;
        }

        /// <summary>Dark translucent card used for the scoring and controls panels.</summary>
        public static Image DarkCard(UiTheme theme, Transform parent, string name)
        {
            Image card = Panel(theme, parent, name, new Color(UiTheme.Ink.r, UiTheme.Ink.g, UiTheme.Ink.b, 0.9f), 1.4f);
            Edge(card, new Color(0f, 0f, 0f, 0.3f), 6f);
            return card;
        }

        // ---------- text ----------

        public static TextMeshProUGUI Text(UiTheme theme, Transform parent, string name, string text, bool display, float size,
            Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, float spacing = 0f, bool bold = false)
        {
            RectTransform rt = Node(name, parent);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = display ? theme.DisplayFont : theme.BodyFont;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = align;
            label.characterSpacing = spacing;
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.richText = true;
            return label;
        }

        public static TextOutline Outline(TMP_Text label, float width = 0.2f, bool dropShadow = true)
        {
            TextOutline outline = label.gameObject.AddComponent<TextOutline>();
            outline.Configure(width, new Color32(0x15, 0x12, 0x2C, 255), dropShadow, new Color32(0x15, 0x12, 0x2C, 255));
            return outline;
        }

        public static Image Icon(UiTheme theme, Transform parent, string name, Sprite sprite, float size, Color? tint = null)
        {
            RectTransform rt = Node(name, parent);
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = tint ?? Color.white;
            Size(rt, size, size);
            return image;
        }

        // ---------- controls ----------

        public static Button Button(UiTheme theme, Transform parent, string name, string label, ButtonStyle style, float height,
            float fontSize, Sprite icon = null, TextAnchor align = TextAnchor.MiddleCenter, int sidePadding = 56)
        {
            Color face, edge, text;
            switch (style)
            {
                case ButtonStyle.Primary:
                    face = UiTheme.Butter;
                    edge = UiTheme.ButterDeep;
                    text = new Color32(0x3B, 0x2A, 0x00, 255);
                    break;
                case ButtonStyle.Paper:
                    face = new Color32(0xE9, 0xDF, 0xC6, 255);
                    edge = new Color32(0xB9, 0xA9, 0x8A, 255);
                    text = UiTheme.PaperInk;
                    break;
                default:
                    face = new Color(UiTheme.Cream.r, UiTheme.Cream.g, UiTheme.Cream.b, 0.12f);
                    edge = new Color(0f, 0f, 0f, 0.4f);
                    text = UiTheme.Cream;
                    break;
            }

            Image bg = Panel(theme, parent, name, face, 1.8f, raycast: true);
            Edge(bg, edge, style == ButtonStyle.Primary ? 10f : 7f);
            Size(bg, -1f, height);
            Row(bg, 30f, align, new RectOffset(sidePadding, sidePadding, 0, 0));

            var button = bg.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            button.transition = Selectable.Transition.None; // MenuButtonFx owns the feedback
            bg.gameObject.AddComponent<MenuButtonFx>();

            if (icon != null)
            {
                Icon(theme, bg.transform, "Icon", icon, fontSize * 0.8f, text);
            }

            TextMeshProUGUI caption = Text(theme, bg.transform, "Label", label.ToUpperInvariant(), true, fontSize, text,
                TextAlignmentOptions.Center, spacing: 4f);
            caption.raycastTarget = false;
            return button;
        }

        public static RectTransform KeyCap(UiTheme theme, Transform parent, string key)
        {
            Image cap = Panel(theme, parent, "Key_" + key, UiTheme.KeyCap, 1.2f);
            Edge(cap, UiTheme.KeyCapEdge, 5f);
            Row(cap, 0f, TextAnchor.MiddleCenter, new RectOffset(16, 16, 0, 4));
            LayoutElement size = Size(cap, -1f, 54f);
            size.minWidth = 54f;
            TextMeshProUGUI text = Text(theme, cap.transform, "Text", key, false, 28f, UiTheme.Cream, bold: true);
            text.raycastTarget = false;
            return (RectTransform)cap.transform;
        }
    }
}
