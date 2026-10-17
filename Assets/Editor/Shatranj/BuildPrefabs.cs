using System.Text;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Level;
using Shatranj.Visuals;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Build Prefabs</c>. Creates / refreshes the level prop prefabs (Bench, Door, BossGate,
    /// Orb, TutorialSign) under Assets/Prefabs/Level. Prompt 7 adds enemies, Prompt 8+ bosses and projectiles.
    /// Prefabs are saved in place so their GUIDs and scene references survive.
    /// </summary>
    public static class BuildPrefabs
    {
        public const string LevelFolder = "Assets/Prefabs/Level";
        public const string EnemiesFolder = "Assets/Prefabs/Enemies";
        public const string BossesFolder = "Assets/Prefabs/Bosses";

        public static readonly string[] LevelPropNames = { "Bench", "Door", "BossGate", "Orb", "TutorialSign" };

        public static string LevelPrefabPath(string name) => $"{LevelFolder}/{name}.prefab";
        public static string EnemyPrefabPath(string name) => $"{EnemiesFolder}/{name}.prefab";
        public static string BossPrefabPath(string name) => $"{BossesFolder}/{name}.prefab";

        [MenuItem("Shatranj/Build Prefabs", false, 50)]
        public static void Run()
        {
            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Build Prefabs ===");
            EnsureLevelProps(force: true, report);
            EnsureEnemyPrefabs(force: true, report);
            EnsureBossPrefabs(force: true, report);
            AssetDatabase.SaveAssets();
            Debug.Log(report.ToString());
        }

        /// <summary>(prefab / set name, BossConfig asset name, boss id, behaviour) for every boss built so far.</summary>
        public static readonly (string name, string config, string bossId, System.Type behaviour)[] BossTypes =
        {
            ("KageWarden", "KageWarden", "kage_warden", typeof(Shatranj.Bosses.KageWarden)),
            ("KageCaller", "KageCaller", "kage_caller", typeof(Shatranj.Bosses.KageCaller)),
        };

        /// <summary>Prefab path for a boss id ("kage_warden" → Assets/Prefabs/Bosses/KageWarden.prefab), or null if not built yet.</summary>
        public static string BossPrefabPathForId(string bossId)
        {
            foreach (var b in BossTypes) if (b.bossId == bossId) return BossPrefabPath(b.name);
            return null;
        }

        /// <summary>Builds the boss prefabs if any is missing (or always when <paramref name="force"/>). Call before NewScene (D-044).</summary>
        public static void EnsureBossPrefabs(bool force = false, StringBuilder report = null)
        {
            bool allExist = true;
            foreach (var b in BossTypes)
                if (AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath(b.name)) == null) { allExist = false; break; }
            if (allExist && !force) return;

            GenerateConfigs.EnsureFolder(BossesFolder);
            foreach (var b in BossTypes)
            {
                var cfg = AssetDatabase.LoadAssetAtPath<BossConfig>(GenerateConfigs.BossPath(b.config));
                if (cfg == null)
                {
                    GenerateConfigs.Run();
                    cfg = AssetDatabase.LoadAssetAtPath<BossConfig>(GenerateConfigs.BossPath(b.config));
                }
                if (cfg == null) { report?.AppendLine($"  FAILED {b.name}: no BossConfig at {GenerateConfigs.BossPath(b.config)}"); continue; }

                GameObject root = BossPrefabFactory.Build(b.name, cfg, b.behaviour);
                if (b.behaviour == typeof(Shatranj.Bosses.KageCaller))
                {
                    // Minions: Shadow Kites only (D-126; Docs/04 §B2 had Crawler / Lunger). Enemy prefabs are built first.
                    var minions = new Object[]
                    {
                        AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath("ShadowKite")),
                    };
                    SceneBuildUtil.SetObjectArray(root.GetComponent<Shatranj.Bosses.KageCaller>(), "minionPrefabs", minions);
                }
                string path = BossPrefabPath(b.name);
                PrefabUtility.SaveAsPrefabAsset(root, path, out bool ok);
                Object.DestroyImmediate(root);
                report?.AppendLine((ok ? "  saved  " : "  FAILED ") + path + $"  (HP {cfg.maxHp})");
            }
        }

        /// <summary>(prefab / set name, config name, behaviour) for every regular enemy built so far.</summary>
        public static readonly (string name, string config, System.Type behaviour)[] EnemyTypes =
        {
            ("Crawler", "Crawler", typeof(Shatranj.Enemies.Crawler)),
            ("Lunger", "Lunger", typeof(Shatranj.Enemies.Lunger)),
            ("ShadowKite", "ShadowKite", typeof(Shatranj.Enemies.ShadowKite)),
            ("Knight", "Knight", typeof(Shatranj.Enemies.Knight)),
            ("Bat", "Bat", typeof(Shatranj.Enemies.Bat)),
            ("Charger", "Charger", typeof(Shatranj.Enemies.Charger)),
            ("Thrower", "Thrower", typeof(Shatranj.Enemies.Thrower)),
        };

        /// <summary>Builds the enemy prefabs if any is missing (or always when <paramref name="force"/>). Call before NewScene (D-044).</summary>
        public static void EnsureEnemyPrefabs(bool force = false, StringBuilder report = null)
        {
            bool allExist = true;
            foreach (var e in EnemyTypes)
                if (AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath(e.name)) == null) { allExist = false; break; }
            if (allExist && !force) return;

            GenerateConfigs.EnsureFolder(EnemiesFolder);
            foreach (var e in EnemyTypes)
            {
                var cfg = AssetDatabase.LoadAssetAtPath<EnemyConfig>(GenerateConfigs.EnemyPath(e.config));
                if (cfg == null)
                {
                    GenerateConfigs.Run();
                    cfg = AssetDatabase.LoadAssetAtPath<EnemyConfig>(GenerateConfigs.EnemyPath(e.config));
                }
                if (cfg == null) { report?.AppendLine($"  FAILED {e.name}: no EnemyConfig at {GenerateConfigs.EnemyPath(e.config)}"); continue; }

                GameObject root = EnemyPrefabFactory.Build(e.name, cfg, e.behaviour);
                string path = EnemyPrefabPath(e.name);
                PrefabUtility.SaveAsPrefabAsset(root, path, out bool ok);
                Object.DestroyImmediate(root);
                report?.AppendLine((ok ? "  saved  " : "  FAILED ") + path + $"  (HP {cfg.maxHp}, {cfg.HitsToKill(1)} nail hits)");
            }
        }

        /// <summary>Builds the level props if any is missing (or always when <paramref name="force"/>).</summary>
        public static void EnsureLevelProps(bool force = false, StringBuilder report = null)
        {
            bool allExist = true;
            foreach (string n in LevelPropNames)
                if (AssetDatabase.LoadAssetAtPath<GameObject>(LevelPrefabPath(n)) == null) { allExist = false; break; }
            if (allExist && !force) return;

            GenerateConfigs.EnsureFolder(LevelFolder);
            if (AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(Art.RebuildAnimations.SetPath("Bench")) == null)
                report?.AppendLine("  (prop animation sets not built yet — prefabs use the first placeholder frame; run Import & Rebuild Animations, then Build Prefabs)");

            Save(BuildBench(), "Bench", report);
            Save(BuildDoor(), "Door", report);
            Save(BuildGate(), "BossGate", report);
            Save(BuildOrb(), "Orb", report);
            Save(BuildSign(), "TutorialSign", report);
        }

        private static void Save(GameObject root, string name, StringBuilder report)
        {
            string path = LevelPrefabPath(name);
            PrefabUtility.SaveAsPrefabAsset(root, path, out bool ok);
            Object.DestroyImmediate(root);
            report?.AppendLine((ok ? "  saved  " : "  FAILED ") + path);
        }

        // ------------------------------------------------------------------
        // Props (Docs/06 §5.3 canvases, Docs/05 legend sizes)
        // ------------------------------------------------------------------

        private static GameObject BuildBench()
        {
            var root = new GameObject("Bench");
            root.layer = GameLayers.Interactable;
            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.2f, 2f);
            col.offset = new Vector2(0f, 1f);
            var bench = root.AddComponent<Bench>();
            SpriteAnimator anim = AddVisual(root, "Bench", "Unlit", "Assets/Art/Props/Bench/Bench_Unlit_00.png", Vector3.zero, GameLayers.Sorting.Props);
            SceneBuildUtil.SetReference(bench, "animator", anim);
            return root;
        }

        private static GameObject BuildDoor()
        {
            var root = new GameObject("Door");
            root.layer = GameLayers.Ground;
            var col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 3f);
            col.offset = new Vector2(0.5f, 1.5f);
            var door = root.AddComponent<Door>();
            SpriteAnimator anim = AddVisual(root, "Door", "Closed", "Assets/Art/Props/Door/Door_Closed_00.png", Vector3.zero, GameLayers.Sorting.Props);
            SceneBuildUtil.SetReference(door, "blocker", col);
            SceneBuildUtil.SetReference(door, "animator", anim);
            return root;
        }

        private static GameObject BuildGate()
        {
            var root = new GameObject("BossGate");
            root.layer = GameLayers.Ground;
            var col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 3f);
            col.offset = new Vector2(0f, 1.5f);
            col.enabled = false;
            var gate = root.AddComponent<BossGate>();
            SpriteAnimator anim = AddVisual(root, "Gate", "Open", "Assets/Art/Props/Gate/Gate_Open_00.png", Vector3.zero, GameLayers.Sorting.Props);
            SceneBuildUtil.SetReference(gate, "blocker", col);
            SceneBuildUtil.SetReference(gate, "animator", anim);
            return root;
        }

        private static GameObject BuildOrb()
        {
            var root = new GameObject("Orb");
            root.layer = GameLayers.Interactable;
            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.1f;
            col.offset = new Vector2(0f, 0.64f);
            var orb = root.AddComponent<Orb>();
            SpriteAnimator anim = AddVisual(root, "Orb", "Idle", "Assets/Art/Props/Orb/Orb_Idle_00.png", new Vector3(0f, 0.64f, 0f), GameLayers.Sorting.Props);
            SceneBuildUtil.SetReference(orb, "animator", anim);
            return root;
        }

        private static GameObject BuildSign()
        {
            var root = new GameObject("TutorialSign");
            root.layer = GameLayers.Interactable;
            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(3f, 2f);
            col.offset = new Vector2(0f, 1f);
            root.AddComponent<TutorialSign>();
            AddVisual(root, "TutorialSign", "Default", "Assets/Art/Props/TutorialSign/TutorialSign_00.png", Vector3.zero, GameLayers.Sorting.Props);
            return root;
        }

        /// <summary>Visual child with a SpriteRenderer + SpriteAnimator bound to the Resources animation set (if built).</summary>
        public static SpriteAnimator AddVisual(GameObject root, string setName, string defaultClip, string fallbackSpritePath, Vector3 localPos, string sortingLayer)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = localPos;

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = sortingLayer;
            Material lit = SceneBuildUtil.SpriteLitMaterial();
            if (lit != null) sr.sharedMaterial = lit;

            var set = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(Art.RebuildAnimations.SetPath(setName));
            SpriteClip clip = set != null ? set.Find(defaultClip) : null;
            sr.sprite = clip != null && clip.IsValid ? clip.frames[0] : AssetDatabase.LoadAssetAtPath<Sprite>(fallbackSpritePath);

            var anim = visual.AddComponent<SpriteAnimator>();
            if (set != null) SceneBuildUtil.SetReference(anim, "set", set);
            SceneBuildUtil.SetString(anim, "defaultClip", defaultClip);
            return anim;
        }
    }
}
