using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Runtime-generated 1×1 unit sprites for FX and projectiles until real art arrives (Prompt 4).
    /// Never saved; created lazily per play session.
    /// </summary>
    public static class PlaceholderSprites
    {
        private static Sprite square;
        private static Sprite circle;

        /// <summary>White 1×1 unit square, pivot centre.</summary>
        public static Sprite Square
        {
            get
            {
                if (square == null) square = Make(8, false);
                return square;
            }
        }

        /// <summary>White 1×1 unit disc with soft edge, pivot centre.</summary>
        public static Sprite Circle
        {
            get
            {
                if (circle == null) circle = Make(32, true);
                return circle;
            }
        }

        private static Sprite Make(int size, bool disc)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };

            var pixels = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    byte a = 255;
                    if (disc)
                    {
                        float dx = x + 0.5f - r, dy = y + 0.5f - r;
                        float d = Mathf.Sqrt(dx * dx + dy * dy) / r;
                        a = (byte)(255f * Mathf.Clamp01((1f - d) * size * 0.5f));
                    }
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.hideFlags = HideFlags.DontSave;
            sprite.name = disc ? "PlaceholderCircle" : "PlaceholderSquare";
            return sprite;
        }
    }
}
