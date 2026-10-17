using System;
using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Runtime pools of <see cref="EnemyProjectile"/>, one per animation set ("BlackSlash_Fly", "Knife_Fly"). Templates
    /// are built in code, so no projectile prefabs are needed and new art just replaces the sprite frames.
    /// </summary>
    public static class ProjectilePool
    {
        private static readonly Dictionary<string, ObjectPool<EnemyProjectile>> pools = new Dictionary<string, ObjectPool<EnemyProjectile>>();
        private static readonly Dictionary<string, Action<EnemyProjectile>> releases = new Dictionary<string, Action<EnemyProjectile>>();
        private static Transform root;

        // QA (Prompt 18): knives / slashes in flight when a scene unloads are recalled instead of flying through the next scene.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void HookSceneLoads()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= ReleaseOnLoad;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += ReleaseOnLoad;
        }

        private static void ReleaseOnLoad(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
        {
            if (root == null) return;
            foreach (ObjectPool<EnemyProjectile> p in pools.Values) p.ReleaseAll();
        }

        public static EnemyProjectile Fire(string setName, Vector2 position, EnemyProjectile.Spec spec, string clip = FxPool.DefaultClip)
        {
            if (!Application.isPlaying) return null;
            if (root == null)
            {
                root = new GameObject("[ProjectilePool]").transform;
                UnityEngine.Object.DontDestroyOnLoad(root.gameObject);
                pools.Clear();
                releases.Clear();
            }

            if (!pools.TryGetValue(setName, out ObjectPool<EnemyProjectile> pool))
            {
                var template = new GameObject("Projectile_" + setName, typeof(SpriteRenderer), typeof(SpriteAnimator), typeof(EnemyProjectile));
                template.transform.SetParent(root, false);
                template.SetActive(false);
                var sr = template.GetComponent<SpriteRenderer>();
                sr.sortingLayerName = GameLayers.Sorting.FX;
                SpriteAnimationSet set = SpriteAnimationSet.Load(setName);
                var anim = template.GetComponent<SpriteAnimator>();
                anim.Set = set;
                if (set == null)
                {
                    // No art built yet: a dark placeholder disc so the attack is still visible and dodgeable.
                    sr.sprite = PlaceholderSprites.Circle;
                    sr.color = new Color(0.1f, 0.02f, 0.03f, 1f);
                }
                pool = new ObjectPool<EnemyProjectile>(template.GetComponent<EnemyProjectile>(), root, 4);
                pools[setName] = pool;
                ObjectPool<EnemyProjectile> captured = pool;
                releases[setName] = p => captured.Release(p);
            }

            EnemyProjectile proj = pool.Get(position);
            proj.Release = releases[setName];
            if (proj.Animator.Set != null) proj.Animator.Play(clip, restart: true);
            proj.Launch(position, spec);
            // The placeholder disc is 1×1 u, so scale it to the hit size (real art is authored at its size).
            if (proj.Animator.Set == null)
                proj.transform.localScale = new Vector3(spec.size.x * (spec.velocity.x < 0f ? -1f : 1f), spec.size.y, 1f);
            return proj;
        }
    }
}
