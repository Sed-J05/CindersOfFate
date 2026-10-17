using System;
using System.Collections;
using Shatranj.Core;
using Shatranj.UI;
using Shatranj.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.Cutscenes
{
    /// <summary>
    /// Cutscene overlay UI built in code (Docs/08 §1): cut-paper caption box with speaker name and a TextMeshPro
    /// typewriter (~35 chars/s), title/ability cards, screen flash, the hold-to-skip ring with "Skipping…", and the
    /// Phase 2 inversion wipe — a circle mask expanding from the flame with the inverted parchment inside (Docs/07 §7).
    /// </summary>
    public class CutsceneUI : MonoBehaviour
    {
        [SerializeField] private float charsPerSecond = 35f;
        [SerializeField] private float autoAdvanceBase = 2.2f;
        [SerializeField] private float autoAdvancePerChar = 0.03f;

        private RectTransform canvas;
        private CanvasGroup captionGroup;
        private TextMeshProUGUI speakerText;
        private TextMeshProUGUI lineText;
        private TextMeshProUGUI nextHint;
        private CanvasGroup cardGroup;
        private TextMeshProUGUI cardTitle;
        private TextMeshProUGUI cardSub;
        private Image flash;
        private GameObject skipRoot;
        private Image skipRing;
        private RectTransform wipeMask;
        private Image wipeInside;

        private const int CardSortingOrder = 32100;

        private void Awake()
        {
            canvas = UIFactory.Canvas("CutsceneCanvas", transform, 900);

            // Card (title / abilities)
            RectTransform cardRoot = UIFactory.Stretch("Card", canvas);
            // Cards draw above SceneFlow's fade overlay (32000) so they can be read on a black screen (credits, "Cycle N").
            var cardCanvas = cardRoot.gameObject.AddComponent<Canvas>();
            cardCanvas.overrideSorting = true;
            cardCanvas.sortingOrder = CardSortingOrder;
            cardGroup = cardRoot.gameObject.AddComponent<CanvasGroup>();
            cardGroup.alpha = 0f;
            cardTitle = UIFactory.Text("Title", cardRoot, "", 80, TextAlignmentOptions.Center, UIFactory.Ink);
            cardTitle.fontStyle = FontStyles.Bold;
            cardTitle.rectTransform.offsetMin = new Vector2(0f, 40f);
            cardSub = UIFactory.Text("Subtitle", cardRoot, "", 40, TextAlignmentOptions.Center, UIFactory.Ink);
            cardSub.fontStyle = FontStyles.Italic;
            cardSub.rectTransform.offsetMax = new Vector2(0f, -90f);

            // Caption box (bottom)
            RectTransform cap = UIFactory.Rect("Caption", canvas, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(1500f, 230f));
            captionGroup = cap.gameObject.AddComponent<CanvasGroup>();
            captionGroup.alpha = 0f;
            Sprite box = UIFactory.CatalogSprite("Button_Normal");
            UIFactory.Image("Box", cap, box, box != null ? new Color(1f, 1f, 1f, 0.94f) : new Color(0.91f, 0.85f, 0.71f, 0.94f));
            speakerText = UIFactory.Text("Speaker", cap, "", 30, TextAlignmentOptions.TopLeft, new Color(0.45f, 0.15f, 0.15f));
            speakerText.fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
            speakerText.rectTransform.offsetMin = new Vector2(48f, 20f);
            speakerText.rectTransform.offsetMax = new Vector2(-48f, -18f);
            lineText = UIFactory.Text("Line", cap, "", 36, TextAlignmentOptions.Center, UIFactory.Ink);
            lineText.rectTransform.offsetMin = new Vector2(60f, 24f);
            lineText.rectTransform.offsetMax = new Vector2(-60f, -52f);
            nextHint = UIFactory.Text("Next", cap, "▶", 28, TextAlignmentOptions.BottomRight, UIFactory.Ink);
            nextHint.rectTransform.offsetMin = new Vector2(0f, 16f);
            nextHint.rectTransform.offsetMax = new Vector2(-30f, 0f);
            nextHint.enabled = false;

            // Flash
            flash = UIFactory.Image("Flash", canvas, null, new Color(1f, 1f, 1f, 0f));

            // Skip ring (bottom-right)
            RectTransform skip = UIFactory.Rect("Skip", canvas, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 290f), new Vector2(320f, 70f));
            skipRoot = skip.gameObject;
            RectTransform ringRect = UIFactory.Rect("Ring", skip, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(56f, 56f));
            skipRing = ringRect.gameObject.AddComponent<Image>();
            skipRing.sprite = PlaceholderSprites.Circle;
            skipRing.type = Image.Type.Filled;
            skipRing.fillMethod = Image.FillMethod.Radial360;
            skipRing.color = new Color(0.13f, 0.11f, 0.10f, 0.85f);
            skipRing.raycastTarget = false;
            TextMeshProUGUI skipText = UIFactory.Text("Label", skip, "Skipping…", 30, TextAlignmentOptions.MidlineRight, UIFactory.Ink);
            skipText.fontStyle = FontStyles.Italic;
            skipRoot.SetActive(false);

            // Inversion wipe: circle mask + inverted parchment child
            wipeMask = UIFactory.Rect("InvertMask", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var maskImage = wipeMask.gameObject.AddComponent<Image>();
            maskImage.sprite = PlaceholderSprites.Circle;
            maskImage.raycastTarget = false;
            var mask = wipeMask.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            RectTransform inside = UIFactory.Rect("Inside", wipeMask, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920f, 1080f));
            wipeInside = inside.gameObject.AddComponent<Image>();
            wipeInside.raycastTarget = false;
            wipeMask.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------
        // Caption
        // ------------------------------------------------------------------

        /// <summary>Types one line; first press completes it, next press (or the auto-advance delay) moves on.</summary>
        public IEnumerator ShowLine(string speaker, string line, string style, Func<bool> advancePressed, Func<bool> skipping)
        {
            captionGroup.alpha = 1f;
            speakerText.text = speaker ?? "";
            ApplyStyle(style);
            lineText.text = line;
            lineText.maxVisibleCharacters = 0;
            nextHint.enabled = false;
            int total = line.Length;
            float shown = 0f;

            while (lineText.maxVisibleCharacters < total)
            {
                if (skipping()) yield break;
                if (advancePressed()) { lineText.maxVisibleCharacters = total; break; }
                shown += charsPerSecond * Time.unscaledDeltaTime;
                lineText.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(shown));
                yield return null;
            }

            nextHint.enabled = true;
            float wait = autoAdvanceBase + autoAdvancePerChar * total;
            for (float t = 0f; t < wait; t += Time.unscaledDeltaTime)
            {
                if (skipping() || advancePressed()) break;
                nextHint.alpha = 0.5f + 0.5f * Mathf.Sin(t * 6f);
                yield return null;
            }
            nextHint.enabled = false;
        }

        public void HideCaption() => captionGroup.alpha = 0f;

        private void ApplyStyle(string style)
        {
            switch (style)
            {
                case "inner":
                    lineText.fontStyle = FontStyles.Italic;
                    lineText.color = new Color(0.12f, 0.29f, 0.55f);
                    break;
                case "boss":
                    lineText.fontStyle = FontStyles.Bold;
                    lineText.color = new Color(0.62f, 0.07f, 0.1f);
                    break;
                default: // narration
                    lineText.fontStyle = FontStyles.Normal;
                    lineText.color = UIFactory.Ink;
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Cards, flash, skip
        // ------------------------------------------------------------------

        public IEnumerator Card(string title, string subtitle, float hold, Color color, Func<bool> skipping)
        {
            cardTitle.text = title ?? "";
            cardSub.text = subtitle ?? "";
            cardTitle.color = color;
            cardSub.color = color;
            const float fade = 0.4f;
            for (float t = 0f; t < fade && !skipping(); t += Time.unscaledDeltaTime) { cardGroup.alpha = t / fade; yield return null; }
            cardGroup.alpha = 1f;
            for (float t = 0f; t < hold && !skipping(); t += Time.unscaledDeltaTime) yield return null;
            for (float t = 0f; t < fade && !skipping(); t += Time.unscaledDeltaTime) { cardGroup.alpha = 1f - t / fade; yield return null; }
            cardGroup.alpha = 0f;
        }

        public IEnumerator Flash(Color color, float time)
        {
            if (time <= 0f) yield break;
            for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
            {
                Color c = color;
                c.a = color.a * (1f - t / time);
                flash.color = c;
                yield return null;
            }
            flash.color = new Color(1f, 1f, 1f, 0f);
        }

        private const int RewindStreaks = 14;
        private RectTransform rewindRoot;
        private Image[] rewindBars;
        private float[] rewindSpeed;

        /// <summary>
        /// Docs/08 §4 beat 8 "the paper rewinds": thin paper-coloured streaks sweep right → left across the screen,
        /// accelerating, while the stage pulls back (zoom beat) and the flame runs back to t = 0 (flame beat).
        /// </summary>
        public IEnumerator Rewind(float time, Color color, Func<bool> skipping)
        {
            if (time <= 0f) yield break;
            EnsureRewind();
            float width = canvas.rect.width > 1f ? canvas.rect.width : 1920f;
            float height = canvas.rect.height > 1f ? canvas.rect.height : 1080f;
            for (int i = 0; i < RewindStreaks; i++)
            {
                RectTransform r = rewindBars[i].rectTransform;
                r.sizeDelta = new Vector2(UnityEngine.Random.Range(width * 0.25f, width * 0.8f), UnityEngine.Random.Range(3f, 14f));
                r.anchoredPosition = new Vector2(UnityEngine.Random.Range(-width, width), UnityEngine.Random.Range(-height * 0.48f, height * 0.48f));
                rewindSpeed[i] = UnityEngine.Random.Range(0.6f, 1.4f);
            }
            rewindRoot.gameObject.SetActive(true);
            for (float t = 0f; t < time && !skipping(); t += Time.unscaledDeltaTime)
            {
                float k = t / time;
                float ramp = Mathf.Sin(Mathf.PI * k);                 // fade in, peak mid-way, fade out
                float speed = Mathf.Lerp(900f, 4200f, k * k);          // accelerating "tape"
                for (int i = 0; i < RewindStreaks; i++)
                {
                    RectTransform r = rewindBars[i].rectTransform;
                    Vector2 p = r.anchoredPosition;
                    p.x -= speed * rewindSpeed[i] * Time.unscaledDeltaTime;
                    if (p.x < -width) { p.x = width; p.y = UnityEngine.Random.Range(-height * 0.48f, height * 0.48f); }
                    r.anchoredPosition = p;
                    Color c = color;
                    c.a = color.a * ramp;
                    rewindBars[i].color = c;
                }
                yield return null;
            }
            rewindRoot.gameObject.SetActive(false);
        }

        private void EnsureRewind()
        {
            if (rewindRoot != null) return;
            rewindRoot = UIFactory.Stretch("Rewind", canvas);
            rewindRoot.SetSiblingIndex(0);   // under the caption and cards
            rewindBars = new Image[RewindStreaks];
            rewindSpeed = new float[RewindStreaks];
            for (int i = 0; i < RewindStreaks; i++)
            {
                RectTransform bar = UIFactory.Rect("Streak" + i, rewindRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 6f));
                rewindBars[i] = bar.gameObject.AddComponent<Image>();
                rewindBars[i].raycastTarget = false;
            }
            rewindRoot.gameObject.SetActive(false);
        }

        public void SetSkipProgress(float progress)
        {
            bool show = progress > 0.01f;
            if (skipRoot.activeSelf != show) skipRoot.SetActive(show);
            skipRing.fillAmount = Mathf.Clamp01(progress);
        }

        // ------------------------------------------------------------------
        // Inversion wipe (Docs/07 §7)
        // ------------------------------------------------------------------

        /// <summary>Circle grows from <paramref name="world"/> until the inverted parchment covers the screen.</summary>
        public IEnumerator InvertWipe(Vector2 world, Sprite insideSprite, Color insideColor, float time, Func<bool> skipping)
        {
            Camera cam = Camera.main;
            Vector2 screen = cam != null ? (Vector2)cam.WorldToScreenPoint(world) : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out Vector2 local);
            Vector2 canvasSize = canvas.rect.size;
            float maxR = canvasSize.magnitude;

            wipeInside.sprite = insideSprite;
            wipeInside.color = insideColor;
            wipeInside.rectTransform.sizeDelta = canvasSize;
            wipeMask.anchoredPosition = local;
            wipeInside.rectTransform.anchoredPosition = -local;   // keep the inside image fixed to the screen
            wipeMask.gameObject.SetActive(true);

            for (float t = 0f; t < time && !skipping(); t += Time.unscaledDeltaTime)
            {
                float r = maxR * Easing.Evaluate(Ease.InOutQuad, t / time);
                wipeMask.sizeDelta = new Vector2(r * 2f, r * 2f);
                yield return null;
            }
            wipeMask.sizeDelta = new Vector2(maxR * 2f, maxR * 2f);
        }

        public void HideInvertWipe() => wipeMask.gameObject.SetActive(false);
    }
}
