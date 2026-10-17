using Shatranj.Core;
using Shatranj.Player;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Docs/04 §C4 Thrower (4 HP). Keeps 6–9 u from the player: retreats when closer than 5 u, advances when farther
    /// than 10 u, stops at ledges and walls; cornered with the player close, it hop-dodges over them. Every 2.2 s:
    /// Telegraph 0.4 s → a knife (0.9 × 0.2 u) in a slight arc at 10 u/s aimed with first-order lead
    /// (player position + velocity × 0.3 s), 1 damage, breaks on walls. Knife size × (1 + 0.5·(scale − 1)).
    /// </summary>
    public class Thrower : EnemyBase
    {
        private enum State { Space, Telegraph, Recover }

        private const float DefaultArcGravity = 6f;
        private const float HopCooldown = 1.5f;
        private const float HopUpSpeed = 7.5f;
        private const float HopTime = 0.55f;
        private const float HandHeight = 1.3f;

        private State state = State.Space;
        private float timer;
        private float throwTimer;
        private float hopCooldown;
        private float hopTimer;
        private readonly Awareness awareness = new Awareness();

        protected override bool IsAttacking => state == State.Telegraph;

        protected override void Start()
        {
            base.Start();
            throwTimer = config.attackInterval;
            SetAnim("Walk");
        }

        protected override void TickAI(float dt)
        {
            if (hopCooldown > 0f) hopCooldown -= dt;
            if (hopTimer > 0f) hopTimer -= dt;
            bool grounded = IsGrounded && hopTimer <= 0f;

            bool sees = Sees(out PlayerController p);
            bool aware = awareness.Tick(sees, p != null ? p.Position : Vector2.zero, dt, config.loseTrackTime) && p != null;

            switch (state)
            {
                case State.Space:
                    if (!grounded) return;   // mid-hop: keep momentum
                    if (!aware) { Patrol(); return; }
                    KeepDistance(p);
                    throwTimer -= dt;
                    if (throwTimer <= 0f && sees) EnterTelegraph();
                    break;

                case State.Telegraph:
                    SetVelocityX(0f);
                    if (p != null) SetFacing(p.Position.x >= rb.position.x ? 1 : -1);
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        EndTelegraph();
                        if (p != null) Throw(p);
                        state = State.Recover;
                        timer = config.attackRecoveryTime;
                        SetAnim("Throw", true);
                    }
                    break;

                case State.Recover:
                    SetVelocityX(0f);
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        state = State.Space;
                        throwTimer = config.attackInterval;
                        SetAnim("Walk");
                    }
                    break;
            }
        }

        private bool Sees(out PlayerController p)
        {
            if (!TryGetPlayer(out p)) return false;
            Vector2 d = p.Position - Feet;
            if (d.sqrMagnitude > config.detectRange * config.detectRange) return false;
            return !config.requiresLineOfSight || EnemySensors.LineOfSight(BodyCenter + new Vector2(0f, 0.4f * FlameScale), PlayerChest(p));
        }

        private void KeepDistance(PlayerController p)
        {
            float dx = p.Position.x - rb.position.x;
            SetFacing(dx >= 0f ? 1 : -1);
            int move = SpacingLogic.BandMove(Mathf.Abs(dx), config.retreatIfCloserThan, config.advanceIfFartherThan);
            if (move == 0) { SetVelocityX(0f); SetAnim("Idle"); return; }

            int dir = move > 0 ? facing : -facing;
            if (Blocked(dir))
            {
                // Cornered while the player closes in → short hop-dodge over them.
                if (move < 0 && hopCooldown <= 0f && !Blocked(facing)) { HopDodge(); return; }
                SetVelocityX(0f);
                SetAnim("Idle");
                return;
            }
            SetVelocityX(dir * config.patrolSpeed);
            SetAnim("Walk");
        }

        private void HopDodge()
        {
            hopCooldown = HopCooldown;
            hopTimer = 0.15f;   // ignore the floor for a moment so the launch is not cancelled
            float vx = facing * (config.hopDodgeDistance + 2f) / HopTime;   // clears the player in front
            rb.linearVelocity = new Vector2(vx, HopUpSpeed);
            SetAnim("Walk");
            AudioManager.Play("thrower_hop");
        }

        private void Patrol()
        {
            if (Blocked(facing)) SetFacing(-facing);
            if (Blocked(facing)) { SetVelocityX(0f); SetAnim("Idle"); return; }
            SetVelocityX(facing * config.patrolSpeed * 0.6f);
            SetAnim("Walk");
        }

        private void EnterTelegraph()
        {
            state = State.Telegraph;
            timer = config.telegraphTime;
            SetVelocityX(0f);
            SetAnim("Throw_Telegraph", true);
            BeginTelegraph();
            AudioManager.Play("thrower_telegraph");
        }

        private void Throw(PlayerController p)
        {
            float s = SpacingLogic.ProjectileScale(FlameScale, config.projectileScaleFactor);
            Vector2 hand = rb.position + new Vector2(facing * 0.4f * FlameScale, HandHeight * FlameScale);
            Vector2 target = SpacingLogic.LeadTarget(PlayerChest(p), p.Velocity, config.projectileLeadTime);
            float gravity = config.projectileArcs ? (config.projectileGravity > 0f ? config.projectileGravity : DefaultArcGravity) : 0f;
            Vector2 velocity = SpacingLogic.ArcVelocity(hand, target, config.projectileSpeed, gravity);

            ProjectilePool.Fire("Knife_Fly", hand, new EnemyProjectile.Spec
            {
                velocity = velocity,
                size = config.projectileSize * s,
                lifetime = config.projectileLifetime > 0f ? config.projectileLifetime : 3f,
                damage = config.projectileDamage,
                gravity = gravity,
                breakFx = null,
                owner = gameObject,
                visualScale = s,
            });
            AudioManager.Play("thrower_throw");
        }

        protected override void OnDeath() => EndTelegraph();
    }
}
