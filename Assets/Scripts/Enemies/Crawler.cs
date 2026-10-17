using Shatranj.Player;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Docs/04 §A1 Shade Crawler: patrols its own route, turning at walls and ledges. It only tracks the player when
    /// the player is on its level (feet within <c>verticalTolerance</c>, clear line of sight, within detect range);
    /// it keeps the chase for <c>trackMemoryTime</c> after that so a jump doesn't break it. Never walks off ledges.
    /// Contact damage only.
    /// </summary>
    public class Crawler : EnemyBase
    {
        private float lastSameLevelTime = float.NegativeInfinity;

        protected override void Start()
        {
            base.Start();
            SetAnim("Walk");
        }

        protected override void TickAI(float dt)
        {
            if (!IsGrounded)
            {
                SetAnim("Idle");
                return;
            }

            if (TryGetPlayer(out PlayerController p))
            {
                float dx = p.Position.x - Feet.x;
                bool inRange = Mathf.Abs(dx) <= config.detectRange;
                bool sameLevel = inRange
                                 && Mathf.Abs(p.Position.y - Feet.y) <= config.verticalTolerance
                                 && EnemySensors.LineOfSight(BodyCenter, PlayerChest(p));
                if (sameLevel) lastSameLevelTime = Time.time;

                if (inRange && Time.time - lastSameLevelTime <= config.trackMemoryTime)
                {
                    Chase(dx);
                    return;
                }
            }

            Patrol(config.patrolSpeed);
        }

        private void Chase(float dx)
        {
            int dir = dx >= 0f ? 1 : -1;
            SetFacing(dir);
            if (Mathf.Abs(dx) < 0.1f || Blocked(dir))
            {
                SetVelocityX(0f);
                SetAnim("Idle");
                return;
            }
            SetVelocityX(dir * config.chaseSpeed);
            SetAnim("Walk");
        }

        private void Patrol(float speed)
        {
            if (Blocked(facing)) SetFacing(-facing);
            if (Blocked(facing))
            {
                SetVelocityX(0f);
                SetAnim("Idle");
                return;
            }
            SetVelocityX(facing * speed);
            SetAnim("Walk");
        }
    }
}
