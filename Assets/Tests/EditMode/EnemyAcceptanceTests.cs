using System;
using NUnit.Framework;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shatranj.Tests
{
    /// <summary>
    /// Prompt 7 acceptance: every attack has a visible telegraph, and each Phase 1 enemy dies in exactly the
    /// number of nail hits Docs/04 §A states. Configs are built from the same defaults the generator uses.
    /// </summary>
    public class EnemyAcceptanceTests
    {
        private static EnemyConfig Make(Action<EnemyConfig> fill)
        {
            var c = ScriptableObject.CreateInstance<EnemyConfig>();
            fill(c);
            return c;
        }

        private static readonly (string name, Action<EnemyConfig> fill, int hits)[] Phase1 =
        {
            ("Crawler", ConfigDefaults.Crawler, 2),
            ("Lunger", ConfigDefaults.Lunger, 3),
            ("ShadowKite", ConfigDefaults.ShadowKite, 2),
        };

        [Test]
        public void Phase1Enemies_DieInDocumentedNailHits()
        {
            foreach (var e in Phase1)
            {
                EnemyConfig cfg = Make(e.fill);
                Assert.AreEqual(e.hits, cfg.maxHp, $"{e.name} HP");
                Assert.AreEqual(e.hits, cfg.HitsToKill(1), $"{e.name} hits-to-kill");

                var go = new GameObject(e.name);
                var health = go.AddComponent<Health>();
                health.Configure(cfg.maxHp, 0f, cfg.hitFlashDuration);
                for (int i = 1; i < e.hits; i++)
                {
                    Assert.IsTrue(health.TakeDamage(DamageInfo.Simple(1, DamageType.Melee)));
                    Assert.IsTrue(health.IsAlive, $"{e.name} should survive hit {i}");
                }
                Assert.IsTrue(health.TakeDamage(DamageInfo.Simple(1, DamageType.Melee)));
                Assert.IsFalse(health.IsAlive, $"{e.name} should die on hit {e.hits}");
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(cfg);
            }
        }

        [Test]
        public void EveryAttack_HasATelegraph()
        {
            foreach (var e in Phase1)
            {
                EnemyConfig cfg = Make(e.fill);
                if (cfg.HasAttack)
                {
                    Assert.Greater(cfg.telegraphTime, 0f, $"{e.name} telegraph time");
                    Assert.Greater(cfg.telegraphFlashTime, 0f, $"{e.name} telegraph flash");
                    Assert.Greater(cfg.telegraphFlashColor.a, 0f, $"{e.name} telegraph flash colour visible");
                }
                Object.DestroyImmediate(cfg);
            }
        }

        [Test]
        public void Lunger_MatchesDocs04()
        {
            EnemyConfig c = Make(ConfigDefaults.Lunger);
            Assert.AreEqual(1.6f, c.patrolSpeed); // tuned D-113 (doc 2)
            Assert.AreEqual(7f, c.detectRange);
            Assert.IsTrue(c.requiresLineOfSight);
            Assert.AreEqual(0.5f, c.telegraphTime);
            Assert.IsTrue(c.showExclamation, "Lunger telegraph shows '!'");
            Assert.AreEqual(11.5f, c.attackSpeed); // tuned D-113 (doc 14)
            Assert.AreEqual(0.35f, c.attackActiveTime);
            Assert.AreEqual(0.8f, c.attackRecoveryTime);
            Assert.AreEqual(1f, c.attackCooldown);
            Assert.AreEqual(4.0f, c.attackSpeed * c.attackActiveTime, 0.03f, "≈ 4 u lunge");
            Object.DestroyImmediate(c);
        }

        [Test]
        public void Crawler_MatchesDocs04()
        {
            EnemyConfig c = Make(ConfigDefaults.Crawler);
            Assert.AreEqual(2.0f, c.patrolSpeed); // tuned D-113 (doc 2.5)
            Assert.AreEqual(2.8f, c.chaseSpeed); // tuned D-113 (doc 3.5)
            Assert.AreEqual(6f, c.detectRange);
            Assert.AreEqual(0.4f, c.verticalTolerance); // D-114: same level only (doc ±1.5 u)
            Assert.IsTrue(c.turnsAtLedges);
            Assert.IsFalse(c.HasAttack, "Crawler deals contact damage only");
            Assert.AreEqual(3f, c.knockbackSpeed);
            Assert.AreEqual(0.15f, c.knockbackDuration);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void ShadowKite_MatchesDocs04()
        {
            EnemyConfig c = Make(ConfigDefaults.ShadowKite);
            Assert.AreEqual(EnemyLocomotion.Flyer, c.locomotion);
            Assert.AreEqual(0.4f, c.hoverAmplitude);
            Assert.AreEqual(1.5f, c.hoverFrequency);
            Assert.AreEqual(9f, c.detectRange);
            Assert.AreEqual(2.5f, c.chaseSpeed);
            Assert.AreEqual(3f, c.hoverHeightAbovePlayer);
            Assert.AreEqual(3f, c.attackInterval);
            Assert.AreEqual(0.5f, c.telegraphTime);
            Assert.AreEqual(9f, c.attackSpeed);
            Assert.AreEqual(6f, c.attackMaxDistance);
            Assert.AreEqual(3f, c.returnSpeed);
            Assert.AreEqual(4f, c.knockbackSpeed);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void Fireball_KillsPhase1EnemiesInOneHit()
        {
            var p = ScriptableObject.CreateInstance<PlayerConfig>();
            foreach (var e in Phase1)
            {
                EnemyConfig cfg = Make(e.fill);
                Assert.AreEqual(1, cfg.HitsToKill(p.fireballDamage), e.name);
                Object.DestroyImmediate(cfg);
            }
            Object.DestroyImmediate(p);
        }
    }
}
