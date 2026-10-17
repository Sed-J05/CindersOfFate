using System.Collections.Generic;
using NUnit.Framework;
using Shatranj.Bosses;
using Shatranj.Data;
using Shatranj.EditorTools;
using Shatranj.EditorTools.Level;
using Shatranj.Level;
using UnityEngine;

namespace Shatranj.Tests
{
    /// <summary>Prompt 13: Iron Colossus numbers (Docs/04 §D), its distance-based selection, and Section C rooms (Docs/05 §5).</summary>
    public class ColossusAndSectionCTests
    {
        private const float Eps = 1e-4f;

        private static BossConfig Colossus()
        {
            var b = ScriptableObject.CreateInstance<BossConfig>();
            ConfigDefaults.IronColossus(b);
            return b;
        }

        [Test]
        public void IronColossus_MatchesDocs04()
        {
            BossConfig b = Colossus();
            Assert.AreEqual("iron_colossus", b.id);
            Assert.AreEqual(45, b.maxHp);
            Assert.AreEqual(1, b.contactDamage, "contact deals 1");
            Assert.AreEqual(1.8f, b.moveSpeed, Eps);
            Assert.IsTrue(b.neverStaggers);
            Assert.AreEqual(new Vector2(2.2f, 2.6f), b.bodySize);
            Assert.AreEqual(new Vector2(1.8f, 2.4f), b.colliderSize);
            Assert.AreEqual(0.4f, b.enrageBelowFraction, Eps);
            Assert.AreEqual(0.15f, b.enrageTelegraphReduction, Eps);
            Assert.AreEqual(1.5f, b.deathSlowmoTime, Eps);
            Assert.AreEqual(2, b.maxSameMoveInRow, "never 3x in a row");
            Assert.IsFalse(b.flameParksDuringFight, "Docs/07: during MB3 the flame keeps moving");

            BossMoveConfig arc = b.FindMove("BigArcSlash");
            Assert.AreEqual(0.7f, arc.telegraph, Eps); Assert.AreEqual(3.2f, arc.reach, Eps);
            Assert.AreEqual(0.15f, arc.active, Eps); Assert.AreEqual(0.9f, arc.recovery, Eps);
            Assert.AreEqual(2f, b.FindMove("ShieldBlock").duration, Eps);
            BossMoveConfig bash = b.FindMove("ShieldBash");
            Assert.AreEqual(0.3f, bash.telegraph, Eps); Assert.AreEqual(2f, bash.reach, Eps);
            BossMoveConfig jump = b.FindMove("JumpSlam");
            Assert.AreEqual(0.6f, jump.telegraph, Eps); Assert.AreEqual(3.5f, jump.extra, Eps, "apex");
            Assert.AreEqual(0.7f, jump.active, Eps, "air time"); Assert.AreEqual(2f, jump.reach, Eps, "shockwave radius");
            BossMoveConfig dash = b.FindMove("Dash");
            Assert.AreEqual(0.5f, dash.telegraph, Eps); Assert.AreEqual(12f, dash.speed, Eps);
            Assert.AreEqual(0.4f, dash.duration, Eps); Assert.AreEqual(0.8f, dash.recovery, Eps);
            Assert.AreEqual(4.8f, dash.speed * dash.duration, Eps, "≈ 5 u");

            foreach (BossMoveConfig m in b.moves)
                if (m.name != "ShieldBlock") Assert.AreEqual(2, m.damage, $"{m.name}: every Colossus hit deals 2 hearts");
            Object.DestroyImmediate(b);
        }

        [Test]
        public void JumpSlam_ArcReachesApexInAirTime()
        {
            // IronColossus.DoJumpSlam: g = 8h/T², vy = 4h/T → peak height h at T/2, back on the ground at T.
            const float h = 3.5f, T = 0.7f;
            float g = 8f * h / (T * T), vy = 4f * h / T;
            float tPeak = vy / g;
            Assert.AreEqual(T * 0.5f, tPeak, 1e-4f);
            Assert.AreEqual(h, vy * tPeak - 0.5f * g * tPeak * tPeak, 1e-4f);
            Assert.AreEqual(0f, vy * T - 0.5f * g * T * T, 1e-4f);
        }

        [Test]
        public void Bands_CloseMidFar()
        {
            Assert.AreEqual(BossMoveSelector.Close, BossMoveSelector.BandFor(2f, 3.5f, 7f));
            Assert.AreEqual(BossMoveSelector.Mid, BossMoveSelector.BandFor(3.5f, 3.5f, 7f));
            Assert.AreEqual(BossMoveSelector.Mid, BossMoveSelector.BandFor(7f, 3.5f, 7f));
            Assert.AreEqual(BossMoveSelector.Far, BossMoveSelector.BandFor(9f, 3.5f, 7f));
        }

