using Shatranj.Core;
using TMPro;
using UnityEngine;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/08 §7 boss intro card: "KAGE WARDEN — Keeper of the Gate" slides in, holds and fades over 1.5 s.
    /// Driven by <see cref="GameEvents.OnBossIntroCard"/>; unscaled time.
    /// </summary>
    public class BossIntroCardUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform slider;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI epithetText;
        [SerializeField] private float totalTime = 1.5f;
        [SerializeField] private float fadeTime = 0.25f;
        [SerializeField] private float slideDistance = 80f;

        private float t = -1f;
        private Vector2 basePos;

        private void Awake()
        {
            if (slider != null) basePos = slider.anchoredPosition;
            if (group != null) group.alpha = 0f;
        }

        private void OnEnable() => GameEvents.OnBossIntroCard += Show;
        private void OnDisable() => GameEvents.OnBossIntroCard -= Show;

        private void Show(string displayName, string epithet)
        {
            if (nameText != null) nameText.text = displayName.ToUpperInvariant();
            if (epithetText != null) epithetText.text = string.IsNullOrEmpty(epithet) ? "" : "— " + epithet;
            t = 0f;
        }

        private void Update()
        {
            if (t < 0f || group == null) return;
            t += Time.unscaledDeltaTime;
            float alpha;
            if (t < fadeTime) alpha = t / fadeTime;
            else if (t < totalTime - fadeTime) alpha = 1f;
            else alpha = Mathf.Clamp01((totalTime - t) / fadeTime);
            group.alpha = alpha;

            if (slider != null)
            {
                float k = Easing.Evaluate(Ease.OutCubic, Mathf.Clamp01(t / (fadeTime * 2f)));
                slider.anchoredPosition = basePos + new Vector2(-slideDistance * (1f - k), 0f);
            }
            if (t >= totalTime) { t = -1f; group.alpha = 0f; }
        }
    }
}
