using System.Collections.Generic;
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
using UnityEngine.Tilemaps;

namespace Shatranj.EditorTools.Level
{
    /// <summary>
    /// Docs/05 §8: ASCII rooms + manifest → a playable scene. One Grid with Ground / OneWay / Spikes / Lava
    /// tilemaps (composite colliders), RoomBounds, spawners with stable ids, benches, doors, gates, orb, signs,
    /// safe points, boss arenas, then the standard rig (camera, light, HUD, player, RoomManager).
    /// Menus: <c>Shatranj/Build Levels</c> (game manifest → phase scenes) and <c>Shatranj/Build LevelTest Scene</c>.
    /// </summary>
    public static class LevelBuilder
    {
        public const string LevelsFolder = "Assets/Data/Levels";
        public const string MainManifestPath = LevelsFolder + "/LevelManifest.json";
        public const string TestManifestPath = LevelsFolder + "/LevelManifest_Test.json";
        public const string TestScenePath = "Assets/Scenes/LevelTest.unity";

        private static readonly Color ParchmentColor = new Color(0.91f, 0.85f, 0.71f);

        [MenuItem("Shatranj/Build Levels", false, 60)]
        public static void BuildLevelsMenu()
        {
            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Build Levels ===");
            LevelSet set = LoadSet(MainManifestPath, report);
            if (set == null) { Debug.LogError(report.ToString()); return; }

            int built = 0;
            foreach (LevelManifestPhase phase in set.manifest.phases)
            {
                if (phase.rooms.Count == 0)
                {
                    report.AppendLine($"{phase.id} ({phase.scene}): no rooms in the manifest yet — skipped (authored in Prompts 8/9/13/14).");
                    continue;
                }
                if (BuildPhaseScene(set, phase, $"Assets/Scenes/{phase.scene}.unity", report)) built++;
            }
            LevelOverviewWriter.Write(set, report);
            report.AppendLine($"Done: {built} scene(s) built.");
            Debug.Log(report.ToString());
        }

        [MenuItem("Shatranj/Build LevelTest Scene", false, 61)]
        public static void BuildTestMenu()
        {
            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Build LevelTest Scene ===");
            LevelSet set = LoadSet(TestManifestPath, report);
            if (set == null) { Debug.LogError(report.ToString()); return; }
            if (set.manifest.phases.Count == 0) { report.AppendLine("Test manifest has no phases."); Debug.LogError(report.ToString()); return; }

            bool ok = BuildPhaseScene(set, set.manifest.phases[0], TestScenePath, report);
            if (ok) Debug.Log(report.ToString()); else Debug.LogError(report.ToString());
        }

        /// <summary>Loads and parses a manifest; reports errors/warnings. Returns null when there are errors.</summary>
        public static LevelSet LoadSet(string manifestPath, StringBuilder report)
        {
            LevelSet set;
            try { set = LevelParser.Load(manifestPath, LevelsFolder); }
            catch (LevelParseException e) { report.AppendLine("FAILED: " + e.Message); return null; }

            foreach (string w in set.warnings) report.AppendLine("  warn  " + w);
            if (set.HasErrors)
            {
                foreach (string e in set.errors) report.AppendLine("  ERROR " + e);
                report.AppendLine("Fix the room files above and run again.");
                return null;
            }
            report.AppendLine($"Manifest {manifestPath}: {set.rooms.Count} room(s) parsed.");
            return set;
        }

        // ------------------------------------------------------------------
        // Scene build
        // ------------------------------------------------------------------

