using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using YesChef.UI;

namespace YesChef.Editor
{
    /// <summary>
    /// Authors the redesigned main menu, briefing and results screens into the Kitchen
    /// scene from <see cref="UiKit"/>. Replaces whatever is under those screens' roots and
    /// rewires the existing behaviour components, so the scripts that drive the game are
    /// untouched. Idempotent: skips when the redesign marker object is present.
    /// </summary>
    [InitializeOnLoad]
    public static class UiRedesignBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Kitchen.unity";
        private const string ThemePath = "Assets/_Project/Art/UI/UiTheme.asset";
        private const string BriefingMarker = "ContentV2"; // bumped whenever the briefing layout changes so the scene rebuilds

        static UiRedesignBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && SceneManager.GetActiveScene().path == ScenePath)
                {
                    Build(force: false);
                }
            };
        }

        [MenuItem("Yes Chef/Rebuild Redesigned UI")]
        public static void Rebuild() => Build(force: true);

        private static void Build(bool force)
        {
            var menu = Object.FindFirstObjectByType<MainMenuUI>(FindObjectsInactive.Include);
            var gameOver = Object.FindFirstObjectByType<GameOverScreenUI>(FindObjectsInactive.Include);
            if (menu == null || gameOver == null)
            {
                return;
            }

            var menuSo = new SerializedObject(menu);
            var screenRoot = ((GameObject)menuSo.FindProperty("_screenRoot").objectReferenceValue);
            if (screenRoot == null)
            {
                return;
            }

            if (!force && screenRoot.transform.Find("BriefingPanel/" + BriefingMarker) != null)
            {
                return;
            }

            UiTheme theme = EnsureTheme();
            if (theme == null)
            {
                return;
            }

            BuildMainMenu(menu, menuSo, screenRoot, theme);
            BuildResults(gameOver, theme);

            EditorSceneManager.MarkSceneDirty(screenRoot.scene);
            EditorSceneManager.SaveScene(screenRoot.scene);
            Debug.Log("[UI] Redesigned menu, briefing and results screens built.");
        }

        // ------------------------------------------------------------------ assets

        private static UiTheme EnsureTheme()
        {
            var display = EnsureFont("Assets/_Project/Art/Fonts/TitanOne-Regular.ttf", "Assets/_Project/Art/Fonts/TitanOne SDF.asset");
            var body = EnsureFont("Assets/_Project/Art/Fonts/VarelaRound-Regular.ttf", "Assets/_Project/Art/Fonts/VarelaRound SDF.asset");
            if (display == null || body == null)
            {
                Debug.LogWarning("[UI] Fonts missing in Assets/_Project/Art/Fonts; redesign skipped.");
                return null;
            }

            var theme = AssetDatabase.LoadAssetAtPath<UiTheme>(ThemePath);
            if (theme == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ThemePath));
                theme = ScriptableObject.CreateInstance<UiTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            const string icons = "Assets/_Project/Art/Icons/";
            theme.DisplayFont = display;
            theme.BodyFont = body;
            theme.Rounded = Load(icons + "ui_rounded.png");
            theme.Circle = Load(icons + "ui_circle.png");
            theme.Gradient = Load(icons + "ui_gradient_h.png");
            theme.Vegetable = Load(icons + "icon_vegetable.png");
            theme.Cheese = Load(icons + "icon_cheese.png");
            theme.Meat = Load(icons + "icon_meat.png");
            theme.Bell = Load(icons + "icon_bell.png");
            theme.Knife = Load(icons + "icon_knife.png");
            theme.Fridge = Load(icons + "icon_fridge.png");
            theme.Flame = Load(icons + "icon_flame.png");
            theme.Star = Load(icons + "icon_star.png");
            theme.Play = Load(icons + "icon_play.png");
            theme.Check = Load(icons + "icon_check.png");
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            return theme;
        }

        private static Sprite Load(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        private static TMP_FontAsset EnsureFont(string ttfPath, string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null)
            {
                return null;
            }

            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                AtlasPopulationMode.Dynamic, true);
            asset.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);
            asset.material.name = asset.name + " Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            foreach (Texture2D atlas in asset.atlasTextures)
            {
                atlas.name = asset.name + " Atlas";
                AssetDatabase.AddObjectToAsset(atlas, asset);
            }

            var ascii = new System.Text.StringBuilder();
            for (char c = ' '; c <= '~'; c++)
            {
                ascii.Append(c);
            }

            asset.TryAddCharacters(ascii.ToString(), out _);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        // ------------------------------------------------------------------ main menu + briefing

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static void BuildMainMenu(MainMenuUI menu, SerializedObject menuSo, GameObject screenRoot, UiTheme theme)
        {
            // The screen root keeps its own graphic only to swallow clicks aimed at the game.
            if (screenRoot.TryGetComponent(out Image rootImage))
            {
                rootImage.color = Color.clear;
                rootImage.raycastTarget = true;
            }

            foreach (var old in screenRoot.GetComponents<MainMenuStyler>())
            {
                Object.DestroyImmediate(old);
            }

            ClearChildren(screenRoot.transform);

            RectTransform menuPanel = BuildMenuPanel(screenRoot.transform, theme,
                out Button play, out Button howTo, out Button quit, out TMP_Text best, out MainMenuStyler styler);
            RectTransform briefing = BuildBriefingPanel(screenRoot.transform, theme, out Button back, out Button start);

            menuSo.FindProperty("_menuPanel").objectReferenceValue = menuPanel.gameObject;
            menuSo.FindProperty("_playButton").objectReferenceValue = play;
            menuSo.FindProperty("_howToPlayButton").objectReferenceValue = howTo;
            menuSo.FindProperty("_quitButton").objectReferenceValue = quit;
            menuSo.FindProperty("_bestScoreText").objectReferenceValue = best;
            menuSo.FindProperty("_briefingPanel").objectReferenceValue = briefing.gameObject;
            menuSo.FindProperty("_startServiceButton").objectReferenceValue = start;
            menuSo.FindProperty("_backButton").objectReferenceValue = back;
            menuSo.ApplyModifiedPropertiesWithoutUndo();

            // The styler lives on the screen root so its intro replays whenever the menu reappears.
            MainMenuStyler live = screenRoot.AddComponent<MainMenuStyler>();
            CopyStylerRefs(styler, live);
            Object.DestroyImmediate(styler);
        }

        private static void CopyStylerRefs(MainMenuStyler from, MainMenuStyler to)
        {
            var src = new SerializedObject(from);
            var dst = new SerializedObject(to);
            foreach (string field in new[] { "_logo", "_bestTicket", "_buttonColumn" })
            {
                dst.FindProperty(field).objectReferenceValue = src.FindProperty(field).objectReferenceValue;
            }

            foreach (string field in new[] { "_buttons", "_floaters" })
            {
                SerializedProperty s = src.FindProperty(field);
                SerializedProperty d = dst.FindProperty(field);
                d.arraySize = s.arraySize;
                for (int i = 0; i < s.arraySize; i++)
                {
                    d.GetArrayElementAtIndex(i).objectReferenceValue = s.GetArrayElementAtIndex(i).objectReferenceValue;
                }
            }

            dst.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform BuildMenuPanel(Transform root, UiTheme theme, out Button play, out Button howTo,
            out Button quit, out TMP_Text bestValue, out MainMenuStyler styler)
        {
            RectTransform panel = UiKit.Node("MenuPanel", root);
            UiKit.Stretch(panel);

            // Left-hand scrim keeps the logo and buttons legible over the bright kitchen.
            RectTransform shade = UiKit.Node("Shade", panel);
            var shadeImage = shade.gameObject.AddComponent<Image>();
            shadeImage.sprite = theme.Gradient;
            shadeImage.color = Color.white;
            shadeImage.raycastTarget = false;
            shade.anchorMin = Vector2.zero;
            shade.anchorMax = new Vector2(0.7f, 1f);
            shade.offsetMin = shade.offsetMax = Vector2.zero;

            // Floating ingredients on the right.
            var floaters = new[]
            {
                MakeFloater(theme, panel, "Floater_Vegetable", theme.Vegetable, new Vector2(0.60f, 0.84f), 150f),
                MakeFloater(theme, panel, "Floater_Meat", theme.Meat, new Vector2(0.76f, 0.70f), 130f),
                MakeFloater(theme, panel, "Floater_Cheese", theme.Cheese, new Vector2(0.90f, 0.46f), 120f),
                MakeFloater(theme, panel, "Floater_Flame", theme.Flame, new Vector2(0.56f, 0.30f), 110f),
            };

            // Logo.
            RectTransform logo = UiKit.Node("Logo", panel);
            UiKit.Place(logo, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, -80f), new Vector2(900f, 560f));

            Image eyebrow = UiKit.Panel(theme, logo, "Eyebrow", UiTheme.Tomato, 1.2f);
            UiKit.Edge(eyebrow, UiTheme.TomatoDeep, 6f);
            UiKit.Place((RectTransform)eyebrow.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(590f, 62f));
            eyebrow.transform.localRotation = Quaternion.Euler(0f, 0f, 2f);
            TMP_Text eyebrowText = UiKit.Text(theme, eyebrow.transform, "Text", "A ONE-MAN KITCHEN ARCADE", false, 26f, Color.white,
                spacing: 5f, bold: true);
            UiKit.Stretch(eyebrowText.rectTransform);

            TMP_Text title = UiKit.Text(theme, logo, "Title", "YES\n<color=#FFC93C>CHEF!</color>", true, 215f, Color.white,
                TextAlignmentOptions.TopLeft);
            title.lineSpacing = -34f;
            UiKit.Outline(title, 0.28f);
            UiKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -90f), new Vector2(900f, 470f));

            // Buttons.
            RectTransform column = UiKit.Node("Buttons", panel);
            UiKit.Place(column, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, -640f), new Vector2(640f, 372f));
            UiKit.Column(column, 20f, TextAnchor.UpperLeft, expandWidth: true);

            play = UiKit.Button(theme, column, "PlayButton", "Play", UiKit.ButtonStyle.Primary, 132f, 62f, theme.Play,
                TextAnchor.MiddleLeft);
            howTo = UiKit.Button(theme, column, "HowToPlayButton", "How to play", UiKit.ButtonStyle.Ghost, 100f, 42f, null,
                TextAnchor.MiddleLeft);
            quit = UiKit.Button(theme, column, "QuitButton", "Quit", UiKit.ButtonStyle.Ghost, 100f, 42f, null,
                TextAnchor.MiddleLeft);
            play.GetComponent<MenuButtonFx>().SetIdlePulse(0.018f);

            // Best-shift ticket.
            Image ticket = UiKit.Ticket(theme, panel, "BestTicket");
            UiKit.Place((RectTransform)ticket.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-110f, -90f), new Vector2(340f, 330f));
            UiKit.Column(ticket, 6f, TextAnchor.MiddleCenter, new RectOffset(24, 24, 28, 28));
            TMP_Text bestLabel = UiKit.Text(theme, ticket.transform, "Label", "BEST SHIFT", false, 24f, UiTheme.PaperMuted, spacing: 8f, bold: true);
            UiKit.Size(bestLabel, -1f, 34f);
            bestValue = UiKit.Text(theme, ticket.transform, "Value", "0", true, 130f, UiTheme.Tomato);
            UiKit.Size(bestValue, -1f, 150f);
            RectTransform starRow = UiKit.Node("Stars", ticket.transform);
            UiKit.Row(starRow, 8f, TextAnchor.MiddleCenter);
            UiKit.Size(starRow, -1f, 62f);
            var stars = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                stars[i] = UiKit.Icon(theme, starRow, "Star" + i, theme.Star, 58f, UiTheme.StarOff);
            }

            AttachStars(ticket.gameObject, stars, StarRatingUI.Source.HighScore);

            // Footer.
            TMP_Text footer = UiKit.Text(theme, panel, "Footer", "KEYBOARD  ·  GAMEPAD", false, 24f, UiTheme.Muted,
                TextAlignmentOptions.BottomLeft, spacing: 6f, bold: true);
            UiKit.Place(footer.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(120f, 40f), new Vector2(700f, 40f));

            // Motion.
            styler = panel.gameObject.AddComponent<MainMenuStyler>();
            var so = new SerializedObject(styler);
            so.FindProperty("_logo").objectReferenceValue = logo;
            so.FindProperty("_bestTicket").objectReferenceValue = ticket.transform;
            so.FindProperty("_buttonColumn").objectReferenceValue = column;
            SetArray(so.FindProperty("_buttons"), new Object[] { play.GetComponent<MenuButtonFx>(),
                howTo.GetComponent<MenuButtonFx>(), quit.GetComponent<MenuButtonFx>() });
            SetArray(so.FindProperty("_floaters"), floaters);
            so.ApplyModifiedPropertiesWithoutUndo();

            return panel;
        }

        private static RectTransform MakeFloater(UiTheme theme, Transform parent, string name, Sprite sprite, Vector2 anchor, float size)
        {
            Image icon = UiKit.Icon(theme, parent, name, sprite, size);
            var rt = (RectTransform)icon.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;
            UiKit.Edge(icon, new Color(0f, 0f, 0f, 0.3f), 8f);
            return rt;
        }

        private static void SetArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void AttachStars(GameObject host, Image[] stars, StarRatingUI.Source source)
        {
            var rating = host.AddComponent<StarRatingUI>();
            var so = new SerializedObject(rating);
            so.FindProperty("_source").enumValueIndex = (int)source;
            SetArray(so.FindProperty("_stars"), stars);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform BuildBriefingPanel(Transform root, UiTheme theme, out Button back, out Button start)
        {
            RectTransform panel = UiKit.Node("BriefingPanel", root);
            UiKit.Stretch(panel);

            Image overlay = UiKit.Panel(theme, panel, "Overlay", new Color(0.063f, 0.055f, 0.14f, 0.94f), raycast: true);
            overlay.sprite = null;
            overlay.type = Image.Type.Simple;
            UiKit.Stretch((RectTransform)overlay.transform);

            // One centred column, generous spacing: heading, three steps, one scoring line,
            // one controls card, one pair of buttons.
            RectTransform content = UiKit.Node(BriefingMarker, panel);
            UiKit.Place(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 960f));
            UiKit.Column(content, 34f, TextAnchor.MiddleCenter);

            TMP_Text title = UiKit.Text(theme, content, "Title", "BEFORE SERVICE", true, 96f, UiTheme.Butter);
            UiKit.Outline(title, 0.26f);
            UiKit.Size(title, -1f, 112f);

            RectTransform steps = UiKit.Node("Steps", content);
            UiKit.Row(steps, 24f, TextAnchor.MiddleCenter, expandWidth: false, expandHeight: true);
            UiKit.Size(steps, -1f, 220f);
            BuildStep(theme, steps, "Grab", "From the fridges", theme.Fridge, new Color32(0xD6, 0xEB, 0xF9, 255));
            BuildArrow(theme, steps);
            BuildStep(theme, steps, "Prep", "Chop or cook", theme.Knife, new Color32(0xFF, 0xE1, 0xD6, 255));
            BuildArrow(theme, steps);
            BuildStep(theme, steps, "Serve", "To the windows", theme.Bell, new Color32(0xFF, 0xF0, 0xBD, 255));

            BuildScoringStrip(theme, content);
            BuildControlsCard(theme, content);

            RectTransform actions = UiKit.Node("Actions", content);
            UiKit.Row(actions, 36f, TextAnchor.MiddleCenter);
            UiKit.Size(actions, -1f, 124f);
            back = UiKit.Button(theme, actions, "BackButton", "Back", UiKit.ButtonStyle.Ghost, 100f, 40f, null);
            UiKit.Size(back, 260f, 100f);
            start = UiKit.Button(theme, actions, "StartServiceButton", "Start service", UiKit.ButtonStyle.Primary, 118f, 54f, theme.Play);
            UiKit.Size(start, 580f, 118f);

            return panel;
        }

        private static void BuildStep(UiTheme theme, Transform parent, string title, string description, Sprite icon, Color ringColor)
        {
            Image card = UiKit.Ticket(theme, parent, "Step_" + title);
            UiKit.Column(card, 6f, TextAnchor.MiddleCenter, new RectOffset(26, 26, 18, 18));
            UiKit.Size(card, -1f, -1f, flexWidth: 1f);

            Image ring = UiKit.Icon(theme, card.transform, "Ring", theme.Circle, 88f, ringColor);
            UiKit.Icon(theme, ring.transform, "Icon", icon, 54f);
            var iconRt = (RectTransform)ring.transform.Find("Icon");
            iconRt.GetComponent<LayoutElement>().ignoreLayout = true;
            UiKit.Place(iconRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(54f, 54f));

            TMP_Text heading = UiKit.Text(theme, card.transform, "Title", title, true, 46f, UiTheme.PaperInk);
            UiKit.Size(heading, -1f, 54f);
            TMP_Text body = UiKit.Text(theme, card.transform, "Description", description, false, 28f,
                new Color32(0x6B, 0x5D, 0x52, 255), bold: true);
            UiKit.Size(body, -1f, 36f);
        }

        private static void BuildArrow(UiTheme theme, Transform parent)
        {
            Image arrow = UiKit.Icon(theme, parent, "Arrow", theme.Play, 40f, UiTheme.Butter);
            UiKit.Size(arrow, 40f, 40f);
        }

        /// <summary>The whole scoring rule on one line: three values and the time penalty.</summary>
        private static void BuildScoringStrip(UiTheme theme, Transform parent)
        {
            Image strip = UiKit.Panel(theme, parent, "Scoring", new Color(UiTheme.Ink.r, UiTheme.Ink.g, UiTheme.Ink.b, 0.9f), 3f);
            UiKit.Edge(strip, new Color(0f, 0f, 0f, 0.3f), 6f);
            UiKit.Row(strip, 44f, TextAnchor.MiddleCenter, new RectOffset(48, 48, 0, 0));
            UiKit.Size(strip, -1f, 92f);

            BuildScoreItem(theme, strip.transform, theme.Vegetable, "+20", "chop 2s");
            BuildScoreItem(theme, strip.transform, theme.Cheese, "+10", "ready as is");
            BuildScoreItem(theme, strip.transform, theme.Meat, "+30", "cook 6s");

            Image divider = UiKit.Panel(theme, strip.transform, "Divider", new Color(1f, 1f, 1f, 0.16f), 1f);
            UiKit.Size(divider, 3f, 48f);

            RectTransform penalty = UiKit.Node("Penalty", strip.transform);
            UiKit.Row(penalty, 12f, TextAnchor.MiddleCenter);
            TMP_Text minus = UiKit.Text(theme, penalty, "Value", "-1", true, 44f, UiTheme.Tomato);
            UiKit.Size(minus, -1f, 60f);
            TMP_Text each = UiKit.Text(theme, penalty, "Label", "every second waiting", false, 26f, UiTheme.Muted, bold: true);
            UiKit.Size(each, -1f, 60f);
        }

        private static void BuildScoreItem(UiTheme theme, Transform parent, Sprite icon, string value, string detail)
        {
            RectTransform item = UiKit.Node("Item_" + value, parent);
            UiKit.Row(item, 14f, TextAnchor.MiddleCenter);
            UiKit.Icon(theme, item, "Icon", icon, 56f);
            TMP_Text points = UiKit.Text(theme, item, "Value", value, true, 44f, UiTheme.Butter);
            UiKit.Size(points, -1f, 60f);
            TMP_Text sub = UiKit.Text(theme, item, "Detail", detail, false, 26f, UiTheme.Muted, bold: true);
            UiKit.Size(sub, -1f, 60f);
        }

        private static void BuildControlsCard(UiTheme theme, Transform parent)
        {
            Image card = UiKit.DarkCard(theme, parent, "ControlsCard");
            UiKit.Size(card, -1f, 240f);
            UiKit.Column(card, 0f, TextAnchor.MiddleCenter, new RectOffset(40, 40, 24, 24));

            RectTransform list = UiKit.Node("ControlsList", card.transform);
            var row = UiKit.Row(list, 24f, TextAnchor.MiddleCenter, expandWidth: true, expandHeight: true);
            row.childControlWidth = true;
            UiKit.Size(list, -1f, -1f, flexWidth: 1f, flexHeight: 1f);
            var controls = list.gameObject.AddComponent<ControlsListUI>();
            var so = new SerializedObject(controls);
            so.FindProperty("_theme").objectReferenceValue = theme;
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        // ------------------------------------------------------------------ results

        private static void BuildResults(GameOverScreenUI screen, UiTheme theme)
        {
            var so = new SerializedObject(screen);
            var root = (GameObject)so.FindProperty("_screenRoot").objectReferenceValue;
            if (root == null)
            {
                return;
            }

            if (root.TryGetComponent(out Image rootImage))
            {
                rootImage.sprite = null;
                rootImage.type = Image.Type.Simple;
                rootImage.color = new Color(0.063f, 0.055f, 0.14f, 0.82f);
                rootImage.raycastTarget = true;
            }

            ClearChildren(root.transform);

            Image ticket = UiKit.Ticket(theme, root.transform, "Ticket", 1.6f);
            UiKit.Place((RectTransform)ticket.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 900f));
            ticket.transform.localRotation = Quaternion.Euler(0f, 0f, -1f);
            UiKit.Column(ticket, 6f, TextAnchor.UpperCenter, new RectOffset(56, 56, 48, 48));

            TMP_Text label = UiKit.Text(theme, ticket.transform, "Label", "SERVICE OVER", false, 28f, UiTheme.PaperMuted, spacing: 10f, bold: true);
            UiKit.Size(label, -1f, 38f);

            TMP_Text headline = UiKit.Text(theme, ticket.transform, "Headline", "Nice shift!", true, 104f, UiTheme.Tomato);
            UiKit.Size(headline, -1f, 122f);

            RectTransform starRow = UiKit.Node("Stars", ticket.transform);
            UiKit.Row(starRow, 10f, TextAnchor.MiddleCenter);
            UiKit.Size(starRow, -1f, 120f);
            var stars = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                stars[i] = UiKit.Icon(theme, starRow, "Star" + i, theme.Star, i == 1 ? 112f : 88f, UiTheme.StarOff);
            }

            AttachStars(starRow.gameObject, stars, StarRatingUI.Source.CurrentScore);
            var rating = starRow.GetComponent<StarRatingUI>();

            TMP_Text finalScore = UiKit.Text(theme, ticket.transform, "FinalScore", "0", true, 190f, UiTheme.PaperInk);
            UiKit.Size(finalScore, -1f, 210f);

            Image banner = UiKit.Panel(theme, ticket.transform, "NewHighScoreBanner", UiTheme.Butter, 2f);
            UiKit.Edge(banner, UiTheme.ButterDeep, 6f);
            UiKit.Row(banner, 0f, TextAnchor.MiddleCenter, new RectOffset(34, 34, 0, 0));
            UiKit.Size(banner, 280f, 58f);
            banner.transform.localRotation = Quaternion.Euler(0f, 0f, -3f);
            TMP_Text bannerText = UiKit.Text(theme, banner.transform, "Text", "NEW BEST", false, 28f, new Color32(0x3B, 0x2A, 0x00, 255),
                spacing: 6f, bold: true);

            RectTransform bestRow = UiKit.Node("BestRow", ticket.transform);
            UiKit.Row(bestRow, 14f, TextAnchor.MiddleCenter, new RectOffset(0, 0, 16, 0));
            UiKit.Size(bestRow, -1f, 76f);
            TMP_Text bestLabel = UiKit.Text(theme, bestRow, "Label", "BEST", false, 28f, UiTheme.PaperMuted, spacing: 8f, bold: true);
            UiKit.Size(bestLabel, 100f, -1f);
            TMP_Text bestValue = UiKit.Text(theme, bestRow, "Value", "0", true, 54f, UiTheme.PaperInk);
            UiKit.Size(bestValue, 150f, -1f);

            RectTransform spacer = UiKit.Node("Spacer", ticket.transform);
            UiKit.Size(spacer, -1f, -1f, flexHeight: 1f);

            RectTransform buttons = UiKit.Node("Buttons", ticket.transform);
            UiKit.Row(buttons, 28f, TextAnchor.MiddleCenter);
            UiKit.Size(buttons, -1f, 108f);
            Button quit = UiKit.Button(theme, buttons, "QuitButton", "Quit", UiKit.ButtonStyle.Paper, 96f, 40f, null);
            UiKit.Size(quit, 230f, 96f);
            Button again = UiKit.Button(theme, buttons, "RestartButton", "Play again", UiKit.ButtonStyle.Primary, 108f, 46f, theme.Play);
            UiKit.Size(again, 430f, 108f);

            so.FindProperty("_finalScoreText").objectReferenceValue = finalScore;
            so.FindProperty("_highScoreText").objectReferenceValue = bestValue;
            so.FindProperty("_newHighScoreBanner").objectReferenceValue = banner.gameObject;
            so.FindProperty("_restartButton").objectReferenceValue = again;
            so.FindProperty("_quitButton").objectReferenceValue = quit;
            so.FindProperty("_headlineText").objectReferenceValue = headline;
            so.FindProperty("_stars").objectReferenceValue = rating;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
