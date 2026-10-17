using System.Text;
using Shatranj.Core;
using Shatranj.EditorTools.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Build All Scenes</c>. Builds every scene and registers them in Build Settings in play order:
    /// MainMenu (first, so a build starts there), Cutscene, the phase scenes from LevelManifest.json (Phase1_Cave,
    /// Phase2_Castle), Sandbox; then opens MainMenu. Also refreshes the SpriteCatalog used by cutscenes and menus.
    /// </summary>
    public static class BuildScenes
    {
        [MenuItem("Shatranj/Build All Scenes", false, 80)]
        public static void Run()
        {
            if (!PlayModeMenuGuards.NotPlaying) { Debug.LogWarning("Build All Scenes: stop Play mode first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Build All Scenes ===");

            // Assets first (prefabs use temporary objects; scene builders load them after creating their scene).
            BuildPrefabs.EnsureLevelProps(false, report);
            BuildPrefabs.EnsureEnemyPrefabs(false, report);
            BuildPrefabs.EnsureBossPrefabs(false, report);
            if (AssetDatabase.LoadAssetAtPath<GameObject>(BuildUIPrefabs.HudPrefabPath) == null && BuildUIPrefabs.TmpEssentialsPresent())
                BuildUIPrefabs.Run();
            CutsceneCatalogBuilder.Build(report);

            MenuSceneBuilder.BuildMainMenu(report);
            MenuSceneBuilder.BuildCutscene(report);
            LevelBuilder.BuildLevelsMenu();
            report.AppendLine("Phase scenes: see the Build Levels summary above.");
            BuildSandboxScene.Run();
            report.AppendLine("Sandbox: rebuilt.");

            SceneBuildUtil.OrderBuildSettings(
                MenuSceneBuilder.MainMenuPath,
                MenuSceneBuilder.CutscenePath,
                $"Assets/Scenes/{GameConstants.ScenePhase1}.unity",
                $"Assets/Scenes/{GameConstants.ScenePhase2}.unity",
                BuildSandboxScene.ScenePath);
            report.AppendLine("Build Settings order: MainMenu, Cutscene, Phase1_Cave, Phase2_Castle, Sandbox (+ others).");

            EditorSceneManager.OpenScene(MenuSceneBuilder.MainMenuPath);
            report.AppendLine("Opened MainMenu — press Play for New Game → Intro → Phase 1.");
            Debug.Log(report.ToString());
        }
    }
}
