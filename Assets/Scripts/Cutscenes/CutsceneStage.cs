using System.Collections;
using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Cutscenes
{
    /// <summary>
    /// The puppet theatre for cutscenes (Docs/08 §1): puppets (in-game sprite sets or single sprites) and full-screen
    /// image panels with Ken Burns pan/zoom. Coordinates are world units relative to the screen centre (19.6 × 11 u),
    /// i.e. relative to this stage, which sits at the locked camera's position.
    /// </summary>
    public class CutsceneStage : MonoBehaviour
    {
        private const float ScreenW = 19.6f;
        private const float ScreenH = 11f;

        private readonly Dictionary<string, GameObject> actors = new Dictionary<string, GameObject>();
        private int imageOrder;

        public GameObject Get(string id) => id != null && actors.TryGetValue(id, out GameObject go) ? go : null;

        public SpriteRenderer RendererOf(string id)
        {
            GameObject go = Get(id);
            return go != null ? go.GetComponent<SpriteRenderer>() : null;
        }

        public Vector3 StagePoint(Vector2 local) => transform.position + new Vector3(local.x, local.y, 0f);

        // ------------------------------------------------------------------
        // Puppets
        // ------------------------------------------------------------------

        /// <summary>spawn: id, sprite | character (+anim), pos[x,y], scale, layer, flip, alpha.</summary>
        public GameObject Spawn(Beat b)
        {
            string id = b.Str("id", "puppet" + actors.Count);
            Remove(id);

            var go = new GameObject("Puppet_" + id);
            go.transform.SetParent(transform, false);
            go.transform.position = StagePoint(b.Vec2("pos"));
            float scale = b.Float("scale", 1f);
            go.transform.localScale = new Vector3(b.Bool("flip") ? -scale : scale, scale, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = b.Str("layer", GameLayers.Sorting.Player);
            sr.sortingOrder = b.Int("order", 0);
            Color c = Color.white;
            c.a = b.Float("alpha", 1f);
            sr.color = c;

            string character = b.Str("character");
            if (!string.IsNullOrEmpty(character))
            {
                var anim = go.AddComponent<SpriteAnimator>();
                anim.Set = SpriteAnimationSet.Load(character);
                if (anim.Set == null) Debug.LogWarning($"Cutscene: no animation set '{character}'");
                anim.Play(b.Str("anim", "Idle"), true);
                go.AddComponent<PuppetShadow>();
            }
            else
            {
                sr.sprite = Catalog(b.Str("sprite"));
            }

            actors[id] = go;
            return go;
        }

        public void PlayAnim(string id, string clip)
        {
            GameObject go = Get(id);
            if (go == null) { Debug.LogWarning($"Cutscene: anim on unknown puppet '{id}'"); return; }
            var anim = go.GetComponent<SpriteAnimator>();
            if (anim != null) anim.Play(clip, true);
        }

        // ------------------------------------------------------------------
        // Images (full-screen panels)
        // ------------------------------------------------------------------

        /// <summary>image: id, sprite, alpha (target), fade (seconds), layer, fit (cover | none), kenburns {from:[scale,x,y], to:[scale,x,y], time}.</summary>
        public IEnumerator ShowImage(Beat b, bool instant)
        {
            string id = b.Str("id", "image" + actors.Count);
            Remove(id);
            Sprite sprite = Catalog(b.Str("sprite"));

            var go = new GameObject("Image_" + id);
            go.transform.SetParent(transform, false);
            go.transform.position = transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = b.Str("layer", GameLayers.Sorting.Props);
            sr.sortingOrder = ++imageOrder;
            SpriteCatalog cat = SpriteCatalog.Instance;
            if (cat != null && cat.unlitMaterial != null) sr.sharedMaterial = cat.unlitMaterial;
            actors[id] = go;

            float baseScale = 1f;
            bool cover = b.Str("fit", "cover") != "none";   // "none" = the sprite's own size (title logo, small panels)
            if (sprite != null && cover)
            {
                Vector2 s = sprite.bounds.size;
                baseScale = Mathf.Max(ScreenW / Mathf.Max(0.01f, s.x), ScreenH / Mathf.Max(0.01f, s.y));
            }

            Beat kb = b.Child("kenburns");
            Vector3 from = kb != null ? KbValue(kb, "from") : new Vector3(1f, 0f, 0f);
            Vector3 to = kb != null ? KbValue(kb, "to") : from;
            float kbTime = kb != null ? kb.Float("time", 6f) : 0f;
            ApplyKb(go.transform, baseScale, from);
            if (kb != null && !instant) StartCoroutine(KenBurns(go.transform, baseScale, from, to, kbTime));
            else if (kb != null) ApplyKb(go.transform, baseScale, to);

            float targetAlpha = b.Float("alpha", 1f);
            float fade = instant ? 0f : b.Float("fade", 0.4f);
            yield return FadeRenderer(sr, 0f, targetAlpha, fade);
        }

        private static Vector3 KbValue(Beat kb, string key)
        {
            if (kb.Raw(key) is List<object> list && list.Count >= 3)
                return new Vector3(Beat.ToFloat(list[0]), Beat.ToFloat(list[1]), Beat.ToFloat(list[2]));
            return new Vector3(1f, 0f, 0f);
        }

        private void ApplyKb(Transform t, float baseScale, Vector3 v)
        {
            float s = baseScale * Mathf.Max(0.01f, v.x);
            t.localScale = new Vector3(s, s, 1f);
            t.position = transform.position + new Vector3(v.y, v.z, 0f);
        }

        private IEnumerator KenBurns(Transform t, float baseScale, Vector3 from, Vector3 to, float time)
        {
            for (float e = 0f; e < time && t != null; e += Time.deltaTime)
            {
                ApplyKb(t, baseScale, Vector3.Lerp(from, to, Easing.Evaluate(Ease.InOutQuad, e / time)));
                yield return null;
            }
            if (t != null) ApplyKb(t, baseScale, to);
        }

        // ------------------------------------------------------------------
        // Common
        // ------------------------------------------------------------------

        public IEnumerator RemoveAfterFade(string id, float fade)
        {
            GameObject go = Get(id);
            if (go == null) yield break;
            actors.Remove(id);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && fade > 0f) yield return FadeRenderer(sr, sr.color.a, 0f, fade);
            if (go != null) Destroy(go);
        }

        public void Remove(string id)
        {
            GameObject go = Get(id);
            if (go == null) return;
            actors.Remove(id);
            Destroy(go);
        }

        public static IEnumerator FadeRenderer(SpriteRenderer sr, float from, float to, float time)
        {
            if (sr == null) yield break;
            Color c = sr.color;
            if (time <= 0f) { c.a = to; sr.color = c; yield break; }
            for (float e = 0f; e < time && sr != null; e += Time.deltaTime)
            {
                c.a = Mathf.Lerp(from, to, e / time);
                sr.color = c;
                yield return null;
            }
            if (sr != null) { c.a = to; sr.color = c; }
        }

        private static Sprite Catalog(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;
            SpriteCatalog cat = SpriteCatalog.Instance;
            Sprite s = cat != null ? cat.Find(spriteName) : null;
            if (s == null) Debug.LogWarning($"Cutscene: sprite '{spriteName}' not in the SpriteCatalog (run Shatranj → Build Cutscene Catalog).");
            return s;
        }
    }
}
