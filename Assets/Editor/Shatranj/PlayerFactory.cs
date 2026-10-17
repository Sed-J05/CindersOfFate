using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Player;
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
        public static GameObject Build(PlayerConfig config, Vector2 position)
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

            // --- Visual child: coloured box, feet on y = 0, flipped by localScale.x ---
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            visual.transform.localScale = new Vector3(1.1f, 1.7f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = PlayerAnimator.TintFor(PlayerState.Idle);
            sr.sortingLayerName = GameLayers.Sorting.Player;
            if (lit != null) sr.sharedMaterial = lit;

            // Simple "eye" so facing is readable on the placeholder.
            var eye = new GameObject("Eye");
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

            // --- Components ---
            var input = root.AddComponent<PlayerInput>();
            var motor = root.AddComponent<PlayerMotor>();
            var controller = root.AddComponent<PlayerController>();
            var animator = root.AddComponent<PlayerAnimator>();

            SceneBuildUtil.SetReference(controller, "config", config);
            SceneBuildUtil.SetReference(controller, "input", input);
            SceneBuildUtil.SetReference(controller, "motor", motor);
            SceneBuildUtil.SetReference(controller, "animator", animator);

            SceneBuildUtil.SetReference(animator, "controller", controller);
            SceneBuildUtil.SetReference(animator, "visualRoot", visual.transform);
            SceneBuildUtil.SetReference(animator, "spriteRenderer", sr);

            return root;
        }
    }
}
