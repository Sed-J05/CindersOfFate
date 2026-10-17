using Shatranj.Core;
using TMPro;
using UnityEngine;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/08 §6 interact prompt ("W — Rest", "W — Touch") anchored above a world position.
    /// Driven by OnInteractPromptShown / Hidden; converts world → canvas space every frame while visible.
    /// </summary>
    public class InteractPromptUI : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform promptRect;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float worldYOffset = 2.2f;
        [SerializeField] private float fadeTime = 0.15f;
        [SerializeField] private float bobAmplitude = 6f;

        private Vector2 worldPosition;
        private bool shown;
        private float alpha;
        private Camera cam;
        private float bobTime;

        private void OnEnable()
        {
            GameEvents.OnInteractPromptShown += HandleShown;
            GameEvents.OnInteractPromptHidden += HandleHidden;
            if (group != null) group.alpha = 0f;
        }

        private void OnDisable()
        {
            GameEvents.OnInteractPromptShown -= HandleShown;
            GameEvents.OnInteractPromptHidden -= HandleHidden;
        }

        private void HandleShown(string text, Vector2 position)
        {
            worldPosition = position;
            if (label != null) label.text = text;
            shown = true;
        }

        private void HandleHidden() => shown = false;

        private void Update()
        {
            if (group == null || promptRect == null || canvasRect == null) return;
            float dt = Time.unscaledDeltaTime;
            alpha = Mathf.MoveTowards(alpha, shown ? 1f : 0f, dt / Mathf.Max(0.01f, fadeTime));
            group.alpha = alpha;
            if (alpha <= 0f) return;

            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            bobTime += dt;
            Vector3 world = new Vector3(worldPosition.x, worldPosition.y + worldYOffset, 0f);
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local))
                promptRect.anchoredPosition = local + new Vector2(0f, Mathf.Sin(bobTime * 3f) * bobAmplitude);
        }
    }
}
