using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Docs/07 §7: applies a <see cref="ThemeConfig"/> to the whole stage and cross-fades between themes —
    /// parchment texture (two-renderer cross-fade) and tint, burnt-edge frame, BG layer sprites, Global Light 2D,
    /// flame light / glow / core colours, drop-shadow alpha and the Volume's colour grade. Follows the game phase
    /// (Phase1_Beige ↔ Phase2_Inverted); cutscenes call <see cref="Apply(string, float)"/> directly.
    /// </summary>
    [DisallowMultipleComponent]
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }

        public const string Phase1Beige = "Phase1_Beige";
        public const string Phase2Inverted = "Phase2_Inverted";
        public const string EndingPurple = "Ending_Purple";
        public const string CutsceneTheme = "Cutscene";

        [SerializeField] private ThemeConfig[] themes = new ThemeConfig[0];
        [SerializeField] private ArtStyleConfig artStyle;
        [SerializeField] private ParchmentRig rig;
        [SerializeField] private FlameLight flameLight;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Volume volume;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float phaseChangeFadeTime = 1.5f;

        private ThemeConfig current;
        private ThemeConfig from;
        private ThemeConfig to;
        private float fadeT = 1f;
        private float fadeDuration;
        private bool spriteSwapped;
        private ColorAdjustments colorAdjustments;
        private Vignette vignette;

        public ThemeConfig Current => current;
        public string CurrentId => current != null ? current.id : "";
        public float ShadowAlpha => PuppetShadow.ThemeAlpha;
        public bool IsFading => fadeT < 1f;

        private void Awake()
        {
            Instance = this;
            if (artStyle != null) PuppetShadow.GloballyEnabled = artStyle.dropShadows;
            if (targetCamera == null) targetCamera = Camera.main;
            if (volume != null && volume.profile != null)
            {
                volume.profile.TryGet(out colorAdjustments);
                volume.profile.TryGet(out vignette);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable() => GameEvents.OnPhaseChanged += HandlePhaseChanged;
        private void OnDisable() => GameEvents.OnPhaseChanged -= HandlePhaseChanged;

        private void Start()
        {
            if (current != null) return;   // a cutscene script already chose the theme
            int phase = GameManager.HasInstance ? GameManager.Instance.Phase : GameConstants.Phase1;
            Apply(phase >= GameConstants.Phase2 ? Phase2Inverted : Phase1Beige, 0f);
        }

        private void HandlePhaseChanged(int phase) => Apply(phase >= GameConstants.Phase2 ? Phase2Inverted : Phase1Beige, phaseChangeFadeTime);

        public ThemeConfig Find(string id)
        {
            for (int i = 0; i < themes.Length; i++) if (themes[i] != null && themes[i].id == id) return themes[i];
            return null;
        }

        public string NextThemeId()
        {
            if (themes.Length == 0) return CurrentId;
            int index = 0;
            for (int i = 0; i < themes.Length; i++) if (themes[i] == current) { index = i; break; }
            return themes[(index + 1) % themes.Length].id;
        }

        /// <summary>Cross-fades to the theme with this id over <paramref name="duration"/> seconds (0 = instant).</summary>
        public void Apply(string id, float duration)
        {
            ThemeConfig next = Find(id);
            if (next == null) { Debug.LogWarning($"ThemeManager: no theme '{id}'."); return; }
            if (current == null || duration <= 0f)
            {
                current = next;
                from = to = next;
                fadeT = 1f;
                ApplyBlend(next, next, 1f);
                SetParchmentSprite(next);
                return;
            }
            from = current;
            to = next;
            current = next;
            fadeT = 0f;
            fadeDuration = duration;
            spriteSwapped = false;
            if (rig != null && rig.ParchmentCrossfade != null)
            {
                rig.ParchmentCrossfade.sprite = ParchmentSpriteOf(next);
                rig.ParchmentCrossfade.enabled = rig.ParchmentCrossfade.sprite != null;
            }
        }

        private void Update()
        {
            if (fadeT >= 1f) return;
            fadeT = Mathf.Min(1f, fadeT + Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeDuration));
            float k = Easing.Evaluate(Ease.InOutQuad, fadeT);
            ApplyBlend(from, to, k);

            if (rig != null && rig.ParchmentCrossfade != null && rig.ParchmentCrossfade.enabled)
            {
                Color c = ParchmentColorOf(to);
                c.a = k;
                rig.ParchmentCrossfade.color = c;
            }
            if (!spriteSwapped && fadeT >= 0.5f) { SwapBackgrounds(to); spriteSwapped = true; }
            if (fadeT >= 1f)
            {
                SetParchmentSprite(to);
                if (rig != null && rig.ParchmentCrossfade != null) rig.ParchmentCrossfade.enabled = false;
            }
        }

        /// <summary>Cutscene "flame colour" beat: tints the flame light, glow and core towards <paramref name="color"/>.</summary>
        public void TintFlame(Color color, float duration)
        {
            if (tintRoutine != null) StopCoroutine(tintRoutine);
            tintRoutine = StartCoroutine(TintFlameRoutine(color, duration));
        }

        private Coroutine tintRoutine;

        private System.Collections.IEnumerator TintFlameRoutine(Color color, float duration)
        {
            Color glowFrom = rig != null && rig.FlameGlow != null ? rig.FlameGlow.color : color;
            Color coreFrom = rig != null && rig.FlameCore != null ? rig.FlameCore.color : color;
            Color lightFrom = flameLight != null && flameLight.Light != null ? flameLight.Light.color : color;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = t / Mathf.Max(0.01f, duration);
                SetFlameColours(Color.Lerp(glowFrom, color, k), Color.Lerp(coreFrom, Color.Lerp(color, Color.white, 0.6f), k), Color.Lerp(lightFrom, color, k));
                yield return null;
            }
            SetFlameColours(color, Color.Lerp(color, Color.white, 0.6f), color);
            tintRoutine = null;
        }

        private void SetFlameColours(Color glow, Color core, Color light)
        {
            if (rig != null && rig.FlameGlow != null) { glow.a = rig.FlameGlow.color.a; rig.FlameGlow.color = glow; }
            if (rig != null && rig.FlameCore != null) { core.a = rig.FlameCore.color.a; rig.FlameCore.color = core; }
            if (flameLight != null && flameLight.Light != null) flameLight.Light.color = light;
        }

        // ------------------------------------------------------------------

        private static Sprite ParchmentSpriteOf(ThemeConfig t) => t.parchmentSprite;

        private static Color ParchmentColorOf(ThemeConfig t) => t.parchmentSprite != null ? t.parchmentSpriteTint : t.parchmentBase;

        private void SetParchmentSprite(ThemeConfig t)
        {
            if (rig == null || rig.Parchment == null) return;
            if (t.parchmentSprite != null) rig.Parchment.sprite = t.parchmentSprite;
            rig.Parchment.color = ParchmentColorOf(t);
            SwapBackgrounds(t);
        }

        private void SwapBackgrounds(ThemeConfig t)
        {
            if (rig == null) return;
            if (rig.BackgroundFar != null)
            {
                rig.BackgroundFar.Renderer.sprite = t.backgroundFar;
                rig.BackgroundFar.Factor = t.parallaxFar;
            }
            if (rig.BackgroundMid != null)
            {
                rig.BackgroundMid.Renderer.sprite = t.backgroundMid;
                rig.BackgroundMid.Factor = t.parallaxMid;
            }
        }

        private void ApplyBlend(ThemeConfig a, ThemeConfig b, float k)
        {
            if (rig != null)
            {
                if (rig.Parchment != null && a == b) rig.Parchment.color = ParchmentColorOf(b);
                if (rig.Frame != null) rig.Frame.color = Color.Lerp(a.frameTint, b.frameTint, k);
                if (rig.FlameGlow != null)
                {
                    Color g = Color.Lerp(a.flameGlow, b.flameGlow, k);
                    g.a = Mathf.Lerp(a.flameGlowAlpha, b.flameGlowAlpha, k);
                    rig.FlameGlow.color = g;
                }
                if (rig.FlameCore != null)
                {
                    Color c = Color.Lerp(a.flameCore, b.flameCore, k);
                    c.a = Mathf.Lerp(a.flameCoreAlpha, b.flameCoreAlpha, k);
                    rig.FlameCore.color = c;
                }
            }

            if (flameLight != null)
                flameLight.ApplyTheme(Color.Lerp(a.flameGlow, b.flameGlow, k),
                                      Mathf.Lerp(a.flameLightIntensity, b.flameLightIntensity, k),
                                      Mathf.Lerp(a.flameLightRadiusScreenMultiplier, b.flameLightRadiusScreenMultiplier, k),
                                      Mathf.Lerp(a.flameLightFalloff, b.flameLightFalloff, k));

            if (globalLight != null)
            {
                globalLight.color = Color.Lerp(a.globalLightColor, b.globalLightColor, k);
                globalLight.intensity = Mathf.Lerp(a.globalLightIntensity, b.globalLightIntensity, k);
            }

            if (targetCamera != null) targetCamera.backgroundColor = Color.Lerp(a.parchmentBase, b.parchmentBase, k);

            PuppetShadow.ThemeAlpha = Mathf.Lerp(a.shadowAlpha, b.shadowAlpha, k);

            if (colorAdjustments != null)
            {
                colorAdjustments.contrast.Override(Mathf.Lerp(a.contrastBoost, b.contrastBoost, k) * 100f);
            }
            if (vignette != null) vignette.color.Override(Color.Lerp(a.parchmentEdge, b.parchmentEdge, k));
        }
    }
}
