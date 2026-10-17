using System;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Owns the pools for every player-side effect: slashes, hit sparks, soul orbs, dash ghosts, fireballs and
    /// impacts. Templates are built in code from placeholder sprites; Prompt 4 can assign real sprites via
    /// the serialized fields without touching callers.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerFxPools : MonoBehaviour
    {
        [Header("Optional real sprites (placeholders when empty)")]
        [SerializeField] private Sprite slashSideSprite;
        [SerializeField] private Sprite slashUpSprite;
        [SerializeField] private Sprite hitSparkSprite;
        [SerializeField] private Sprite soulOrbSprite;
        [SerializeField] private Sprite fireballSprite;
        [SerializeField] private Sprite fireballImpactSprite;

        private static readonly Color SlashColor = new Color(0.92f, 0.97f, 1f, 0.85f);
        private static readonly Color SparkColor = new Color(1f, 0.95f, 0.6f, 0.95f);
        private static readonly Color GhostColor = new Color(0.55f, 0.9f, 1f, 0.55f);
        private static readonly Color OrbRimColor = new Color(0.31f, 0.64f, 1f, 1f);
        private static readonly Color OrbCoreColor = new Color(0.05f, 0.03f, 0.08f, 1f);
        private static readonly Color FireballColor = new Color(0.81f, 0.92f, 1f, 1f);
        private static readonly Color ImpactColor = new Color(0.6f, 0.85f, 1f, 0.8f);

        private Transform fxRoot;
        private ObjectPool<SpriteFx> fxPool;
        private ObjectPool<SoulOrbFx> orbPool;
        private ObjectPool<FireballProjectile> fireballPool;

        private Action<SpriteFx> releaseFx;
        private Action<SoulOrbFx> releaseOrb;
        private Action<FireballProjectile> releaseFireball;
        private Action<Vector2> fireballImpact;

        private void Awake()
        {
            fxRoot = new GameObject("[PlayerFX]").transform;

            releaseFx = f => fxPool.Release(f);
            releaseOrb = o => orbPool.Release(o);
            releaseFireball = p => fireballPool.Release(p);
            fireballImpact = PlayFireballImpact;

            fxPool = new ObjectPool<SpriteFx>(BuildFxTemplate(), fxRoot, 12);
            orbPool = new ObjectPool<SoulOrbFx>(BuildOrbTemplate(), fxRoot, 6);
            fireballPool = new ObjectPool<FireballProjectile>(BuildFireballTemplate(), fxRoot, 3);
        }

        private void OnDestroy()
        {
            if (fxRoot != null) Destroy(fxRoot.gameObject);
        }

        // ------------------------------------------------------------------
        // Templates (inactive; the pool clones them)
        // ------------------------------------------------------------------

        private SpriteFx BuildFxTemplate()
        {
            var go = new GameObject("FxTemplate");
            go.transform.SetParent(fxRoot, false);
            go.SetActive(false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderSprites.Square;
            sr.sortingLayerName = GameLayers.Sorting.FX;
            return go.AddComponent<SpriteFx>();
        }

        private SoulOrbFx BuildOrbTemplate()
        {
            var go = new GameObject("SoulOrbTemplate");
            go.transform.SetParent(fxRoot, false);
            go.SetActive(false);
            var rim = go.AddComponent<SpriteRenderer>();
            rim.sprite = soulOrbSprite != null ? soulOrbSprite : PlaceholderSprites.Circle;
            rim.color = soulOrbSprite != null ? Color.white : OrbRimColor;
            rim.sortingLayerName = GameLayers.Sorting.FX;
            rim.sortingOrder = 5;
            if (soulOrbSprite == null)
            {
                go.transform.localScale = Vector3.one * 0.48f;
                var core = new GameObject("Core");
                core.transform.SetParent(go.transform, false);
                core.transform.localScale = Vector3.one * 0.7f;
                var coreSr = core.AddComponent<SpriteRenderer>();
                coreSr.sprite = PlaceholderSprites.Circle;
                coreSr.color = OrbCoreColor;
                coreSr.sortingLayerName = GameLayers.Sorting.FX;
                coreSr.sortingOrder = 6;
            }
            return go.AddComponent<SoulOrbFx>();
        }

        private FireballProjectile BuildFireballTemplate()
        {
            var go = new GameObject("FireballTemplate");
            go.transform.SetParent(fxRoot, false);
            go.SetActive(false);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = fireballSprite != null ? fireballSprite : PlaceholderSprites.Circle;
            sr.color = fireballSprite != null ? Color.white : FireballColor;
            sr.sortingLayerName = GameLayers.Sorting.FX;
            sr.sortingOrder = 4;
            if (fireballSprite == null) visual.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
            return go.AddComponent<FireballProjectile>();
        }

        // ------------------------------------------------------------------
        // Spawners
        // ------------------------------------------------------------------

        /// <summary>Slash crescent at the hitbox centre, mirrored by facing; alternate swings flip vertically.</summary>
        private SpriteFx GetFx(string sortingLayer, int order)
        {
            SpriteFx fx = fxPool.Get();
            fx.Init(releaseFx);
            fx.Renderer.sortingLayerName = sortingLayer;
            fx.Renderer.sortingOrder = order;
            return fx;
        }

        public void PlaySlash(Vector2 center, Vector2 size, bool up, int facing, bool alternate, float lifetime)
        {
            SpriteFx fx = GetFx(GameLayers.Sorting.FX, 0);
            Sprite sprite = up ? slashUpSprite : slashSideSprite;
            Vector3 scale;
            if (sprite != null) scale = new Vector3(up ? 1f : facing, alternate && !up ? -1f : 1f, 1f);
            else scale = new Vector3(size.x * (up ? 1f : facing), size.y * (alternate && !up ? -1f : 1f), 1f);
            Vector3 to = new Vector3(scale.x * 1.12f, scale.y * 1.12f, 1f);
            fx.Play(sprite != null ? sprite : PlaceholderSprites.Square, center, scale, to, SlashColor, lifetime, fade: true);
        }

        /// <summary>Small bright spark at the contact point.</summary>
        public void PlayHitSpark(Vector2 point, float lifetime)
        {
            SpriteFx fx = GetFx(GameLayers.Sorting.FX, 2);
            Sprite sprite = hitSparkSprite != null ? hitSparkSprite : PlaceholderSprites.Circle;
            float s = hitSparkSprite != null ? 1f : 0.45f;
            fx.Play(sprite, point, Vector3.one * (s * 0.6f), Vector3.one * (s * 1.4f), SparkColor, lifetime, fade: true, useUnscaledTime: true);
        }

        /// <summary>Dash afterimage: copies the current sprite, position and scale, fades out.</summary>
        public void SpawnGhost(SpriteRenderer source, float fadeTime)
        {
            if (source == null) return;
            SpriteFx fx = GetFx(GameLayers.Sorting.Player, -1);
            Vector3 scale = source.transform.lossyScale;
            fx.Play(source.sprite, source.transform.position, scale, scale, GhostColor, fadeTime, fade: true);
        }

        public void SpawnSoulOrb(Vector2 from, Transform target, Vector2 offset, float travelTime, Action onArrive)
        {
            SoulOrbFx orb = orbPool.Get();
            orb.Init(releaseOrb);
            orb.Play(from, target, offset, travelTime, onArrive);
        }

        public FireballProjectile SpawnFireball(Vector2 position, int facing, PlayerConfig config, GameObject owner)
        {
            FireballProjectile p = fireballPool.Get();
            p.Init(releaseFireball, fireballImpact);
            p.Launch(position, facing, config, owner);
            AudioManager.Play("player_fireball");
            return p;
        }

        private void PlayFireballImpact(Vector2 at)
        {
            SpriteFx fx = GetFx(GameLayers.Sorting.FX, 3);
            Sprite sprite = fireballImpactSprite != null ? fireballImpactSprite : PlaceholderSprites.Circle;
            float s = fireballImpactSprite != null ? 1f : 1.0f;
            fx.Play(sprite, at, Vector3.one * (s * 0.8f), Vector3.one * (s * 1.9f), ImpactColor, 0.25f, fade: true);
            AudioManager.Play("fireball_impact");
        }

        /// <summary>Returns every pooled effect (death, room change).</summary>
        public void ReleaseAll()
        {
            fxPool.ReleaseAll();
            orbPool.ReleaseAll();
            fireballPool.ReleaseAll();
        }
    }
}
