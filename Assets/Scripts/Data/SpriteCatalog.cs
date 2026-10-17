using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shatranj.Data
{
    /// <summary>
    /// Runtime lookup for art that scripts reference by name (cutscene images, title logo, UI button sprites) and for
    /// the cutscene JSON files. Lives at Resources/SpriteCatalog; rebuilt by Shatranj/Build Cutscene Catalog (also run
    /// by Build All Scenes). In the Editor, cutscene JSON is read straight from Assets/Data/Cutscenes so edits apply
    /// on the next play without reimporting (Docs/08 §9).
    /// </summary>
    [CreateAssetMenu(menuName = "Shatranj/Sprite Catalog", fileName = "SpriteCatalog")]
    public class SpriteCatalog : ScriptableObject
    {
        [Serializable]
        public class SpriteEntry { public string name; public Sprite sprite; }

        [Serializable]
        public class ScriptEntry { public string id; public TextAsset json; }

        public List<SpriteEntry> sprites = new List<SpriteEntry>();
        public List<ScriptEntry> cutscenes = new List<ScriptEntry>();
        [Tooltip("Sprite-Unlit-Default, for full-screen cutscene images that should show their true colours.")]
        public Material unlitMaterial;

        public const string ResourcePath = "SpriteCatalog";
        public const string CutsceneFolder = "Data/Cutscenes";

        private static SpriteCatalog cached;
        private Dictionary<string, Sprite> lookup;

        public static SpriteCatalog Instance
        {
            get
            {
                if (cached == null) cached = Resources.Load<SpriteCatalog>(ResourcePath);
                return cached;
            }
        }

        public Sprite Find(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;
            if (lookup == null)
            {
                lookup = new Dictionary<string, Sprite>();
                foreach (SpriteEntry e in sprites) if (e != null && e.sprite != null && !string.IsNullOrEmpty(e.name)) lookup[e.name] = e.sprite;
            }
            return lookup.TryGetValue(spriteName, out Sprite s) ? s : null;
        }

        /// <summary>Cutscene JSON text: the file on disk in the Editor (live edits), the catalogued TextAsset in builds.</summary>
        public string LoadCutsceneText(string id)
        {
#if UNITY_EDITOR
            string path = System.IO.Path.Combine(Application.dataPath, CutsceneFolder, id + ".json");
            if (System.IO.File.Exists(path)) return System.IO.File.ReadAllText(path);
#endif
            foreach (ScriptEntry e in cutscenes)
                if (e != null && e.id == id && e.json != null) return e.json.text;
            return null;
        }

        public void InvalidateLookup() => lookup = null;
    }
}
