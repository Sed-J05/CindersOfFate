using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Level;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Shared enemy behaviour (Docs/02 §5, Docs/04 global rules): HP via <see cref="Health"/>, white hit-flash,
    /// small knockback, Hurt/Death clips, Death_Smoke, contact damage (EnemyAttack trigger + DamageDealer),
    /// room activation, stable-id kill persistence through the spawner, telegraph flash + '!' marker,
    /// and the flame-scale hook (no effect unless the config scales with the flame — Phase 2).
    /// Subclasses implement <see cref="TickAI"/>. Soul on hit is granted by PlayerCombat.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public abstract class EnemyBase : MonoBehaviour, IRoomActivatable, ISpawnedEnemy
    {
        [SerializeField] protected EnemyConfig config;
        [SerializeField] protected Transform visualRoot;
        [SerializeField] protected SpriteRenderer body;
        [SerializeField] protected SpriteAnimator animator;
        [SerializeField] protected SpriteRenderer flashRenderer;
        [SerializeField] protected GameObject exclamation;
        [SerializeField] protected Collider2D bodyCollider;
        [SerializeField] protected Collider2D hurtbox;
        [SerializeField] protected DamageDealer contact;

        protected Rigidbody2D rb;
        protected Health health;
        protected int facing = 1;

        private EnemySpawner spawner;
        private string roomId = "";
        private bool roomActive = true;
        private bool dead;
        private bool dying;
        private float deathTimer;
        private float knockbackTimer;
        private Vector2 knockbackVelocity;
        private float flashTimer;
        private float hurtTimer;
        private string desiredClip;

        public EnemyConfig Config => config;
        public Health Health => health;
        public bool IsDead => dead;
        public int Facing => facing;
        public string RoomId => roomId;
        public bool RoomActive => roomActive;
        public float FlameScale { get; private set; } = 1f;

        protected bool IsFlyer => config.locomotion == EnemyLocomotion.Flyer;
        protected bool InKnockback => knockbackTimer > 0f;
        /// <summary>Telegraph / active phases: no Hurt clip interruption, optional knockback immunity.</summary>
        protected virtual bool IsAttacking => false;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            if (config == null)
            {
                Debug.LogError($"{name}: no EnemyConfig assigned — using defaults.", this);
                config = ScriptableObject.CreateInstance<EnemyConfig>();
            }

            health.Configure(config.maxHp, 0f, config.hitFlashDuration);
            if (contact != null) contact.Configure(config.contactDamage, DamageType.Contact, gameObject);

            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (IsFlyer)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = config.gravityScale;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            if (flashRenderer != null) flashRenderer.enabled = false;
            if (exclamation != null) exclamation.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            health.OnHitFlash += HandleHitFlash;
            health.OnDamaged += HandleDamaged;
            health.OnDied += HandleDied;
        }

        protected virtual void OnDisable()
        {
            health.OnHitFlash -= HandleHitFlash;
            health.OnDamaged -= HandleDamaged;
            health.OnDied -= HandleDied;
        }

        protected virtual void Start()
        {
            SetFacing(-1);
            ResolveHomeRoom();
        }

        // ------------------------------------------------------------------
        // Home room (D-137): enemies never leave the room they belong to
        // ------------------------------------------------------------------

        private bool hasHome;
        private Rect homeRect;
        private const float HomeEdgeMargin = 0.1f;

        /// <summary>World rect of the room this enemy is confined to (only valid when <see cref="HasHome"/>).</summary>
        protected Rect HomeRect => homeRect;
        protected bool HasHome => hasHome;

        private void ResolveHomeRoom()
        {
            RoomManager rooms = RoomManager.Instance;
            if (rooms == null) return;
            RoomBounds home = !string.IsNullOrEmpty(roomId) ? rooms.Find(roomId) : null;
            if (home == null) home = rooms.RoomAt(BodyCenter);
            if (home == null) return;
            homeRect = home.WorldRect;
            hasHome = true;
        }

        /// <summary>True if one more step in <paramref name="dir"/> would put the body past the home room's edge.</summary>
        protected bool LeavesHome(int dir)
        {
            if (!hasHome) return false;
            float front = rb.position.x + dir * (HalfWidth + HomeEdgeMargin);
            return front < homeRect.xMin || front > homeRect.xMax;
        }

        /// <summary>Clamps a point so a body of <paramref name="radius"/> stays inside the home room.</summary>
        protected Vector2 ClampToHome(Vector2 point, float radius)
        {
            if (!hasHome) return point;
            float r = radius + HomeEdgeMargin;
            point.x = Mathf.Clamp(point.x, homeRect.xMin + r, homeRect.xMax - r);
            point.y = Mathf.Clamp(point.y, homeRect.yMin + r, homeRect.yMax - r);
            return point;
        }

        // ------------------------------------------------------------------
        // Room / spawner integration
        // ------------------------------------------------------------------

        public void OnSpawnedBy(EnemySpawner source)
        {
            spawner = source;
            roomId = source != null ? source.RoomId : "";
        }

        public void SetRoomActive(bool active)
        {
            roomActive = active;
            if (rb != null) rb.simulated = active;
            if (animator != null) animator.Speed = active ? 1f : 0f;
        }

        // ------------------------------------------------------------------
        // Loop
        // ------------------------------------------------------------------

        private void FixedUpdate()
        {
            if (!roomActive) return;
            float dt = Time.fixedDeltaTime;
            if (dead)
            {
                if (!IsFlyer) SetVelocityX(0f);
                return;
            }

            TickAI(dt);

            if (knockbackTimer > 0f)
            {
                knockbackTimer -= dt;
                ApplyKnockback(knockbackVelocity, dt);
            }
        }

        protected abstract void TickAI(float dt);

        /// <summary>Ground enemies take the horizontal push as velocity (never across the home edge); flyers override.</summary>
        protected virtual void ApplyKnockback(Vector2 velocity, float dt)
        {
            int dir = velocity.x > 0f ? 1 : -1;
            SetVelocityX(LeavesHome(dir) ? 0f : velocity.x);
        }

        protected virtual void Update()
        {
            // Flashes use unscaled time so they read during hit-stop.
            if (flashTimer > 0f)
            {
                flashTimer -= Time.unscaledDeltaTime;
                if (flashTimer <= 0f && flashRenderer != null) flashRenderer.enabled = false;
            }

            if (hurtTimer > 0f)
            {
                hurtTimer -= Time.deltaTime;
                if (hurtTimer <= 0f && !dead && animator != null && desiredClip != null) animator.Play(desiredClip, true);
            }

            if (dying)
            {
                deathTimer -= Time.deltaTime;
                if (deathTimer <= 0f) FinishDeath();
            }
        }

        private void LateUpdate()
        {
            if (flashRenderer != null && flashRenderer.enabled && body != null) flashRenderer.sprite = body.sprite;
        }

        // ------------------------------------------------------------------
        // Visual helpers
        // ------------------------------------------------------------------

        protected void SetFacing(int direction)
        {
            if (direction == 0) return;
            facing = direction > 0 ? 1 : -1;
            if (visualRoot == null) return;
            Vector3 s = visualRoot.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            visualRoot.localScale = s;
        }

        /// <summary>Requests the looping/base clip; deferred while a Hurt override plays.</summary>
        protected void SetAnim(string clip, bool restart = false)
        {
            if (clip == desiredClip && !restart) return;
            desiredClip = clip;
            if (hurtTimer > 0f || dead || animator == null) return;
            animator.Play(clip, restart);
        }

        protected void Flash(Color color, float duration)
        {
            if (flashRenderer == null || duration <= 0f) return;
            flashRenderer.color = color;
            if (body != null) flashRenderer.sprite = body.sprite;
            flashRenderer.enabled = true;
            flashTimer = Mathf.Max(flashTimer, duration);
        }

        /// <summary>Docs/04: telegraph = pose (caller's clip) + 0.1 s flash + '!' for enemies that use it.</summary>
        protected void BeginTelegraph()
        {
            Flash(config.telegraphFlashColor, config.telegraphFlashTime);
            if (exclamation != null && config.showExclamation) exclamation.SetActive(true);
        }

        protected void EndTelegraph()
        {
            if (exclamation != null) exclamation.SetActive(false);
        }

        // ------------------------------------------------------------------
        // Movement / sensing helpers
        // ------------------------------------------------------------------

        protected Vector2 Feet => rb.position;

        protected Vector2 BodyCenter => IsFlyer ? rb.position : rb.position + new Vector2(0f, config.colliderSize.y * 0.5f * FlameScale);

        protected float HalfWidth => (config.colliderShape == EnemyColliderShape.Circle ? config.colliderRadius : config.colliderSize.x * 0.5f) * FlameScale;

        protected void SetVelocityX(float vx)
        {
            Vector2 v = rb.linearVelocity;
            v.x = vx;
            rb.linearVelocity = v;
        }

        protected bool IsGrounded => EnemySensors.Grounded(Feet, HalfWidth);

        protected bool WallAhead(int dir) => EnemySensors.WallAhead(BodyCenter, dir, HalfWidth, config.colliderSize.y * FlameScale);

        protected bool LedgeAhead(int dir) => !EnemySensors.GroundAhead(Feet, dir, HalfWidth, config.ledgeProbeDistance);

        /// <summary>Wall in front, the home room's edge, or (for ledge-aware enemies on the ground) no floor in front.</summary>
        protected bool Blocked(int dir) => WallAhead(dir) || LeavesHome(dir) || (config.turnsAtLedges && IsGrounded && LedgeAhead(dir));

        private RoomBounds cachedRoom;
        private float nextRoomCheck;
        private const float RoomCheckInterval = 0.2f;

        /// <summary>
        /// The live player, but only when the player is in the same room as this enemy (D-136): enemies in neighbouring
        /// rooms stay active (so they never freeze in view) yet can't see or chase the player through a doorway.
        /// Without a RoomManager (Sandbox) the room rule is skipped.
        /// </summary>
        protected bool TryGetPlayer(out PlayerController player)
        {
            player = PlayerController.Active;
            if (player == null || player.State == PlayerState.Dead) return false;

            RoomManager rooms = RoomManager.Instance;
            if (rooms == null || rooms.Current == null) return true;
            if (Time.time >= nextRoomCheck)
            {
                nextRoomCheck = Time.time + RoomCheckInterval;
                RoomBounds here = rooms.RoomAt(BodyCenter);
                if (here != null) cachedRoom = here;   // keep the last room while standing exactly in a doorway seam
            }
            return cachedRoom == null || cachedRoom == rooms.Current;
        }

        private ContactFilter2D playerHurtFilter;
        private bool playerHurtFilterReady;
        private readonly Collider2D[] hitBuffer = new Collider2D[4];

        /// <summary>Melee hit check against the player's hurtbox (Phase 2 sword / bite windows). Returns true on a hit.</summary>
        protected bool TryHitPlayer(Vector2 center, Vector2 size, int damage)
        {
            if (!playerHurtFilterReady)
            {
                playerHurtFilter = new ContactFilter2D();
                playerHurtFilter.SetLayerMask(GameLayers.PlayerHurtboxMask);
                playerHurtFilter.useTriggers = true;
                playerHurtFilterReady = true;
            }
            int n = Physics2D.OverlapBox(center, size, 0f, playerHurtFilter, hitBuffer);
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

        /// <summary>Box in front of a ground enemy from <paramref name="near"/> to <paramref name="reach"/> (both × flame scale).</summary>
        protected void FrontBox(float near, float reach, float height, out Vector2 center, out Vector2 size)
        {
            float s = FlameScale;
            float width = Mathf.Max(0.1f, (reach - near) * s);
            center = rb.position + new Vector2(facing * (near * s + width * 0.5f), height * s * 0.5f + 0.1f);
            size = new Vector2(width, height * s);
        }

        /// <summary>Swept move of a flyer's anchor point, confined to the home room. Returns the distance moved.</summary>
        protected float SweepMove(ref Vector2 anchor, Vector2 delta, float radius, float margin, bool slide = true)
        {
            float dist = delta.magnitude;
            if (dist < 1e-5f) return 0f;
            Vector2 next;
            if (slide) next = EnemySensors.SweepSlide(anchor, radius, delta);
            else
            {
                Vector2 dir = delta / dist;
                next = anchor + dir * EnemySensors.SweepDistance(anchor, radius, dir, dist);
            }
            Vector2 target = ClampToHome(next, radius + margin);
            float moved = (target - anchor).magnitude;
            anchor = target;
            return moved;
        }

        protected static Vector2 PlayerChest(PlayerController p)
        {
            Vector2 offset = p.Config != null ? p.Config.colliderOffset : new Vector2(0f, 0.75f);
            return p.Position + offset;
        }

        // ------------------------------------------------------------------
        // Flame scaling hook (Docs/07 §4)
        // ------------------------------------------------------------------

        /// <summary>
        /// Scales sprite + colliders around the root (feet for ground enemies, centre for flyers).
        /// Ignored unless the config scales with the flame (Phase 2 only); Prompt 12 adds smoothing and wall-nudge.
        /// </summary>
        public virtual void SetFlameScale(float scale)
        {
            if (config == null || !config.scalesWithFlame) return;
            FlameScale = Mathf.Max(0.1f, scale);
            transform.localScale = new Vector3(FlameScale, FlameScale, 1f);
        }

        // ------------------------------------------------------------------
        // Damage / death
        // ------------------------------------------------------------------

        private void HandleHitFlash(float duration) => Flash(Color.white, duration);

        private void HandleDamaged(DamageInfo info)
        {
            AudioManager.Play("enemy_hit");
            if (!health.IsAlive) return;

            bool immune = config.knockbackImmuneDuringAttack && IsAttacking;
            if (!immune && config.knockbackSpeed > 0f && config.knockbackDuration > 0f)
            {
                float dirX;
                if (Mathf.Abs(info.knockbackDir.x) > 0.01f) dirX = Mathf.Sign(info.knockbackDir.x);
                else if (info.source != null) dirX = transform.position.x >= info.source.transform.position.x ? 1f : -1f;
                else dirX = -facing;
                knockbackVelocity = new Vector2(dirX * config.knockbackSpeed, 0f);
                knockbackTimer = config.knockbackDuration;
            }

            if (!IsAttacking && animator != null && animator.HasClip("Hurt"))
            {
                animator.Play("Hurt", true);
                hurtTimer = config.hurtAnimTime;
            }
            OnHurt(info);
        }

        protected virtual void OnHurt(DamageInfo info) { }

        private void HandleDied(DamageInfo info)
        {
            if (dead) return;
            dead = true;
            knockbackTimer = 0f;
            hurtTimer = 0f;

            if (hurtbox != null) hurtbox.enabled = false;
            if (contact != null)
            {
                contact.Active = false;
                Collider2D c = contact.GetComponent<Collider2D>();
                if (c != null) c.enabled = false;
            }
            if (exclamation != null) exclamation.SetActive(false);
            if (IsFlyer) rb.linearVelocity = Vector2.zero;

            if (animator != null && animator.HasClip("Death")) animator.Play("Death", true);
            AudioManager.Play("enemy_death");

            if (spawner != null) spawner.MarkKilled();
            else GameEvents.RaiseEnemyKilled(name);

            OnDeath();
            dying = true;
            deathTimer = config.deathAnimTime;
            // D-124 insurance: the corpse is removed even if the death timer were ever interrupted.
            Destroy(gameObject, config.deathAnimTime + 1f);
            // A dead body must keep animating and counting down wherever it is.
            SetRoomActive(true);
        }

        protected virtual void OnDeath() { }

        private void FinishDeath()
        {
            dying = false;
            FxPool.Play("Death_Smoke", BodyCenter, 1, FlameScale);
            Destroy(gameObject);
        }

        protected virtual void OnDrawGizmos()
        {
            if (!DebugCommands.ShowHitboxGizmos || config == null) return;
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, config.detectRange);
            if (hurtbox != null && hurtbox.enabled)
            {
                Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.8f);
                Gizmos.DrawWireCube(hurtbox.bounds.center, hurtbox.bounds.size);
            }
        }
    }
}
