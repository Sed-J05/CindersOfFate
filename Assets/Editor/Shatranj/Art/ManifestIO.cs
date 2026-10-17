using System.IO;
using Shatranj.Data;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools.Art
{
    /// <summary>
    /// Reads / writes Assets/Data/AnimationManifest.json and resolves which manifest set a texture path
    /// belongs to (for pivots in the importer). Menu: <c>Shatranj/Generate Animation Manifest</c>.
    /// </summary>
    public static class ManifestIO
    {
        public const string Path = "Assets/Data/AnimationManifest.json";

        private static ArtManifest cached;
        private static System.DateTime cachedStamp;

        [MenuItem("Shatranj/Generate Animation Manifest", false, 29)]
        public static void GenerateMenu()
        {
            if (File.Exists(Path))
            {
                Debug.Log($"=== Shatranj / Generate Animation Manifest ===\n{Path} already exists — kept (delete it to regenerate from Docs/06).");
                return;
            }
            ArtManifest m = Generate();
            Debug.Log($"=== Shatranj / Generate Animation Manifest ===\nWrote {Path}: {m.sets.Count} sets, {CountFrames(m)} frames. Edit it by hand; the tools read it.");
        }

        /// <summary>Writes the Docs/06 tables to disk and returns them.</summary>
        public static ArtManifest Generate()
        {
            ArtManifest m = ArtSpecTables.Build();
            Save(m);
            return m;
        }

        public static void Save(ArtManifest m)
        {
            string dir = System.IO.Path.GetDirectoryName(Path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(Path, JsonUtility.ToJson(m, prettyPrint: true));
            AssetDatabase.ImportAsset(Path);
            cached = null;
        }

        /// <summary>Loads the manifest, generating it from Docs/06 when the file is missing.</summary>
        public static ArtManifest LoadOrGenerate()
        {
            ArtManifest m = Load();
            return m ?? Generate();
        }

        /// <summary>Loads the manifest (cached by file timestamp). Null when missing or unreadable.</summary>
        public static ArtManifest Load()
        {
            if (!File.Exists(Path)) return null;
            System.DateTime stamp = File.GetLastWriteTimeUtc(Path);
            if (cached != null && stamp == cachedStamp) return cached;
            try
            {
                cached = JsonUtility.FromJson<ArtManifest>(File.ReadAllText(Path));
                cachedStamp = stamp;
                return cached;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ManifestIO: cannot parse {Path} — {e.Message}");
                return null;
            }
        }

        public static int CountFrames(ArtManifest m)
        {
            int n = 0;
            foreach (SpriteSetDef s in m.sets) n += s.TotalFrames;
            return n;
        }

        /// <summary>
        /// Finds the set that owns <paramref name="assetPath"/> by folder + longest matching name prefix
        /// (e.g. "Slash_Side_01.png" → "Slash_Side", not "Slash_Up"; "Tile_Cave_07.png" → "Tile_Cave_07").
        /// </summary>
        public static SpriteSetDef FindSetForPath(ArtManifest m, string assetPath)
        {
            if (m == null || string.IsNullOrEmpty(assetPath)) return null;
            string norm = assetPath.Replace('\\', '/');
            if (!norm.StartsWith(SpriteSetDef.ArtRoot + "/")) return null;

            string file = System.IO.Path.GetFileNameWithoutExtension(norm);
            string dir = System.IO.Path.GetDirectoryName(norm).Replace('\\', '/');

            SpriteSetDef best = null;
            int bestLen = -1;
            foreach (SpriteSetDef s in m.sets)
            {
                string root = SpriteSetDef.ArtRoot + "/" + s.folder;
                if (!dir.StartsWith(root)) continue;
                if (!file.StartsWith(s.name)) continue;
                bool boundary = file.Length == s.name.Length || file[s.name.Length] == '_';
                if (!boundary) continue;
                if (s.name.Length > bestLen) { best = s; bestLen = s.name.Length; }
            }
            return best;
        }
    }
}
