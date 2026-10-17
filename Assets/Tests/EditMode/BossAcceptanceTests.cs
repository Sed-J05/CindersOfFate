using NUnit.Framework;
using Shatranj.Data;
using Shatranj.EditorTools;
using UnityEngine;

namespace Shatranj.Tests
{
    /// <summary>Kage Warden numbers per Docs/04 §B1, and every damaging move has a telegraph.</summary>
    public class BossAcceptanceTests
    {
        private static BossConfig Warden()
        {
            var b = ScriptableObject.CreateInstance<BossConfig>();
            ConfigDefaults.KageWarden(b);
            return b;
        }

        [Test]
        public void KageWarden_MatchesDocs04()
        {
            BossConfig b = Warden();
            Assert.AreEqual(20, b.maxHp);
            Assert.AreEqual(1, b.contactDamage);
            Assert.AreEqual(3f, b.moveSpeed);
            Assert.AreEqual(0.8f, b.advanceTimeMin);
            Assert.AreEqual(1.4f, b.advanceTimeMax);
            Assert.AreEqual(3f, b.backstepDistance);
            Assert.AreEqual(0.35f, b.backstepTime);
            Assert.AreEqual(1.5f, b.blockDuration);
            Assert.AreEqual(2f, b.blockPushback);
            Assert.AreEqual(2, b.blockMinHitsToTrigger);
            Assert.AreEqual(2f, b.blockHitWindow);
            Assert.AreEqual(0.2f, b.blockRandomChance, 1e-5f);
            Assert.AreEqual(2.5f, b.counterSlashRange);
            Assert.AreEqual(1.2f, b.deathSlowmoTime);
            Assert.AreEqual("Keeper of the Gate", b.epithet);

            BossMoveConfig slash = b.FindMove("SlashCombo");
            Assert.IsNotNull(slash);
            Assert.AreEqual(0.45f, slash.telegraph);
            Assert.AreEqual(2.2f, slash.reach);
            Assert.AreEqual(0.1f, slash.active);
            Assert.AreEqual(0.25f, slash.hitGap);
            Assert.AreEqual(0.6f, slash.recovery);
            Assert.AreEqual(1, slash.damage);

            BossMoveConfig counter = b.FindMove("CounterSlash");
            Assert.IsNotNull(counter);
            Assert.AreEqual(0.3f, counter.telegraph);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void EveryDamagingBossMove_HasATelegraph()
        {
            BossConfig b = Warden();
            foreach (BossMoveConfig m in b.moves)
                if (m.damage > 0) Assert.Greater(m.telegraph, 0f, $"{b.id}/{m.name}");
            Assert.Greater(b.telegraphFlashTime, 0f);
            Object.DestroyImmediate(b);
        }
    }
}
