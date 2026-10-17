using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Physics half of the player (Docs/03 §3, Docs/02 §6): velocity, custom gravity, ground / wall / ceiling
    /// checks, run acceleration, jump impulses. Owns no state logic — <see cref="PlayerController"/> drives it.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlayerMotor : MonoBehaviour
    {
        private PlayerConfig config;
        private Rigidbody2D rb;
        private CapsuleCollider2D body;

        private readonly RaycastHit2D[] hits = new RaycastHit2D[4];
        private ContactFilter2D walkableFilter;
        private ContactFilter2D wallFilter;

        public Rigidbody2D Body => rb;
        public CapsuleCollider2D Collider => body;

        public Vector2 Velocity
        {
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
        }

        public bool IsGrounded { get; private set; }
        public bool WasGrounded { get; private set; }
        public bool OnOneWay { get; private set; }
        public bool TouchingWallLeft { get; private set; }
        public bool TouchingWallRight { get; private set; }
        public bool HitCeiling { get; private set; }
        public Vector2 GroundNormal { get; private set; } = Vector2.up;

        /// <summary>1 = normal, 0 = none (dash). Reset by the controller per state.</summary>
        public float GravityMultiplier { get; set; } = 1f;

        public float FeetY => rb.position.y + body.offset.y - body.size.y * 0.5f;

        public void Initialize(PlayerConfig playerConfig)
        {
            config = playerConfig;
            rb = GetComponent<Rigidbody2D>();
            body = GetComponent<CapsuleCollider2D>();

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;              // gravity is applied here, not by Physics2D
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.linearDamping = 0f;

            walkableFilter = new ContactFilter2D();
            walkableFilter.SetLayerMask(GameLayers.WalkableMask);
            walkableFilter.useTriggers = false;
            wallFilter = new ContactFilter2D();
            wallFilter.SetLayerMask(GameLayers.WallMask);
            wallFilter.useTriggers = false;

            if (config != null)
            {
                body.direction = CapsuleDirection2D.Vertical;
                body.size = config.colliderSize;
                body.offset = config.colliderOffset;
            }
        }

        // ------------------------------------------------------------------
        // Checks (call once per FixedUpdate before state logic)
        // ------------------------------------------------------------------

        public void UpdateChecks()
        {
            WasGrounded = IsGrounded;
            Vector2 vel = rb.linearVelocity;
            Vector2 centre = rb.position + body.offset;
            Vector2 size = body.size;

            // Ground: thin box just under the feet, slightly narrower than the capsule.
            float inset = config != null ? config.groundCheckInset : 0.06f;
            float groundDist = config != null ? config.groundCheckDistance : 0.06f;
            Vector2 footBox = new Vector2(Mathf.Max(0.05f, size.x - inset), 0.04f);
            Vector2 footOrigin = new Vector2(centre.x, FeetY + footBox.y * 0.5f + 0.01f);

            IsGrounded = false;
            OnOneWay = false;
            GroundNormal = Vector2.up;

            int n = Physics2D.BoxCast(footOrigin, footBox, 0f, Vector2.down, walkableFilter, hits, groundDist + 0.01f);
            for (int i = 0; i < n; i++)
            {
                RaycastHit2D h = hits[i];
                if (h.collider == null) continue;
                bool oneWay = h.collider.gameObject.layer == GameLayers.OneWay;
                // One-way platforms only count while not moving up through them and when the surface is below the feet.
                if (oneWay && (vel.y > 0.05f || h.point.y > FeetY + 0.08f)) continue;
                if (h.normal.y < 0.5f) continue; // walls are not ground (no slopes in this game)
                IsGrounded = true;
                OnOneWay = oneWay;
                GroundNormal = h.normal;
                break;
            }

            // Walls: thin vertical boxes on each side, shorter than the capsule so steps don't register as walls.
            float wallDist = config != null ? config.wallCheckDistance : 0.05f;
            Vector2 wallBox = new Vector2(0.04f, size.y * 0.7f);
            TouchingWallLeft = CastSolid(centre, wallBox, Vector2.left, size.x * 0.5f + wallDist);
            TouchingWallRight = CastSolid(centre, wallBox, Vector2.right, size.x * 0.5f + wallDist);

            // Ceiling.
            float ceilDist = config != null ? config.ceilingCheckDistance : 0.05f;
            Vector2 headBox = new Vector2(Mathf.Max(0.05f, size.x - inset), 0.04f);
            HitCeiling = CastSolid(centre, headBox, Vector2.up, size.y * 0.5f + ceilDist);
        }

        private bool CastSolid(Vector2 origin, Vector2 boxSize, Vector2 dir, float distance)
        {
            int n = Physics2D.BoxCast(origin, boxSize, 0f, dir, wallFilter, hits, distance);
            for (int i = 0; i < n; i++)
                if (hits[i].collider != null) return true;
            return false;
        }

        // ------------------------------------------------------------------
        // Movement primitives
        // ------------------------------------------------------------------

        /// <summary>Accelerates toward inputX × runSpeed using accel/decel times; air control scales the rates.</summary>
        public void ApplyRun(float inputX, float dt)
        {
            Vector2 v = rb.linearVelocity;
            float target = inputX * config.runSpeed;

            // Wall-stick prevention: never push into a wall you're touching.
            if (inputX > 0f && TouchingWallRight) target = 0f;
            if (inputX < 0f && TouchingWallLeft) target = 0f;

            bool accelerating = Mathf.Abs(target) > 0.01f && (Mathf.Sign(target) == Mathf.Sign(v.x) || Mathf.Abs(v.x) < 0.01f) && Mathf.Abs(target) > Mathf.Abs(v.x);
            float time = accelerating ? config.accelTime : config.decelTime;
            float rate = config.runSpeed / Mathf.Max(0.0001f, time);
            if (!IsGrounded) rate *= config.airControl;

            v.x = Mathf.MoveTowards(v.x, target, rate * dt);
            if (TouchingWallRight && v.x > 0f) v.x = 0f;
            if (TouchingWallLeft && v.x < 0f) v.x = 0f;
            rb.linearVelocity = v;
        }

        /// <summary>Horizontal velocity decays to zero (hurt, dead, rooted states).</summary>
        public void ApplyBrake(float dt, float decelTimeOverride = -1f)
        {
            Vector2 v = rb.linearVelocity;
            float time = decelTimeOverride > 0f ? decelTimeOverride : config.decelTime;
            v.x = Mathf.MoveTowards(v.x, 0f, (config.runSpeed / Mathf.Max(0.0001f, time)) * dt);
            rb.linearVelocity = v;
        }

        /// <summary>Rise gravity while moving up, fall gravity (×1.5) while moving down, clamped to max fall speed.</summary>
        public void ApplyGravity(float dt)
        {
            if (GravityMultiplier <= 0f) return;
            Vector2 v = rb.linearVelocity;

            if (IsGrounded && v.y <= 0f)
            {
                v.y = -config.groundStickSpeed;
            }
            else
            {
                float g = v.y > 0f ? config.RiseGravity : config.FallGravity;
                v.y -= g * GravityMultiplier * dt;
                if (v.y < -config.maxFallSpeed) v.y = -config.maxFallSpeed;
            }

            if (HitCeiling && v.y > 0f) v.y = 0f;
            rb.linearVelocity = v;
        }

        public void Jump()
        {
            Vector2 v = rb.linearVelocity;
            v.y = config.JumpVelocity;
            rb.linearVelocity = v;
            IsGrounded = false;
            OnOneWay = false;
        }

        /// <summary>Variable jump: on release while rising, vy *= releaseMultiplier (Docs/03 §3).</summary>
        public void CutJump()
        {
            Vector2 v = rb.linearVelocity;
            if (v.y > 0f)
            {
                v.y *= config.jumpReleaseMultiplier;
                rb.linearVelocity = v;
            }
        }

        public void SetVelocity(Vector2 v) => rb.linearVelocity = v;

        public void SetVelocityX(float x)
        {
            Vector2 v = rb.linearVelocity;
            v.x = x;
            rb.linearVelocity = v;
        }

        public void SetVelocityY(float y)
        {
            Vector2 v = rb.linearVelocity;
            v.y = y;
            rb.linearVelocity = v;
        }

        public void Teleport(Vector2 position)
        {
            rb.position = position;
            transform.position = position;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
