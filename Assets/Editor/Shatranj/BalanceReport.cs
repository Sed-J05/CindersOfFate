using System.Collections.Generic;
using System.IO;
using System.Text;
using Shatranj.Data;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Balance Report</c> (Prompt 17). Simulates nothing: reads every EnemyConfig / BossConfig and the
    /// PlayerConfig and prints HP, nail hits to kill, fireball hits, damage, telegraphs and a punish-window estimate of
    /// fight length against the Docs/04 §G targets. Checks the hard rules (readable telegraphs, fights that can't fit
    /// their target, hits that take half the hearts) and lists every value that drifted from the documented defaults.
    /// Also written to Docs/BALANCE_REPORT.md.
    /// </summary>
    public static class BalanceReport
    {
        public const string ReportPath = "Docs/BALANCE_REPORT.md";
        /// <summary>CLAUDE.md §5: every attack needs a visible telegraph; below this it isn't readable.</summary>
        public const float MinReadableTelegraph = 0.25f;

        /// <summary>Docs/04 §G balance targets (skilled player).</summary>
        public struct Target
        {
            public string label; public string id; public float minSeconds, maxSeconds; public int minHearts, maxHearts;
            public Target(string label, string id, float minS, float maxS, int minH, int maxH)
            { this.label = label; this.id = id; minSeconds = minS; maxSeconds = maxS; minHearts = minH; maxHearts = maxH; }
        }

        public static readonly Target RegularTarget = new Target("Regular enemy", "", 2f, 6f, 0, 0);
        public static readonly Target[] BossTargets =
        {
            new Target("MB1", "kage_warden", 45f, 70f, 1, 2),
            new Target("MB2", "kage_caller", 70f, 110f, 2, 3),
            new Target("MB3", "iron_colossus", 90f, 140f, 3, 4),
            new Target("Final boss", "zulmat", 150f, 240f, 4, 6),
        };

        public static readonly string[] EnemyNames = { "Crawler", "Lunger", "ShadowKite", "Knight", "Bat", "Charger", "Thrower" };
        public static readonly string[] BossNames = { "KageWarden", "KageCaller", "IronColossus", "Zulmat" };

        // ------------------------------------------------------------------
        // Pure maths (unit-tested)
        // ------------------------------------------------------------------

        public static int HitsToKill(int hp, int damagePerHit) => damagePerHit <= 0 ? int.MaxValue : Mathf.CeilToInt(hp / (float)damagePerHit);

        /// <summary>Fastest possible kill: the first hit is free, then one per attack cooldown.</summary>
        public static float MinKillSeconds(int hits, float attackCooldown) => hits <= 1 ? 0f : (hits - 1) * attackCooldown;

        /// <summary>Telegraph the player actually sees: enraged reduction applied, never below the config floor.</summary>
        public static float EffectiveTelegraph(BossConfig b, BossMoveConfig m, bool enraged)
        {
            float reduced = m.telegraph - (enraged ? b.enrageTelegraphReduction : 0f);
            return Mathf.Max(Mathf.Max(0.05f, b.minTelegraph), reduced);
        }

        /// <summary>
        /// Punish-window hit rate (hits / s) a skilled player can sustain: over the weighted damaging moves, the time
        /// after each attack (recovery + fairness gap) divided by the whole move cycle, spent swinging at the attack
        /// cooldown. Movement / summon moves count as half-open windows.
        /// </summary>
        public static float PunishHitRate(BossConfig b, float attackCooldown)
        {
            float cycleSum = 0f, windowSum = 0f, weightSum = 0f;
            foreach (BossMoveConfig m in b.moves)
            {
                if (m == null || m.weight <= 0f) continue;
                float hits = Mathf.Max(1, m.hits);
                float active = m.damage > 0 ? m.active * hits + m.hitGap * (hits - 1) : Mathf.Max(m.active, m.duration);
                float cycle = m.telegraph + active + m.recovery + b.fairnessGap;
                float window = m.damage > 0 ? m.recovery + b.fairnessGap : cycle * 0.5f;
                cycleSum += cycle * m.weight;
                windowSum += window * m.weight;
                weightSum += m.weight;
            }
            if (weightSum <= 0f || cycleSum <= 0f || attackCooldown <= 0f) return 0f;
            return windowSum / cycleSum / attackCooldown;
        }

        /// <summary>Skilled-play estimate: hits at the punish rate + intro + stage transitions.</summary>
        public static float EstimatedFightSeconds(BossConfig b, int hits, float attackCooldown)
        {
            float rate = PunishHitRate(b, attackCooldown);
            if (rate <= 0f) return float.PositiveInfinity;
            return hits / rate + b.introDuration + b.stageThresholds.Length * b.stageTransitionInvulnerability;
        }

        // ------------------------------------------------------------------
        // Report
        // ------------------------------------------------------------------

        public class Result
        {
            public readonly StringBuilder text = new StringBuilder(4096);
            public readonly List<string> failures = new List<string>();
            public readonly List<string> warnings = new List<string>();
            public readonly List<string> drift = new List<string>();
        }

        [MenuItem("Shatranj/Balance Report", false, 72)]
        public static void Run()
        {
            Result r = Build();
            string summary = $"=== Shatranj / Balance Report === {r.failures.Count} failure(s), {r.warnings.Count} warning(s), {r.drift.Count} value(s) differ from the doc defaults. Full report: {ReportPath}\n";
            File.WriteAllText(ReportPath, r.text.ToString());
            AssetDatabase.Refresh();
            if (r.failures.Count > 0) Debug.LogError(summary + r.text);
            else Debug.Log(summary + r.text);
        }

        public static Result Build()
        {
            var r = new Result();
            var player = AssetDatabase.LoadAssetAtPath<PlayerConfig>(GenerateConfigs.PlayerPath);
            if (player == null) player = ScriptableObject.CreateInstance<PlayerConfig>();
            float cd = player.attackCooldown;
            int nail = player.attackDamage, fireball = player.fireballDamage;
            int fireballsFromHits = Mathf.Max(1, Mathf.CeilToInt(player.fireballCost / (float)Mathf.Max(1, player.soulPerHit)));

            StringBuilder t = r.text;
            t.AppendLine("# BALANCE REPORT (generated by Shatranj/Balance Report — do not edit)").AppendLine();
            t.AppendLine($"Player: nail {nail} dmg every {cd:0.00} s · fireball {fireball} dmg for {player.fireballCost} soul ({fireballsFromHits} nail hits of soul) · hearts {player.heartsPhase1} → {player.heartsPhase2}.");
            t.AppendLine($"Rules: every attack telegraph ≥ {MinReadableTelegraph:0.00} s (after enrage), a fight must be finishable inside its target, no single hit may take half the hearts.").AppendLine();

            // ---- Regular enemies ----
            t.AppendLine("## Regular enemies (target 2–6 s, 0 hearts lost)").AppendLine();
            t.AppendLine("| Enemy | Phase | HP | Nail hits | Fireball hits | Fastest kill | Contact | Attack | Projectile | Telegraph | Hits to empty hearts |");
            t.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|");
            foreach (string n in EnemyNames)
            {
                var e = AssetDatabase.LoadAssetAtPath<EnemyConfig>(GenerateConfigs.EnemyPath(n));
                if (e == null) { r.warnings.Add($"{n}: no EnemyConfig asset (run Generate Config Assets)"); continue; }
                bool p2 = e.scalesWithFlame;
                int hearts = p2 ? player.heartsPhase2 : player.heartsPhase1;
                int hits = HitsToKill(e.maxHp, nail);
                float fastest = MinKillSeconds(hits, cd);
                int maxHit = Mathf.Max(e.contactDamage, Mathf.Max(e.attackDamage, e.projectileDamage));
                bool hasAttack = !string.IsNullOrEmpty(e.attackName);
                t.AppendLine($"| {n} | {(p2 ? 2 : 1)} | {e.maxHp} | {hits} | {HitsToKill(e.maxHp, fireball)} | {fastest:0.0} s | {e.contactDamage} | {(hasAttack ? e.attackDamage.ToString() : "—")} | {(e.projectileSpeed > 0f ? e.projectileDamage.ToString() : "—")} | {(hasAttack ? e.telegraphTime.ToString("0.00") + " s" : "contact only")} | {Mathf.CeilToInt(hearts / (float)Mathf.Max(1, maxHit))} |");

                if (hasAttack && e.telegraphTime < MinReadableTelegraph) r.failures.Add($"{n}: attack '{e.attackName}' telegraph {e.telegraphTime:0.00} s < {MinReadableTelegraph:0.00} s");
                if (fastest > RegularTarget.maxSeconds) r.failures.Add($"{n}: fastest kill {fastest:0.0} s exceeds the {RegularTarget.maxSeconds} s target");
                if (maxHit * 2 >= hearts) r.warnings.Add($"{n}: one hit ({maxHit}) takes half the hearts ({hearts})");
                Drift(e, n, r);
            }
            t.AppendLine();

            // ---- Bosses ----
            t.AppendLine("## Bosses (Docs/04 §G)").AppendLine();
            t.AppendLine("| Boss | HP | Nail hits | Fastest (pure DPS) | Punish rate | Estimated | Target | Biggest hit | Hearts | Stages |");
            t.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
            var moveLines = new StringBuilder();
            foreach (string n in BossNames)
            {
                var b = AssetDatabase.LoadAssetAtPath<BossConfig>(GenerateConfigs.BossPath(n));
                if (b == null) { r.warnings.Add($"{n}: no BossConfig asset (run Generate Config Assets)"); continue; }
                Target target = FindTarget(b.id);
                int hearts = b.phase >= 2 ? player.heartsPhase2 : player.heartsPhase1;
                int hits = HitsToKill(b.maxHp, nail);
                float fastest = MinKillSeconds(hits, cd);
                float rate = PunishHitRate(b, cd);
                float estimate = EstimatedFightSeconds(b, hits, cd);
                int biggest = b.contactDamage;
                foreach (BossMoveConfig m in b.moves) if (m != null) biggest = Mathf.Max(biggest, m.damage);
                t.AppendLine($"| {b.displayName} | {b.maxHp} | {hits} | {fastest:0} s | {rate:0.00} hits/s | {estimate:0} s | {target.minSeconds:0}–{target.maxSeconds:0} s | {biggest} | {target.minHearts}–{target.maxHearts} of {hearts} | {b.StageCount} |");

                if (fastest > target.maxSeconds) r.failures.Add($"{b.displayName}: even pure DPS takes {fastest:0} s, over the {target.maxSeconds:0} s target");
                if (estimate < target.minSeconds / 3f) r.warnings.Add($"{b.displayName}: punish-window estimate {estimate:0} s is far under the {target.minSeconds:0} s target (check in playtest)");
                if (biggest * 2 >= hearts) r.warnings.Add($"{b.displayName}: one hit ({biggest}) takes half the hearts ({hearts})");

                moveLines.AppendLine($"### {b.displayName} moves").AppendLine();
                moveLines.AppendLine("| Move | Stage | Dmg | Telegraph | Enraged | Active | Recovery | Weight |");
                moveLines.AppendLine("|---|---|---|---|---|---|---|---|");
                foreach (BossMoveConfig m in b.moves)
                {
                    if (m == null) continue;
                    float normal = EffectiveTelegraph(b, m, false), enraged = EffectiveTelegraph(b, m, b.enrageBelowFraction > 0f);
                    moveLines.AppendLine($"| {m.name} | {m.minStage}+ | {m.damage} | {normal:0.00} s | {enraged:0.00} s | {m.active:0.00} s | {m.recovery:0.00} s | {m.weight:0.0} |");
                    if (m.damage > 0 && enraged < MinReadableTelegraph - 1e-4f)
                        r.failures.Add($"{b.displayName}/{m.name}: telegraph {enraged:0.00} s after enrage < {MinReadableTelegraph:0.00} s");
                }
                moveLines.AppendLine();
                Drift(b, n, r);
            }
            t.AppendLine();
            t.AppendLine("Punish rate = time after each attack (recovery + fairness gap) ÷ the whole move cycle ÷ the attack cooldown. Estimated = nail hits ÷ punish rate + intro + stage transitions. Real fights are longer (spacing, dodging, blocks): use these numbers to compare bosses, then confirm in playtest.").AppendLine();
            t.Append(moveLines);

            // ---- Findings ----
            t.AppendLine("## Findings").AppendLine();
            if (r.failures.Count == 0 && r.warnings.Count == 0) t.AppendLine("No rule violations.");
            foreach (string f in r.failures) t.AppendLine("- **FAIL** " + f);
            foreach (string w in r.warnings) t.AppendLine("- WARN " + w);
            t.AppendLine().AppendLine("## Values that differ from the doc defaults (ConfigDefaults)").AppendLine();
            if (r.drift.Count == 0) t.AppendLine("None — every config matches the documented numbers.");
            foreach (string d in r.drift) t.AppendLine("- " + d);
            return r;
        }

        private static Target FindTarget(string id)
        {
            foreach (Target t in BossTargets) if (t.id == id) return t;
            return new Target(id, id, 0f, float.MaxValue, 0, 99);
        }

        /// <summary>Lists every serialized value of the asset that differs from a freshly generated default (doc numbers).</summary>
        private static void Drift(ScriptableObject asset, string name, Result r)
        {
            ScriptableObject fresh = FreshDefault(asset, name);
            if (fresh == null) return;
            var a = new SerializedObject(asset);
            var b = new SerializedObject(fresh);
            SerializedProperty it = a.GetIterator();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = true;
                if (it.propertyPath == "m_Script") continue;
                if (it.propertyType == SerializedPropertyType.ObjectReference) { enter = false; continue; }
                if (it.hasVisibleChildren && it.propertyType == SerializedPropertyType.Generic) continue;   // compare leaves only
                SerializedProperty other = b.FindProperty(it.propertyPath);
                if (other == null) { r.drift.Add($"{name}.{it.propertyPath}: not in the defaults"); continue; }
                if (!SerializedProperty.DataEquals(it, other)) r.drift.Add($"{name}.{it.propertyPath}: {Describe(it)} (doc default {Describe(other)})");
            }
            Object.DestroyImmediate(fresh);
        }

        private static ScriptableObject FreshDefault(ScriptableObject asset, string name)
        {
            switch (asset)
            {
                case EnemyConfig _:
                {
                    var c = ScriptableObject.CreateInstance<EnemyConfig>();
                    switch (name)
                    {
                        case "Crawler": ConfigDefaults.Crawler(c); break;
                        case "Lunger": ConfigDefaults.Lunger(c); break;
                        case "ShadowKite": ConfigDefaults.ShadowKite(c); break;
                        case "Knight": ConfigDefaults.Knight(c); break;
                        case "Bat": ConfigDefaults.Bat(c); break;
                        case "Charger": ConfigDefaults.Charger(c); break;
                        case "Thrower": ConfigDefaults.Thrower(c); break;
                        default: Object.DestroyImmediate(c); return null;
                    }
                    return c;
                }
                case BossConfig _:
                {
                    var c = ScriptableObject.CreateInstance<BossConfig>();
                    switch (name)
                    {
                        case "KageWarden": ConfigDefaults.KageWarden(c); break;
                        case "KageCaller": ConfigDefaults.KageCaller(c); break;
                        case "IronColossus": ConfigDefaults.IronColossus(c); break;
                        case "Zulmat": ConfigDefaults.Zulmat(c); break;
                        default: Object.DestroyImmediate(c); return null;
                    }
                    return c;
                }
            }
            return null;
        }

        private static string Describe(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer: return p.intValue.ToString();
                case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                case SerializedPropertyType.Float: return p.floatValue.ToString("0.###");
                case SerializedPropertyType.String: return "\"" + p.stringValue + "\"";
                case SerializedPropertyType.Vector2: return p.vector2Value.ToString("0.##");
                case SerializedPropertyType.Vector2Int: return p.vector2IntValue.ToString();
                case SerializedPropertyType.Color: return "#" + ColorUtility.ToHtmlStringRGBA(p.colorValue);
                case SerializedPropertyType.Enum: return p.enumValueIndex >= 0 && p.enumValueIndex < p.enumDisplayNames.Length ? p.enumDisplayNames[p.enumValueIndex] : p.enumValueIndex.ToString();
                case SerializedPropertyType.ArraySize: return "size " + p.intValue;
                default: return p.propertyType.ToString();
            }
        }
    }
}
