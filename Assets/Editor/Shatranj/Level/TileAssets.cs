using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Shatranj.EditorTools.Level
{
    /// <summary>
    /// Tile assets (UnityEngine.Tilemaps.Tile) built from the placeholder tile sprites (Docs/06 §5.1):
    /// 16 bitmask tiles, one-way L/M/R, spikes, lava per theme, under Assets/Data/Tiles/&lt;Theme&gt;.
    /// Updated in place so GUIDs survive; artists replacing a tile PNG need no rebuild.
    /// </summary>
    public static class TileAssets
    {
        public const string Root = "Assets/Data/Tiles";
        public const string SpriteRoot = "Assets/Art/Tiles";

        public class ThemeTiles
        {
            public string theme;
            public Tile[] bitmask = new Tile[16];
            public Tile oneWayL, oneWayM, oneWayR;
            public Tile spikes;
            public Tile lava;
            /// <summary>Lava animation frames Tile_Castle_Lava_00..03 (animated at runtime by LavaSurface).</summary>
            public Tile[] lavaFrames = new Tile[0];
            public int missing;

            /// <summary>kind: -1 left end, 0 middle, +1 right end.</summary>
            public Tile OneWay(int kind) => kind < 0 ? (oneWayL ?? oneWayM) : kind > 0 ? (oneWayR ?? oneWayM) : oneWayM;
        }

        public static string TilePath(string theme, string name) => $"{Root}/{theme}/{name}.asset";
        public static string SpritePath(string theme, string name) => $"{SpriteRoot}/{theme}/{name}.png";

        /// <summary>Loads (creating if needed) every tile of a theme. Missing sprites are counted, not fatal.</summary>
        public static ThemeTiles Load(string theme, StringBuilder report = null)
        {
            if (AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath(theme, $"Tile_{theme}_00")) == null)
            {
                report?.AppendLine($"Tile sprites for '{theme}' missing → generating placeholder art.");
                Art.GeneratePlaceholderArt.Run();
            }

            GenerateConfigs.EnsureFolder(Root);
            GenerateConfigs.EnsureFolder($"{Root}/{theme}");

            var t = new ThemeTiles { theme = theme };
            for (int i = 0; i < 16; i++) t.bitmask[i] = GetOrCreate(theme, $"Tile_{theme}_{i:00}", Tile.ColliderType.Grid, t);
            t.oneWayL = GetOrCreate(theme, $"Tile_{theme}_OneWay_L", Tile.ColliderType.Grid, t);
            t.oneWayM = GetOrCreate(theme, $"Tile_{theme}_OneWay_M", Tile.ColliderType.Grid, t);
            t.oneWayR = GetOrCreate(theme, $"Tile_{theme}_OneWay_R", Tile.ColliderType.Grid, t);
            t.spikes = GetOrCreate(theme, $"Tile_{theme}_Spikes", Tile.ColliderType.Grid, t);
            if (theme == "Castle")
            {
                t.lavaFrames = new Tile[4];
                for (int i = 0; i < t.lavaFrames.Length; i++) t.lavaFrames[i] = GetOrCreate(theme, $"Tile_Castle_Lava_{i:00}", Tile.ColliderType.Grid, t);
                t.lava = t.lavaFrames[0];
            }
            else t.lava = null;

            if (t.missing > 0) report?.AppendLine($"  ! {t.missing} tile sprite(s) missing for theme {theme}");
            return t;
        }

        private static Tile GetOrCreate(string theme, string name, Tile.ColliderType collider, ThemeTiles owner)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath(theme, name));
            if (sprite == null) { owner.missing++; return null; }

            string path = TilePath(theme, name);
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.colliderType = collider;
                tile.name = name;
                AssetDatabase.CreateAsset(tile, path);
            }
            else if (tile.sprite != sprite || tile.colliderType != collider)
            {
                tile.sprite = sprite;
                tile.colliderType = collider;
                EditorUtility.SetDirty(tile);
            }
            return tile;
        }
    }
}
