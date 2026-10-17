using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Shatranj.Data;
using Shatranj.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shatranj.Tests
{
    /// <summary>Prompt 17: feel presets, balance rules over the documented configs, and the audio hook list.</summary>
    public class PolishBalanceTests
    {
        // ---------------- Feel ----------------

        [Test]
        public void FeelDefaults_HaveExactlyOnePresetPerEvent()
        {
            FeelPreset[] presets = FeelConfig.Defaults();
            foreach (FeelEvent e in Enum.GetValues(typeof(FeelEvent)))
            {
                int n = Array.FindAll(presets, p => p.id == e).Length;
                Assert.AreEqual(1, n, $"{e}: {n} presets");
            }
            foreach (FeelPreset p in presets)
            {
                Assert.LessOrEqual(p.hitStop, 0.12f, $"{p.id}: Docs/02 §4 hit-stop is 0.05–0.12 s");
                Assert.LessOrEqual(p.rumbleLow, 1f); Assert.LessOrEqual(p.rumbleHigh, 1f);
            }
            var cfg = ScriptableObject.CreateInstance<FeelConfig>();
            Assert.IsNotNull(cfg.Find(FeelEvent.BossDeath));
            Object.DestroyImmediate(cfg);
        }

        // ---------------- Balance maths ----------------

        [Test]
        public void BalanceMath_Basics()
        {
            Assert.AreEqual(5, BalanceReport.HitsToKill(5, 1));
            Assert.AreEqual(3, BalanceReport.HitsToKill(7, 3), "fireball 3 dmg vs 7 HP");
            Assert.AreEqual(int.MaxValue, BalanceReport.HitsToKill(5, 0));
            Assert.AreEqual(0f, BalanceReport.MinKillSeconds(1, 0.4f));
            Assert.AreEqual(1.6f, BalanceReport.MinKillSeconds(5, 0.4f), 1e-5f);
        }

        [Test]
        public void TelegraphFloor_KeepsEnragedBossMovesReadable()
        {
            var colossus = ScriptableObject.CreateInstance<BossConfig>();
            ConfigDefaults.IronColossus(colossus);
            Assert.AreEqual(0.25f, colossus.minTelegraph, 1e-5f);
            Assert.AreEqual(0.30f, BalanceReport.EffectiveTelegraph(colossus, colossus.FindMove("ShieldBash"), false), 1e-5f);
            Assert.AreEqual(0.25f, BalanceReport.EffectiveTelegraph(colossus, colossus.FindMove("ShieldBash"), true), 1e-5f, "0.30 − 0.15 would be 0.15 s");
            Assert.AreEqual(0.55f, BalanceReport.EffectiveTelegraph(colossus, colossus.FindMove("BigArcSlash"), true), 1e-5f, "longer telegraphs keep the full −0.15");

            var zulmat = ScriptableObject.CreateInstance<BossConfig>();
            ConfigDefaults.Zulmat(zulmat);
            Assert.AreEqual(0.25f, BalanceReport.EffectiveTelegraph(zulmat, zulmat.FindMove("ShrinkLunge"), true), 1e-5f, "0.35 − 0.15 would be 0.20 s");
            Object.DestroyImmediate(colossus);
            Object.DestroyImmediate(zulmat);
        }

        private static readonly (string name, Action<EnemyConfig> fill)[] Enemies =
        {
            ("Crawler", ConfigDefaults.Crawler), ("Lunger", ConfigDefaults.Lunger), ("ShadowKite", ConfigDefaults.ShadowKite),
            ("Knight", ConfigDefaults.Knight), ("Bat", ConfigDefaults.Bat), ("Charger", ConfigDefaults.Charger), ("Thrower", ConfigDefaults.Thrower),
        };

        private static readonly (string id, Action<BossConfig> fill)[] Bosses =
        {
            ("kage_warden", ConfigDefaults.KageWarden), ("kage_caller", ConfigDefaults.KageCaller),
            ("iron_colossus", ConfigDefaults.IronColossus), ("zulmat", ConfigDefaults.Zulmat),
        };

        [Test]
        public void DocumentedEnemies_PassTheBalanceRules()
        {
            var player = ScriptableObject.CreateInstance<PlayerConfig>();
            foreach (var (name, fill) in Enemies)
            {
                var e = ScriptableObject.CreateInstance<EnemyConfig>();
                fill(e);
                int hearts = e.scalesWithFlame ? player.heartsPhase2 : player.heartsPhase1;
                int hits = BalanceReport.HitsToKill(e.maxHp, player.attackDamage);
                Assert.LessOrEqual(BalanceReport.MinKillSeconds(hits, player.attackCooldown), BalanceReport.RegularTarget.maxSeconds, name);
                if (!string.IsNullOrEmpty(e.attackName))
                    Assert.GreaterOrEqual(e.telegraphTime, BalanceReport.MinReadableTelegraph, name + " telegraph");
                int maxHit = Mathf.Max(e.contactDamage, Mathf.Max(e.attackDamage, e.projectileDamage));
                Assert.Less(maxHit * 2, hearts, name + ": one hit takes half the hearts");
                Object.DestroyImmediate(e);
            }
            Object.DestroyImmediate(player);
        }

        [Test]
        public void DocumentedBosses_PassTheHardBalanceRules()
        {
            var player = ScriptableObject.CreateInstance<PlayerConfig>();
            foreach (var (id, fill) in Bosses)
            {
                var b = ScriptableObject.CreateInstance<BossConfig>();
                fill(b);
                BalanceReport.Target target = Array.Find(BalanceReport.BossTargets, t => t.id == id);
                int hearts = b.phase >= 2 ? player.heartsPhase2 : player.heartsPhase1;
                int hits = BalanceReport.HitsToKill(b.maxHp, player.attackDamage);

                Assert.LessOrEqual(BalanceReport.MinKillSeconds(hits, player.attackCooldown), target.maxSeconds, id + ": pure DPS must fit the target");
                // The punish-window estimate is a playtest hint (a WARN in the report), not a rule: only check it is computed.
                Assert.Greater(BalanceReport.PunishHitRate(b, player.attackCooldown), 0f, id);
                Assert.Less(BalanceReport.EstimatedFightSeconds(b, hits, player.attackCooldown), float.PositiveInfinity, id);

                int biggest = b.contactDamage;
                foreach (BossMoveConfig m in b.moves)
                {
                    biggest = Mathf.Max(biggest, m.damage);
                    if (m.damage > 0)
                        Assert.GreaterOrEqual(BalanceReport.EffectiveTelegraph(b, m, b.enrageBelowFraction > 0f), BalanceReport.MinReadableTelegraph - 1e-4f, $"{id}/{m.name} enraged telegraph");
                }
                Assert.Less(biggest * 2, hearts, id + ": one hit takes half the hearts");
                Object.DestroyImmediate(b);
            }
            Object.DestroyImmediate(player);
        }

        // ---------------- Audio hooks ----------------

        [Test]
        public void EveryAudioIdInCode_IsListedInAudioHooksDoc()
        {
            string doc = File.ReadAllText("Docs/AUDIO_HOOKS.md");
            var call = new Regex("AudioManager\\.(?:Play|PlayMusic)\\(\\s*\"([a-z0-9_]+)\"");
            var ternary = new Regex("AudioManager\\.(?:Play|PlayMusic)\\([^;]*?\\?\\s*\"([a-z0-9_]+)\"\\s*:\\s*\"([a-z0-9_]+)\"");
            foreach (string file in Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories))
            {
                string src = File.ReadAllText(file);
                foreach (Match m in call.Matches(src))
                    StringAssert.Contains("`" + m.Groups[1].Value + "`", doc, $"{m.Groups[1].Value} ({file}) missing from Docs/AUDIO_HOOKS.md — rerun the generator");
                foreach (Match m in ternary.Matches(src))
                {
                    StringAssert.Contains("`" + m.Groups[1].Value + "`", doc, file);
                    StringAssert.Contains("`" + m.Groups[2].Value + "`", doc, file);
                }
            }
        }
    }
}