        [Test]
        public void Selection_FollowsDistanceTable()
        {
            BossConfig b = Colossus();
            var none = new List<string>();
            var far = new HashSet<string>(); var mid = new HashSet<string>(); var close = new HashSet<string>();
            for (int i = 0; i < 100; i++)
            {
                float roll = (i + 0.5f) / 100f;
                far.Add(BossMoveSelector.Pick(b.moves, BossMoveSelector.Far, 1, none, b.maxSameMoveInRow, roll).name);
                mid.Add(BossMoveSelector.Pick(b.moves, BossMoveSelector.Mid, 1, none, b.maxSameMoveInRow, roll).name);
                close.Add(BossMoveSelector.Pick(b.moves, BossMoveSelector.Close, 1, none, b.maxSameMoveInRow, roll).name);
            }
            CollectionAssert.AreEquivalent(new[] { "Dash", "JumpSlam" }, far, "far → Dash or Jump");
            CollectionAssert.AreEquivalent(new[] { "JumpSlam", "BigArcSlash" }, mid, "mid → Jump or Arc");
            CollectionAssert.AreEquivalent(new[] { "BigArcSlash", "ShieldBlock", "ShieldBash" }, close, "close → Arc, Block, Bash");
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Selection_NeverRepeatsAMoveThreeTimes()
        {
            BossConfig b = Colossus();
            var history = new List<string> { "Dash", "Dash" };
            for (int i = 0; i < 50; i++)
            {
                BossMoveConfig m = BossMoveSelector.Pick(b.moves, BossMoveSelector.Far, 1, history, b.maxSameMoveInRow, (i + 0.5f) / 50f);
                Assert.AreEqual("JumpSlam", m.name, "Dash twice → the third far move must be the jump");
            }
            history = new List<string> { "Dash", "JumpSlam", "JumpSlam" };
            Assert.AreEqual("Dash", BossMoveSelector.Pick(b.moves, BossMoveSelector.Far, 1, history, b.maxSameMoveInRow, 0.99f).name);
            Assert.AreEqual(2, BossMoveSelector.TrailingCount(history, "JumpSlam"));
            Assert.AreEqual(0, BossMoveSelector.TrailingCount(history, "Dash"));
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Selection_RespectsMinStage_AndReturnsNullWhenNothingFits()
        {
            var moves = new[] { new BossMoveConfig("Late", 0.5f, 0.1f, 0.5f, 1f, 1) { minStage = 2 } };
            Assert.IsNull(BossMoveSelector.Pick(moves, BossMoveSelector.Close, 1, new List<string>(), 2, 0.5f));
            Assert.AreEqual("Late", BossMoveSelector.Pick(moves, BossMoveSelector.Close, 2, new List<string>(), 2, 0.5f).name);
        }

        // ---------------- Section C rooms (real files) ----------------

        [Test]
        public void SectionC_HasTwentyEnemies_KnightsAndBatsOnly_AndABossRoom()
        {
            LevelSet set = LevelParser.Load(LevelBuilder.MainManifestPath, LevelBuilder.LevelsFolder);
            Assert.IsEmpty(set.errors, string.Join("\n", set.errors));
            LevelManifestPhase p2 = set.manifest.phases.Find(p => p.id == "P2");
            Assert.IsNotNull(p2);
            int count = 0, bosses = 0;
            var perRoom = new Dictionary<string, int>();
            foreach (RoomData room in set.RoomsOfPhase(p2))
            {
                if (room.section != "C") continue;
                perRoom[room.id] = room.CountEnemies();
                count += room.CountEnemies();
                if (room.IsBossRoom) bosses++;
                for (int y = 0; y < room.height; y++)
                    for (int x = 0; x < room.width; x++)
                    {
                        char g = room.cells[x, y];
                        if (Glyphs.IsEnemy(g)) Assert.IsTrue(g == Glyphs.Knight || g == Glyphs.Bat, $"{room.id}: '{g}' is not a Section C enemy");
                        if (g == Glyphs.Bat) Assert.IsTrue(room.IsSolid(x, y + 1), $"{room.id}: bat at ({x},{y}) is not under a ceiling");
                    }
            }
            Assert.AreEqual(20, count);
            Assert.AreEqual(1, bosses);
            // Docs/05 §5 table
            Assert.AreEqual(0, perRoom["P2_C0"]); Assert.AreEqual(3, perRoom["P2_C1"]); Assert.AreEqual(4, perRoom["P2_C2"]);
            Assert.AreEqual(3, perRoom["P2_C3"]); Assert.AreEqual(4, perRoom["P2_C4"]); Assert.AreEqual(3, perRoom["P2_C5"]);
            Assert.AreEqual(3, perRoom["P2_C6"]);
        }
    }
}
