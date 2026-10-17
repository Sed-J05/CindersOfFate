using NUnit.Framework;
using Shatranj.Enemies;
using UnityEngine;

namespace Shatranj.Tests
{
    /// <summary>Prompt 12: the reusable "better AI" toolkit (Docs/04 §C) — spacing, waiting, awareness, aiming.</summary>
    public class SmartAITests
    {
        private const float Eps = 1e-4f;

        // ---------------- Spacing ----------------

        [Test]
        public void BandMove_Thrower_RetreatsBelow5_AdvancesAbove10_HoldsBetween()
        {
            Assert.AreEqual(-1, SpacingLogic.BandMove(3f, 5f, 10f));
            Assert.AreEqual(-1, SpacingLogic.BandMove(4.99f, 5f, 10f));
            Assert.AreEqual(0, SpacingLogic.BandMove(5f, 5f, 10f));
            Assert.AreEqual(0, SpacingLogic.BandMove(7.5f, 5f, 10f), "6 to 9 u is the comfort band");
            Assert.AreEqual(0, SpacingLogic.BandMove(10f, 5f, 10f));
            Assert.AreEqual(1, SpacingLogic.BandMove(10.01f, 5f, 10f));
            Assert.AreEqual(1, SpacingLogic.BandMove(14f, 5f, 10f));
        }

        [Test]
        public void ApproachToRange_Knight_ClosesTo2u_BacksOffWhenTooClose()
        {
            Assert.AreEqual(1, SpacingLogic.ApproachToRange(6f, 2f, 0.3f));
            Assert.AreEqual(0, SpacingLogic.ApproachToRange(2.25f, 2f, 0.3f));
            Assert.AreEqual(0, SpacingLogic.ApproachToRange(1.75f, 2f, 0.3f));
            Assert.AreEqual(-1, SpacingLogic.ApproachToRange(1f, 2f, 0.3f));
        }

        [Test]
        public void ApproachToRange_ScaledPreferredDistance_GrowsWithTheEnemy()
        {
            // Knight reach grows with the flame, so its preferred distance does too (2.0 u × 1.4 = 2.8 u).
            Assert.AreEqual(1, SpacingLogic.ApproachToRange(2.5f, 2f, 0.3f));
            Assert.AreEqual(0, SpacingLogic.ApproachToRange(2.8f, 2f * 1.4f, 0.3f));
        }

        [Test]
        public void ShouldWait_OnlyWhenPlayerAirborneAboveThreshold()
        {
            Assert.IsTrue(SpacingLogic.ShouldWaitForAirbornePlayer(false, 2f, 1.5f));
            Assert.IsFalse(SpacingLogic.ShouldWaitForAirbornePlayer(false, 1f, 1.5f), "low hop: swing");
            Assert.IsFalse(SpacingLogic.ShouldWaitForAirbornePlayer(true, 2f, 1.5f), "standing on a ledge: not airborne");
            Assert.IsFalse(SpacingLogic.ShouldWaitForAirbornePlayer(false, 5f, 0f), "threshold 0 disables waiting");
        }

        // ---------------- Awareness / lose-track ----------------

        [Test]
        public void Awareness_LosesTrackAfter3sWithoutSighting()
        {
            var a = new Awareness();
            Assert.IsFalse(a.Tick(false, Vector2.zero, 0.1f, 3f), "starts unaware");
            Assert.IsTrue(a.Tick(true, new Vector2(4f, 1f), 0.1f, 3f));
            Assert.AreEqual(new Vector2(4f, 1f), a.LastKnownPosition);

            // 0.25 s steps are exact in floating point (0.1 s steps would sum to 2.9999998 after 30 ticks).
            for (int i = 0; i < 11; i++) Assert.IsTrue(a.Tick(false, Vector2.zero, 0.25f, 3f), $"still aware at {(i + 1) * 0.25f:0.00}s");
            Assert.IsFalse(a.Tick(false, Vector2.zero, 0.25f, 3f), "forgot at 3.0 s");
            Assert.AreEqual(new Vector2(4f, 1f), a.LastKnownPosition, "last known position is kept");
        }

