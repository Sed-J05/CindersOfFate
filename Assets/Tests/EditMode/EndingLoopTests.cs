using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Shatranj.Core;
using Shatranj.Cutscenes;

namespace Shatranj.Tests
{
    /// <summary>Prompt 16: Cutscene 3 script, the loop reset (no stale progress) and Continue's pending cutscene.</summary>
    public class EndingLoopTests
    {
        private const string Cs3Path = "Assets/Data/Cutscenes/cs3_ending.json";
        private string tempPath;

        [SetUp]
        public void SetUp()
        {
            tempPath = Path.Combine(Path.GetTempPath(), "shatranj_loop_" + System.Guid.NewGuid().ToString("N") + ".json");
            SaveSystem.PathOverride = tempPath;
            SaveSystem.Delete();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.Delete();
            SaveSystem.PathOverride = null;
        }

        private static List<Beat> Flatten(IEnumerable<Beat> beats)
        {
            var all = new List<Beat>();
            foreach (Beat b in beats)
            {
                all.Add(b);
                if (b.Type == "parallel" || b.Type == "sequence") all.AddRange(Flatten(b.Beats("beats")));
            }
            return all;
        }

        private static CutsceneScript Cs3() => CutsceneScript.Parse(File.ReadAllText(Cs3Path), "cs3_ending");

        [Test]
        public void Cs3_FollowsDoc08Section4()
        {
            CutsceneScript s = Cs3();
            Assert.AreEqual("cs3_ending", s.id);
            List<Beat> all = Flatten(s.beats);

            Assert.IsTrue(all.Exists(b => b.Type == "theme" && b.Str("theme") == "Ending_Purple"), "flame turns purple");
            Assert.IsTrue(all.Exists(b => b.Type == "theme" && b.Str("theme") == "Phase1_Beige"), "purple → orange on the rewind");
            Assert.IsTrue(all.Exists(b => b.Type == "flame" && b.Has("t") && b.Float("t", 1f) == 0f), "flame back to t = 0");
            foreach (string img in new[] { "CS3_Merge_01", "CS3_Merge_02", "CS3_Merge_03", "CS3_GodsStab", "CS3_Split" })
                Assert.IsTrue(all.Exists(b => b.Type == "image" && b.Str("sprite") == img), img);
            Assert.IsTrue(all.Exists(b => b.Type == "spawn" && b.Str("id") == "whole"), "merge into one figure");
            Assert.IsTrue(all.Exists(b => b.Type == "zoom" && b.Float("value", 1f) > 1f), "camera pans back");
            Assert.IsTrue(all.Exists(b => b.Type == "rewind"), "paper rewinds");
            Assert.IsTrue(all.Exists(b => b.Type == "card" && b.Str("text").Contains("Thanks for playing")), "credits card");
            Assert.IsTrue(all.Exists(b => b.Type == "card" && b.Str("text").Contains("{cycle}")), "Cycle N caption");

            Assert.AreEqual(1, all.FindAll(b => b.Type == "setflag" && b.Str("flag") == "cycle").Count, "cycle++ exactly once");
            Assert.IsTrue(all.Exists(b => b.Type == "setflag" && b.Str("flag") == "titleCard" && b.Str("value").Contains("{cycle}")), "Cycle N card on Phase 1");

            Beat last = s.beats[s.beats.Count - 1];
            Assert.AreEqual("loadscene", last.Type);
            Assert.AreEqual("cs1_intro", last.Str("cutscene"), "loop goes through Cutscene 1 (skippable from cycle 2)");
            Assert.AreEqual("Phase1_Cave", last.Str("next"));

            // The cycle flag comes after every story text, so quitting mid-ending replays the story, not the reset.
            int cycleIndex = s.beats.FindIndex(b => b.Type == "setflag" && b.Str("flag") == "cycle");
            int lastText = s.beats.FindLastIndex(b => b.Type == "text");
            Assert.Greater(cycleIndex, lastText);
        }

        [Test]
        public void NewCycle_LeavesNoStaleProgress_KeepsCycle()
        {
            var save = new SaveData { phase = GameConstants.Phase2, cycle = 2, flameT = 0.83f, lastBenchId = "P2_D8_B", lastBenchScene = "Phase2_Castle", lastBenchX = 3f, lastBenchY = 4f };
            save.FromAbilities(PlayerAbilities.ForPhase(GameConstants.Phase2));
            save.MarkKilled("P1_A1_c_0"); save.MarkKilled("P2_D3_h_0");
            save.MarkDefeated("kage_warden"); save.MarkDefeated("kage_caller"); save.MarkDefeated("iron_colossus");
            save.SetFlag("door_orb", true);
            save.pendingCutscene = GameConstants.CutsceneEnding; save.pendingNextScene = GameConstants.ScenePhase1;
            // BossArena marks the final boss after the final-boss event; the reset in Cutscene 3 still clears it.
            save.MarkDefeated("zulmat");

            save.ClearProgressForNewCycle();

            Assert.AreEqual(2, save.cycle, "the cycle number is kept (GameManager increments it)");
            Assert.AreEqual(GameConstants.Phase1, save.phase);
            Assert.AreEqual(0f, save.flameT);
            Assert.AreEqual("", save.lastBenchId); Assert.AreEqual("", save.lastBenchScene);
            Assert.IsEmpty(save.killedEnemyIds);
            Assert.IsEmpty(save.defeatedBossIds);
            Assert.IsFalse(save.HasFlag("door_orb"));
            Assert.IsEmpty(save.flags);
            Assert.IsFalse(save.HasPendingCutscene, "a reset never resumes the ending again (no double cycle++)");
            Assert.AreEqual(GameConstants.HeartsPhase1, save.maxHearts);
            Assert.IsFalse(save.canDash); Assert.IsFalse(save.canFireball);
        }

        [Test]
        public void PendingCutscene_SurvivesSaveAndLoad_AndOldSavesLoadClean()
        {
            var save = new SaveData { phase = GameConstants.Phase2, cycle = 3, pendingCutscene = GameConstants.CutsceneEnding, pendingNextScene = GameConstants.ScenePhase1 };
            Assert.IsTrue(SaveSystem.Save(save));
            SaveData loaded = SaveSystem.Load();
            Assert.AreEqual(GameConstants.CutsceneEnding, loaded.pendingCutscene);
            Assert.AreEqual(GameConstants.ScenePhase1, loaded.pendingNextScene);
            Assert.IsTrue(loaded.HasPendingCutscene);

            // A save written before Prompt 16 has no pending fields.
            File.WriteAllText(tempPath, "{\"version\":1,\"phase\":2,\"cycle\":1,\"killedEnemyIds\":[],\"defeatedBossIds\":[],\"flags\":[\"door_orb\"]}");
            SaveData old = SaveSystem.Load();
            Assert.IsNotNull(old);
            Assert.AreEqual("", old.pendingCutscene);
            Assert.IsFalse(old.HasPendingCutscene);
            Assert.AreEqual("", old.lastBenchScene);
        }

        [Test]
        public void CycleToken_Expands()
        {
            // No GameManager in edit mode: cycle 1.
            Assert.AreEqual("Cycle 1", CutsceneRunner.Expand("Cycle {cycle}"));
            Assert.AreEqual("plain", CutsceneRunner.Expand("plain"));
            Assert.IsNull(CutsceneRunner.Expand(null));
        }
    }
}
