using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Builds the player GameObject in code (Docs/02 §6, Docs/03 §1). Used by the Sandbox builder now and by
    /// Build Prefabs later, so there is exactly one definition of the player's hierarchy.
    /// </summary>
    public static class PlayerFactory
    {
        public const string ProtagonistMaterialPath = EnemyPrefabFactory.MaterialsFolder + "/SpriteProtagonistLit.mat";
        public const string ProtagonistShaderName = "Shatranj/SpriteProtagonistLit";

        /// <summary>
        /// Sprite-Lit material with the theme recolour (D-154: black/red protagonist in Phase 2). Created once; falls back
        /// to URP Sprite-Lit-Default (no recolour) if the shader has not imported yet.
        /// </summary>
        public static Material GetOrCreateProtagonistMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(ProtagonistMaterialPath);
            if (mat != null) return mat;
            Shader shader = Shader.Find(ProtagonistShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"PlayerFactory: shader '{ProtagonistShaderName}' not found — protagonist uses Sprite-Lit-Default (no Phase 2 recolour) until it imports.");
                return SceneBuildUtil.SpriteLitMaterial();
            }
            GenerateConfigs.EnsureFolder(EnemyPrefabFactory.MaterialsFolder);
            mat = new Material(shader) { name = "SpriteProtagonistLit" };
            AssetDatabase.CreateAsset(mat, ProtagonistMaterialPath);
            return mat;
        }

        public static GameObject Build(PlayerConfig config, Vector2 position, Transform respawnPoint = null)
        {
            Sprite sprite = SceneBuildUtil.GetOrCreateWhiteSprite();
            Material lit = SceneBuildUtil.SpriteLitMaterial();
            PhysicsMaterial2D zeroFriction = SceneBuildUtil.GetOrCreateZeroFrictionMaterial();

            var root = new GameObject("Player");
            root.layer = GameLayers.Player;
            root.tag = GameLayers.Tags.Player;
            root.transform.position = position;

            // --- Physics body (Docs/02 §6) ---
            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

            var capsule = root.AddComponent<CapsuleCollider2D>();
            capsule.direction = CapsuleDirection2D.Vertical;
            capsule.size = config.colliderSize;
            capsule.offset = config.colliderOffset;
            capsule.sharedMaterial = zeroFriction;

            // --- Visual child, flipped by localScale.x. Real sprites (Prompt 4) → scale 1 at the feet;
            //     otherwise a coloured 1.1×1.7 box with its feet on y = 0. ---
            var animSet = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(Art.RebuildAnimations.SetPath(PlayerAnimator.ProtagonistSetName));
            bool hasClips = animSet != null && animSet.HasAnyFrames;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = hasClips ? Vector3.zero : new Vector3(0f, 0.85f, 0f);
            visual.transform.localScale = hasClips ? Vector3.one : new Vector3(1.1f, 1.7f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = hasClips ? animSet.Find("Idle")?.frames[0] : sprite;
            sr.color = hasClips ? Color.white : PlayerAnimator.TintFor(PlayerState.Idle);
            sr.sortingLayerName = GameLayers.Sorting.Player;
            Material protagonist = GetOrCreateProtagonistMaterial();
            if (protagonist != null) sr.sharedMaterial = protagonist;
            var spriteAnimator = visual.AddComponent<SpriteAnimator>();
            visual.AddComponent<PuppetShadow>();   // Docs/07 §5
            if (hasClips) SceneBuildUtil.SetReference(spriteAnimator, "set", animSet);

            // Simple "eye" so facing is readable on the placeholder box (hidden when real clips exist).
            var eye = new GameObject("Eye");
            eye.SetActive(!hasClips);
            eye.transform.SetParent(visual.transform, false);
            eye.transform.localPosition = new Vector3(0.3f, 0.3f, 0f);
            eye.transform.localScale = new Vector3(0.12f, 0.08f, 1f);
            var eyeSr = eye.AddComponent<SpriteRenderer>();
            eyeSr.sprite = sprite;
            eyeSr.color = new Color(0.1f, 0.2f, 0.45f);
            eyeSr.sortingLayerName = GameLayers.Sorting.Player;
            eyeSr.sortingOrder = 1;
            if (lit != null) eyeSr.sharedMaterial = lit;

            // --- Hurtbox trigger (Docs/03 §1) ---
            var hurtbox = new GameObject("Hurtbox");
            hurtbox.transform.SetParent(root.transform, false);
            hurtbox.layer = GameLayers.PlayerHurtbox;
            var hurtCol = hurtbox.AddComponent<CapsuleCollider2D>();
            hurtCol.direction = CapsuleDirection2D.Vertical;
            hurtCol.size = config.colliderSize;
            hurtCol.offset = config.colliderOffset;
            hurtCol.isTrigger = true;

            // --- Components (order = Awake order) ---
            var input = root.AddComponent<PlayerInput>();
            var motor = root.AddComponent<PlayerMotor>();
            var controller = root.AddComponent<PlayerController>();
            var animator = root.AddComponent<PlayerAnimator>();
            var health = root.AddComponent<Health>();
            var fxPools = root.AddComponent<PlayerFxPools>();
            var soul = root.AddComponent<PlayerSoul>();
            var playerHealth = root.AddComponent<PlayerHealth>();
            root.AddComponent<PlayerCombat>();
            root.AddComponent<PlayerSpells>();
            var dash = root.AddComponent<PlayerDash>();
            var hurtboxComp = hurtbox.AddComponent<PlayerHurtbox>();

            SceneBuildUtil.SetReference(controller, "config", config);
            SceneBuildUtil.SetReference(controller, "input", input);
            SceneBuildUtil.SetReference(controller, "motor", motor);
            SceneBuildUtil.SetReference(controller, "animator", animator);

            SceneBuildUtil.SetReference(animator, "controller", controller);
            SceneBuildUtil.SetReference(animator, "visualRoot", visual.transform);
            SceneBuildUtil.SetReference(animator, "spriteRenderer", sr);
            SceneBuildUtil.SetReference(animator, "spriteAnimator", spriteAnimator);
            if (hasClips) SceneBuildUtil.SetReference(animator, "animationSet", animSet);

            SceneBuildUtil.SetInt(health, "maxHp", config.heartsPhase1);
            SceneBuildUtil.SetFloat(health, "invulnerabilityDuration", config.hurtInvulnerabilityTime);
            SceneBuildUtil.SetFloat(health, "hitFlashDuration", 0.08f);

            if (respawnPoint != null) SceneBuildUtil.SetReference(playerHealth, "respawnPoint", respawnPoint);
            SceneBuildUtil.SetReference(hurtboxComp, "owner", playerHealth);
            SceneBuildUtil.SetReference(dash, "ghostSource", sr);

            return root;
        }
    }
}
