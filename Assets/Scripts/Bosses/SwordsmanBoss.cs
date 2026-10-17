using System.Collections;
using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>
    /// Moves shared by the two Phase 1 swordsmen (Docs/04 §B1/§B2): Advance toward the player, the 2-hit slash combo
    /// whose second hit turns around if the player got behind, and the back-step hop. Speeds and telegraphs respect
    /// enrage (<see cref="BossBase.SpeedMultiplier"/>, <see cref="BossBase.TelegraphReduction"/>).
    /// </summary>
    public abstract class SwordsmanBoss : BossBase
    {
        protected BossMoveConfig advance, slash, backStep;

        protected const float StopDistance = 1.3f;
        private const float BackStepHopSpeed = 4f;
        private const float MinTelegraph = 0.05f;

        protected override void Awake()
        {
            base.Awake();
            advance = config.FindMove("Advance") ?? new BossMoveConfig("Advance", 0f, 0f, 0f, 0f, 0) { duration = 1.1f, speed = config.moveSpeed };
            slash = config.FindMove("SlashCombo") ?? new BossMoveConfig("SlashCombo", 0.45f, 0.1f, 0.6f, 2.2f, 1, 2f) { hits = 2, hitGap = 0.25f };
            backStep = config.FindMove("BackStep") ?? new BossMoveConfig("BackStep", 0f, 0f, 0f, config.backstepDistance, 0) { duration = config.backstepTime };
        }

        protected float Telegraph(BossMoveConfig move) => Mathf.Max(MinTelegraph, move.telegraph - TelegraphReduction);

        protected IEnumerator DoAdvance()
        {
            float duration = Random.Range(config.advanceTimeMin, config.advanceTimeMax);
            float speed = (advance.speed > 0f ? advance.speed : config.moveSpeed) * SpeedMultiplier;
            Play("Walk", false);
            float t = 0f;
            while (t < duration)
            {
                FacePlayer();
                if (DistanceToPlayer <= StopDistance) break;
                SetVelocityX(facing * speed);
                t += Time.deltaTime;
                yield return null;
            }
            Stop();
        }

        /// <summary>Telegraph → slash A → gap → (turn if the player is behind) slash B → recovery.</summary>
        protected IEnumerator DoSlashCombo(BossMoveConfig move)
        {
            FacePlayer();
            Stop();
            Play("Slash_Telegraph");
            TelegraphFlash();
            AudioManager.Play(config.id + "_telegraph");
            yield return Wait(Telegraph(move));

            Play("Slash_A");
            AudioManager.Play(config.id + "_slash");
            yield return HitWindow(move.reach, move.active, move.damage);

            if (move.hits > 1)
            {
                yield return Wait(move.hitGap);
                if (PlayerIsBehind) SetFacing(-facing);
                Play("Slash_B");
                AudioManager.Play(config.id + "_slash");
                yield return HitWindow(move.reach, move.active, move.damage);
            }

            Play("Idle", false);
            yield return Wait(move.recovery);
        }

        protected IEnumerator DoBackStep()
        {
            FacePlayer();
            float duration = Mathf.Max(0.05f, backStep.duration);
            float speed = backStep.reach / duration;
            Play("Walk", false);
            rb.linearVelocity = new Vector2(-facing * speed, BackStepHopSpeed);
            float t = 0f;
            while (t < duration)
            {
                SetVelocityX(-facing * speed);
                t += Time.deltaTime;
                yield return null;
            }
            Stop();
        }
    }
}
