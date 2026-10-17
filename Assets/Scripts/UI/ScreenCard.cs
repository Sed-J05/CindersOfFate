using System.Collections;
using TMPro;
using UnityEngine;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/08 §7 text cards built in code: phase card ("PHASE II — THE INVERTED THEATRE"), death text
    /// ("You were extinguished"), cycle card. Optional backdrop, fade in → hold → fade out, unscaled time.
    /// </summary>
    public class ScreenCard : MonoBehaviour
    {
        private CanvasGroup group;

        /// <summary>Shows a card and returns the coroutine; yield on it to wait for the whole card.</summary>
        public static Coroutine Show(MonoBehaviour host, string title, string subtitle, float backdropAlpha, float fade, float hold, Color? textColor = null)
        {
            RectTransform canvas = UIFactory.Canvas("[ScreenCard]", null, 31500);
            var card = canvas.gameObject.AddComponent<ScreenCard>();
            card.group = canvas.gameObject.AddComponent<CanvasGroup>();
            card.group.alpha = 0f;
            card.group.blocksRaycasts = false;

            if (backdropAlpha > 0f) UIFactory.Image("Backdrop", canvas, null, new Color(0f, 0f, 0f, backdropAlpha));
            Color c = textColor ?? UIFactory.Paper;
            TextMeshProUGUI t = UIFactory.Text("Title", canvas, title, 72, TextAlignmentOptions.Center, c);
            t.fontStyle = FontStyles.Bold;
            if (!string.IsNullOrEmpty(subtitle))
            {
                t.rectTransform.offsetMin = new Vector2(0f, 60f);
                TextMeshProUGUI s = UIFactory.Text("Subtitle", canvas, subtitle, 38, TextAlignmentOptions.Center, c);
                s.fontStyle = FontStyles.Italic;
                s.rectTransform.offsetMax = new Vector2(0f, -70f);
            }
            return host.StartCoroutine(card.Run(fade, hold));
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
