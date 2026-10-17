using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Sits on the "Hurtbox" child (PlayerHurtbox layer, trigger capsule). Forwards touches with any
    /// <see cref="DamageDealer"/> (hazards, enemy attacks) to <see cref="PlayerHealth"/>. Stay is used too, so
    /// standing in spikes or lava re-applies once i-frames expire.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerHurtbox : MonoBehaviour
    {
        [SerializeField] private PlayerHealth owner;

        private Collider2D lastCollider;
        private DamageDealer lastDealer;

        private void Awake()
        {
            if (owner == null) owner = GetComponentInParent<PlayerHealth>();
            gameObject.layer = GameLayers.PlayerHurtbox;
        }

        private void OnTriggerEnter2D(Collider2D other) => Handle(other);
        private void OnTriggerStay2D(Collider2D other) => Handle(other);

        private void Handle(Collider2D other)
        {
            if (owner == null) return;

            DamageDealer dealer;
            if (other == lastCollider) dealer = lastDealer;
            else
            {
                dealer = other.GetComponentInParent<DamageDealer>();
                lastCollider = other;
                lastDealer = dealer;
            }
            if (dealer == null || !dealer.Active) return;

            Vector2 point = other.bounds.ClosestPoint(transform.position);
            owner.ReceiveHit(dealer, point);
        }

        private void OnDrawGizmos()
        {
            if (!DebugCommands.ShowHitboxGizmos) return;
            var col = GetComponent<Collider2D>();
            if (col == null) return;
            Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.8f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
