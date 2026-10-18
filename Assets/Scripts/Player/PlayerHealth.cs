using System.Collections;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Docs/03 §7: hearts (5 → 7 by abilities), hurt knockback + stun + 1.2 s i-frames with blink, hit-stop and
    /// shake on being hit, hazards (spikes → safe-ground respawn), death flow (slow-mo → fade → respawn).
    /// Wraps the generic <see cref="Health"/> component; raises the player events on <see cref="GameEvents"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private Transform respawnPoint;

        private Health health;
        private PlayerController controller;
        private PlayerAnimator animator;
        private PlayerSoul soul;
        private PlayerFxPools fx;
        private PlayerConfig config;

        private Vector2 lastSafePosition;
        private float safeTimer;
        private bool dying;
        private Coroutine hazardRoutine;
        private ContactFilter2D hazardFilter;
        private readonly Collider2D[] hazardOverlaps = new Collider2D[2];

        public int Hearts => health.CurrentHp;
        public int MaxHearts => health.MaxHp;
        public bool IsAlive => health.IsAlive && !dying;
        public bool IsInvulnerable => health.IsInvulnerable;
        public Vector2 LastSafePosition => lastSafePosition;
        /// <summary>Where death respawns. The spawn point now; benches set it from Prompt 6.</summary>
        public Vector2 RespawnPosition { get; set; }
        public Health Health => health;

        private void Awake()
        {
            health = GetComponent<Health>();
            controller = GetComponent<PlayerController>();
            animator = GetComponent<PlayerAnimator>();
            soul = GetComponent<PlayerSoul>();
            fx = GetComponent<PlayerFxPools>();
            config = controller.Config;

            int hearts = GameManager.HasInstance ? GameManager.Instance.Abilities.maxHearts : config.heartsPhase1;
            health.Configure(hearts, config.hurtInvulnerabilityTime, 0.08f);

            RespawnPosition = respawnPoint != null ? (Vector2)respawnPoint.position : (Vector2)transform.position;
            lastSafePosition = transform.position;

            hazardFilter = new ContactFilter2D();
            hazardFilter.SetLayerMask(GameLayers.HazardMask);
            hazardFilter.useTriggers = true;
        }

        /// <summary>True while the body overlaps any Hazard-layer collider (spikes, lava).</summary>
        private bool TouchingHazard()
        {
            Vector2 center = (Vector2)transform.position + config.colliderOffset;
            return Physics2D.OverlapBox(center, config.colliderSize, 0f, hazardFilter, hazardOverlaps) > 0;
        }

        private void OnEnable()
        {
            health.OnDamaged += HandleDamaged;
            health.OnDied += HandleDied;
            health.OnHealthChanged += HandleHealthChanged;
            GameEvents.OnAbilitiesChanged += HandleAbilitiesChanged;
            DebugCommands.OnGodModeChanged += HandleGodMode;
            health.Invulnerable = DebugCommands.GodMode;
        }

        private void OnDisable()
        {
            health.OnDamaged -= HandleDamaged;
            health.OnDied -= HandleDied;
            health.OnHealthChanged -= HandleHealthChanged;
            GameEvents.OnAbilitiesChanged -= HandleAbilitiesChanged;
            DebugCommands.OnGodModeChanged -= HandleGodMode;
        }

        private void Start()
        {
            GameEvents.RaiseHealthChanged(health.CurrentHp, health.MaxHp);
        }

        private const float OutOfWorldMargin = 4f;

        private void Update()
        {
            // Safety net: fell below every room (collision hole / bad geometry) → back to safe ground, logged.
            if (!dying && hazardRoutine == null && Level.RoomManager.Instance != null
                && transform.position.y < Level.RoomManager.Instance.WorldBottom - OutOfWorldMargin)
            {
                Debug.LogWarning($"PlayerHealth: fell out of the level near x={transform.position.x:0.0} — returning to safe ground. Check the collision in that room.");
                controller.Respawn(lastSafePosition);
                if (Level.RoomManager.Instance != null) Level.RoomManager.Instance.LocatePlayer(instant: true);
                if (CameraController.Main != null) CameraController.Main.SnapToTarget();
                return;
            }

            // Docs/03 §7: safe ground = standing grounded for 0.5 s outside hazards (hurt/hazard states don't count).
            PlayerState s = controller.State;
            bool calm = controller.IsGrounded && (s == PlayerState.Idle || s == PlayerState.Run || s == PlayerState.Land) && !TouchingHazard();
            if (calm)
            {
                safeTimer += Time.deltaTime;
                if (safeTimer >= config.safeGroundRecordTime) lastSafePosition = transform.position;
            }
            else safeTimer = 0f;
        }

        // ------------------------------------------------------------------
        // Receiving hits
        // ------------------------------------------------------------------

        public bool ReceiveHit(DamageDealer dealer, Vector2 hitPoint)
        {
            if (dying) return false;
            return health.TakeDamage(dealer.BuildDamageInfo(hitPoint, transform.position));
        }

        public bool ReceiveHit(DamageInfo info)
        {
            if (dying) return false;
            return health.TakeDamage(info);
        }

        private void HandleDamaged(DamageInfo info)
        {
            HitStop.Do(config.hurtHitStop);
            ScreenShake.Shake(config.hurtShakeAmplitude, config.hurtShakeDuration);
            Feel.RumbleOnly(FeelEvent.PlayerHurt);
            AudioManager.Play("player_hurt");
            GameEvents.RaisePlayerDamaged(info);

            if (!health.IsAlive) return; // death flow takes over

            float dirX = info.knockbackDir.x != 0f ? Mathf.Sign(info.knockbackDir.x) : -controller.Facing;
            var knockback = new Vector2(dirX * config.hurtKnockbackX, config.hurtKnockbackY);
            controller.ApplyHurt(knockback, config.hurtStunTime);
            if (animator != null) animator.StartBlink(config.hurtInvulnerabilityTime, config.hurtBlinkHz);

            if (info.type == DamageType.Hazard) AudioManager.Play("hazard_spikes");
            else if (info.type == DamageType.Lava) AudioManager.Play("hazard_lava");

            if (info.type == DamageType.Hazard)
            {
                if (hazardRoutine != null) StopCoroutine(hazardRoutine);
                hazardRoutine = StartCoroutine(HazardRespawn());
            }
        }

        private IEnumerator HazardRespawn()
        {
            yield return new WaitForSeconds(config.hazardRespawnDelay);
            if (dying) yield break;

            SceneFlow flow = SceneFlow.Instance;
            flow.FadeTo(1f, config.hazardRespawnFadeTime);
            yield return new WaitForSecondsRealtime(config.hazardRespawnFadeTime);

            // Docs/05 legend 'S': an explicit safe point in the current room wins over the last recorded ground.
            Vector2 target = lastSafePosition;
            if (Level.RoomManager.Instance != null && Level.RoomManager.Instance.TryNearestSafePoint(transform.position, out Vector2 safePoint))
                target = safePoint;
            controller.Respawn(target);
            health.GrantInvulnerability(config.hurtInvulnerabilityTime);
            if (animator != null) animator.StartBlink(config.hurtInvulnerabilityTime, config.hurtBlinkHz);
            if (CameraController.Main != null) CameraController.Main.SnapToTarget();

            flow.FadeTo(0f, config.hazardRespawnFadeTime);
            hazardRoutine = null;
        }

        private void HandleHealthChanged(int current, int max) => GameEvents.RaiseHealthChanged(current, max);

        private void HandleAbilitiesChanged(PlayerAbilities abilities)
        {
            // Docs/03 §7: full heal on phase change — refill when hearts grow, clamp when they shrink (debug toggle).
            bool grew = abilities.maxHearts > health.MaxHp;
            health.SetMax(abilities.maxHearts, refill: grew);
        }

        private void HandleGodMode(bool on) => health.Invulnerable = on;

        // ------------------------------------------------------------------
        // Healing
        // ------------------------------------------------------------------

        public bool IsFull => health.IsFull;

        /// <summary>Restores one heart (Docs/03 §5 heal). Returns false when already full or dead.</summary>
        public bool HealHearts(int amount) => health.Heal(amount) > 0;

        /// <summary>Benches and phase changes.</summary>
        public void FullHeal() => health.Heal(health.MaxHp);

        // ------------------------------------------------------------------
        // Death (Docs/03 §7)
        // ------------------------------------------------------------------

        private void HandleDied(DamageInfo info)
        {
            if (dying) return;
            dying = true;
            if (hazardRoutine != null) { StopCoroutine(hazardRoutine); hazardRoutine = null; }

            controller.Die();
            if (animator != null) animator.StopBlink();
            if (GameManager.HasInstance) GameManager.Instance.SetState(GameState.Dead);
            AudioManager.Play("player_death");
            Feel.Play(FeelEvent.PlayerDeath);
            GameEvents.RaisePlayerDied();
            StartCoroutine(DeathFlow());
        }

        private void OnDestroy() => TimeScaleGuard.EndSlowMo(this);   // scene change mid death slow-mo

        private IEnumerator DeathFlow()
        {
            HitStop.Cancel();
            TimeScaleGuard.BeginSlowMo(this, config.deathSlowmoScale);
            yield return new WaitForSecondsRealtime(config.deathSlowmoDuration);
            TimeScaleGuard.EndSlowMo(this);

            // Docs/07 §6: death uses the burn wipe (orange-rimmed circle closing on the puppet); plain fade as fallback.
            ScreenWipe wipe = ScreenWipe.Instance;
            if (wipe != null)
            {
                yield return wipe.BurnClose(transform.position, config.deathFadeTime);
                // Docs/08 §7: "You were extinguished" for ~1.2 s on black.
                AudioManager.PlayVoice("vo_extinguished");
                yield return UI.ScreenCard.Show(this, "You were extinguished", "", 0f, 0.3f, 0.6f, new Color(0.91f, 0.85f, 0.71f));
                Respawn();
                yield return null;
                yield return wipe.BurnOpen(transform.position, config.deathFadeTime);
                yield break;
            }

            SceneFlow flow = SceneFlow.Instance;
            flow.FadeTo(1f, config.deathFadeTime);
            yield return new WaitForSecondsRealtime(config.deathFadeTime + 0.05f);

            Respawn();
            yield return null;
            flow.FadeTo(0f, config.deathFadeTime);
        }

        /// <summary>Back to the respawn point with full hearts and zero soul (Docs/01 §10).</summary>
        public void Respawn()
        {
            if (fx != null) fx.ReleaseAll();
            controller.Respawn(RespawnPosition);
            health.ResetToFull();
            if (soul != null) soul.ResetSoul();
            if (animator != null) animator.StopBlink();
            lastSafePosition = RespawnPosition;
            safeTimer = 0f;
            dying = false;
            if (GameManager.HasInstance) GameManager.Instance.SetState(GameState.Playing);
            if (Level.RoomManager.Instance != null) Level.RoomManager.Instance.LocatePlayer(instant: true);
            if (CameraController.Main != null) CameraController.Main.SnapToTarget();
        }
    }
}
