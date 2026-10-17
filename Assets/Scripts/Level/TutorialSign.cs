using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>Docs/05 §7 sign: shows its manifest text as the interact prompt while the player stands near.</summary>
    public class TutorialSign : InteractableBase
    {
        public void Configure(string text)
        {
            promptText = text;
            requiresKey = false;
            promptOffset = new Vector2(0f, 0.3f);
        }

        protected override void Interact() { }
    }
}
