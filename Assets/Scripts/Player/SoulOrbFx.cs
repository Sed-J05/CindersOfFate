using System;
using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Docs/03 §5: a dark shadow-orb flies from the hit enemy to the player's chest over 0.25 s (eased);
    /// soul is added when it arrives. Pooled; tracks the moving player each frame.
    /// </summary>
    public class SoulOrbFx : MonoBehaviour, IPoolable
    {
        private Vector2 from;
        private Transform target;
        private Vector2 offset;
        private float travelTime;
        private float elapsed;
        private bool flying;
        private Action onArrive;
        private Action<SoulOrbFx> onFinished;

        public void Init(Action<SoulOrbFx> finished) => onFinished = finished;

        public void Play(Vector2 start, Transform chaseTarget, Vector2 targetOffset, float time, Action arrived)
        {
            from = start;
            target = chaseTarget;
            offset = targetOffset;
            travelTime = Mathf.Max(0.01f, time);
            onArrive = arrived;
            elapsed = 0f;
            flying = true;
            transform.position = start;
            transform.localScale = Vector3.one * 0.6f;
        }

        private void Update()
        {
            if (!flying) return;
            if (target == null) { Finish(false); return; }

            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / travelTime);
            float e = Easing.Evaluate(Ease.InQuad, k);
            Vector2 dest = (Vector2)target.position + offset;
            transform.position = Vector2.LerpUnclamped(from, dest, e);
            transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, k);

            if (k >= 1f) Finish(true);
        }

        private void Finish(bool arrived)
        {
            flying = false;
            if (arrived) onArrive?.Invoke();
            onArrive = null;
            onFinished?.Invoke(this);
        }

        public void OnSpawned() { flying = false; }
        public void OnDespawned() { flying = false; target = null; onArrive = null; }
    }
}
