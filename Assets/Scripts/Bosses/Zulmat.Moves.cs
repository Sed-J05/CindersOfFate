using System.Collections;
using Shatranj.Core;
using Shatranj.Enemies;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>Zulmat's six moves (Docs/04 §E). Numbers come from the BossConfig moves list and its "Zulmat" section.</summary>
    public partial class Zulmat
    {
        private const float SwipeArtWidth = 7.68f;   // Giant_Swipe 768 px at 100 px/u, pivot left-centre
        private const float VineWarnArtWidth = 1.92f;
        private const float SmokeTime = 0.25f;
        private const float LandTimeout = 1.5f;

        // ------------------------------------------------------------------
        // Sword Slash — telegraph 0.4 → 2-hit combo, reach 2.8, dmg 1 (second hit turns if the player got behind)
        // ------------------------------------------------------------------

        private IEnumerator DoSwordSlash()
        {
            if (DistanceToPlayer > slash.reach + 0.4f) yield return Walk(1.2f, slash.reach * 0.8f);
            Stop();
            BeginTelegraph("Slash_Telegraph");
            AudioManager.Play("zulmat_telegraph");
            yield return Wait(Telegraph(slash));

            Play("Slash_A");
            AudioManager.Play("zulmat_slash");
            yield return HitWindow(slash.reach, slash.active, slash.damage, config.colliderSize.y, 0f, 0f);
            for (int i = 1; i < Mathf.Max(1, slash.hits); i++)
            {
                yield return Wait(slash.hitGap);
                if (PlayerIsBehind) SetFacing(-facing);
                Play(i % 2 == 1 ? "Slash_B" : "Slash_A");
                AudioManager.Play("zulmat_slash");
                yield return HitWindow(slash.reach, slash.active, slash.damage, config.colliderSize.y, 0f, 0f);
            }
            Play("Idle", false);
            yield return Wait(slash.recovery);
        }

        // ------------------------------------------------------------------
        // Vine Summon — telegraph 0.8 with a ground glow at the player's X, then vines erupt one after another,
        // spaced 3 u, marching toward the far wall, 0.35 s apart, each staying 0.7 s
        // ------------------------------------------------------------------

        private IEnumerator DoVineSummon()
        {
            Stop();
            float startX = Mathf.Clamp(PlayerX(), leftWall + config.vineSize.x, rightWall - config.vineSize.x);
            float dir = Mathf.Sign(startX - rb.position.x);
            if (dir == 0f) dir = facing;
            BeginTelegraph("Vine_Cast");
            AudioManager.Play("zulmat_cast");
            float tel = Telegraph(vine);
            TimedFx.Spawn("Vine_Warn", new Vector2(startX, floorY), tel, config.vineSize.x / VineWarnArtWidth * 1.5f, GameLayers.Sorting.FX);
            yield return Wait(tel);

            for (int i = 0; i < Mathf.Max(1, config.vineCount); i++)
            {
                float x = startX + dir * config.vineSpacing * i;
                if (x < leftWall + config.vineSize.x * 0.5f || x > rightWall - config.vineSize.x * 0.5f) break;
                if (i > 0)
                {
                    // Each following vine gets a short glow first so the march stays readable.
                    TimedFx.Spawn("Vine_Warn", new Vector2(x, floorY), config.vineInterval, config.vineSize.x / VineWarnArtWidth * 1.5f, GameLayers.Sorting.FX);
                    yield return Wait(config.vineInterval);
                }
                vines.Add(VineHazard.Spawn(new Vector2(x, floorY), config.vineSize, config.vineStayTime, vine.damage, gameObject, transform.parent));
            }
            Play("Idle", false);
            yield return Wait(vine.recovery);
        }

        // ------------------------------------------------------------------
        // Giant Swipe — grow ×1.8 (0.5 s, rooted, still damageable), telegraph 0.8, sweep 6 u in front from the floor
        // up to 2.5 u (jump above it or dash through it), then shrink back
        // ------------------------------------------------------------------

        private IEnumerator DoGiantSwipe()
        {
            FacePlayer();
            Stop();
            Play("Grow");
            AudioManager.Play("zulmat_grow");
            yield return ScaleTo(config.giantScale, Mathf.Max(0.05f, config.transformTime));

            BeginTelegraph("Swipe_Telegraph");
            AudioManager.Play("zulmat_telegraph");
            yield return Wait(Telegraph(swipe));

            Play("Swipe");
            AudioManager.Play("zulmat_swipe");
            ScreenShake.Shake(0.2f, 0.25f);
            FxPool.Play("Giant_Swipe", new Vector2(rb.position.x, floorY + config.swipeHeight * 0.5f), facing, swipe.reach / SwipeArtWidth);
            // Full floor width in front: from the body centre to `reach`, from the floor to `swipeHeight`.
            yield return HitWindow(swipe.reach, swipe.active, swipe.damage, config.swipeHeight, 0f, 0f);

            Play("Idle", false);
            yield return Wait(swipe.recovery);
            Play("Shrink");
            yield return ScaleTo(1f, Mathf.Max(0.05f, config.transformTime));
        }

        // ------------------------------------------------------------------
        // Shrink & Lunge — shrink ×0.5 (0.4 s), vanish in smoke, reappear at an arena edge at the player's height,
        // telegraph 0.35, lunge across at 16 u/s (hit box follows), twice (second from the opposite side), small slash
        // ------------------------------------------------------------------

        private IEnumerator DoShrinkLunge()
        {
            Stop();
            Play("Shrink");
            AudioManager.Play("zulmat_shrink");
            yield return ScaleTo(config.shrinkScale, Mathf.Max(0.05f, config.shrinkTime));

            FxPool.Play("Death_Smoke", rb.position + new Vector2(0f, config.colliderSize.y * FormScale * 0.5f), 1, 1.2f);
            SetVisible(false);
            SetKinematic(true);
            yield return Wait(SmokeTime);

            // Start on the side away from the player so the lunge crosses toward them.
            int side = PlayerX() < ArenaCenterX ? 1 : -1;
            float halfWidth = config.colliderSize.x * FormScale * 0.5f;
            for (int n = 0; n < Mathf.Max(1, config.lungeCount); n++)
            {
                float y = TryGetPlayer(out PlayerController p) ? Mathf.Max(floorY, p.Position.y) : floorY;
                WallsAt(y + 0.3f, out float lx, out float rx);
                float minX = lx + halfWidth + config.lungeEdgeMargin, maxX = rx - halfWidth - config.lungeEdgeMargin;
                float startX = side > 0 ? maxX : minX, endX = side > 0 ? minX : maxX;

                rb.position = new Vector2(startX, y);
                transform.position = rb.position;
                SetFacing(-side);
                SetVisible(true);
                FxPool.Play("Death_Smoke", rb.position + new Vector2(0f, 0.4f), 1, 0.8f);
                BeginTelegraph("Lunge_Telegraph");
                SetFacing(-side);   // BeginTelegraph faces the player; the lunge always crosses the room
                AudioManager.Play("zulmat_telegraph");
                yield return Wait(Telegraph(lunge));

                Play("Lunge");
                AudioManager.Play("zulmat_lunge");
                float speed = config.lungeSpeed > 0f ? config.lungeSpeed : 16f;
                bool hit = false;
                while (!Mathf.Approximately(rb.position.x, endX))
                {
                    Vector2 next = new Vector2(Mathf.MoveTowards(rb.position.x, endX, speed * Time.deltaTime), y);
                    rb.MovePosition(next);
                    rb.position = next;
                    if (!hit)
                    {
                        Vector2 size = new Vector2(config.colliderSize.x, config.colliderSize.y) * FormScale;
                        hit = TryHitPlayer(next + new Vector2(0f, size.y * 0.5f), size, lunge.damage);
                    }
                    yield return null;
                }
                side = -side;   // the next lunge starts from this side, toward the other
            }

            // Small closing slash.
            FacePlayer();
            Play("Slash_A");
            yield return HitWindow(config.lungeEndSlashReach, slash.active, lunge.damage, config.colliderSize.y * FormScale, 0f, 0f);

            SetKinematic(false);
            yield return WaitForLanding();
            Play("Grow");
            yield return ScaleTo(1f, Mathf.Max(0.05f, config.transformTime));
            Play("Idle", false);
            yield return Wait(lunge.recovery);
        }

        // ------------------------------------------------------------------
        // Reposition (team addition, D-168) — shrink, vanish in smoke, reappear on the far side of the arena (away
        // from where he stood, never on top of the player), grow back. Not an attack; the next move follows the gap.
        // ------------------------------------------------------------------

        private IEnumerator DoReposition()
        {
            Stop();
            BeginTelegraph("Shrink");
            AudioManager.Play("zulmat_shrink");
            yield return ScaleTo(config.shrinkScale, Mathf.Max(0.05f, config.shrinkTime));

            FxPool.Play("Death_Smoke", rb.position + new Vector2(0f, config.colliderSize.y * FormScale * 0.5f), 1, 1.2f);
            SetVisible(false);
            SetKinematic(true);
            yield return Wait(config.repositionHideTime);

            Vector2 dest = RepositionTarget();
            rb.position = dest;
            transform.position = dest;
            SetKinematic(false);
            SetVisible(true);
            FacePlayer();
            FxPool.Play("Death_Smoke", dest + new Vector2(0f, 0.6f), 1, 1.2f);
            yield return WaitForLanding();

            Play("Grow");
            yield return ScaleTo(1f, Mathf.Max(0.05f, config.transformTime));
            Play("Idle", false);
            yield return Wait(reposition.recovery);
        }

        /// <summary>Floor spot near the wall on the side opposite to where he is now, kept at least 3 u from the player.</summary>
        private Vector2 RepositionTarget()
        {
            float half = config.colliderSize.x * 0.5f;
            bool goRight = rb.position.x < ArenaCenterX;
            float x = goRight ? rightWall - config.repositionWallGap - half : leftWall + config.repositionWallGap + half;
            float px = PlayerX();
            const float minGap = 3f;
            if (Mathf.Abs(x - px) < minGap) x = px + (goRight ? -minGap : minGap);
            x = Mathf.Clamp(x, leftWall + half + 0.2f, rightWall - half - 0.2f);

            // Land on whatever floor is there (the dais raises it near the throne).
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, ceilingY - 0.5f), Vector2.down, ceilingY - floorY + 2f, GameLayers.WalkableMask);
            float y = hit.collider != null ? hit.point.y : floorY;
            return new Vector2(x, y);
        }

        // ------------------------------------------------------------------
        // Summon Regulars — while small: fly to ceiling-centre, summon 2 (Knight, Bat) on both sides, cap 4 alive,
        // vulnerable the whole time, cooldown 20 s (−20 % in stage 3)
        // ------------------------------------------------------------------

        private IEnumerator DoSummonRegulars()
        {
            Stop();
            Play("Shrink");
            AudioManager.Play("zulmat_shrink");
            yield return ScaleTo(config.shrinkScale, Mathf.Max(0.05f, config.shrinkTime));

            SetKinematic(true);
            var perch = new Vector2(ArenaCenterX, Mathf.Max(floorY, ceilingY - config.summonHoverBelowCeiling - config.colliderSize.y * FormScale));
            while ((rb.position - perch).sqrMagnitude > 0.0025f)
            {
                Vector2 next = Vector2.MoveTowards(rb.position, perch, FlySpeed * Time.deltaTime);
                rb.MovePosition(next);
                rb.position = next;
                yield return null;
            }

            int want = IsEnraged && config.summonCountEnraged > 0 ? config.summonCountEnraged : config.summonCount;
            int count = Mathf.Clamp(Mathf.Min(want, config.summonCap - AliveMinions), 0, 8);
            var points = new Vector2[count];
            bool batLeft = Random.value < 0.5f;
            for (int i = 0; i < count; i++)
            {
                float sideSign = (i % 2 == 0) ? -1f : 1f;
                float x = Mathf.Clamp(ArenaCenterX + sideSign * config.summonSideOffset, leftWall + 1f, rightWall - 1f);
                bool bat = IsBatSlot(i, batLeft);
                points[i] = new Vector2(x, bat ? floorY + config.summonBatHeight : floorY);
                TimedFx.Spawn("Exclamation", points[i] + new Vector2(0f, bat ? 0f : 1.2f), Telegraph(summon), 1f, GameLayers.Sorting.FX, 10);
            }

            BeginTelegraph("Summon_Cast");
            AudioManager.Play("zulmat_summon");
            yield return Wait(Telegraph(summon));
            for (int i = 0; i < count; i++) SpawnMinion(points[i], IsBatSlot(i, batLeft));
            summonCooldown = summon.cooldown * CooldownMultiplier;
            yield return Wait(summon.active);   // keeps hovering, small and hittable

            SetKinematic(false);
            yield return WaitForLanding();
            Play("Grow");
            yield return ScaleTo(1f, Mathf.Max(0.05f, config.transformTime));
            Play("Idle", false);
            yield return Wait(summon.recovery);
        }

        /// <summary>Slot 0 is the left side, slot 1 the right; one side gets the Knight, the other the Bat.</summary>
        private static bool IsBatSlot(int slot, bool batLeft) => (slot % 2 == 0) == batLeft;

        private void SpawnMinion(Vector2 at, bool bat)
        {
            GameObject prefab = null;
            for (int i = 0; i < minionPrefabs.Length; i++)
            {
                if (minionPrefabs[i] == null) continue;
                bool isBat = minionPrefabs[i].GetComponent<Bat>() != null;
                if (isBat == bat) { prefab = minionPrefabs[i]; break; }
            }
            if (prefab == null) prefab = minionPrefabs.Length > 0 ? minionPrefabs[0] : null;
            if (prefab == null) return;

            FxPool.Play("Death_Smoke", at + new Vector2(0f, 0.8f), 1, 1f);
            GameObject go = Instantiate(prefab, at, Quaternion.identity, transform.parent);
            if (go.TryGetComponent(out Bat b)) b.SpawnAwake();
            if (go.TryGetComponent(out EnemyBase enemy)) minions.Add(enemy);
        }

        // ------------------------------------------------------------------
        // Lava Zones (stage 3) — telegraph 1.0 (glowing cracks, 4–6 u wide) → lava for 6 s, max 3 zones
        // ------------------------------------------------------------------

        private IEnumerator DoLavaZones()
        {
            Stop();
            BeginTelegraph("Lava_Cast");
            AudioManager.Play("zulmat_cast");
            float tel = Telegraph(lava);

            int free = Mathf.Max(1, config.maxLavaZones) - ActiveLavaZones;
            int make = Mathf.Min(free, Mathf.Max(1, config.lavaZonesPerCast));
            for (int i = 0; i < make; i++)
            {
                float width = Random.Range(config.lavaZoneWidthMin, Mathf.Max(config.lavaZoneWidthMin, config.lavaZoneWidthMax));
                // First zone under the player (forces a jump), the rest split the remaining floor.
                float x = i == 0 ? PlayerX() : Random.Range(leftWall + width * 0.5f + 1f, rightWall - width * 0.5f - 1f);
                if (!TryPlaceLava(ref x, width)) continue;
                lavaZones.Add(LavaZoneHazard.Spawn(new Vector2(x, floorY), width, tel, config.lavaDuration, config.lavaRiseTime, lava.damage, transform.parent));
            }
            yield return Wait(tel);
            Play("Idle", false);
            yield return Wait(lava.recovery);
        }

        /// <summary>Keeps a zone inside the walls and off the other zones; false when there is no room.</summary>
        private bool TryPlaceLava(ref float x, float width)
        {
            float min = leftWall + width * 0.5f + 0.5f, max = rightWall - width * 0.5f - 0.5f;
            if (min > max) return false;
            x = Mathf.Clamp(x, min, max);
            for (int attempt = 0; attempt < 8; attempt++)
            {
                bool clear = true;
                for (int i = 0; i < lavaZones.Count; i++)
                    if (lavaZones[i] != null && !lavaZones[i].Finished && lavaZones[i].Overlaps(x, width)) { clear = false; break; }
                if (clear) return true;
                x = Random.Range(min, max);
            }
            return false;
        }

        // ------------------------------------------------------------------

        private IEnumerator WaitForLanding()
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(GameLayers.WalkableMask);
            filter.useTriggers = false;
            var probe = new Collider2D[1];
            for (float t = 0f; t < LandTimeout; t += Time.deltaTime)
            {
                var size = new Vector2(config.colliderSize.x * FormScale * 0.9f, 0.1f);
                if (rb.linearVelocity.y <= 0.01f && Physics2D.OverlapBox(rb.position + new Vector2(0f, -0.03f), size, 0f, filter, probe) > 0) yield break;
                yield return null;
            }
        }
    }
}
