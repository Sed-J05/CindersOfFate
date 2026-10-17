using System;
using System.Text;
using Shatranj.Data;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Generate Config Assets</c>. Creates every ScriptableObject config under
    /// Assets/Data/Configs with the doc defaults (<see cref="ConfigDefaults"/>). Existing assets are skipped,
    /// so hand-tuned values survive re-runs. Delete an asset to regenerate it.
    /// </summary>
    public static class GenerateConfigs
    {
        public const string Root = "Assets/Data/Configs";
        public const string EnemiesFolder = Root + "/Enemies";
        public const string BossesFolder = Root + "/Bosses";
        public const string ThemesFolder = Root + "/Themes";
        public const string FlameFolder = Root + "/Flame";

        public const string PlayerPath = Root + "/PlayerConfig.asset";
        public const string ArtStylePath = Root + "/ArtStyleConfig.asset";
        public const string CameraPath = Root + "/CameraConfig.asset";

        public static string EnemyPath(string name) => $"{EnemiesFolder}/Enemy_{name}.asset";
        public static string BossPath(string name) => $"{BossesFolder}/Boss_{name}.asset";
        public const string ResourcesFolder = "Assets/Resources";
        public const string FeelPath = ResourcesFolder + "/" + FeelConfig.ResourcePath + ".asset";

        public static string ThemePath(string name) => $"{ThemesFolder}/Theme_{name}.asset";
        public static string FlamePath(string name) => $"{FlameFolder}/Flame_{name}.asset";

        [MenuItem("Shatranj/Generate Config Assets", false, 10)]
        public static void Run()
        {
            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Generate Config Assets ===");
            int created = 0, skipped = 0;

            try
            {
                EnsureFolder(Root);
                EnsureFolder(EnemiesFolder);
                EnsureFolder(BossesFolder);
                EnsureFolder(ThemesFolder);
                EnsureFolder(FlameFolder);

                Create<PlayerConfig>(PlayerPath, null, ref created, ref skipped, report);
                Create<ArtStyleConfig>(ArtStylePath, null, ref created, ref skipped, report);
                Create<CameraConfig>(CameraPath, null, ref created, ref skipped, report);

                Create<EnemyConfig>(EnemyPath("Crawler"),    ConfigDefaults.Crawler,    ref created, ref skipped, report);
                Create<EnemyConfig>(EnemyPath("Lunger"),     ConfigDefaults.Lunger,     ref created, ref skipped, report);
                Create<EnemyConfig>(EnemyPath("ShadowKite"), ConfigDefaults.ShadowKite, ref created, ref skipped, report);
                Create<EnemyConfig>(EnemyPath("Knight"),     ConfigDefaults.Knight,     ref created, ref skipped, report);
                Create<EnemyConfig>(EnemyPath("Bat"),        ConfigDefaults.Bat,        ref created, ref skipped, report);
                Create<EnemyConfig>(EnemyPath("Charger"),    ConfigDefaults.Charger,    ref created, ref skipped, report);
                Create<EnemyConfig>(EnemyPath("Thrower"),    ConfigDefaults.Thrower,    ref created, ref skipped, report);

                Create<BossConfig>(BossPath("KageWarden"),   ConfigDefaults.KageWarden,   ref created, ref skipped, report);
                Create<BossConfig>(BossPath("KageCaller"),   ConfigDefaults.KageCaller,   ref created, ref skipped, report);
                Create<BossConfig>(BossPath("IronColossus"), ConfigDefaults.IronColossus, ref created, ref skipped, report);
                Create<BossConfig>(BossPath("Zulmat"),       ConfigDefaults.Zulmat,       ref created, ref skipped, report);

                Create<ThemeConfig>(ThemePath("Phase1_Beige"),    ConfigDefaults.ThemePhase1Beige,    ref created, ref skipped, report);
                Create<ThemeConfig>(ThemePath("Phase2_Inverted"), ConfigDefaults.ThemePhase2Inverted, ref created, ref skipped, report);
                Create<ThemeConfig>(ThemePath("Ending_Purple"),   ConfigDefaults.ThemeEndingPurple,   ref created, ref skipped, report);
                Create<ThemeConfig>(ThemePath("Cutscene"),        ConfigDefaults.ThemeCutscene,       ref created, ref skipped, report);

                Create<FlameConfig>(FlamePath("Phase1"), ConfigDefaults.FlamePhase1, ref created, ref skipped, report);
                Create<FlameConfig>(FlamePath("Phase2"), ConfigDefaults.FlamePhase2, ref created, ref skipped, report);

                // Prompt 17: game-feel presets, loaded by Feel from Resources (no scene references needed).
                EnsureFolder(ResourcesFolder);
                Create<FeelConfig>(FeelPath, null, ref created, ref skipped, report);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                report.AppendLine("FAILED: " + e);
                Debug.LogError(report.ToString());
                return;
            }

            report.Append("Done: ").Append(created).Append(" created, ").Append(skipped).AppendLine(" skipped (already exist).");
            Debug.Log(report.ToString());
        }

        private static void Create<T>(string path, Action<T> fill, ref int created, ref int skipped, StringBuilder report) where T : ScriptableObject
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
            {
                skipped++;
                report.Append("  skip   ").AppendLine(path);
                return;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            fill?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, path);
            created++;
            report.Append("  create ").AppendLine(path);
        }

        /// <summary>Creates "Assets/A/B/C" folder by folder through the AssetDatabase so .meta files are correct.</summary>
        public static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
