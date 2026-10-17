using System.Collections;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>
    /// Docs/04 §B1 Kage Warden (MB1, 20 HP). Loop picks a move by weighted random with no immediate repeat:
    /// 1 Advance (3 u/s for 0.8–1.4 s) · 2 Slash combo (telegraph 0.45 → slash reach 2.2, active 0.1 → 0.25 gap →
    /// second slash turns around if the player got behind → recovery 0.6) · 3 Back-step (hop back 3 u in 0.35 s)
    /// then Advance · 4 Block (1.5 s; frontal hits deal 0, clink spark, player pushed 2 u/s; hits from behind land)
    /// ending in a counter-slash (telegraph 0.3) if the player is within 2.5 u.
    /// Block never twice in a row; only after ≥ 2 player attacks in 2 s, or a 20 % roll.
    /// </summary>
    public class KageWarden : SwordsmanBoss
    {
        private enum Move { None, Advance, SlashCombo, BackStep, Block }

        private Move last = Move.None;
        private bool blocking;
        private BossMoveConfig block, counter;

        private const float BlockStartTime = 3f / 16f;   // Block_Start: 3 frames @ 16 fps

        protected override void Awake()
        {
            base.Awake();
            block = config.FindMove("Block") ?? new BossMoveConfig("Block", 0f, 1.5f, 0f, 0f, 0) { duration = 1.5f };
            counter = config.FindMove("CounterSlash") ?? new BossMoveConfig("CounterSlash", 0.3f, 0.1f, 0.5f, 2.5f, 1, 0f);
        }

        protected override IEnumerator Fight()
        {
            while (true)
            {
                Move next = Choose();
                switch (next)
                {
                    case Move.Advance: yield return DoAdvance(); break;
                    case Move.SlashCombo: yield return DoSlashCombo(slash); break;
                    case Move.BackStep: yield return DoBackStep(); yield return DoAdvance(); break;
                    case Move.Block: yield return DoBlock(); break;
                }
                last = next;
                Stop();
                Play("Idle", false);
                yield return Wait(config.fairnessGap);
            }
        }

        /// <summary>Weighted random, no immediate repeat; weights lean on distance so slashes are thrown in reach.</summary>
        private Move Choose()
        {
            if (last != Move.Block)
            {
                if (AttemptsInLast(config.blockHitWindow) >= config.blockMinHitsToTrigger) return Move.Block;
                if (Random.value < config.blockRandomChance) return Move.Block;
            }

            float d = DistanceToPlayer;
            bool inReach = d <= slash.reach + 1.2f;
            float wAdvance = last == Move.Advance ? 0f : advance.weight * (inReach ? 0.4f : 2f);
            float wSlash = last == Move.SlashCombo ? 0f : slash.weight * (inReach ? 2f : 0.25f);
            float wBack = last == Move.BackStep ? 0f : backStep.weight * (d < 2f ? 1.5f : 0.3f);
            float total = wAdvance + wSlash + wBack;
            if (total <= 0f) return Move.Advance;

            float roll = Random.value * total;
            if (roll < wAdvance) return Move.Advance;
            if (roll < wAdvance + wSlash) return Move.SlashCombo;
            return Move.BackStep;
        }

        private IEnumerator DoBlock()
        {
            FacePlayer();
            Stop();
            blocking = true;
            Play("Block_Start");
            float duration = config.blockDuration > 0f ? config.blockDuration : block.duration;
            float t = 0f;
            while (t < duration)
            {
                if (animator != null)
                {
                    if (ClipFinished("Block_Start") || ClipFinished("Block_Impact") || (t >= BlockStartTime && animator.CurrentClip == "Block_Start"))
                        Play("Block_Hold", false);
                }
                t += Time.deltaTime;
                yield return null;
            }
            blocking = false;

            if (DistanceToPlayer <= config.counterSlashRange)
            {
                FacePlayer();
                Play("Slash_Telegraph");
                TelegraphFlash();
                yield return Wait(Telegraph(counter));
                Play("Slash_A");
                AudioManager.Play("warden_slash");
                yield return HitWindow(counter.reach, counter.active, counter.damage);
                Play("Idle", false);
                yield return Wait(counter.recovery);
            }
        }

        /// <summary>Guard: frontal hits are blocked with a clink and a pushback; hits from behind land.</summary>
        protected override bool AllowDamage(DamageInfo info)
        {
            if (!blocking || info.source == null) return true;
            float side = info.source.transform.position.x >= rb.position.x ? 1f : -1f;
            if (Mathf.Approximately(side, facing))
            {
                Vector2 spark = rb.position + new Vector2(facing * 0.8f, config.colliderSize.y * 0.6f);
                FxPool.Play("Block_Clink", spark);
                Play("Block_Impact");
                AudioManager.Play("warden_clink");
                Feel.Play(FeelEvent.GuardBlock);
                var pc = info.source.GetComponent<PlayerController>();
                if (pc != null) pc.ApplyRecoil(side * config.blockPushback, config.blockPushbackTime);
                return false;
            }
            return true;
        }
    }
}
