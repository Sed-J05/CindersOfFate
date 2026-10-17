using Shatranj.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/08 §6 soul vessel: Soul_Frame + vertical Soul_Fill (fill = soul / max, smoothed 0.2 s) + Soul_Glow
    /// pulsing when soul ≥ 33. Phase 2 inverts the fill tint. Driven by OnSoulChanged / OnPhaseChanged.
    /// </summary>
    public class SoulVesselUI : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Image glow;
        [SerializeField] private float smoothTime = 0.2f;
        [SerializeField] private int glowThreshold = 33;
        [SerializeField] private float glowPulseHz = 1.2f;
        [SerializeField, Range(0f, 1f)] private float glowMinAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float glowMaxAlpha = 0.8f;
        [SerializeField] private Color phase1FillTint = Color.white;
        [SerializeField] private Color phase2FillTint = new Color(0.85f, 0.92f, 1f, 1f);

        private float target;
        private float velocity;
        private int soul;
        private int max = 99;
        private float pulseTime;

        public int Soul => soul;
        public int Max => max;

        private void OnEnable()
        {
            GameEvents.OnSoulChanged += HandleSoulChanged;
            GameEvents.OnPhaseChanged += HandlePhaseChanged;
            if (GameManager.HasInstance) HandlePhaseChanged(GameManager.Instance.Phase);
        }

        private void OnDisable()
        {
            GameEvents.OnSoulChanged -= HandleSoulChanged;
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandleSoulChanged(int current, int maximum)
        {
            soul = Mathf.Max(0, current);
            max = Mathf.Max(1, maximum);
            target = (float)soul / max;
        }

        private void HandlePhaseChanged(int phase)
        {
            if (fill != null) fill.color = phase >= GameConstants.Phase2 ? phase2FillTint : phase1FillTint;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (fill != null)
                fill.fillAmount = Mathf.SmoothDamp(fill.fillAmount, target, ref velocity, smoothTime, Mathf.Infinity, dt);

            if (glow != null)
            {
                bool ready = soul >= glowThreshold;
                if (ready)
                {
                    pulseTime += dt;
                    float k = 0.5f + 0.5f * Mathf.Sin(pulseTime * glowPulseHz * Mathf.PI * 2f);
                    Color c = glow.color;
                    c.a = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, k);
                    glow.color = c;
                    if (!glow.enabled) glow.enabled = true;
                }
                else if (glow.enabled)
                {
                    glow.enabled = false;
                    pulseTime = 0f;
                }
            }
        }
    }
}
