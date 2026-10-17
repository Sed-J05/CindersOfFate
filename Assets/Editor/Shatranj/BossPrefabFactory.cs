using System;
using Shatranj.Bosses;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Builds a boss GameObject from its BossConfig (Docs/04 sizes): Enemy-layer root at the feet with a dynamic
    /// Rigidbody2D, body collider, Health and behaviour; Visual (SpriteAnimator), Flash overlay, Hurtbox (visible body),
    /// Contact (collider size, contact damage).
    /// </summary>
    public static class BossPrefabFactory
    {
        public static GameObject Build(string setName, BossConfig cfg, Type behaviour)
        {
            Material lit = SceneBuildUtil.SpriteLitMaterial();
            Material silhouette = EnemyPrefabFactory.GetOrCreateSilhouetteMaterial();

            var root = new GameObject(setName);
            root.layer = GameLayers.Enemy;
            root.tag = GameLayers.Tags.Boss;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = cfg.gravityScale;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var bodyCol = root.AddComponent<BoxCollider2D>();
            bodyCol.size = cfg.colliderSize;
            bodyCol.offset = new Vector2(0f, cfg.colliderSize.y * 0.5f);
            bodyCol.sharedMaterial = SceneBuildUtil.GetOrCreateZeroFrictionMaterial();

            var health = root.AddComponent<Health>();
            SceneBuildUtil.SetInt(health, "maxHp", cfg.maxHp);
            SceneBuildUtil.SetFloat(health, "invulnerabilityDuration", 0f);
            SceneBuildUtil.SetFloat(health, "hitFlashDuration", cfg.hitFlashDuration);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = GameLayers.Sorting.Enemies;
            sr.sortingOrder = 10;
            if (lit != null) sr.sharedMaterial = lit;
            var set = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(Art.RebuildAnimations.SetPath(setName));
            SpriteClip idle = set != null ? set.Find("Idle") : null;
            sr.sprite = idle != null && idle.IsValid ? idle.frames[0] : null;
            var anim = visual.AddComponent<SpriteAnimator>();
            visual.AddComponent<PuppetShadow>();   // Docs/07 §5
            if (set != null) SceneBuildUtil.SetReference(anim, "set", set);
            SceneBuildUtil.SetString(anim, "defaultClip", "Idle");

            SpriteRenderer flash = null;
            if (silhouette != null)
            {
                var flashGo = new GameObject("Flash");
                flashGo.transform.SetParent(visual.transform, false);
                flash = flashGo.AddComponent<SpriteRenderer>();
                flash.sharedMaterial = silhouette;
                flash.sortingLayerName = GameLayers.Sorting.Enemies;
                flash.sortingOrder = 15;
                flash.sprite = sr.sprite;
                flash.enabled = false;
            }

            var hurtGo = new GameObject("Hurtbox");
            hurtGo.transform.SetParent(root.transform, false);
            hurtGo.layer = GameLayers.EnemyHurtbox;
            var hurt = hurtGo.AddComponent<BoxCollider2D>();
            hurt.isTrigger = true;
            hurt.size = cfg.bodySize;
            hurt.offset = new Vector2(0f, cfg.bodySize.y * 0.5f);

            var contactGo = new GameObject("Contact");
            contactGo.transform.SetParent(root.transform, false);
            contactGo.layer = GameLayers.EnemyAttack;
            var contactCol = contactGo.AddComponent<BoxCollider2D>();
            contactCol.isTrigger = true;
            contactCol.size = cfg.colliderSize;
            contactCol.offset = new Vector2(0f, cfg.colliderSize.y * 0.5f);
            var dealer = contactGo.AddComponent<DamageDealer>();
            SceneBuildUtil.SetInt(dealer, "damage", cfg.contactDamage);
            SceneBuildUtil.SetEnum(dealer, "type", (int)DamageType.Contact);

            var boss = (BossBase)root.AddComponent(behaviour);
            SceneBuildUtil.SetReference(boss, "config", cfg);
            SceneBuildUtil.SetReference(boss, "visualRoot", visual.transform);
            SceneBuildUtil.SetReference(boss, "body", sr);
            SceneBuildUtil.SetReference(boss, "animator", anim);
            if (flash != null) SceneBuildUtil.SetReference(boss, "flashRenderer", flash);
            SceneBuildUtil.SetReference(boss, "bodyCollider", bodyCol);
            SceneBuildUtil.SetReference(boss, "hurtbox", hurt);
            SceneBuildUtil.SetReference(boss, "contact", dealer);
            return root;
        }
    }
}
