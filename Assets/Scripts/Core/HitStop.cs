using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Brief timeScale freeze on hits (Docs/02 §4: 0.05–0.12 s). Timed with unscaled time.
    /// Overlapping requests extend to the longer remaining duration. Pausing cancels it and the
    /// restore step never un-pauses the game.
    /// </summary>
    public static class HitStop
    {
        private static HitStopRunner runner;

        public static bool IsActive => runner != null && runner.Active;

        /// <summary>Freeze for <paramref name="duration"/> seconds of real time.</summary>
        public static void Do(float duration)
        {
            if (duration <= 0f) return;
            if (!Application.isPlaying) return;
            if (GameManager.HasInstance && GameManager.Instance.IsPaused) return;
            Runner.Request(duration);
        }

        /// <summary>Ends an active hit-stop immediately (used before pausing).</summary>
        public static void Cancel()
        {
            if (runner != null) runner.Finish(restore: !(GameManager.HasInstance && GameManager.Instance.IsPaused));
        }

        private static HitStopRunner Runner
        {
            get
            {
                if (runner != null) return runner;
                var go = new GameObject("[HitStop]");
                Object.DontDestroyOnLoad(go);
                runner = go.AddComponent<HitStopRunner>();
                return runner;
            }
        }

        private class HitStopRunner : MonoBehaviour
        {
            private float remaining;
            private float restoreScale = 1f;

            public bool Active { get; private set; }

            public void Request(float duration)
            {
                if (!Active)
                {
                    Active = true;
                    restoreScale = Time.timeScale > 0f ? Time.timeScale : 1f;
                    remaining = duration;
                    Time.timeScale = 0f;
                }
                else
                {
                    remaining = Mathf.Max(remaining, duration);
                }
            }

            public void Finish(bool restore)
            {
                if (!Active) return;
                Active = false;
                remaining = 0f;
                if (restore) Time.timeScale = restoreScale;
            }

            private void Update()
            {
                if (!Active) return;
                remaining -= Time.unscaledDeltaTime;
                if (remaining > 0f) return;

                bool paused = GameManager.HasInstance && GameManager.Instance.IsPaused;
                Finish(restore: !paused);
            }

            private void OnDisable()
            {
                // Never leave the game frozen if this object is destroyed mid-freeze.
                bool paused = GameManager.HasInstance && GameManager.Instance.IsPaused;
                Finish(restore: !paused);
            }

            private void OnDestroy()
            {
                if (runner == this) runner = null;
            }
        }
    }
}
