using System.Collections.Generic;
using System.IO;
using System.Text;
using Shatranj.Data;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools.Art
{
    /// <summary>
    /// Menu: <c>Shatranj/Import &amp; Rebuild Animations</c>.
    /// 1) Re-applies import settings to every PNG under Assets/Art that drifted.
    /// 2) Builds / updates one <see cref="SpriteAnimationSet"/> per animated set under Assets/Resources/Animations,
    ///    reading the frames that actually exist on disk (so a changed frame count just works).
    /// Existing set assets are updated in place, so their GUIDs and every reference to them survive.
    /// </summary>
    public static class RebuildAnimations
    {
        public const string SetsFolder = "Assets/Resources/" + SpriteAnimationSet.ResourcesFolder;
        private const int MaxFramesScan = 128;

        public static string SetPath(string setName) => $"{SetsFolder}/{setName}.asset";

        [MenuItem("Shatranj/Import & Rebuild Animations", false, 31)]
        public static void Run()
        {
            ArtManifest manifest = ManifestIO.LoadOrGenerate();
            ArtImportSettings.InvalidateCache();
            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Import & Rebuild Animations ===");

            try
            {
                int reimported = ReimportDrifted(manifest, report);
                report.Append("Import settings: ").Append(reimported).AppendLine(" texture(s) corrected.");

                GenerateConfigs.EnsureFolder(SetsFolder);
                int built = 0, skipped = 0;
                var problems = new List<string>();
                int index = 0;
                foreach (SpriteSetDef set in manifest.sets)
                {
                    index++;
                    if (!set.IsAnimated) continue;
                    EditorUtility.DisplayProgressBar("Shatranj — rebuilding animations", set.name, index / (float)manifest.sets.Count);
                    if (BuildSet(set, report, problems)) built++; else skipped++;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                report.Append("Animation sets: ").Append(built).Append(" built/updated, ").Append(skipped).AppendLine(" skipped (no frames on disk).");
                if (problems.Count > 0)
                {
                    report.Append(problems.Count).AppendLine(" warning(s):");
                    foreach (string p in problems) report.Append("  ! ").AppendLine(p);
                }
                report.AppendLine("Sets live in Assets/Resources/Animations and are loaded by name at runtime (SpriteAnimationSet.Load).");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log(report.ToString());
        }

        private static int ReimportDrifted(ArtManifest manifest, StringBuilder report)
        {
            if (!Directory.Exists(SpriteSetDef.ArtRoot)) return 0;
            string[] files = Directory.GetFiles(SpriteSetDef.ArtRoot, "*.png", SearchOption.AllDirectories);
            int count = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < files.Length; i++)
                {
                    string path = files[i].Replace('\\', '/');
                    if (!ArtImportSettings.ShouldManage(path)) continue;
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;
                    if (ArtImportSettings.Apply(importer, path, manifest))
                    {
                        importer.SaveAndReimport();
                        count++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            if (count > 0) AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return count;
        }

        /// <summary>Builds or updates the set asset. Returns false when no frame of any clip exists.</summary>
        private static bool BuildSet(SpriteSetDef set, StringBuilder report, List<string> problems)
        {
            var clips = new List<SpriteClip>();
            int totalFrames = 0;

            foreach (AnimDef anim in set.anims)
            {
                List<Sprite> frames = LoadFrames(set, anim, out int missingInside);
                if (frames.Count == 0)
                {
                    problems.Add($"{set.name}/{anim.name}: no frames found ({set.FramePath(anim, 0)})");
                    continue;
                }
                if (missingInside > 0) problems.Add($"{set.name}/{anim.name}: {missingInside} missing frame(s) inside the sequence");
                if (frames.Count != anim.frames) problems.Add($"{set.name}/{anim.name}: manifest says {anim.frames} frames, disk has {frames.Count} (using disk)");

                clips.Add(new SpriteClip
                {
                    name = string.IsNullOrEmpty(anim.name) ? "Default" : anim.name,
                    frames = frames.ToArray(),
                    fps = anim.fps > 0f ? anim.fps : 1f,
                    loop = anim.loop,
                    holdLast = true,
                });
                totalFrames += frames.Count;
            }

            if (clips.Count == 0) return false;

            string path = SetPath(set.name);
            var asset = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(path);
            bool created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SpriteAnimationSet>();
                asset.setName = set.name;
                AssetDatabase.CreateAsset(asset, path);
                created = true;
            }
            asset.setName = set.name;
            asset.clips = clips;
            EditorUtility.SetDirty(asset);

            report.Append("  ").Append(created ? "create " : "update ").Append(set.name).Append(": ")
                  .Append(clips.Count).Append(" clip(s), ").Append(totalFrames).AppendLine(" frame(s)");
            return true;
        }

        /// <summary>Loads consecutive frames from disk; stops at the first gap after at least one frame beyond the manifest count.</summary>
        private static List<Sprite> LoadFrames(SpriteSetDef set, AnimDef anim, out int missingInside)
        {
            var frames = new List<Sprite>();
            missingInside = 0;
            int expected = Mathf.Max(1, anim.frames);

            for (int i = 0; i < MaxFramesScan; i++)
            {
                string path = set.FramePath(anim, i);
                if (!File.Exists(path))
                {
                    if (i >= expected) break;      // past the manifest count and nothing more on disk
                    if (frames.Count > 0) missingInside++;
                    continue;
                }
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
                if (sprite != null) frames.Add(sprite);
                else missingInside++;
            }
            return frames;
        }
    }
}
