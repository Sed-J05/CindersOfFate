using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// A looping sprite animation that lives for a fixed time, then removes itself (vine ground glow, '!' summon
    /// markers). <see cref="FxPool"/> only returns one-shot clips, so looping warnings use this instead.
    /// </summary>
    public static class TimedFx
    {
        public static GameObject Spawn(string setName, Vector2 position, float duration, float scale = 1f,
                                       string sortingLayer = null, int sortingOrder = 0, string clip = FxPool.DefaultClip)
        {
            if (!Application.isPlaying || duration <= 0f) return null;
            SpriteAnimationSet set = SpriteAnimationSet.Load(setName);
            if (set == null || !set.Has(clip)) return null;

            var go = new GameObject(setName + "_Timed");
            go.layer = GameLayers.Ignore;
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = string.IsNullOrEmpty(sortingLayer) ? GameLayers.Sorting.FX : sortingLayer;
            sr.sortingOrder = sortingOrder;
            var anim = go.AddComponent<SpriteAnimator>();
            anim.Set = set;
            anim.Play(clip, restart: true);
            Object.Destroy(go, duration);
            return go;
        }
    }
}
