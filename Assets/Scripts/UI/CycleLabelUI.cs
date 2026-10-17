using Shatranj.Core;
using TMPro;
using UnityEngine;

namespace Shatranj.UI
{
    /// <summary>Docs/08 §6: small "Cycle N" label bottom-left, only when N ≥ 2. Driven by OnCycleChanged.</summary>
    public class CycleLabelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private string format = "Cycle {0}";

        private void OnEnable()
        {
            GameEvents.OnCycleChanged += HandleCycle;
            HandleCycle(GameManager.HasInstance ? GameManager.Instance.Cycle : 1);
        }

        private void OnDisable()
        {
            GameEvents.OnCycleChanged -= HandleCycle;
        }

        private void HandleCycle(int cycle)
        {
            if (label == null) return;
            bool show = cycle >= 2;
            label.enabled = show;
            if (show) label.text = string.Format(format, cycle);
        }
    }
}
