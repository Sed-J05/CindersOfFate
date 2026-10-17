using System.Text;
using Shatranj.Core;
using Shatranj.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Build UI Prefabs</c>. Builds Assets/Prefabs/UI/HUD.prefab from code using the Prompt 4
    /// placeholder UI sprites (Docs/08 §6 layout). Overwrites the prefab in place, so its GUID and scene
    /// references survive. Requires TMP Essential Resources.
    /// </summary>
    public static class BuildUIPrefabs
    {
        public const string PrefabFolder = "Assets/Prefabs/UI";
        public const string HudPrefabPath = PrefabFolder + "/HUD.prefab";
        private const string UiArt = "Assets/Art/UI/";

        [MenuItem("Shatranj/Build UI Prefabs", false, 40)]
        public static void Run()
        {
            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Build UI Prefabs ===");

            if (!TmpEssentialsPresent())
            {
                report.AppendLine("FAILED: TextMeshPro Essential Resources are missing.");
                report.AppendLine("Run  Window → TextMeshPro → Import TMP Essential Resources  once, then run this menu again.");
                Debug.LogError(report.ToString());
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<Sprite>(UiArt + "Heart_Full.png") == null)
            {
                report.AppendLine("UI placeholder sprites missing → generating placeholder art first.");
                Art.GeneratePlaceholderArt.Run();
            }

            GenerateConfigs.EnsureFolder(PrefabFolder);
            GameObject root = BuildHud(report);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath, out bool success);
            Object.DestroyImmediate(root);

            if (!success || prefab == null)
            {
                report.AppendLine("FAILED to save " + HudPrefabPath);
                Debug.LogError(report.ToString());
                return;
            }

            AssetDatabase.SaveAssets();
            report.AppendLine("Saved " + HudPrefabPath + " (Canvas 1920×1080, hearts, soul vessel, boss bar, flame arc, ability popup, cycle label, interact prompt, F10 debug panel).");
            Debug.Log(report.ToString());
        }

        public static bool TmpEssentialsPresent()
        {
            return AssetDatabase.FindAssets("t:TMP_Settings").Length > 0;
        }

        private static Sprite UiSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(UiArt + name + ".png");

        // ------------------------------------------------------------------
        // Hierarchy
        // ------------------------------------------------------------------

        private static GameObject BuildHud(StringBuilder report)
        {
            var root = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            var hud = root.AddComponent<HUDController>();
            var canvasRect = (RectTransform)root.transform;

            // --- Soul vessel: 192×192 at (60, −60) from top-left ---
            RectTransform soulRoot = Rect("SoulVessel", canvasRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -60f), new Vector2(192f, 192f));
            Image soulGlow = Img("Glow", soulRoot, UiSprite("Soul_Glow"), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(256f, 256f), new Color(1f, 1f, 1f, 0.5f));
            Image soulFill = Img("Fill", soulRoot, UiSprite("Soul_Fill"), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            soulFill.type = Image.Type.Filled;
            soulFill.fillMethod = Image.FillMethod.Vertical;
            soulFill.fillOrigin = (int)Image.OriginVertical.Bottom;
            soulFill.fillAmount = 0f;
            Img("Frame", soulRoot, UiSprite("Soul_Frame"), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            var soulUi = soulRoot.gameObject.AddComponent<SoulVesselUI>();
            SceneBuildUtil.SetReference(soulUi, "fill", soulFill);
            SceneBuildUtil.SetReference(soulUi, "glow", soulGlow);

            // --- Hearts: 64×64, spacing 8, from (270, −70) ---
            RectTransform heartsRoot = Rect("Hearts", canvasRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(270f, -70f), new Vector2(7 * 72f, 64f));
            var heartsUi = heartsRoot.gameObject.AddComponent<HeartsUI>();
            SceneBuildUtil.SetReference(heartsUi, "container", heartsRoot);
            SceneBuildUtil.SetReference(heartsUi, "fullSprite", UiSprite("Heart_Full"));
            SceneBuildUtil.SetReference(heartsUi, "emptySprite", UiSprite("Heart_Empty"));
            SetSpriteArray(heartsUi, "breakFrames", new[] { UiSprite("Heart_Break_00"), UiSprite("Heart_Break_01"), UiSprite("Heart_Break_02"), UiSprite("Heart_Break_03") });

            // --- Boss bar: 900×48 at bottom-centre, y = 90, name above ---
            RectTransform bossRoot = Rect("BossBar", canvasRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(900f, 48f));
            var bossGroup = bossRoot.gameObject.AddComponent<CanvasGroup>();
            bossGroup.alpha = 0f;
            RectTransform fillArea = Rect("FillArea", bossRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-12f, -12f));
            Image bossFill = Img("Fill", fillArea, UiSprite("BossBar_Fill"), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            bossFill.type = Image.Type.Filled;
            bossFill.fillMethod = Image.FillMethod.Horizontal;
            bossFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            bossFill.fillAmount = 0f;
            RectTransform ticks = Rect("Ticks", fillArea, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Img("Frame", bossRoot, UiSprite("BossBar_Frame"), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            TextMeshProUGUI bossName = Text("Name", bossRoot, "BOSS", 36, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(900f, 48f));
            bossName.fontStyle = FontStyles.Bold;
            bossName.color = new Color(0.95f, 0.92f, 0.85f);
            var bossUi = bossRoot.gameObject.AddComponent<BossBarUI>();
            SceneBuildUtil.SetReference(bossUi, "group", bossGroup);
            SceneBuildUtil.SetReference(bossUi, "fill", bossFill);
            SceneBuildUtil.SetReference(bossUi, "fillArea", fillArea);
            SceneBuildUtil.SetReference(bossUi, "nameText", bossName);
            SceneBuildUtil.SetReference(bossUi, "ticksContainer", ticks);

            // --- Flame arc: 200×60 top-centre (Phase 2) ---
            RectTransform arcRoot = Rect("FlameArc", canvasRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(200f, 60f));
            var arcGroup = arcRoot.gameObject.AddComponent<CanvasGroup>();
            arcGroup.alpha = 0f;
            RectTransform arcArea = Rect("ArcArea", arcRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(200f, 48f));
            Sprite glowSprite = UiSprite("Soul_Glow");
            for (int i = 0; i <= 20; i++)
            {
                Vector2 p = FlameArcIndicator.ArcPoint(i / 20f, 200f, 48f);
                Img($"ArcDot_{i}", arcArea, glowSprite, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), p, new Vector2(7f, 7f), new Color(1f, 0.95f, 0.85f, 0.55f));
            }
            Image dot = Img("FlameDot", arcArea, glowSprite, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), FlameArcIndicator.ArcPoint(0f, 200f, 48f), new Vector2(22f, 22f), new Color(0.81f, 0.92f, 1f, 1f));
            var arcUi = arcRoot.gameObject.AddComponent<FlameArcIndicator>();
            SceneBuildUtil.SetReference(arcUi, "group", arcGroup);
            SceneBuildUtil.SetReference(arcUi, "arcArea", arcArea);
            SceneBuildUtil.SetReference(arcUi, "dot", dot.rectTransform);
            SceneBuildUtil.SetReference(arcUi, "dotImage", dot);

            // --- Ability popup: centre, slightly above middle ---
            RectTransform popupRoot = Rect("AbilityPopup", canvasRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(600f, 120f));
            var popupGroup = popupRoot.gameObject.AddComponent<CanvasGroup>();
            popupGroup.alpha = 0f;
            Img("Backdrop", popupRoot, UiSprite("Button_Normal"), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.85f));
            Image popupIcon = Img("Icon", popupRoot, UiSprite("Ability_Icon_Dash"), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(60f, 0f), new Vector2(96f, 96f), Color.white);
            TextMeshProUGUI popupText = Text("Name", popupRoot, "ABILITY", 34, TextAlignmentOptions.Left, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(130f, 0f), new Vector2(-150f, 60f));
            popupText.fontStyle = FontStyles.Bold;
            popupText.color = new Color(0.15f, 0.15f, 0.17f);
            var popupUi = popupRoot.gameObject.AddComponent<AbilityPopupUI>();
            SceneBuildUtil.SetReference(popupUi, "group", popupGroup);
            SceneBuildUtil.SetReference(popupUi, "icon", popupIcon);
            SceneBuildUtil.SetReference(popupUi, "nameText", popupText);
            SceneBuildUtil.SetReference(popupUi, "dashIcon", UiSprite("Ability_Icon_Dash"));
            SceneBuildUtil.SetReference(popupUi, "fireballIcon", UiSprite("Ability_Icon_Fireball"));

            // --- Cycle label: bottom-left ---
            RectTransform cycleRoot = Rect("CycleLabel", canvasRect, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(20f, 16f), new Vector2(300f, 36f));
            TextMeshProUGUI cycleText = Text("Text", cycleRoot, "Cycle 2", 24, TextAlignmentOptions.Left, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            cycleText.color = new Color(0.9f, 0.86f, 0.75f, 0.9f);
            cycleText.enabled = false;
            var cycleUi = cycleRoot.gameObject.AddComponent<CycleLabelUI>();
            SceneBuildUtil.SetReference(cycleUi, "label", cycleText);

            // --- Interact prompt: positioned at runtime from a world point ---
            RectTransform promptRoot = Rect("InteractPrompt", canvasRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 44f));
            var promptGroup = promptRoot.gameObject.AddComponent<CanvasGroup>();
            promptGroup.alpha = 0f;
            Img("Backdrop", promptRoot, UiSprite("Button_Normal"), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.8f));
            TextMeshProUGUI promptText = Text("Text", promptRoot, "W — Rest", 26, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            promptText.color = new Color(0.15f, 0.15f, 0.17f);
            var promptUi = promptRoot.gameObject.AddComponent<InteractPromptUI>();
            SceneBuildUtil.SetReference(promptUi, "canvasRect", canvasRect);
            SceneBuildUtil.SetReference(promptUi, "promptRect", promptRoot);
            SceneBuildUtil.SetReference(promptUi, "group", promptGroup);
            SceneBuildUtil.SetReference(promptUi, "label", promptText);

            // --- Debug panel (F10): right side ---
            RectTransform debugRoot = Rect("DebugPanel", canvasRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(620f, 220f));
            var debugGroup = debugRoot.gameObject.AddComponent<CanvasGroup>();
            debugGroup.alpha = 0f;
            Img("Backdrop", debugRoot, null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.6f));
            TextMeshProUGUI debugText = Text("Text", debugRoot, "HUD DEBUG", 20, TextAlignmentOptions.TopLeft, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-24f, -16f));
            debugText.color = Color.white;
            var debugUi = debugRoot.gameObject.AddComponent<HudDebugPanel>();
            SceneBuildUtil.SetReference(debugUi, "group", debugGroup);
            SceneBuildUtil.SetReference(debugUi, "text", debugText);

            // --- Controller references ---
            SceneBuildUtil.SetReference(hud, "canvas", canvas);
            SceneBuildUtil.SetReference(hud, "rootGroup", root.GetComponent<CanvasGroup>());
            SceneBuildUtil.SetReference(hud, "hearts", heartsUi);
            SceneBuildUtil.SetReference(hud, "soulVessel", soulUi);
            SceneBuildUtil.SetReference(hud, "bossBar", bossUi);
            SceneBuildUtil.SetReference(hud, "flameArc", arcUi);
            SceneBuildUtil.SetReference(hud, "abilityPopup", popupUi);
            SceneBuildUtil.SetReference(hud, "cycleLabel", cycleUi);
            SceneBuildUtil.SetReference(hud, "interactPrompt", promptUi);
            SceneBuildUtil.SetReference(hud, "debugPanel", debugUi);

            report.AppendLine("Widgets: SoulVessel, Hearts, BossBar, FlameArc, AbilityPopup, CycleLabel, InteractPrompt, DebugPanel.");
            return root;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return rect;
        }

        private static Image Img(string name, Transform parent, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
        {
            RectTransform rect = Rect(name, parent, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), anchoredPos, size);
            var img = rect.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private static TextMeshProUGUI Text(string name, Transform parent, string content, float size, TextAlignmentOptions align, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            RectTransform rect = Rect(name, parent, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            return tmp;
        }

        private static void SetSpriteArray(Object target, string field, Sprite[] sprites)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"BuildUIPrefabs: field '{field}' not found on {target.GetType().Name}"); return; }
            p.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
