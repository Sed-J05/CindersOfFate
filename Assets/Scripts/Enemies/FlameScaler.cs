using Shatranj.Core;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Docs/07 §4 Phase 2 enemy growth: target scale = FlameController.EnemyScale (lerp 1 → 1.4 with easeInQuad of the
    /// monotonic or peak closeness, per FlameConfig), approached smoothly (lerp at scaleLerpSpeed ≈ 2 s⁻¹) and applied to
    /// sprite AND colliders via the enemy root (pivot = feet for ground enemies, centre for flyers). Before growing it
    /// checks the scaled body against walls and nudges the enemy sideways if needed; if it can't fit, it waits.
    /// Stats never change. Dying restores the bench flame progress (FlameController), so sizes follow.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyBase))]
    public class FlameScaler : MonoBehaviour
    {
        [SerializeField] private float nudgeStep = 0.05f;
        [SerializeField] private float maxNudge = 0.35f;
        [SerializeField] private float defaultLerpSpeed = 2f;

        private EnemyBase enemy;
        private Rigidbody2D rb;
        private ContactFilter2D wallFilter;
        private readonly Collider2D[] overlaps = new Collider2D[4];
        private float current = 1f;

        public float Current => current;

        private void Awake()
        {
            enemy = GetComponent<EnemyBase>();
            rb = GetComponent<Rigidbody2D>();
            wallFilter = new ContactFilter2D();
            wallFilter.SetLayerMask(GameLayers.WallMask);
            wallFilter.useTriggers = false;
        }

        private void Start()
        {
            // Spawned mid-phase: start at the current size instead of visibly growing from 1.
            float target = TargetScale();
            if (Fits(target, rb.position)) current = target;
            enemy.SetFlameScale(current);
        }

        private float TargetScale()
        {
            if (enemy.Config == null || !enemy.Config.scalesWithFlame) return 1f;
            FlameController flame = FlameController.Instance;
            return flame != null ? flame.EnemyScale : 1f;
        }

        private void Update()
        {
            if (enemy.IsDead || enemy.Config == null || !enemy.Config.scalesWithFlame || !enemy.RoomActive) return;
            float target = TargetScale();
            if (Mathf.Abs(target - current) < 0.0005f) return;

            FlameController flame = FlameController.Instance;
            float speed = flame != null && flame.Config != null ? flame.Config.scaleLerpSpeed : defaultLerpSpeed;
            float next = Smooth(current, target, speed, Time.deltaTime);

            if (next > current && !Fits(next, rb.position))
            {
                if (!TryNudge(next)) return;   // no room yet — keep the current size
            }
            current = next;
            enemy.SetFlameScale(current);
        }

        /// <summary>
        /// One frame of frame-rate-independent smoothing toward <paramref name="target"/>:
        /// lerp(current, target, 1 - e^(-speed*dt)), snapping when within 0.001.
        /// </summary>
        public static float Smooth(float current, float target, float speed, float dt)
        {
            float next = Mathf.Lerp(current, target, 1f - Mathf.Exp(-Mathf.Max(0f, speed) * Mathf.Max(0f, dt)));
            return Mathf.Abs(target - next) < 0.001f ? target : next;
        }

        /// <summary>Would the body collider at <paramref name="scale"/> overlap solid ground at <paramref name="root"/>?</summary>
        private bool Fits(float scale, Vector2 root)
        {
            Data.EnemyConfig cfg = enemy.Config;
            bool flyer = cfg.locomotion == Data.EnemyLocomotion.Flyer;
            Vector2 size = cfg.colliderShape == Data.EnemyColliderShape.Circle
                ? new Vector2(cfg.colliderRadius * 2f, cfg.colliderRadius * 2f)
                : cfg.colliderSize;
            size *= scale;
            Vector2 center = flyer ? root : root + new Vector2(0f, size.y * 0.5f + 0.03f);   // feet rest on the floor
            Vector2 test = new Vector2(Mathf.Max(0.02f, size.x - 0.04f), Mathf.Max(0.02f, size.y - 0.06f));
            return Physics2D.OverlapBox(center, test, 0f, wallFilter, overlaps) == 0;
        }

        private bool TryNudge(float scale)
        {
            for (float d = nudgeStep; d <= maxNudge + 1e-4f; d += nudgeStep)
            {
                for (int k = 0; k < 2; k++)
                {
                    float sign = k == 0 ? 1f : -1f;
                    Vector2 p = rb.position + new Vector2(sign * d, 0f);
                    if (!Fits(scale, p)) continue;
                    rb.position = p;
                    transform.position = p;
                    return true;
                }
            }
            return false;
        }
    }
}
