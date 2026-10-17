using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Level;
using UnityEngine;

namespace Shatranj.EditorTools.Level
{
    /// <summary>
    /// Bakes tile cells into a small set of BoxCollider2D rectangles (greedy: horizontal runs per row, then identical
    /// runs merged upward). Solid groups get a static Rigidbody2D + CompositeCollider2D (Outlines) so edges are seamless;
    /// trigger groups (hazards) stay plain boxes. Replaces TilemapCollider2D (D-112): collision no longer depends on
    /// the tilemap collider's runtime generation, which produced no shapes for the 1 261-tile Phase 1 ground.
    /// </summary>
    public static class ColliderBaker
    {
        /// <summary>Fraction of a tile removed from the top of hazard boxes (spike tips sit low in the 50 px tile).</summary>
        public const float HazardTopInset = 0.45f;
        /// <summary>World units removed from each side of a hazard run.</summary>
        public const float HazardSideInset = 0.1f;

        public struct Rect2i
        {
            public int x, y, w, h;
            public Rect2i(int x, int y, int w, int h) { this.x = x; this.y = y; this.w = w; this.h = h; }
        }

        /// <summary>Greedy rectangle cover of the cells (exact: every cell covered once, nothing extra).</summary>
        public static List<Rect2i> Merge(HashSet<Vector2Int> cells)
        {
            var result = new List<Rect2i>();
            if (cells.Count == 0) return result;

            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (Vector2Int c in cells)
            {
                if (c.x < minX) minX = c.x; if (c.y < minY) minY = c.y;
                if (c.x > maxX) maxX = c.x; if (c.y > maxY) maxY = c.y;
            }

            // Row runs.
            var open = new Dictionary<(int x, int w), Rect2i>();
            for (int y = minY; y <= maxY + 1; y++)
            {
                var runs = new List<(int x, int w)>();
                if (y <= maxY)
                {
                    int x = minX;
                    while (x <= maxX)
                    {
                        if (!cells.Contains(new Vector2Int(x, y))) { x++; continue; }
                        int start = x;
                        while (x <= maxX && cells.Contains(new Vector2Int(x, y))) x++;
                        runs.Add((start, x - start));
                    }
                }

                var next = new Dictionary<(int x, int w), Rect2i>();
                foreach (var run in runs)
                {
                    if (open.TryGetValue(run, out Rect2i r)) { r.h++; next[run] = r; open.Remove(run); }
                    else next[run] = new Rect2i(run.x, y, run.w, 1);
                }
                foreach (Rect2i closed in open.Values) result.Add(closed);
                open = next;
            }
            foreach (Rect2i closed in open.Values) result.Add(closed);
            return result;
        }

        /// <summary>
        /// Creates the collision object for one cell group under <paramref name="parent"/>.
        /// Returns the GameObject (so the caller can add DamageDealer / effector components).
        /// </summary>
        public static GameObject Bake(string name, Transform parent, HashSet<Vector2Int> cells, int layer, bool trigger, bool oneWay, out int boxCount)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.layer = layer;

            List<Rect2i> rects = Merge(cells);
            boxCount = rects.Count;
            float t = RoomData.TileSize;

            Rigidbody2D rb = null;
            CompositeCollider2D comp = null;
            if (!trigger)
            {
                rb = root.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                comp = root.AddComponent<CompositeCollider2D>();
                comp.geometryType = CompositeCollider2D.GeometryType.Outlines;
                comp.generationType = CompositeCollider2D.GenerationType.Synchronous;
            }

            for (int i = 0; i < rects.Count; i++)
            {
                Rect2i r = rects[i];
                var box = new GameObject($"Box_{r.x}_{r.y}_{r.w}x{r.h}");
                box.transform.SetParent(root.transform, false);
                box.layer = layer;

                float left = r.x * t, bottom = r.y * t, width = r.w * t, height = r.h * t;
                if (trigger)
                {
                    // D-113: hazards hurt only where the spikes are drawn — the lower part of the top tile, inset at
                    // the sides — so standing on a neighbouring ledge or jumping off its edge is safe.
                    height -= t * HazardTopInset;
                    left += HazardSideInset;
                    width = Mathf.Max(0.05f, width - 2f * HazardSideInset);
                }
                box.transform.position = new Vector3(left + width * 0.5f, bottom + height * 0.5f, 0f);
                var col = box.AddComponent<BoxCollider2D>();
                col.size = new Vector2(width, height);
                col.isTrigger = trigger;
                if (!trigger) col.compositeOperation = Collider2D.CompositeOperation.Merge;
            }

            if (oneWay && comp != null)
            {
                var eff = root.AddComponent<PlatformEffector2D>();
                eff.useOneWay = true;
                eff.useOneWayGrouping = true;
                eff.surfaceArc = 170f;
                eff.useSideFriction = false;
                eff.useSideBounce = false;
                comp.usedByEffector = true;
            }
            return root;
        }
    }
}
