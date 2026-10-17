using UnityEngine;

namespace Shatranj.Data
{
    public enum FlameMotion { PingPong, OneWay, Parked }
    public enum FlameScaleMode { Monotonic, Peak }

    /// <summary>Pure flame maths from Docs/07 §3-4, kept static so EditMode tests can cover it.</summary>
    public static class FlameMath
    {
        /// <summary>pos(t) = (lerp(x0, x1, t), baseY + height·sin(π·t)), viewport coords 0..1.</summary>
        public static Vector2 Arc(float t, float startX, float endX, float baseY, float height)
        {
            t = Mathf.Clamp01(t);
            return new Vector2(Mathf.Lerp(startX, endX, t), baseY + height * Mathf.Sin(Mathf.PI * t));
        }

        /// <summary>closeness = t (Monotonic) or sin(π·t) (Peak).</summary>
        public static float Closeness(float t, FlameScaleMode mode)
        {
            t = Mathf.Clamp01(t);
            return mode == FlameScaleMode.Peak ? Mathf.Sin(Mathf.PI * t) : t;
        }

        /// <summary>scale = lerp(1, max, easeInQuad(closeness)).</summary>
        public static float Scale(float t, FlameScaleMode mode, float maxScale)
        {
            float c = Closeness(t, mode);
            return Mathf.Lerp(1f, maxScale, c * c);
        }

        /// <summary>Ping-pong progress for a cosmetic sweep: 0→1→0 over 2·sweepDuration.</summary>
        public static float PingPong(float elapsedSeconds, float sweepDuration)
        {
            if (sweepDuration <= 0f) return 0f;
            return Mathf.PingPong(elapsedSeconds / sweepDuration, 1f);
        }
    }

    /// <summary>Flame behaviour per phase (Docs/07 §3) and the enemy-scaling curve (Docs/07 §4).</summary>
    [CreateAssetMenu(menuName = "Shatranj/Configs/Flame Config", fileName = "FlameConfig")]
    public class FlameConfig : ScriptableObject
    {
        [Header("Identity")]
        public string id = "Flame_Phase1";
        [Range(1, 2)] public int phase = 1;

        [Header("Motion")]
        public FlameMotion motion = FlameMotion.PingPong;
        [Tooltip("Seconds for one sweep of the arc (P1 480 ping-pong, P2 1200 one-way).")]
        public float sweepDuration = 480f;
        public float arcStartX = 0.05f;
        public float arcEndX = 0.95f;
        public float arcBaseY = 0.05f;
        public float arcHeight = 0.85f;
        [Tooltip("Viewport position while parked (final boss: top-centre).")]
        public Vector2 parkedPosition = new Vector2(0.5f, 0.85f);
        public float parkedIntensityMultiplier = 1.2f;

        [Header("Look")]
        public Color coreColor = new Color(1f, 0.824f, 0.478f);  // #FFD27A
        public Color glowColor = new Color(1f, 0.541f, 0.169f);  // #FF8A2B
        public float baseIntensity = 1.2f;
        [Range(0f, 0.5f)] public float flickerAmplitude = 0.08f;
        public float flickerFrequency = 6f;
        public float hitSpikeAmount = 0.3f;
        public float hitSpikeDuration = 0.1f;

        [Header("Enemy scaling (Docs/07 §4)")]
        public bool affectsEnemyScale = false;
        public float scaleMax = 1.4f;
        public FlameScaleMode scaleMode = FlameScaleMode.Monotonic;
        [Tooltip("Lerp speed (per second) toward the target scale.")]
        public float scaleLerpSpeed = 2f;

        public Vector2 EvaluateArc(float t) => FlameMath.Arc(t, arcStartX, arcEndX, arcBaseY, arcHeight);

        public float EvaluateScale(float t) => affectsEnemyScale ? FlameMath.Scale(t, scaleMode, scaleMax) : 1f;
    }
}
