using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.UI
{
    /// <summary>
    /// Placeholder end card (Prompt 9): a full-screen black overlay with "To be continued…" that fades in, holds, and
    /// fades out. Built in code with the legacy font so it works without TMP or a prefab. Unscaled time.
    /// Replaced by Cutscene 2 in Prompt 11.
    /// </summary>
    public class ToBeContinuedCard : MonoBehaviour
    {
        private CanvasGroup group;

        public static Coroutine Show(MonoBehaviour host, string text, float fade = 0.8f, float hold = 3f)
        {
            var go = new GameObject("[ToBeContinued]", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 31000;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            var card = go.AddComponent<ToBeContinuedCard>();
            card.group = go.GetComponent<CanvasGroup>();
            card.group.alpha = 0f;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            Stretch((RectTransform)bg.transform);
            bg.GetComponent<Image>().color = Color.black;

            var label = new GameObject("Text", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(go.transform, false);
            Stretch((RectTransform)label.transform);
            var t = label.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 72;
            t.fontStyle = FontStyle.Italic;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color(0.91f, 0.85f, 0.71f);
            t.text = text;

            return host.StartCoroutine(card.Run(fade, hold));
        }

        private static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        private IEnumerator Run(float fade, float hold)
        {
            for (float t = 0f; t < fade; t += Time.unscaledDeltaTime) { group.alpha = t / fade; yield return null; }
            group.alpha = 1f;
            for (float t = 0f; t < hold; t += Time.unscaledDeltaTime) yield return null;
            for (float t = 0f; t < fade; t += Time.unscaledDeltaTime) { group.alpha = 1f - t / fade; yield return null; }
            Destroy(gameObject);
        }
    }
}
