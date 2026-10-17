using System.Collections.Generic;
using System.IO;
using System.Text;
using Shatranj.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Build Windows Player</c> and <c>…(Development)</c> (Prompt 18, Docs/02 §10). Puts the game scenes
    /// in Build Settings in flow order (MainMenu, Cutscene, Phase1_Cave, Phase2_Castle; the Sandbox stays listed but
    /// unticked for release), applies the player defaults (product "Cinders of Fate" — D-179, windowed 1920×1080, resizable,
    /// pauses when unfocused) and runs BuildPipeline into &lt;project&gt;/Builds/Windows/Cinders of Fate.exe (Builds/ is git-ignored).
    /// The Development build adds the Sandbox and the DEVELOPMENT_BUILD debug hotkeys.
    /// </summary>
    public static class BuildPlayer
    {
        /// <summary>The game's title (D-179). "Shatranj" stays the internal code name (namespaces, menus).</summary>
        public const string ProductName = "Cinders of Fate";
        public const string CompanyName = "Team Cinders of Fate";
        public const string Version = "1.0.0";
        public const int DefaultWidth = 1920, DefaultHeight = 1080;
        public const string OutputFolder = "Builds/Windows";

        public static readonly string[] GameScenes =
        {
            MenuSceneBuilder.MainMenuPath,
            MenuSceneBuilder.CutscenePath,
            "Assets/Scenes/" + GameConstants.ScenePhase1 + ".unity",
            "Assets/Scenes/" + GameConstants.ScenePhase2 + ".unity",
        };

        [MenuItem("Shatranj/Build Windows Player", false, 90)]
        public static void BuildRelease() => Build(development: false);

        [MenuItem("Shatranj/Build Windows Player (Development)", false, 91)]
        public static void BuildDevelopment() => Build(development: true);

        public static BuildReport Build(bool development)
        {
            var log = new StringBuilder($"=== Shatranj / Build Windows Player{(development ? " (Development)" : "")} ===\n");
            if (!PlayModeMenuGuards.NotPlaying) { Debug.LogWarning("Build Windows Player: stop Play mode first."); return null; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return null;

            List<string> missing = GameScenes.FindAllMissing();
            if (missing.Count > 0)
            {
                Debug.LogError(log.Append("Missing scenes (run Shatranj → Build All Scenes first): ").Append(string.Join(", ", missing)).ToString());
                return null;
            }

            ApplyBuildSettingsScenes(development, log);
            ApplyPlayerSettings(log);

            var scenes = new List<string>(GameScenes);
            if (development && File.Exists(BuildSandboxScene.ScenePath)) scenes.Add(BuildSandboxScene.ScenePath);
            string folder = development ? OutputFolder + "_Dev" : OutputFolder;
            Directory.CreateDirectory(folder);
            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = Path.Combine(folder, ProductName + ".exe"),
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = development ? BuildOptions.Development : BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary s = report.summary;
            log.AppendLine($"Result: {s.result} · {s.totalErrors} error(s), {s.totalWarnings} warning(s) · {s.totalSize / (1024f * 1024f):0.0} MB · {s.totalTime.TotalSeconds:0} s");
            log.AppendLine($"Output: {Path.GetFullPath(options.locationPathName)}");
            if (s.result == BuildResult.Succeeded) Debug.Log(log.ToString());
            else Debug.LogError(log.Append("If Windows Build Support (IL2CPP/Mono) is missing, add it in Unity Hub → Installs → Add modules.").ToString());
            return report;
        }

        /// <summary>Flow order first; any other scene (Sandbox, LevelTest) stays listed, ticked only for Development.</summary>
        public static void ApplyBuildSettingsScenes(bool development, StringBuilder log = null)
        {
            var result = new List<EditorBuildSettingsScene>();
            var seen = new HashSet<string>();
            foreach (string p in GameScenes)
                if (File.Exists(p) && seen.Add(p)) result.Add(new EditorBuildSettingsScene(p, true));
            foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
                if (seen.Add(s.path) && File.Exists(s.path)) result.Add(new EditorBuildSettingsScene(s.path, development && s.path == BuildSandboxScene.ScenePath));
            EditorBuildSettings.scenes = result.ToArray();
            log?.AppendLine($"Build Settings: {result.Count} scene(s), first = {result[0].path}");
        }

        public static void ApplyPlayerSettings(StringBuilder log = null)
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.defaultScreenWidth = DefaultWidth;
            PlayerSettings.defaultScreenHeight = DefaultHeight;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.allowFullscreenSwitch = true;
            PlayerSettings.runInBackground = false;      // with GameManager's focus-loss pause: alt-tab never plays on alone
            PlayerSettings.visibleInBackground = false;
            PlayerSettings.forceSingleInstance = true;
            log?.AppendLine($"Player: {ProductName} {Version}, windowed {DefaultWidth}×{DefaultHeight}, resizable, pauses when unfocused.");
        }

        private static List<string> FindAllMissing(this string[] paths)
        {
            var missing = new List<string>();
            foreach (string p in paths) if (!File.Exists(p)) missing.Add(p);
            return missing;
        }
    }
}