        public static bool BuildPhaseScene(LevelSet set, LevelManifestPhase phase, string scenePath, StringBuilder report)
        {
            List<RoomData> rooms = set.RoomsOfPhase(phase);
            if (rooms.Count == 0) { report.AppendLine($"{phase.id}: no rooms."); return false; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            BuildPrefabs.EnsureLevelProps(false, report);
            BuildPrefabs.EnsureEnemyPrefabs(false, report);
            BuildPrefabs.EnsureBossPrefabs(false, report);

            // D-044: create the scene before loading any asset we will reference.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var playerConfig = AssetDatabase.LoadAssetAtPath<PlayerConfig>(GenerateConfigs.PlayerPath);
            var cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(GenerateConfigs.CameraPath);
            if (playerConfig == null || cameraConfig == null)
            {
                GenerateConfigs.Run();
                playerConfig = AssetDatabase.LoadAssetAtPath<PlayerConfig>(GenerateConfigs.PlayerPath);
                cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(GenerateConfigs.CameraPath);
            }

            var themes = new Dictionary<string, TileAssets.ThemeTiles>();
            var prefabs = new PropPrefabs();

            // --- Grid + tilemaps ---
            var gridGo = new GameObject("Level");
            var grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(RoomData.TileSize, RoomData.TileSize, 0f);
            // Tilemaps are visuals only; collision is baked into boxes below (D-112).
            Tilemap ground = MakeTilemap(grid.transform, "Ground", 0);
            Tilemap oneWay = MakeTilemap(grid.transform, "OneWay", 1);
            Tilemap spikes = MakeTilemap(grid.transform, "Spikes", 2);
            Tilemap lava = MakeTilemap(grid.transform, "Lava", 2);
            var groundCells = new HashSet<Vector2Int>();
            var oneWayCells = new HashSet<Vector2Int>();
            var spikeCells = new HashSet<Vector2Int>();
            var lavaCells = new HashSet<Vector2Int>();

            Transform roomsRoot = new GameObject("Rooms").transform;
            var sectionCounts = new Dictionary<string, int>();
            var sectionHasBoss = new HashSet<string>();
            RoomBounds startRoom = null;
            Transform spawnTransform = null;
            Vector2 playerStart = Vector2.zero;
            int tiles = 0, spawners = 0, props = 0;

            foreach (RoomData room in rooms)
            {
                string themeName = room.ThemeName;
                if (!themes.TryGetValue(themeName, out TileAssets.ThemeTiles t)) themes[themeName] = t = TileAssets.Load(themeName, report);

                // Tiles
                for (int y = 0; y < room.height; y++)
                {
                    for (int x = 0; x < room.width; x++)
                    {
                        char g = room.cells[x, y];
                        var cell = new Vector3Int(room.originX + x, room.originY + y, 0);
                        var key = new Vector2Int(cell.x, cell.y);
                        if (g == Glyphs.Solid) { ground.SetTile(cell, t.bitmask[BitmaskIndex(room, x, y)]); groundCells.Add(key); tiles++; }
                        else if (g == Glyphs.OneWay) { oneWay.SetTile(cell, t.OneWay(OneWayKind(room, x, y))); oneWayCells.Add(key); tiles++; }
                        else if (g == Glyphs.Spikes) { spikes.SetTile(cell, t.spikes); spikeCells.Add(key); tiles++; }
                        else if (g == Glyphs.Lava) { lava.SetTile(cell, t.lava != null ? t.lava : t.spikes); lavaCells.Add(key); tiles++; }
                    }
                }

                // Room object
                var roomGo = new GameObject("Room_" + room.id);
                roomGo.transform.SetParent(roomsRoot, false);
                roomGo.transform.position = room.WorldCenter;
                roomGo.layer = GameLayers.RoomBounds;
                roomGo.AddComponent<BoxCollider2D>();
                var bounds = roomGo.AddComponent<RoomBounds>();
                Vector2 entry = ComputeEntry(room);
                bounds.Configure(room.id, room.WorldRect.size, room.phase, room.section, room.theme, room.IsBossRoom, entry, room.orderIndex);

                // Markers & props
                var gates = new List<BossGate>();
                var slots = new List<Vector2>();
                Vector2 bossSpawn = room.WorldCenter;

                int benchIdx = 0, signIdx = 0, doorIdx = 0, safeIdx = 0;
                var enemyIndex = new Dictionary<char, int>();
                for (int y = room.height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < room.width; x++)
                    {
                        char g = room.cells[x, y];
                        Vector2 feet = room.CellBottomCenter(x, y);
                        switch (g)
                        {
                            case Glyphs.PlayerStart:
                            {
                                var p = new GameObject("PlayerSpawn", typeof(PlayerSpawnPoint));
                                p.transform.SetParent(roomGo.transform, false);
                                p.transform.position = feet;
                                spawnTransform = p.transform;
                                playerStart = feet;
                                startRoom = bounds;
                                break;
                            }
                            case Glyphs.SafeGround:
                            {
                                var s = new GameObject($"Safe_{room.id}_{safeIdx++}", typeof(SafeGroundPoint));
                                s.transform.SetParent(roomGo.transform, false);
                                s.transform.position = feet;
                                s.GetComponent<SafeGroundPoint>().Configure(room.id);
                                break;
                            }
                            case Glyphs.Bench:
                            {
                                GameObject b = prefabs.Instantiate("Bench", scene, roomGo.transform, feet);
                                if (b != null) { b.GetComponent<Bench>().Configure($"{room.id}_B{benchIdx}"); props++; }
                                benchIdx++;
                                break;
                            }
                            case Glyphs.Sign:
                            {
                                GameObject sgn = prefabs.Instantiate("TutorialSign", scene, roomGo.transform, feet);
                                string text = set.manifest.SignText(room.id, signIdx) ?? $"(sign {room.id} #{signIdx}: add text in LevelManifest.json)";
                                if (sgn != null) { sgn.GetComponent<TutorialSign>().Configure(text); props++; }
                                signIdx++;
                                break;
                            }
                            case Glyphs.Orb:
                            {
                                GameObject o = prefabs.Instantiate("Orb", scene, roomGo.transform, feet);
                                if (o != null) props++;
                                break;
                            }
                            case Glyphs.Door:
                            {
                                GameObject d = prefabs.Instantiate("Door", scene, roomGo.transform, room.CellBottomLeft(x, y));
                                if (d != null) { d.GetComponent<Door>().Configure($"{room.id}_D{doorIdx}", room.doorFlag); props++; }
                                doorIdx++;
                                break;
                            }
                            case Glyphs.Gate:
                            {
                                GameObject gt = prefabs.Instantiate("BossGate", scene, roomGo.transform, room.CellBottomLeft(x, y) + new Vector2(RoomData.TileSize, 0f));
                                if (gt != null) { gates.Add(gt.GetComponent<BossGate>()); props++; }
                                break;
                            }
                            case Glyphs.MiniBoss:
                            case Glyphs.FinalBoss:
                                bossSpawn = feet;
                                break;
                            default:
                                if (Glyphs.IsDropSlot(g)) slots.Add(room.CellCenter(x, y));
                                else if (Glyphs.IsEnemy(g))
                                {
                                    enemyIndex.TryGetValue(g, out int idx);
                                    enemyIndex[g] = idx + 1;
                                    string stableId = $"{room.id}_{g}_{idx}";
                                    var sp = new GameObject("Spawner_" + stableId, typeof(EnemySpawner));
                                    sp.transform.SetParent(roomGo.transform, false);
                                    bool flyer = g == Glyphs.Kite || g == Glyphs.Bat;
                                    sp.transform.position = flyer ? room.CellCenter(x, y) : feet;
                                    var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BuildPrefabs.EnemyPrefabPath(Glyphs.EnemyName(g)));
                                    sp.GetComponent<EnemySpawner>().Configure(stableId, room.id, Glyphs.EnemyId(g), g, enemyPrefab);
                                    spawners++;
                                }
                                break;
                        }
                    }
                }

                if (room.IsBossRoom)
                {
                    var arenaGo = new GameObject("BossArena_" + (string.IsNullOrEmpty(room.bossId) ? room.id : room.bossId), typeof(BossArena));
                    arenaGo.transform.SetParent(roomGo.transform, false);
                    arenaGo.transform.position = bossSpawn;
                    var arena = arenaGo.GetComponent<BossArena>();
                    arena.Configure(room.bossId, room.id, bossSpawn, slots, gates);
                    string bossPath = BuildPrefabs.BossPrefabPathForId(room.bossId);
                    GameObject bossPrefab = bossPath != null ? AssetDatabase.LoadAssetAtPath<GameObject>(bossPath) : null;
                    if (bossPrefab != null) SceneBuildUtil.SetReference(arena, "bossPrefab", bossPrefab);
                    else report.AppendLine($"  note  {room.id}: boss '{room.bossId}' has no prefab yet — the arena locks but nothing spawns.");
                }

                sectionCounts.TryGetValue(room.section, out int count);
                sectionCounts[room.section] = count + room.CountEnemies();
                if (room.IsBossRoom) sectionHasBoss.Add(room.section);
            }

