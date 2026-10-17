using System.Collections;
using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Enemies;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>
    /// Docs/04 §E Zulmat, the Severed — final boss (90 HP, contact 1). Three stages at 67 % / 34 %: each change plays the
    /// Roar with screenshake and 1.5 s of invulnerability (nothing is summoned). Moves (see Zulmat.Moves.cs): Sword Slash,
    /// Vine Summon, Giant Swipe, Reposition (stage 1+), Shrink &amp; Lunge, Summon Regulars (2+), Lava Zones (3); stage 3 adds the
    /// combos Slash → Vine and Lunge → Giant Swipe with telegraphs −0.15 s and cooldowns −20 % (enrage fields).
    /// Weighted random that avoids his last two moves when possible (never an immediate repeat) and a 0.6 s fairness gap. The flame parks at top-centre for the whole
    /// fight (enemies stop scaling) and pulses on every telegraph. Death: 2.5 s slow-mo, white flash, OnFinalBossDefeated.
    /// Every number lives in the BossConfig (moves list + the "Zulmat" section).
    /// </summary>
    public partial class Zulmat : BossBase
    {
        [SerializeField] private GameObject[] minionPrefabs = new GameObject[0];

        private const float MinTelegraph = 0.05f;
        private const float FlySpeed = 9f;
        private const float WhiteFlashIn = 0.12f, WhiteFlashOut = 0.9f;

        private BossMoveConfig slash, vine, swipe, lunge, summon, lava, reposition;
        private readonly List<BossMoveConfig> moveList = new List<BossMoveConfig>(7);
        private readonly List<string> history = new List<string>(6);
        private readonly List<EnemyBase> minions = new List<EnemyBase>();
        private readonly List<LavaZoneHazard> lavaZones = new List<LavaZoneHazard>();
        private readonly List<VineHazard> vines = new List<VineHazard>();
        private System.Predicate<BossMoveConfig> available;

        private float summonCooldown;
        private float baseGravity;
        private bool walkedLast;
        private bool defeated;
        private bool parkedFlame;

        // Arena (measured at spawn from the walls around the spawn point).
        private float floorY, ceilingY, leftWall, rightWall;
        private float ArenaCenterX => (leftWall + rightWall) * 0.5f;

        private float FormScale => transform.localScale.y;

        protected override void Awake()
        {
            base.Awake();
            slash = config.FindMove("SwordSlash") ?? new BossMoveConfig("SwordSlash", 0.4f, 0.1f, 0.6f, 2.8f, 1, 2f) { hits = 2, hitGap = 0.25f };
            vine = config.FindMove("VineSummon") ?? new BossMoveConfig("VineSummon", 0.8f, 0.1f, 0.6f, 3f, 1, 2f);
            swipe = config.FindMove("GiantSwipe") ?? new BossMoveConfig("GiantSwipe", 0.8f, 0.15f, 0.8f, 6f, 1, 1.5f);
            lunge = config.FindMove("ShrinkLunge") ?? new BossMoveConfig("ShrinkLunge", 0.35f, 0.4f, 0.5f, 0f, 1, 2f) { minStage = 2 };
            summon = config.FindMove("SummonRegulars") ?? new BossMoveConfig("SummonRegulars", 0.8f, 1f, 0.5f, 0f, 0, 1f) { minStage = 2, cooldown = 20f };
            lava = config.FindMove("LavaZones") ?? new BossMoveConfig("LavaZones", 1f, 0.1f, 0.5f, 5f, 1, 1.5f) { minStage = 3 };
            reposition = config.FindMove("Reposition") ?? new BossMoveConfig("Reposition", 0.2f, 0f, 0.3f, 0f, 0, 1.2f);
            moveList.Add(slash); moveList.Add(vine); moveList.Add(swipe); moveList.Add(lunge); moveList.Add(summon); moveList.Add(lava); moveList.Add(reposition);
            available = IsAvailable;
            baseGravity = config.gravityScale;
        }

        protected override void Start()
        {
            MeasureArena();
            ParkFlame();
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
            if (summonCooldown > 0f) summonCooldown -= Time.deltaTime;
        }

        // ------------------------------------------------------------------
        // Fight loop
        // ------------------------------------------------------------------

        protected override IEnumerator Fight()
        {
            while (true)
            {
                FacePlayer();
                if (!walkedLast && DistanceToPlayer > config.farDistance && Random.value < config.walkChance)
                {
                    walkedLast = true;
                    yield return Walk(Random.Range(config.advanceTimeMin, config.advanceTimeMax), slash.reach);
                    continue;
                }
                walkedLast = false;

                // Variety (D-168): avoid the last `noRepeatWithinLast` moves when anything else is available.
                BossMoveConfig move = BossMoveSelector.PickVaried(moveList, BossMoveSelector.AnyBand, Stage, history, config.noRepeatWithinLast, Random.value, available);
                if (move == null) { walkedLast = true; yield return Walk(config.advanceTimeMax, slash.reach); continue; }

                Remember(move);
                yield return Run(move);

                // Stage 3 combos (Docs/04 §E): Slash → Vine, Lunge → Giant Swipe.
                if (Stage >= 3 && config.stage3Combos)
                {
                    if (move == slash) { Remember(vine); yield return DoVineSummon(); }
                    else if (move == lunge) { Remember(swipe); yield return DoGiantSwipe(); }
                }

                Stop();
                Play("Idle", false);
                yield return Wait(config.fairnessGap);
            }
        }

        private IEnumerator Run(BossMoveConfig move)
        {
            if (move == slash) return DoSwordSlash();
            if (move == vine) return DoVineSummon();
            if (move == swipe) return DoGiantSwipe();
            if (move == lunge) return DoShrinkLunge();
            if (move == summon) return DoSummonRegulars();
            if (move == reposition) return DoReposition();
            return DoLavaZones();
        }

        private void Remember(BossMoveConfig move)
        {
            history.Add(move.name);
            if (history.Count > 6) history.RemoveAt(0);
        }

        /// <summary>Move gates beyond stage / repeat: summon needs its cooldown and room under the cap; lava needs a free zone slot.</summary>
        private bool IsAvailable(BossMoveConfig m)
        {
            if (m == summon) return summonCooldown <= 0f && AliveMinions < config.summonCap && minionPrefabs.Length > 0;
            if (m == lava) return ActiveLavaZones < Mathf.Max(1, config.maxLavaZones);
            return true;
        }

        private float Telegraph(BossMoveConfig move) => Mathf.Max(MinTelegraph, move.telegraph - TelegraphReduction);

        private float CooldownMultiplier => IsEnraged ? config.enrageCooldownMultiplier : 1f;

        /// <summary>Telegraph start: pose flash + the flame light pulses (Docs/07 §3).</summary>
        private void BeginTelegraph(string clip)
        {
            FacePlayer();
            Play(clip);
            TelegraphFlash();
            if (FlameController.Instance != null) FlameController.Instance.Spike();
        }

        private IEnumerator Walk(float duration, float stopDistance)
        {
            Play("Walk", false);
            float speed = config.moveSpeed * SpeedMultiplier;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                FacePlayer();
                if (DistanceToPlayer <= stopDistance) break;
                SetVelocityX(facing * speed);
                yield return null;
            }
            Stop();
        }

        // ------------------------------------------------------------------
        // Stages
        // ------------------------------------------------------------------

        protected override void OnStageChanged(int newStage)
        {
            if (IsDead) return;
            StopAllCoroutines();          // interrupt the current move
            ResetForm();
            StartCoroutine(StageTransition());
        }

        private IEnumerator StageTransition()
        {
            health.Invulnerable = true;
            Stop();
            FacePlayer();
            Play("Roar");
            ScreenShake.Shake(config.stageRoarShake, 0.6f);
            if (FlameController.Instance != null) FlameController.Instance.Spike();
            AudioManager.Play("zulmat_roar");
            yield return Wait(config.stageTransitionInvulnerability);
            health.Invulnerable = false;
            Play("Idle", false);
            yield return Fight();
        }

        // ------------------------------------------------------------------
        // Forms: scale, vanish, physics
        // ------------------------------------------------------------------

        private void SetScale(float s) => transform.localScale = new Vector3(s, s, 1f);

        private IEnumerator ScaleTo(float target, float time)
        {
            float from = FormScale;
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                SetScale(Mathf.Lerp(from, target, Easing.Evaluate(Ease.InOutQuad, t / time)));
                if (rb.bodyType == RigidbodyType2D.Dynamic) Stop();
                yield return null;
            }
            SetScale(target);
        }

        private void SetVisible(bool on)
        {
            if (visualRoot != null) visualRoot.gameObject.SetActive(on);
            if (hurtbox != null) hurtbox.enabled = on;
            if (contact != null)
            {
                contact.Active = on;
                Collider2D c = contact.GetComponent<Collider2D>();
                if (c != null) c.enabled = on;
            }
        }

        /// <summary>Kinematic: moved by code (lunges, flying to the ceiling); dynamic: normal gravity body.</summary>
        private void SetKinematic(bool on)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = on ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            rb.gravityScale = on ? 0f : baseGravity;
            if (bodyCollider != null) bodyCollider.enabled = !on;
        }

        /// <summary>Back to the normal form (after an interrupted move or before dying).</summary>
        private void ResetForm()
        {
            SetVisible(true);
            SetKinematic(false);
            SetScale(1f);
        }

        // ------------------------------------------------------------------
        // Arena + flame
        // ------------------------------------------------------------------

        private void MeasureArena()
        {
            floorY = rb.position.y;
            Vector2 mid = rb.position + new Vector2(0f, 1.5f);
            RaycastHit2D l = Physics2D.Raycast(mid, Vector2.left, 40f, GameLayers.WallMask);
            RaycastHit2D r = Physics2D.Raycast(mid, Vector2.right, 40f, GameLayers.WallMask);
            RaycastHit2D u = Physics2D.Raycast(rb.position + new Vector2(0f, 0.5f), Vector2.up, 30f, GameLayers.WallMask);
            leftWall = l.collider != null ? l.point.x : rb.position.x - 12f;
            rightWall = r.collider != null ? r.point.x : rb.position.x + 12f;
            ceilingY = u.collider != null ? u.point.y : floorY + 10f;
        }

        /// <summary>Walls at a given height (lunges run at the player's height, where the dais may stick out).</summary>
        private void WallsAt(float y, out float left, out float right)
        {
            var origin = new Vector2(Mathf.Clamp(PlayerX(), leftWall + 0.5f, rightWall - 0.5f), y);
            RaycastHit2D l = Physics2D.Raycast(origin, Vector2.left, 40f, GameLayers.WallMask);
            RaycastHit2D r = Physics2D.Raycast(origin, Vector2.right, 40f, GameLayers.WallMask);
            left = l.collider != null ? l.point.x : leftWall;
            right = r.collider != null ? r.point.x : rightWall;
        }

        private float PlayerX() => TryGetPlayer(out PlayerController p) ? p.Position.x : rb.position.x;

        private void ParkFlame()
        {
            if (!config.flameParksDuringFight || FlameController.Instance == null) return;
            FlameController.Instance.Park(config.flameParkViewport);
            parkedFlame = true;
        }

        private void UnparkFlame()
        {
            if (!parkedFlame || FlameController.Instance == null) return;
            FlameController.Instance.Unpark();
            parkedFlame = false;
        }

        // ------------------------------------------------------------------
        // Minions / hazards bookkeeping
        // ------------------------------------------------------------------

        private int AliveMinions
        {
            get
            {
                for (int i = minions.Count - 1; i >= 0; i--)
                    if (minions[i] == null || minions[i].IsDead) minions.RemoveAt(i);
                return minions.Count;
            }
        }

        private int ActiveLavaZones
        {
            get
            {
                for (int i = lavaZones.Count - 1; i >= 0; i--)
                    if (lavaZones[i] == null || lavaZones[i].Finished) lavaZones.RemoveAt(i);
                return lavaZones.Count;
            }
        }

        private void ClearHazards(bool killMinions)
        {
            for (int i = 0; i < minions.Count; i++)
            {
                if (minions[i] == null) continue;
                if (killMinions && !minions[i].IsDead) minions[i].Health.Kill(gameObject);
                else if (!killMinions) Destroy(minions[i].gameObject);
            }
            minions.Clear();
            for (int i = 0; i < lavaZones.Count; i++) if (lavaZones[i] != null) Destroy(lavaZones[i].gameObject);
            lavaZones.Clear();
            for (int i = 0; i < vines.Count; i++) if (vines[i] != null) Destroy(vines[i].gameObject);
            vines.Clear();
        }

        // ------------------------------------------------------------------
        // Death / reset
        // ------------------------------------------------------------------

        protected override void OnDeathStarted()
        {
            defeated = true;
            ResetForm();
            ClearHazards(killMinions: true);
        }

        /// <summary>Docs/04 §E: after the 2.5 s slow-mo, a white flash, then the final-boss event (Cutscene 3, Prompt 16).</summary>
        protected override void OnDefeated()
        {
            SceneFlow flow = SceneFlow.Instance;
            Color previous = flow.FadeColor;
            flow.FadeColor = Color.white;
            flow.FadeTo(1f, WhiteFlashIn, () => flow.FadeTo(0f, WhiteFlashOut, () => flow.FadeColor = previous));
            AudioManager.Play("zulmat_death");
            GameEvents.RaiseFinalBossDefeated();
        }

        protected override void OnDestroy()
        {
            // Fight abandoned (player died) or finished: no stray minions, vines or lava; the flame moves again unless he lost.
            ClearHazards(killMinions: false);
            if (!defeated) UnparkFlame();
            base.OnDestroy();
        }
    }
}
