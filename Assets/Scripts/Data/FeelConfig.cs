using System;
using UnityEngine;

namespace Shatranj.Data
{
    /// <summary>Named game-feel moments (Prompt 17). One preset each, so every hit of the same kind feels the same.</summary>
    public enum FeelEvent
    {
        PlayerHitEnemy,
        PlayerHitBoss,
        PlayerHurt,
        PlayerDeath,
        PlayerDash,
        GuardBlock,
        EnemyDeath,
        LightImpact,
        MediumImpact,
        HeavyImpact,
        BossStageChange,
        BossDeath,
        BenchRest,
    }

    /// <summary>Hit-stop + camera shake + gamepad rumble for one <see cref="FeelEvent"/>.</summary>
    [Serializable]
    public class FeelPreset
    {
        public FeelEvent id;
        [Tooltip("Real-time freeze (s). 0 = none. Player hits use PlayerConfig's hit-stop instead.")]
        public float hitStop;
        public float shakeAmplitude;
        public float shakeDuration;
        [Range(0f, 1f)] public float rumbleLow;
        [Range(0f, 1f)] public float rumbleHigh;
        public float rumbleDuration;

        public FeelPreset() { }

        public FeelPreset(FeelEvent id, float hitStop, float shakeAmplitude, float shakeDuration, float rumbleLow, float rumbleHigh, float rumbleDuration)
        {
            this.id = id;
            this.hitStop = hitStop;
            this.shakeAmplitude = shakeAmplitude;
            this.shakeDuration = shakeDuration;
            this.rumbleLow = rumbleLow;
            this.rumbleHigh = rumbleHigh;
            this.rumbleDuration = rumbleDuration;
        }
    }

    /// <summary>
    /// Game-feel presets (Docs/02 §4, Docs/03 §11, CLAUDE.md §5): hit-stop, shake and rumble for every recurring moment.
    /// Lives in Resources so <c>Feel</c> finds it without references; missing asset → these defaults.
    /// </summary>
    [CreateAssetMenu(menuName = "Shatranj/Configs/Feel Config", fileName = "FeelConfig")]
    public class FeelConfig : ScriptableObject
    {
        public const string ResourcePath = "FeelConfig";

        [Tooltip("Gamepad rumble on/off (also a player option later).")]
        public bool rumbleEnabled = true;
        [Range(0f, 1f)] public float rumbleStrength = 1f;

        public FeelPreset[] presets = Defaults();

        public FeelPreset Find(FeelEvent id)
        {
            if (presets != null)
                for (int i = 0; i < presets.Length; i++)
                    if (presets[i] != null && presets[i].id == id) return presets[i];
            return null;
        }

        /// <summary>The tuned defaults (also used by the config generator).</summary>
        public static FeelPreset[] Defaults() => new[]
        {
            //               event                      hitStop shakeAmp shakeDur  rLow  rHigh  rDur
            new FeelPreset(FeelEvent.PlayerHitEnemy,   0f,     0.06f,   0.08f,    0.15f, 0.25f, 0.06f),
            new FeelPreset(FeelEvent.PlayerHitBoss,    0f,     0.10f,   0.10f,    0.25f, 0.35f, 0.08f),
            new FeelPreset(FeelEvent.PlayerHurt,       0f,     0f,      0f,       0.60f, 0.80f, 0.25f),
            new FeelPreset(FeelEvent.PlayerDeath,      0f,     0.40f,   0.40f,    1.00f, 1.00f, 0.50f),
            new FeelPreset(FeelEvent.PlayerDash,       0f,     0f,      0f,       0.10f, 0f,    0.05f),
            new FeelPreset(FeelEvent.GuardBlock,       0.04f,  0.05f,   0.06f,    0.20f, 0.10f, 0.06f),
            new FeelPreset(FeelEvent.EnemyDeath,       0.03f,  0.08f,   0.12f,    0.20f, 0.30f, 0.08f),
            new FeelPreset(FeelEvent.LightImpact,      0f,     0.07f,   0.10f,    0.10f, 0.20f, 0.06f),
            new FeelPreset(FeelEvent.MediumImpact,     0f,     0.15f,   0.18f,    0.35f, 0.45f, 0.15f),
            new FeelPreset(FeelEvent.HeavyImpact,      0f,     0.30f,   0.30f,    0.60f, 0.80f, 0.25f),
            new FeelPreset(FeelEvent.BossStageChange,  0f,     0.35f,   0.60f,    0.50f, 0.60f, 0.50f),
            new FeelPreset(FeelEvent.BossDeath,        0f,     0.50f,   0.80f,    0.80f, 1.00f, 0.70f),
            new FeelPreset(FeelEvent.BenchRest,        0f,     0f,      0f,       0.15f, 0.10f, 0.12f),
        };
    }
}
