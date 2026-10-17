using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Gameplay-scene bootstrap: puts the GameManager into Playing when the scene is opened directly in the editor
    /// (only if it is still in its initial Menu state, so normal flow is untouched) and makes sure the pause menu exists.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameState stateOnStart = GameState.Playing;
        [SerializeField] private bool onlyIfInMenuState = true;
        [SerializeField] private bool createPauseMenu = true;

        private void Start()
        {
            GameManager gm = GameManager.Ensure();
            if (createPauseMenu) UI.PauseMenu.Ensure();
            if (onlyIfInMenuState && gm.State != GameState.Menu) return;
            gm.SetState(stateOnStart);
        }
    }
}
