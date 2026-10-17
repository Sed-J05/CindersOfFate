using Shatranj.Core;
using Shatranj.Player;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>
    /// Shared trigger logic for benches, the orb and signs: tracks the player inside the trigger, shows the
    /// interact prompt through <see cref="GameEvents"/>, and fires <see cref="Interact"/> on W (Docs/03 §10).
    /// Signs set <c>requiresKey = false</c> to just show their text.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractableBase : MonoBehaviour
    {
        [SerializeField] protected string promptText = "W — Interact";
        [SerializeField] protected bool requiresKey = true;
        [SerializeField] protected Vector2 promptOffset = new Vector2(0f, 0.2f);

        protected PlayerController player;
        private bool promptShown;

        protected virtual bool CanInteract => true;

        private void Awake()
        {
            gameObject.layer = GameLayers.Interactable;
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != GameLayers.Player) return;
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (pc == null) return;
            player = pc;
            ShowPrompt();
            OnPlayerEnter();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (player == null || other.gameObject.layer != GameLayers.Player) return;
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (pc != player) return;
            HidePrompt();
            player = null;
            OnPlayerExit();
        }

        private void OnDisable()
        {
            if (promptShown) HidePrompt();
            player = null;
        }

        protected virtual void Update()
        {
            if (player == null || !requiresKey || !CanInteract) return;
            if (!player.Input.InteractPressed) return;
            PlayerState s = player.State;
            if (s != PlayerState.Idle && s != PlayerState.Run && s != PlayerState.Land) return;
            Interact();
        }

        protected void ShowPrompt()
        {
            if (!CanInteract || string.IsNullOrEmpty(promptText)) return;
            promptShown = true;
            GameEvents.RaiseInteractPromptShown(promptText, (Vector2)transform.position + promptOffset);
        }

        protected void HidePrompt()
        {
            if (!promptShown) return;
            promptShown = false;
            GameEvents.RaiseInteractPromptHidden();
        }

        protected abstract void Interact();
        protected virtual void OnPlayerEnter() { }
        protected virtual void OnPlayerExit() { }
    }
}
