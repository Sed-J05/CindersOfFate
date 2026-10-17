using System;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Enemies;
using Shatranj.Visuals;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Builds an enemy GameObject from its EnemyConfig (Docs/04 sizes): Enemy-layer root with Rigidbody2D + body
    /// collider + Health + behaviour, Visual (SpriteAnimator), Flash overlay (silhouette shader), Hurtbox
    /// (EnemyHurtbox trigger = visible body size), Contact (EnemyAttack trigger = collider size + DamageDealer),
    /// optional '!' marker. Root sits at the feet for ground enemies, at the centre for flyers.
    /// </summary>
    public static class EnemyPrefabFactory
    {
        public const string MaterialsFolder = "Assets/Data/Materials";
        public const string SilhouetteMaterialPath = MaterialsFolder + "/SpriteSilhouette.mat";
        public const string SilhouetteShaderName = "Shatranj/SpriteSilhouette";

        public static Material GetOrCreateSilhouetteMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(SilhouetteMaterialPath);
            if (mat != null) return mat;
            Shader shader = Shader.Find(SilhouetteShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"EnemyPrefabFactory: shader '{SilhouetteShaderName}' not found — hit flash disabled until it imports.");
                return null;
            }
            GenerateConfigs.EnsureFolder(MaterialsFolder);
            mat = new Material(shader) { name = "SpriteSilhouette" };
            AssetDatabase.CreateAsset(mat, SilhouetteMaterialPath);
            return mat;
        }

        public static GameObject Build(string setName, EnemyConfig cfg, Type behaviour)
        {
            bool flyer = cfg.locomotion == EnemyLocomotion.Flyer;
            Material lit = SceneBuildUtil.SpriteLitMaterial();
            Material silhouette = GetOrCreateSilhouetteMaterial();

            var root = new GameObject(setName);
            root.layer = GameLayers.Enemy;
            root.tag = GameLayers.Tags.Enemy;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = flyer ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            rb.gravityScale = flyer ? 0f : cfg.gravityScale;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (!flyer) rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            Vector2 colliderCenter = flyer ? Vector2.zero : new Vector2(0f, cfg.colliderSize.y * 0.5f);
            Vector2 bodyCenter = flyer ? Vector2.zero : new Vector2(0f, cfg.bodySize.y * 0.5f);

            Collider2D bodyCol = AddShape(root, cfg, colliderCenter, trigger: false);
            bodyCol.sharedMaterial = SceneBuildUtil.GetOrCreateZeroFrictionMaterial();

            var health = root.AddComponent<Health>();
            SceneBuildUtil.SetInt(health, "maxHp", cfg.maxHp);
            SceneBuildUtil.SetFloat(health, "invulnerabilityDuration", 0f);
            SceneBuildUtil.SetFloat(health, "hitFlashDuration", cfg.hitFlashDuration);

            // Visual
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = GameLayers.Sorting.Enemies;
            if (lit != null) sr.sharedMaterial = lit;
            var set = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(Art.RebuildAnimations.SetPath(setName));
            string defaultClip = flyer ? "Fly" : "Idle";
            SpriteClip first = set != null ? set.Find(defaultClip) : null;
            sr.sprite = first != null && first.IsValid ? first.frames[0] : null;
            var anim = visual.AddComponent<SpriteAnimator>();
            visual.AddComponent<PuppetShadow>();   // Docs/07 §5
            if (set != null) SceneBuildUtil.SetReference(anim, "set", set);
            SceneBuildUtil.SetString(anim, "defaultClip", defaultClip);

            // Flash overlay (same sprite, solid colour)
            SpriteRenderer flash = null;
            if (silhouette != null)
            {
                var flashGo = new GameObject("Flash");
                flashGo.transform.SetParent(visual.transform, false);
                flash = flashGo.AddComponent<SpriteRenderer>();
                flash.sharedMaterial = silhouette;
                flash.sortingLayerName = GameLayers.Sorting.Enemies;
                flash.sortingOrder = 5;
                flash.sprite = sr.sprite;
                flash.enabled = false;
            }

            // Hurtbox (what the nail hits) — visible body size
            var hurtGo = new GameObject("Hurtbox");
            hurtGo.transform.SetParent(root.transform, false);
            hurtGo.layer = GameLayers.EnemyHurtbox;
            var hurt = hurtGo.AddComponent<BoxCollider2D>();
            hurt.isTrigger = true;
            hurt.size = cfg.bodySize;
            hurt.offset = bodyCenter;

            // Contact damage — collider size
            var contactGo = new GameObject("Contact");
            contactGo.transform.SetParent(root.transform, false);
            contactGo.layer = GameLayers.EnemyAttack;
            AddShape(contactGo, cfg, colliderCenter, trigger: true);
            var dealer = contactGo.AddComponent<DamageDealer>();
            SceneBuildUtil.SetInt(dealer, "damage", cfg.contactDamage);
            SceneBuildUtil.SetEnum(dealer, "type", (int)DamageType.Contact);

            // '!' marker
            GameObject exclamation = null;
            if (cfg.showExclamation)
            {
                exclamation = new GameObject("Exclamation");
                exclamation.transform.SetParent(root.transform, false);
                exclamation.transform.localPosition = new Vector3(0f, (flyer ? cfg.bodySize.y * 0.5f : cfg.bodySize.y) + 0.45f, 0f);
                var exSr = exclamation.AddComponent<SpriteRenderer>();
                exSr.sortingLayerName = GameLayers.Sorting.FX;
                var exSet = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(Art.RebuildAnimations.SetPath("Exclamation"));
                SpriteClip exClip = exSet != null ? exSet.Find(FxPool.DefaultClip) : null;
                exSr.sprite = exClip != null && exClip.IsValid ? exClip.frames[0] : null;
                var exAnim = exclamation.AddComponent<SpriteAnimator>();
                if (exSet != null) SceneBuildUtil.SetReference(exAnim, "set", exSet);
                SceneBuildUtil.SetString(exAnim, "defaultClip", FxPool.DefaultClip);
                exclamation.SetActive(false);
            }

            var enemy = (EnemyBase)root.AddComponent(behaviour);
            SceneBuildUtil.SetReference(enemy, "config", cfg);
            SceneBuildUtil.SetReference(enemy, "visualRoot", visual.transform);
            SceneBuildUtil.SetReference(enemy, "body", sr);
            SceneBuildUtil.SetReference(enemy, "animator", anim);
            if (flash != null) SceneBuildUtil.SetReference(enemy, "flashRenderer", flash);
            if (exclamation != null) SceneBuildUtil.SetReference(enemy, "exclamation", exclamation);
            SceneBuildUtil.SetReference(enemy, "bodyCollider", bodyCol);
            SceneBuildUtil.SetReference(enemy, "hurtbox", hurt);
            SceneBuildUtil.SetReference(enemy, "contact", dealer);
            if (cfg.scalesWithFlame) root.AddComponent<FlameScaler>();   // Docs/07 §4 (Phase 2 enemies grow with the flame)
            return root;
        }

        private static Collider2D AddShape(GameObject go, EnemyConfig cfg, Vector2 center, bool trigger)
        {
            if (cfg.colliderShape == EnemyColliderShape.Circle)
            {
                var c = go.AddComponent<CircleCollider2D>();
                c.radius = cfg.colliderRadius;
                c.offset = center;
                c.isTrigger = trigger;
                return c;
            }
            var b = go.AddComponent<BoxCollider2D>();
            b.size = cfg.colliderSize;
            b.offset = center;
            b.isTrigger = trigger;
            return b;
        }
    }
}
