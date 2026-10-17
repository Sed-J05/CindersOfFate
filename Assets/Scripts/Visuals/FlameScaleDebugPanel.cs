using System.Text;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Enemies;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace Shatranj.Visuals
{
    /// <summary>
    /// F12 debug panel (Prompt 12, Sandbox): a slider that sets flameT (0 to 1) so you can watch Phase 2 enemies grow.
    /// Drag the slider with the mouse, or use [ and ] (hold Shift for fine steps). While the panel is open the flame is
    /// on manual override; closing it hands the flame back to its normal motion. Shows the target scale, both curve
    /// modes and how many enemies are scaling. Scaling only happens in Phase 2: press F4 to toggle the phase.
    /// Editor / Development builds only. Built in code, legacy font.
    /// </summary>
    public class FlameScaleDebugPanel : MonoBehaviour
    {
        [SerializeField] private float keyStep = 0.1f;
        [SerializeField] private float fineStep = 0.02f;

        private GameObject root;
        private Slider slider;
        private Text text;
        private bool open;
        private float refreshTimer;
        private readonly StringBuilder sb = new StringBuilder(256);

        private void Awake()
        {
            root = new GameObject("FlameScaleDebugCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 610;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var bg = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(root.transform, false);
            var br = (RectTransform)bg.transform;
            br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f);
            br.pivot = new Vector2(0.5f, 1f);
            br.anchoredPosition = new Vector2(0f, -20f);
            br.sizeDelta = new Vector2(700f, 250f);
            bg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

            GameObject sliderGo = DefaultControls.CreateSlider(new DefaultControls.Resources());
            sliderGo.name = "FlameT";
            sliderGo.transform.SetParent(bg.transform, false);
            var sr = (RectTransform)sliderGo.transform;
            sr.anchorMin = new Vector2(0f, 1f); sr.anchorMax = new Vector2(1f, 1f);
            sr.pivot = new Vector2(0.5f, 1f);
            sr.offsetMin = new Vector2(24f, -54f); sr.offsetMax = new Vector2(-24f, -24f);
            slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f;
            Tint(sliderGo.transform, "Background", new Color(0.25f, 0.22f, 0.2f));
            Tint(sliderGo.transform, "Fill Area/Fill", new Color(1f, 0.6f, 0.2f));
            Tint(sliderGo.transform, "Handle Slide Area/Handle", new Color(1f, 0.85f, 0.5f));
            slider.onValueChanged.AddListener(SetFlame);

            var tgo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            tgo.transform.SetParent(bg.transform, false);
            var tr = (RectTransform)tgo.transform;
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(24f, 12f); tr.offsetMax = new Vector2(-24f, -66f);
            text = tgo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.raycastTarget = false;
            root.SetActive(false);
        }

        private static void Tint(Transform parent, string path, Color color)
        {
            Transform t = parent.Find(path);
            if (t != null && t.TryGetComponent(out Image img)) img.color = color;
        }

        private static void SetFlame(float value)
        {
            FlameController flame = FlameController.Instance;
            if (flame == null) return;
            flame.ManualOverride = true;
            flame.SetProgress(value);
        }

        private void OnDisable()
        {
            if (open && FlameController.Instance != null) FlameController.Instance.ManualOverride = false;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (kb.f12Key.wasPressedThisFrame) Toggle();
            if (!open) return;

            FlameController flame = FlameController.Instance;
            if (flame != null)
            {
                float step = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed ? fineStep : keyStep;
                if (kb.leftBracketKey.wasPressedThisFrame) SetFlame(flame.Progress - step);
                if (kb.rightBracketKey.wasPressedThisFrame) SetFlame(flame.Progress + step);
                slider.SetValueWithoutNotify(flame.Progress);
            }

            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer <= 0f) { refreshTimer = 0.1f; Refresh(flame); }
        }

        private void Toggle()
        {
            open = !open;
            root.SetActive(open);
            if (open) UI.UIFactory.EnsureEventSystem();
            FlameController flame = FlameController.Instance;
            if (flame != null) flame.ManualOverride = open;
            refreshTimer = 0f;
        }
#endif

        private void Refresh(FlameController flame)
        {
            sb.Clear();
            sb.Append("F12 FLAME SCALE   drag the slider, or [ and ] (Shift = fine)\n");
            if (flame == null || flame.Config == null)
            {
                sb.Append("No FlameController in this scene.");
                text.text = sb.ToString();
                return;
            }

            FlameConfig cfg = flame.Config;
            float t = flame.Progress;
            int phase = GameManager.HasInstance ? GameManager.Instance.Phase : GameConstants.Phase1;
            sb.Append("flameT ").Append(t.ToString("0.00"))
              .Append("   phase ").Append(phase)
              .Append("   config ").Append(cfg.id).Append('\n');
            if (!cfg.affectsEnemyScale)
            {
                sb.Append("This flame does not scale enemies. Press F4 for Phase 2.\n");
            }
            else
            {
                sb.Append("target scale ").Append(flame.EnemyScale.ToString("0.000"))
                  .Append("   (").Append(cfg.scaleMode).Append(", max ").Append(cfg.scaleMax.ToString("0.0")).Append(")\n");
                sb.Append("monotonic ").Append(FlameMath.Scale(t, FlameScaleMode.Monotonic, cfg.scaleMax).ToString("0.000"))
                  .Append("   peak ").Append(FlameMath.Scale(t, FlameScaleMode.Peak, cfg.scaleMax).ToString("0.000")).Append('\n');
            }

            FlameScaler[] scalers = FindObjectsByType<FlameScaler>(FindObjectsSortMode.None);
            float min = float.MaxValue, max = 0f;
            foreach (FlameScaler s in scalers) { min = Mathf.Min(min, s.Current); max = Mathf.Max(max, s.Current); }
            sb.Append("scaling enemies ").Append(scalers.Length);
            if (scalers.Length > 0) sb.Append("   now ").Append(min.ToString("0.00")).Append(" to ").Append(max.ToString("0.00"));
            sb.Append("\nSpawn Phase 2 enemies with keys 5 to 8 at the cursor.");
            text.text = sb.ToString();
        }
    }
}
