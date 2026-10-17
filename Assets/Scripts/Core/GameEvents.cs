using System;
using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Global static C# events (Docs/02 §4). Systems subscribe in OnEnable and unsubscribe in OnDisable.
    /// Raise* helpers null-check so callers never do. <see cref="ClearAllSubscribers"/> exists for tests and hard resets.
    /// </summary>
    public static class GameEvents
    {
        // --- Player ---
        public static event Action<DamageInfo> OnPlayerDamaged;
        public static event Action OnPlayerDied;
        /// <summary>(current, max) hearts.</summary>
        public static event Action<int, int> OnHealthChanged;
        /// <summary>(current, max) soul.</summary>
        public static event Action<int, int> OnSoulChanged;
        public static event Action<PlayerAbilities> OnAbilitiesChanged;

        // --- Enemies / bosses ---
        /// <summary>Stable enemy id (roomId_glyph_index) used by the kill-save.</summary>
        public static event Action<string> OnEnemyKilled;
        /// <summary>(bossId, displayName, stageThresholds as HP fractions, e.g. {0.67, 0.34}; empty for single-stage bosses)</summary>
        public static event Action<string, string, float[]> OnBossStarted;
        /// <summary>(bossId, currentHp, maxHp)</summary>
        public static event Action<string, int, int> OnBossHealthChanged;
        public static event Action<string> OnBossDefeated;
        public static event Action OnFinalBossDefeated;

        // --- World / flow ---
        public static event Action<string> OnRoomEntered;
        public static event Action<string> OnBenchRested;
        public static event Action OnOrbTouched;
        public static event Action<int> OnPhaseChanged;
        public static event Action<int> OnCycleChanged;
        /// <summary>(previous, next)</summary>
        public static event Action<GameState, GameState> OnGameStateChanged;
        public static event Action<string> OnCutsceneStart;
        public static event Action<string> OnCutsceneEnd;

        // --- Visuals / HUD ---
        /// <summary>Flame arc progress t in [0,1] (Docs/07 §3). Raised by the FlameController, shown by the HUD.</summary>
        public static event Action<float> OnFlameProgressChanged;
        /// <summary>An ability was unlocked during play ("dash" / "fireball"); the HUD shows the popup (Docs/08 §6).</summary>
        public static event Action<string> OnAbilityUnlocked;
        /// <summary>(text, world position) — show "W — Rest" style prompt above an interactable.</summary>
        public static event Action<string, Vector2> OnInteractPromptShown;
        public static event Action OnInteractPromptHidden;

        public const string AbilityDash = "dash";
        public const string AbilityFireball = "fireball";

        /// <summary>(flag, value) — a save flag changed (doors listen).</summary>
        public static event Action<string, bool> OnFlagChanged;

        /// <summary>Last value passed to <see cref="RaiseFlameProgressChanged"/>; benches store it in the checkpoint.</summary>
        public static float LastFlameProgress { get; private set; }

        // --- Raise helpers ---
        public static void RaisePlayerDamaged(DamageInfo info) => OnPlayerDamaged?.Invoke(info);
        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaiseHealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
        public static void RaiseSoulChanged(int current, int max) => OnSoulChanged?.Invoke(current, max);
        public static void RaiseAbilitiesChanged(PlayerAbilities abilities) => OnAbilitiesChanged?.Invoke(abilities);

        public static void RaiseEnemyKilled(string enemyId) => OnEnemyKilled?.Invoke(enemyId);
        public static void RaiseBossStarted(string bossId, string displayName, float[] stageThresholds = null)
            => OnBossStarted?.Invoke(bossId, displayName, stageThresholds ?? System.Array.Empty<float>());
        public static void RaiseBossHealthChanged(string bossId, int current, int max) => OnBossHealthChanged?.Invoke(bossId, current, max);
        public static void RaiseBossDefeated(string bossId) => OnBossDefeated?.Invoke(bossId);
        public static void RaiseFinalBossDefeated() => OnFinalBossDefeated?.Invoke();

        public static void RaiseRoomEntered(string roomId) => OnRoomEntered?.Invoke(roomId);
        public static void RaiseBenchRested(string benchId) => OnBenchRested?.Invoke(benchId);
        public static void RaiseOrbTouched() => OnOrbTouched?.Invoke();
        public static void RaisePhaseChanged(int phase) => OnPhaseChanged?.Invoke(phase);
        public static void RaiseCycleChanged(int cycle) => OnCycleChanged?.Invoke(cycle);
        public static void RaiseGameStateChanged(GameState previous, GameState next) => OnGameStateChanged?.Invoke(previous, next);
        public static void RaiseCutsceneStart(string id) => OnCutsceneStart?.Invoke(id);
        public static void RaiseCutsceneEnd(string id) => OnCutsceneEnd?.Invoke(id);

        public static void RaiseFlameProgressChanged(float t)
        {
            LastFlameProgress = Mathf.Clamp01(t);
            OnFlameProgressChanged?.Invoke(LastFlameProgress);
        }

        public static void RaiseFlagChanged(string flag, bool value) => OnFlagChanged?.Invoke(flag, value);
        public static void RaiseAbilityUnlocked(string abilityId) => OnAbilityUnlocked?.Invoke(abilityId);
        public static void RaiseInteractPromptShown(string text, Vector2 worldPosition) => OnInteractPromptShown?.Invoke(text, worldPosition);
        public static void RaiseInteractPromptHidden() => OnInteractPromptHidden?.Invoke();

        /// <summary>Drops every subscriber. Tests and full-game resets only; never call during normal play.</summary>
        public static void ClearAllSubscribers()
        {
            OnPlayerDamaged = null;
            OnPlayerDied = null;
            OnHealthChanged = null;
            OnSoulChanged = null;
            OnAbilitiesChanged = null;
            OnEnemyKilled = null;
            OnBossStarted = null;
            OnBossHealthChanged = null;
            OnBossDefeated = null;
            OnFinalBossDefeated = null;
            OnRoomEntered = null;
            OnBenchRested = null;
            OnOrbTouched = null;
            OnPhaseChanged = null;
            OnCycleChanged = null;
            OnGameStateChanged = null;
            OnCutsceneStart = null;
            OnCutsceneEnd = null;
            OnFlameProgressChanged = null;
            OnAbilityUnlocked = null;
            OnInteractPromptShown = null;
            OnInteractPromptHidden = null;
            OnFlagChanged = null;
        }
    }
}
