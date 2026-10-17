using System.Text;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace Shatranj.Visuals
{
    /// <summary>
    /// F11 debug panel (Prompt 10): scrub the flame arc, cycle themes, toggle drop-shadows, park the flame, spike it.
    /// Keys work only while the panel is open (Editor / Development builds). Built in code, legacy font.
    /// </summary>
    public class VisualDebugPanel : MonoBehaviour
    {
        [SerializeField] private float scrubSpeed = 0.25f;

        private GameObject root;
        private Text text;
        private bool open;
        private readonly StringBuilder sb = new StringBuilder(256);

        private void Awake()
        {
            root = new GameObject("VisualDebugCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 600;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var bg = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(root.transform, false);
            var br = (RectTransform)bg.transform;
            br.anchorMin = br.anchorMax = new Vector2(1f, 0f);
            br.pivot = new Vector2(1f, 0f);
            br.anchoredPosition = new Vector2(-20f, 20f);
            br.sizeDelta = new Vector2(620f, 230f);
            bg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            var tgo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            tgo.transform.SetParent(bg.transform, false);
            var tr = (RectTransform)tgo.transform;
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(14f, 10f); tr.offsetMax = new Vector2(-14f, -10f);
            text = tgo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            root.SetActive(false);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (kb.f11Key.wasPressedThisFrame)
            {
                open = !open;
                root.SetActive(open);
                if (FlameController.Instance != null) FlameController.Instance.ManualOverride = false;
            }
            if (!open) return;

            FlameController flame = FlameController.Instance;
            ThemeManager theme = ThemeManager.Instance;

            float scrub = 0f;
            if (kb.commaKey.isPressed) scrub -= 1f;
            if (kb.periodKey.isPressed) scrub += 1f;
            if (flame != null && scrub != 0f)
            {
                flame.ManualOverride = true;
                flame.SetProgress(flame.Progress + scrub * scrubSpeed * Time.unscaledDeltaTime);
            }
            if (flame != null && kb.mKey.wasPressedThisFrame) flame.ManualOverride = !flame.ManualOverride;
            if (flame != null && kb.pKey.wasPressedThisFrame) { if (flame.IsParked) flame.Unpark(); else flame.Park(); }
            if (flame != null && kb.gKey.wasPressedThisFrame) flame.Spike();
            if (theme != null && kb.tKey.wasPressedThisFrame) theme.Apply(theme.NextThemeId(), 1f);
            if (kb.yKey.wasPressedThisFrame) PuppetShadow.GloballyEnabled = !PuppetShadow.GloballyEnabled;

            sb.Clear();
            sb.Append("VISUALS (F11)\n");
            if (flame != null)
                sb.Append("Flame t ").Append(flame.Progress.ToString("0.000"))
                  .Append("  ").Append(flame.Config != null ? flame.Config.motion.ToString() : "-")
                  .Append(flame.ManualOverride ? "  [frozen]" : "").Append(flame.IsParked ? "  [parked]" : "")
                  .Append("  scale ").Append(flame.EnemyScale.ToString("0.00")).Append('\n');
            sb.Append(",/. scrub t (hold)   M freeze/resume   P park   G spike\n");
            sb.Append("T next theme   [").Append(theme != null ? theme.CurrentId : "no ThemeManager").Append("]\n");
            sb.Append("Y drop-shadows   [").Append(PuppetShadow.GloballyEnabled ? "on" : "off").Append("]");
            text.text = sb.ToString();
        }
#endif
    }
}
