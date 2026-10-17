using System.IO;
using System.Text;
using Shatranj.Data;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools.Art
{
    /// <summary>
    /// Menu: <c>Shatranj/Generate Placeholder Art</c>. Writes a labelled placeholder PNG for every frame of
    /// every set in the manifest, at the exact canvas size and path (Docs/06). Existing files are never touched
    /// (CLAUDE.md §2.6), so artists can overwrite any file and re-run safely.
    /// </summary>
    public static class GeneratePlaceholderArt
    {
        [MenuItem("Shatranj/Generate Placeholder Art", false, 30)]
        public static void Run()
        {
            ArtManifest manifest = ManifestIO.LoadOrGenerate();
            var report = new StringBuilder();
            report.AppendLine("=== Shatranj / Generate Placeholder Art ===");

            int created = 0, skipped = 0, failed = 0;
            int total = ManifestIO.CountFrames(manifest);
            int done = 0;

            try
            {
                foreach (SpriteSetDef set in manifest.sets)
                {
                    int setCreated = 0, setSkipped = 0;
                    foreach (AnimDef anim in set.anims)
                    {
                        int frames = Mathf.Max(1, anim.frames);
                        for (int i = 0; i < frames; i++)
                        {
                            done++;
                            string path = set.FramePath(anim, i);
                            if (File.Exists(path)) { skipped++; setSkipped++; continue; }

                            if (EditorUtility.DisplayCancelableProgressBar("Shatranj — placeholder art", path, done / (float)total))
                                throw new System.OperationCanceledException();

                            try
                            {
                                string dir = Path.GetDirectoryName(path);
                                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                                File.WriteAllBytes(path, PlaceholderPainter.Paint(set, anim, i));
                                created++;
                                setCreated++;
                            }
                            catch (System.Exception e)
                            {
                                failed++;
                                report.Append("  FAILED ").Append(path).Append(": ").AppendLine(e.Message);
                            }
                        }
                    }
                    if (setCreated > 0)
                        report.Append("  ").Append(set.name).Append(": ").Append(setCreated).Append(" created")
                              .Append(setSkipped > 0 ? $", {setSkipped} kept" : "").AppendLine();
                }
            }
            catch (System.OperationCanceledException)
            {
                report.AppendLine("Cancelled by user.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // One refresh: the AssetPostprocessor applies import settings (PPU, pivot, filter…) on import.
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            report.Append("Done: ").Append(created).Append(" created, ").Append(skipped).Append(" kept (existing art is never overwritten)");
            if (failed > 0) report.Append(", ").Append(failed).Append(" FAILED");
            report.AppendLine(".");
            report.AppendLine("Next: Shatranj → Import & Rebuild Animations.");

            if (failed > 0) Debug.LogError(report.ToString()); else Debug.Log(report.ToString());
        }
    }
}
