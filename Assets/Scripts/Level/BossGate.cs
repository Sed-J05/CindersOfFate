using Shatranj.Core;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>Docs/05 legend 'G': arena gate. Open by default; the BossArena closes it on engage and opens it on defeat / reset.</summary>
    public class BossGate : MonoBehaviour
    {
        [SerializeField] private Collider2D blocker;
        [SerializeField] private SpriteAnimator animator;

        private bool closed;

        public bool IsClosed => closed;

        private void Start()
        {
            if (blocker == null) blocker = GetComponent<Collider2D>();
            if (animator == null) animator = GetComponentInChildren<SpriteAnimator>();
            gameObject.layer = GameLayers.Ground;
            if (blocker != null) blocker.enabled = closed;
            if (animator != null && !closed) animator.Play("Open");
        }

        public void Close()
        {
            closed = true;
            if (blocker != null) blocker.enabled = true;
            if (animator != null) animator.Play("Close", true);
            AudioManager.Play("gate_close");
        }

        public void Open()
        {
            closed = false;
            if (blocker != null) blocker.enabled = false;
            if (animator != null) animator.Play("Open", true);
            AudioManager.Play("gate_open");
        }
    }
}
