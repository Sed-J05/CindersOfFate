using System.Collections.Generic;
using Shatranj.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.UI
{
    /// <summary>
    /// Docs/08 §6 ability popup: icon + name for 3 s with fades. Queues when several unlock at once.
    /// Listens to OnAbilityUnlocked (explicit, used by cutscenes) and to OnAbilitiesChanged diffs (F4 debug).
    /// </summary>
    public class AbilityPopupUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Sprite dashIcon;
        [SerializeField] private Sprite fireballIcon;
        [SerializeField] private string dashName = "TIME SLIP  (L)";
        [SerializeField] private string fireballName = "FLAMECALL  (K)";
        [SerializeField] private float fadeIn = 0.3f;
        [SerializeField] private float hold = 2.4f;
        [SerializeField] private float fadeOut = 0.3f;

        private readonly Queue<string> queue = new Queue<string>();
        private PlayerAbilities previous;
        private float t = -1f;

        private void OnEnable()
        {
            GameEvents.OnAbilityUnlocked += Enqueue;
            GameEvents.OnAbilitiesChanged += HandleAbilitiesChanged;
            if (group != null) group.alpha = 0f;
            if (GameManager.HasInstance) previous = GameManager.Instance.Abilities.Clone();
        }

        private void OnDisable()
        {
            GameEvents.OnAbilityUnlocked -= Enqueue;
            GameEvents.OnAbilitiesChanged -= HandleAbilitiesChanged;
        }

        private void HandleAbilitiesChanged(PlayerAbilities abilities)
        {
            if (previous != null)
            {
                if (!previous.canDash && abilities.canDash) Enqueue(GameEvents.AbilityDash);
                if (!previous.canFireball && abilities.canFireball) Enqueue(GameEvents.AbilityFireball);
            }
            previous = abilities.Clone();
        }

        public void Enqueue(string abilityId)
        {
            if (string.IsNullOrEmpty(abilityId)) return;
            queue.Enqueue(abilityId);
        }

        private void Update()
        {
            if (group == null) return;
            float dt = Time.unscaledDeltaTime;

            if (t < 0f)
            {
                if (queue.Count == 0) return;
                Show(queue.Dequeue());
                t = 0f;
            }

            t += dt;
            float total = fadeIn + hold + fadeOut;
            if (t < fadeIn) group.alpha = t / Mathf.Max(0.01f, fadeIn);
            else if (t < fadeIn + hold) group.alpha = 1f;
            else if (t < total) group.alpha = 1f - (t - fadeIn - hold) / Mathf.Max(0.01f, fadeOut);
            else { group.alpha = 0f; t = -1f; }
        }

        private void Show(string abilityId)
        {
            bool dash = abilityId == GameEvents.AbilityDash;
            if (icon != null)
            {
                icon.sprite = dash ? dashIcon : fireballIcon;
                icon.enabled = icon.sprite != null;
            }
            if (nameText != null) nameText.text = dash ? dashName : fireballName;
            AudioManager.Play("ability_unlocked");
        }
    }
}
