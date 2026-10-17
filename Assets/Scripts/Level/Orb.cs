using System.Collections;
using Shatranj.Core;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shatranj.Level
{
    /// <summary>
    /// Docs/05 legend 'O': the memory orb at the end of Phase 1. W → plays Touch, raises OnOrbTouched, then (placeholder
    /// until Cutscene 2 in Prompt 11) fades out and loads Phase2_Castle if that scene is in Build Settings, otherwise
    /// shows a "To be continued…" card and returns control.
    /// </summary>
    public class Orb : InteractableBase
    {
        [SerializeField] private SpriteAnimator animator;
        [SerializeField] private bool runPlaceholderSequence = true;
        [SerializeField] private float touchHoldTime = 1f;
        [SerializeField] private float fadeTime = 0.8f;

        private bool touched;

        public bool Touched => touched;

        protected override bool CanInteract => !touched;

        private void Start()
        {
            promptText = "W — Touch";
            if (animator == null) animator = GetComponentInChildren<SpriteAnimator>();
            if (animator != null) animator.Play("Idle");
        }

        protected override void Interact()
        {
            if (touched) return;
            touched = true;
            HidePrompt();
            if (animator != null) animator.Play("Touch", true);
            AudioManager.Play("orb_touch");
            GameEvents.RaiseOrbTouched();
            if (runPlaceholderSequence) StartCoroutine(PlaceholderSequence(player));
        }

        private IEnumerator PlaceholderSequence(PlayerController who)
        {
            if (who != null) who.SetCutsceneControl(true);
            if (GameManager.HasInstance) GameManager.Instance.SetState(GameState.Cutscene);
            yield return new WaitForSeconds(touchHoldTime);

            if (Application.CanStreamedLevelBeLoaded(GameConstants.ScenePhase2))
            {
                if (GameManager.HasInstance)
                {
                    GameManager.Instance.SetPhase(GameConstants.Phase2);
                    GameManager.Instance.Save.lastBenchId = "";
                    GameManager.Instance.Save.lastBenchScene = "";
                    GameManager.Instance.WriteSave();
                }
                SceneFlow.Load(GameConstants.ScenePhase2, fadeTime);
                yield break;
            }

            // Phase 2 isn't built yet: end card, then hand control back.
            yield return UI.ToBeContinuedCard.Show(this, "To be continued…", fadeTime, 3f);
            if (GameManager.HasInstance) GameManager.Instance.SetState(GameState.Playing);
            if (who != null) who.SetCutsceneControl(false);
        }
    }
}
