#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shatranj.Core
{
    /// <summary>
    /// CLAUDE.md §6 debug hotkeys. Compiled only in the Editor and Development builds.
    /// Spawns itself after the first scene loads. Keys raise <see cref="DebugCommands"/> requests that
    /// later systems (player, rooms, enemies, flame) act on; F4 and F9 work from Prompt 1 on.
    /// </summary>
    public class DebugHotkeys : MonoBehaviour
    {
        private static DebugHotkeys instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Spawn()
        {
            if (instance != null) return;
            var go = new GameObject("[DebugHotkeys]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<DebugHotkeys>();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f1Key.wasPressedThisFrame)
            {
                DebugCommands.GodMode = !DebugCommands.GodMode;
                Log($"F1 god mode = {DebugCommands.GodMode}");
            }
            if (kb.f2Key.wasPressedThisFrame)
            {
                DebugCommands.RequestFullSoul();
                Log("F2 full soul");
            }
            if (kb.f3Key.wasPressedThisFrame)
            {
                DebugCommands.RequestSkipRoom();
                Log("F3 skip to next room");
            }
            if (kb.f4Key.wasPressedThisFrame)
            {
                // Toggle: Phase 1 → Phase 2 (dash, fireball, 7 hearts) → back to Phase 1.
                GameManager gm = GameManager.Ensure();
                int next = gm.Phase == GameConstants.Phase2 ? GameConstants.Phase1 : GameConstants.Phase2;
                gm.SetPhase(next);
                if (next == GameConstants.Phase2) DebugCommands.NotifyPhase2Applied();
                Log($"F4 phase {next} abilities: {gm.Abilities}");
            }
            bool shift = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            if (kb.f5Key.wasPressedThisFrame)
            {
                if (shift) { DebugCommands.RequestKillBoss(); Log("Shift+F5 kill the boss in this room"); }
                else { DebugCommands.RequestKillAllEnemies(); Log("F5 kill all enemies in room"); }
            }
            if (kb.f6Key.wasPressedThisFrame)
            {
                if (shift)
                {
                    // Jump straight to Phase 2 (as if Cutscene 2 had played): loop testing without replaying Phase 1.
                    GameManager gm = GameManager.Ensure();
                    gm.SetPhase(GameConstants.Phase2);
                    gm.Save.flameT = 0f;
                    gm.Save.lastBenchId = "";
                    gm.Save.lastBenchScene = "";
                    gm.SetState(GameState.Playing);
                    Log("Shift+F6 jump to Phase2_Castle");
                    SceneFlow.Load(GameConstants.ScenePhase2, 0.3f);
                }
                else { DebugCommands.RequestTeleportToBoss(); Log("F6 teleport to boss arena"); }
            }
            if (kb.f7Key.wasPressedThisFrame)
            {
                DebugCommands.ShowHitboxGizmos = !DebugCommands.ShowHitboxGizmos;
                Log($"F7 hitbox gizmos = {DebugCommands.ShowHitboxGizmos}");
            }
            if (kb.f8Key.wasPressedThisFrame)
            {
                DebugCommands.RequestFlameProgress(0.9f);
                Log("F8 flame progress = 0.9");
            }
            if (kb.f9Key.wasPressedThisFrame)
            {
                Log("F9 reload scene");
                SceneFlow.ReloadActive(0.2f);
            }
        }

        private static void Log(string message) => Debug.Log($"[Debug] {message}");
    }
}
#endif
