using Shatranj.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/08 §7 main menu over the parchment stage: Title_Logo, Start (new game → Cutscene 1 → Phase 1),
    /// Continue (only with a save; loads the saved phase scene at the last bench), Quit. Built in code; keyboard,
    /// gamepad and mouse navigation via the Input System UI module.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private float fadeTime = 0.8f;

        private Button startButton;
        private Button continueButton;
        private Button quitButton;
        private bool leaving;

        private void Start()
        {
            AudioManager.PlayMusic("music_menu");
            GameManager gm = GameManager.Ensure();
            gm.SetState(GameState.Menu);
            Time.timeScale = 1f;
            Build();
        }

        private void Build()
        {
            RectTransform canvas = UIFactory.Canvas("MainMenuCanvas", transform, 100);

            Sprite logo = UIFactory.CatalogSprite("Title_Logo");
            if (logo != null)
            {
                Image img = UIFactory.Image("Logo", canvas, logo, Color.white, stretch: false);
                img.rectTransform.anchorMin = img.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                img.rectTransform.pivot = new Vector2(0.5f, 1f);
                img.rectTransform.anchoredPosition = new Vector2(0f, -60f);
                img.rectTransform.sizeDelta = new Vector2(1200f, 400f);
            }
            else
            {
                TextMeshProUGUI t = UIFactory.Text("Logo", canvas, "SHATRANJ", 140, TextAlignmentOptions.Top, UIFactory.Ink);
                t.rectTransform.offsetMax = new Vector2(0f, -120f);
                t.fontStyle = FontStyles.Bold;
            }

            RectTransform buttons = UIFactory.Rect("Buttons", canvas, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(400f, 320f));
            startButton = UIFactory.Button("Start", buttons, "Start", new Vector2(0f, 100f), OnStart);
            continueButton = UIFactory.Button("Continue", buttons, "Continue", new Vector2(0f, 0f), OnContinue);
            quitButton = UIFactory.Button("Quit", buttons, "Quit", new Vector2(0f, -100f), OnQuit);

            bool hasSave = SaveSystem.Exists;
            continueButton.interactable = hasSave;
            continueButton.GetComponentInChildren<TextMeshProUGUI>().alpha = hasSave ? 1f : 0.35f;
            UIFactory.ChainVertical(startButton, continueButton, quitButton);

            EventSystem es = UIFactory.EnsureEventSystem();
            es.SetSelectedGameObject((hasSave ? continueButton : startButton).gameObject);

            TextMeshProUGUI hint = UIFactory.Text("Hint", canvas, "W/S or arrows · Enter / Space · gamepad A", 22, TextAlignmentOptions.Bottom, new Color(0.13f, 0.11f, 0.10f, 0.6f));
            hint.rectTransform.offsetMin = new Vector2(0f, 24f);
        }

        private void OnStart()
        {
            if (leaving) return;
            leaving = true;
            GameManager gm = GameManager.Ensure();
            gm.StartNewGame();
            if (Application.CanStreamedLevelBeLoaded(GameConstants.SceneCutscene))
            {
                gm.BeginCutscene(GameConstants.CutsceneIntro, GameConstants.ScenePhase1);
                SceneFlow.Load(GameConstants.SceneCutscene, fadeTime);
            }
            else
            {
                gm.SetState(GameState.Playing);
                SceneFlow.Load(GameConstants.ScenePhase1, fadeTime);
            }
        }

        private void OnContinue()
        {
            if (leaving) return;
            GameManager gm = GameManager.Ensure();
            if (!gm.TryContinue()) return;
            leaving = true;
            SceneFlow.Load(ContinueTarget(gm), fadeTime);
        }

        /// <summary>
        /// Where Continue goes (Prompt 16): an unfinished story cutscene first (cs2 / cs3 / the post-loop cs1), otherwise the
        /// saved bench — but only if it belongs to the current phase's scene — otherwise the phase scene's start.
        /// </summary>
        public static string ContinueTarget(GameManager gm)
        {
            SaveData save = gm.Save;
            if (save.HasPendingCutscene)
            {
                string next = !string.IsNullOrEmpty(save.pendingNextScene) ? save.pendingNextScene : gm.CurrentPhaseScene;
                gm.BeginCutscene(save.pendingCutscene, next);
                return GameConstants.SceneCutscene;
            }
            gm.SetState(GameState.Playing);
            bool benchHere = !string.IsNullOrEmpty(save.lastBenchScene) && save.lastBenchScene == gm.CurrentPhaseScene;
            return benchHere ? save.lastBenchScene : gm.CurrentPhaseScene;
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
