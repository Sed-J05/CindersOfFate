using System;

namespace Shatranj.Core
{
    /// <summary>
    /// Flags and requests raised by <see cref="DebugHotkeys"/> (CLAUDE.md §6). Gameplay systems that exist
    /// later (player, rooms, enemies, flame) subscribe here; this class compiles in every build so
    /// subscribers never need #if guards. Nothing here is raised in release builds.
    /// </summary>
    public static class DebugCommands
    {
        private static bool godMode;
        private static bool showHitboxGizmos;

        /// <summary>F1 — player takes no damage.</summary>
        public static bool GodMode
        {
            get => godMode;
            set { if (godMode == value) return; godMode = value; OnGodModeChanged?.Invoke(value); }
        }

        /// <summary>F7 — draw attack/hurt boxes.</summary>
        public static bool ShowHitboxGizmos
        {
            get => showHitboxGizmos;
            set { if (showHitboxGizmos == value) return; showHitboxGizmos = value; OnHitboxGizmosChanged?.Invoke(value); }
        }

        public static event Action<bool> OnGodModeChanged;
        public static event Action<bool> OnHitboxGizmosChanged;
        /// <summary>F2</summary>
        public static event Action OnFullSoulRequested;
        /// <summary>F3</summary>
        public static event Action OnSkipRoomRequested;
        /// <summary>F4 (after GameManager applied Phase 2)</summary>
        public static event Action OnPhase2Applied;
        /// <summary>F5</summary>
        public static event Action OnKillAllEnemiesRequested;
        /// <summary>F6</summary>
        public static event Action OnTeleportToBossRequested;
        /// <summary>F8 — value is the requested flame progress t.</summary>
        public static event Action<float> OnFlameProgressRequested;

        public static void RequestFullSoul() => OnFullSoulRequested?.Invoke();
        public static void RequestSkipRoom() => OnSkipRoomRequested?.Invoke();
        public static void NotifyPhase2Applied() => OnPhase2Applied?.Invoke();
        public static void RequestKillAllEnemies() => OnKillAllEnemiesRequested?.Invoke();

        /// <summary>Shift+F5: kill the boss fighting in the current room (loop testing, Prompt 16).</summary>
        public static event Action OnKillBossRequested;
        public static void RequestKillBoss() => OnKillBossRequested?.Invoke();
        public static void RequestTeleportToBoss() => OnTeleportToBossRequested?.Invoke();
        public static void RequestFlameProgress(float t) => OnFlameProgressRequested?.Invoke(t);
    }
}
