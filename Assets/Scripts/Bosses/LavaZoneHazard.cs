using System.Collections;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>
    /// One Zulmat lava zone (Docs/04 §E Lava Zones): a strip of Lava_Zone tiles (100×50, 4-frame loop, tileable) on the
    /// floor. Telegraph: dim, flickering "cracks" for <c>warnTime</c> (harmless). Then the lava rises (<c>riseTime</c>) and
    /// hurts on touch for <c>duration</c> (Hazard trigger, DamageType.Lava: 1 heart, player i-frames, no teleport,
    /// knockback lifts the player), then sinks and removes itself. Built at runtime — no prefab.
    /// </summary>
    public class LavaZoneHazard : MonoBehaviour
    {
        private const float TileWidth = 1f;      // 100 px at 100 px/u
        private const float TileHeight = 0.5f;   // 50 px
        private const float HurtHeight = 0.45f;
        private static readonly Color CrackColor = new Color(1f, 0.35f, 0.15f, 0.45f);

        private SpriteRenderer[] tiles;
        private Transform strip;
        private BoxCollider2D box;
        private float warnTime, duration, riseTime;

        public float Width { get; private set; }
        public float CenterX => transform.position.x;
        public bool Finished { get; private set; }

        public static LavaZoneHazard Spawn(Vector2 floorCenter, float width, float warnTime, float duration, float riseTime, int damage, Transform parent)
        {
            var go = new GameObject("LavaZone");
            go.layer = GameLayers.Hazard;
            go.transform.SetParent(parent, true);
            go.transform.position = floorCenter;

            var stripGo = new GameObject("Strip");
            stripGo.layer = GameLayers.Ignore;
            stripGo.transform.SetParent(go.transform, false);
            int count = Mathf.Max(1, Mathf.CeilToInt(width / TileWidth));
            var tiles = new SpriteRenderer[count];
            SpriteAnimationSet set = SpriteAnimationSet.Load("Lava_Zone");
            float start = -count * TileWidth * 0.5f + TileWidth * 0.5f;
            for (int i = 0; i < count; i++)
            {
                var t = new GameObject("Tile_" + i);
                t.layer = GameLayers.Ignore;
                t.transform.SetParent(stripGo.transform, false);
                t.transform.localPosition = new Vector3(start + i * TileWidth, TileHeight * 0.5f, 0f);
                var sr = t.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = GameLayers.Sorting.FX;
                sr.color = CrackColor;
                var anim = t.AddComponent<SpriteAnimator>();
                anim.Set = set;
                anim.Play(FxPool.DefaultClip, restart: true);
                tiles[i] = sr;
            }

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(count * TileWidth, HurtHeight);
            box.offset = new Vector2(0f, HurtHeight * 0.5f);
            box.enabled = false;
            go.AddComponent<DamageDealer>().Configure(damage, DamageType.Lava, null, horizontal: false);

            var zone = go.AddComponent<LavaZoneHazard>();
            zone.tiles = tiles;
            zone.strip = stripGo.transform;
            zone.box = box;
            zone.warnTime = warnTime;
            zone.duration = duration;
            zone.riseTime = Mathf.Max(0.01f, riseTime);
            zone.Width = count * TileWidth;
            return zone;
        }

        /// <summary>Do two zones overlap (with a small gap)?</summary>
        public bool Overlaps(float centerX, float width, float gap = 0.5f) => Mathf.Abs(centerX - CenterX) < (width + Width) * 0.5f + gap;

        private void Start() => StartCoroutine(Life());

        private IEnumerator Life()
        {
            // Telegraph: glowing cracks, low and flickering.
            SetStripHeight(0.35f);
            float t = 0f;
            while (t < warnTime)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(0.25f, 0.65f, 0.5f + 0.5f * Mathf.Sin(t * 18f));
                SetColor(new Color(CrackColor.r, CrackColor.g, CrackColor.b, a));
                yield return null;
            }

            // Rise and hurt.
            AudioManager.Play("zulmat_lava");
            box.enabled = true;
            SetColor(Color.white);
            for (t = 0f; t < riseTime; t += Time.deltaTime) { SetStripHeight(Mathf.Lerp(0.35f, 1f, t / riseTime)); yield return null; }
            SetStripHeight(1f);
            for (t = 0f; t < duration; t += Time.deltaTime) yield return null;

            // Sink.
            box.enabled = false;
            for (t = 0f; t < riseTime; t += Time.deltaTime) { SetStripHeight(Mathf.Lerp(1f, 0f, t / riseTime)); yield return null; }
            Finished = true;
            Destroy(gameObject);
        }

        private void SetStripHeight(float k) => strip.localScale = new Vector3(1f, Mathf.Max(0.01f, k), 1f);

        private void SetColor(Color c)
        {
            for (int i = 0; i < tiles.Length; i++) if (tiles[i] != null) tiles[i].color = c;
        }
    }
}
