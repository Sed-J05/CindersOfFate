using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Docs/07 §5 soft drop-shadow: two child copies of the puppet's sprite drawn just behind it, tinted black —
    /// alpha = theme shadow alpha (0.30 P1 / 0.35 P2) and a blur copy at alpha 0.15, scale ×1.08 — offset away from the
    /// flame by 0.10 u, growing to 0.35 u as the puppet nears the flame. Same material as the puppet (Sprite-Lit or
    /// Unlit default), so no custom shader. Global toggle: ArtStyleConfig.dropShadows / F11.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PuppetShadow : MonoBehaviour
    {
        [SerializeField] private float baseOffset = 0.10f;
        [SerializeField] private float maxOffset = 0.35f;
        [SerializeField] private float shadowScale = 1.04f;
        [SerializeField] private float blurScale = 1.08f;
        [SerializeField, Range(0f, 1f)] private float blurAlpha = 0.15f;

        /// <summary>Global switch (ArtStyleConfig.dropShadows, F11 debug).</summary>
        public static bool GloballyEnabled = true;
        /// <summary>Shadow alpha for the current theme, set by ThemeManager.</summary>
        public static float ThemeAlpha = 0.30f;

        private SpriteRenderer source;
        private SpriteRenderer shadow;
        private SpriteRenderer blur;
        private static readonly Vector2 FallbackDirection = new Vector2(0.6f, -0.8f);

        private void Awake()
        {
            source = GetComponent<SpriteRenderer>();
            shadow = CreateCopy("PuppetShadow", -2);
            blur = CreateCopy("PuppetShadowBlur", -3);
        }

        private SpriteRenderer CreateCopy(string copyName, int orderOffset)
        {
            var go = new GameObject(copyName);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = source.sharedMaterial;
            sr.sortingLayerID = source.sortingLayerID;
            sr.sortingOrder = source.sortingOrder + orderOffset;
            sr.enabled = false;
            return sr;
        }

        private void LateUpdate()
        {
            bool show = GloballyEnabled && ThemeAlpha > 0f && source.enabled && source.sprite != null;
            shadow.enabled = show;
            blur.enabled = show;
            if (!show) return;

            shadow.sprite = source.sprite;
            blur.sprite = source.sprite;
            shadow.flipX = blur.flipX = source.flipX;
            shadow.flipY = blur.flipY = source.flipY;
            shadow.sortingLayerID = blur.sortingLayerID = source.sortingLayerID;
            shadow.sortingOrder = source.sortingOrder - 2;
            blur.sortingOrder = source.sortingOrder - 3;

            Vector2 pos = transform.position;
            Vector2 dir = FallbackDirection;
            float k = baseOffset;
            FlameController flame = FlameController.Instance;
            if (flame != null)
            {
                Vector2 away = pos - flame.WorldPosition;
                float dist = away.magnitude;
                if (dist > 0.001f) dir = away / dist;
                Camera cam = flame.Camera;
                float screenWidth = cam != null ? cam.orthographicSize * 2f * cam.aspect : 19.6f;
                float closeness = 1f - Mathf.Clamp01(dist / screenWidth);
                k = Mathf.Lerp(baseOffset, maxOffset, closeness);
            }

            Vector3 offset = new Vector3(dir.x * k, dir.y * k, 0f);
            shadow.transform.position = transform.position + offset;
            blur.transform.position = transform.position + offset * 1.15f;
            shadow.transform.localScale = new Vector3(shadowScale, shadowScale, 1f);
            blur.transform.localScale = new Vector3(blurScale, blurScale, 1f);
            shadow.color = new Color(0f, 0f, 0f, ThemeAlpha);
            blur.color = new Color(0f, 0f, 0f, blurAlpha * (ThemeAlpha / 0.3f));
        }
    }
}
