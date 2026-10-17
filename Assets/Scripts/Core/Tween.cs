using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.Core
{
    public enum Ease
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        OutBack,
        OutElastic,
    }

    /// <summary>Easing curves, t in [0,1].</summary>
    public static class Easing
    {
        public static float Evaluate(Ease ease, float t)
        {
            t = Mathf.Clamp01(t);
            switch (ease)
            {
                case Ease.InQuad:     return t * t;
                case Ease.OutQuad:    return 1f - (1f - t) * (1f - t);
                case Ease.InOutQuad:  return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                case Ease.InCubic:    return t * t * t;
                case Ease.OutCubic:   return 1f - Mathf.Pow(1f - t, 3f);
                case Ease.InOutCubic: return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
                case Ease.OutBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                }
                case Ease.OutElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    const float c4 = (2f * Mathf.PI) / 3f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
                }
                default: return t;
            }
        }
    }

    /// <summary>Handle to a running tween. Chain <see cref="OnComplete"/>; call <see cref="Kill"/> to stop early.</summary>
    public class TweenHandle
    {
        internal float duration;
        internal float elapsed;
        internal bool unscaled;
        internal Ease ease;
        internal Action<float> apply;      // receives eased 0..1
        internal Action onComplete;
        internal object target;            // for KillTweensOf
        internal bool started;
        internal Action onStart;           // captures "from" lazily so chained tweens work

        public bool IsDone { get; internal set; }
        public bool IsKilled { get; private set; }

        public TweenHandle OnComplete(Action callback) { onComplete += callback; return this; }

        /// <summary>Stops the tween where it is. With <paramref name="complete"/> it jumps to the end first.</summary>
        public void Kill(bool complete = false)
        {
            if (IsDone || IsKilled) return;
            if (complete)
            {
                if (!started) { started = true; onStart?.Invoke(); }
                apply?.Invoke(1f);
            }
            IsKilled = true;
            IsDone = true;
        }
    }

    /// <summary>
    /// Tiny tween utility (Docs/02 §4): move, scale, colour, alpha, float, delay, with easing.
    /// A hidden runner ticks all tweens once per frame; no coroutines, no per-frame allocation.
    /// </summary>
    public static class Tween
    {
        private static TweenRunner runner;

        private static TweenRunner Runner
        {
            get
            {
                if (runner != null) return runner;
                var go = new GameObject("[TweenRunner]");
                UnityEngine.Object.DontDestroyOnLoad(go);
                runner = go.AddComponent<TweenRunner>();
                return runner;
            }
        }

        // ------------------------------------------------------------------
        // Generic
        // ------------------------------------------------------------------

        /// <summary>Drives <paramref name="onUpdate"/> with a value from <paramref name="from"/> to <paramref name="to"/>.</summary>
        public static TweenHandle Float(float from, float to, float duration, Action<float> onUpdate, Ease ease = Ease.OutQuad, bool unscaled = false, object target = null)
        {
            return Start(duration, ease, unscaled, target, null, k => onUpdate(Mathf.LerpUnclamped(from, to, k)));
        }

        /// <summary>Calls <paramref name="action"/> after <paramref name="seconds"/>.</summary>
        public static TweenHandle Delay(float seconds, Action action, bool unscaled = false, object target = null)
        {
            return Start(seconds, Ease.Linear, unscaled, target, null, null).OnComplete(action);
        }

        // ------------------------------------------------------------------
        // Transform
        // ------------------------------------------------------------------

        public static TweenHandle Move(Transform t, Vector3 to, float duration, Ease ease = Ease.OutQuad, bool unscaled = false)
        {
            Vector3 from = default;
            return Start(duration, ease, unscaled, t,
                () => from = t.position,
                k => { if (t != null) t.position = Vector3.LerpUnclamped(from, to, k); });
        }

        public static TweenHandle MoveLocal(Transform t, Vector3 to, float duration, Ease ease = Ease.OutQuad, bool unscaled = false)
        {
            Vector3 from = default;
            return Start(duration, ease, unscaled, t,
                () => from = t.localPosition,
                k => { if (t != null) t.localPosition = Vector3.LerpUnclamped(from, to, k); });
        }

        public static TweenHandle Scale(Transform t, Vector3 to, float duration, Ease ease = Ease.OutQuad, bool unscaled = false)
        {
            Vector3 from = default;
            return Start(duration, ease, unscaled, t,
                () => from = t.localScale,
                k => { if (t != null) t.localScale = Vector3.LerpUnclamped(from, to, k); });
        }

        public static TweenHandle Scale(Transform t, float uniform, float duration, Ease ease = Ease.OutQuad, bool unscaled = false)
        {
            return Scale(t, new Vector3(uniform, uniform, uniform), duration, ease, unscaled);
        }

        public static TweenHandle Rotate(Transform t, float toZDegrees, float duration, Ease ease = Ease.OutQuad, bool unscaled = false)
        {
            float from = 0f;
            return Start(duration, ease, unscaled, t,
                () => from = t.localEulerAngles.z,
                k => { if (t != null) t.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(from, toZDegrees, k)); });
        }

        // ------------------------------------------------------------------
        // Colour / alpha
        // ------------------------------------------------------------------

        public static TweenHandle Color(SpriteRenderer r, Color to, float duration, Ease ease = Ease.Linear, bool unscaled = false)
        {
            Color from = default;
            return Start(duration, ease, unscaled, r,
                () => from = r.color,
                k => { if (r != null) r.color = UnityEngine.Color.LerpUnclamped(from, to, k); });
        }

        public static TweenHandle Alpha(SpriteRenderer r, float to, float duration, Ease ease = Ease.Linear, bool unscaled = false)
        {
            float from = 0f;
            return Start(duration, ease, unscaled, r,
                () => from = r.color.a,
                k => { if (r == null) return; Color c = r.color; c.a = Mathf.LerpUnclamped(from, to, k); r.color = c; });
        }

        public static TweenHandle Color(Graphic g, Color to, float duration, Ease ease = Ease.Linear, bool unscaled = true)
        {
            Color from = default;
            return Start(duration, ease, unscaled, g,
                () => from = g.color,
                k => { if (g != null) g.color = UnityEngine.Color.LerpUnclamped(from, to, k); });
        }

        public static TweenHandle Alpha(Graphic g, float to, float duration, Ease ease = Ease.Linear, bool unscaled = true)
        {
            float from = 0f;
            return Start(duration, ease, unscaled, g,
                () => from = g.color.a,
                k => { if (g == null) return; Color c = g.color; c.a = Mathf.LerpUnclamped(from, to, k); g.color = c; });
        }

        public static TweenHandle Alpha(CanvasGroup cg, float to, float duration, Ease ease = Ease.Linear, bool unscaled = true)
        {
            float from = 0f;
            return Start(duration, ease, unscaled, cg,
                () => from = cg.alpha,
                k => { if (cg != null) cg.alpha = Mathf.LerpUnclamped(from, to, k); });
        }

        // ------------------------------------------------------------------
        // Management
        // ------------------------------------------------------------------

        /// <summary>Kills every tween whose target is <paramref name="target"/> (a Transform, renderer, etc.).</summary>
        public static void KillTweensOf(object target, bool complete = false)
        {
            if (runner == null || target == null) return;
            runner.KillTweensOf(target, complete);
        }

        public static void KillAll()
        {
            if (runner != null) runner.KillAll();
        }

        public static int ActiveCount => runner != null ? runner.Count : 0;

        private static TweenHandle Start(float duration, Ease ease, bool unscaled, object target, Action onStart, Action<float> apply)
        {
            var h = new TweenHandle
            {
                duration = Mathf.Max(0f, duration),
                ease = ease,
                unscaled = unscaled,
                target = target,
                onStart = onStart,
                apply = apply,
            };
            if (Application.isPlaying) Runner.Add(h);
            else
            {
                // Edit mode / tests: complete instantly so callers still get their end state.
                h.started = true;
                onStart?.Invoke();
                apply?.Invoke(1f);
                h.IsDone = true;
                h.onComplete?.Invoke();
            }
            return h;
        }

        private class TweenRunner : MonoBehaviour
        {
            private readonly List<TweenHandle> tweens = new List<TweenHandle>(64);

            public int Count => tweens.Count;

            public void Add(TweenHandle h) => tweens.Add(h);

            private void Update()
            {
                float dt = Time.deltaTime;
                float udt = Time.unscaledDeltaTime;

                for (int i = tweens.Count - 1; i >= 0; i--)
                {
                    TweenHandle h = tweens[i];
                    if (h.IsDone) { tweens.RemoveAt(i); continue; }

                    if (!h.started)
                    {
                        h.started = true;
                        h.onStart?.Invoke();
                    }

                    h.elapsed += h.unscaled ? udt : dt;
                    float k = h.duration <= 0f ? 1f : Mathf.Clamp01(h.elapsed / h.duration);
                    h.apply?.Invoke(Easing.Evaluate(h.ease, k));

                    if (k >= 1f)
                    {
                        h.IsDone = true;
                        tweens.RemoveAt(i);
                        h.onComplete?.Invoke();
                    }
                }
            }

            public void KillTweensOf(object target, bool complete)
            {
                for (int i = tweens.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(tweens[i].target, target))
                    {
                        tweens[i].Kill(complete);
                        tweens.RemoveAt(i);
                    }
                }
            }

            public void KillAll()
            {
                for (int i = 0; i < tweens.Count; i++) tweens[i].Kill();
                tweens.Clear();
            }

            private void OnDestroy()
            {
                if (runner == this) runner = null;
            }
        }
    }
}
