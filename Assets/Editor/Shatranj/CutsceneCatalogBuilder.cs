using System.IO;
using System.Text;
using Shatranj.Data;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Menu: <c>Shatranj/Build Cutscene Catalog</c>. Fills Assets/Resources/SpriteCatalog.asset with every flat sprite
    /// in Art/Cutscenes, Art/UI and Art/Backgrounds (by file name) and every cutscene JSON in Data/Cutscenes, plus the
    /// Sprite-Unlit material. Updated in place (GUID kept). Run by Build All Scenes.
    /// </summary>
    public static class CutsceneCatalogBuilder
    {
        public const string CatalogPath = "Assets/Resources/" + SpriteCatalog.ResourcePath + ".asset";
        public const string CutsceneFolder = "Assets/" + SpriteCatalog.CutsceneFolder;
        private static readonly string[] SpriteFolders = { "Assets/Art/Cutscenes", "Assets/Art/UI", "Assets/Art/Backgrounds" };

        [MenuItem("Shatranj/Build Cutscene Catalog", false, 79)]
        public static void Run() => Debug.Log(Build(new StringBuilder("=== Shatranj / Build Cutscene Catalog ===\n")).ToString());

        public static StringBuilder Build(StringBuilder report)
        {
            GenerateConfigs.EnsureFolder("Assets/Resources");
            GenerateConfigs.EnsureFolder(CutsceneFolder);
            var cat = AssetDatabase.LoadAssetAtPath<SpriteCatalog>(CatalogPath);
            if (cat == null)
            {
                cat = ScriptableObject.CreateInstance<SpriteCatalog>();
                AssetDatabase.CreateAsset(cat, CatalogPath);
            }

            cat.sprites.Clear();
            foreach (string folder in SpriteFolders)
            {
                if (!Directory.Exists(folder)) continue;
                foreach (string file in Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
                {
                    string path = file.Replace('\\', '/');
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null) cat.sprites.Add(new SpriteCatalog.SpriteEntry { name = Path.GetFileNameWithoutExtension(path), sprite = sprite });
                }
            }

            cat.cutscenes.Clear();
            foreach (string file in Directory.GetFiles(CutsceneFolder, "*.json", SearchOption.TopDirectoryOnly))
            {
                string path = file.Replace('\\', '/');
                var text = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (text != null) cat.cutscenes.Add(new SpriteCatalog.ScriptEntry { id = Path.GetFileNameWithoutExtension(path), json = text });
            }

            cat.unlitMaterial = SceneBuildUtil.SpriteUnlitMaterial();
            cat.InvalidateLookup();
            EditorUtility.SetDirty(cat);
            AssetDatabase.SaveAssets();
            report.Append("SpriteCatalog: ").Append(cat.sprites.Count).Append(" sprites, ").Append(cat.cutscenes.Count).AppendLine(" cutscene script(s).");
            return report;
        }
    }
}
