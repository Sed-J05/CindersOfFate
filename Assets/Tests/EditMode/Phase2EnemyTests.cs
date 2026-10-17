using System;
using NUnit.Framework;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.EditorTools;
using Shatranj.Enemies;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shatranj.Tests
{
    /// <summary>
    /// Prompt 12: Phase 2 enemies match Docs/04 §C numbers, telegraph every attack, scale with the flame, and the
    /// Phase 2 state is 7 hearts with dash and fireball. Also the flame-scale smoothing used by FlameScaler.
    /// </summary>
    public class Phase2EnemyTests
    {
        private const float Eps = 1e-4f;

        private static EnemyConfig Make(Action<EnemyConfig> fill)
        {
            var c = ScriptableObject.CreateInstance<EnemyConfig>();
            fill(c);
            return c;
        }

        private static readonly (string name, Action<EnemyConfig> fill, int hp)[] Phase2 =
        {
            ("Knight", ConfigDefaults.Knight, 5),
            ("Bat", ConfigDefaults.Bat, 3),
            ("Charger", ConfigDefaults.Charger, 7),
            ("Thrower", ConfigDefaults.Thrower, 4),
        };

        [Test]
        public void Phase2Enemies_HaveDocumentedHp_TelegraphAndScaleWithFlame()
        {
            foreach (var e in Phase2)
            {
                EnemyConfig cfg = Make(e.fill);
                Assert.AreEqual(e.hp, cfg.maxHp, $"{e.name} HP");
                Assert.AreEqual(e.hp, cfg.HitsToKill(1), $"{e.name} nail hits");
                Assert.Greater(cfg.telegraphTime, 0f, $"{e.name} telegraph");
                Assert.IsTrue(cfg.scalesWithFlame, $"{e.name} scales with the flame");
                Assert.AreEqual(3f, cfg.loseTrackTime, Eps, $"{e.name} lose-track timer");
                Object.DestroyImmediate(cfg);
            }
        }

        [Test]
        public void Knight_MatchesDoc()
        {
            EnemyConfig c = Make(ConfigDefaults.Knight);
            Assert.AreEqual(2f, c.patrolSpeed, Eps);
            Assert.AreEqual(9f, c.detectRange, Eps);
            Assert.AreEqual(2f, c.preferredDistance, Eps);
            Assert.AreEqual(0.4f, c.telegraphTime, Eps);
            Assert.AreEqual(2, c.comboHits);
            Assert.AreEqual(1.6f, c.attackReach, Eps);
            Assert.AreEqual(0.3f, c.comboGap, Eps);
            Assert.AreEqual(2.5f, c.retreatDistance, Eps);
            Assert.AreEqual(0.5f, c.retreatTime, Eps);
            Assert.AreEqual(0.2f, c.feintChance, Eps);
            Assert.AreEqual(1.5f, c.waitIfPlayerAboveHeight, Eps);
            Assert.IsTrue(c.requiresLineOfSight);
            Assert.IsTrue(c.turnsAtLedges, "ledge avoidance");
            Object.DestroyImmediate(c);
        }

        [Test]
        public void Bat_MatchesDoc()
        {
            EnemyConfig c = Make(ConfigDefaults.Bat);
            Assert.AreEqual(EnemyLocomotion.Flyer, c.locomotion);
            Assert.IsTrue(c.startsAsleep, "hangs from the ceiling");
            Assert.AreEqual(8f, c.detectRange, Eps);
            Assert.AreEqual(0.4f, c.wakeTime, Eps);
            Assert.AreEqual(3.5f, c.wanderSpeed, Eps);
            Assert.AreEqual(3f, c.circleRadiusMin, Eps);
            Assert.AreEqual(4f, c.circleRadiusMax, Eps);
            Assert.AreEqual(2.5f, c.attackInterval, Eps);
            Assert.AreEqual(0.35f, c.telegraphTime, Eps);
            Assert.AreEqual(8f, c.attackSpeed, Eps);
            Assert.AreEqual(0.6f, c.desyncOffset, Eps, "dives desync by +0.6 s per bat");
            Assert.IsTrue(c.scalePivotCenter, "flyers scale around the centre");
            Object.DestroyImmediate(c);
        }

        [Test]
        public void Charger_MatchesDoc()
        {
            EnemyConfig c = Make(ConfigDefaults.Charger);
            Assert.AreEqual(1.5f, c.patrolSpeed, Eps);
            Assert.AreEqual(10f, c.detectRange, Eps);
            Assert.AreEqual(0.6f, c.telegraphTime, Eps);
            Assert.AreEqual(13f, c.attackSpeed, Eps);
            Assert.AreEqual(10f, c.attackMaxDistance, Eps);
            Assert.AreEqual(1.2f, c.wallStunTime, Eps);
            Assert.IsTrue(c.knockbackImmuneDuringAttack);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void Thrower_MatchesDoc()
        {
            EnemyConfig c = Make(ConfigDefaults.Thrower);
            Assert.AreEqual(5f, c.retreatIfCloserThan, Eps);
            Assert.AreEqual(10f, c.advanceIfFartherThan, Eps);
            Assert.AreEqual(2.2f, c.attackInterval, Eps);
            Assert.AreEqual(0.4f, c.telegraphTime, Eps);
            Assert.AreEqual(10f, c.projectileSpeed, Eps);
            Assert.AreEqual(new Vector2(0.9f, 0.2f), c.projectileSize);
            Assert.AreEqual(0.3f, c.projectileLeadTime, Eps);
            Assert.IsTrue(c.projectileArcs);
            Assert.AreEqual(1, c.projectileDamage);
            Assert.AreEqual(0.5f, c.projectileScaleFactor, Eps);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void Phase2State_SevenHearts_DashAndFireball()
        {
            PlayerAbilities p1 = PlayerAbilities.ForPhase(GameConstants.Phase1);
            PlayerAbilities p2 = PlayerAbilities.ForPhase(GameConstants.Phase2);
            Assert.AreEqual(5, p1.maxHearts);
            Assert.IsFalse(p1.canDash);
            Assert.IsFalse(p1.canFireball);
            Assert.AreEqual(7, p2.maxHearts);
            Assert.IsTrue(p2.canDash);
            Assert.IsTrue(p2.canFireball);
        }

        [Test]
        public void Phase2Flame_HalfwayIs1Point1_PeakModeIsMaxMidway()
        {
            var f = ScriptableObject.CreateInstance<FlameConfig>();
            ConfigDefaults.FlamePhase2(f);
            Assert.AreEqual(FlameScaleMode.Monotonic, f.scaleMode);
            Assert.AreEqual(1.1f, f.EvaluateScale(0.5f), Eps, "lerp(1, 1.4, 0.5^2)");
            Assert.AreEqual(1f, f.EvaluateScale(0f), Eps);
            f.scaleMode = FlameScaleMode.Peak;
            Assert.AreEqual(1.4f, f.EvaluateScale(0.5f), Eps);
            Assert.AreEqual(f.EvaluateScale(0.25f), f.EvaluateScale(0.75f), 1e-3f, "peak mode is symmetric");
            Object.DestroyImmediate(f);
        }

        [Test]
        public void FlameScalerSmooth_ApproachesTargetWithoutOvershoot_FrameRateIndependent()
        {
            // 1 s at speed 2 covers 1 - e^-2 (about 86 %) of the gap, whatever the frame rate.
            float a = 1f, b = 1f;
            for (int i = 0; i < 60; i++) a = FlameScaler.Smooth(a, 1.4f, 2f, 1f / 60f);
            for (int i = 0; i < 30; i++) b = FlameScaler.Smooth(b, 1.4f, 2f, 1f / 30f);
            float expected = 1f + 0.4f * (1f - Mathf.Exp(-2f));
            Assert.AreEqual(expected, a, 1e-3f);
            Assert.AreEqual(expected, b, 1e-3f);
            Assert.LessOrEqual(a, 1.4f);

            Assert.AreEqual(1.4f, FlameScaler.Smooth(1.3995f, 1.4f, 2f, 1f / 60f), 0f, "snaps when within 0.001");
            Assert.AreEqual(1.2f, FlameScaler.Smooth(1.2f, 1.4f, 2f, 0f), Eps, "no time, no change");
            Assert.Less(FlameScaler.Smooth(1.4f, 1f, 2f, 0.1f), 1.4f, "shrinks too (peak mode, death restore)");
        }
    }
}
