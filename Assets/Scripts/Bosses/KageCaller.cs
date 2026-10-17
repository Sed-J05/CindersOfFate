using System.Collections;
using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Enemies;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>
    /// Docs/04 §B2 Kage Caller (MB2, 30 HP, guards the orb door).
    /// Regular slash: like MB1 but faster (telegraph 0.35, 2-hit). Black Slash: telegraph 0.5 (raised blade, red glow)
    /// → ground crescent 1.4×0.6 u at 11 u/s, 2 s, breaks on walls; 1 shot, 2 shots 0.5 s apart below 66 %.
    /// Summon: telegraph 0.8 with '!' markers at the X positions where minions will fall → 2 Crawlers/Lungers (50/50)
    /// drop from the ceiling slots; cap 3 alive; cooldown 12 s (8 s below 50 %). Advance / back-step at 3.5 u/s.
    /// Below 50 %: speed ×1.25, telegraphs −0.1 s, summons 3. On death the minions die and flag door_orb opens the door.
    /// </summary>
    public class KageCaller : SwordsmanBoss
    {
        [SerializeField] private GameObject[] minionPrefabs = new GameObject[0];
        [SerializeField] private string unlockFlag = GameConstants.FlagDoorOrb;
        [SerializeField, Tooltip("First summon becomes available this many seconds into the fight.")]
        private float firstSummonDelay = 6f;
        [SerializeField] private float dropHeightFallback = 8f;

        private enum Move { None, Advance, SlashCombo, BlackSlash, Summon, BackStep }

        private Move last = Move.None;
        private BossMoveConfig blackSlash, summon;
        private float summonTimer;
        private readonly List<EnemyBase> minions = new List<EnemyBase>();
        private readonly List<GameObject> markers = new List<GameObject>();
        private readonly List<Vector2> dropPoints = new List<Vector2>(3);
        private readonly RaycastHit2D[] floorHits = new RaycastHit2D[2];

        private const string ProjectileSet = "BlackSlash_Fly";
        private const float MinionCeilingClearance = 0.2f;

        protected override void Awake()
        {
            base.Awake();
            blackSlash = config.FindMove("BlackSlash") ?? new BossMoveConfig("BlackSlash", 0.5f, 0.1f, 0.6f, 0f, 1, 2f) { speed = 11f };
            summon = config.FindMove("Summon") ?? new BossMoveConfig("Summon", 0.8f, 0.1f, 0.8f, 0f, 0, 1f) { cooldown = 12f };
            summonTimer = firstSummonDelay;
        }

        protected override void Update()
        {
            base.Update();
            if (IsFighting && summonTimer > 0f) summonTimer -= Time.deltaTime;
        }

        private int AliveMinions
        {
            get
            {
                int n = 0;
                for (int i = minions.Count - 1; i >= 0; i--)
                {
                    if (minions[i] == null || minions[i].IsDead) minions.RemoveAt(i);
                    else n++;
                }
                return n;
            }
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
                    case Move.BlackSlash: yield return DoBlackSlash(); break;
                    case Move.Summon: yield return DoSummon(); break;
                    case Move.BackStep: yield return DoBackStep(); break;
                }
                last = next;
                Stop();
                Play("Idle", false);
                yield return Wait(config.fairnessGap);
            }
        }

        /// <summary>Summon has priority when ready and below the cap; otherwise weighted random by distance, no repeats.</summary>
        private Move Choose()
        {
            if (last != Move.Summon && summonTimer <= 0f && AliveMinions < config.summonCap && minionPrefabs.Length > 0)
                return Move.Summon;

            float d = DistanceToPlayer;
            bool inReach = d <= slash.reach + 1.2f;
            bool far = d > 5f;
            float wAdvance = last == Move.Advance ? 0f : advance.weight * (inReach ? 0.3f : 1.5f);
            float wSlash = last == Move.SlashCombo ? 0f : slash.weight * (inReach ? 2.5f : 0.2f);
            float wShot = last == Move.BlackSlash ? 0f : blackSlash.weight * (far ? 2f : 0.8f);
            float wBack = last == Move.BackStep ? 0f : backStep.weight * (d < 2f ? 1.5f : 0.2f);
            float total = wAdvance + wSlash + wShot + wBack;
            if (total <= 0f) return Move.Advance;

            float roll = Random.value * total;
            if ((roll -= wAdvance) < 0f) return Move.Advance;
            if ((roll -= wSlash) < 0f) return Move.SlashCombo;
            if ((roll -= wShot) < 0f) return Move.BlackSlash;
            return Move.BackStep;
        }

        private IEnumerator DoBlackSlash()
        {
            FacePlayer();
            Stop();
            Play("Cast_Telegraph");
            TelegraphFlash();
            AudioManager.Play("caller_blackslash_telegraph");
            yield return Wait(Telegraph(blackSlash));

            int shots = config.projectileSecondBelowFraction > 0f && health.Fraction < config.projectileSecondBelowFraction ? 2 : 1;
            for (int i = 0; i < shots; i++)
            {
                if (i > 0)
                {
                    yield return Wait(config.projectileSecondDelay);
                    FacePlayer();
                }
                Play("Cast_Fire");
                FireBlackSlash();
            }
            Play("Idle", false);
            yield return Wait(blackSlash.recovery);
        }

        private void FireBlackSlash()
        {
            float speed = blackSlash.speed > 0f ? blackSlash.speed : config.projectileSpeed;
            Vector2 size = config.projectileSize.sqrMagnitude > 0f ? config.projectileSize : new Vector2(1.4f, 0.6f);
            // Travels along the ground: bottom edge at the floor, so it must be jumped.
            Vector2 spawn = rb.position + new Vector2(facing * (config.colliderSize.x * 0.5f + size.x * 0.5f), size.y * 0.5f + 0.02f);
            ProjectilePool.Fire(ProjectileSet, spawn, new EnemyProjectile.Spec
            {
                velocity = new Vector2(facing * speed, 0f),
                size = size,
                lifetime = config.projectileLifetime > 0f ? config.projectileLifetime : 2f,
                damage = blackSlash.damage > 0 ? blackSlash.damage : config.attackDamage,
                gravity = 0f,
                breakFx = "BlackSlash_Break",
                owner = gameObject,
            });
            AudioManager.Play("caller_blackslash");
        }

        private IEnumerator DoSummon()
        {
            Stop();
            FacePlayer();
            int want = IsEnraged ? config.summonCountEnraged : config.summonCount;
            int count = Mathf.Min(want, config.summonCap - AliveMinions);
            PickDropPoints(count);

            Play("Summon_Raise");
            TelegraphFlash();
            ShowMarkers(true);
            AudioManager.Play("caller_summon");
            float telegraph = Telegraph(summon);
            float t = 0f;
            while (t < telegraph)
            {
                if (ClipFinished("Summon_Raise")) Play("Summon_Hold", false);
                t += Time.deltaTime;
                yield return null;
            }
            ShowMarkers(false);

            for (int i = 0; i < dropPoints.Count; i++) SpawnMinion(dropPoints[i]);
            summonTimer = IsEnraged && config.summonCooldownEnraged > 0f ? config.summonCooldownEnraged
                        : (config.summonCooldown > 0f ? config.summonCooldown : summon.cooldown);

            Play("Idle", false);
            yield return Wait(summon.recovery);
        }

        /// <summary>Arena ceiling slots '1'–'9' when bound to an arena (random distinct), else spots around the player (Sandbox).</summary>
        private void PickDropPoints(int count)
        {
            dropPoints.Clear();
            if (count <= 0) return;
            IReadOnlyList<Vector2> slots = Arena != null ? Arena.DropSlots : null;
            if (slots != null && slots.Count > 0)
            {
                var order = new List<int>(slots.Count);
                for (int i = 0; i < slots.Count; i++) order.Add(i);
                for (int i = order.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (order[i], order[j]) = (order[j], order[i]); }
                for (int i = 0; i < count && i < order.Count; i++) dropPoints.Add(slots[order[i]]);
                return;
            }
            float baseX = TryGetPlayer(out var p) ? p.Position.x : rb.position.x;
            float[] offsets = { -3f, 3f, 0f };
            for (int i = 0; i < count && i < offsets.Length; i++)
                dropPoints.Add(new Vector2(baseX + offsets[i], rb.position.y + dropHeightFallback));
        }

        /// <summary>'!' markers on the floor under each drop point for the telegraph (Docs/04: at the X positions).</summary>
        private void ShowMarkers(bool on)
        {
            if (!on)
            {
                for (int i = 0; i < markers.Count; i++) if (markers[i] != null) markers[i].SetActive(false);
                return;
            }
            var filter = new ContactFilter2D();
            filter.SetLayerMask(GameLayers.WalkableMask);
            filter.useTriggers = false;
            SpriteAnimationSet set = SpriteAnimationSet.Load("Exclamation");
            for (int i = 0; i < dropPoints.Count; i++)
            {
                if (i >= markers.Count) markers.Add(CreateMarker(set));
                GameObject m = markers[i];
                Vector2 at = dropPoints[i];
                if (Physics2D.Raycast(at, Vector2.down, filter, floorHits, 30f) > 0) at = floorHits[0].point + new Vector2(0f, 0.6f);
                m.transform.position = at;
                m.SetActive(true);
                var anim = m.GetComponent<SpriteAnimator>();
                if (anim != null && anim.Set != null) anim.Play(FxPool.DefaultClip, true);
            }
        }

        private GameObject CreateMarker(SpriteAnimationSet set)
        {
            var go = new GameObject("DropMarker", typeof(SpriteRenderer), typeof(SpriteAnimator));
            go.transform.SetParent(transform.parent, true);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingLayerName = GameLayers.Sorting.FX;
            var anim = go.GetComponent<SpriteAnimator>();
            anim.Set = set;
            if (set == null) { sr.sprite = PlaceholderSprites.Circle; sr.color = new Color(1f, 0.25f, 0.2f, 0.9f); go.transform.localScale = Vector3.one * 0.5f; }
            go.SetActive(false);
            return go;
        }

        private void SpawnMinion(Vector2 at)
        {
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
            if (prefab == null) return;
            // D-125: drop slots sit in the row under the ceiling; spawning the feet there put the body inside the
            // ceiling and physics pushed it out on top. Lower the feet so the whole body starts in open air.
            var template = prefab.GetComponent<EnemyBase>();
            float bodyHeight = template != null && template.Config != null ? template.Config.bodySize.y : 1f;
            Vector2 feet = at - new Vector2(0f, 0.25f + bodyHeight + MinionCeilingClearance);
            GameObject go = Instantiate(prefab, feet, Quaternion.identity, transform.parent);
            var enemy = go.GetComponent<EnemyBase>();
            if (enemy != null) minions.Add(enemy);
        }

        private void KillMinions()
        {
            for (int i = 0; i < minions.Count; i++)
                if (minions[i] != null && !minions[i].IsDead) minions[i].Health.Kill(gameObject);
            minions.Clear();
            ShowMarkers(false);
        }

        protected override void OnDeathStarted()
        {
            KillMinions();
            if (!string.IsNullOrEmpty(unlockFlag) && GameManager.HasInstance) GameManager.Instance.SetFlag(unlockFlag, true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Fight abandoned (player died) or finished: no stray minions or markers stay behind.
            for (int i = 0; i < minions.Count; i++) if (minions[i] != null) Destroy(minions[i].gameObject);
            minions.Clear();
            for (int i = 0; i < markers.Count; i++) if (markers[i] != null) Destroy(markers[i]);
            markers.Clear();
        }
    }
}
