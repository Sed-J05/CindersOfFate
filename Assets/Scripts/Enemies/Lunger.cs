using Shatranj.Core;
using Shatranj.Player;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Docs/04 §A2 Ninja Lunger: patrols at 2 u/s. Player within 7 u with line of sight → Telegraph 0.5 s
    /// (crouch clip, red flash, '!') → Lunge 14 u/s for 0.35 s (≈ 5 u) toward where the player was when the
    /// telegraph began → Recovery 0.8 s (vulnerable, no turning) → 1 s cooldown. Contact damage throughout.
    /// </summary>
    public class Lunger : EnemyBase
    {
        private enum State { Patrol, Telegraph, Lunge, Recover }

        private State state = State.Patrol;
        private float timer;
        private float cooldown;
        private float lungeStartX;

        protected override bool IsAttacking => state == State.Telegraph || state == State.Lunge;

        protected override void Start()
        {
            base.Start();
            SetAnim("Walk");
        }

        protected override void TickAI(float dt)
        {
            switch (state)
            {
                case State.Patrol:
                    cooldown -= dt;
                    if (!IsGrounded) { SetAnim("Idle"); return; }
                    if (cooldown <= 0f && CanSeePlayer(out PlayerController p)) { EnterTelegraph(p); return; }
                    Patrol();
                    break;

                case State.Telegraph:
                    SetVelocityX(0f);
                    timer -= dt;
                    if (timer <= 0f) EnterLunge();
                    break;

                case State.Lunge:
                    timer -= dt;
                    float travelled = Mathf.Abs(rb.position.x - lungeStartX);
                    if (timer <= 0f || travelled >= config.attackMaxDistance || Blocked(facing)) EnterRecover();
                    else SetVelocityX(facing * config.attackSpeed);
                    break;

                case State.Recover:
                    SetVelocityX(0f);
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        state = State.Patrol;
                        cooldown = config.attackCooldown;
                        SetAnim("Walk");
                    }
                    break;
            }
        }

        private bool CanSeePlayer(out PlayerController p)
        {
            if (!TryGetPlayer(out p)) return false;
            if (!EnemySensors.WithinBox(Feet, p.Position, config.detectRange, config.verticalTolerance)) return false;
            return !config.requiresLineOfSight || EnemySensors.LineOfSight(BodyCenter, PlayerChest(p));
        }

        private void Patrol()
        {
            if (Blocked(facing)) SetFacing(-facing);
            if (Blocked(facing)) { SetVelocityX(0f); SetAnim("Idle"); return; }
            SetVelocityX(facing * config.patrolSpeed);
            SetAnim("Walk");
        }

        private void EnterTelegraph(PlayerController p)
        {
            state = State.Telegraph;
            timer = config.telegraphTime;
            SetFacing(p.Position.x >= rb.position.x ? 1 : -1); // committed: lunges toward where the player WAS
            SetVelocityX(0f);
            SetAnim("Telegraph", true);
            BeginTelegraph();
            AudioManager.Play("lunger_telegraph");
        }

        private void EnterLunge()
        {
            EndTelegraph();
            state = State.Lunge;
            timer = config.attackActiveTime;
            lungeStartX = rb.position.x;
            SetAnim("Lunge", true);
            AudioManager.Play("lunger_lunge");
        }

        private void EnterRecover()
        {
            state = State.Recover;
            timer = config.attackRecoveryTime;
            SetVelocityX(0f);
            SetAnim("Recover", true);
        }

        protected override void OnDeath() => EndTelegraph();
    }
}
