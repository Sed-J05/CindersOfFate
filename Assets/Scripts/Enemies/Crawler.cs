using Shatranj.Player;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Docs/04 §A1 Shade Crawler: patrols at 2.5 u/s, turning at walls and ledges. If the player is within 6 u on
    /// the same platform (±1.5 u vertical) it chases at 3.5 u/s, still refusing to walk off ledges. Contact damage only.
    /// </summary>
    public class Crawler : EnemyBase
    {
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

            if (TryGetPlayer(out PlayerController p) && EnemySensors.WithinBox(Feet, p.Position, config.detectRange, config.verticalTolerance))
            {
                float dx = p.Position.x - Feet.x;
                int dir = dx >= 0f ? 1 : -1;
                SetFacing(dir);
                if (Mathf.Abs(dx) < 0.1f || Blocked(dir))
                {
                    SetVelocityX(0f);
                    SetAnim("Idle");
                }
                else
                {
                    SetVelocityX(dir * config.chaseSpeed);
                    SetAnim("Walk");
                }
                return;
            }

            Patrol(config.patrolSpeed);
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
