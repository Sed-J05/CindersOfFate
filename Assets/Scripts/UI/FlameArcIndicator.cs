using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/07 §4 / Docs/08 §6: thin 200×60 arc at the top-centre with a glowing dot that follows flameT.
    /// Red-tinged above 0.75. Visible in Phase 2 only. Driven by OnFlameProgressChanged / OnPhaseChanged.
    /// </summary>
    public class FlameArcIndicator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform arcArea;
        [SerializeField] private RectTransform dot;
        [SerializeField] private Image dotImage;
        [SerializeField] private Color normalColor = new Color(0.81f, 0.92f, 1f, 1f);
        [SerializeField] private Color dangerColor = new Color(1f, 0.35f, 0.3f, 1f);
        [SerializeField, Range(0f, 1f)] private float dangerAbove = 0.75f;
        [SerializeField] private float smoothTime = 0.3f;

        private float target;
        private float current;
        private float velocity;
        private float pulse;

        public float Progress => current;

        private void OnEnable()
        {
            GameEvents.OnFlameProgressChanged += HandleProgress;
            GameEvents.OnPhaseChanged += HandlePhase;
            HandlePhase(GameManager.HasInstance ? GameManager.Instance.Phase : GameConstants.Phase1);
        }

        private void OnDisable()
        {
            GameEvents.OnFlameProgressChanged -= HandleProgress;
            GameEvents.OnPhaseChanged -= HandlePhase;
        }

        private void HandleProgress(float t) => target = Mathf.Clamp01(t);

        private void HandlePhase(int phase)
        {
            if (group != null) group.alpha = phase >= GameConstants.Phase2 ? 1f : 0f;
        }

        private void Update()
        {
            if (arcArea == null || dot == null) return;
            float dt = Time.unscaledDeltaTime;
            current = Mathf.SmoothDamp(current, target, ref velocity, smoothTime, Mathf.Infinity, dt);
            dot.anchoredPosition = ArcPoint(current, arcArea.rect.width, arcArea.rect.height);

            if (dotImage != null)
            {
                pulse += dt;
                float k = 0.85f + 0.15f * Mathf.Sin(pulse * 6f);
                Color c = current > dangerAbove ? dangerColor : normalColor;
                c.a *= k;
                dotImage.color = c;
            }
        }

        /// <summary>Same arc shape as the flame (Docs/07 §3), mapped into a width×height box with its base at y = 0.</summary>
        public static Vector2 ArcPoint(float t, float width, float height)
        {
            Vector2 p = FlameMath.Arc(t, 0f, 1f, 0f, 1f);
            return new Vector2((p.x - 0.5f) * width, p.y * height);
        }
    }
}
