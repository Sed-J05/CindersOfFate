using System;
using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Pooled one-shot sprite effect: scale from→to and optional fade over a lifetime, then hands itself back.
    /// Used for slashes, hit sparks, dash afterimages and impact puffs.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFx : MonoBehaviour, IPoolable
    {
        private SpriteRenderer sr;
        private float lifetime;
        private float elapsed;
        private Color startColor;
        private Vector3 scaleFrom;
        private Vector3 scaleTo;
        private bool fadeOut;
        private bool unscaled;
        private bool playing;
        private Action<SpriteFx> onFinished;

        public SpriteRenderer Renderer => sr != null ? sr : (sr = GetComponent<SpriteRenderer>());

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        /// <summary>Called by the pool owner so the effect can release itself.</summary>
        public void Init(Action<SpriteFx> finished) => onFinished = finished;

        public void Play(Sprite sprite, Vector3 position, Vector3 fromScale, Vector3 toScale, Color color, float life, bool fade, bool useUnscaledTime = false, float rotationZ = 0f)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sprite != null) sr.sprite = sprite;
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            transform.localScale = fromScale;
            sr.color = color;
            startColor = color;
            scaleFrom = fromScale;
            scaleTo = toScale;
            lifetime = Mathf.Max(0.0001f, life);
            fadeOut = fade;
            unscaled = useUnscaledTime;
            elapsed = 0f;
            playing = true;
        }

        private void Update()
        {
            if (!playing) return;
            elapsed += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / lifetime);

            transform.localScale = Vector3.LerpUnclamped(scaleFrom, scaleTo, k);
            if (fadeOut)
            {
                Color c = startColor;
                c.a = startColor.a * (1f - k);
                sr.color = c;
            }

            if (k >= 1f)
            {
                playing = false;
                onFinished?.Invoke(this);
            }
        }

        public void OnSpawned() { playing = false; }
        public void OnDespawned() { playing = false; }
    }
}
