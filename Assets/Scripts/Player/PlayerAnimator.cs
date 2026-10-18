using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Visual driver. With a Protagonist <see cref="SpriteAnimationSet"/> (Prompt 4) it plays the Docs/06 clips per
    /// state; without one it falls back to tinting the placeholder box. Also: facing flip on the visual child,
    /// land squash / jump stretch, i-frame blink, heal aura.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteAnimator spriteAnimator;
        [SerializeField] private SpriteAnimationSet animationSet;
        [Tooltip("Phase 2 look (black/red protagonist art). Falls back to the Phase 1 set + theme recolour if empty.")]
        [SerializeField] private SpriteAnimationSet phase2Set;
        [SerializeField] private float landSquashTime = 0.08f;
        [SerializeField] private Vector3 landSquashScale = new Vector3(1.15f, 0.8f, 1f);
        [SerializeField] private Vector3 jumpStretchScale = new Vector3(0.9f, 1.12f, 1f);

        public const string ProtagonistSetName = "Protagonist";
        public const string Phase2SetName = "Protagonist_P2";

        private PlayerCombat combat;
        private Vector3 baseScale = Vector3.one;
        private int facing = 1;
        private float blinkRemaining;
        private float blinkHz = 10f;
        private GameObject healAura;
        private string overrideClip;

        /// <summary>True once real animation clips drive the sprite; tint fallback is then off.</summary>
        public bool HasClips { get; private set; }

        private void Awake()
        {
            if (controller == null) controller = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            if (visualRoot == null && spriteRenderer != null) visualRoot = spriteRenderer.transform;
            if (spriteRenderer == null && visualRoot != null) spriteRenderer = visualRoot.GetComponent<SpriteRenderer>();
            if (visualRoot != null) baseScale = visualRoot.localScale;

            TryBindClips();
        }

        /// <summary>Finds the animation set (serialized or Resources/Animations/Protagonist) and switches to clip mode.</summary>
        private void TryBindClips()
        {
            if (visualRoot == null || spriteRenderer == null) return;

            if (animationSet == null) animationSet = SpriteAnimationSet.Load(ProtagonistSetName);
            if (animationSet == null || !animationSet.HasAnyFrames) return;
            if (phase2Set == null) phase2Set = SpriteAnimationSet.Load(Phase2SetName);

            if (spriteAnimator == null) spriteAnimator = visualRoot.GetComponent<SpriteAnimator>();
            if (spriteAnimator == null) spriteAnimator = visualRoot.gameObject.AddComponent<SpriteAnimator>();
            spriteAnimator.Set = SetForPhase(GameManager.HasInstance ? GameManager.Instance.Phase : GameConstants.Phase1);
            spriteAnimator.OnClipFinished += HandleClipFinished;

            // Real sprites: 256×256 canvas at 100 PPU with a bottom pivot → scale 1, feet at the root.
            visualRoot.localPosition = Vector3.zero;
            baseScale = Vector3.one;
            ApplyScale(baseScale);
            spriteRenderer.color = Color.white;
            for (int i = 0; i < visualRoot.childCount; i++) visualRoot.GetChild(i).gameObject.SetActive(false); // placeholder eye
            HasClips = true;
        }

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.OnStateChanged += HandleStateChanged;
                controller.OnLanded += HandleLanded;
                controller.OnJumped += HandleJumped;
                ApplyState(controller.State);
            }
            GameEvents.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnStateChanged -= HandleStateChanged;
                controller.OnLanded -= HandleLanded;
                controller.OnJumped -= HandleJumped;
            }
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
        }

        /// <summary>Phase 2 swaps to the black/red art when it exists (Phase 1 set otherwise).</summary>
        private SpriteAnimationSet SetForPhase(int phase)
        {
            bool p2 = phase == GameConstants.Phase2 && phase2Set != null && phase2Set.HasAnyFrames;
            return p2 ? phase2Set : animationSet;
        }

        private void HandlePhaseChanged(int phase)
        {
            if (!HasClips || spriteAnimator == null) return;
            SpriteAnimationSet target = SetForPhase(phase);
            if (spriteAnimator.Set == target) return;
            spriteAnimator.Set = target;
            if (controller != null) ApplyState(controller.State);
        }

        private void OnDestroy()
        {
            if (spriteAnimator != null) spriteAnimator.OnClipFinished -= HandleClipFinished;
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

        // ------------------------------------------------------------------
        // Facing / squash
        // ------------------------------------------------------------------

        public void SetFacing(int direction)
        {
            facing = direction >= 0 ? 1 : -1;
            ApplyScale(baseScale);
        }

        private void ApplyScale(Vector3 scale)
        {
            if (visualRoot == null) return;
            scale.x = Mathf.Abs(scale.x) * facing;
            visualRoot.localScale = scale;
        }

        private Vector3 Flip(Vector3 scale)
        {
            scale.x = Mathf.Abs(scale.x) * facing;
            return scale;
        }

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

        // ------------------------------------------------------------------
        // State → clip / tint
        // ------------------------------------------------------------------

        private void HandleStateChanged(PlayerState previous, PlayerState next) => ApplyState(next);

        private void ApplyState(PlayerState state)
        {
            if (HasClips) PlayClipForState(state);
            else if (spriteRenderer != null) spriteRenderer.color = TintFor(state);
        }

        private void PlayClipForState(PlayerState state)
        {
            overrideClip = null;
            string clip = ClipFor(state, out bool restart);
            if (!spriteAnimator.Play(clip, restart) && clip != "Idle") spriteAnimator.Play("Idle");
        }

        /// <summary>Docs/06 §3.1 clip names per Docs/03 §8 state.</summary>
        private string ClipFor(PlayerState state, out bool restart)
        {
            restart = false;
            switch (state)
            {
                case PlayerState.Idle:       return "Idle";
                case PlayerState.Run:        return "Run";
                case PlayerState.JumpRise:   restart = true; return "Jump_Rise";
                case PlayerState.Fall:       return "Jump_Fall";
                case PlayerState.Land:       restart = true; return "Land";
                case PlayerState.AttackSide: restart = true; return combat != null && combat.LastSwingWasB ? "Attack_Side_B" : "Attack_Side_A";
                case PlayerState.AttackUp:   restart = true; return "Attack_Up";
                case PlayerState.Cast:       restart = true; return "Cast";
                case PlayerState.HealCharge: restart = true; return "Heal_Charge";
                case PlayerState.Dash:       restart = true; return "Dash";
                case PlayerState.Hurt:       restart = true; return "Hurt";
                case PlayerState.Dead:       restart = true; return "Death";
                case PlayerState.Resting:    return "Rest";
                case PlayerState.Cutscene:   return "Idle";
                default:                     return "Idle";
            }
        }

        /// <summary>Plays a one-shot clip (e.g. Heal_Release); the state clip resumes when it ends.</summary>
        public void PlayOverride(string clipName)
        {
            if (!HasClips || !spriteAnimator.HasClip(clipName)) return;
            overrideClip = clipName;
            spriteAnimator.Play(clipName, restart: true);
        }

        /// <summary>Cutscene hook: play any protagonist clip directly (loops stay until the next request).</summary>
        public bool PlayClip(string clipName, bool restart = false)
        {
            if (!HasClips) return false;
            overrideClip = null;
            return spriteAnimator.Play(clipName, restart);
        }

        private void HandleClipFinished(string clipName)
        {
            if (overrideClip != null && clipName == overrideClip)
            {
                overrideClip = null;
                if (controller != null) PlayClipForState(controller.State);
            }
        }

        // ------------------------------------------------------------------
        // Blink / aura
        // ------------------------------------------------------------------

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
                sr.sprite = PlaceholderSprites.Circle;
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