        [Test]
        public void Awareness_SightingResetsTimer()
        {
            var a = new Awareness();
            a.Tick(true, Vector2.zero, 0f, 3f);
            for (int i = 0; i < 25; i++) a.Tick(false, Vector2.zero, 0.1f, 3f);
            a.Tick(true, Vector2.one, 0.1f, 3f);
            Assert.AreEqual(0f, a.TimeSinceSeen, Eps);
            for (int i = 0; i < 25; i++) Assert.IsTrue(a.Tick(false, Vector2.zero, 0.1f, 3f));
        }

        [Test]
        public void Awareness_ZeroLoseTrack_NeverForgets_ForgetResets()
        {
            var a = new Awareness();
            a.Tick(true, Vector2.zero, 0f, 0f);
            for (int i = 0; i < 1000; i++) a.Tick(false, Vector2.zero, 0.1f, 0f);
            Assert.IsTrue(a.Aware);
            a.Forget();
            Assert.IsFalse(a.Aware);
            Assert.IsTrue(float.IsPositiveInfinity(a.TimeSinceSeen));
        }

        // ---------------- Aiming (Thrower) ----------------

        [Test]
        public void LeadTarget_AimsAtPositionPlusVelocityTimesLead()
        {
            Vector2 aim = SpacingLogic.LeadTarget(new Vector2(10f, 2f), new Vector2(8f, 0f), 0.3f);
            Assert.AreEqual(12.4f, aim.x, Eps);
            Assert.AreEqual(2f, aim.y, Eps);
            Assert.AreEqual(new Vector2(3f, 1f), SpacingLogic.LeadTarget(new Vector2(3f, 1f), Vector2.zero, 0.3f));
        }

        [Test]
        public void ArcVelocity_LandsOnTarget_UnderGravity()
        {
            Vector2 from = new Vector2(0f, 1.3f), to = new Vector2(8f, 1f);
            const float speed = 10f, g = 6f;
            Vector2 v = SpacingLogic.ArcVelocity(from, to, speed, g);
            Assert.AreEqual(speed, v.x, Eps, "horizontal speed equals the knife speed");
            Assert.Greater(v.y, 0f, "slight upward arc");

            // Simulate the projectile (gravity pulls down) and check it reaches the target point.
            float t = Mathf.Abs(to.x - from.x) / speed;
            Vector2 p = from + v * t + 0.5f * new Vector2(0f, -g) * t * t;
            Assert.AreEqual(to.x, p.x, 1e-3f);
            Assert.AreEqual(to.y, p.y, 1e-3f);
        }

        [Test]
        public void ArcVelocity_LeftAndNoGravity_IsStraight()
        {
            Vector2 v = SpacingLogic.ArcVelocity(new Vector2(5f, 0f), new Vector2(-5f, 0f), 10f, 0f);
            Assert.AreEqual(-10f, v.x, Eps);
            Assert.AreEqual(0f, v.y, Eps);
        }

        [Test]
        public void ArcVelocity_PointBlank_UsesMinimumFlightTime()
        {
            Vector2 v = SpacingLogic.ArcVelocity(Vector2.zero, new Vector2(0f, 1f), 10f, 6f);
            Assert.IsFalse(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsInfinity(v.y));
            Assert.AreEqual(1f / 0.15f + 0.5f * 6f * 0.15f, v.y, 1e-3f);
        }

        [Test]
        public void ProjectileScale_IsHalfTheEnemyGrowth()
        {
            Assert.AreEqual(1f, SpacingLogic.ProjectileScale(1f, 0.5f), Eps);
            Assert.AreEqual(1.2f, SpacingLogic.ProjectileScale(1.4f, 0.5f), Eps, "1 + 0.5 x (1.4 - 1)");
            Assert.AreEqual(1.4f, SpacingLogic.ProjectileScale(1.4f, 1f), Eps);
        }
    }
}
