using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// One call per game-feel moment (Prompt 17): <c>Feel.Play(FeelEvent.EnemyDeath)</c> applies that preset's hit-stop,
    /// camera shake and gamepad rumble from <see cref="FeelConfig"/> (Resources/FeelConfig), so the same kind of hit always
    /// feels the same and every number is tunable in one asset.
    /// </summary>
    public static class Feel
    {
        private static FeelConfig config;

        public static FeelConfig Config
        {
            get
            {
                if (config == null)
                {
                    config = Resources.Load<FeelConfig>(FeelConfig.ResourcePath);
                    if (config == null) config = ScriptableObject.CreateInstance<FeelConfig>();
                    Rumble.Enabled = config.rumbleEnabled;
                    Rumble.Strength = config.rumbleStrength;
                }
                return config;
            }
        }

        /// <summary>
        /// Plays a preset. <paramref name="hitStopOverride"/> ≥ 0 replaces the preset's hit-stop (player hits use PlayerConfig);
        /// <paramref name="shakeScale"/> scales the shake (e.g. by the attacker's size).
        /// </summary>
        public static void Play(FeelEvent id, float hitStopOverride = -1f, float shakeScale = 1f)
        {
            FeelPreset p = Config.Find(id);
            if (p == null) return;
            float stop = hitStopOverride >= 0f ? hitStopOverride : p.hitStop;
            if (stop > 0f) HitStop.Do(stop);
            if (p.shakeAmplitude > 0f) ScreenShake.Shake(p.shakeAmplitude * shakeScale, p.shakeDuration);
            if (p.rumbleDuration > 0f) Rumble.Pulse(p.rumbleLow, p.rumbleHigh, p.rumbleDuration);
        }

        /// <summary>Only the rumble of a preset (when hit-stop and shake already come from another config, e.g. player hurt).</summary>
        public static void RumbleOnly(FeelEvent id)
        {
            FeelPreset p = Config.Find(id);
            if (p != null && p.rumbleDuration > 0f) Rumble.Pulse(p.rumbleLow, p.rumbleHigh, p.rumbleDuration);
        }
    }
}
