using Shatranj.Core;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>
    /// Docs/05 legend 'D': 2×6-tile door locked until a save flag (e.g. "door_orb") is set.
    /// Solid collider on the Ground layer while locked; plays Open and drops the collider once unlocked.
    /// </summary>
    public class Door : MonoBehaviour
    {
        [SerializeField] private string doorId = "door";
        [SerializeField] private string requiredFlag = "door_x";
        [SerializeField] private Collider2D blocker;
        [SerializeField] private SpriteAnimator animator;

        private bool open;

        public string DoorId => doorId;
        public string RequiredFlag => requiredFlag;
        public bool IsOpen => open;

        public void Configure(string id, string flag)
        {
            doorId = id;
            requiredFlag = flag;
        }

        private void OnEnable()
        {
            GameEvents.OnFlagChanged += HandleFlagChanged;
            GameEvents.OnBossDefeated += HandleBossDefeated;
        }

        private void OnDisable()
        {
            GameEvents.OnFlagChanged -= HandleFlagChanged;
            GameEvents.OnBossDefeated -= HandleBossDefeated;
        }

        private void Start()
        {
            if (blocker == null) blocker = GetComponent<Collider2D>();
            if (animator == null) animator = GetComponentInChildren<SpriteAnimator>();
            gameObject.layer = GameLayers.Ground;
            Refresh(instant: true);
        }

        private void HandleFlagChanged(string flag, bool value)
        {
            if (flag == requiredFlag) Refresh(instant: false);
        }

        private void HandleBossDefeated(string bossId) => Refresh(instant: false);

        private void Refresh(bool instant)
        {
            bool unlocked = GameManager.HasInstance && GameManager.Instance.Save.HasFlag(requiredFlag);
            if (unlocked && !open) Open(instant);
            else if (!unlocked && open) Close();
        }

        public void Open(bool instant)
        {
            open = true;
            if (blocker != null) blocker.enabled = false;
            if (animator != null)
            {
                if (instant) { if (!animator.ShowFrame("Open", 5) && !animator.ShowFrame("Open", 0)) animator.Play("Open"); }
                else animator.Play("Open", true);
            }
            if (!instant) AudioManager.Play("door_open");
        }

        public void Close()
        {
            open = false;
            if (blocker != null) blocker.enabled = true;
            if (animator != null) animator.Play("Closed");
        }

        /// <summary>Convenience for bosses / scripts: sets the flag (which also opens the door through the event).</summary>
        public void Unlock()
        {
            if (GameManager.HasInstance) GameManager.Instance.SetFlag(requiredFlag, true);
            else Open(false);
        }
    }
}
