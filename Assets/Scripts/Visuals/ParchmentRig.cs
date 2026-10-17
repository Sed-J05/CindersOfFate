using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Camera-child screen stack (Docs/07 §1): the parchment (tiled, never scrolls) with a second renderer used to
    /// cross-fade textures between themes, the soft FlameGlow and the blurred FlameCore at the flame's screen position,
    /// and the burnt-edge ParchmentFrame overlay. Everything is resized to the camera every frame (zoom-safe).
    /// Parallax BG layers are separate <see cref="ParallaxLayer"/> children.
    /// </summary>
    public class ParchmentRig : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private SpriteRenderer parchment;
        [SerializeField] private SpriteRenderer parchmentCrossfade;
        [SerializeField] private SpriteRenderer flameGlow;
        [SerializeField] private SpriteRenderer flameCore;
        [SerializeField] private SpriteRenderer frame;
        [SerializeField] private ParallaxLayer backgroundFar;
        [SerializeField] private ParallaxLayer backgroundMid;
        [SerializeField, Tooltip("Glow diameter as a fraction of the screen height.")]
        private float glowSize = 1.1f;
        [SerializeField, Tooltip("Core size as a fraction of the screen height.")]
        private float coreSize = 0.35f;

        public SpriteRenderer Parchment => parchment;
        public SpriteRenderer ParchmentCrossfade => parchmentCrossfade;
        public SpriteRenderer FlameGlow => flameGlow;
        public SpriteRenderer FlameCore => flameCore;
        public SpriteRenderer Frame => frame;
        public ParallaxLayer BackgroundFar => backgroundFar;
        public ParallaxLayer BackgroundMid => backgroundMid;

        private void Awake()
        {
            if (targetCamera == null) targetCamera = GetComponentInParent<Camera>();
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;
            float h = targetCamera.orthographicSize * 2f;
            float w = h * targetCamera.aspect;
            var screen = new Vector2(w * 1.04f, h * 1.04f);

            FitTiled(parchment, screen);
            FitTiled(parchmentCrossfade, screen);
            FitStretched(frame, screen);

            FlameController f = FlameController.Instance;
            if (f != null)
            {
                Vector2 local = new Vector2((f.ViewportPosition.x - 0.5f) * w, (f.ViewportPosition.y - 0.5f) * h);
                Place(flameGlow, local, h * glowSize);
                Place(flameCore, local, h * coreSize);
            }
        }

        private static void FitTiled(SpriteRenderer r, Vector2 size)
        {
            if (r == null || r.sprite == null) return;
            r.drawMode = SpriteDrawMode.Tiled;
            r.transform.localScale = Vector3.one;
            r.size = size;
        }

        private static void FitStretched(SpriteRenderer r, Vector2 size)
        {
            if (r == null || r.sprite == null) return;
            Vector2 s = r.sprite.bounds.size;
            r.transform.localScale = new Vector3(size.x / Mathf.Max(0.01f, s.x), size.y / Mathf.Max(0.01f, s.y), 1f);
        }

        private static void Place(SpriteRenderer r, Vector2 localPos, float diameter)
        {
            if (r == null || r.sprite == null) return;
            Vector3 p = r.transform.localPosition;
            r.transform.localPosition = new Vector3(localPos.x, localPos.y, p.z);
            float s = diameter / Mathf.Max(0.01f, r.sprite.bounds.size.y);
            r.transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
