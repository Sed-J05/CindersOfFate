using NUnit.Framework;
using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Tests
{
    public class DamageInfoTests
    {
        [Test]
        public void Simple_HasNoKnockback()
        {
            var d = DamageInfo.Simple(2, DamageType.Contact);
            Assert.AreEqual(2, d.amount);
            Assert.IsFalse(d.HasKnockback);
            Assert.AreEqual(Vector2.zero, d.KnockbackVelocity);
            Assert.AreEqual(DamageType.Contact, d.type);
            Assert.IsFalse(d.ignoreInvulnerability);
        }

        [Test]
        public void Directional_NormalisesDirection()
        {
            var d = DamageInfo.Directional(1, new Vector2(0f, 0f), new Vector2(3f, 4f), 6f, DamageType.Melee);
            Assert.AreEqual(0.6f, d.knockbackDir.x, 1e-5f);
            Assert.AreEqual(0.8f, d.knockbackDir.y, 1e-5f);
            Assert.AreEqual(1f, d.knockbackDir.magnitude, 1e-5f);
            Assert.IsTrue(d.HasKnockback);
        }

        [Test]
        public void Directional_KnockbackVelocityHasForceMagnitude()
        {
            var d = DamageInfo.Directional(1, Vector2.zero, new Vector2(-2f, 0f), 6f, DamageType.Melee);
            Assert.AreEqual(6f, d.KnockbackVelocity.magnitude, 1e-5f);
            Assert.AreEqual(-6f, d.KnockbackVelocity.x, 1e-5f);
            Assert.AreEqual(0f, d.KnockbackVelocity.y, 1e-5f);
        }

        [Test]
        public void Directional_CoincidentPoints_FallBackToPlusX()
        {
            var d = DamageInfo.Directional(1, new Vector2(5f, 5f), new Vector2(5f, 5f), 3f, DamageType.Melee);
            Assert.AreEqual(Vector2.right, d.knockbackDir);
            Assert.IsFalse(float.IsNaN(d.KnockbackVelocity.x));
            Assert.AreEqual(3f, d.KnockbackVelocity.x, 1e-5f);
        }

        [Test]
        public void Horizontal_PushesAwayFromAttacker()
        {
            var right = DamageInfo.Horizontal(1, fromX: 0f, toX: 1f, force: 6f, DamageType.Contact);
            var left = DamageInfo.Horizontal(1, fromX: 2f, toX: 1f, force: 6f, DamageType.Contact);
            Assert.AreEqual(Vector2.right, right.knockbackDir);
            Assert.AreEqual(Vector2.left, left.knockbackDir);
            Assert.AreEqual(-6f, left.KnockbackVelocity.x, 1e-5f);
        }

        [Test]
        public void ZeroForce_MeansNoKnockbackEvenWithDirection()
        {
            var d = new DamageInfo(1, Vector2.up, 0f, null, DamageType.Melee);
            Assert.IsFalse(d.HasKnockback);
            Assert.AreEqual(Vector2.zero, d.KnockbackVelocity);
        }

        [Test]
        public void WithAmount_CopiesEverythingElse()
        {
            var d = DamageInfo.Directional(1, Vector2.zero, Vector2.up, 5f, DamageType.Projectile);
            var e = d.WithAmount(3);
            Assert.AreEqual(3, e.amount);
            Assert.AreEqual(1, d.amount);
            Assert.AreEqual(d.knockbackDir, e.knockbackDir);
            Assert.AreEqual(d.knockbackForce, e.knockbackForce);
            Assert.AreEqual(d.type, e.type);
        }

        [Test]
        public void WithKnockback_ReplacesDirectionAndForce()
        {
            var d = DamageInfo.Simple(1, DamageType.Melee).WithKnockback(Vector2.left, 2f);
            Assert.IsTrue(d.HasKnockback);
            Assert.AreEqual(-2f, d.KnockbackVelocity.x, 1e-5f);
        }

        [Test]
        public void GrantsSoul_OnlyForMelee()
        {
            Assert.IsTrue(DamageInfo.Simple(1, DamageType.Melee).GrantsSoul);
            Assert.IsFalse(DamageInfo.Simple(1, DamageType.Spell).GrantsSoul);
            Assert.IsFalse(DamageInfo.Simple(1, DamageType.Hazard).GrantsSoul);
            Assert.IsFalse(DamageInfo.Simple(1, DamageType.Contact).GrantsSoul);
        }

        [Test]
        public void DirectionBetween_IsUnitLength()
        {
            Vector2 dir = DamageInfo.DirectionBetween(new Vector2(1f, 1f), new Vector2(-4f, 13f));
            Assert.AreEqual(1f, dir.magnitude, 1e-5f);
        }
    }
}
