using System.Collections.Generic;
using NUnit.Framework;
using Shatranj.Bosses;
using Shatranj.Data;
using Shatranj.EditorTools;
using UnityEngine;

namespace Shatranj.Tests
{
    /// <summary>Prompt 15: Zulmat numbers (Docs/04 §E), stage thresholds, and move selection rules.</summary>
    public class ZulmatTests
    {
        private const float Eps = 1e-4f;

        private static BossConfig Zulmat()
        {
            var b = ScriptableObject.CreateInstance<BossConfig>();
            ConfigDefaults.Zulmat(b);
            return b;
        }

        [Test]
        public void Zulmat_MatchesDocs04()
        {
            BossConfig b = Zulmat();
            Assert.AreEqual("zulmat", b.id);
            Assert.AreEqual(90, b.maxHp);
            Assert.AreEqual(1, b.contactDamage);
            Assert.AreEqual(new Vector2(2.0f, 3.2f), b.bodySize);
            Assert.AreEqual(new Vector2(1.3f, 3.0f), b.colliderSize);
            CollectionAssert.AreEqual(new[] { 0.67f, 0.34f }, b.stageThresholds);
            Assert.AreEqual(1.5f, b.stageTransitionInvulnerability, Eps);
            Assert.AreEqual(2.5f, b.deathSlowmoTime, Eps);
            Assert.AreEqual(0.6f, b.fairnessGap, Eps);
            Assert.IsTrue(b.noImmediateRepeat);
            Assert.IsTrue(b.flameParksDuringFight);
            Assert.AreEqual(new Vector2(0.5f, 0.85f), b.flameParkViewport);
            Assert.AreEqual(0.15f, b.enrageTelegraphReduction, Eps, "stage 3: telegraphs −0.15 s");
            Assert.AreEqual(0.8f, b.enrageCooldownMultiplier, Eps, "stage 3: cooldowns −20 %");
            Assert.AreEqual(0.34f, b.enrageBelowFraction, Eps, "enrage = stage 3");

            BossMoveConfig slash = b.FindMove("SwordSlash");
            Assert.AreEqual(0.4f, slash.telegraph, Eps); Assert.AreEqual(2.8f, slash.reach, Eps);
            Assert.AreEqual(2, slash.hits); Assert.AreEqual(1, slash.minStage);

            Assert.AreEqual(0.8f, b.FindMove("VineSummon").telegraph, Eps);
            Assert.AreEqual(3, b.vineCount); Assert.AreEqual(3f, b.vineSpacing, Eps);
            Assert.AreEqual(0.35f, b.vineInterval, Eps); Assert.AreEqual(0.7f, b.vineStayTime, Eps);
            Assert.AreEqual(new Vector2(1f, 3f), b.vineSize);

            BossMoveConfig swipe = b.FindMove("GiantSwipe");
            Assert.AreEqual(0.8f, swipe.telegraph, Eps); Assert.AreEqual(6f, swipe.reach, Eps);
            Assert.AreEqual(2.5f, b.swipeHeight, Eps); Assert.AreEqual(1.8f, b.giantScale, Eps);
            Assert.AreEqual(0.5f, b.transformTime, Eps, "grow time");
            Assert.Less(b.swipeHeight, 2.6f, "jumping above 2.6 u clears it");

            BossMoveConfig lunge = b.FindMove("ShrinkLunge");
            Assert.AreEqual(0.35f, lunge.telegraph, Eps); Assert.AreEqual(2, lunge.minStage);
            Assert.AreEqual(0.5f, b.shrinkScale, Eps); Assert.AreEqual(0.4f, b.shrinkTime, Eps);
            Assert.AreEqual(16f, b.lungeSpeed, Eps); Assert.AreEqual(2, b.lungeCount);

            BossMoveConfig summon = b.FindMove("SummonRegulars");
            Assert.AreEqual(2, summon.minStage); Assert.AreEqual(20f, summon.cooldown, Eps);
            Assert.AreEqual(2, b.summonCount); Assert.AreEqual(4, b.summonCap);
            CollectionAssert.AreEquivalent(new[] { "knight", "bat" }, b.summonEnemyIds);

            BossMoveConfig lava = b.FindMove("LavaZones");
            Assert.AreEqual(1f, lava.telegraph, Eps); Assert.AreEqual(3, lava.minStage);
            Assert.AreEqual(6f, b.lavaDuration, Eps); Assert.AreEqual(3, b.maxLavaZones);
            Assert.AreEqual(4f, b.lavaZoneWidthMin, Eps); Assert.AreEqual(6f, b.lavaZoneWidthMax, Eps);

            foreach (BossMoveConfig m in b.moves)
                if (m.damage > 0) Assert.AreEqual(1, m.damage, $"{m.name}: Zulmat hits deal 1");
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Stages_At67And34Percent()
        {
            BossConfig b = Zulmat();
            Assert.AreEqual(3, b.StageCount);
            Assert.AreEqual(1, b.StageForFraction(1f));
            Assert.AreEqual(1, b.StageForFraction(0.68f));
            Assert.AreEqual(2, b.StageForFraction(0.67f));
            Assert.AreEqual(2, b.StageForFraction(0.35f));
            Assert.AreEqual(3, b.StageForFraction(0.34f));
            Assert.AreEqual(3, b.StageForFraction(0.01f));
            Object.DestroyImmediate(b);
        }

        private static HashSet<string> Reachable(BossConfig b, int stage, List<string> history, System.Predicate<BossMoveConfig> available = null)
        {
            var set = new HashSet<string>();
            for (int i = 0; i < 200; i++)
            {
                BossMoveConfig m = BossMoveSelector.Pick(b.moves, BossMoveSelector.AnyBand, stage, history, 1, (i + 0.5f) / 200f, available);
                if (m != null) set.Add(m.name);
            }
            return set;
        }

        [Test]
        public void MovesUnlockByStage()
        {
            BossConfig b = Zulmat();
            var none = new List<string>();
            CollectionAssert.AreEquivalent(new[] { "SwordSlash", "VineSummon", "GiantSwipe", "Reposition" }, Reachable(b, 1, none));
            CollectionAssert.AreEquivalent(new[] { "SwordSlash", "VineSummon", "GiantSwipe", "Reposition", "ShrinkLunge", "SummonRegulars" }, Reachable(b, 2, none));
            CollectionAssert.AreEquivalent(new[] { "SwordSlash", "VineSummon", "GiantSwipe", "Reposition", "ShrinkLunge", "SummonRegulars", "LavaZones" }, Reachable(b, 3, none));
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Variety_AvoidsTheLastTwoMoves_FallsBackWhenNeeded()
        {
            BossConfig b = Zulmat();
            Assert.AreEqual(2, b.noRepeatWithinLast);
            var history = new List<string> { "VineSummon", "SwordSlash" };
            for (int i = 0; i < 100; i++)
            {
                BossMoveConfig m = BossMoveSelector.PickVaried(b.moves, BossMoveSelector.AnyBand, 1, history, b.noRepeatWithinLast, (i + 0.5f) / 100f);
                Assert.AreNotEqual("SwordSlash", m.name);
                Assert.AreNotEqual("VineSummon", m.name, "used two moves ago: avoided while other moves exist");
            }
            // Only the two recent moves are allowed: fall back to no-immediate-repeat, so the older one comes back.
            System.Predicate<BossMoveConfig> onlyRecent = m => m.name == "SwordSlash" || m.name == "VineSummon";
            Assert.AreEqual("VineSummon", BossMoveSelector.PickVaried(b.moves, BossMoveSelector.AnyBand, 1, history, 2, 0.5f, onlyRecent).name);
            Assert.IsTrue(BossMoveSelector.UsedRecently(history, "VineSummon", 2));
            Assert.IsFalse(BossMoveSelector.UsedRecently(history, "VineSummon", 1));
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Reposition_IsAStageOneNonAttack_AndOldAssetsGetIt()
        {
            BossConfig b = Zulmat();
            BossMoveConfig r = b.FindMove("Reposition");
            Assert.IsNotNull(r);
            Assert.AreEqual(1, r.minStage);
            Assert.AreEqual(0, r.damage);
            Assert.IsFalse(ConfigDefaults.AddMissingZulmatMoves(b), "already present");

            var old = ScriptableObject.CreateInstance<BossConfig>();
            ConfigDefaults.Zulmat(old);
            old.moves = System.Array.FindAll(old.moves, m => m.name != "Reposition");
            old.FindMove("SwordSlash").telegraph = 0.55f;   // a tuned value must survive the migration
            Assert.IsTrue(ConfigDefaults.AddMissingZulmatMoves(old));
            Assert.IsNotNull(old.FindMove("Reposition"));
            Assert.AreEqual(0.55f, old.FindMove("SwordSlash").telegraph, 1e-5f);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(old);
        }

        [Test]
        public void NoImmediateRepeat_AndAvailabilityGates()
        {
            BossConfig b = Zulmat();
            foreach (string last in new[] { "SwordSlash", "VineSummon", "GiantSwipe", "Reposition", "ShrinkLunge", "SummonRegulars", "LavaZones" })
                CollectionAssert.DoesNotContain(Reachable(b, 3, new List<string> { last }), last, $"{last} right after {last}");

            // Summon on cooldown / at the cap, lava at 3 zones: neither can be picked.
            System.Predicate<BossMoveConfig> blocked = m => m.name != "SummonRegulars" && m.name != "LavaZones";
            HashSet<string> picks = Reachable(b, 3, new List<string>(), blocked);
            CollectionAssert.DoesNotContain(picks, "SummonRegulars");
            CollectionAssert.DoesNotContain(picks, "LavaZones");
            Assert.IsNotEmpty(picks);
            Object.DestroyImmediate(b);
        }
    }
}
