using Shatranj.Core;
using Shatranj.Player;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Docs/04 §A3 Shadow Kite (flyer): hovers with a sine bob (0.4 u, 1.5 Hz) near its spawn. Player within 9 u →
    /// drifts toward the player at 2.5 u/s keeping ~3 u above. Every 3 s: Telegraph 0.5 s (flash, tail-flare clip)
    /// → Dive at 9 u/s along the line to where the player was at telegraph start, until it hits ground or has
    /// travelled 6 u → returns to hover height at 3 u/s. Kinematic; all moves are swept against solid tiles.
    /// </summary>
    public class ShadowKite : EnemyBase
    {
        private enum State { Hover, Telegraph, Dive, Return }

        private const float ArriveDistance = 0.25f;
        private const float BobBlendSpeed = 5f;

        private State state = State.Hover;
        private float timer;
        private float attackTimer;
        private Vector2 home;
        private Vector2 basePos;
        private Vector2 diveTarget;
        private Vector2 diveDir;
        private float travelled;
        private float bobTime;
        private float bobWeight = 1f;

        protected override bool IsAttacking => state == State.Telegraph || state == State.Dive;

        private float Radius => Mathf.Max(0.05f, config.colliderRadius * FlameScale * 0.9f);

        protected override void Start()
        {
            base.Start();
            home = ClampToHome(rb.position, Radius + config.hoverAmplitude);
            basePos = home;
            attackTimer = config.attackInterval;
            SetAnim("Fly");
        }

        protected override void TickAI(float dt)
        {
            bool aware = TryGetPlayer(out PlayerController p) && (PlayerChest(p) - basePos).sqrMagnitude <= config.detectRange * config.detectRange;
            Vector2 anchor = aware ? PlayerChest(p) + new Vector2(0f, config.hoverHeightAbovePlayer) : home;

            switch (state)
            {
                case State.Hover:
                {
                    float speed = aware ? config.chaseSpeed : config.returnSpeed;
                    Vector2 step = Vector2.MoveTowards(basePos, anchor, speed * dt) - basePos;
                    step += EnemySensors.Separation(basePos, config.separationRadius, transform) * speed * dt;
                    Move(step);
                    if (aware)
                    {
                        SetFacing(p.Position.x >= basePos.x ? 1 : -1);
                        attackTimer -= dt;
                        if (attackTimer <= 0f) EnterTelegraph(p);
                    }
                    break;
                }

                case State.Telegraph:
                    timer -= dt;
                    if (timer <= 0f) EnterDive();
                    break;

                case State.Dive:
                {
                    Vector2 step = diveDir * (config.attackSpeed * dt);
                    float moved = Move(step, slide: false);   // a dive ends on impact
                    travelled += moved;
                    if (moved < step.magnitude - 1e-4f || travelled >= config.attackMaxDistance) EnterReturn();
                    break;
                }

                case State.Return:
                {
                    Vector2 want = Vector2.MoveTowards(basePos, anchor, config.returnSpeed * dt) - basePos;
                    float progress = Move(want);
                    // The hover point can sit inside a low ceiling: stop returning once it can't get any closer.
                    bool stalled = want.sqrMagnitude > 1e-8f && progress < want.magnitude * 0.1f;
                    if (stalled || (basePos - anchor).sqrMagnitude <= ArriveDistance * ArriveDistance)
                    {
                        state = State.Hover;
                        attackTimer = config.attackInterval;
                        SetAnim("Fly");
                    }
                    break;
                }
            }

            // Sine bob only while hovering / telegraphing; blended so dives don't snap.
            bool bobbing = state == State.Hover || state == State.Telegraph;
            bobWeight = Mathf.MoveTowards(bobWeight, bobbing ? 1f : 0f, BobBlendSpeed * dt);
            bobTime += dt;
            float bob = Mathf.Sin(bobTime * config.hoverFrequency * Mathf.PI * 2f) * config.hoverAmplitude * bobWeight;
            rb.MovePosition(basePos + new Vector2(0f, bob));
        }

        /// <summary>Swept move of the hover anchor point. Returns the distance actually moved.</summary>
        private float Move(Vector2 delta, bool slide = true)
        {
            float dist = delta.magnitude;
            if (dist < 1e-5f) return 0f;
            Vector2 dir = delta / dist;
            Vector2 next = slide ? EnemySensors.SweepSlide(basePos, Radius, delta)
                                 : basePos + dir * EnemySensors.SweepDistance(basePos, Radius, dir, dist);
            // D-137: the home room's edges are walls too (bob amplitude included so the visible body stays inside).
            Vector2 target = ClampToHome(next, Radius + config.hoverAmplitude);
            float moved = (target - basePos).magnitude;
            basePos = target;
            return moved;
        }

        protected override void ApplyKnockback(Vector2 velocity, float dt) => Move(velocity * dt);

        private void EnterTelegraph(PlayerController p)
        {
            state = State.Telegraph;
            timer = config.telegraphTime;
            diveTarget = PlayerChest(p);
            SetAnim("Telegraph", true);
            BeginTelegraph();
            AudioManager.Play("kite_telegraph");
        }

        private void EnterDive()
        {
            EndTelegraph();
            state = State.Dive;
            travelled = 0f;
            Vector2 d = diveTarget - basePos;
            diveDir = d.sqrMagnitude > 1e-4f ? d.normalized : Vector2.down;
            SetFacing(diveDir.x >= 0f ? 1 : -1);
            SetAnim("Dive", true);
            AudioManager.Play("kite_dive");
        }

        private void EnterReturn()
        {
            state = State.Return;
            SetAnim("Fly");
        }

        protected override void OnDeath() => EndTelegraph();

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (!DebugCommands.ShowHitboxGizmos || !Application.isPlaying) return;
            Gizmos.color = new Color(0.6f, 0.9f, 1f, 0.6f);
            Gizmos.DrawWireSphere(home, 0.2f);
            if (state == State.Telegraph || state == State.Dive) Gizmos.DrawLine(basePos, diveTarget);
        }
    }
}
