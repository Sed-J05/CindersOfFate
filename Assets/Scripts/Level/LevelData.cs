using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>Docs/05 §3 legend.</summary>
    public static class Glyphs
    {
        public const char Solid = '#';
        public const char Empty = '.';
        public const char Space = ' ';
        public const char OneWay = '=';
        public const char Spikes = 'x';
        public const char PlayerStart = 'P';
        public const char Bench = 'B';
        public const char Crawler = 'c';
        public const char Lunger = 'l';
        public const char Kite = 'f';
        public const char Knight = 'k';
        public const char Bat = 'b';
        public const char Charger = 'h';
        public const char Thrower = 't';
        public const char MiniBoss = 'M';
        public const char FinalBoss = 'Z';
        public const char Door = 'D';
        public const char Orb = 'O';
        public const char Gate = 'G';
        public const char Lava = 'L';
        public const char Sign = 'T';
        public const char SafeGround = 'S';

        public const string All = "#. =xPBclfkbhtMZDOGLTS123456789";

        public static bool IsKnown(char c) => All.IndexOf(c) >= 0;
        public static bool IsEmpty(char c) => c == Empty || c == Space;
        public static bool IsSolid(char c) => c == Solid;
        public static bool IsEnemy(char c) => c == Crawler || c == Lunger || c == Kite || c == Knight || c == Bat || c == Charger || c == Thrower;
        public static bool IsBoss(char c) => c == MiniBoss || c == FinalBoss;
        public static bool IsDropSlot(char c) => c >= '1' && c <= '9';
        /// <summary>A surface the player can stand on.</summary>
        public static bool IsStandable(char c) => c == Solid || c == OneWay;
        /// <summary>Cells that block movement (solid only; markers and one-way platforms don't).</summary>
        public static bool BlocksMovement(char c) => c == Solid;
        /// <summary>Cells where an object marker sits in otherwise empty space.</summary>
        public static bool IsPassable(char c) => !IsSolid(c);

        /// <summary>EnemyConfig id for an enemy glyph (Docs/04).</summary>
        public static string EnemyId(char c)
        {
            switch (c)
            {
                case Crawler: return "crawler";
                case Lunger: return "lunger";
                case Kite: return "shadow_kite";
                case Knight: return "knight";
                case Bat: return "bat";
                case Charger: return "charger";
                case Thrower: return "thrower";
                default: return null;
            }
        }

        /// <summary>Prefab / set name for an enemy glyph.</summary>
        public static string EnemyName(char c)
        {
            switch (c)
            {
                case Crawler: return "Crawler";
                case Lunger: return "Lunger";
                case Kite: return "ShadowKite";
                case Knight: return "Knight";
                case Bat: return "Bat";
                case Charger: return "Charger";
                case Thrower: return "Thrower";
                default: return null;
            }
        }

        public static int EnemyPhase(char c) => c == Crawler || c == Lunger || c == Kite ? 1 : 2;
    }

    /// <summary>One parsed room file. Cells are indexed (x, y) with y = 0 at the BOTTOM row.</summary>
    public class RoomData
    {
        public string id = "";
        public string name = "";
        public string theme = "cave";
        public string music = "none";
        public string fileName = "";
        public int width;
        public int height;
        public int originX;
        public int originY;
        public char[,] cells;
        public readonly Dictionary<string, string> header = new Dictionary<string, string>();

        // Filled from the manifest.
        public string section = "";
        public int phase = 1;
        public string bossId = "";
        public string doorFlag = "";
        public int orderIndex;

        public const float TileSize = 0.5f;

        public string ThemeName => theme != null && theme.Equals("castle", StringComparison.OrdinalIgnoreCase) ? "Castle" : "Cave";

        /// <summary>Glyph at (x, y); outside the room counts as solid.</summary>
        public char Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return Glyphs.Solid;
            return cells[x, y];
        }

        public bool IsSolid(int x, int y) => Glyphs.IsSolid(Get(x, y));

        public RectInt TileRect => new RectInt(originX, originY, width, height);

        public Rect WorldRect => new Rect(originX * TileSize, originY * TileSize, width * TileSize, height * TileSize);

        public Vector2 WorldCenter => WorldRect.center;

        /// <summary>World position of the cell's centre.</summary>
        public Vector2 CellCenter(int x, int y) => new Vector2((originX + x) * TileSize + TileSize * 0.5f, (originY + y) * TileSize + TileSize * 0.5f);

        /// <summary>World position of the cell's bottom-left corner.</summary>
        public Vector2 CellBottomLeft(int x, int y) => new Vector2((originX + x) * TileSize, (originY + y) * TileSize);

        /// <summary>World position of the cell's bottom-centre (feet of a ground object).</summary>
        public Vector2 CellBottomCenter(int x, int y) => new Vector2((originX + x) * TileSize + TileSize * 0.5f, (originY + y) * TileSize);

        /// <summary>All cells holding <paramref name="glyph"/>, in reading order (top row first, left to right).</summary>
        public List<Vector2Int> Find(char glyph)
        {
            var list = new List<Vector2Int>();
            for (int y = height - 1; y >= 0; y--)
                for (int x = 0; x < width; x++)
                    if (cells[x, y] == glyph) list.Add(new Vector2Int(x, y));
            return list;
        }

        public int CountEnemies()
        {
            int n = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (Glyphs.IsEnemy(cells[x, y])) n++;
            return n;
        }

        public bool Contains(char glyph)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (cells[x, y] == glyph) return true;
            return false;
        }

        public bool IsBossRoom => Contains(Glyphs.MiniBoss) || Contains(Glyphs.FinalBoss);
    }

    // ------------------------------------------------------------------
    // LevelManifest.json (JsonUtility-compatible)
    // ------------------------------------------------------------------

    [Serializable]
    public class LevelManifestRoom
    {
        public string id = "";
        public string file = "";
        public string section = "";
        public string bossId = "";
        public bool useManifestOrigin = false;
        public int originX;
        public int originY;
    }

    [Serializable]
    public class LevelManifestSection
    {
        public string id = "";
        public int expectedEnemies;
        public string bossId = "";
    }

    [Serializable]
    public class LevelManifestPhase
    {
        public string id = "P1";
        public int phase = 1;
        public string scene = "Phase1_Cave";
        public string theme = "cave";
        public List<LevelManifestRoom> rooms = new List<LevelManifestRoom>();
        public List<LevelManifestSection> sections = new List<LevelManifestSection>();

        public LevelManifestSection FindSection(string sectionId)
        {
            for (int i = 0; i < sections.Count; i++) if (sections[i].id == sectionId) return sections[i];
            return null;
        }
    }

    [Serializable]
    public class LevelManifestSign
    {
        public string room = "";
        public int index;
        public string text = "";
    }

    [Serializable]
    public class LevelManifestDoor
    {
        public string room = "";
        public string flag = "";
    }

    [Serializable]
    public class LevelManifest
    {
        public int version = 1;
        public List<LevelManifestPhase> phases = new List<LevelManifestPhase>();
        public List<LevelManifestSign> signs = new List<LevelManifestSign>();
        public List<LevelManifestDoor> doors = new List<LevelManifestDoor>();

        public string SignText(string roomId, int index)
        {
            for (int i = 0; i < signs.Count; i++)
                if (signs[i].room == roomId && signs[i].index == index) return signs[i].text;
            return null;
        }

        public string DoorFlag(string roomId)
        {
            for (int i = 0; i < doors.Count; i++)
                if (doors[i].room == roomId) return doors[i].flag;
            return "door_" + roomId.ToLowerInvariant();
        }
    }

    /// <summary>Manifest + every parsed room, plus the parse/validation messages.</summary>
    public class LevelSet
    {
        public LevelManifest manifest;
        public readonly List<RoomData> rooms = new List<RoomData>();
        public readonly Dictionary<string, RoomData> byId = new Dictionary<string, RoomData>();
        public readonly List<string> errors = new List<string>();
        public readonly List<string> warnings = new List<string>();

        public bool HasErrors => errors.Count > 0;

        public RoomData Find(string id) => byId.TryGetValue(id, out RoomData r) ? r : null;

        public List<RoomData> RoomsOfPhase(LevelManifestPhase phase)
        {
            var list = new List<RoomData>();
            foreach (LevelManifestRoom mr in phase.rooms)
            {
                RoomData r = Find(mr.id);
                if (r != null) list.Add(r);
            }
            return list;
        }

        public List<RoomData> RoomsOfSection(LevelManifestPhase phase, string sectionId)
        {
            var list = new List<RoomData>();
            foreach (RoomData r in RoomsOfPhase(phase)) if (r.section == sectionId) list.Add(r);
            return list;
        }
    }
}
