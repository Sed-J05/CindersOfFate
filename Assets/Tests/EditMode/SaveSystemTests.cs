using System.IO;
using NUnit.Framework;
using Shatranj.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace Shatranj.Tests
{
    public class SaveSystemTests
    {
        private string tempPath;

        [SetUp]
        public void SetUp()
        {
            tempPath = Path.Combine(Path.GetTempPath(), "shatranj_test_" + System.Guid.NewGuid().ToString("N") + ".json");
            SaveSystem.PathOverride = tempPath;
            SaveSystem.Delete();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.Delete();
            SaveSystem.PathOverride = null;
        }

        [Test]
        public void Load_WhenNoFile_ReturnsNull()
        {
            Assert.IsFalse(SaveSystem.Exists);
            Assert.IsNull(SaveSystem.Load());
        }

        [Test]
        public void SaveThenLoad_RoundTripsEveryField()
        {
            var data = SaveSystem.CreateNew();
            data.phase = 2;
            data.cycle = 3;
            data.lastBenchId = "P2_C6_B";
            data.lastBenchScene = GameConstants.ScenePhase2;
            data.lastBenchX = 12.5f;
            data.lastBenchY = -3.25f;
            data.flameT = 0.42f;
            data.maxHearts = 7;
            data.canDash = true;
            data.canFireball = true;
            data.MarkKilled("P1_A1_c_0");
            data.MarkKilled("P1_A1_c_1");
            data.MarkDefeated("kage_warden");
            data.SetFlag(GameConstants.FlagDoorOrb, true);

            Assert.IsTrue(SaveSystem.Save(data));
            Assert.IsTrue(SaveSystem.Exists);

            SaveData loaded = SaveSystem.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(2, loaded.phase);
            Assert.AreEqual(3, loaded.cycle);
            Assert.AreEqual("P2_C6_B", loaded.lastBenchId);
            Assert.AreEqual(GameConstants.ScenePhase2, loaded.lastBenchScene);
            Assert.AreEqual(12.5f, loaded.lastBenchX, 1e-6f);
            Assert.AreEqual(-3.25f, loaded.lastBenchY, 1e-6f);
            Assert.AreEqual(0.42f, loaded.flameT, 1e-6f);
            Assert.AreEqual(7, loaded.maxHearts);
            Assert.IsTrue(loaded.canDash);
            Assert.IsTrue(loaded.canFireball);
            Assert.AreEqual(2, loaded.killedEnemyIds.Count);
            Assert.IsTrue(loaded.HasKilled("P1_A1_c_1"));
            Assert.IsTrue(loaded.HasDefeated("kage_warden"));
            Assert.IsTrue(loaded.HasFlag(GameConstants.FlagDoorOrb));
            Assert.IsFalse(string.IsNullOrEmpty(loaded.savedAtUtc));
        }

        [Test]
        public void Save_OverwritesPreviousFile()
        {
            var first = SaveSystem.CreateNew();
            first.cycle = 1;
            SaveSystem.Save(first);

            var second = SaveSystem.CreateNew();
            second.cycle = 2;
            SaveSystem.Save(second);

            Assert.AreEqual(2, SaveSystem.Load().cycle);
            Assert.IsFalse(File.Exists(tempPath + ".tmp"));
        }

        [Test]
        public void Delete_RemovesFile()
        {
            SaveSystem.Save(SaveSystem.CreateNew());
            Assert.IsTrue(SaveSystem.Exists);
            SaveSystem.Delete();
            Assert.IsFalse(SaveSystem.Exists);
            Assert.IsNull(SaveSystem.Load());
        }

        [Test]
        public void Load_CorruptFile_ReturnsNull()
        {
            File.WriteAllText(tempPath, "{ this is not json");
            LogAssert.ignoreFailingMessages = true;
            SaveData loaded = SaveSystem.Load();
            LogAssert.ignoreFailingMessages = false;
            Assert.IsNull(loaded);
        }

        [Test]
        public void MarkKilled_IsIdempotent()
        {
            var data = SaveSystem.CreateNew();
            data.MarkKilled("x");
            data.MarkKilled("x");
            Assert.AreEqual(1, data.killedEnemyIds.Count);
        }

        [Test]
        public void SetFlag_FalseRemovesIt()
        {
            var data = SaveSystem.CreateNew();
            data.SetFlag("door_orb", true);
            Assert.IsTrue(data.HasFlag("door_orb"));
            data.SetFlag("door_orb", false);
            Assert.IsFalse(data.HasFlag("door_orb"));
        }

        [Test]
        public void ClearProgressForNewCycle_KeepsCycleResetsRest()
        {
            var data = SaveSystem.CreateNew();
            data.cycle = 4;
            data.phase = 2;
            data.flameT = 0.9f;
            data.canDash = true;
            data.maxHearts = 7;
            data.MarkKilled("a");
            data.MarkDefeated("zulmat");
            data.SetFlag("door_orb", true);

            data.ClearProgressForNewCycle();

            Assert.AreEqual(4, data.cycle);
            Assert.AreEqual(1, data.phase);
            Assert.AreEqual(0f, data.flameT);
            Assert.IsFalse(data.canDash);
            Assert.AreEqual(GameConstants.HeartsPhase1, data.maxHearts);
            Assert.AreEqual(0, data.killedEnemyIds.Count);
            Assert.AreEqual(0, data.defeatedBossIds.Count);
            Assert.AreEqual(0, data.flags.Count);
        }

        [Test]
        public void Abilities_RoundTripThroughSaveData()
        {
            var data = SaveSystem.CreateNew();
            data.FromAbilities(PlayerAbilities.ForPhase(2));
            PlayerAbilities a = data.ToAbilities();
            Assert.IsTrue(a.canDash);
            Assert.IsTrue(a.canFireball);
            Assert.AreEqual(GameConstants.HeartsPhase2, a.maxHearts);
        }
    }
}
