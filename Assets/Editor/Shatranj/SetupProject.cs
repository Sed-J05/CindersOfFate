using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Shatranj.Core;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Setup Project</c>. Idempotent one-shot project configuration:
    /// folder structure, layers, tags, sorting layers, Physics2D collision matrix,
    /// fixed timestep, gravity and the root .gitignore. Prints a summary to the Console.
    /// Safe to re-run at any time; never deletes or renames anything that already exists.
    /// </summary>
    public static class SetupProject
    {
        private const float FixedTimestep = 0.01666f;
        private static readonly Vector2 Gravity = new Vector2(0f, -9.81f);

        /// <summary>Folder structure from Docs/02 §2.</summary>
        private static readonly string[] Folders =
        {
            "Assets/Art/Characters", "Assets/Art/FX", "Assets/Art/Tiles", "Assets/Art/Backgrounds",
            "Assets/Art/UI", "Assets/Art/Cutscenes", "Assets/Art/Props",
            "Assets/Audio",
            "Assets/Data/Configs", "Assets/Data/Levels", "Assets/Data/Cutscenes",
            "Assets/Prefabs/Player", "Assets/Prefabs/Enemies", "Assets/Prefabs/Bosses", "Assets/Prefabs/Projectiles",
            "Assets/Prefabs/FX", "Assets/Prefabs/UI", "Assets/Prefabs/Level",
            "Assets/Scenes",
            "Assets/Scripts/Core", "Assets/Scripts/Player", "Assets/Scripts/Enemies", "Assets/Scripts/Bosses",
            "Assets/Scripts/Level", "Assets/Scripts/UI", "Assets/Scripts/Cutscenes", "Assets/Scripts/Visuals",
            "Assets/Scripts/Data", "Assets/Scripts/Utility",
            "Assets/Editor/Shatranj",
            "Assets/Settings",
        };

        /// <summary>Lines that must be present in .gitignore (patterns from the standard Unity template).</summary>
        private static readonly string[] GitIgnoreEssentials =
        {
            "/[Ll]ibrary/", "/[Tt]emp/", "/[Oo]bj/", "/[Bb]uild/", "/[Bb]uilds/", "/[Ll]ogs/",
            "/[Uu]ser[Ss]ettings/", "/[Mm]emoryCaptures/", "/[Rr]ecordings/", ".vs/",
            "*.csproj", "*.sln", "*.suo", "*.user", "*.pidb", "*.booproj", "*.svd", "*.pdb", "*.mdb",
            "*.opendb", "*.VC.db", "sysinfo.txt", "*.apk", "*.aab", "*.unitypackage", "*.app",
            "crashlytics-build.properties", ".DS_Store", "Thumbs.db",
        };

        [MenuItem("Shatranj/Setup Project", false, 0)]
        public static void Run()
        {
            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Setup Project ===");
            report.Append("Unity ").Append(Application.unityVersion).Append("  |  project: ")
                  .AppendLine(Path.GetFileName(ProjectRoot));

            int failures = 0;

            Stage(report, ref failures, "Folders", () => EnsureFolders(report));

            SerializedObject tagManager = null;
            Stage(report, ref failures, "TagManager", () =>
            {
                tagManager = LoadTagManager();
                SetupLayers(tagManager, report);
                SetupTags(tagManager, report);
                SetupSortingLayers(tagManager, report);
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                tagManager.Update();
            });

            Stage(report, ref failures, "Physics2D matrix", () => SetupCollisionMatrix(report));
            Stage(report, ref failures, "Time", () => SetupTime(report));
            Stage(report, ref failures, "Gravity", () => SetupGravity(report));
            Stage(report, ref failures, ".gitignore", () => EnsureGitIgnore(report));
            Stage(report, ref failures, "Input handling", () => ReportInputHandling(report));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string validation;
            bool ok = GameLayers.Validate(out validation);
            report.AppendLine(ok ? "Validation: PASS" : "Validation: FAIL\n" + validation);

            if (failures == 0 && ok)
            {
                report.AppendLine("Done. Re-run any time; the operation is idempotent.");
                Debug.Log(report.ToString());
            }
            else
            {
                report.Append("Done with ").Append(failures).AppendLine(" failed stage(s) — see lines marked FAILED above.");
                Debug.LogError(report.ToString());
            }
        }

        /// <summary>Runs one stage; a failure is recorded in the report instead of aborting the whole setup.</summary>
        private static void Stage(StringBuilder report, ref int failures, string name, Action action)
        {
            try { action(); }
            catch (Exception e)
            {
                failures++;
                report.Append("FAILED ").Append(name).Append(": ").AppendLine(e.Message);
            }
        }

        // ------------------------------------------------------------------
        // Type-aware SerializedProperty access. Unity 6 serializes some settings as
        // uint / bool / double, and the plain intValue / floatValue setters reject those.
        // ------------------------------------------------------------------

        private static int GetInt(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Boolean: return p.boolValue ? 1 : 0;
                case SerializedPropertyType.Integer:
                    switch (p.numericType)
                    {
                        case SerializedPropertyNumericType.UInt32: return unchecked((int)p.uintValue);
                        case SerializedPropertyNumericType.Int64:  return unchecked((int)p.longValue);
                        case SerializedPropertyNumericType.UInt64: return unchecked((int)p.ulongValue);
                        default: return p.intValue;
                    }
                default:
                    throw new InvalidOperationException($"'{p.name}' is {p.propertyType}/{p.numericType}, not an integer");
            }
        }

        private static void SetInt(SerializedProperty p, int value)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Boolean: p.boolValue = value != 0; break;
                case SerializedPropertyType.Integer:
                    switch (p.numericType)
                    {
                        case SerializedPropertyNumericType.UInt32: p.uintValue = unchecked((uint)value); break;
                        case SerializedPropertyNumericType.Int64:  p.longValue = value; break;
                        case SerializedPropertyNumericType.UInt64: p.ulongValue = unchecked((uint)value); break;
                        default: p.intValue = value; break;
                    }
                    break;
                default:
                    throw new InvalidOperationException($"'{p.name}' is {p.propertyType}/{p.numericType}, not an integer");
            }
        }

        /// <summary>
        /// Sets a float, double, or (Unity 6 TimeManager) a struct that wraps a single numeric field.
        /// Returns the property path actually written.
        /// </summary>
        private static string SetFloat(SerializedProperty p, float value)
        {
            if (p.propertyType == SerializedPropertyType.Float)
            {
                if (p.numericType == SerializedPropertyNumericType.Double) p.doubleValue = value;
                else p.floatValue = value;
                return p.propertyPath;
            }

            if (p.propertyType == SerializedPropertyType.Generic)
            {
                // Walk the struct's children and write the first numeric leaf.
                SerializedProperty it = p.Copy();
                SerializedProperty end = p.GetEndProperty();
                bool enterChildren = true;
                var seen = new List<string>();
                while (it.Next(enterChildren) && !SerializedProperty.EqualContents(it, end))
                {
                    enterChildren = it.propertyType == SerializedPropertyType.Generic;
                    seen.Add(it.propertyPath + ":" + it.propertyType);
                    if (it.propertyType == SerializedPropertyType.Float)
                    {
                        if (it.numericType == SerializedPropertyNumericType.Double) it.doubleValue = value;
                        else it.floatValue = value;
                        return it.propertyPath;
                    }
                    if (it.propertyType == SerializedPropertyType.Integer)
                    {
                        // Some wrappers store seconds*N or similar; refuse rather than guess.
                        continue;
                    }
                }
                throw new InvalidOperationException($"'{p.name}' is a struct with no float child. Children: {string.Join(", ", seen)}");
            }

            throw new InvalidOperationException($"'{p.name}' is {p.propertyType}/{p.numericType}, not a float");
        }

        // ------------------------------------------------------------------
        // Folders
        // ------------------------------------------------------------------

        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        private static void EnsureFolders(StringBuilder report)
        {
            int created = 0, kept = 0;
            foreach (string rel in Folders)
            {
                string full = Path.Combine(ProjectRoot, rel.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(full))
                {
                    Directory.CreateDirectory(full);
                    created++;
                }

                // Unity ignores dot-files, so .gitkeep never gets a .meta and never shows in the Project window.
                if (IsEffectivelyEmpty(full))
                {
                    string keep = Path.Combine(full, ".gitkeep");
                    if (!File.Exists(keep)) { File.WriteAllText(keep, string.Empty); kept++; }
                }
            }
            report.Append("Folders: ").Append(Folders.Length).Append(" ensured (").Append(created)
                  .Append(" created, ").Append(kept).AppendLine(" .gitkeep added)");
        }

        private static bool IsEffectivelyEmpty(string dir)
        {
            foreach (string entry in Directory.EnumerateFileSystemEntries(dir))
            {
                string name = Path.GetFileName(entry);
                if (name == ".gitkeep") return false;
                if (name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                return false;
            }
            return true;
        }

        // ------------------------------------------------------------------
        // TagManager: layers, tags, sorting layers
        // ------------------------------------------------------------------

        private static SerializedObject LoadTagManager()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
                throw new InvalidOperationException("Could not load ProjectSettings/TagManager.asset");
            return new SerializedObject(assets[0]);
        }

        private static void SetupLayers(SerializedObject tagManager, StringBuilder report)
        {
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray) throw new InvalidOperationException("TagManager has no 'layers' array");

            int created = 0, ok = 0;
            var problems = new List<string>();

            foreach (GameLayers.LayerDef def in GameLayers.All)
            {
                if (def.Index < GameLayers.FirstUserLayer || def.Index >= layers.arraySize)
                {
                    problems.Add($"'{def.Name}' index {def.Index} is not a user layer slot");
                    continue;
                }

                int existingAt = IndexOfString(layers, def.Name);
                if (existingAt >= 0 && existingAt != def.Index)
                {
                    problems.Add($"layer '{def.Name}' already exists at slot {existingAt}, GameLayers expects {def.Index} — fix one of them");
                    continue;
                }

                SerializedProperty slot = layers.GetArrayElementAtIndex(def.Index);
                if (slot.stringValue == def.Name) { ok++; continue; }
                if (!string.IsNullOrEmpty(slot.stringValue))
                {
                    problems.Add($"slot {def.Index} is occupied by '{slot.stringValue}', wanted '{def.Name}' — not overwritten");
                    continue;
                }

                slot.stringValue = def.Name;
                created++;
            }

            report.Append("Layers: ").Append(created).Append(" created, ").Append(ok).Append(" already OK");
            AppendProblems(report, problems);
        }

        private static void SetupTags(SerializedObject tagManager, StringBuilder report)
        {
            SerializedProperty tags = tagManager.FindProperty("tags");
            if (tags == null || !tags.isArray) throw new InvalidOperationException("TagManager has no 'tags' array");

            int created = 0, ok = 0;
            foreach (string tag in GameLayers.Tags.All)
            {
                if (IndexOfString(tags, tag) >= 0) { ok++; continue; }
                tags.arraySize++;
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
                created++;
            }
            report.Append("Tags: ").Append(created).Append(" created, ").Append(ok).AppendLine(" already OK");
        }

        private static void SetupSortingLayers(SerializedObject tagManager, StringBuilder report)
        {
            SerializedProperty prop = tagManager.FindProperty("m_SortingLayers");
            if (prop == null || !prop.isArray) throw new InvalidOperationException("TagManager has no 'm_SortingLayers' array");

            // Remember what exists (name -> id/locked) so IDs stay stable and foreign layers survive.
            var existing = new Dictionary<string, (int id, int locked)>();
            var existingOrder = new List<string>();
            for (int i = 0; i < prop.arraySize; i++)
            {
                SerializedProperty e = prop.GetArrayElementAtIndex(i);
                string name = e.FindPropertyRelative("name").stringValue;
                int id = GetInt(e.FindPropertyRelative("uniqueID"));
                int locked = GetInt(e.FindPropertyRelative("locked"));
                // ID 0 belongs to Default only; any other layer with 0 is broken and gets a fresh ID below.
                if (id == 0 && name != GameLayers.Sorting.Default) continue;
                if (!existing.ContainsKey(name)) { existing[name] = (id, locked); existingOrder.Add(name); }
            }

            // Desired order: Default (always id 0, kept at the back), then ours, then any unknown extras.
            var order = new List<string> { GameLayers.Sorting.Default };
            foreach (string s in GameLayers.Sorting.OrderBackToFront) if (!order.Contains(s)) order.Add(s);
            foreach (string s in existingOrder) if (!order.Contains(s)) order.Add(s);

            var usedIds = new HashSet<int>();
            foreach (var kv in existing) usedIds.Add(kv.Value.id);

            int created = 0;
            prop.arraySize = order.Count;
            for (int i = 0; i < order.Count; i++)
            {
                string name = order[i];
                int id, locked;
                if (existing.TryGetValue(name, out var found)) { id = found.id; locked = found.locked; }
                else
                {
                    id = name == GameLayers.Sorting.Default ? 0 : StableUniqueId(name, usedIds);
                    locked = 0;
                    created++;
                }

                SerializedProperty e = prop.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("name").stringValue = name;
                SetInt(e.FindPropertyRelative("uniqueID"), id);
                SetInt(e.FindPropertyRelative("locked"), locked);
            }

            report.Append("Sorting layers: ").Append(created).Append(" created; order back→front: ")
                  .AppendLine(string.Join(" < ", order));
        }

        /// <summary>Deterministic positive non-zero ID (FNV-1a, 31-bit) so re-runs on other machines produce the same IDs.</summary>
        private static int StableUniqueId(string name, HashSet<int> used)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (char c in name) { h ^= c; h *= 16777619u; }
                int id = (int)(h & 0x7FFFFFFFu);
                while (id == 0 || used.Contains(id)) id++;
                used.Add(id);
                return id;
            }
        }

        private static int IndexOfString(SerializedProperty array, string value)
        {
            for (int i = 0; i < array.arraySize; i++)
                if (array.GetArrayElementAtIndex(i).stringValue == value) return i;
            return -1;
        }

        // ------------------------------------------------------------------
        // Physics2D collision matrix
        // ------------------------------------------------------------------

        private static void SetupCollisionMatrix(StringBuilder report)
        {
            int[] managed = GameLayers.CollisionManagedLayers;
            int enabled = 0, disabled = 0;
            var mismatches = new List<string>();

            for (int i = 0; i < managed.Length; i++)
            {
                for (int j = i; j < managed.Length; j++)
                {
                    int a = managed[i], b = managed[j];
                    bool collide = GameLayers.ShouldCollide(a, b);
                    // This is exactly what the Physics 2D settings inspector calls; it persists to Physics2DSettings.asset.
                    Physics2D.IgnoreLayerCollision(a, b, !collide);
                    if (collide) enabled++; else disabled++;

                    if (Physics2D.GetIgnoreLayerCollision(a, b) == collide)
                        mismatches.Add($"{LayerMask.LayerToName(a)}×{LayerMask.LayerToName(b)}");
                }
            }

            report.Append("Physics2D matrix: ").Append(enabled).Append(" colliding pairs, ")
                  .Append(disabled).Append(" ignored pairs across ").Append(managed.Length).Append(" layers");
            AppendProblems(report, mismatches, "did not persist");

            var sb = new StringBuilder("  colliding: ");
            foreach (var p in GameLayers.CollisionPairs)
                sb.Append(LayerMask.LayerToName(p.a)).Append('×').Append(LayerMask.LayerToName(p.b)).Append("  ");
            report.AppendLine(sb.ToString().TrimEnd());
        }

        // ------------------------------------------------------------------
        // Time & gravity (SerializedObject on the settings singletons = persisted on disk; API call = live value)
        // ------------------------------------------------------------------

        private const int FixedStepHz = 60; // 1/60 s exactly; the doc's 0.01666 is this value rounded

        private static void SetupTime(StringBuilder report)
        {
            string writtenPath = null;
            bool persisted = TrySetSingletonProperty("TimeManager", "Fixed Timestep", p =>
            {
                // Unity 6 stores the step as a rational time: seconds = m_Count * m_Denominator / m_Numerator.
                SerializedProperty count = p.FindPropertyRelative("m_Count");
                SerializedProperty num   = p.FindPropertyRelative("m_Rate.m_Numerator");
                SerializedProperty den   = p.FindPropertyRelative("m_Rate.m_Denominator");
                if (count != null && num != null && den != null)
                {
                    SetInt(count, 1);
                    SetInt(num, FixedStepHz);
                    SetInt(den, 1);
                    writtenPath = $"{p.propertyPath} = 1 / ({FixedStepHz}/1)";
                }
                else
                {
                    writtenPath = SetFloat(p, FixedTimestep); // older layouts: plain float
                }
            });
            Time.fixedDeltaTime = FixedTimestep;
            report.Append("Time.fixedDeltaTime = 1/").Append(FixedStepHz).Append(" s")
                  .AppendLine(persisted ? $" (TimeManager.asset updated: {writtenPath})" : " (runtime only — set Project Settings > Time manually)");
        }

        private static void SetupGravity(StringBuilder report)
        {
            bool persisted = TrySetSingletonProperty("Physics2DSettings", "m_Gravity", p => p.vector2Value = Gravity);
            Physics2D.gravity = Gravity;
            report.Append("Physics2D.gravity = ").Append(Gravity)
                  .AppendLine(persisted ? " (Physics2DSettings.asset updated; bodies use code-driven gravity scale)" : " (runtime only)");
        }

        private static bool TrySetSingletonProperty(string singleton, string property, Action<SerializedProperty> set)
        {
            UnityEngine.Object target = Unsupported.GetSerializedAssetInterfaceSingleton(singleton);
            if (target == null) return false;
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(property);
            if (p == null) return false;
            set(p);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            return true;
        }

        // ------------------------------------------------------------------
        // .gitignore
        // ------------------------------------------------------------------

        private static void EnsureGitIgnore(StringBuilder report)
        {
            string path = Path.Combine(ProjectRoot, ".gitignore");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, BuildStandardGitIgnore());
                report.AppendLine(".gitignore: created (standard Unity template + /Builds/)");
                return;
            }

            string[] lines = File.ReadAllLines(path);
            var present = new HashSet<string>();
            foreach (string l in lines) present.Add(l.Trim());

            var missing = new List<string>();
            foreach (string e in GitIgnoreEssentials) if (!present.Contains(e)) missing.Add(e);

            if (missing.Count == 0)
            {
                report.AppendLine(".gitignore: OK (all essential Unity patterns present)");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("# --- Added by Shatranj/Setup Project (missing standard Unity patterns) ---");
            foreach (string m in missing) sb.AppendLine(m);
            File.AppendAllText(path, sb.ToString());
            report.Append(".gitignore: appended ").Append(missing.Count).AppendLine(" missing pattern(s)");
        }

        private static string BuildStandardGitIgnore()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Shatranj — standard Unity .gitignore (github/gitignore Unity template + Builds)");
            sb.AppendLine("# Maintained by: Shatranj/Setup Project (appends missing essentials, never deletes)");
            sb.AppendLine();
            sb.AppendLine("# Unity generated folders");
            sb.AppendLine("/[Ll]ibrary/");
            sb.AppendLine("/[Tt]emp/");
            sb.AppendLine("/[Oo]bj/");
            sb.AppendLine("/[Bb]uild/");
            sb.AppendLine("/[Bb]uilds/");
            sb.AppendLine("/[Ll]ogs/");
            sb.AppendLine("/[Uu]ser[Ss]ettings/");
            sb.AppendLine("/[Mm]emoryCaptures/");
            sb.AppendLine("/[Rr]ecordings/");
            sb.AppendLine();
            sb.AppendLine("# IDE / solution files");
            sb.AppendLine("/[Aa]ssets/Plugins/Editor/JetBrains*");
            sb.AppendLine(".vs/");
            sb.AppendLine(".gradle/");
            sb.AppendLine("ExportedObj/");
            sb.AppendLine(".consulo/");
            sb.AppendLine("*.csproj");
            sb.AppendLine("*.unityproj");
            sb.AppendLine("*.sln");
            sb.AppendLine("*.suo");
            sb.AppendLine("*.tmp");
            sb.AppendLine("*.user");
            sb.AppendLine("*.userprefs");
            sb.AppendLine("*.pidb");
            sb.AppendLine("*.booproj");
            sb.AppendLine("*.svd");
            sb.AppendLine("*.pdb");
            sb.AppendLine("*.mdb");
            sb.AppendLine("*.opendb");
            sb.AppendLine("*.VC.db");
            sb.AppendLine();
            sb.AppendLine("# Unity generated meta files / crash reports");
            sb.AppendLine("*.pidb.meta");
            sb.AppendLine("*.pdb.meta");
            sb.AppendLine("*.mdb.meta");
            sb.AppendLine("sysinfo.txt");
            sb.AppendLine();
            sb.AppendLine("# Builds");
            sb.AppendLine("*.apk");
            sb.AppendLine("*.aab");
            sb.AppendLine("*.unitypackage");
            sb.AppendLine("*.app");
            sb.AppendLine("crashlytics-build.properties");
            sb.AppendLine("/[Aa]ssets/[Aa]ddressable[Aa]ssets[Dd]ata/*/*.bin*");
            sb.AppendLine("/[Aa]ssets/[Ss]treamingAssets/aa.meta");
            sb.AppendLine("/[Aa]ssets/[Ss]treamingAssets/aa/*");
            sb.AppendLine();
            sb.AppendLine("# OS junk");
            sb.AppendLine(".DS_Store");
            sb.AppendLine("Thumbs.db");
            return sb.ToString();
        }

        // ------------------------------------------------------------------
        // Diagnostics
        // ------------------------------------------------------------------

        private static void ReportInputHandling(StringBuilder report)
        {
            UnityEngine.Object target = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
            if (target == null) { report.AppendLine("Input handling: could not read PlayerSettings"); return; }
            SerializedProperty p = new SerializedObject(target).FindProperty("activeInputHandler");
            if (p == null) { report.AppendLine("Input handling: property not found"); return; }

            switch (GetInt(p))
            {
                case 0:
                    report.AppendLine("WARNING Input handling: Input Manager (Old). Set Project Settings > Player > Active Input Handling to 'Input System Package (New)' or 'Both' (editor restart required).");
                    break;
                case 1:
                    report.AppendLine("Input handling: Input System Package (New) — OK");
                    break;
                default:
                    report.AppendLine("Input handling: Both — OK");
                    break;
            }
        }

        private static void AppendProblems(StringBuilder report, List<string> problems, string label = "problems")
        {
            if (problems.Count == 0) { report.AppendLine(); return; }
            report.Append("  — ").Append(problems.Count).Append(' ').Append(label).AppendLine(":");
            foreach (string p in problems) report.Append("    ! ").AppendLine(p);
        }
    }
}
