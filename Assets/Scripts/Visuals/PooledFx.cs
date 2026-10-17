using System;
using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>One pooled sprite-animation effect (death smoke, dust…). Returns itself to <see cref="FxPool"/> when its clip ends.</summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteAnimator))]
    public class PooledFx : MonoBehaviour, IPoolable
    {
        public SpriteRenderer Renderer { get; private set; }
        public SpriteAnimator Animator { get; private set; }
        internal Action<PooledFx> Release;

        private void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
            Animator = GetComponent<SpriteAnimator>();
            Animator.OnClipFinished += HandleFinished;
        }

        private void OnDestroy()
        {
            if (Animator != null) Animator.OnClipFinished -= HandleFinished;
        }

        private void HandleFinished(string clip) => Release?.Invoke(this);

        /// <summary>Returns a looping effect (shield arc) to the pool; one-shot effects return by themselves.</summary>
        public void ReleaseNow() => Release?.Invoke(this);

        public void OnSpawned() { }

        public void OnDespawned()
        {
            if (Animator != null) Animator.Stop();
        }
    }
}
