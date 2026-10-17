using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Sandbox training dummy: takes hits via its EnemyHurtbox child, flashes white (Docs/04 global rules),
    /// wobbles in the knockback direction, "dies" at 0 HP and revives after a delay so the arena keeps working.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public class DamageableDummy : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private Collider2D hurtbox;
        [SerializeField] private float respawnDelay = 2f;

        private Health health;
        private Color baseColor = Color.white;
        private Vector3 baseScale = Vector3.one;
        private float flashTimer;
        private float respawnTimer;
        private bool dead;

        public Health Health => health;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (body == null) body = GetComponentInChildren<SpriteRenderer>();
            if (body != null) { baseColor = body.color; baseScale = body.transform.localScale; }
        }

        private void OnEnable()
        {
            health.OnHitFlash += HandleFlash;
            health.OnDamaged += HandleDamaged;
            health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            health.OnHitFlash -= HandleFlash;
            health.OnDamaged -= HandleDamaged;
            health.OnDied -= HandleDied;
        }

        private void Update()
        {
            // Flash runs on unscaled time so it is visible during hit-stop.
            if (flashTimer > 0f)
            {
                flashTimer -= Time.unscaledDeltaTime;
                if (body != null) body.color = flashTimer > 0f ? Color.white : (dead ? DeadColor : baseColor);
            }

            if (dead)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f) Revive();
            }
        }

        private static readonly Color DeadColor = new Color(0.25f, 0.25f, 0.28f, 1f);

        private void HandleFlash(float duration) => flashTimer = duration;

        private void HandleDamaged(DamageInfo info)
        {
            if (body == null) return;
            Tween.KillTweensOf(body.transform);
            float lean = Mathf.Clamp(info.knockbackDir.x, -1f, 1f) * 0.12f;
            body.transform.localScale = new Vector3(baseScale.x * 1.15f, baseScale.y * 0.88f, 1f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, -lean * 90f);
            Tween.Scale(body.transform, baseScale, 0.18f, Ease.OutBack);
            Tween.Rotate(body.transform, 0f, 0.18f, Ease.OutQuad);
        }

        private void HandleDied(DamageInfo info)
        {
            dead = true;
            respawnTimer = respawnDelay;
            if (body != null) body.color = DeadColor;
            if (hurtbox != null) hurtbox.enabled = false;
        }

        private void Revive()
        {
            dead = false;
            health.ResetToFull();
            if (body != null)
            {
                body.color = baseColor;
                body.transform.localScale = baseScale * 0.6f;
                Tween.Scale(body.transform, baseScale, 0.25f, Ease.OutBack);
            }
            if (hurtbox != null) hurtbox.enabled = true;
        }

        private void OnDrawGizmos()
        {
            if (!DebugCommands.ShowHitboxGizmos || hurtbox == null) return;
            Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.8f);
            Gizmos.DrawWireCube(hurtbox.bounds.center, hurtbox.bounds.size);
        }
    }
}
