using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Visual driver. Until Prompt 4 wires real sprite animations, it tints the placeholder sprite per state,
    /// flips the visual child by facing and squashes on landing so playback is readable.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float landSquashTime = 0.08f;
        [SerializeField] private Vector3 landSquashScale = new Vector3(1.15f, 0.8f, 1f);
        [SerializeField] private Vector3 jumpStretchScale = new Vector3(0.9f, 1.12f, 1f);

        private Vector3 baseScale = Vector3.one;
        private int facing = 1;
        private float blinkRemaining;
        private float blinkHz = 10f;
        private GameObject healAura;

        /// <summary>True once real animation clips are driving the sprite (Prompt 4 sets this).</summary>
        public bool HasClips { get; set; }

        private void Awake()
        {
            if (controller == null) controller = GetComponent<PlayerController>();
            if (visualRoot == null && spriteRenderer != null) visualRoot = spriteRenderer.transform;
            if (visualRoot != null) baseScale = visualRoot.localScale;
        }

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.OnStateChanged += HandleStateChanged;
                controller.OnLanded += HandleLanded;
                controller.OnJumped += HandleJumped;
                ApplyStateTint(controller.State);
            }
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnStateChanged -= HandleStateChanged;
                controller.OnLanded -= HandleLanded;
                controller.OnJumped -= HandleJumped;
            }
        }

        public void SetFacing(int direction)
        {
            facing = direction >= 0 ? 1 : -1;
            ApplyScale(baseScale);
        }

        private void Update()
        {
            if (blinkRemaining <= 0f || visualRoot == null) return;
            blinkRemaining -= Time.deltaTime;
            if (blinkRemaining <= 0f)
            {
                visualRoot.gameObject.SetActive(true);
                return;
            }
            // Docs/03 §7: sprite blinks at 10 Hz during i-frames (whole visual, so the eye blinks too).
            bool visible = Mathf.FloorToInt(blinkRemaining * blinkHz * 2f) % 2 == 0;
            if (visualRoot.gameObject.activeSelf != visible) visualRoot.gameObject.SetActive(visible);
        }

        /// <summary>Starts the i-frame blink.</summary>
        public void StartBlink(float duration, float hz)
        {
            blinkRemaining = duration;
            blinkHz = Mathf.Max(1f, hz);
        }

        public void StopBlink()
        {
            blinkRemaining = 0f;
            if (visualRoot != null) visualRoot.gameObject.SetActive(true);
        }

        /// <summary>Placeholder heal aura: a soft green disc behind the puppet while charging.</summary>
        public void SetHealAura(bool on)
        {
            if (on && healAura == null)
            {
                healAura = new GameObject("HealAura");
                healAura.transform.SetParent(transform, false);
                healAura.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                healAura.transform.localScale = Vector3.one * 2.4f;
                var sr = healAura.AddComponent<SpriteRenderer>();
                sr.sprite = Visuals.PlaceholderSprites.Circle;
                sr.color = new Color(0.45f, 1f, 0.65f, 0.28f);
                sr.sortingLayerName = GameLayers.Sorting.FX;
                sr.sortingOrder = -1;
            }
            if (healAura != null)
            {
                healAura.SetActive(on);
                if (on)
                {
                    Tween.KillTweensOf(healAura.transform);
                    healAura.transform.localScale = Vector3.one * 1.6f;
                    Tween.Scale(healAura.transform, 2.6f, 0.9f, Ease.OutQuad);
                }
            }
        }

        private void ApplyScale(Vector3 scale)
        {
            if (visualRoot == null) return;
            scale.x = Mathf.Abs(scale.x) * facing;
            visualRoot.localScale = scale;
        }

        private void HandleStateChanged(PlayerState previous, PlayerState next) => ApplyStateTint(next);

        private void HandleLanded()
        {
            if (visualRoot == null) return;
            Tween.KillTweensOf(visualRoot);
            ApplyScale(Vector3.Scale(baseScale, landSquashScale));
            Tween.Scale(visualRoot, Flip(baseScale), landSquashTime, Ease.OutQuad);
        }

        private void HandleJumped()
        {
            if (visualRoot == null) return;
            Tween.KillTweensOf(visualRoot);
            ApplyScale(Vector3.Scale(baseScale, jumpStretchScale));
            Tween.Scale(visualRoot, Flip(baseScale), 0.12f, Ease.OutQuad);
        }

        private Vector3 Flip(Vector3 scale)
        {
            scale.x = Mathf.Abs(scale.x) * facing;
            return scale;
        }

        private void ApplyStateTint(PlayerState state)
        {
            if (HasClips || spriteRenderer == null) return;
            spriteRenderer.color = TintFor(state);
        }

        /// <summary>Placeholder palette: protagonist blue/white family, with clear reads for special states.</summary>
        public static Color TintFor(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Idle:       return new Color(0.92f, 0.96f, 1f);
                case PlayerState.Run:        return new Color(0.31f, 0.64f, 1f);
                case PlayerState.JumpRise:   return new Color(0.55f, 0.85f, 1f);
                case PlayerState.Fall:       return new Color(0.20f, 0.42f, 0.80f);
                case PlayerState.Land:       return new Color(0.75f, 0.90f, 1f);
                case PlayerState.AttackSide: return new Color(1f, 0.95f, 0.55f);
                case PlayerState.AttackUp:   return new Color(1f, 0.85f, 0.45f);
                case PlayerState.Cast:       return new Color(1f, 0.60f, 0.25f);
                case PlayerState.HealCharge: return new Color(0.45f, 1f, 0.65f);
                case PlayerState.Dash:       return new Color(0.50f, 1f, 1f);
                case PlayerState.Hurt:       return new Color(1f, 0.30f, 0.30f);
                case PlayerState.Dead:       return new Color(0.15f, 0.15f, 0.20f);
                case PlayerState.Cutscene:   return new Color(0.70f, 0.70f, 0.75f);
                case PlayerState.Resting:    return new Color(1f, 0.80f, 0.60f);
                default:                     return Color.white;
            }
        }
    }
}
