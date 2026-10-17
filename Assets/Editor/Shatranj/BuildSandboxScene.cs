using System.Text;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Level;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Build Sandbox Scene</c>. Rebuilds Assets/Scenes/Sandbox.unity from scratch:
    /// floor, walls, steps, a pit, a wall-stick pillar, a low ceiling, stacked one-way platforms, spawn point,
    /// camera rig with RoomBounds, code-built player and a debug HUD. Registers the scene in Build Settings.
    /// The scene is generated, so re-running overwrites it (Docs/02 §13).
    /// </summary>
    public static class BuildSandboxScene
    {
        public const string ScenePath = "Assets/Scenes/Sandbox.unity";

        private static readonly Color SolidColor = new Color(0.149f, 0.149f, 0.165f);   // #26262A
        private static readonly Color StepColor = new Color(0.29f, 0.29f, 0.32f);       // #4A4A52
        private static readonly Color OneWayColor = new Color(0.42f, 0.23f, 0.23f);     // #6B3A3A
        private static readonly Color ParchmentColor = new Color(0.91f, 0.85f, 0.71f);  // #E8D9B5
        private static readonly Color EnemyBodyColor = new Color(0.086f, 0.035f, 0.039f); // #16090A
        private static readonly Color EnemyRedColor = new Color(0.757f, 0.153f, 0.176f); // #C1272D
        private static readonly Color SpikeColor = new Color(0.64f, 0.23f, 0.23f);       // muted red, reads as danger

        [MenuItem("Shatranj/Build Sandbox Scene", false, 20)]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Build Sandbox Scene ===");

            // Prefab building uses temporary objects, so it happens before the new scene exists.
            BuildPrefabs.EnsureEnemyPrefabs(false, report);
            BuildPrefabs.EnsureBossPrefabs(false, report);

            // Create the scene FIRST: NewScene(Single) unloads unused assets, which would turn config
            // objects loaded before this call into dead references (saved as null). See D-044.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayerConfig playerConfig = AssetDatabase.LoadAssetAtPath<PlayerConfig>(GenerateConfigs.PlayerPath);
            CameraConfig cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(GenerateConfigs.CameraPath);
            if (playerConfig == null || cameraConfig == null)
            {
                GenerateConfigs.Run();
                playerConfig = AssetDatabase.LoadAssetAtPath<PlayerConfig>(GenerateConfigs.PlayerPath);
                cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(GenerateConfigs.CameraPath);
                report.AppendLine("Configs were missing → generated.");
            }

            // --- Environment ---
            var env = new GameObject("Environment");
            int boxes = BuildGeometry(env.transform);
            report.Append("Geometry: ").Append(boxes).AppendLine(" boxes");

            // --- Room bounds (covers the whole sandbox) ---
            var roomGo = new GameObject("Room_Sandbox");
            roomGo.layer = GameLayers.RoomBounds;
            roomGo.transform.position = new Vector3(15f, 6f, 0f);
            roomGo.AddComponent<BoxCollider2D>();
            var room = roomGo.AddComponent<RoomBounds>();
            room.Configure("SANDBOX", new Vector2(54f, 22f));

            // --- Spawn + player ---
            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.position = new Vector3(0f, 0f, 0f);
            GameObject player = PlayerFactory.Build(playerConfig, spawn.transform.position, spawn.transform);
            var controller = player.GetComponent<PlayerController>();

            // --- Combat targets + hazards (Prompt 3) ---
            var targets = new GameObject("Targets").transform;
            SceneBuildUtil.CreateDummy("Dummy_Ground", new Vector2(14f, 0f), 10, new Vector2(0.9f, 1.0f), EnemyBodyColor, EnemyRedColor).transform.SetParent(targets, true);
            SceneBuildUtil.CreateDummy("Dummy_Platform", new Vector2(20f, 4.65f), 10, new Vector2(0.9f, 1.0f), EnemyBodyColor, EnemyRedColor).transform.SetParent(targets, true);
            SceneBuildUtil.CreateDummy("Dummy_Overhead", new Vector2(3f, 3.7f), 10, new Vector2(1.0f, 0.8f), EnemyBodyColor, EnemyRedColor).transform.SetParent(targets, true);
            SceneBuildUtil.CreateDummy("Dummy_Tall", new Vector2(26f, 0f), 20, new Vector2(1.6f, 2.4f), EnemyBodyColor, EnemyRedColor).transform.SetParent(targets, true);

            SceneBuildUtil.CreateSpikes("Spikes_Floor", env.transform, new Vector2(5f, 0.2f), new Vector2(2f, 0.4f), SpikeColor);
            SceneBuildUtil.CreateSpikes("Spikes_Pit", env.transform, new Vector2(32.5f, -2.8f), new Vector2(3f, 0.4f), SpikeColor);
            report.AppendLine("Targets: 4 dummies (10/10/10/20 HP). Hazards: floor spikes at x≈5, pit spikes at x≈32.5.");

            // --- Enemy spawn keys (Prompt 7/12) at the mouse cursor: 1 Crawler, 2 Lunger, 3 Kite, 4 Kage Warden,
            //     5 Knight, 6 Bat, 7 Charger, 8 Thrower; Shift+1 Kage Warden, Shift+2 Kage Caller; 0 clear ---
            string[] regularKeys = { "Crawler", "Lunger", "ShadowKite", null, "Knight", "Bat", "Charger", "Thrower" };
            var enemyPrefabs = new Object[regularKeys.Length];
            for (int i = 0; i < regularKeys.Length; i++)
                enemyPrefabs[i] = regularKeys[i] != null
                    ? AssetDatabase.LoadAssetAtPath<GameObject>(BuildPrefabs.EnemyPrefabPath(regularKeys[i]))
                    : AssetDatabase.LoadAssetAtPath<GameObject>(BuildPrefabs.BossPrefabPath("KageWarden"));
            var bossPrefabs = new Object[BuildPrefabs.BossTypes.Length];
            for (int i = 0; i < BuildPrefabs.BossTypes.Length; i++)
                bossPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(BuildPrefabs.BossPrefabPath(BuildPrefabs.BossTypes[i].name));
            var spawnerGo = new GameObject("[SandboxEnemySpawner]");
            var sandboxSpawner = spawnerGo.AddComponent<Shatranj.Enemies.SandboxEnemySpawner>();
            SceneBuildUtil.SetObjectArray(sandboxSpawner, "prefabs", enemyPrefabs);
            SceneBuildUtil.SetObjectArray(sandboxSpawner, "shiftPrefabs", bossPrefabs);
            int missing = 0;
            foreach (Object o in enemyPrefabs) if (o == null) missing++;
            foreach (Object o in bossPrefabs) if (o == null) missing++;
            report.AppendLine("Enemy keys: 1 Crawler, 2 Lunger, 3 Kite, 4 Kage Warden, 5 Knight, 6 Bat, 7 Charger, 8 Thrower; "
                              + "Shift+1 Kage Warden, Shift+2 Kage Caller, Shift+3 Iron Colossus; 0 clear." + (missing > 0 ? $"  WARNING: {missing} prefab(s) missing" : ""));

            // --- F12 flame-scale slider (Prompt 12) ---
            new GameObject("[FlameScaleDebugPanel]").AddComponent<Shatranj.Visuals.FlameScaleDebugPanel>();

            // --- Camera rig ---
            Camera cam = SceneBuildUtil.CreateMainCamera(new Vector3(0f, cameraConfig.verticalOffset, -10f), cameraConfig.orthographicSize, ParchmentColor);
            var camController = cam.gameObject.AddComponent<CameraController>();
            SceneBuildUtil.SetReference(camController, "config", cameraConfig);
            SceneBuildUtil.SetReference(camController, "targetBehaviour", controller);
            SceneBuildUtil.SetReference(camController, "initialRoom", room);

            VisualRigFactory.Build(cam, report);   // Prompt 10: parchment, flame, lights, PostFX, themes, wipes

            // --- Game HUD prefab (Prompt 5) ---
            GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BuildUIPrefabs.HudPrefabPath);
            if (hudPrefab == null && BuildUIPrefabs.TmpEssentialsPresent())
            {
                BuildUIPrefabs.Run();
                hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BuildUIPrefabs.HudPrefabPath);
            }
            if (hudPrefab != null)
            {
                var hudInstance = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, scene);
                hudInstance.name = "HUD";
                report.AppendLine("HUD: instantiated from " + BuildUIPrefabs.HudPrefabPath + " (F10 = HUD debug panel).");
            }
            else
            {
                report.AppendLine("HUD: skipped — no HUD prefab (import TMP Essentials, run Shatranj → Build UI Prefabs, then rebuild the sandbox).");
            }

            // --- Bootstrap + debug HUD ---
            new GameObject("[SceneBootstrap]").AddComponent<SceneBootstrap>();

            UnityEngine.UI.Text text = SceneBuildUtil.CreateDebugTextCanvas("DebugCanvas", out Canvas canvas);
            var hud = canvas.gameObject.AddComponent<PlayerDebugHud>();
            SceneBuildUtil.SetReference(hud, "player", controller);
            SceneBuildUtil.SetReference(hud, "text", text);

            // --- Save + register ---
            GenerateConfigs.EnsureFolder("Assets/Scenes");
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            SceneBuildUtil.AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();

            report.AppendLine(saved ? $"Saved {ScenePath} and added it to Build Settings." : $"FAILED to save {ScenePath}");
            report.AppendLine("Player: capsule 0.7×1.5, zero-friction, PlayerInput/Motor/Controller/Animator wired.");
            bool playerCfgOk = new SerializedObject(controller).FindProperty("config").objectReferenceValue != null;
            bool camCfgOk = new SerializedObject(camController).FindProperty("config").objectReferenceValue != null;
            report.Append("Config references: PlayerConfig ").Append(playerCfgOk ? "OK" : "MISSING")
                  .Append(", CameraConfig ").AppendLine(camCfgOk ? "OK" : "MISSING");
            if (!playerCfgOk || !camCfgOk) saved = false;
            report.AppendLine("Press Play: A/D move, Space jump, J attack (W+J up), K heal/fireball, L dash, F4 phase toggle, F7 gizmos, F9 reload.");
            Selection.activeGameObject = player;

            if (saved) Debug.Log(report.ToString()); else Debug.LogError(report.ToString());
        }

        /// <summary>Layout in world units. Floor top is y = 0. Returns the number of boxes created.</summary>
        private static int BuildGeometry(Transform parent)
        {
            int count = 0;

            // Floor in two parts with a pit between x = 31 and x = 34.
            Solid(parent, "Floor_A", new Vector2(10f, -0.5f), new Vector2(42f, 1f), SolidColor, ref count);
            Solid(parent, "Floor_B", new Vector2(37.5f, -0.5f), new Vector2(7f, 1f), SolidColor, ref count);
            Solid(parent, "Pit_Floor", new Vector2(32.5f, -3.5f), new Vector2(3f, 1f), SolidColor, ref count);
            Solid(parent, "Pit_Wall_L", new Vector2(30.5f, -2f), new Vector2(1f, 3f), SolidColor, ref count);
            Solid(parent, "Pit_Wall_R", new Vector2(34.5f, -2f), new Vector2(1f, 3f), SolidColor, ref count);

            // Outer walls.
            Solid(parent, "Wall_Left", new Vector2(-11.5f, 6f), new Vector2(1f, 16f), SolidColor, ref count);
            Solid(parent, "Wall_Right", new Vector2(41.5f, 6f), new Vector2(1f, 16f), SolidColor, ref count);

            // Steps: 0.5 / 1.0 / 1.5 / 2.0 u (1 / 2 / 3 / 4 tiles).
            for (int i = 0; i < 4; i++)
            {
                float h = 0.5f * (i + 1);
                Solid(parent, $"Step_{i + 1}", new Vector2(8.5f + i, h * 0.5f), new Vector2(1f, h), StepColor, ref count);
            }

            // Wall-stick test pillar (6 u tall) and a 2.5 u ledge on top of the step stack side.
            Solid(parent, "Pillar", new Vector2(28f, 3f), new Vector2(1f, 6f), SolidColor, ref count);
            Solid(parent, "Ledge_High", new Vector2(24f, 2.5f), new Vector2(3f, 0.5f), StepColor, ref count);

            // Low ceiling corridor: 3 u clearance (Docs/05 min corridor height P1).
            Solid(parent, "Ceiling_Low", new Vector2(37.5f, 3.5f), new Vector2(7f, 1f), SolidColor, ref count);

            // One-way platforms near spawn and a climbable stack (≤ 2.5 u apart, Docs/05 §2).
            OneWay(parent, "OneWay_Spawn", new Vector2(3f, 2f), new Vector2(3f, 0.3f), ref count);
            OneWay(parent, "OneWay_1", new Vector2(16f, 2.5f), new Vector2(4f, 0.3f), ref count);
            OneWay(parent, "OneWay_2", new Vector2(20f, 4.5f), new Vector2(4f, 0.3f), ref count);
            OneWay(parent, "OneWay_3", new Vector2(16f, 6.5f), new Vector2(4f, 0.3f), ref count);
            OneWay(parent, "OneWay_4", new Vector2(20f, 8.5f), new Vector2(4f, 0.3f), ref count);

            // Gap test: two pillars 4 u apart (safe max gap without dash) at the far left.
            Solid(parent, "Gap_L", new Vector2(-6f, 1f), new Vector2(2f, 2f), StepColor, ref count);
            Solid(parent, "Gap_R", new Vector2(-1.0f, 1f), new Vector2(2f, 2f), StepColor, ref count);

            return count;
        }

        private static void Solid(Transform parent, string name, Vector2 center, Vector2 size, Color color, ref int count)
        {
            SceneBuildUtil.CreateBox(name, parent, center, size, color, GameLayers.Ground, GameLayers.Sorting.Tiles);
            count++;
        }

        private static void OneWay(Transform parent, string name, Vector2 center, Vector2 size, ref int count)
        {
            SceneBuildUtil.CreateBox(name, parent, center, size, OneWayColor, GameLayers.OneWay, GameLayers.Sorting.Tiles, oneWay: true);
            count++;
        }
    }
}
