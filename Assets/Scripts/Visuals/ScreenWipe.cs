using System.Collections;
using Shatranj.Core;
using Shatranj.Level;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Docs/07 §6 paper transitions, built in code: a quick parchment-coloured sweep on room changes (0.25 s) and the
    /// death burn wipe — an orange-rimmed circle closing on the player to black, then opening on the respawn point.
    /// Overlay canvas below the SceneFlow fade; unscaled time.
    /// </summary>
    public class ScreenWipe : MonoBehaviour
    {
        public static ScreenWipe Instance { get; private set; }

        [SerializeField] private float roomWipeTime = 0.25f;
        [SerializeField] private bool roomWipeEnabled = true;
        [SerializeField] private Color roomWipeColor = new Color(0.91f, 0.85f, 0.71f, 0.9f);

        private const int TextureSize = 256;
        private const float HoleRadiusPx = 10f;
        private const float RimWidthPx = 10f;
        private const float HoleFraction = HoleRadiusPx / TextureSize;   // hole radius / texture width

        private RectTransform canvasRect;
        private Image sweep;
        private Image hole;
        private Image black;
        private Coroutine sweepRoutine;
        private bool burning;
        private RoomManager roomManager;

        public bool IsBurning => burning;

        private void Awake()
        {
            Instance = this;
            Build();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (roomManager != null) roomManager.OnRoomChanged -= HandleRoomChanged;
        }

        private void Start()
        {
            roomManager = RoomManager.Instance;
            if (roomManager != null) roomManager.OnRoomChanged += HandleRoomChanged;
        }

        private void Build()
        {
            var canvasGo = new GameObject("WipeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasRect = (RectTransform)canvasGo.transform;

            sweep = MakeImage("Sweep", true);
            sweep.type = Image.Type.Filled;
            sweep.fillMethod = Image.FillMethod.Horizontal;
            sweep.sprite = PlaceholderSprites.Square;
            sweep.enabled = false;

            black = MakeImage("Black", true);
            black.color = Color.black;
            black.enabled = false;

            hole = MakeImage("BurnHole", false);
            hole.sprite = BuildHoleSprite();
            hole.enabled = false;
        }

        private Image MakeImage(string n, bool stretch)
        {
            var go = new GameObject(n, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvasRect, false);
            var r = (RectTransform)go.transform;
            if (stretch)
            {
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
                r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            }
            else
            {
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 0.5f);
            }
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Black square with a transparent circular hole and an orange→ember rim (the "burn").</summary>
        private static Sprite BuildHoleSprite()
        {
            var tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave,
            };
            var px = new Color32[TextureSize * TextureSize];
            var orange = new Color(1f, 0.55f, 0.15f, 1f);
            var ember = new Color(0.35f, 0.05f, 0.02f, 1f);
            float c = TextureSize * 0.5f;
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - c) * (x + 0.5f - c) + (y + 0.5f - c) * (y + 0.5f - c));
                    Color col;
                    if (d < HoleRadiusPx) col = new Color(0f, 0f, 0f, 0f);
                    else if (d < HoleRadiusPx + RimWidthPx)
                    {
                        float k = (d - HoleRadiusPx) / RimWidthPx;
                        col = k < 0.35f ? Color.Lerp(new Color(1f, 0.8f, 0.4f, Mathf.Lerp(0.3f, 1f, k / 0.35f)), orange, k / 0.35f)
                                        : Color.Lerp(orange, ember, (k - 0.35f) / 0.65f);
                        if (k > 0.85f) col = Color.Lerp(ember, Color.black, (k - 0.85f) / 0.15f);
                    }
                    else col = Color.black;
                    px[y * TextureSize + x] = col;
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 100f);
            s.hideFlags = HideFlags.DontSave;
            return s;
        }

        // ------------------------------------------------------------------
        // Room sweep
        // ------------------------------------------------------------------

        private bool skippedFirstRoom;

        private void HandleRoomChanged(RoomBounds room, bool instant)
        {
            if (!skippedFirstRoom) { skippedFirstRoom = true; return; }
            if (!roomWipeEnabled || instant || burning) return;
            AudioManager.Play("room_transition");
            Sweep(roomWipeColor, roomWipeTime);
        }

        /// <summary>Paper sweep: covers left→right, then uncovers left→right.</summary>
        public void Sweep(Color color, float duration)
        {
            if (sweepRoutine != null) StopCoroutine(sweepRoutine);
            sweepRoutine = StartCoroutine(SweepRoutine(color, duration));
        }

        private IEnumerator SweepRoutine(Color color, float duration)
        {
            sweep.color = color;
            sweep.enabled = true;
            float half = Mathf.Max(0.02f, duration * 0.5f);
            sweep.fillOrigin = (int)Image.OriginHorizontal.Left;
            for (float t = 0f; t < half; t += Time.unscaledDeltaTime) { sweep.fillAmount = Easing.Evaluate(Ease.OutQuad, t / half); yield return null; }
            sweep.fillOrigin = (int)Image.OriginHorizontal.Right;
            for (float t = 0f; t < half; t += Time.unscaledDeltaTime) { sweep.fillAmount = 1f - Easing.Evaluate(Ease.InQuad, t / half); yield return null; }
            sweep.enabled = false;
            sweepRoutine = null;
        }

        // ------------------------------------------------------------------
        // Death burn
        // ------------------------------------------------------------------

        /// <summary>Burn closes on <paramref name="world"/> until the screen is black.</summary>
        public IEnumerator BurnClose(Vector2 world, float duration)
        {
            burning = true;
            if (sweepRoutine != null) { StopCoroutine(sweepRoutine); sweep.enabled = false; }
            float maxR = MaxRadius();
            float minR = maxR / (0.5f / HoleFraction);   // below this the hole sprite no longer covers the screen
            PositionHole(world);
            hole.enabled = true;
            black.enabled = false;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = Easing.Evaluate(Ease.InQuad, t / duration);
                SetHoleRadius(Mathf.Lerp(maxR, minR, k));
                yield return null;
            }
            black.enabled = true;
            hole.enabled = false;
        }

        /// <summary>Burn opens from black around <paramref name="world"/>.</summary>
        public IEnumerator BurnOpen(Vector2 world, float duration)
        {
            float maxR = MaxRadius();
            float minR = maxR / (0.5f / HoleFraction);
            PositionHole(world);
            SetHoleRadius(minR);
            hole.enabled = true;
            black.enabled = false;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = Easing.Evaluate(Ease.OutQuad, t / duration);
                SetHoleRadius(Mathf.Lerp(minR, maxR, k));
                yield return null;
            }
            hole.enabled = false;
            burning = false;
        }

        private float MaxRadius()
        {
            Vector2 size = canvasRect.rect.size;
            return size.magnitude;
        }

        private void PositionHole(Vector2 world)
        {
            Camera cam = Camera.main;
            Vector2 screen = cam != null ? (Vector2)cam.WorldToScreenPoint(world) : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
            hole.rectTransform.anchoredPosition = local;
        }

        private void SetHoleRadius(float radius)
        {
            float size = radius / HoleFraction;
            hole.rectTransform.sizeDelta = new Vector2(size, size);
        }
    }
}
