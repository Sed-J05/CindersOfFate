using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Singleton, DontDestroyOnLoad (Docs/02 §4). Owns game state, phase, cycle, ability flags, the in-memory
    /// save and the cutscene hand-off (<see cref="PendingCutscene"/> / <see cref="NextSceneAfterCutscene"/>).
    /// Auto-created before the first scene loads so every scene (Sandbox included) can rely on it.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        public GameState State { get; private set; } = GameState.Menu;
        public int Phase { get; private set; } = GameConstants.Phase1;
        public int Cycle { get; private set; } = 1;
        public PlayerAbilities Abilities { get; private set; } = PlayerAbilities.ForPhase(GameConstants.Phase1);

        /// <summary>Cutscene id the Cutscene scene should play next (Docs/02 §8).</summary>
        public string PendingCutscene { get; set; }
        /// <summary>Scene the Cutscene scene loads when it finishes.</summary>
        public string NextSceneAfterCutscene { get; set; }

        /// <summary>Title card shown once when the next level scene starts ("PHASE II — THE INVERTED THEATRE").</summary>
        public string PendingTitleCard { get; set; }

        /// <summary>Live session data. Written to disk by <see cref="WriteSave"/>.</summary>
        public SaveData Save { get; private set; } = new SaveData();

        public bool IsPaused => State == GameState.Paused;
        public bool IsPlaying => State == GameState.Playing;
        public bool InputEnabled => State == GameState.Playing;

        private GameState stateBeforePause = GameState.Playing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate() => Ensure();

        /// <summary>Returns the instance, creating it if the scene didn't contain one.</summary>
        public static GameManager Ensure()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("[GameManager]");
            return go.AddComponent<GameManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ------------------------------------------------------------------
        // State
        // ------------------------------------------------------------------

        public void SetState(GameState next)
        {
            if (State == next) return;
            GameState previous = State;
            State = next;

            if (next == GameState.Paused)
            {
                HitStop.Cancel();
                Time.timeScale = 0f;
            }
            else if (previous == GameState.Paused || (next == GameState.Playing && !HitStop.IsActive))
            {
                // Safety: never leave the game frozen when returning to play / menus.
                Time.timeScale = 1f;
            }

            GameEvents.RaiseGameStateChanged(previous, next);
        }

        public void Pause()
        {
            if (State != GameState.Playing) return;
            stateBeforePause = State;
            SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            SetState(stateBeforePause);
        }

        public void TogglePause()
        {
            if (State == GameState.Paused) Resume();
            else Pause();
        }

        // ------------------------------------------------------------------
        // Phase / cycle / abilities
        // ------------------------------------------------------------------

        /// <summary>Sets the phase and applies that phase's default abilities (Docs/03 §9).</summary>
        public void SetPhase(int phase, bool raiseEvents = true)
        {
            Phase = Mathf.Clamp(phase, GameConstants.Phase1, GameConstants.Phase2);
            Abilities = PlayerAbilities.ForPhase(Phase);
            Save.phase = Phase;
            Save.FromAbilities(Abilities);
            if (raiseEvents)
            {
                GameEvents.RaisePhaseChanged(Phase);
                GameEvents.RaiseAbilitiesChanged(Abilities);
            }
        }

        /// <summary>Overrides abilities without changing the phase (debug / tuning).</summary>
        public void SetAbilities(PlayerAbilities abilities)
        {
            Abilities = abilities.Clone();
            Save.FromAbilities(Abilities);
            GameEvents.RaiseAbilitiesChanged(Abilities);
        }

        /// <summary>Docs/02 §9: Cycle++, back to phase 1, progress cleared, cycle number kept.</summary>
        public void AdvanceCycle()
        {
            Cycle++;
            Save.ClearProgressForNewCycle();
            Save.cycle = Cycle;
            SetPhase(GameConstants.Phase1);
            GameEvents.RaiseCycleChanged(Cycle);
            WriteSave();
        }

        // ------------------------------------------------------------------
        // New game / continue / save
        // ------------------------------------------------------------------

        /// <summary>Fresh save, cycle 1, phase 1. Queues the intro cutscene before Phase 1.</summary>
        public void StartNewGame()
        {
            Cycle = 1;
            Save = SaveSystem.CreateNew();
            SetPhase(GameConstants.Phase1, raiseEvents: false);
            PendingCutscene = GameConstants.CutsceneIntro;
            NextSceneAfterCutscene = GameConstants.ScenePhase1;
            GameEvents.RaiseCycleChanged(Cycle);
            GameEvents.RaisePhaseChanged(Phase);
            GameEvents.RaiseAbilitiesChanged(Abilities);
        }

        /// <summary>Loads the save from disk if it exists and applies it. Returns false when there is none.</summary>
        public bool TryContinue()
        {
            SaveData data = SaveSystem.Load();
            if (data == null) return false;
            ApplySave(data);
            return true;
        }

        public void ApplySave(SaveData data)
        {
            Save = data ?? SaveSystem.CreateNew();
            Cycle = Mathf.Max(1, Save.cycle);
            Phase = Mathf.Clamp(Save.phase, GameConstants.Phase1, GameConstants.Phase2);
            Abilities = Save.ToAbilities();
            PendingCutscene = null;
            NextSceneAfterCutscene = Phase == GameConstants.Phase2 ? GameConstants.ScenePhase2 : GameConstants.ScenePhase1;
            GameEvents.RaiseCycleChanged(Cycle);
            GameEvents.RaisePhaseChanged(Phase);
            GameEvents.RaiseAbilitiesChanged(Abilities);
        }

        /// <summary>Syncs runtime fields into <see cref="Save"/> and writes it to disk.</summary>
        public bool WriteSave()
        {
            Save.phase = Phase;
            Save.cycle = Cycle;
            Save.FromAbilities(Abilities);
            return SaveSystem.Save(Save);
        }

        /// <summary>Sets a save flag (e.g. "door_orb") and notifies listeners such as doors.</summary>
        public void SetFlag(string flag, bool value)
        {
            if (string.IsNullOrEmpty(flag)) return;
            Save.SetFlag(flag, value);
            GameEvents.RaiseFlagChanged(flag, value);
        }

        /// <summary>The scene the player should be in for the current phase.</summary>
        public string CurrentPhaseScene => Phase == GameConstants.Phase2 ? GameConstants.ScenePhase2 : GameConstants.ScenePhase1;

        // ------------------------------------------------------------------
        // Cutscene hand-off (Docs/02 §8)
        // ------------------------------------------------------------------

        /// <summary>Records which cutscene to play and where to go after it, and enters the Cutscene state.</summary>
        public void BeginCutscene(string cutsceneId, string nextScene)
        {
            PendingCutscene = cutsceneId;
            NextSceneAfterCutscene = nextScene;
            SetState(GameState.Cutscene);
            GameEvents.RaiseCutsceneStart(cutsceneId);
        }

        /// <summary>Clears the pending cutscene and returns to Playing. The caller loads <see cref="NextSceneAfterCutscene"/>.</summary>
        public string EndCutscene()
        {
            string id = PendingCutscene;
            PendingCutscene = null;
            SetState(GameState.Playing);
            GameEvents.RaiseCutsceneEnd(id);
            return NextSceneAfterCutscene;
        }
    }
}
