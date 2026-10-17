using System;
using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Docs/03 §5 fireball: 1.2×0.8 u, 14 u/s, 1.2 s, pierces enemies, damage 3 (Spell → no soul), dies on walls.
    /// Moves by transform and uses overlap queries, so it needs no Rigidbody2D and never tunnels at 60 Hz.
    /// </summary>
    public class FireballProjectile : MonoBehaviour, IPoolable
    {
        private readonly List<Collider2D> hitAlready = new List<Collider2D>(8);
        private readonly Collider2D[] overlaps = new Collider2D[8];
        private ContactFilter2D targetFilter;
        private ContactFilter2D wallFilter;

        private int direction = 1;
        private float speed;
        private float lifetime;
        private float elapsed;
        private int damage;
        private bool pierces;
        private Vector2 size;
        private bool active;
        private GameObject owner;
        private Action<FireballProjectile> onFinished;
        private Action<Vector2> onImpact;

        private void Awake()
        {
            targetFilter = new ContactFilter2D();
            targetFilter.SetLayerMask(GameLayers.PlayerAttackTargetsMask);
            targetFilter.useTriggers = true;
            wallFilter = new ContactFilter2D();
            wallFilter.SetLayerMask(GameLayers.ProjectileBlockersMask);
            wallFilter.useTriggers = false;
        }

        public void Init(Action<FireballProjectile> finished, Action<Vector2> impact)
        {
            onFinished = finished;
            onImpact = impact;
        }

        public void Launch(Vector2 position, int facing, PlayerConfig config, GameObject source)
        {
            transform.position = position;
            direction = facing >= 0 ? 1 : -1;
            speed = config.fireballSpeed;
            lifetime = config.fireballLifetime;
            damage = config.fireballDamage;
            pierces = config.fireballPierces;
            size = config.fireballSize;
            owner = source;
            elapsed = 0f;
            hitAlready.Clear();
            transform.localScale = new Vector3(direction, 1f, 1f);
            active = true;
        }

        private void FixedUpdate()
        {
            if (!active) return;
            float dt = Time.fixedDeltaTime;
            elapsed += dt;

            Vector2 pos = (Vector2)transform.position + new Vector2(direction * speed * dt, 0f);
            transform.position = pos;

            // Wall?
            if (Physics2D.OverlapBox(pos, size, 0f, wallFilter, overlaps) > 0)
            {
                Impact(pos);
                return;
            }

            // Enemies.
            int n = Physics2D.OverlapBox(pos, size, 0f, targetFilter, overlaps);
            for (int i = 0; i < n; i++)
            {
                Collider2D col = overlaps[i];
                if (col == null || hitAlready.Contains(col)) continue;
                hitAlready.Add(col);

                IDamageable target = col.GetComponentInParent<IDamageable>();
                if (target == null) continue;

                var info = new DamageInfo(damage, new Vector2(direction, 0f), 1f, owner, DamageType.Spell);
                if (target.TakeDamage(info) && !pierces)
                {
                    Impact(pos);
                    return;
                }
            }

            if (elapsed >= lifetime) Finish();
        }

        private void Impact(Vector2 at)
        {
            onImpact?.Invoke(at);
            Finish();
        }

        private void Finish()
        {
            if (!active) return;
            active = false;
            onFinished?.Invoke(this);
        }

        public void OnSpawned() { active = false; }
        public void OnDespawned() { active = false; hitAlready.Clear(); }

        private void OnDrawGizmos()
        {
            if (!active || !DebugCommands.ShowHitboxGizmos) return;
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
        }
    }
}
