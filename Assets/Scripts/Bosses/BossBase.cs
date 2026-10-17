using System.Collections;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Level;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>
    /// Shared boss behaviour (Docs/02 §5, Docs/04): intro (invulnerable, intro card, boss bar fill-in), health
    /// stages + enrage, white hit flash and telegraph flash, melee hit windows against the player's hurtbox,
    /// contact damage, block filter hook, slow-mo death → OnBossDefeated → arena opens its gates, and
    /// OnBossReset when the fight is abandoned. Spawned by <see cref="BossArena"/> (or standalone in the Sandbox).
    /// Bosses never stagger and take no knockback. Subclasses write the fight as a coroutine.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public abstract class BossBase : MonoBehaviour, IBossTarget
    {
        [SerializeField] protected BossConfig config;
        [SerializeField] protected Transform visualRoot;
        [SerializeField] protected SpriteRenderer body;
        [SerializeField] protected SpriteAnimator animator;
        [SerializeField] protected SpriteRenderer flashRenderer;
        [SerializeField] protected Collider2D bodyCollider;
        [SerializeField] protected Collider2D hurtbox;
        [SerializeField] protected DamageDealer contact;
        [SerializeField] private bool autoStartWithoutArena = true;

        protected Rigidbody2D rb;
        protected Health health;
        protected int facing = -1;

        private BossArena arena;
        private bool started;
        private bool dead;
        private int stage = 1;
        private float flashTimer;
        private ContactFilter2D playerFilter;
        private readonly Collider2D[] hitBuffer = new Collider2D[4];
        private readonly float[] attemptTimes = new float[16];
        private int attemptIndex;
        private Vector2 debugBoxCenter, debugBoxSize;
        private float debugBoxUntil;

        public string BossId => config != null ? config.id : name;
        public BossConfig Config => config;
        public bool IsDead => dead;
        public bool IsFighting => started && !dead;
        public int Stage => stage;
        public BossArena Arena => arena;

        protected bool IsEnraged => config.enrageBelowFraction > 0f && health.Fraction < config.enrageBelowFraction;
        protected float TelegraphReduction => IsEnraged ? config.enrageTelegraphReduction : 0f;
        protected float SpeedMultiplier => IsEnraged ? config.enrageSpeedMultiplier : 1f;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            if (config == null)
            {
                Debug.LogError($"{name}: no BossConfig assigned — using defaults.", this);
                config = ScriptableObject.CreateInstance<BossConfig>();
            }
            health.Configure(config.maxHp, 0f, config.hitFlashDuration);
            health.DamageFilter = FilterDamage;
            if (contact != null) contact.Configure(config.contactDamage, DamageType.Contact, gameObject);

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = config.gravityScale;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            playerFilter = new ContactFilter2D();
            playerFilter.SetLayerMask(GameLayers.PlayerHurtboxMask);
            playerFilter.useTriggers = true;

            if (flashRenderer != null) flashRenderer.enabled = false;
            for (int i = 0; i < attemptTimes.Length; i++) attemptTimes[i] = float.NegativeInfinity;
        }

        protected virtual void OnEnable()
        {
            health.OnHitFlash += HandleHitFlash;
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDied += HandleDied;
        }

        protected virtual void OnDisable()
        {
            health.OnHitFlash -= HandleHitFlash;
            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDied -= HandleDied;
        }

        protected virtual void Start()
        {
            FacePlayer();
            if (!started && arena == null && autoStartWithoutArena) Begin();
        }

        protected virtual void OnDestroy()
        {
            // Removed mid-fight (player death, sandbox clear): hide the bar without counting a victory.
            if (started && !dead) GameEvents.RaiseBossReset(BossId);
        }

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        public void Bind(BossArena owner) => arena = owner;

        public void Begin()
        {
            if (started) return;
            started = true;
            StartCoroutine(Lifecycle());
        }

        private IEnumerator Lifecycle()
        {
            health.Invulnerable = true;
            FacePlayer();
            Play("Intro");
            GameEvents.RaiseBossStarted(BossId, config.displayName, config.stageThresholds);
            GameEvents.RaiseBossIntroCard(config.displayName, config.epithet);
            GameEvents.RaiseBossHealthChanged(BossId, health.CurrentHp, health.MaxHp);
            AudioManager.Play("boss_intro");
            yield return Wait(config.introDuration);
            health.Invulnerable = false;
            Play("Idle");
            yield return Fight();
        }

        /// <summary>The boss's move loop. Runs until death (then all coroutines stop).</summary>
        protected abstract IEnumerator Fight();

        /// <summary>Called when HP crosses a stage threshold (Zulmat). Default: nothing.</summary>
        protected virtual void OnStageChanged(int newStage) { }

        /// <summary>Return false to block a hit (guard). Default: every hit lands.</summary>
        protected virtual bool AllowDamage(DamageInfo info) => true;

        // ------------------------------------------------------------------
        // Helpers for subclasses
        // ------------------------------------------------------------------

        /// <summary>Scaled-time wait, so hit-stop and pause freeze the boss too.</summary>
        protected IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        protected static bool TryGetPlayer(out PlayerController p)
        {
            p = PlayerController.Active;
            return p != null && p.State != PlayerState.Dead;
        }

        protected float DistanceToPlayer => TryGetPlayer(out PlayerController p) ? Mathf.Abs(p.Position.x - rb.position.x) : float.MaxValue;

        protected bool PlayerIsBehind => TryGetPlayer(out PlayerController p) && Mathf.Sign(p.Position.x - rb.position.x) != facing;

        protected void FacePlayer()
        {
            if (TryGetPlayer(out PlayerController p)) SetFacing(p.Position.x >= transform.position.x ? 1 : -1);
        }

        protected void SetFacing(int direction)
        {
            if (direction == 0) return;
            facing = direction > 0 ? 1 : -1;
            if (visualRoot == null) return;
            Vector3 s = visualRoot.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            visualRoot.localScale = s;
        }

        protected void SetVelocityX(float vx)
        {
            Vector2 v = rb.linearVelocity;
            v.x = vx;
            rb.linearVelocity = v;
        }

        protected void Stop() => SetVelocityX(0f);

        protected void Play(string clip, bool restart = true)
        {
            if (animator != null && !animator.Play(clip, restart) && clip != "Idle") animator.Play("Idle");
        }

        protected bool ClipFinished(string clip) => animator != null && animator.CurrentClip == clip && animator.IsFinished;

        protected void Flash(Color color, float duration)
        {
            if (flashRenderer == null || duration <= 0f) return;
            flashRenderer.color = color;
            if (body != null) flashRenderer.sprite = body.sprite;
            flashRenderer.enabled = true;
            flashTimer = Mathf.Max(flashTimer, duration);
        }

        /// <summary>Docs/04: every attack starts with a flash (plus the caller's telegraph clip).</summary>
        protected void TelegraphFlash() => Flash(config.telegraphFlashColor, config.telegraphFlashTime);

        /// <summary>Number of player hit attempts (landed or blocked) in the last <paramref name="seconds"/>.</summary>
        protected int AttemptsInLast(float seconds)
        {
            float since = Time.time - seconds;
            int n = 0;
            for (int i = 0; i < attemptTimes.Length; i++) if (attemptTimes[i] >= since) n++;
            return n;
        }

        /// <summary>
        /// A melee hit window: a box in front of the boss (from <paramref name="nearX"/> to <paramref name="reach"/> along
        /// the facing direction, <paramref name="height"/> tall starting at <paramref name="bottomY"/>), checked every frame
        /// for <paramref name="duration"/>; hits the player at most once.
        /// </summary>
        protected IEnumerator HitWindow(float reach, float duration, int damage, float height = 2f, float bottomY = 0.2f, float nearX = 0.3f)
        {
            bool hit = false;
            float t = 0f;
            do
            {
                float width = Mathf.Max(0.1f, reach - nearX);
                Vector2 center = rb.position + new Vector2(facing * (nearX + width * 0.5f), bottomY + height * 0.5f);
                var size = new Vector2(width, height);
                debugBoxCenter = center; debugBoxSize = size; debugBoxUntil = Time.time + 0.05f;
                if (!hit) hit = TryHitPlayer(center, size, damage);
                t += Time.deltaTime;
                yield return null;
            } while (t < duration);
        }

        protected bool TryHitPlayer(Vector2 center, Vector2 size, int damage)
        {
            int n = Physics2D.OverlapBox(center, size, 0f, playerFilter, hitBuffer);
            for (int i = 0; i < n; i++)
            {
                PlayerHealth ph = hitBuffer[i].GetComponentInParent<PlayerHealth>();
                if (ph == null) continue;
                float dir = ph.transform.position.x >= rb.position.x ? 1f : -1f;
                ph.ReceiveHit(new DamageInfo(damage, new Vector2(dir, 0f), 1f, gameObject, DamageType.Melee));
                return true;
            }
            return false;
        }

        // ------------------------------------------------------------------
        // Health / death
        // ------------------------------------------------------------------

        private bool FilterDamage(DamageInfo info)
        {
            attemptTimes[attemptIndex] = Time.time;
            attemptIndex = (attemptIndex + 1) % attemptTimes.Length;
            return AllowDamage(info);
        }

        private void HandleHitFlash(float duration) => Flash(Color.white, duration);

        private void HandleHealthChanged(int current, int max)
        {
            if (!started) return;
            GameEvents.RaiseBossHealthChanged(BossId, current, max);
            int newStage = config.StageForFraction(max > 0 ? (float)current / max : 0f);
            if (newStage > stage && current > 0)
            {
                stage = newStage;
                OnStageChanged(stage);
            }
        }

        private void HandleDied(DamageInfo info)
        {
            if (dead) return;
            dead = true;
            StopAllCoroutines();
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            if (hurtbox != null) hurtbox.enabled = false;
            if (contact != null)
            {
                contact.Active = false;
                Collider2D c = contact.GetComponent<Collider2D>();
                if (c != null) c.enabled = false;
            }
            Stop();
            Play("Death");
            AudioManager.Play("boss_death");
            OnDeathStarted();

            // Docs/04: slow-mo death. Real-time wait so the slow-mo length is exact; paused time doesn't count.
            HitStop.Cancel();
            Time.timeScale = config.deathSlowmoScale;
            float t = 0f;
            while (t < config.deathSlowmoTime)
            {
                if (!(GameManager.HasInstance && GameManager.Instance.IsPaused)) t += Time.unscaledDeltaTime;
                yield return null;
            }
            if (!(GameManager.HasInstance && GameManager.Instance.IsPaused)) Time.timeScale = 1f;

            FxPool.Play("Death_Smoke", rb.position + new Vector2(0f, config.colliderSize.y * 0.5f), 1, 2f);
            GameEvents.RaiseBossDefeated(BossId);
            Destroy(gameObject);
        }

        protected virtual void OnDeathStarted() { }

        // ------------------------------------------------------------------
        // Frame
        // ------------------------------------------------------------------

        protected virtual void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.unscaledDeltaTime;
                if (flashTimer <= 0f && flashRenderer != null) flashRenderer.enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (flashRenderer != null && flashRenderer.enabled && body != null) flashRenderer.sprite = body.sprite;
        }

        protected virtual void OnDrawGizmos()
        {
            if (!DebugCommands.ShowHitboxGizmos || !Application.isPlaying) return;
            if (Time.time <= debugBoxUntil)
            {
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
                Gizmos.DrawCube(debugBoxCenter, debugBoxSize);
            }
            if (hurtbox != null && hurtbox.enabled)
            {
                Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.8f);
                Gizmos.DrawWireCube(hurtbox.bounds.center, hurtbox.bounds.size);
            }
        }
    }
}