            // --- Collision (D-112) ---
            var collisionRoot = new GameObject("Collision").transform;
            ColliderBaker.Bake("Ground", collisionRoot, groundCells, GameLayers.Ground, false, false, out int groundBoxes);
            ColliderBaker.Bake("OneWay", collisionRoot, oneWayCells, GameLayers.OneWay, false, true, out int oneWayBoxes);
            if (spikeCells.Count > 0)
                ColliderBaker.Bake("Spikes", collisionRoot, spikeCells, GameLayers.Hazard, true, false, out _).AddComponent<DamageDealer>().Configure(1, DamageType.Hazard);
            if (lavaCells.Count > 0)
                ColliderBaker.Bake("Lava", collisionRoot, lavaCells, GameLayers.Hazard, true, false, out _).AddComponent<DamageDealer>().Configure(1, DamageType.Lava);
            report.AppendLine($"  collision: {groundCells.Count} solid tiles → {groundBoxes} boxes, {oneWayCells.Count} one-way tiles → {oneWayBoxes} boxes");

            if (startRoom == null)
            {
                RoomData first = rooms[0];
                playerStart = ComputeEntry(first);
                startRoom = roomsRoot.GetChild(0).GetComponent<RoomBounds>();
                var p = new GameObject("PlayerSpawn", typeof(PlayerSpawnPoint));
                p.transform.position = playerStart;
                spawnTransform = p.transform;
                report.AppendLine("  warn  no 'P' in this phase — player starts at the first room's entry point.");
            }

