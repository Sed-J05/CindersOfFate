using System.Collections;
using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>
    /// Docs/04 §D Iron Colossus (MB3, 45 HP, 1.8 u/s, never staggers, every attack deals 2 hearts, contact 1).
    /// Moves: <b>Big Arc Slash</b> (telegraph 0.7 → reach 3.2 u, active 0.15 → recovery 0.9) ·
    /// <b>Big Shield Block</b> (2 s, frontal hits blocked with a clink + pushback, hits from behind land) ending in a
    /// <b>Shield Bash</b> (telegraph 0.3, reach 2) · <b>Jump Slam</b> (telegraph 0.6 → leap to the player's X, apex 3.5 u,
    /// 0.7 s → shockwave radius 2 u along the ground; jump it) · <b>Dash</b> (telegraph 0.5 → 12 u/s for 0.4 s,
    /// recovery 0.8). Selection by distance: far → Dash/Jump, mid → Jump/Arc, close → Arc/Block/Bash; no move 3× in a row
    /// (<see cref="BossMoveSelector"/>). Mid range it sometimes walks in first. Enrage below 40 %: telegraphs −0.15 s and
    /// every Jump Slam chains into a Big Arc Slash. Fixed size (bosses never scale with the flame).
    /// </summary>
    public class IronColossus : BossBase
    {
        private const float MinTelegraph = 0.05f;
        private const float ShockwaveHeight = 1.2f;   // the wave hugs the floor: jumping clears it
        private const float ShockwaveArtWidth = 2.56f; // Shockwave canvas 256 px at 100 px/u
        private const float ShieldArcScale = 0.8f;
        private const float DashHitReach = 1.3f;
        private const float LandTimeout = 1.2f;

        private BossMoveConfig arc, block, bash, jump, dash;
        private readonly List<string> history = new List<string>(8);
        private bool blocking;
        private bool walkedLast;
        private PooledFx shieldFx;
        private float baseGravityScale;
        private ContactFilter2D groundFilter, wallFilter;
        private readonly Collider2D[] probe = new Collider2D[2];

        protected override void Awake()
        {
            base.Awake();
            arc = config.FindMove("BigArcSlash") ?? new BossMoveConfig("BigArcSlash", 0.7f, 0.15f, 0.9f, 3.2f, 2, 2f) { distanceBand = BossMoveSelector.Close | BossMoveSelector.Mid };
            block = config.FindMove("ShieldBlock") ?? new BossMoveConfig("ShieldBlock", 0f, 2f, 0f, 0f, 0, 1f) { duration = 2f, distanceBand = BossMoveSelector.Close };
            bash = config.FindMove("ShieldBash") ?? new BossMoveConfig("ShieldBash", 0.3f, 0.1f, 0.6f, 2f, 2, 1f) { distanceBand = BossMoveSelector.Close };
            jump = config.FindMove("JumpSlam") ?? new BossMoveConfig("JumpSlam", 0.6f, 0.7f, 0.6f, 2f, 2, 2f) { distanceBand = BossMoveSelector.Mid | BossMoveSelector.Far, extra = 3.5f };
            dash = config.FindMove("Dash") ?? new BossMoveConfig("Dash", 0.5f, 0.4f, 0.8f, 0f, 2, 2f) { speed = 12f, duration = 0.4f, distanceBand = BossMoveSelector.Far };
            baseGravityScale = config.gravityScale;

            // D-161: the Colossus ignores one-way platforms entirely — it walks under them and can never land on one,
            // so the arena platform is a real refuge from its ground attacks (the jump slam still reaches the floor).
            rb.excludeLayers = rb.excludeLayers | GameLayers.OneWayMask;
            groundFilter = new ContactFilter2D();
            groundFilter.SetLayerMask(GameLayers.GroundMask);
            groundFilter.useTriggers = false;
            wallFilter = new ContactFilter2D();
            wallFilter.SetLayerMask(GameLayers.WallMask);
            wallFilter.useTriggers = false;
        }

        protected override void Start()
        {
            IgnoreOneWayPlatforms();
            base.Start();
        }

        /// <summary>
        /// D-161: pair-wise ignore every one-way platform collider in the scene (the layer exclusion alone was not
        /// honoured by the platform effector). Platforms are static scene geometry, so once at spawn is enough.
        /// </summary>
        private void IgnoreOneWayPlatforms()
        {
            Collider2D[] own = GetComponentsInChildren<Collider2D>(true);
            Collider2D[] all = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Collider2D c = all[i];
                if (c == null || c.gameObject.layer != GameLayers.OneWay) continue;
                for (int j = 0; j < own.Length; j++) Physics2D.IgnoreCollision(own[j], c, true);
            }
        }

        private float Telegraph(BossMoveConfig move) => Mathf.Max(MinTelegraph, move.telegraph - TelegraphReduction);

        private int Damage(BossMoveConfig move) => move.damage > 0 ? move.damage : config.attackDamage;

        // ------------------------------------------------------------------
        // Fight loop
        // ------------------------------------------------------------------

        protected override IEnumerator Fight()
        {
            var moveList = new List<BossMoveConfig> { arc, block, bash, jump, dash };
            while (true)
            {
                FacePlayer();
                int band = BossMoveSelector.BandFor(DistanceToPlayer, config.closeDistance, config.farDistance);

                if (band == BossMoveSelector.Mid && !walkedLast && Random.value < config.walkChance)
                {
                    walkedLast = true;
                    yield return Walk();
                    continue;
                }
                walkedLast = false;

                BossMoveConfig move = BossMoveSelector.Pick(moveList, band, Stage, history, config.maxSameMoveInRow, Random.value);
                if (move == null)
                {
                    walkedLast = true;
                    yield return Walk();
                    continue;
                }

                Remember(move.name);
                if (move == arc) yield return DoArcSlash();
                else if (move == block) yield return DoBlock();
                else if (move == bash) yield return DoBash();
                else if (move == jump)
                {
                    yield return DoJumpSlam();
                    if (IsEnraged) { Remember(arc.name); yield return DoArcSlash(); }   // enrage combo: jump → arc
                }
                else if (move == dash) yield return DoDash();

                Stop();
                Play("Idle", false);
                yield return Wait(config.fairnessGap);
            }
        }

        private void Remember(string move)
        {
            history.Add(move);
            if (history.Count > 6) history.RemoveAt(0);
        }

        private IEnumerator Walk()
        {
            float duration = Random.Range(config.advanceTimeMin, config.advanceTimeMax);
            float speed = config.moveSpeed * SpeedMultiplier;
            Play("Walk", false);
            float t = 0f;
            while (t < duration)
            {
                FacePlayer();
                if (DistanceToPlayer < config.closeDistance || WallAhead(0.2f)) break;
                SetVelocityX(facing * speed);
                t += Time.deltaTime;
                yield return null;
            }
            Stop();
        }

        // ------------------------------------------------------------------
        // Moves
        // ------------------------------------------------------------------

        private IEnumerator DoArcSlash()
        {
            FacePlayer();
            Stop();
            Play("Slash_Telegraph");
            TelegraphFlash();
            AudioManager.Play("colossus_telegraph");
            yield return Wait(Telegraph(arc));

            Play("Slash");
            AudioManager.Play("colossus_slash");
            ScreenShake.Shake(0.08f, 0.12f);
            // Wide arc: from just behind the body centre to 3.2 u in front, taller than the boss.
            yield return HitWindow(arc.reach, arc.active, Damage(arc), config.bodySize.y + 0.6f, 0f, -0.3f);

            Play("Idle", false);
            yield return Wait(arc.recovery);
        }

        private IEnumerator DoBlock()
        {
            FacePlayer();
            Stop();
            blocking = true;
            Play("Block_Start");
            ShowShield(true);
            float duration = config.blockDuration > 0f ? config.blockDuration : block.duration;
            float t = 0f;
            while (t < duration)
            {
                if (animator != null && (ClipFinished("Block_Start") || ClipFinished("Block_Impact"))) Play("Block_Hold", false);
                t += Time.deltaTime;
                yield return null;
            }
            blocking = false;
            ShowShield(false);
            yield return DoBash();   // Docs/04: the block always ends with a shield bash
        }

        private IEnumerator DoBash()
        {
            FacePlayer();
            Stop();
            Play("Block_Start");
            TelegraphFlash();
            AudioManager.Play("colossus_telegraph");
            yield return Wait(Telegraph(bash));

            Play("Bash");
            AudioManager.Play("colossus_bash");
            SetVelocityX(facing * 3f);   // small lunge into the bash
            yield return HitWindow(bash.reach, bash.active, Damage(bash), config.bodySize.y, 0f, 0f);
            Stop();
            Play("Idle", false);
            yield return Wait(bash.recovery);
        }

        private IEnumerator DoJumpSlam()
        {
            FacePlayer();
            Stop();
            Play("Jump_Telegraph");
            TelegraphFlash();
            AudioManager.Play("colossus_telegraph");
            yield return Wait(Telegraph(jump));

            // Ballistic leap that peaks at `extra` (3.5 u) and lands after `active` (0.7 s): g = 8h/T², vy = 4h/T.
            float airTime = Mathf.Max(0.2f, jump.active);
            float apex = jump.extra > 0f ? jump.extra : 3.5f;
            float g = 8f * apex / (airTime * airTime);
            float vy = 4f * apex / airTime;
            float targetX = TryGetPlayer(out PlayerController p) ? p.Position.x : rb.position.x;
            targetX = ClampToWalls(targetX, apex * 0.5f);
            float vx = (targetX - rb.position.x) / airTime;

            rb.gravityScale = g / Mathf.Abs(Physics2D.gravity.y);
            rb.linearVelocity = new Vector2(vx, vy);
            Play("Jump_Air");
            AudioManager.Play("colossus_jump");

            float t = 0f;
            while (true)
            {
                t += Time.deltaTime;
                if (WallAhead(0.05f, Mathf.Sign(vx))) SetVelocityX(0f);
                else SetVelocityX(vx);
                if (t > airTime * 0.5f && rb.linearVelocity.y <= 0.01f && IsGrounded()) break;
                if (t > airTime + LandTimeout) break;
                yield return null;
            }
            rb.gravityScale = baseGravityScale;
            Stop();

            Play("Jump_Land");
            Shockwave(jump.reach);
            yield return Wait(jump.recovery);
        }

        private void Shockwave(float radius)
        {
            Vector2 feet = rb.position;
            float scale = radius * 2f / ShockwaveArtWidth;
            FxPool.Play("Shockwave", feet + new Vector2(0f, 0.3f), 1, scale);
            FxPool.Play("Dust_Puff", feet + new Vector2(-radius * 0.6f, 0.3f), -1, 1.5f);
            FxPool.Play("Dust_Puff", feet + new Vector2(radius * 0.6f, 0.3f), 1, 1.5f);
            ScreenShake.Shake(0.3f, 0.3f);
            AudioManager.Play("colossus_slam");
            TryHitPlayer(feet + new Vector2(0f, ShockwaveHeight * 0.5f), new Vector2(radius * 2f, ShockwaveHeight), Damage(jump));
        }

        private IEnumerator DoDash()
        {
            FacePlayer();
            Stop();
            Play("Dash_Telegraph");
            TelegraphFlash();
            AudioManager.Play("colossus_telegraph");
            yield return Wait(Telegraph(dash));

            Play("Dash");
            AudioManager.Play("colossus_dash");
            float duration = dash.duration > 0f ? dash.duration : dash.active;
            float speed = dash.speed > 0f ? dash.speed : 12f;
            bool hit = false;
            float t = 0f;
            while (t < duration)
            {
                if (WallAhead(0.15f)) { ScreenShake.Shake(0.15f, 0.15f); break; }
                SetVelocityX(facing * speed);
                if (!hit)
                {
                    float half = config.colliderSize.x * 0.5f;
                    Vector2 center = rb.position + new Vector2(facing * (half + DashHitReach * 0.5f - 0.3f), config.colliderSize.y * 0.5f);
                    hit = TryHitPlayer(center, new Vector2(DashHitReach, config.colliderSize.y), Damage(dash));
                }
                t += Time.deltaTime;
                yield return null;
            }
            Stop();
            Play("Idle", false);
            yield return Wait(dash.recovery);
        }

        // ------------------------------------------------------------------
        // Guard
        // ------------------------------------------------------------------

        /// <summary>Tower shield: frontal hits are blocked (clink, pushback); hits from behind land.</summary>
        protected override bool AllowDamage(DamageInfo info)
        {
            if (!blocking || info.source == null) return true;
            float side = info.source.transform.position.x >= rb.position.x ? 1f : -1f;
            if (!Mathf.Approximately(side, facing)) return true;

            Vector2 spark = rb.position + new Vector2(facing * (config.colliderSize.x * 0.5f + 0.3f), config.colliderSize.y * 0.6f);
            FxPool.Play("Block_Clink", spark);
            Play("Block_Impact");
            AudioManager.Play("colossus_clink");
            var pc = info.source.GetComponent<PlayerController>();
            if (pc != null) pc.ApplyRecoil(side * config.blockPushback, config.blockPushbackTime);
            return false;
        }

        private void ShowShield(bool on)
        {
            if (shieldFx != null) { shieldFx.ReleaseNow(); shieldFx = null; }
            if (!on) return;
            Vector2 pos = rb.position + new Vector2(facing * (config.colliderSize.x * 0.5f + 0.2f), config.bodySize.y * 0.55f);
            shieldFx = FxPool.Play("Shield_Arc", pos, facing, ShieldArcScale, FxPool.DefaultClip, GameLayers.Sorting.FX, 5);
        }

        protected override void OnDeathStarted()
        {
            blocking = false;
            ShowShield(false);
            rb.gravityScale = baseGravityScale;
        }

        protected override void OnDestroy()
        {
            ShowShield(false);
            base.OnDestroy();
        }

        // ------------------------------------------------------------------
        // Probes
        // ------------------------------------------------------------------

        private bool IsGrounded()
        {
            var size = new Vector2(config.colliderSize.x * 0.9f, 0.12f);
            return Physics2D.OverlapBox(rb.position + new Vector2(0f, -0.04f), size, 0f, groundFilter, probe) > 0;
        }

        private bool WallAhead(float distance) => WallAhead(distance, facing);

        private bool WallAhead(float distance, float dir)
        {
            if (dir == 0f) return false;
            float half = config.colliderSize.x * 0.5f;
            var size = new Vector2(Mathf.Max(0.05f, distance), config.colliderSize.y * 0.8f);
            Vector2 center = rb.position + new Vector2(Mathf.Sign(dir) * (half + size.x * 0.5f), config.colliderSize.y * 0.5f);
            return Physics2D.OverlapBox(center, size, 0f, wallFilter, probe) > 0;
        }

        /// <summary>Keeps a jump target inside the arena walls (raycast at <paramref name="height"/> above the feet).</summary>
        private float ClampToWalls(float targetX, float height)
        {
            float dx = targetX - rb.position.x;
            if (Mathf.Abs(dx) < 0.01f) return targetX;
            float dir = Mathf.Sign(dx);
            Vector2 origin = rb.position + new Vector2(0f, Mathf.Min(height, config.colliderSize.y * 0.5f));
            RaycastHit2D hit = Physics2D.Raycast(origin, new Vector2(dir, 0f), Mathf.Abs(dx) + config.colliderSize.x, GameLayers.WallMask);
            if (hit.collider == null) return targetX;
            float limit = hit.point.x - dir * (config.colliderSize.x * 0.5f + 0.1f);
            return dir > 0f ? Mathf.Min(targetX, limit) : Mathf.Max(targetX, limit);
        }
    }
}
