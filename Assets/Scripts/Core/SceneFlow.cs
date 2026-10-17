using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shatranj.Core
{
    /// <summary>
    /// Async scene loading behind a persistent full-screen fade (Docs/02 §8). The overlay Canvas is built in
    /// code, lives in DontDestroyOnLoad and uses unscaled time so it works while paused or in hit-stop.
    /// </summary>
    public class SceneFlow : MonoBehaviour
    {
        private const int OverlaySortingOrder = 32000;

        private static SceneFlow instance;

        private Canvas canvas;
        private Image overlay;
        private Coroutine running;

        /// <summary>Colour of the fade. Black by default; the theme system can set parchment tones later.</summary>
        public Color FadeColor { get; set; } = Color.black;

        public bool IsTransitioning { get; private set; }
        public float OverlayAlpha => overlay != null ? overlay.color.a : 0f;

        public static SceneFlow Instance => Ensure();

        public static SceneFlow Ensure()
        {
            if (instance != null) return instance;
            var go = new GameObject("[SceneFlow]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SceneFlow>();
            instance.BuildOverlay();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void BuildOverlay()
        {
            if (overlay != null) return;

            var canvasGo = new GameObject("FadeCanvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = OverlaySortingOrder;

            var imageGo = new GameObject("Fade", typeof(RectTransform));
            imageGo.transform.SetParent(canvasGo.transform, false);
            var rect = (RectTransform)imageGo.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            overlay = imageGo.AddComponent<Image>();
            overlay.raycastTarget = false;
            SetOverlayImmediate(0f);
        }

        /// <summary>Sets the overlay alpha with no animation.</summary>
        public void SetOverlayImmediate(float alpha)
        {
            if (overlay == null) BuildOverlay();
            Color c = FadeColor;
            c.a = Mathf.Clamp01(alpha);
            overlay.color = c;
            overlay.enabled = c.a > 0f;
        }

        /// <summary>Fades the overlay to <paramref name="alpha"/> over <paramref name="time"/> seconds (unscaled).</summary>
        public void FadeTo(float alpha, float time, Action onDone = null)
        {
            StopRunning();
            running = StartCoroutine(FadeRoutine(alpha, time, onDone));
        }

        /// <summary>Fade out, load the scene asynchronously, fade in. Single-scene mode.</summary>
        public void LoadScene(string sceneName, float fadeTime = 0.5f, Action onLoaded = null)
        {
            StopRunning();
            running = StartCoroutine(LoadRoutine(sceneName, fadeTime, onLoaded));
        }

        /// <summary>Convenience wrapper: <c>SceneFlow.Load("Phase1_Cave")</c>.</summary>
        public static void Load(string sceneName, float fadeTime = 0.5f, Action onLoaded = null)
        {
            Instance.LoadScene(sceneName, fadeTime, onLoaded);
        }

        /// <summary>Reloads the active scene (F9 debug, restart from bench).</summary>
        public static void ReloadActive(float fadeTime = 0.2f, Action onLoaded = null)
        {
            Load(SceneManager.GetActiveScene().name, fadeTime, onLoaded);
        }

        private void StopRunning()
        {
            if (running != null) { StopCoroutine(running); running = null; }
            IsTransitioning = false;
        }

        private IEnumerator FadeRoutine(float targetAlpha, float time, Action onDone)
        {
            if (overlay == null) BuildOverlay();
            float start = overlay.color.a;
            targetAlpha = Mathf.Clamp01(targetAlpha);
            overlay.enabled = true;

            if (time <= 0f)
            {
                SetOverlayImmediate(targetAlpha);
            }
            else
            {
                float t = 0f;
                while (t < time)
                {
                    t += Time.unscaledDeltaTime;
                    SetOverlayImmediateKeepEnabled(Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t / time)));
                    yield return null;
                }
                SetOverlayImmediate(targetAlpha);
            }

            running = null;
            onDone?.Invoke();
        }

        private void SetOverlayImmediateKeepEnabled(float alpha)
        {
            Color c = FadeColor;
            c.a = alpha;
            overlay.color = c;
        }

        private IEnumerator LoadRoutine(string sceneName, float fadeTime, Action onLoaded)
        {
            IsTransitioning = true;
            yield return FadeRoutine(1f, fadeTime, null);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError($"SceneFlow: scene '{sceneName}' is not in Build Settings.");
                IsTransitioning = false;
                yield return FadeRoutine(0f, fadeTime, null);
                yield break;
            }

            while (!op.isDone) yield return null;
            yield return null; // let Awake/Start of the new scene run before revealing it

            onLoaded?.Invoke();
            IsTransitioning = false;
            yield return FadeRoutine(0f, fadeTime, null);
        }
    }
}