            // --- Rig ---
            GameObject player = PlayerFactory.Build(playerConfig, playerStart, spawnTransform);
            var controller = player.GetComponent<PlayerController>();

            Camera cam = SceneBuildUtil.CreateMainCamera(new Vector3(playerStart.x, playerStart.y + cameraConfig.verticalOffset, -10f), cameraConfig.orthographicSize, ParchmentColor);
            var camController = cam.gameObject.AddComponent<CameraController>();
            SceneBuildUtil.SetReference(camController, "config", cameraConfig);
            SceneBuildUtil.SetReference(camController, "targetBehaviour", controller);
            SceneBuildUtil.SetReference(camController, "initialRoom", startRoom);
            VisualRigFactory.Build(cam, report);   // Prompt 10: parchment, flame, lights, PostFX, themes, wipes

            GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BuildUIPrefabs.HudPrefabPath);
            if (hudPrefab == null && BuildUIPrefabs.TmpEssentialsPresent()) { BuildUIPrefabs.Run(); hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BuildUIPrefabs.HudPrefabPath); }
            if (hudPrefab != null) ((GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, scene)).name = "HUD";
            else report.AppendLine("  warn  HUD prefab missing (import TMP Essentials + Build UI Prefabs).");

            new GameObject("[SceneBootstrap]").AddComponent<SceneBootstrap>();
            new GameObject("[RoomManager]").AddComponent<RoomManager>();
            var setup = new GameObject("[LevelSceneSetup]").AddComponent<LevelSceneSetup>();
            SceneBuildUtil.SetReference(setup, "defaultSpawn", spawnTransform);

            // --- Counts table (Docs/05 §8) ---
            report.AppendLine($"{phase.id} → {scenePath}: {rooms.Count} rooms, {tiles} tiles, {spawners} spawners, {props} props.");
            bool allPass = true;
            foreach (LevelManifestSection section in phase.sections)
            {
                if (!sectionCounts.TryGetValue(section.id, out int found) || !sectionHasBoss.Contains(section.id))
                {
                    report.AppendLine($"  section {section.id}: PENDING ({found}/{section.expectedEnemies} so far, no boss room yet)");
                    continue;
                }
                bool pass = found == section.expectedEnemies;
                allPass &= pass;
                report.AppendLine($"  section {section.id}: {found}/{section.expectedEnemies} enemies  {(pass ? "PASS" : "FAIL")}");
            }
            foreach (KeyValuePair<string, int> kv in sectionCounts)
                if (phase.FindSection(kv.Key) == null) report.AppendLine($"  section '{kv.Key}': {kv.Value} enemies (not declared in manifest)");

            // --- Save ---
            GenerateConfigs.EnsureFolder("Assets/Scenes");
            bool saved = EditorSceneManager.SaveScene(scene, scenePath);
            if (saved) SceneBuildUtil.AddSceneToBuildSettings(scenePath);
            AssetDatabase.SaveAssets();
            report.AppendLine(saved ? $"  saved {scenePath}" : $"  FAILED to save {scenePath}");
            return saved && allPass;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>Docs/06 §5.1: index = N·1 + E·2 + S·4 + W·8, bit set when the neighbour is solid (outside = solid).</summary>
        public static int BitmaskIndex(RoomData room, int x, int y)
        {
            int i = 0;
            if (room.IsSolid(x, y + 1)) i |= 1;
            if (room.IsSolid(x + 1, y)) i |= 2;
            if (room.IsSolid(x, y - 1)) i |= 4;
            if (room.IsSolid(x - 1, y)) i |= 8;
            return i;
        }

        /// <summary>-1 left end, 0 middle, +1 right end of a one-way run.</summary>
        public static int OneWayKind(RoomData room, int x, int y)
        {
            bool left = room.Get(x - 1, y) == Glyphs.OneWay;
            bool right = room.Get(x + 1, y) == Glyphs.OneWay;
            if (!left && right) return -1;
            if (left && !right) return 1;
            return 0;
        }

        /// <summary>Teleport target for F3/F6: 'P', else first 'S', else first 'B', else a floor cell near the room's centre.</summary>
        public static Vector2 ComputeEntry(RoomData room)
        {
            foreach (char g in new[] { Glyphs.PlayerStart, Glyphs.SafeGround, Glyphs.Bench })
            {
                List<Vector2Int> found = room.Find(g);
                if (found.Count > 0) return room.CellBottomCenter(found[0].x, found[0].y);
            }
            int cx = room.width / 2;
            for (int dx = 0; dx < room.width / 2; dx++)
            {
                foreach (int x in new[] { cx + dx, cx - dx })
                {
                    if (x < 1 || x >= room.width - 1) continue;
                    for (int y = 1; y < room.height - 2; y++)
                        if (Glyphs.IsPassable(room.Get(x, y)) && Glyphs.IsPassable(room.Get(x, y + 1)) && Glyphs.IsStandable(room.Get(x, y - 1)))
                            return room.CellBottomCenter(x, y);
                }
            }
            return room.WorldCenter;
        }

        /// <summary>Visual-only tilemap (no collider; collision is baked separately).</summary>
        private static Tilemap MakeTilemap(Transform parent, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = GameLayers.Ignore;
            var tm = go.AddComponent<Tilemap>();
            var tr = go.AddComponent<TilemapRenderer>();
            tr.sortingLayerName = GameLayers.Sorting.Tiles;
            tr.sortingOrder = order;
            return tm;
        }

        /// <summary>Caches the level prop prefabs and instantiates them into the scene being built.</summary>
        private class PropPrefabs
        {
            private readonly Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();

            public GameObject Instantiate(string name, Scene scene, Transform parent, Vector2 position)
            {
                if (!cache.TryGetValue(name, out GameObject prefab))
                {
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BuildPrefabs.LevelPrefabPath(name));
                    cache[name] = prefab;
                }
                if (prefab == null) { Debug.LogWarning($"LevelBuilder: prefab '{name}' missing — run Shatranj → Build Prefabs."); return null; }
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                go.transform.SetParent(parent, true);
                go.transform.position = position;
                return go;
            }
        }
    }
}
