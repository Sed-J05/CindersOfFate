using System.Text;
using Shatranj.Core;
using Shatranj.Cutscenes;
using Shatranj.Data;
using Shatranj.UI;
using Shatranj.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Builds the two non-level scenes (Docs/02 §3): MainMenu (parchment stage, flame, idle protagonist puppet on a
    /// ledge, MainMenuController) and Cutscene (parchment stage, locked camera, CutsceneStage + CutsceneUI + CutsceneRunner).
    /// Menus: <c>Shatranj/Build Main Menu Scene</c>, <c>Shatranj/Build Cutscene Scene</c>; both run by Build All Scenes.
    /// </summary>
    public static class MenuSceneBuilder
    {
        public const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
        public const string CutscenePath = "Assets/Scenes/Cutscene.unity";

        [MenuItem("Shatranj/Build Main Menu Scene", false, 81)]
        public static void BuildMainMenuMenu() => Log(BuildMainMenu(new StringBuilder("=== Shatranj / Build Main Menu Scene ===\n")));

        [MenuItem("Shatranj/Build Cutscene Scene", false, 82)]
        public static void BuildCutsceneMenu() => Log(BuildCutscene(new StringBuilder("=== Shatranj / Build Cutscene Scene ===\n")));

        private static void Log(StringBuilder sb) => Debug.Log(sb.ToString());

        public static StringBuilder BuildMainMenu(StringBuilder report)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return report;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera cam = StageCamera(report);
            Material lit = SceneBuildUtil.SpriteLitMaterial();

            // Ledge + idle protagonist puppet (Docs/08 §7).
            var ledge = new GameObject("Ledge");
            ledge.transform.position = new Vector3(-5.5f, -4.4f, 0f);
            ledge.transform.localScale = new Vector3(6f, 1.4f, 1f);
            var ledgeSr = ledge.AddComponent<SpriteRenderer>();
            ledgeSr.sprite = SceneBuildUtil.GetOrCreateWhiteSprite();
            ledgeSr.color = new Color(0.149f, 0.149f, 0.165f);
            ledgeSr.sortingLayerName = GameLayers.Sorting.Tiles;
            if (lit != null) ledgeSr.sharedMaterial = lit;

            var puppet = new GameObject("ProtagonistPuppet");
            puppet.transform.position = new Vector3(-5.5f, -3.7f, 0f);
            var sr = puppet.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = GameLayers.Sorting.Player;
            if (lit != null) sr.sharedMaterial = lit;
            var set = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(Art.RebuildAnimations.SetPath("Protagonist"));
            SpriteClip idle = set != null ? set.Find("Idle") : null;
            sr.sprite = idle != null && idle.IsValid ? idle.frames[0] : null;
            var anim = puppet.AddComponent<SpriteAnimator>();
            if (set != null) SceneBuildUtil.SetReference(anim, "set", set);
            SceneBuildUtil.SetString(anim, "defaultClip", "Idle");
            puppet.AddComponent<PuppetShadow>();

            new GameObject("[MainMenu]").AddComponent<MainMenuController>();

            Save(scene, MainMenuPath, report);
            return report;
        }

        public static StringBuilder BuildCutscene(StringBuilder report)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return report;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            StageCamera(report);
            var runnerGo = new GameObject("[Cutscene]");
            runnerGo.transform.position = Vector3.zero;
            var stage = runnerGo.AddComponent<CutsceneStage>();
            SceneBuildUtil.SetReference(stage, "protagonistMaterial", PlayerFactory.GetOrCreateProtagonistMaterial());
            runnerGo.AddComponent<CutsceneUI>();
            runnerGo.AddComponent<CutsceneRunner>();

            Save(scene, CutscenePath, report);
            return report;
        }

        /// <summary>Camera at the origin (screen centre = stage origin), camera controller for shakes, full visual rig.</summary>
        private static Camera StageCamera(StringBuilder report)
        {
            var cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(GenerateConfigs.CameraPath);
            Camera cam = SceneBuildUtil.CreateMainCamera(new Vector3(0f, 0f, -10f), cameraConfig != null ? cameraConfig.orthographicSize : 5.5f, new Color(0.91f, 0.85f, 0.71f));
            var controller = cam.gameObject.AddComponent<CameraController>();
            if (cameraConfig != null) SceneBuildUtil.SetReference(controller, "config", cameraConfig);
            VisualRigFactory.Build(cam, report);
            return cam;
        }

        private static void Save(Scene scene, string path, StringBuilder report)
        {
            GenerateConfigs.EnsureFolder("Assets/Scenes");
            bool ok = EditorSceneManager.SaveScene(scene, path);
            if (ok) SceneBuildUtil.AddSceneToBuildSettings(path);
            report.AppendLine(ok ? "Saved " + path : "FAILED to save " + path);
        }
    }
}
