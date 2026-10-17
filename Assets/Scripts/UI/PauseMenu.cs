using Shatranj.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/08 §7 pause menu: dim overlay with Resume · Restart from bench · Quit to menu. Opens whenever the game
    /// enters the Paused state (Esc / Start), closes on any other state. Works at timeScale 0 (UI is unscaled).
    /// Spawned by SceneBootstrap in gameplay scenes; built in code.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private GameObject root;
        private Button resume;
        private Button restart;
        private Button quit;

        public static PauseMenu Ensure()
        {
            PauseMenu existing = FindFirstObjectByType<PauseMenu>();
            if (existing != null) return existing;
            return new GameObject("[PauseMenu]").AddComponent<PauseMenu>();
        }

        private void Awake()
        {
            RectTransform canvas = UIFactory.Canvas("PauseCanvas", transform, 1000);
            root = canvas.gameObject;
            UIFactory.Image("Dim", canvas, null, new Color(0f, 0f, 0f, 0.6f)).raycastTarget = true;
            TextMeshProUGUI title = UIFactory.Text("Title", canvas, "PAUSED", 90, TextAlignmentOptions.Center, UIFactory.Paper);
            title.fontStyle = FontStyles.Bold;
            title.rectTransform.offsetMin = new Vector2(0f, 330f);

            resume = UIFactory.Button("Resume", canvas, "Resume", new Vector2(0f, 90f), OnResume);
            restart = UIFactory.Button("Restart", canvas, "Restart from bench", new Vector2(0f, -10f), OnRestart);
            quit = UIFactory.Button("Quit", canvas, "Quit to menu", new Vector2(0f, -110f), OnQuit);
            restart.GetComponent<RectTransform>().sizeDelta = new Vector2(420f, 80f);
            UIFactory.ChainVertical(resume, restart, quit);
            root.SetActive(false);
        }

        private void OnEnable() => GameEvents.OnGameStateChanged += HandleState;
        private void OnDisable() => GameEvents.OnGameStateChanged -= HandleState;

        private void HandleState(GameState previous, GameState next)
        {
            bool show = next == GameState.Paused;
            root.SetActive(show);
            if (!show) return;
            EventSystem es = UIFactory.EnsureEventSystem();
            es.SetSelectedGameObject(resume.gameObject);
        }

        private void OnResume()
        {
            if (GameManager.HasInstance) GameManager.Instance.Resume();
        }

        /// <summary>Reloads the scene: the player returns to the last bench; killed enemies stay dead (save kept in memory).</summary>
        private void OnRestart()
        {
            if (GameManager.HasInstance) GameManager.Instance.Resume();
            SceneFlow.ReloadActive(0.4f);
        }

        private void OnQuit()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.Resume();
                GameManager.Instance.SetState(GameState.Menu);
            }
            SceneFlow.Load(GameConstants.SceneMainMenu, 0.6f);
        }
    }
}
