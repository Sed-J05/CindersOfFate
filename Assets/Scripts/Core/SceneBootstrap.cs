using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Puts the GameManager into the right state when a scene is opened directly in the editor
    /// (Sandbox, LevelTest, a phase scene pressed Play on). Harmless when the game flows in normally.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameState stateOnStart = GameState.Playing;
        [SerializeField] private bool onlyIfInMenuState = true;

        private void Start()
        {
            GameManager gm = GameManager.Ensure();
            if (onlyIfInMenuState && gm.State != GameState.Menu) return;
            gm.SetState(stateOnStart);
        }
    }
}
