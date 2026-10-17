using Shatranj.Core;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>
    /// Docs/05 legend 'O': the memory orb at the end of Phase 1. W → plays Touch and raises OnOrbTouched;
    /// Prompt 9/11 turn that into Cutscene 2.
    /// </summary>
    public class Orb : InteractableBase
    {
        [SerializeField] private SpriteAnimator animator;

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
        }
    }
}
