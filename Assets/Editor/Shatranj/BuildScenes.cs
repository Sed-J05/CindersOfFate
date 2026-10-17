using System.Text;
using Shatranj.EditorTools.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Build All Scenes</c>. Builds every scene that can be built so far and registers it in
    /// Build Settings: the phase scenes from LevelManifest.json (Phase1_Cave now; Phase2_Castle once its rooms
    /// exist) and the Sandbox. MainMenu and Cutscene arrive in Prompt 11. Re-run any time.
    /// </summary>
    public static class BuildScenes
    {
        [MenuItem("Shatranj/Build All Scenes", false, 80)]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Build All Scenes ===");

            // Prefabs first (they use temporary objects; scene builders load them after creating their scene).
            BuildPrefabs.EnsureLevelProps(false, report);
            BuildPrefabs.EnsureEnemyPrefabs(false, report);
            BuildPrefabs.EnsureBossPrefabs(false, report);
            if (AssetDatabase.LoadAssetAtPath<GameObject>(BuildUIPrefabs.HudPrefabPath) == null && BuildUIPrefabs.TmpEssentialsPresent())
                BuildUIPrefabs.Run();

            LevelBuilder.BuildLevelsMenu();
            report.AppendLine("Phase scenes: see the Build Levels summary above.");

            BuildSandboxScene.Run();
            report.AppendLine("Sandbox: rebuilt.");

            report.AppendLine("MainMenu, Cutscene: PENDING (Prompt 11).");
            report.AppendLine("Opening Phase1_Cave.");
            if (System.IO.File.Exists("Assets/Scenes/Phase1_Cave.unity"))
                EditorSceneManager.OpenScene("Assets/Scenes/Phase1_Cave.unity");
            Debug.Log(report.ToString());
        }
    }
}
