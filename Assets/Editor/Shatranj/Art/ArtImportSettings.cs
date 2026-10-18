using Shatranj.Data;
using UnityEditor;
using UnityEngine;

namespace Shatranj.EditorTools.Art
{
    /// <summary>
    /// Import rules for everything under Assets/Art (Docs/06 §1): PPU 100, single sprite, pivot from the
    /// manifest, filter from ArtStyleConfig, no compression, no mipmaps, Full Rect mesh. Backgrounds repeat.
    /// Applied automatically by <see cref="ShatranjSpriteImporter"/> and in bulk by Import &amp; Rebuild Animations.
    /// </summary>
    public static class ArtImportSettings
    {
        public const string ManagedRoot = "Assets/Art/";
        public const string ExcludedRoot = "Assets/Art/Placeholders/";

        public static bool ShouldManage(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return false;
            string p = assetPath.Replace('\\', '/');
            if (!p.StartsWith(ManagedRoot)) return false;
            if (p.StartsWith(ExcludedRoot)) return false;
            return p.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase);
        }

        private static ArtStyleConfig cachedStyle;

        public static bool UsePointFilter()
        {
            if (cachedStyle == null) cachedStyle = AssetDatabase.LoadAssetAtPath<ArtStyleConfig>(GenerateConfigs.ArtStylePath);
            return cachedStyle != null && cachedStyle.usePointFilter;
        }

        public static void InvalidateCache() => cachedStyle = null;

        /// <summary>Applies the rules. Returns true when anything changed (caller decides whether to reimport).</summary>
        public static bool Apply(TextureImporter importer, string assetPath, ArtManifest manifest)
        {
            bool changed = false;
            SpriteSetDef set = ManifestIO.FindSetForPath(manifest, assetPath);
            Vector2 pivot = set != null ? set.PivotVector : new Vector2(0.5f, 0.5f);
            bool repeat = set != null && (set.style == "parchment" || set.style == "parchment_inv" || set.style.StartsWith("bg_"));
            // Pixel sprites go crisp with the point filter; painted backdrops, cutscene panels and glows stay smooth.
            bool painted = set != null && (set.folder == "Backgrounds" || (set.folder == "Cutscenes" && set.style != "god") || set.style == "ui_soul_glow");
            FilterMode filter = UsePointFilter() && !painted ? FilterMode.Point : FilterMode.Bilinear;
            int maxSize = Mathf.Max(importer.maxTextureSize, set != null ? Mathf.NextPowerOfTwo(Mathf.Max(set.width, set.height)) : 2048);
            maxSize = Mathf.Clamp(maxSize, 2048, 8192);

            if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
            if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
            if (!Mathf.Approximately(importer.spritePixelsPerUnit, manifest != null ? manifest.pixelsPerUnit : 100)) { importer.spritePixelsPerUnit = manifest != null ? manifest.pixelsPerUnit : 100; changed = true; }
            if (importer.filterMode != filter) { importer.filterMode = filter; changed = true; }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
            if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
            if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
            if (importer.npotScale != TextureImporterNPOTScale.None) { importer.npotScale = TextureImporterNPOTScale.None; changed = true; }
            if (importer.maxTextureSize != maxSize) { importer.maxTextureSize = maxSize; changed = true; }
            TextureWrapMode wrap = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            if (importer.wrapMode != wrap) { importer.wrapMode = wrap; changed = true; }

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            bool settingsChanged = false;
            if (settings.spriteMeshType != SpriteMeshType.FullRect) { settings.spriteMeshType = SpriteMeshType.FullRect; settingsChanged = true; }
            if (settings.spriteAlignment != (int)SpriteAlignment.Custom) { settings.spriteAlignment = (int)SpriteAlignment.Custom; settingsChanged = true; }
            if ((settings.spritePivot - pivot).sqrMagnitude > 1e-6f) { settings.spritePivot = pivot; settingsChanged = true; }
            if (settings.spriteGenerateFallbackPhysicsShape) { settings.spriteGenerateFallbackPhysicsShape = false; settingsChanged = true; }
            if (settingsChanged) { importer.SetTextureSettings(settings); changed = true; }

            return changed;
        }
    }

    /// <summary>Applies <see cref="ArtImportSettings"/> to every texture imported under Assets/Art.</summary>
    public class ShatranjSpriteImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!ArtImportSettings.ShouldManage(assetPath)) return;
            var importer = assetImporter as TextureImporter;
            if (importer == null) return;
            ArtImportSettings.Apply(importer, assetPath, ManifestIO.Load());
        }
    }
}
