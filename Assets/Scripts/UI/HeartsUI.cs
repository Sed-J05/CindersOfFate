using System.Collections.Generic;
using Shatranj.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/08 §6 hearts: 64×64, 8 px spacing, from (270, −70). Full/Empty sprites, Heart_Break animation
    /// (0.25 s) on loss, scale pulse on gain, new hearts fade in when the maximum grows (phase change).
    /// Driven only by <see cref="GameEvents.OnHealthChanged"/>.
    /// </summary>
    public class HeartsUI : MonoBehaviour
    {
        [SerializeField] private RectTransform container;
        [SerializeField] private Sprite fullSprite;
        [SerializeField] private Sprite emptySprite;
        [SerializeField] private Sprite[] breakFrames;
        [SerializeField] private float heartSize = 64f;
        [SerializeField] private float spacing = 8f;
        [SerializeField] private float breakTime = 0.25f;
        [SerializeField] private float gainPulseTime = 0.25f;
        [SerializeField] private float gainPulseScale = 1.35f;
        [SerializeField] private float fadeInTime = 0.5f;

        private class Heart
        {
            public RectTransform rect;
            public Image image;
            public CanvasGroup group;
            public bool full;
            public float breakT = -1f;
            public float pulseT = -1f;
            public float fadeT = -1f;
        }

        private readonly List<Heart> hearts = new List<Heart>();
        private bool initialized;

        public int Current { get; private set; }
        public int Max { get; private set; }

        private void OnEnable()
        {
            GameEvents.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(int current, int max)
        {
            max = Mathf.Max(1, max);
            current = Mathf.Clamp(current, 0, max);
            EnsureCount(max);

            for (int i = 0; i < hearts.Count; i++)
            {
                Heart h = hearts[i];
                bool nowFull = i < current;
                if (initialized)
                {
                    if (h.full && !nowFull) { h.breakT = 0f; h.pulseT = -1f; }
                    else if (!h.full && nowFull) { h.pulseT = 0f; h.breakT = -1f; }
                }
                h.full = nowFull;
                if (h.breakT < 0f) h.image.sprite = nowFull ? fullSprite : emptySprite;
            }

            Current = current;
            Max = max;
            initialized = true;
        }

        private void EnsureCount(int count)
        {
            while (hearts.Count < count)
            {
                int i = hearts.Count;
                var go = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                var rect = (RectTransform)go.transform;
                rect.SetParent(container, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(heartSize, heartSize);
                rect.anchoredPosition = new Vector2(i * (heartSize + spacing) + heartSize * 0.5f, -heartSize * 0.5f);
                var img = go.GetComponent<Image>();
                img.sprite = emptySprite;
                img.raycastTarget = false;
                img.preserveAspect = true;
                var group = go.GetComponent<CanvasGroup>();
                group.alpha = initialized ? 0f : 1f;
                hearts.Add(new Heart { rect = rect, image = img, group = group, fadeT = initialized ? 0f : -1f });
            }
            for (int i = 0; i < hearts.Count; i++) hearts[i].rect.gameObject.SetActive(i < count);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < hearts.Count; i++)
            {
                Heart h = hearts[i];

                if (h.breakT >= 0f)
                {
                    h.breakT += dt / Mathf.Max(0.01f, breakTime);
                    if (breakFrames != null && breakFrames.Length > 0 && h.breakT < 1f)
                    {
                        int f = Mathf.Clamp(Mathf.FloorToInt(h.breakT * breakFrames.Length), 0, breakFrames.Length - 1);
                        if (breakFrames[f] != null) h.image.sprite = breakFrames[f];
                    }
                    float shake = (1f - Mathf.Clamp01(h.breakT)) * 4f;
                    h.rect.anchoredPosition = BasePosition(i) + new Vector2(Mathf.Sin(h.breakT * 60f) * shake, 0f);
                    if (h.breakT >= 1f)
                    {
                        h.breakT = -1f;
                        h.image.sprite = h.full ? fullSprite : emptySprite;
                        h.rect.anchoredPosition = BasePosition(i);
                    }
                }

                if (h.pulseT >= 0f)
                {
                    h.pulseT += dt / Mathf.Max(0.01f, gainPulseTime);
                    float k = Mathf.Clamp01(h.pulseT);
                    float s = Mathf.Lerp(gainPulseScale, 1f, Easing.Evaluate(Ease.OutBack, k));
                    h.rect.localScale = new Vector3(s, s, 1f);
                    if (h.pulseT >= 1f) { h.pulseT = -1f; h.rect.localScale = Vector3.one; }
                }

                if (h.fadeT >= 0f)
                {
                    h.fadeT += dt / Mathf.Max(0.01f, fadeInTime);
                    h.group.alpha = Mathf.Clamp01(h.fadeT);
                    if (h.fadeT >= 1f) { h.fadeT = -1f; h.group.alpha = 1f; }
                }
            }
        }

        private Vector2 BasePosition(int i) => new Vector2(i * (heartSize + spacing) + heartSize * 0.5f, -heartSize * 0.5f);
    }
}
