using Shatranj.Core;
using Shatranj.Player;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Docs/04 §C1 Castle Knight (5 HP). Patrols at 2 u/s. Sees the player within 9 u (line of sight, loses track after
    /// 3 s) → approaches to ~2.0 u → Telegraph 0.4 s → 2-hit combo (reach 1.6 u; hit 1 at 0.1 s, hit 2 after a 0.3 s gap)
    /// → retreats 2.5 u in 0.5 s → short cooldown → repeats. 20 % of telegraphs are feints (cancel, back off).
    /// Waits instead of swinging while the player is airborne more than 1.5 u above it. Avoids ledges, keeps apart from
    /// other enemies. Reach scales with the flame.
    /// </summary>
    public class Knight : EnemyBase
    {
        private enum State { Patrol, Approach, Telegraph, Attack, Retreat }

        private const float RangeTolerance = 0.3f;
        private const float HitNear = 0.2f;
        private const float HitHeight = 1.6f;
        private const float SeparationGain = 1.5f;

        private State state = State.Patrol;
        private float timer;
        private float cooldown;
        private bool feint;
        private bool hit1Done, hit2Done;
        private readonly Awareness awareness = new Awareness();

        protected override bool IsAttacking => state == State.Telegraph || state == State.Attack;

        protected override void Start()
        {
            base.Start();
            SetAnim("Walk");
        }

        protected override void TickAI(float dt)
        {
            if (cooldown > 0f) cooldown -= dt;
            if (!IsGrounded) return;

            bool sees = Sees(out PlayerController p);
            bool aware = awareness.Tick(sees, p != null ? p.Position : Vector2.zero, dt, config.loseTrackTime);
            if (p == null) aware = false;

            switch (state)
            {
                case State.Patrol:
                    if (aware) { state = State.Approach; break; }
                    Patrol();
                    break;

                case State.Approach:
                    if (!aware) { state = State.Patrol; break; }
                    Approach(p, dt);
                    break;

                case State.Telegraph:
                    SetVelocityX(0f);
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        EndTelegraph();
                        if (feint) EnterRetreat();
                        else EnterAttack();
                    }
                    break;

                case State.Attack:
                    SetVelocityX(0f);
                    timer += dt;
                    float hit1Start = 0.1f, hit1End = hit1Start + config.attackActiveTime;
                    float hit2Start = hit1End + config.comboGap, hit2End = hit2Start + config.attackActiveTime;
                    if (!hit1Done && timer >= hit1Start) { if (timer <= hit1End) hit1Done = Strike(); else hit1Done = true; }
                    if (timer >= hit2Start && animator != null && animator.CurrentClip == "Attack_A") SetAnim("Attack_B", true);
                    if (config.comboHits > 1 && !hit2Done && timer >= hit2Start) { if (timer <= hit2End) hit2Done = Strike(); else hit2Done = true; }
                    if (timer >= (config.comboHits > 1 ? hit2End : hit1End) + 0.05f) EnterRetreat();
                    break;

                case State.Retreat:
                    timer -= dt;
                    float speed = config.retreatDistance / Mathf.Max(0.05f, config.retreatTime);
                    SetVelocityX(Blocked(-facing) ? 0f : -facing * speed);
                    if (timer <= 0f)
                    {
                        cooldown = config.attackCooldown;
                        state = aware ? State.Approach : State.Patrol;
                        SetAnim("Walk");
                    }
                    break;
            }
        }

        private bool Sees(out PlayerController p)
        {
            if (!TryGetPlayer(out p)) return false;
            Vector2 d = p.Position - Feet;
            if (Mathf.Abs(d.x) > config.detectRange || Mathf.Abs(d.y) > 3f) return false;
            return !config.requiresLineOfSight || EnemySensors.LineOfSight(BodyCenter, PlayerChest(p));
        }

        private void Approach(PlayerController p, float dt)
        {
            float dx = p.Position.x - Feet.x;
            SetFacing(dx >= 0f ? 1 : -1);
            int move = SpacingLogic.ApproachToRange(Mathf.Abs(dx), config.preferredDistance * FlameScale, RangeTolerance);
            bool wait = SpacingLogic.ShouldWaitForAirbornePlayer(p.IsGrounded, p.Position.y - Feet.y, config.waitIfPlayerAboveHeight);

            if (move == 0 && !wait && cooldown <= 0f) { EnterTelegraph(); return; }

            float vx = 0f;
            if (move != 0)
            {
                int dir = move > 0 ? facing : -facing;
                vx = Blocked(dir) ? 0f : dir * (move > 0 ? config.chaseSpeed : config.patrolSpeed);
            }
            // Don't stack on other enemies (separation steering, horizontal only for ground units).
            Vector2 sep = EnemySensors.Separation(BodyCenter, config.separationRadius * FlameScale, transform);
            if (Mathf.Abs(sep.x) > 0.01f && !Blocked(sep.x > 0f ? 1 : -1)) vx += sep.x * SeparationGain;
            SetVelocityX(vx);
            SetAnim(Mathf.Abs(vx) > 0.05f ? "Walk" : "Idle");
        }

        private void Patrol()
        {
            if (Blocked(facing)) SetFacing(-facing);
            if (Blocked(facing)) { SetVelocityX(0f); SetAnim("Idle"); return; }
            SetVelocityX(facing * config.patrolSpeed);
            SetAnim("Walk");
        }

        private void EnterTelegraph()
        {
            state = State.Telegraph;
            timer = config.telegraphTime;
            feint = Random.value < config.feintChance;
            SetVelocityX(0f);
            SetAnim("Telegraph", true);
            BeginTelegraph();
            AudioManager.Play("knight_telegraph");
        }

        private void EnterAttack()
        {
            state = State.Attack;
            timer = 0f;
            hit1Done = hit2Done = false;
            SetAnim("Attack_A", true);
            AudioManager.Play("knight_slash");
        }

        private void EnterRetreat()
        {
            state = State.Retreat;
            timer = config.retreatTime;
            SetAnim("Walk");
        }

        private bool Strike()
        {
            FrontBox(HitNear, config.attackReach, HitHeight, out Vector2 center, out Vector2 size);
            return TryHitPlayer(center, size, config.attackDamage);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (!DebugCommands.ShowHitboxGizmos || !Application.isPlaying || config == null) return;
            FrontBox(HitNear, config.attackReach, HitHeight, out Vector2 c, out Vector2 s);
            Gizmos.color = state == State.Attack ? new Color(1f, 0.2f, 0.2f, 0.6f) : new Color(1f, 0.6f, 0.2f, 0.25f);
            Gizmos.DrawWireCube(c, s);
        }
    }
}
