using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// The flame behind the paper (Docs/07 §3). Owns the arc progress t and the screen position:
    /// Phase 1 = slow cosmetic ping-pong (480 s per sweep), Phase 2 = one-way 20-minute arc persisted in the save
    /// (benches store it, death restores it — Docs/01 §12.8), parking for the final boss, Perlin flicker and short
    /// intensity spikes on boss hits. Publishes progress through <see cref="GameEvents.RaiseFlameProgressChanged"/>.
    /// Colours come from the active theme (ThemeManager); this class only decides where and how bright.
    /// </summary>
    [DisallowMultipleComponent]
    public class FlameController : MonoBehaviour
    {
        public static FlameController Instance { get; private set; }

        [SerializeField] private FlameConfig phase1Config;
        [SerializeField] private FlameConfig phase2Config;
        [SerializeField] private Camera targetCamera;

        private FlameConfig config;
        private float t;
        private float pingPongElapsed;
        private bool parked;
        private Vector2 parkedViewport;
        private float spikeTimer;
        private float noiseSeed;
        private bool manualOverride;
        private float lastPublished = -1f;

        public FlameConfig Config => config;
        /// <summary>Arc progress 0..1.</summary>
        public float Progress => t;
        public bool IsParked => parked;
        public bool ManualOverride { get => manualOverride; set => manualOverride = value; }
        /// <summary>Flame position in viewport coordinates (0..1).</summary>
        public Vector2 ViewportPosition { get; private set; }
        /// <summary>Flame position in world space at the world plane (z = 0).</summary>
        public Vector2 WorldPosition { get; private set; }
        /// <summary>Current light intensity multiplier including flicker, parking boost and spikes.</summary>
        public float Intensity { get; private set; } = 1f;
        /// <summary>Phase 2 enemy scale for the current t (1 in Phase 1).</summary>
        public float EnemyScale => config != null ? config.EvaluateScale(t) : 1f;
        public Camera Camera => targetCamera;

        private void Awake()
        {
            Instance = this;
            if (targetCamera == null) targetCamera = Camera.main;
            noiseSeed = Random.value * 100f;
            SelectConfig(GameManager.HasInstance ? GameManager.Instance.Phase : GameConstants.Phase1, restoreFromSave: true);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            GameEvents.OnPhaseChanged += HandlePhaseChanged;
            GameEvents.OnPlayerDied += HandlePlayerDied;
            GameEvents.OnBossHealthChanged += HandleBossHit;
            DebugCommands.OnFlameProgressRequested += SetProgress;
        }

        private void OnDisable()
        {
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
            GameEvents.OnBossHealthChanged -= HandleBossHit;
            DebugCommands.OnFlameProgressRequested -= SetProgress;
        }

        private void SelectConfig(int phase, bool restoreFromSave)
        {
            FlameConfig next = phase >= GameConstants.Phase2 && phase2Config != null ? phase2Config : phase1Config;
            if (next == null) next = ScriptableObject.CreateInstance<FlameConfig>();
            config = next;
            pingPongElapsed = 0f;
            t = 0f;
            if (restoreFromSave && config.motion == FlameMotion.OneWay && GameManager.HasInstance)
                t = Mathf.Clamp01(GameManager.Instance.Save.flameT);
            Publish(force: true);
        }

        private void HandlePhaseChanged(int phase) => SelectConfig(phase, restoreFromSave: false);

        /// <summary>Docs/01 §12.8: dying restores the bench's flame progress, so deaths don't compound the size penalty.</summary>
        private void HandlePlayerDied()
        {
            if (config.motion != FlameMotion.OneWay || !GameManager.HasInstance) return;
            t = Mathf.Clamp01(GameManager.Instance.Save.flameT);
            Publish(force: true);
        }

        private void HandleBossHit(string bossId, int current, int max) => Spike();

        // ------------------------------------------------------------------

        private void Update()
        {
            float dt = Time.deltaTime;
            bool advancing = !manualOverride && !parked && (!GameManager.HasInstance || GameManager.Instance.IsPlaying);

            if (advancing)
            {
                switch (config.motion)
                {
                    case FlameMotion.PingPong:
                        pingPongElapsed += dt;
                        t = FlameMath.PingPong(pingPongElapsed, config.sweepDuration);
                        break;
                    case FlameMotion.OneWay:
                        if (config.sweepDuration > 0f) t = Mathf.Min(1f, t + dt / config.sweepDuration);
                        break;
                }
            }

            ViewportPosition = parked ? parkedViewport : config.EvaluateArc(t);
            if (targetCamera != null)
            {
                Vector3 w = targetCamera.ViewportToWorldPoint(new Vector3(ViewportPosition.x, ViewportPosition.y, -targetCamera.transform.position.z));
                WorldPosition = w;
            }

            // Flicker: Perlin noise around 1 (±amplitude at the configured frequency), plus parking boost and hit spikes.
            float n = Mathf.PerlinNoise(noiseSeed, Time.time * config.flickerFrequency) * 2f - 1f;
            float flicker = 1f + n * config.flickerAmplitude;
            float spike = 1f;
            if (spikeTimer > 0f)
            {
                spikeTimer -= Time.unscaledDeltaTime;
                spike += config.hitSpikeAmount * Mathf.Clamp01(spikeTimer / Mathf.Max(0.01f, config.hitSpikeDuration));
            }
            Intensity = flicker * spike * (parked ? config.parkedIntensityMultiplier : 1f);

            Publish(force: false);
        }

        private void Publish(bool force)
        {
            if (!force && Mathf.Abs(t - lastPublished) < 0.0005f) return;
            lastPublished = t;
            GameEvents.RaiseFlameProgressChanged(t);
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        /// <summary>Jumps the arc to <paramref name="value"/> (F8 debug, F11 scrubbing, cutscene beats).</summary>
        public void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);
            t = value;
            if (config.motion == FlameMotion.PingPong) pingPongElapsed = value * config.sweepDuration;
            Publish(force: true);
        }

        /// <summary>Docs/07 §3: the final boss parks the flame (default top-centre, intensity ×1.2).</summary>
        public void Park(Vector2? viewport = null)
        {
            parked = true;
            parkedViewport = viewport ?? config.parkedPosition;
        }

        public void Unpark() => parked = false;

        /// <summary>Docs/07 §6: the flame light spikes +30 % for 0.1 s (boss hits, boss moves).</summary>
        public void Spike() => spikeTimer = config.hitSpikeDuration;
    }
}
