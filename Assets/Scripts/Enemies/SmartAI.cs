using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Pure decision helpers for the Phase 2 "better AI" (Docs/04 §C): keep spacing, back off, react to a jumping
    /// player, lead moving targets, lob arcs, scale projectiles. No Unity objects, so they are unit-tested directly.
    /// </summary>
    public static class SpacingLogic
    {
        /// <summary>Hold a distance band: −1 = retreat (closer than <paramref name="retreatIfCloser"/>), +1 = advance
        /// (farther than <paramref name="advanceIfFarther"/>), 0 = stay. Thrower: 5 / 10 → keeps 6–9 u.</summary>
        public static int BandMove(float distance, float retreatIfCloser, float advanceIfFarther)
        {
            if (distance < retreatIfCloser) return -1;
            if (distance > advanceIfFarther) return 1;
            return 0;
        }

        /// <summary>Close to a preferred distance: +1 approach, −1 back off, 0 in range (± <paramref name="tolerance"/>). Knight: 2.0 u.</summary>
        public static int ApproachToRange(float distance, float preferred, float tolerance)
        {
            if (distance > preferred + tolerance) return 1;
            if (distance < preferred - tolerance) return -1;
            return 0;
        }

        /// <summary>Docs/04 §C: a knight won't swing at a player who is mid-air above the threshold — it waits.</summary>
        public static bool ShouldWaitForAirbornePlayer(bool playerGrounded, float playerHeightAboveEnemyFeet, float threshold)
        {
            if (threshold <= 0f || playerGrounded) return false;
            return playerHeightAboveEnemyFeet > threshold;
        }

        /// <summary>First-order lead: aim where the target will be after <paramref name="leadTime"/> seconds.</summary>
        public static Vector2 LeadTarget(Vector2 position, Vector2 velocity, float leadTime) => position + velocity * leadTime;

        /// <summary>
        /// Launch velocity for a slight arc under <paramref name="gravity"/> that reaches <paramref name="to"/>, with the
        /// horizontal speed set so flight time = horizontal distance / <paramref name="speed"/> (min 0.15 s).
        /// </summary>
        public static Vector2 ArcVelocity(Vector2 from, Vector2 to, float speed, float gravity)
        {
            Vector2 d = to - from;
            float t = Mathf.Max(0.15f, Mathf.Abs(d.x) / Mathf.Max(0.01f, speed));
            return new Vector2(d.x / t, d.y / t + 0.5f * gravity * t);
        }

        /// <summary>Docs/04 §C4: projectile size × (1 + factor × (s − 1)).</summary>
        public static float ProjectileScale(float enemyScale, float factor) => 1f + factor * (enemyScale - 1f);
    }

    /// <summary>
    /// Perception memory (Docs/04 §C): becomes aware when the player is seen and loses track after
    /// <c>loseTrackTime</c> seconds without a sighting (0 = never forgets).
    /// </summary>
    public class Awareness
    {
        public bool Aware { get; private set; }
        public float TimeSinceSeen { get; private set; } = float.PositiveInfinity;
        public Vector2 LastKnownPosition { get; private set; }

        public bool Tick(bool seesPlayer, Vector2 playerPosition, float dt, float loseTrackTime)
        {
            if (seesPlayer)
            {
                Aware = true;
                TimeSinceSeen = 0f;
                LastKnownPosition = playerPosition;
                return true;
            }
            TimeSinceSeen += dt;
            if (Aware && loseTrackTime > 0f && TimeSinceSeen >= loseTrackTime) Aware = false;
            return Aware;
        }

        public void Forget()
        {
            Aware = false;
            TimeSinceSeen = float.PositiveInfinity;
        }
    }
}
