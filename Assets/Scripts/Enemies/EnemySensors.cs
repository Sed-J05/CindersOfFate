using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Shared, allocation-free AI queries (Docs/02 §5): ground under the feet, ledge / wall ahead, line of sight,
    /// separation from other enemies, swept moves for flyers. All masks come from <see cref="GameLayers"/>.
    /// </summary>
    public static class EnemySensors
    {
        private static readonly RaycastHit2D[] hits = new RaycastHit2D[4];
        private static readonly Collider2D[] overlaps = new Collider2D[16];
        private static ContactFilter2D walkable, wall, enemies;
        private static bool initialized;

        private static void Init()
        {
            if (initialized) return;
            initialized = true;
            walkable = Filter(GameLayers.WalkableMask);
            wall = Filter(GameLayers.WallMask);
            enemies = Filter(GameLayers.EnemyMask);
        }

        private static ContactFilter2D Filter(int mask)
        {
            var f = new ContactFilter2D();
            f.SetLayerMask(mask);
            f.useTriggers = false;
            return f;
        }

        /// <summary>Standing on Ground or OneWay.</summary>
        public static bool Grounded(Vector2 feet, float halfWidth)
        {
            Init();
            var box = new Vector2(Mathf.Max(0.05f, halfWidth * 1.8f), 0.04f);
            return Physics2D.BoxCast(feet + new Vector2(0f, 0.05f), box, 0f, Vector2.down, walkable, hits, 0.1f) > 0;
        }

        /// <summary>Is there floor just beyond the front foot? False = ledge.</summary>
        public static bool GroundAhead(Vector2 feet, int dir, float halfWidth, float probe)
        {
            Init();
            var origin = new Vector2(feet.x + dir * (halfWidth + 0.08f), feet.y + 0.1f);
            return Physics2D.Raycast(origin, Vector2.down, walkable, hits, 0.1f + probe) > 0;
        }

        /// <summary>Solid wall directly in front of the body (one-way platforms don't count).</summary>
        public static bool WallAhead(Vector2 center, int dir, float halfWidth, float height)
        {
            Init();
            var box = new Vector2(0.04f, Mathf.Max(0.05f, height * 0.6f));
            return Physics2D.BoxCast(center, box, 0f, new Vector2(dir, 0f), wall, hits, halfWidth + 0.06f) > 0;
        }

        /// <summary>No solid tile between the two points.</summary>
        public static bool LineOfSight(Vector2 from, Vector2 to)
        {
            Init();
            return Physics2D.Linecast(from, to, wall, hits) == 0;
        }

        /// <summary>Within <paramref name="maxDx"/> horizontally and <paramref name="maxDy"/> vertically ("same platform").</summary>
        public static bool WithinBox(Vector2 a, Vector2 b, float maxDx, float maxDy)
            => Mathf.Abs(b.x - a.x) <= maxDx && Mathf.Abs(b.y - a.y) <= maxDy;

        /// <summary>Push away from nearby enemy bodies; magnitude 0..~1 per neighbour.</summary>
        public static Vector2 Separation(Vector2 position, float radius, Transform self)
        {
            Init();
            if (radius <= 0f) return Vector2.zero;
            int n = Physics2D.OverlapCircle(position, radius, enemies, overlaps);
            Vector2 push = Vector2.zero;
            for (int i = 0; i < n; i++)
            {
                Rigidbody2D other = overlaps[i].attachedRigidbody;
                if (other == null || other.transform == self) continue;
                Vector2 d = position - other.position;
                float m = d.magnitude;
                if (m < 1e-3f) { d = Vector2.right; m = 1e-3f; }
                push += d / m * (1f - Mathf.Clamp01(m / radius));
            }
            return push;
        }

        /// <summary>How far a circle can travel along <paramref name="dir"/> before touching a solid tile (up to <paramref name="distance"/>).</summary>
        public static float SweepDistance(Vector2 from, float radius, Vector2 dir, float distance)
        {
            Init();
            int n = Physics2D.CircleCast(from, Mathf.Max(0.02f, radius), dir, wall, hits, distance);
            float allowed = distance;
            for (int i = 0; i < n; i++)
                if (hits[i].distance < allowed) allowed = hits[i].distance;
            return Mathf.Max(0f, allowed - 0.01f);
        }
    }
}
