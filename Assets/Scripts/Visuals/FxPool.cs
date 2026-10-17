using System;
using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Pooled one-shot sprite effects by animation-set name (Docs/06 §4: Death_Smoke, Dust_Puff, Hit_Spark…).
    /// Sets are loaded once from Resources/Animations. Missing sets are a silent no-op so gameplay never breaks.
    /// </summary>
    public static class FxPool
    {
        public const string DefaultClip = "Default";

        private static readonly Dictionary<string, SpriteAnimationSet> sets = new Dictionary<string, SpriteAnimationSet>();
        private static ObjectPool<PooledFx> pool;
        private static Transform root;
        private static Action<PooledFx> release;

        // QA (Prompt 18): effects still playing when a scene unloads return to the pool instead of drifting into the next scene.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void HookSceneLoads()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= ReleaseOnLoad;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += ReleaseOnLoad;
        }

        private static void ReleaseOnLoad(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
        {
            if (pool != null && root != null) pool.ReleaseAll();
        }

        private static void Ensure()
        {
            if (pool != null && root != null) return;
            root = new GameObject("[FxPool]").transform;
            UnityEngine.Object.DontDestroyOnLoad(root.gameObject);
            var template = new GameObject("FxTemplate", typeof(SpriteRenderer), typeof(SpriteAnimator), typeof(PooledFx));
            template.transform.SetParent(root, false);
            template.SetActive(false);
            pool = new ObjectPool<PooledFx>(template.GetComponent<PooledFx>(), root, 6);
            release = fx => pool.Release(fx);
        }

        private static SpriteAnimationSet GetSet(string setName)
        {
            if (!sets.TryGetValue(setName, out SpriteAnimationSet set))
            {
                set = SpriteAnimationSet.Load(setName);
                sets[setName] = set;
            }
            return set;
        }

        /// <summary>Plays <paramref name="clip"/> of set <paramref name="setName"/> once at <paramref name="position"/>. Returns null when unavailable.</summary>
        public static PooledFx Play(string setName, Vector2 position, int facing = 1, float scale = 1f, string clip = DefaultClip, string sortingLayer = null, int sortingOrder = 0)
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(setName)) return null;
            SpriteAnimationSet set = GetSet(setName);
            if (set == null || !set.Has(clip)) return null;

            Ensure();
            PooledFx fx = pool.Get(position);
            fx.Release = release;
            fx.transform.localScale = new Vector3(facing >= 0 ? scale : -scale, scale, 1f);
            fx.Renderer.sortingLayerName = string.IsNullOrEmpty(sortingLayer) ? GameLayers.Sorting.FX : sortingLayer;
            fx.Renderer.sortingOrder = sortingOrder;
            fx.Renderer.color = Color.white;
            fx.Animator.Set = set;
            fx.Animator.Play(clip, restart: true);
            return fx;
        }
    }
}
