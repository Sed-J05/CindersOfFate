using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Shatranj.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Build Windows Player</c>, <c>Build macOS Player</c>, their Development variants, and
    /// <c>Build for Submission (Windows + macOS)</c> (Prompt 18, D-187). Puts the game scenes in Build Settings in flow
    /// order (MainMenu, Cutscene, Phase1_Cave, Phase2_Castle; the Sandbox stays listed but unticked for release), applies
    /// the player defaults (product "Cinders of Fate" — D-179, windowed 1920×1080, resizable, pauses when unfocused) and
    /// builds into &lt;project&gt;/Builds/&lt;platform&gt; (git-ignored). Release builds use LZ4HC compression. The submission
    /// build also zips both players into &lt;project&gt;/Release/ (committed), with the Mac binary marked executable.
    /// </summary>
    public static class BuildPlayer
    {
        /// <summary>The game's title (D-179). "Shatranj" stays the internal code name (namespaces, menus).</summary>
        public const string ProductName = "Cinders of Fate";
        public const string CompanyName = "Team Cinders of Fate";
        public const string Version = "1.0.0";
        public const int DefaultWidth = 1920, DefaultHeight = 1080;
        public const string OutputFolder = "Builds/Windows";
        public const string MacOutputFolder = "Builds/macOS";
        public const string ReleaseFolder = "Release";
        public const string WindowsZip = "CindersOfFate_Windows.zip";
        public const string MacZip = "CindersOfFate_macOS.zip";

        /// <summary>GitHub rejects files over 100 MB; warn a little before that.</summary>
        private const long GitHubFileLimitWarn = 95L * 1024 * 1024;

        public static readonly string[] GameScenes =
        {
            MenuSceneBuilder.MainMenuPath,
            MenuSceneBuilder.CutscenePath,
            "Assets/Scenes/" + GameConstants.ScenePhase1 + ".unity",
            "Assets/Scenes/" + GameConstants.ScenePhase2 + ".unity",
        };

        [MenuItem("Shatranj/Build Windows Player", false, 90)]
        public static void BuildRelease() => Build(BuildTarget.StandaloneWindows64, development: false);

        [MenuItem("Shatranj/Build Windows Player (Development)", false, 91)]
        public static void BuildDevelopment() => Build(BuildTarget.StandaloneWindows64, development: true);

        [MenuItem("Shatranj/Build macOS Player", false, 92)]
        public static void BuildMacRelease() => Build(BuildTarget.StandaloneOSX, development: false);

        [MenuItem("Shatranj/Build for Submission (Windows + macOS)", false, 93)]
        public static void BuildSubmission()
        {
            var log = new StringBuilder("=== Shatranj / Build for Submission ===\n");
            BuildReport win = Build(BuildTarget.StandaloneWindows64, development: false);
            if (win == null || win.summary.result != BuildResult.Succeeded) { Debug.LogError(log.Append("Windows build failed — see above.").ToString()); return; }
            log.AppendLine(Zip(OutputFolder, Path.Combine(ReleaseFolder, WindowsZip), "Cinders of Fate", null, WindowsReadme));

            if (!MacSupportInstalled())
            {
                Debug.LogWarning(log.Append(MacModuleHelp).ToString());
                return;
            }
            BuildReport mac = Build(BuildTarget.StandaloneOSX, development: false);
            if (mac == null || mac.summary.result != BuildResult.Succeeded) { Debug.LogError(log.Append("macOS build failed — see above.").ToString()); return; }
            log.AppendLine(Zip(MacOutputFolder, Path.Combine(ReleaseFolder, MacZip), "", ProductName + ".app/Contents/MacOS/", MacReadme));
            log.AppendLine($"Done. Commit the {ReleaseFolder}/ folder (git add {ReleaseFolder}) and push.");
            Debug.Log(log.ToString());
            EditorUtility.RevealInFinder(Path.GetFullPath(Path.Combine(ReleaseFolder, WindowsZip)));
        }

        public static bool MacSupportInstalled() => BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);

        private const string MacModuleHelp =
            "macOS build skipped: Mac Build Support is not installed. Unity Hub → Installs → 6000.0.32f1 → ⚙ → Add modules → " +
            "tick \"Mac Build Support (Mono)\" → Install, restart Unity, then run Build for Submission again.";

        public static BuildReport Build(BuildTarget target, bool development)
        {
            bool mac = target == BuildTarget.StandaloneOSX;
            string platform = mac ? "macOS" : "Windows";
            var log = new StringBuilder($"=== Shatranj / Build {platform} Player{(development ? " (Development)" : "")} ===\n");
            if (!PlayModeMenuGuards.NotPlaying) { Debug.LogWarning($"Build {platform} Player: stop Play mode first."); return null; }
            if (mac && !MacSupportInstalled()) { Debug.LogError(MacModuleHelp); return null; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return null;

            List<string> missing = GameScenes.FindAllMissing();
            if (missing.Count > 0)
            {
                Debug.LogError(log.Append("Missing scenes (run Shatranj → Build All Scenes first): ").Append(string.Join(", ", missing)).ToString());
                return null;
            }

            ApplyBuildSettingsScenes(development, log);
            ApplyPlayerSettings(log);
            if (mac)
            {
                // Universal binary: runs natively on Intel and Apple silicon Macs.
                EditorUserBuildSettings.SetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneOSX), "Architecture", "x64ARM64");
            }

            var scenes = new List<string>(GameScenes);
            if (development && File.Exists(BuildSandboxScene.ScenePath)) scenes.Add(BuildSandboxScene.ScenePath);
            string folder = (mac ? MacOutputFolder : OutputFolder) + (development ? "_Dev" : "");
            if (Directory.Exists(folder)) Directory.Delete(folder, true);   // no stale files from older builds in the zip
            Directory.CreateDirectory(folder);
            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = Path.Combine(folder, ProductName + (mac ? ".app" : ".exe")),
                target = target,
                targetGroup = BuildTargetGroup.Standalone,
                options = development ? BuildOptions.Development : BuildOptions.CompressWithLz4HC,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary s = report.summary;
            log.AppendLine($"Result: {s.result} · {s.totalErrors} error(s), {s.totalWarnings} warning(s) · {s.totalSize / (1024f * 1024f):0.0} MB · {s.totalTime.TotalSeconds:0} s");
            log.AppendLine($"Output: {Path.GetFullPath(options.locationPathName)}");
            if (s.result == BuildResult.Succeeded) Debug.Log(log.ToString());
            else Debug.LogError(log.Append($"If {platform} Build Support is missing, add it in Unity Hub → Installs → Add modules.").ToString());
            return report;
        }

        /// <summary>
        /// Zips a build folder. Debug-symbol folders ("…DoNotShip") are left out. Files under <paramref name="executablePrefix"/>
        /// get Unix mode 755 so the Mac app still launches after unzipping (a zip made on Windows otherwise drops the +x bit).
        /// A short how-to-run text goes in the zip root.
        /// </summary>
        public static string Zip(string sourceFolder, string zipPath, string rootName, string executablePrefix, string readme)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(zipPath));
            if (File.Exists(zipPath)) File.Delete(zipPath);
            string root = Path.GetFullPath(sourceFolder).Replace('\\', '/').TrimEnd('/') + "/";
            string prefix = string.IsNullOrEmpty(rootName) ? "" : rootName.TrimEnd('/') + "/";
            int count = 0;
            using (FileStream fs = File.Create(zipPath))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                {
                    string rel = file.Replace('\\', '/').Substring(root.Length);
                    if (rel.Contains("DoNotShip")) continue;
                    ZipArchiveEntry entry = zip.CreateEntryFromFile(file, prefix + rel, System.IO.Compression.CompressionLevel.Optimal);
                    bool exec = executablePrefix != null && rel.StartsWith(executablePrefix);
                    entry.ExternalAttributes = ((exec ? 0x81ED : 0x81A4) << 16);   // regular file, 755 or 644
                    count++;
                }
                ZipArchiveEntry info = zip.CreateEntry(prefix + "HOW TO RUN.txt");
                info.ExternalAttributes = 0x81A4 << 16;
                using (var w = new StreamWriter(info.Open())) w.Write(readme);
            }
            long size = new FileInfo(zipPath).Length;
            string line = $"Zipped {count} files → {Path.GetFullPath(zipPath)} ({size / (1024f * 1024f):0.0} MB)";
            if (size > GitHubFileLimitWarn)
                line += " — WARNING: over GitHub's 100 MB file limit. Upload this zip as a GitHub Release asset instead of committing it.";
            return line;
        }

        private const string WindowsReadme =
            "Cinders of Fate (Windows)\r\n\r\n1. Unzip the whole folder (don't run it from inside the zip).\r\n" +
            "2. Double-click \"Cinders of Fate.exe\".\r\n3. If Windows SmartScreen says \"Windows protected your PC\", click More info → Run anyway " +
            "(the game isn't code-signed).\r\n\r\nControls: A/D or arrows move · Space jump · J attack (hold W to aim up) · hold K heal, tap K fireball (Phase 2) · L dash (Phase 2) · W interact · Esc pause. Gamepads work too.\r\n";

        private const string MacReadme =
            "Cinders of Fate (macOS, Intel + Apple silicon)\n\n1. Unzip, then move \"Cinders of Fate.app\" to Applications (or leave it in the folder).\n" +
            "2. The game isn't signed by Apple, so the first launch needs one of these:\n" +
            "   a) Right-click (Control-click) the app → Open → Open, or\n" +
            "   b) If macOS says the app \"is damaged\", open Terminal and run:\n" +
            "      xattr -cr \"/path/to/Cinders of Fate.app\"\n      (drag the app onto the Terminal window to fill in the path), then open it again.\n\n" +
            "Controls: A/D or arrows move · Space jump · J attack (hold W to aim up) · hold K heal, tap K fireball (Phase 2) · L dash (Phase 2) · W interact · Esc pause. Gamepads work too.\n";

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
