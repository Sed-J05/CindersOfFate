using NUnit.Framework;
using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Tests
{
    public class HealthTests
    {
        private GameObject go;
        private Health health;
        private float now;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("HealthTest");
            health = go.AddComponent<Health>();
            now = 10f;
            health.SetClock(() => now);
            health.Configure(3, 1.0f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        private static DamageInfo Hit(int amount = 1) => DamageInfo.Simple(amount, DamageType.Melee);

        [Test]
        public void Configure_StartsFullAndAlive()
        {
            Assert.AreEqual(3, health.MaxHp);
            Assert.AreEqual(3, health.CurrentHp);
            Assert.IsTrue(health.IsAlive);
            Assert.IsFalse(health.IsInvulnerable);
        }

        [Test]
        public void TakeDamage_ReducesHpAndFiresEvents()
        {
            int damagedCalls = 0, changedCalls = 0; float flash = -1f;
            health.OnDamaged += _ => damagedCalls++;
            health.OnHealthChanged += (c, m) => changedCalls++;
            health.OnHitFlash += d => flash = d;

            Assert.IsTrue(health.TakeDamage(Hit()));
            Assert.AreEqual(2, health.CurrentHp);
            Assert.AreEqual(1, damagedCalls);
            Assert.AreEqual(1, changedCalls);
            Assert.AreEqual(health.HitFlashDuration, flash);
        }

        [Test]
        public void SecondHitInsideWindow_IsRejected()
        {
            Assert.IsTrue(health.TakeDamage(Hit()));
            Assert.IsTrue(health.IsInvulnerable);

            now += 0.5f;
            Assert.IsFalse(health.TakeDamage(Hit()));
            Assert.AreEqual(2, health.CurrentHp);
        }

        [Test]
        public void HitAfterWindow_IsAccepted()
        {
            health.TakeDamage(Hit());
            now += 1.0f;
            Assert.IsFalse(health.IsInvulnerable);
            Assert.IsTrue(health.TakeDamage(Hit()));
            Assert.AreEqual(1, health.CurrentHp);
        }

        [Test]
        public void InvulnerabilityRemaining_CountsDown()
        {
            health.TakeDamage(Hit());
            Assert.AreEqual(1.0f, health.InvulnerabilityRemaining, 1e-4f);
            now += 0.25f;
            Assert.AreEqual(0.75f, health.InvulnerabilityRemaining, 1e-4f);
            now += 2f;
            Assert.AreEqual(0f, health.InvulnerabilityRemaining, 1e-4f);
        }

        [Test]
        public void IgnoreInvulnerability_BypassesWindow()
        {
            health.TakeDamage(Hit());
            var piercing = new DamageInfo(1, Vector2.zero, 0f, null, DamageType.Debug, ignoreInvulnerability: true);
            Assert.IsTrue(health.TakeDamage(piercing));
            Assert.AreEqual(1, health.CurrentHp);
        }

        [Test]
        public void ExternalInvulnerableFlag_BlocksDamage()
        {
            health.Invulnerable = true;
            Assert.IsFalse(health.TakeDamage(Hit()));
            Assert.AreEqual(3, health.CurrentHp);
            health.Invulnerable = false;
            Assert.IsTrue(health.TakeDamage(Hit()));
        }

        [Test]
        public void ZeroDamage_IsRejectedWithoutEvents()
        {
            int calls = 0;
            health.OnDamaged += _ => calls++;
            Assert.IsFalse(health.TakeDamage(Hit(0)));
            Assert.AreEqual(0, calls);
            Assert.AreEqual(3, health.CurrentHp);
        }

        [Test]
        public void ReachingZero_FiresDiedOnceAndRejectsFurtherHits()
        {
            int died = 0;
            health.OnDied += _ => died++;

            Assert.IsTrue(health.TakeDamage(Hit(3)));
            Assert.AreEqual(0, health.CurrentHp);
            Assert.IsFalse(health.IsAlive);
            Assert.AreEqual(1, died);

            now += 5f;
            Assert.IsFalse(health.TakeDamage(Hit()));
            Assert.AreEqual(1, died);
        }

        [Test]
        public void Overkill_ClampsToZero()
        {
            health.TakeDamage(Hit(99));
            Assert.AreEqual(0, health.CurrentHp);
        }

        [Test]
        public void Heal_ClampsToMaxAndReportsRestored()
        {
            health.TakeDamage(Hit(2));
            int healedEvent = 0;
            health.OnHealed += n => healedEvent = n;

            Assert.AreEqual(2, health.Heal(5));
            Assert.AreEqual(3, health.CurrentHp);
            Assert.AreEqual(2, healedEvent);
            Assert.AreEqual(0, health.Heal(1));
        }

        [Test]
        public void Heal_WhenDead_DoesNothing()
        {
            health.TakeDamage(Hit(3));
            Assert.AreEqual(0, health.Heal(1));
            Assert.IsFalse(health.IsAlive);
        }

        [Test]
        public void GrantInvulnerability_ExtendsButNeverShortens()
        {
            health.TakeDamage(Hit()); // window until now + 1.0
            health.GrantInvulnerability(0.2f);
            Assert.AreEqual(1.0f, health.InvulnerabilityRemaining, 1e-4f);
            health.GrantInvulnerability(2f);
            Assert.AreEqual(2.0f, health.InvulnerabilityRemaining, 1e-4f);
        }

        [Test]
        public void SetMax_RefillOrClamp()
        {
            health.TakeDamage(Hit(2)); // 1/3
            health.SetMax(7, refill: false);
            Assert.AreEqual(1, health.CurrentHp);
            Assert.AreEqual(7, health.MaxHp);
            health.SetMax(5, refill: true);
            Assert.AreEqual(5, health.CurrentHp);
        }

        [Test]
        public void ResetToFull_RevivesAndClearsWindow()
        {
            health.TakeDamage(Hit(3));
            health.ResetToFull();
            Assert.IsTrue(health.IsAlive);
            Assert.AreEqual(3, health.CurrentHp);
            Assert.IsFalse(health.IsInvulnerable);
        }

        [Test]
        public void Kill_BypassesInvulnerability()
        {
            health.TakeDamage(Hit());
            health.Kill();
            Assert.IsFalse(health.IsAlive);
        }

        [Test]
        public void NoInvulnerabilityDuration_AllowsBackToBackHits()
        {
            health.Configure(5, 0f);
            Assert.IsTrue(health.TakeDamage(Hit()));
            Assert.IsTrue(health.TakeDamage(Hit()));
            Assert.AreEqual(3, health.CurrentHp);
        }
    }
}
