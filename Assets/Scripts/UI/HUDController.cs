using UnityEngine;

namespace Shatranj.UI
{
    /// <summary>
    /// Root of the HUD prefab (Docs/08 §6). Holds the sub-widgets; each widget subscribes to
    /// <see cref="Core.GameEvents"/> on its own, so the HUD never references gameplay objects.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [SerializeField] private Canvas canvas;
        [SerializeField] private HeartsUI hearts;
        [SerializeField] private SoulVesselUI soulVessel;
        [SerializeField] private BossBarUI bossBar;
        [SerializeField] private FlameArcIndicator flameArc;
        [SerializeField] private AbilityPopupUI abilityPopup;
        [SerializeField] private CycleLabelUI cycleLabel;
        [SerializeField] private InteractPromptUI interactPrompt;
        [SerializeField] private HudDebugPanel debugPanel;
        [SerializeField] private CanvasGroup rootGroup;

        public Canvas Canvas => canvas;
        public HeartsUI Hearts => hearts;
        public SoulVesselUI SoulVessel => soulVessel;
        public BossBarUI BossBar => bossBar;
        public FlameArcIndicator FlameArc => flameArc;
        public AbilityPopupUI AbilityPopup => abilityPopup;
        public CycleLabelUI CycleLabel => cycleLabel;
        public InteractPromptUI InteractPrompt => interactPrompt;
        public HudDebugPanel DebugPanel => debugPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (rootGroup == null) rootGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Hides the whole HUD (cutscenes, menus) with an optional fade.</summary>
        public void SetVisible(bool visible, float fadeTime = 0f)
        {
            if (rootGroup == null) { gameObject.SetActive(visible); return; }
            if (fadeTime <= 0f) { rootGroup.alpha = visible ? 1f : 0f; return; }
            Core.Tween.KillTweensOf(rootGroup);
            Core.Tween.Alpha(rootGroup, visible ? 1f : 0f, fadeTime, Core.Ease.Linear, unscaled: true);
        }
    }
}
