using Shatranj.Core;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace Shatranj.UI
{
    /// <summary>
    /// F10 debug panel (Prompt 5): fakes damage / heal / soul / boss / flame / cycle / ability events by raising
    /// <see cref="GameEvents"/> directly, so the HUD can be exercised without touching the player.
    /// Keys only work in the Editor and Development builds.
    /// </summary>
    public class HudDebugPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TextMeshProUGUI text;

        private int hearts = 5, maxHearts = 5;
        private int soul = 0, maxSoul = GameConstants.SoulMax;
        private int bossHp = 90, bossMax = 90;
        private bool bossShown;
        private float flameT;
        private int cycle = 1;
        private bool visible;

        private void OnEnable()
        {
            GameEvents.OnHealthChanged += HandleHealth;
            GameEvents.OnSoulChanged += HandleSoul;
            GameEvents.OnCycleChanged += HandleCycle;
            GameEvents.OnFlameProgressChanged += HandleFlame;
            if (group != null) group.alpha = 0f;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnHealthChanged -= HandleHealth;
            GameEvents.OnSoulChanged -= HandleSoul;
            GameEvents.OnCycleChanged -= HandleCycle;
            GameEvents.OnFlameProgressChanged -= HandleFlame;
        }

        private void HandleHealth(int c, int m) { hearts = c; maxHearts = m; }
        private void HandleSoul(int c, int m) { soul = c; maxSoul = m; }
        private void HandleCycle(int c) { cycle = c; }
        private void HandleFlame(float t) { flameT = t; }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f10Key.wasPressedThisFrame)
            {
                visible = !visible;
                if (group != null) group.alpha = visible ? 1f : 0f;
                Refresh();
            }
            if (!visible) return;

            if (kb.digit1Key.wasPressedThisFrame) { hearts = Mathf.Max(0, hearts - 1); GameEvents.RaiseHealthChanged(hearts, maxHearts); }
            if (kb.digit2Key.wasPressedThisFrame) { hearts = Mathf.Min(maxHearts, hearts + 1); GameEvents.RaiseHealthChanged(hearts, maxHearts); }
            if (kb.digit3Key.wasPressedThisFrame) { maxHearts = maxHearts == 5 ? 7 : 5; hearts = Mathf.Min(hearts, maxHearts); GameEvents.RaiseHealthChanged(hearts, maxHearts); }
            if (kb.digit4Key.wasPressedThisFrame) { soul = Mathf.Min(maxSoul, soul + GameConstants.SoulPerHit); GameEvents.RaiseSoulChanged(soul, maxSoul); }
            if (kb.digit5Key.wasPressedThisFrame) { soul = Mathf.Max(0, soul - GameConstants.SpellCost); GameEvents.RaiseSoulChanged(soul, maxSoul); }
            if (kb.digit6Key.wasPressedThisFrame)
            {
                if (!bossShown) { bossShown = true; bossHp = bossMax; GameEvents.RaiseBossStarted("debug_zulmat", "ZULMAT", new[] { 0.67f, 0.34f }); }
                else { bossShown = false; GameEvents.RaiseBossDefeated("debug_zulmat"); }
            }
            if (kb.digit7Key.wasPressedThisFrame && bossShown) { bossHp = Mathf.Max(0, bossHp - 9); GameEvents.RaiseBossHealthChanged("debug_zulmat", bossHp, bossMax); }
            if (kb.digit8Key.wasPressedThisFrame) { flameT = flameT >= 0.99f ? 0f : Mathf.Min(1f, flameT + 0.1f); GameEvents.RaiseFlameProgressChanged(flameT); }
            if (kb.digit9Key.wasPressedThisFrame) { GameEvents.RaiseAbilityUnlocked(GameEvents.AbilityDash); GameEvents.RaiseAbilityUnlocked(GameEvents.AbilityFireball); }
            if (kb.digit0Key.wasPressedThisFrame) { cycle = cycle >= 3 ? 1 : cycle + 1; GameEvents.RaiseCycleChanged(cycle); }
            if (kb.pKey.wasPressedThisFrame)
            {
                Vector2 at = Camera.main != null ? (Vector2)Camera.main.transform.position : Vector2.zero;
                GameEvents.RaiseInteractPromptShown("W — Rest", at);
            }
            if (kb.oKey.wasPressedThisFrame) GameEvents.RaiseInteractPromptHidden();

            Refresh();
#endif
        }

        private void Refresh()
        {
            if (text == null) return;
            text.text =
                "HUD DEBUG (F10)\n" +
                $"1/2 heart -/+   3 max 5<>7   [{hearts}/{maxHearts}]\n" +
                $"4 soul +11   5 soul -33   [{soul}/{maxSoul}]\n" +
                $"6 boss bar on/off   7 boss -9   [{(bossShown ? bossHp + "/" + bossMax : "off")}]\n" +
                $"8 flame t +0.1   [{flameT:0.0}]   (F4 toggles phase → arc visible)\n" +
                "9 ability popups   0 cycle label\n" +
                "P interact prompt   O hide prompt";
        }
    }
}
