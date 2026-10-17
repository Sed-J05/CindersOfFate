using System.Collections.Generic;
using System.Text;
using Shatranj.Core;
using Shatranj.Level;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools.Level
{
    /// <summary>
    /// Menu: <c>Shatranj/Validate Project</c>. Layers, manifest + rooms, enemy counts per section vs Docs/01 §6,
    /// room overlaps, opening alignment and height, enemies near entrances, horizontal gaps vs Docs/05 §2,
    /// missing prefabs, scenes in Build Settings. Readable PASS/FAIL report in the Console.
    /// </summary>
    public static class ValidateProject
    {
        private const int MaxGapNoDash = 11, SafeGapNoDash = 8, MaxGapDash = 16, SafeGapDash = 12;

        [MenuItem("Shatranj/Validate Project", false, 70)]
        public static void Run()
        {
            var r = new Report();
            r.Line("=== Shatranj / Validate Project ===");

            // 1. Layers / sorting layers
            bool layersOk = GameLayers.Validate(out string layerReport);
            r.Line(layersOk ? "[PASS] layers & sorting layers" : "[FAIL] layers: " + layerReport.Trim(), !layersOk);

            // 2. Manifest + rooms
            LevelSet set = null;
            try { set = LevelParser.Load(LevelBuilder.MainManifestPath, LevelBuilder.LevelsFolder); }
            catch (LevelParseException e) { r.Line("[FAIL] " + e.Message, true); }

            if (set != null)
            {
                foreach (string w in set.warnings) r.Line("[WARN] " + w, false, true);
                foreach (string e in set.errors) r.Line("[FAIL] " + e, true);
                ValidateCounts(set, r);
                foreach (LevelManifestPhase phase in set.manifest.phases) ValidateGeometry(set, phase, r);
            }

            // 3. Prefabs & scenes
            ValidatePrefabs(set, r);
            ValidateScenes(set, r);

            r.Line($"Result: {r.Errors} error(s), {r.Warnings} warning(s). " + (r.Errors == 0 ? "PROJECT VALID (pending items are expected until later prompts)." : "Fix the [FAIL] lines."));
            if (r.Errors > 0) Debug.LogError(r.ToString()); else Debug.Log(r.ToString());
        }

        // ------------------------------------------------------------------

        private static void ValidateCounts(LevelSet set, Report r)
        {
            r.Line("Enemy counts per section (Docs/01 §6):");
            foreach (LevelManifestPhase phase in set.manifest.phases)
            {
                List<RoomData> rooms = set.RoomsOfPhase(phase);
                if (rooms.Count == 0)
                {
                    foreach (LevelManifestSection s in phase.sections)
                        r.Line($"  [PENDING] {phase.id}/{s.id}: expected {s.expectedEnemies}, no rooms authored yet");
                    continue;
                }
                int phaseTotal = 0;
                foreach (LevelManifestSection s in phase.sections)
                {
                    int found = 0, sectionRooms = 0;
                    foreach (RoomData room in rooms) if (room.section == s.id) { found += room.CountEnemies(); sectionRooms++; }
                    if (sectionRooms == 0)
                    {
                        r.Line($"  [PENDING] {phase.id}/{s.id}: expected {s.expectedEnemies}, no rooms authored yet");
                        continue;
                    }
                    phaseTotal += found;
                    bool ok = found == s.expectedEnemies;
                    r.Line($"  [{(ok ? "PASS" : "FAIL")}] {phase.id}/{s.id}: {found}/{s.expectedEnemies}", !ok);

                    foreach (RoomData room in rooms)
                    {
                        if (room.section != s.id) continue;
                        for (int y = 0; y < room.height; y++)
                            for (int x = 0; x < room.width; x++)
                            {
                                char g = room.cells[x, y];
                                if (Glyphs.IsEnemy(g) && Glyphs.EnemyPhase(g) != phase.phase)
                                    r.Line($"  [WARN] {room.id}: enemy '{g}' is a Phase {Glyphs.EnemyPhase(g)} enemy in a Phase {phase.phase} room", false, true);
                            }
                    }
                }
                r.Line($"  {phase.id} total: {phaseTotal}");
            }
        }

        private static void ValidateGeometry(LevelSet set, LevelManifestPhase phase, Report r)
        {
            List<RoomData> rooms = set.RoomsOfPhase(phase);
            if (rooms.Count == 0) return;
            int minOpening = phase.phase >= 2 ? 8 : 6;
            r.Line($"Geometry {phase.id}:");

            // Overlaps
            for (int i = 0; i < rooms.Count; i++)
                for (int j = i + 1; j < rooms.Count; j++)
                    if (rooms[i].TileRect.Overlaps(rooms[j].TileRect))
                        r.Line($"  [FAIL] rooms {rooms[i].id} and {rooms[j].id} overlap in world space", true);

            foreach (RoomData room in rooms)
            {
                // Outer ring + openings
                var openings = new List<Vector2Int>();
                for (int x = 0; x < room.width; x++)
                {
                    if (!room.IsSolid(x, 0)) openings.Add(new Vector2Int(x, 0));
                    if (!room.IsSolid(x, room.height - 1)) openings.Add(new Vector2Int(x, room.height - 1));
                }
                for (int y = 1; y < room.height - 1; y++)
                {
                    if (!room.IsSolid(0, y)) openings.Add(new Vector2Int(0, y));
                    if (!room.IsSolid(room.width - 1, y)) openings.Add(new Vector2Int(room.width - 1, y));
                }

                // Each opening cell must face an open cell of a neighbouring room.
                int dead = 0;
                foreach (Vector2Int o in openings)
                {
                    int wx = room.originX + o.x, wy = room.originY + o.y;
                    int nx = wx, ny = wy;
                    if (o.x == 0) nx = wx - 1; else if (o.x == room.width - 1) nx = wx + 1;
                    else if (o.y == 0) ny = wy - 1; else ny = wy + 1;

                    RoomData other = RoomAtTile(rooms, nx, ny, room);
                    if (other == null) { dead++; continue; }
                    char oc = other.Get(nx - other.originX, ny - other.originY);
                    if (Glyphs.IsSolid(oc))
                        r.Line($"  [FAIL] {room.id} opening at tile ({o.x},{o.y}) faces a solid cell in {other.id}", true);
                }
                if (dead > 0) r.Line($"  [WARN] {room.id}: {dead} boundary cell(s) are open but no room is on the other side", false, true);

                // Opening heights on the vertical walls.
                foreach (int x in new[] { 0, room.width - 1 })
                {
                    int run = 0;
                    for (int y = 0; y <= room.height; y++)
                    {
                        bool open = y < room.height && !room.IsSolid(x, y);
                        if (open) run++;
                        else
                        {
                            if (run > 0 && run < minOpening)
                                r.Line($"  [FAIL] {room.id}: side opening at x={x} is {run} tiles tall, minimum {minOpening} (Docs/05 §3 rule 3)", true);
                            run = 0;
                        }
                    }
                }

                // Enemies within 6 tiles of an opening (rule 5).
                for (int y = 0; y < room.height; y++)
                    for (int x = 0; x < room.width; x++)
                    {
                        if (!Glyphs.IsEnemy(room.cells[x, y])) continue;
                        foreach (Vector2Int o in openings)
                            if (Mathf.Max(Mathf.Abs(o.x - x), Mathf.Abs(o.y - y)) < 6)
                            {
                                r.Line($"  [WARN] {room.id}: enemy '{room.cells[x, y]}' at ({x},{y}) is within 6 tiles of an entrance", false, true);
                                break;
                            }
                    }

                // Benches never in boss arenas (rule 6).
                if (room.IsBossRoom && room.Contains(Glyphs.Bench))
                    r.Line($"  [FAIL] {room.id}: bench inside a boss arena", true);

                // Horizontal gaps (Docs/05 §2).
                int maxGap = phase.phase >= 2 ? MaxGapDash : MaxGapNoDash;
                int safeGap = phase.phase >= 2 ? SafeGapDash : SafeGapNoDash;
                for (int y = 1; y < room.height - 1; y++)
                {
                    int gap = 0; bool inGap = false;
                    for (int x = 1; x < room.width - 1; x++)
                    {
                        bool standable = Glyphs.IsPassable(room.Get(x, y)) && Glyphs.IsStandable(room.Get(x, y - 1));
                        bool pit = Glyphs.IsPassable(room.Get(x, y)) && !Glyphs.IsStandable(room.Get(x, y - 1)) && DangerousBelow(room, x, y);
                        if (standable)
                        {
                            if (inGap)
                            {
                                if (gap > maxGap) r.Line($"  [FAIL] {room.id}: {gap}-tile gap at row y={y} (x≈{x - gap}) exceeds the hard max {maxGap}", true);
                                else if (gap > safeGap) r.Line($"  [WARN] {room.id}: {gap}-tile gap at row y={y} (x≈{x - gap}) is above the safe {safeGap}", false, true);
                            }
                            inGap = true; gap = 0;
                        }
                        else if (inGap && pit) gap++;
                        else { inGap = false; gap = 0; }
                    }
                }
            }
            r.Line("  (vertical reach is not checked automatically — Docs/05 §2: ≤ 5 tiles up per jump)");
        }

        /// <summary>A gap only counts if falling in is punished: the first non-empty cell below is spikes/lava, or there is no floor (D-101).</summary>
        private static bool DangerousBelow(RoomData room, int x, int y)
        {
            for (int yy = y - 1; yy >= 0; yy--)
            {
                char c = room.Get(x, yy);
                if (c == Glyphs.Spikes || c == Glyphs.Lava) return true;
                if (Glyphs.IsStandable(c)) return false;
            }
            return true;
        }

        private static RoomData RoomAtTile(List<RoomData> rooms, int tx, int ty, RoomData exclude)
        {
            foreach (RoomData room in rooms)
                if (room != exclude && room.TileRect.Contains(new Vector2Int(tx, ty))) return room;
            return null;
        }

        private static void ValidatePrefabs(LevelSet set, Report r)
        {
            r.Line("Prefabs:");
            foreach (string n in BuildPrefabs.LevelPropNames)
            {
                bool ok = AssetDatabase.LoadAssetAtPath<GameObject>(BuildPrefabs.LevelPrefabPath(n)) != null;
                r.Line($"  [{(ok ? "PASS" : "WARN")}] {BuildPrefabs.LevelPrefabPath(n)}" + (ok ? "" : " missing — run Shatranj → Build Prefabs"), false, !ok);
            }
            bool hud = AssetDatabase.LoadAssetAtPath<GameObject>(BuildUIPrefabs.HudPrefabPath) != null;
            r.Line($"  [{(hud ? "PASS" : "WARN")}] {BuildUIPrefabs.HudPrefabPath}" + (hud ? "" : " missing — run Shatranj → Build UI Prefabs"), false, !hud);

            var usedGlyphs = new HashSet<char>();
            if (set != null)
                foreach (RoomData room in set.rooms)
                    for (int y = 0; y < room.height; y++)
                        for (int x = 0; x < room.width; x++)
                            if (Glyphs.IsEnemy(room.cells[x, y])) usedGlyphs.Add(room.cells[x, y]);
            foreach (char g in usedGlyphs)
            {
                string path = BuildPrefabs.EnemyPrefabPath(Glyphs.EnemyName(g));
                bool ok = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
                r.Line($"  [{(ok ? "PASS" : "PENDING")}] enemy '{g}' → {path}" + (ok ? "" : " (Prompt 7)"));
            }
        }

        private static void ValidateScenes(LevelSet set, Report r)
        {
            r.Line("Build Settings scenes:");
            var inBuild = new HashSet<string>();
            foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes) inBuild.Add(System.IO.Path.GetFileNameWithoutExtension(s.path));
            var expected = new List<string> { GameConstants.SceneMainMenu, GameConstants.ScenePhase1, GameConstants.ScenePhase2, GameConstants.SceneCutscene, GameConstants.SceneSandbox };
            foreach (string name in expected)
                r.Line($"  [{(inBuild.Contains(name) ? "PASS" : "PENDING")}] {name}");
        }

        private class Report
        {
            private readonly StringBuilder sb = new StringBuilder();
            public int Errors, Warnings;
            public void Line(string text, bool isError = false, bool isWarning = false)
            {
                sb.AppendLine(text);
                if (isError) Errors++;
                if (isWarning) Warnings++;
            }
            public override string ToString() => sb.ToString();
        }
    }
}
