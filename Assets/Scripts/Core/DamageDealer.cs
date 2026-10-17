using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Anything that hurts on touch: spikes, lava, enemy contact bodies, enemy attack hitboxes, projectiles.
    /// Sits next to a trigger collider on the Hazard / EnemyAttack layer. The player's hurtbox asks it for a
    /// <see cref="DamageInfo"/>; knockback direction is computed from positions so the push is always away.
    /// </summary>
    [DisallowMultipleComponent]
    public class DamageDealer : MonoBehaviour
    {
        [SerializeField, Min(0)] private int damage = 1;
        [SerializeField] private DamageType type = DamageType.Contact;
        [SerializeField] private bool active = true;
        [SerializeField] private bool horizontalKnockback = true;
        [SerializeField, Min(0f)] private float knockbackForce = 1f;
        [SerializeField] private GameObject owner;

        public int Damage { get => damage; set => damage = Mathf.Max(0, value); }
        public DamageType Type { get => type; set => type = value; }
        public bool Active { get => active && isActiveAndEnabled; set => active = value; }
        public GameObject Owner => owner != null ? owner : gameObject;

        public void Configure(int newDamage, DamageType newType, GameObject newOwner = null, bool horizontal = true, float force = 1f)
        {
            damage = Mathf.Max(0, newDamage);
            type = newType;
            owner = newOwner;
            horizontalKnockback = horizontal;
            knockbackForce = force;
        }

        /// <summary>Builds the hit for a victim at <paramref name="victimPosition"/>.</summary>
        public DamageInfo BuildDamageInfo(Vector2 hitPoint, Vector2 victimPosition)
        {
            Vector2 origin = transform.position;
            return horizontalKnockback
                ? DamageInfo.Horizontal(damage, origin.x, victimPosition.x, knockbackForce, type, Owner)
                : DamageInfo.Directional(damage, hitPoint, victimPosition, knockbackForce, type, Owner);
        }

        private void OnDrawGizmos()
        {
            if (!DebugCommands.ShowHitboxGizmos) return;
            var col = GetComponent<Collider2D>();
            if (col == null) return;
            Gizmos.color = Active ? new Color(1f, 0.2f, 0.2f, 0.9f) : new Color(1f, 0.2f, 0.2f, 0.3f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
