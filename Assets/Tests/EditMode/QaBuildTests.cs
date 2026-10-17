using System.IO;
using NUnit.Framework;
using Shatranj.Core;
using Shatranj.EditorTools;
using UnityEngine;

namespace Shatranj.Tests
{
    /// <summary>Prompt 18: time-scale safety net and the Windows build scene list.</summary>
    public class QaBuildTests
    {
        private float savedScale;

        [SetUp] public void SetUp() { savedScale = Time.timeScale; TimeScaleGuard.Restore(); }
        [TearDown] public void TearDown() { TimeScaleGuard.Restore(); Time.timeScale = savedScale; }

        [Test]
        public void SlowMo_StartsAndEnds_BackToNormalSpeed()
        {
            var owner = new GameObject("SlowMoOwner");
            TimeScaleGuard.BeginSlowMo(owner, 0.25f);
            Assert.AreEqual(0.25f, Time.timeScale, 1e-5f);
            Assert.IsTrue(TimeScaleGuard.SlowMoActive);
            Assert.AreEqual(0.25f, TimeScaleGuard.ResumeScale, 1e-5f, "resume from pause keeps the slow-mo");
            TimeScaleGuard.EndSlowMo(owner);
            Assert.AreEqual(1f, Time.timeScale, 1e-5f);
            Assert.IsFalse(TimeScaleGuard.SlowMoActive);
            Assert.AreEqual(1f, TimeScaleGuard.ResumeScale, 1e-5f);
            Object.DestroyImmediate(owner);
        }

        [Test]
        public void DestroyedOwner_NeverHoldsTheClock()
        {
            var owner = new GameObject("Boss");
            TimeScaleGuard.BeginSlowMo(owner, 0.25f);
            Object.DestroyImmediate(owner);   // boss despawned mid slow-mo (player died)
            Assert.IsFalse(TimeScaleGuard.SlowMoActive, "a destroyed owner is ignored, so the watchdog can reset the scale");
            TimeScaleGuard.Restore();
            Assert.AreEqual(1f, Time.timeScale, 1e-5f);
        }

        [Test]
        public void TwoOwners_SpeedReturnsOnlyWhenBothEnd()
        {
            var a = new GameObject("A");
            var b = new GameObject("B");
            TimeScaleGuard.BeginSlowMo(a, 0.25f);
            TimeScaleGuard.BeginSlowMo(b, 0.25f);
            TimeScaleGuard.EndSlowMo(a);
            Assert.AreEqual(0.25f, Time.timeScale, 1e-5f);
            TimeScaleGuard.EndSlowMo(b);
            Assert.AreEqual(1f, Time.timeScale, 1e-5f);
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void WindowsBuild_ShipsTheGameScenesInFlowOrder()
        {
            CollectionAssert.AreEqual(new[]
            {
                "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Cutscene.unity",
                "Assets/Scenes/Phase1_Cave.unity", "Assets/Scenes/Phase2_Castle.unity",
            }, BuildPlayer.GameScenes);
            foreach (string scene in BuildPlayer.GameScenes) Assert.IsTrue(File.Exists(scene), scene + " (run Build All Scenes)");
            CollectionAssert.DoesNotContain(BuildPlayer.GameScenes, "Assets/Scenes/Sandbox.unity", "the release build doesn't ship the Sandbox");
            Assert.AreEqual("Cinders of Fate", BuildPlayer.ProductName);
            Assert.AreEqual(1920, BuildPlayer.DefaultWidth);
            Assert.AreEqual(1080, BuildPlayer.DefaultHeight);
        }
    }
}
