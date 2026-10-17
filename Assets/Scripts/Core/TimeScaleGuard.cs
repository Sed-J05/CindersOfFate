using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shatranj.Core
{
    /// <summary>
    /// QA safety net (Prompt 18): <c>Time.timeScale</c> can never stay below 1 by accident.
    /// Deliberate slow-mo (boss deaths) registers with <see cref="BeginSlowMo"/> / <see cref="EndSlowMo"/>; pause and
    /// hit-stop manage themselves. Every scene load cancels hit-stop, forgets slow-mo owners and restores 1 (unless paused).
    /// While Playing or in a Cutscene, a timeScale below 1 with no hit-stop and no slow-mo owner for
    /// <see cref="StuckGrace"/> real seconds is reset to 1 (logged), e.g. a boss destroyed mid slow-mo.
    /// </summary>
    public static class TimeScaleGuard
    {
        public const float StuckGrace = 0.5f;

        private static readonly HashSet<Object> slowMoOwners = new HashSet<Object>();
        private static float slowMoScale = 1f;

        /// <summary>The speed the game should run at when not paused or hit-stopped: the slow-mo scale, else 1.</summary>
        public static float ResumeScale => SlowMoActive ? slowMoScale : 1f;
        private static GuardRunner runner;

        public static bool SlowMoActive
        {
            get
            {
                slowMoOwners.RemoveWhere(o => o == null);   // destroyed owners never hold the clock
                return slowMoOwners.Count > 0;
            }
        }

        public static void BeginSlowMo(Object owner, float scale)
        {
            if (owner == null) return;
            slowMoOwners.Add(owner);
            slowMoScale = Mathf.Clamp(scale, 0.01f, 1f);
            if (!(GameManager.HasInstance && GameManager.Instance.IsPaused)) Time.timeScale = slowMoScale;
            EnsureRunner();
        }

        public static void EndSlowMo(Object owner)
        {
            if (owner == null || !slowMoOwners.Remove(owner)) return;
            if (!SlowMoActive && !HitStop.IsActive && !(GameManager.HasInstance && GameManager.Instance.IsPaused)) Time.timeScale = 1f;
        }

        /// <summary>Back to normal speed unless the game is paused (scene loads, menus).</summary>
        public static void Restore()
        {
            slowMoOwners.Clear();
            HitStop.Cancel();
            if (!(GameManager.HasInstance && GameManager.Instance.IsPaused)) Time.timeScale = 1f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            slowMoOwners.Clear();
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRunner()
        {
            if (runner != null || !Application.isPlaying) return;
            var go = new GameObject("[TimeScaleGuard]");
            Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<GuardRunner>();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => Restore();

        private class GuardRunner : MonoBehaviour
        {
            private float stuck;

            private void Update()
            {
                bool watching = GameManager.HasInstance
                                && (GameManager.Instance.State == GameState.Playing || GameManager.Instance.State == GameState.Cutscene);
                if (watching && Time.timeScale < 1f && !HitStop.IsActive && !SlowMoActive)
                {
                    stuck += Time.unscaledDeltaTime;
                    if (stuck >= StuckGrace)
                    {
                        Debug.LogWarning($"TimeScaleGuard: timeScale was stuck at {Time.timeScale:0.00} with no hit-stop or slow-mo — reset to 1.");
                        Time.timeScale = 1f;
                        stuck = 0f;
                    }
                }
                else stuck = 0f;
            }

            private void OnDestroy() { if (runner == this) runner = null; }
        }
    }
}
