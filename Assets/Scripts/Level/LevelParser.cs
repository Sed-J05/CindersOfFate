using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Shatranj.Level
{
    public class LevelParseException : Exception
    {
        public LevelParseException(string message) : base(message) { }
    }

    /// <summary>
    /// Parses Docs/05 §3 room files and LevelManifest.json. Pure C# (no Unity objects), so EditMode tests
    /// and editor tools share it. Errors name the file, line and column.
    /// </summary>
    public static class LevelParser
    {
        private static readonly Regex HeaderLine = new Regex(@"^#\s*(id|size|origin|theme|music|name)\s*:\s*(.*)$", RegexOptions.IgnoreCase);
        private static readonly Regex SizeValue = new Regex(@"^\s*(\d+)\s*[xX×]\s*(\d+)\s*$");
        private static readonly Regex OriginValue = new Regex(@"^\s*(-?\d+)\s*,\s*(-?\d+)\s*$");

        /// <summary>Parses one room file. Throws <see cref="LevelParseException"/> with a precise message.</summary>
        public static RoomData ParseRoom(string text, string fileName, List<string> warnings = null)
        {
            if (text == null) throw new LevelParseException($"{fileName}: file is empty");
            var room = new RoomData { fileName = fileName };
            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            var rows = new List<string>();
            var rowLineNumbers = new List<int>();
            bool inHeader = true;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNo = i + 1;

                if (inHeader)
                {
                    Match m = HeaderLine.Match(line);
                    if (m.Success)
                    {
                        string key = m.Groups[1].Value.ToLowerInvariant();
                        string value = m.Groups[2].Value.Trim();
                        int comment = value.IndexOf("  (", StringComparison.Ordinal); // "60x22   (width x height …)"
                        if (comment > 0) value = value.Substring(0, comment).Trim();
                        room.header[key] = value;
                        continue;
                    }
                    if (line.Trim().Length == 0) continue;
                    inHeader = false;
                }

                if (line.Length == 0) continue; // blank separator lines are ignored; a row of spaces has length W
                rows.Add(line);
                rowLineNumbers.Add(lineNo);
            }

            // --- header ---
            if (!room.header.TryGetValue("id", out string id) || string.IsNullOrEmpty(id))
                throw new LevelParseException($"{fileName}: missing header '# id:'");
            room.id = id;
            room.name = room.header.TryGetValue("name", out string nm) ? nm : id;
            room.theme = room.header.TryGetValue("theme", out string th) ? th.ToLowerInvariant() : "cave";
            room.music = room.header.TryGetValue("music", out string mu) ? mu : "none";

            if (!room.header.TryGetValue("size", out string size))
                throw new LevelParseException($"{fileName}: missing header '# size: WxH'");
            Match sm = SizeValue.Match(size);
            if (!sm.Success) throw new LevelParseException($"{fileName}: bad size '{size}' (expected e.g. 60x22)");
            room.width = int.Parse(sm.Groups[1].Value);
            room.height = int.Parse(sm.Groups[2].Value);
            if (room.width <= 0 || room.height <= 0) throw new LevelParseException($"{fileName}: size must be positive, got {size}");

            if (room.header.TryGetValue("origin", out string origin))
            {
                Match om = OriginValue.Match(origin);
                if (!om.Success) throw new LevelParseException($"{fileName}: bad origin '{origin}' (expected e.g. 40,0)");
                room.originX = int.Parse(om.Groups[1].Value);
                room.originY = int.Parse(om.Groups[2].Value);
            }
            else
            {
                warnings?.Add($"{fileName}: no '# origin:' header, defaulting to 0,0");
            }

            // --- rows ---
            if (rows.Count != room.height)
                throw new LevelParseException($"{fileName}: found {rows.Count} map rows but '# size:' says height {room.height}");

            room.cells = new char[room.width, room.height];
            int players = 0;
            for (int r = 0; r < rows.Count; r++)
            {
                string row = rows[r];
                int lineNo = rowLineNumbers[r];
                if (row.Length > room.width)
                    throw new LevelParseException($"{fileName} line {lineNo}: row has {row.Length} characters, expected {room.width}");
                if (row.Length < room.width)
                {
                    warnings?.Add($"{fileName} line {lineNo}: row has {row.Length} characters, padded to {room.width} with '.'");
                    row = row.PadRight(room.width, Glyphs.Empty);
                }

                int y = room.height - 1 - r; // top row first
                for (int x = 0; x < room.width; x++)
                {
                    char c = row[x];
                    if (!Glyphs.IsKnown(c))
                        throw new LevelParseException($"{fileName} line {lineNo} col {x + 1}: unknown glyph '{c}'");
                    if (c == Glyphs.Space) c = Glyphs.Empty;
                    if (c == Glyphs.PlayerStart) players++;
                    room.cells[x, y] = c;
                }
            }

            if (players > 1) throw new LevelParseException($"{fileName}: {players} player starts 'P' (max 1)");
            return room;
        }

        public static LevelManifest ParseManifest(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new LevelParseException("LevelManifest: file is empty");
            LevelManifest m;
            try { m = JsonUtility.FromJson<LevelManifest>(json); }
            catch (Exception e) { throw new LevelParseException("LevelManifest: " + e.Message); }
            if (m == null) throw new LevelParseException("LevelManifest: could not parse JSON");
            m.phases ??= new List<LevelManifestPhase>();
            m.signs ??= new List<LevelManifestSign>();
            m.doors ??= new List<LevelManifestDoor>();
            foreach (LevelManifestPhase p in m.phases)
            {
                p.rooms ??= new List<LevelManifestRoom>();
                p.sections ??= new List<LevelManifestSection>();
            }
            return m;
        }

        /// <summary>
        /// Loads the manifest and every room it lists. Room errors are collected in <see cref="LevelSet.errors"/>
        /// (one bad file doesn't hide the others); a missing or broken manifest throws.
        /// </summary>
        public static LevelSet Load(string manifestPath, string levelsFolder)
        {
            if (!File.Exists(manifestPath)) throw new LevelParseException($"Manifest not found: {manifestPath}");
            var set = new LevelSet { manifest = ParseManifest(File.ReadAllText(manifestPath)) };

            int order = 0;
            foreach (LevelManifestPhase phase in set.manifest.phases)
            {
                foreach (LevelManifestRoom mr in phase.rooms)
                {
                    string file = string.IsNullOrEmpty(mr.file) ? mr.id + ".txt" : mr.file;
                    string path = Path.Combine(levelsFolder, file);
                    if (!File.Exists(path)) { set.errors.Add($"{phase.id}/{mr.id}: room file not found: {path}"); continue; }

                    RoomData room;
                    try { room = ParseRoom(File.ReadAllText(path), file, set.warnings); }
                    catch (LevelParseException e) { set.errors.Add(e.Message); continue; }

                    if (room.id != mr.id) set.warnings.Add($"{file}: header id '{room.id}' differs from manifest id '{mr.id}' (manifest wins)");
                    room.id = mr.id;
                    room.section = mr.section ?? "";
                    room.phase = phase.phase;
                    room.orderIndex = order++;
                    if (mr.useManifestOrigin) { room.originX = mr.originX; room.originY = mr.originY; }
                    if (string.IsNullOrEmpty(room.theme)) room.theme = phase.theme;

                    LevelManifestSection section = phase.FindSection(room.section);
                    room.bossId = !string.IsNullOrEmpty(mr.bossId) ? mr.bossId : (room.IsBossRoom && section != null ? section.bossId : "");
                    room.doorFlag = set.manifest.DoorFlag(room.id);

                    if (set.byId.ContainsKey(room.id)) { set.errors.Add($"duplicate room id '{room.id}'"); continue; }
                    set.rooms.Add(room);
                    set.byId[room.id] = room;
                }
            }
            return set;
        }
    }
}
