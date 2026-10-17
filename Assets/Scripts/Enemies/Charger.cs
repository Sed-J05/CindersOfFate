using Shatranj.Core;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Docs/04 §C3 Siege Ram Knight (7 HP). Patrols at 1.5 u/s. Player within 10 u, in line on the same level, with line
    /// of sight → Telegraph 0.6 s (paws the ground, dust, flash, '!') → Charge 13 u/s for up to 10 u. Hitting a wall
    /// stuns it for 1.2 s (big opening, normal damage); a ledge, the room edge or the 10 u limit just ends the charge.
    /// Jumping over it makes it overshoot into the wall. No knockback while telegraphing or charging.
    /// </summary>
    public class Charger : EnemyBase
    {
        private enum State { Patrol, Telegraph, Charge, Stunned, Recover }

        private State state = State.Patrol;
        private float timer;
        private float cooldown;
        private float chargeStartX;

        protected override bool IsAttacking => state == State.Telegraph || state == State.Charge;

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
                    if (cooldown > 0f) cooldown -= dt;
                    if (!IsGrounded) { SetAnim("Idle"); return; }
                    if (cooldown <= 0f && InLine(out PlayerController p)) { EnterTelegraph(p); return; }
                    Patrol();
                    break;

                case State.Telegraph:
                    SetVelocityX(0f);
                    timer -= dt;
                    if (timer <= 0f) EnterCharge();
                    break;

                case State.Charge:
                    if (WallAhead(facing)) { EnterStun(); break; }
                    float travelled = Mathf.Abs(rb.position.x - chargeStartX);
                    if (travelled >= config.attackMaxDistance * FlameScale || LeavesHome(facing) || (IsGrounded && LedgeAhead(facing)))
                    {
                        EnterRecover();
                        break;
                    }
                    SetVelocityX(facing * config.attackSpeed);
                    break;

                case State.Stunned:
                case State.Recover:
                    SetVelocityX(0f);
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        if (TryGetPlayer(out PlayerController target)) SetFacing(target.Position.x >= rb.position.x ? 1 : -1);
                        cooldown = config.attackCooldown;
                        state = State.Patrol;
                        SetAnim("Walk");
                    }
                    break;
            }
        }

        private bool InLine(out PlayerController p)
        {
            if (!TryGetPlayer(out p)) return false;
            Vector2 d = p.Position - Feet;
            if (Mathf.Abs(d.x) > config.detectRange || Mathf.Abs(d.y) > config.verticalTolerance) return false;
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
            SetFacing(p.Position.x >= rb.position.x ? 1 : -1);
            SetVelocityX(0f);
            SetAnim("Telegraph", true);
            BeginTelegraph();
            FxPool.Play("Dust_Puff", Feet + new Vector2(-facing * 0.6f * FlameScale, 0.3f), -facing, FlameScale);
            AudioManager.Play("charger_telegraph");
        }

        private void EnterCharge()
        {
            EndTelegraph();
            state = State.Charge;
            chargeStartX = rb.position.x;
            SetAnim("Charge", true);
            AudioManager.Play("charger_charge");
        }

        private void EnterStun()
        {
            state = State.Stunned;
            timer = config.wallStunTime;
            SetVelocityX(0f);
            SetAnim("Stunned", true);
            FxPool.Play("Dust_Puff", BodyCenter + new Vector2(facing * config.colliderSize.x * 0.5f * FlameScale, 0f), facing, FlameScale);
            ScreenShake.Shake(0.15f, 0.2f);
            AudioManager.Play("charger_stun");
        }

        private void EnterRecover()
        {
            state = State.Recover;
            timer = config.attackRecoveryTime;
            SetVelocityX(0f);
            SetAnim("Idle");
        }

        protected override void OnDeath() => EndTelegraph();
    }
}
