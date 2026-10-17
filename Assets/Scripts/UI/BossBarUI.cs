using System.Collections.Generic;
using Shatranj.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/08 §6 boss bar: frame + horizontal fill at bottom-centre, boss name above, fill-in over 0.8 s on
    /// engage, stage ticks at the thresholds (Zulmat 67 % / 34 %), fade-out on defeat.
    /// Driven by OnBossStarted / OnBossHealthChanged / OnBossDefeated.
    /// </summary>
    public class BossBarUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image fill;
        [SerializeField] private RectTransform fillArea;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private RectTransform ticksContainer;
        [SerializeField] private float fillInTime = 0.8f;
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private float fadeTime = 0.6f;
        [SerializeField] private float tickWidth = 4f;
        [SerializeField] private Color tickColor = new Color(1f, 1f, 1f, 0.8f);

        private readonly List<RectTransform> ticks = new List<RectTransform>();
        private string bossId;
        private float target = 1f;
        private float velocity;
        private float fillInT = -1f;
        private float fadeT = -1f;
        private float[] thresholds = new float[0];
        private float lastShownFraction = 1f;
        private float tickFlashT = -1f;

        public bool IsVisible => group != null && group.alpha > 0f;
        public string BossId => bossId;

        private void OnEnable()
        {
            GameEvents.OnBossStarted += HandleBossStarted;
            GameEvents.OnBossHealthChanged += HandleBossHealth;
            GameEvents.OnBossDefeated += HandleBossDefeated;
            if (group != null) group.alpha = 0f;
        }

        private void OnDisable()
        {
            GameEvents.OnBossStarted -= HandleBossStarted;
            GameEvents.OnBossHealthChanged -= HandleBossHealth;
            GameEvents.OnBossDefeated -= HandleBossDefeated;
        }

        private void HandleBossStarted(string id, string displayName, float[] stageThresholds)
        {
            bossId = id;
            thresholds = stageThresholds ?? new float[0];
            if (nameText != null) nameText.text = displayName;
            BuildTicks();
            target = 1f;
            lastShownFraction = 1f;
            fillInT = 0f;
            fadeT = -1f;
            if (fill != null) fill.fillAmount = 0f;
            if (group != null) group.alpha = 1f;
        }

        private void HandleBossHealth(string id, int current, int max)
        {
            if (id != bossId) return;
            target = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            for (int i = 0; i < thresholds.Length; i++)
                if (lastShownFraction > thresholds[i] && target <= thresholds[i]) tickFlashT = 0f; // stage crossed
            lastShownFraction = target;
        }

        private void HandleBossDefeated(string id)
        {
            if (id != bossId) return;
            target = 0f;
            fadeT = 0f;
        }

        private void BuildTicks()
        {
            for (int i = 0; i < ticks.Count; i++) if (ticks[i] != null) Destroy(ticks[i].gameObject);
            ticks.Clear();
            if (ticksContainer == null || fillArea == null) return;

            float width = fillArea.rect.width;
            for (int i = 0; i < thresholds.Length; i++)
            {
                var go = new GameObject($"Tick_{i}", typeof(RectTransform), typeof(Image));
                var rect = (RectTransform)go.transform;
                rect.SetParent(ticksContainer, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(tickWidth, fillArea.rect.height);
                rect.anchoredPosition = new Vector2(Mathf.Clamp01(thresholds[i]) * width, 0f);
                var img = go.GetComponent<Image>();
                img.color = tickColor;
                img.raycastTarget = false;
                ticks.Add(rect);
            }
        }

        private void Update()
        {
            if (group == null || fill == null) return;
            float dt = Time.unscaledDeltaTime;

            if (fillInT >= 0f)
            {
                fillInT += dt / Mathf.Max(0.01f, fillInTime);
                fill.fillAmount = Mathf.Lerp(0f, target, Easing.Evaluate(Ease.OutCubic, Mathf.Clamp01(fillInT)));
                if (fillInT >= 1f) fillInT = -1f;
            }
            else
            {
                fill.fillAmount = Mathf.SmoothDamp(fill.fillAmount, target, ref velocity, smoothTime, Mathf.Infinity, dt);
            }

            if (tickFlashT >= 0f)
            {
                tickFlashT += dt / 0.4f;
                float s = 1f + (1f - Mathf.Clamp01(tickFlashT)) * 1.5f;
                for (int i = 0; i < ticks.Count; i++) ticks[i].localScale = new Vector3(s, 1f, 1f);
                if (tickFlashT >= 1f) tickFlashT = -1f;
            }

            if (fadeT >= 0f)
            {
                fadeT += dt / Mathf.Max(0.01f, fadeTime);
                group.alpha = 1f - Mathf.Clamp01(fadeT);
                if (fadeT >= 1f) { fadeT = -1f; group.alpha = 0f; bossId = null; }
            }
        }
    }
}
