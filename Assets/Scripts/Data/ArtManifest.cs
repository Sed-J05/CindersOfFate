using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shatranj.Data
{
    /// <summary>One animation of a sprite set: frame count, fps, loop. fps 0 + 1 frame = static image.</summary>
    [Serializable]
    public class AnimDef
    {
        public string name = "";
        public int frames = 1;
        public float fps = 0f;
        public bool loop = false;
        public bool holdLast = false;

        public AnimDef() { }

        public AnimDef(string name, int frames, float fps, bool loop = false, bool holdLast = false)
        {
            this.name = name;
            this.frames = frames;
            this.fps = fps;
            this.loop = loop;
            this.holdLast = holdLast;
        }

        public bool IsStatic => frames <= 1 && fps <= 0f;
    }

    /// <summary>
    /// One character / FX / prop / image group from Docs/06. <see cref="layout"/> decides the file naming:
    /// "character" → Art/{folder}/{Name}/{Anim}/{Name}_{Anim}_NN.png ·
    /// "set" → Art/{folder}/{Name}/{Name}[_{Anim}]_NN.png ·
    /// "flat" → Art/{folder}/{Name}[_{Anim}][_NN].png (a single static frame has no number).
    /// </summary>
    [Serializable]
    public class SpriteSetDef
    {
        public string name = "";
        public string folder = "Characters";
        public string layout = "character";
        public string pivot = "bottom";
        public string style = "";
        public int width = 128;
        public int height = 128;
        public List<AnimDef> anims = new List<AnimDef>();

        public const string ArtRoot = "Assets/Art";

        public Vector2 PivotVector => ArtPivot.ToVector(pivot);
        public bool IsAnimated => layout != "flat";

        public string FolderPath(AnimDef anim)
        {
            string root = ArtRoot + "/" + folder;
            switch (layout)
            {
                case "character": return $"{root}/{name}/{anim.name}";
                case "set": return $"{root}/{name}";
                default: return root;
            }
        }

        /// <summary>Asset path of frame <paramref name="index"/> of <paramref name="anim"/>.</summary>
        public string FramePath(AnimDef anim, int index)
        {
            string dir = FolderPath(anim);
            bool hasAnimName = !string.IsNullOrEmpty(anim.name);
            string stem = hasAnimName ? $"{name}_{anim.name}" : name;

            if (layout == "flat" && !hasAnimName && anim.IsStatic)
                return $"{dir}/{stem}.png";

            return $"{dir}/{stem}_{index:00}.png";
        }

        /// <summary>File-name stem (no number, no extension) of an animation.</summary>
        public string Stem(AnimDef anim) => string.IsNullOrEmpty(anim.name) ? name : $"{name}_{anim.name}";

        public AnimDef FindAnim(string animName)
        {
            for (int i = 0; i < anims.Count; i++)
                if (anims[i].name == animName) return anims[i];
            return null;
        }

        public int TotalFrames
        {
            get { int n = 0; for (int i = 0; i < anims.Count; i++) n += anims[i].frames; return n; }
        }
    }

    /// <summary>Assets/Data/AnimationManifest.json — the human-editable art spec (Docs/06).</summary>
    [Serializable]
    public class ArtManifest
    {
        public int version = 1;
        public int pixelsPerUnit = 100;
        public List<SpriteSetDef> sets = new List<SpriteSetDef>();

        public SpriteSetDef Find(string setName)
        {
            for (int i = 0; i < sets.Count; i++)
                if (sets[i].name == setName) return sets[i];
            return null;
        }
    }

    /// <summary>Pivot keywords used in the manifest.</summary>
    public static class ArtPivot
    {
        public const string Bottom = "bottom";
        public const string Center = "center";
        public const string BottomLeft = "bottomLeft";
        public const string LeftCenter = "leftCenter";

        public static Vector2 ToVector(string pivot)
        {
            switch (pivot)
            {
                case Bottom: return new Vector2(0.5f, 0f);
                case BottomLeft: return new Vector2(0f, 0f);
                case LeftCenter: return new Vector2(0f, 0.5f);
                default: return new Vector2(0.5f, 0.5f);
            }
        }
    }
}
