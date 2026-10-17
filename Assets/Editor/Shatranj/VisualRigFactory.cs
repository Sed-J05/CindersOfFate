using System.Text;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Builds the shadow-puppet stage for a scene (Docs/07): the camera-child ParchmentRig (parchment + cross-fade
    /// renderer, FlameGlow, FlameCore, ParchmentFrame, BG_Far / BG_Mid parallax), a [Visuals] object with
    /// FlameController, the flame Point Light 2D, a Global Light 2D, one global Volume (Bloom, Vignette, Film Grain,
    /// Color Adjustments), ThemeManager, ScreenWipe and the F11 panel. Also fills the theme assets' sprite slots
    /// (only empty ones) and creates the post-processing profile once. Call after NewScene (D-044).
    /// </summary>
    public static class VisualRigFactory
    {
        public const string ProfilePath = "Assets/Settings/Shatranj_PostFX.asset";
        private const string Backgrounds = "Assets/Art/Backgrounds/";
        private const string ThemeNames = "Phase1_Beige,Phase2_Inverted,Ending_Purple,Cutscene";

        public static void Build(Camera cam, StringBuilder report)
        {
            FillThemeSprites(report);
            VolumeProfile profile = GetOrCreateProfile(report);

            Material lit = SceneBuildUtil.SpriteLitMaterial();
            Material unlit = SceneBuildUtil.SpriteUnlitMaterial();

            // --- camera settings ---
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            // --- camera child rig ---
            Transform existing = cam.transform.Find("Rig");
            var rigGo = existing != null ? existing.gameObject : new GameObject("Rig");
            rigGo.transform.SetParent(cam.transform, false);
            rigGo.transform.localPosition = Vector3.zero;
            var rig = rigGo.AddComponent<ParchmentRig>();

            SpriteRenderer parchment = Layer(rigGo.transform, "Parchment", GameLayers.Sorting.Parchment, 0, lit, Sprite(Backgrounds + "Parchment_Beige.png"), 20f);
            SpriteRenderer crossfade = Layer(rigGo.transform, "ParchmentCrossfade", GameLayers.Sorting.Parchment, 1, lit, null, 19.9f);
            crossfade.enabled = false;
            SpriteRenderer glow = Layer(rigGo.transform, "FlameGlow", GameLayers.Sorting.Parchment, 2, unlit, Sprite(Backgrounds + "FlameGlow.png"), 19.8f);
            SpriteRenderer core = Layer(rigGo.transform, "FlameCore", GameLayers.Sorting.Parchment, 3, unlit, Sprite(Backgrounds + "FlameCore_00.png"), 19.7f);
            var coreAnim = core.gameObject.AddComponent<SpriteAnimator>();
            var coreSet = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(Art.RebuildAnimations.SetPath("FlameCore"));
            if (coreSet != null) SceneBuildUtil.SetReference(coreAnim, "set", coreSet);
            SceneBuildUtil.SetString(coreAnim, "defaultClip", "Default");
            SpriteRenderer frame = Layer(rigGo.transform, "ParchmentFrame", GameLayers.Sorting.LightOverlay, 0, unlit, Sprite(Backgrounds + "ParchmentFrame.png"), 1f);

            ParallaxLayer far = Parallax(rigGo.transform, "BG_Far", GameLayers.Sorting.BG_Far, lit, Sprite(Backgrounds + "BG_Cave_Far.png"), 0.1f, 18f, cam);
            ParallaxLayer mid = Parallax(rigGo.transform, "BG_Mid", GameLayers.Sorting.BG_Mid, lit, Sprite(Backgrounds + "BG_Cave_Mid.png"), 0.3f, 17f, cam);

            SceneBuildUtil.SetReference(rig, "targetCamera", cam);
            SceneBuildUtil.SetReference(rig, "parchment", parchment);
            SceneBuildUtil.SetReference(rig, "parchmentCrossfade", crossfade);
            SceneBuildUtil.SetReference(rig, "flameGlow", glow);
            SceneBuildUtil.SetReference(rig, "flameCore", core);
            SceneBuildUtil.SetReference(rig, "frame", frame);
            SceneBuildUtil.SetReference(rig, "backgroundFar", far);
            SceneBuildUtil.SetReference(rig, "backgroundMid", mid);

            // --- [Visuals] ---
            var visuals = new GameObject("[Visuals]");
            var flame = visuals.AddComponent<FlameController>();
            SceneBuildUtil.SetReference(flame, "phase1Config", AssetDatabase.LoadAssetAtPath<FlameConfig>(GenerateConfigs.FlamePath("Phase1")));
            SceneBuildUtil.SetReference(flame, "phase2Config", AssetDatabase.LoadAssetAtPath<FlameConfig>(GenerateConfigs.FlamePath("Phase2")));
            SceneBuildUtil.SetReference(flame, "targetCamera", cam);

            var lightGo = new GameObject("FlameLight2D");
            lightGo.transform.SetParent(visuals.transform, false);
            var point = lightGo.AddComponent<Light2D>();
            point.lightType = Light2D.LightType.Point;
            point.shadowsEnabled = false;
            var flameLight = lightGo.AddComponent<FlameLight>();
            SceneBuildUtil.SetReference(flameLight, "flame", flame);

            var globalGo = new GameObject("GlobalLight2D");
            globalGo.transform.SetParent(visuals.transform, false);
            var global = globalGo.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.intensity = 0.55f;

            var volumeGo = new GameObject("PostFX Volume");
            volumeGo.transform.SetParent(visuals.transform, false);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.sharedProfile = profile;

            var themeManager = visuals.AddComponent<ThemeManager>();
            var themes = new Object[4];
            string[] names = ThemeNames.Split(',');
            for (int i = 0; i < names.Length; i++) themes[i] = AssetDatabase.LoadAssetAtPath<ThemeConfig>(GenerateConfigs.ThemePath(names[i]));
            SceneBuildUtil.SetObjectArray(themeManager, "themes", themes);
            SceneBuildUtil.SetReference(themeManager, "artStyle", AssetDatabase.LoadAssetAtPath<ArtStyleConfig>(GenerateConfigs.ArtStylePath));
            SceneBuildUtil.SetReference(themeManager, "rig", rig);
            SceneBuildUtil.SetReference(themeManager, "flameLight", flameLight);
            SceneBuildUtil.SetReference(themeManager, "globalLight", global);
            SceneBuildUtil.SetReference(themeManager, "volume", volume);
            SceneBuildUtil.SetReference(themeManager, "targetCamera", cam);

            visuals.AddComponent<ScreenWipe>();
            visuals.AddComponent<VisualDebugPanel>();

            report?.AppendLine("Visual rig: parchment + flame glow/core + frame + BG parallax, flame Light2D, global Light2D, PostFX volume, ThemeManager, ScreenWipe, F11 panel.");
        }

        // ------------------------------------------------------------------

        private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        private static SpriteRenderer Layer(Transform parent, string name, string sortingLayer, int order, Material mat, Sprite sprite, float z)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, z);
            go.layer = GameLayers.Ignore;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = order;
            if (mat != null) sr.sharedMaterial = mat;
            return sr;
        }

        private static ParallaxLayer Parallax(Transform parent, string name, string sortingLayer, Material mat, Sprite sprite, float factor, float z, Camera cam)
        {
            SpriteRenderer sr = Layer(parent, name, sortingLayer, 0, mat, sprite, z);
            sr.drawMode = SpriteDrawMode.Tiled;
            var layer = sr.gameObject.AddComponent<ParallaxLayer>();
            SceneBuildUtil.SetReference(layer, "targetCamera", cam);
            SceneBuildUtil.SetFloat(layer, "factor", factor);
            SceneBuildUtil.SetFloat(layer, "depth", z);
            return layer;
        }

        /// <summary>Fills empty sprite slots on the four theme assets (never overwrites a sprite set by hand).</summary>
        private static void FillThemeSprites(StringBuilder report)
        {
            UpgradeTheme("Phase1_Beige", ConfigDefaults.ThemePhase1Beige, report);
            UpgradeTheme("Phase2_Inverted", ConfigDefaults.ThemePhase2Inverted, report);
            UpgradeTheme("Ending_Purple", ConfigDefaults.ThemeEndingPurple, report);
            UpgradeTheme("Cutscene", ConfigDefaults.ThemeCutscene, report);

            Fill("Phase1_Beige", "Parchment_Beige", "BG_Cave_Far", "BG_Cave_Mid", Color.white);
            // D-154: Phase 2 paper is the beige texture tinted grey-blue (Parchment_Inverted.png stays untouched, unused).
            Fill("Phase2_Inverted", "Parchment_Beige", "BG_Castle_Far", "BG_Castle_Mid", ConfigDefaults.Phase2ParchmentTint);
            Fill("Ending_Purple", "Parchment_Inverted", "BG_Castle_Far", "BG_Castle_Mid", ConfigDefaults.EndingParchmentTint);
            Fill("Cutscene", "Parchment_Beige", "BG_Cave_Far", "BG_Cave_Mid", new Color(0.88f, 0.86f, 0.82f));
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Re-applies the generator defaults (colours and numbers, never sprite slots) to a theme asset made by an older
        /// generator (<c>defaultsRevision</c> &lt; <see cref="ConfigDefaults.ThemeRevision"/>). Revision 2 also moves the
        /// Phase 2 paper from the navy Parchment_Inverted placeholder to the tinted beige texture.
        /// </summary>
        private static void UpgradeTheme(string theme, System.Action<ThemeConfig> defaults, StringBuilder report)
        {
            var t = AssetDatabase.LoadAssetAtPath<ThemeConfig>(GenerateConfigs.ThemePath(theme));
            if (t == null || t.defaultsRevision >= ConfigDefaults.ThemeRevision) return;
            int was = t.defaultsRevision;
            defaults(t);
            if (theme == "Phase2_Inverted" && t.parchmentSprite == Sprite(Backgrounds + "Parchment_Inverted.png"))
                t.parchmentSprite = Sprite(Backgrounds + "Parchment_Beige.png");
            EditorUtility.SetDirty(t);
            report?.AppendLine($"  theme  {theme}: defaults revision {was} → {ConfigDefaults.ThemeRevision}");
        }

        private static void Fill(string theme, string parchment, string far, string mid, Color tint)
        {
            var t = AssetDatabase.LoadAssetAtPath<ThemeConfig>(GenerateConfigs.ThemePath(theme));
            if (t == null) return;
            bool dirty = false;
            if (t.parchmentSprite == null) { t.parchmentSprite = Sprite(Backgrounds + parchment + ".png"); t.parchmentSpriteTint = tint; dirty = true; }
            if (t.frameSprite == null) { t.frameSprite = Sprite(Backgrounds + "ParchmentFrame.png"); dirty = true; }
            if (t.backgroundFar == null) { t.backgroundFar = Sprite(Backgrounds + far + ".png"); dirty = true; }
            if (t.backgroundMid == null) { t.backgroundMid = Sprite(Backgrounds + mid + ".png"); dirty = true; }
            if (dirty) EditorUtility.SetDirty(t);
        }

        /// <summary>One shared profile: Bloom, Vignette, Film Grain, Color Adjustments (Docs/07 §1 item 7, §6).</summary>
        private static VolumeProfile GetOrCreateProfile(StringBuilder report)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile != null) return profile;

            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.6f);
            bloom.threshold.Override(0.9f);
            bloom.scatter.Override(0.7f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.28f);
            vignette.smoothness.Override(0.45f);
            vignette.color.Override(new Color(0.61f, 0.52f, 0.38f));

            var grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Medium1);
            grain.intensity.Override(0.25f);

            var grade = profile.Add<ColorAdjustments>(true);
            grade.contrast.Override(0f);
            grade.saturation.Override(0f);

            foreach (VolumeComponent c in profile.components)
            {
                c.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(c, profile);
            }
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            report?.AppendLine("Created " + ProfilePath + " (Bloom, Vignette, Film Grain, Color Adjustments).");
            return profile;
        }
    }
}
