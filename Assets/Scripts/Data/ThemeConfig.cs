using UnityEngine;
using UnityEngine.Rendering;

namespace Shatranj.Data
{
    /// <summary>
    /// A full theatre look (Docs/07 §2, §7): parchment, environment, enemy and protagonist palettes,
    /// lighting, shadows, sprites and the post-processing volume. <c>ThemeManager</c> cross-fades between these.
    /// Sprite / profile references are filled in by later prompts (placeholder art, visual rig).
    /// </summary>
    [CreateAssetMenu(menuName = "Shatranj/Configs/Theme Config", fileName = "ThemeConfig")]
    public class ThemeConfig : ScriptableObject
    {
        [Header("Identity")]
        public string id = "Phase1_Beige";
        public string tileTheme = "Cave";

        [Header("Parchment")]
        public Color parchmentBase = Hex("#E8D9B5");
        public Color parchmentEdge = Hex("#9C8460");
        public Color frameTint = Hex("#9C8460");
        public Sprite parchmentSprite;
        public Sprite frameSprite;
        [Tooltip("Colour multiplied onto the parchment sprite (white = the texture as painted). Without a sprite, parchmentBase is used.")]
        public Color parchmentSpriteTint = Color.white;
        [Range(0f, 1f), Tooltip("Opacity of the soft flame glow drawn on the paper.")]
        public float flameGlowAlpha = 0.55f;
        [Range(0f, 1f), Tooltip("Opacity of the blurred flame core seen through the paper.")]
        public float flameCoreAlpha = 0.35f;

        [Header("Environment")]
        public Color environmentDark = Hex("#26262A");
        public Color environmentMid = Hex("#4A4A52");
        public Color environmentAccent = Hex("#6B3A3A");
        public Sprite backgroundFar;
        public Sprite backgroundMid;
        public float parallaxFar = 0.1f;
        public float parallaxMid = 0.3f;

        [Header("Enemies")]
        public Color enemyBase = Hex("#16090A");
        public Color enemyPrimary = Hex("#C1272D");
        public Color enemyAccent = Hex("#FF4B3E");
        public Color enemyAccentTint = Color.white;

        [Header("Protagonist")]
        public Color protagonistWhite = Hex("#EAF6FF");
        public Color protagonistBlue = Hex("#4FA3FF");
        public Color protagonistDeepBlue = Hex("#1B4F9C");
        public bool protagonistOutline = false;
        public Color protagonistOutlineColor = Color.white;
        public Color protagonistGlowColor = new Color(0.4f, 1f, 1f, 0.5f);

        [Header("Flame & lights (Docs/07 §3)")]
        public Color flameCore = Hex("#FFD27A");
        public Color flameGlow = Hex("#FF8A2B");
        public float flameLightIntensity = 1.2f;
        [Tooltip("Light2D outer radius as a multiple of the screen width.")]
        public float flameLightRadiusScreenMultiplier = 1.6f;
        [Range(0f, 1f)] public float flameLightFalloff = 0.6f;
        public Color globalLightColor = Color.white;
        public float globalLightIntensity = 0.55f;

        [Header("Shadows & puppet feel (Docs/07 §5-6)")]
        [Range(0f, 1f)] public float shadowAlpha = 0.30f;
        public float shadowOffsetBase = 0.10f;
        public float shadowOffsetMax = 0.35f;

        [Header("Post & UI")]
        public VolumeProfile volumeProfile;
        public Color uiTint = Color.white;
        public float contrastBoost = 0f;

        /// <summary>"#RRGGBB" or "#RRGGBBAA" → Color. Falls back to magenta on a bad string so mistakes are visible.</summary>
        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.magenta;
        }
    }
}
