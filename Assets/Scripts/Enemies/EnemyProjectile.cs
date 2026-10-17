using System;
using Shatranj.Core;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Pooled hostile projectile (Kage Caller's Black Slash now; Thrower knives in Prompt 12). Moves by transform,
    /// checks the PlayerHurtbox layer and solid walls with overlap boxes, hits the player once and breaks
    /// (or breaks on a wall / when its lifetime ends) with an optional break effect.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteAnimator))]
    public class EnemyProjectile : MonoBehaviour, IPoolable
    {
        public struct Spec
        {
            public Vector2 velocity;
            public Vector2 size;
            public float lifetime;
            public int damage;
            public float gravity;
            public string breakFx;
            public GameObject owner;
        }

        private readonly Collider2D[] buffer = new Collider2D[4];
        private ContactFilter2D playerFilter;
        private ContactFilter2D wallFilter;
        private Spec spec;
        private Vector2 velocity;
        private float elapsed;
        private bool active;
        internal Action<EnemyProjectile> Release;

        public SpriteRenderer Renderer { get; private set; }
        public SpriteAnimator Animator { get; private set; }

        private void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
            Animator = GetComponent<SpriteAnimator>();
            playerFilter = new ContactFilter2D();
            playerFilter.SetLayerMask(GameLayers.PlayerHurtboxMask);
            playerFilter.useTriggers = true;
            wallFilter = new ContactFilter2D();
            wallFilter.SetLayerMask(GameLayers.ProjectileBlockersMask);
            wallFilter.useTriggers = false;
        }

        public void Launch(Vector2 position, Spec s)
        {
            spec = s;
            velocity = s.velocity;
            elapsed = 0f;
            transform.position = position;
            transform.localScale = new Vector3(velocity.x < 0f ? -1f : 1f, 1f, 1f);
            active = true;
        }

        private void FixedUpdate()
        {
            if (!active) return;
            float dt = Time.fixedDeltaTime;
            elapsed += dt;
            velocity.y -= spec.gravity * dt;
            Vector2 pos = (Vector2)transform.position + velocity * dt;
            transform.position = pos;
            if (spec.gravity > 0f && velocity.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(velocity.y, Mathf.Abs(velocity.x)) * Mathf.Rad2Deg * Mathf.Sign(velocity.x));

            int n = Physics2D.OverlapBox(pos, spec.size, 0f, playerFilter, buffer);
            for (int i = 0; i < n; i++)
            {
                PlayerHealth ph = buffer[i].GetComponentInParent<PlayerHealth>();
                if (ph == null) continue;
                ph.ReceiveHit(new DamageInfo(spec.damage, new Vector2(Mathf.Sign(velocity.x), 0f), 1f, spec.owner, DamageType.Projectile));
                Break(pos);
                return;
            }

            if (Physics2D.OverlapBox(pos, spec.size * 0.6f, 0f, wallFilter, buffer) > 0 || elapsed >= spec.lifetime) Break(pos);
        }

        private void Break(Vector2 at)
        {
            if (!active) return;
            active = false;
            if (!string.IsNullOrEmpty(spec.breakFx)) FxPool.Play(spec.breakFx, at, velocity.x < 0f ? -1 : 1);
            Release?.Invoke(this);
        }

        public void OnSpawned() { active = false; }

        public void OnDespawned()
        {
            active = false;
            transform.rotation = Quaternion.identity;
        }

        private void OnDrawGizmos()
        {
            if (!active || !DebugCommands.ShowHitboxGizmos) return;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
            Gizmos.DrawWireCube(transform.position, spec.size);
        }
    }
}
