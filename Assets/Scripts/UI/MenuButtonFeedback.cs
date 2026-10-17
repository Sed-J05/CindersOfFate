using Shatranj.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shatranj.UI
{
    /// <summary>
    /// Menu polish (Prompt 17): a selected / hovered button grows slightly (unscaled time, so it works while paused) and
    /// plays "ui_move"; added by <see cref="UIFactory.Button"/> to every code-built menu button.
    /// </summary>
    [DisallowMultipleComponent]
    public class MenuButtonFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float selectedScale = 1.06f;
        [SerializeField] private float speed = 14f;

        private bool selected;
        private bool hovered;

        public void OnSelect(BaseEventData eventData) { if (!selected) AudioManager.Play("ui_move"); selected = true; }
        public void OnDeselect(BaseEventData eventData) => selected = false;
        public void OnPointerEnter(PointerEventData eventData) { if (!hovered && !selected) AudioManager.Play("ui_move"); hovered = true; }
        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        private void OnDisable()
        {
            selected = hovered = false;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            float target = selected || hovered ? selectedScale : 1f;
            float s = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
