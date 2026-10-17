using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Gameplay-scene bootstrap: puts the GameManager into Playing when the scene is opened directly in the editor
    /// (only if it is still in its initial Menu state, so normal flow is untouched) and makes sure the pause menu exists.
    /// A phase scene also declares its phase: opened directly (Menu state), Phase2_Castle switches the game to Phase 2
    /// before anything reads it — 7 hearts, dash + fireball, full heal, Theme_Phase2 and the Phase 2 flame (Docs/03 §7).
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameState stateOnStart = GameState.Playing;
        [SerializeField] private bool onlyIfInMenuState = true;
        [SerializeField] private bool createPauseMenu = true;
        [Tooltip("Phase this scene belongs to (1 or 2). 0 = don't care (Sandbox, test scenes).")]
        [SerializeField] private int scenePhase = 0;

        public int ScenePhase => scenePhase;

        private void Awake()
        {
            GameManager gm = GameManager.Ensure();
            if (scenePhase <= 0 || gm.State != GameState.Menu) return;
            if (gm.Phase != scenePhase) gm.SetPhase(scenePhase);
        }

        private void Start()
        {
            GameManager gm = GameManager.Ensure();
            if (createPauseMenu) UI.PauseMenu.Ensure();
            if (onlyIfInMenuState && gm.State != GameState.Menu) return;
            gm.SetState(stateOnStart);
        }
    }
}
