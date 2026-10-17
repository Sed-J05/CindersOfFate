using System.Collections;
using Shatranj.Core;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shatranj.Level
{
    /// <summary>
    /// Docs/03 §10 "Candle Shrine": stand in the trigger, press W → 0.8 s rest, full hearts, checkpoint saved
    /// (scene, bench id, position, flame progress, flags). Does NOT respawn enemies. Death returns here.
    /// </summary>
    public class Bench : InteractableBase
    {
        [SerializeField] private string benchId = "bench";
        [SerializeField] private SpriteAnimator animator;
        [SerializeField] private float restTimeFallback = 0.8f;

        private bool resting;
        private bool lit;

        public string BenchId => benchId;
        public bool IsLit => lit;
        public Vector2 RespawnPoint => transform.position;

        protected override bool CanInteract => !resting;

        public void Configure(string id)
        {
            benchId = id;
            promptText = "W — Rest";
        }

        private void Start()
        {
            if (animator == null) animator = GetComponentInChildren<SpriteAnimator>();
            if (animator != null && !lit) animator.Play("Unlit");
        }

        protected override void Interact()
        {
            if (resting) return;
            StartCoroutine(Rest());
        }

        private IEnumerator Rest()
        {
            resting = true;
            HidePrompt();
            float restTime = player.Config != null ? player.Config.benchRestTime : restTimeFallback;
            player.EnterTimedState(PlayerState.Resting, restTime);
            player.SetFacing(transform.position.x >= player.Position.x ? 1 : -1);
            AudioManager.Play("bench_rest");

            if (animator != null)
            {
                if (!animator.Play("Activate", true)) animator.Play("Lit");
            }

            // Rest FX (Prompt 17): the healing aura wraps the player while resting, then a soft flame pulse.
            TimedFx.Spawn("Heal_Aura", player.Position + new Vector2(0f, 0.85f), restTime, 0.8f, GameLayers.Sorting.FX);

            yield return new WaitForSeconds(restTime);

            if (FlameController.Instance != null) FlameController.Instance.Spike();
            Feel.Play(Data.FeelEvent.BenchRest);
            AudioManager.Play("bench_saved");
            SetLit(true);
            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.FullHeal();
                health.RespawnPosition = RespawnPoint;
            }
            SaveCheckpoint();
            GameEvents.RaiseBenchRested(benchId);
            resting = false;
            if (player != null) ShowPrompt();
        }

        private void SaveCheckpoint()
        {
            if (!GameManager.HasInstance) return;
            GameManager gm = GameManager.Instance;
            gm.Save.lastBenchId = benchId;
            gm.Save.lastBenchScene = SceneManager.GetActiveScene().name;
            gm.Save.lastBenchX = transform.position.x;
            gm.Save.lastBenchY = transform.position.y;
            gm.Save.flameT = GameEvents.LastFlameProgress;
            gm.WriteSave();
        }

        /// <summary>Lit benches show the flame (the last-used bench is lit on scene load).</summary>
        public void SetLit(bool on)
        {
            lit = on;
            if (animator == null) animator = GetComponentInChildren<SpriteAnimator>();
            if (animator == null) return;
            if (on) { if (animator.CurrentClip != "Activate" || animator.IsFinished) animator.Play("Lit"); }
            else animator.Play("Unlit");
        }

        protected override void Update()
        {
            base.Update();
            if (lit && animator != null && animator.CurrentClip == "Activate" && animator.IsFinished) animator.Play("Lit");
        }
    }
}
