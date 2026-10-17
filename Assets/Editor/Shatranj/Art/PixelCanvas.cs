using UnityEngine;

namespace Shatranj.EditorTools.Art
{
    /// <summary>
    /// Minimal software rasteriser for placeholder PNGs. Y grows upward (row 0 = bottom), matching Unity.
    /// All fills alpha-blend over the existing pixel.
    /// </summary>
    public sealed class PixelCanvas
    {
        public readonly int W;
        public readonly int H;
        private readonly Color32[] px;

        public PixelCanvas(int width, int height)
        {
            W = Mathf.Max(1, width);
            H = Mathf.Max(1, height);
            px = new Color32[W * H];
        }

        public void Clear(Color32 c)
        {
            for (int i = 0; i < px.Length; i++) px[i] = c;
        }

        public Color32 Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return default;
            return px[y * W + x];
        }

        public void Set(int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            px[y * W + x] = c;
        }

        /// <summary>Alpha-over blend.</summary>
        public void Blend(int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H || c.a == 0) return;
            int i = y * W + x;
            if (c.a == 255) { px[i] = c; return; }
            Color32 d = px[i];
            float a = c.a / 255f;
            float da = d.a / 255f;
            float outA = a + da * (1f - a);
            if (outA <= 0f) { px[i] = default; return; }
            float r = (c.r * a + d.r * da * (1f - a)) / outA;
            float g = (c.g * a + d.g * da * (1f - a)) / outA;
            float b = (c.b * a + d.b * da * (1f - a)) / outA;
            px[i] = new Color32((byte)r, (byte)g, (byte)b, (byte)(outA * 255f));
        }

        public void FillRect(int x, int y, int w, int h, Color32 c)
        {
            int x0 = Mathf.Max(0, x), y0 = Mathf.Max(0, y);
            int x1 = Mathf.Min(W, x + w), y1 = Mathf.Min(H, y + h);
            for (int yy = y0; yy < y1; yy++)
                for (int xx = x0; xx < x1; xx++) Blend(xx, yy, c);
        }

        public void FillRoundRect(int x, int y, int w, int h, int r, Color32 c)
        {
            r = Mathf.Clamp(r, 0, Mathf.Min(w, h) / 2);
            for (int yy = y; yy < y + h; yy++)
            {
                for (int xx = x; xx < x + w; xx++)
                {
                    int dx = 0, dy = 0;
                    if (xx < x + r) dx = x + r - xx; else if (xx >= x + w - r) dx = xx - (x + w - r - 1);
                    if (yy < y + r) dy = y + r - yy; else if (yy >= y + h - r) dy = yy - (y + h - r - 1);
                    if (dx * dx + dy * dy <= r * r) Blend(xx, yy, c);
                }
            }
        }

        public void FillCircle(int cx, int cy, int r, Color32 c) => FillEllipse(cx, cy, r, r, c);

        public void FillEllipse(int cx, int cy, int rx, int ry, Color32 c)
        {
            if (rx <= 0 || ry <= 0) return;
            for (int yy = cy - ry; yy <= cy + ry; yy++)
            {
                for (int xx = cx - rx; xx <= cx + rx; xx++)
                {
                    float nx = (xx - cx) / (float)rx, ny = (yy - cy) / (float)ry;
                    if (nx * nx + ny * ny <= 1f) Blend(xx, yy, c);
                }
            }
        }

        /// <summary>Disc with a soft edge: full alpha inside r·(1-soft), fading to 0 at r.</summary>
        public void FillSoftCircle(int cx, int cy, int r, Color32 c, float soft)
        {
            if (r <= 0) return;
            soft = Mathf.Clamp01(soft);
            for (int yy = cy - r; yy <= cy + r; yy++)
            {
                for (int xx = cx - r; xx <= cx + r; xx++)
                {
                    float d = Mathf.Sqrt((xx - cx) * (xx - cx) + (yy - cy) * (yy - cy)) / r;
                    if (d > 1f) continue;
                    float k = soft <= 0f ? 1f : Mathf.Clamp01((1f - d) / soft);
                    Color32 cc = c;
                    cc.a = (byte)(c.a * k);
                    Blend(xx, yy, cc);
                }
            }
        }

        public void Ring(int cx, int cy, int rOuter, int rInner, Color32 c)
        {
            for (int yy = cy - rOuter; yy <= cy + rOuter; yy++)
            {
                for (int xx = cx - rOuter; xx <= cx + rOuter; xx++)
                {
                    int d2 = (xx - cx) * (xx - cx) + (yy - cy) * (yy - cy);
                    if (d2 <= rOuter * rOuter && d2 >= rInner * rInner) Blend(xx, yy, c);
                }
            }
        }

        /// <summary>Crescent = circle minus a second circle shifted by (ox, oy).</summary>
        public void FillCrescent(int cx, int cy, int r, int ox, int oy, Color32 c)
        {
            for (int yy = cy - r; yy <= cy + r; yy++)
            {
                for (int xx = cx - r; xx <= cx + r; xx++)
                {
                    int d2 = (xx - cx) * (xx - cx) + (yy - cy) * (yy - cy);
                    int e2 = (xx - cx - ox) * (xx - cx - ox) + (yy - cy - oy) * (yy - cy - oy);
                    if (d2 <= r * r && e2 > r * r) Blend(xx, yy, c);
                }
            }
        }

        public void FillTriangle(Vector2Int a, Vector2Int b, Vector2Int c, Color32 col)
        {
            int minX = Mathf.Max(0, Mathf.Min(a.x, Mathf.Min(b.x, c.x)));
            int maxX = Mathf.Min(W - 1, Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
            int minY = Mathf.Max(0, Mathf.Min(a.y, Mathf.Min(b.y, c.y)));
            int maxY = Mathf.Min(H - 1, Mathf.Max(a.y, Mathf.Max(b.y, c.y)));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int w0 = Edge(b, c, x, y), w1 = Edge(c, a, x, y), w2 = Edge(a, b, x, y);
                    if ((w0 >= 0 && w1 >= 0 && w2 >= 0) || (w0 <= 0 && w1 <= 0 && w2 <= 0)) Blend(x, y, col);
                }
            }
        }

        private static int Edge(Vector2Int a, Vector2Int b, int x, int y) => (b.x - a.x) * (y - a.y) - (b.y - a.y) * (x - a.x);

        public void Line(int x0, int y0, int x1, int y1, Color32 c, int thickness = 1)
        {
            int dx = Mathf.Abs(x1 - x0), dy = -Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            int half = thickness / 2;
            while (true)
            {
                if (thickness <= 1) Blend(x0, y0, c);
                else FillRect(x0 - half, y0 - half, thickness, thickness, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        /// <summary>Frame of <paramref name="thickness"/> pixels along the canvas edge.</summary>
        public void Border(int thickness, Color32 c)
        {
            FillRect(0, 0, W, thickness, c);
            FillRect(0, H - thickness, W, thickness, c);
            FillRect(0, 0, thickness, H, c);
            FillRect(W - thickness, 0, thickness, H, c);
        }

        /// <summary>Plus-shaped marker centred on the pivot.</summary>
        public void PivotMarker(int x, int y, int size, Color32 c)
        {
            int h = size / 2;
            FillRect(x - h, y, size, 1, c);
            FillRect(x, y - h, 1, size, c);
            Set(x, y, c);
        }

        /// <summary>Draws text with the pixel font; (x, yTop) is the top-left corner.</summary>
        public void DrawText(string text, int x, int yTop, int scale, Color32 color, bool shadow = true)
        {
            if (string.IsNullOrEmpty(text)) return;
            scale = Mathf.Max(1, scale);
            Color32 shadowColor = new Color32(0, 0, 0, 200);
            int cx = x;
            foreach (char ch in text)
            {
                string[] rows = PixelFont.Get(ch);
                for (int r = 0; r < PixelFont.GlyphHeight; r++)
                {
                    for (int cI = 0; cI < PixelFont.GlyphWidth; cI++)
                    {
                        if (rows[r][cI] != '#') continue;
                        int px0 = cx + cI * scale;
                        int py0 = yTop - (r + 1) * scale;
                        if (shadow) FillRect(px0 + scale, py0 - scale, scale, scale, shadowColor);
                        FillRect(px0, py0, scale, scale, color);
                    }
                }
                cx += PixelFont.Advance * scale;
            }
        }

        public byte[] ToPng()
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.SetPixels32(px);
            tex.Apply(false, false);
            byte[] bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            return bytes;
        }

        // --- helpers ---
        public static Color32 Hex(string hex, byte alpha = 255)
        {
            Color c = ColorUtility.TryParseHtmlString(hex, out Color parsed) ? parsed : Color.magenta;
            return new Color32((byte)(c.r * 255f), (byte)(c.g * 255f), (byte)(c.b * 255f), alpha);
        }

        public static Color32 WithAlpha(Color32 c, byte a) { c.a = a; return c; }

        public static Color32 Lerp(Color32 a, Color32 b, float t) => Color32.Lerp(a, b, Mathf.Clamp01(t));

        /// <summary>Deterministic hash noise in [0,1).</summary>
        public static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393) ^ (uint)(y * 668265263) ^ (uint)(seed * 2147483647);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777216f;
            }
        }

        /// <summary>Seamless value noise using integer periods so it tiles at (W, H).</summary>
        public static float TileNoise(int x, int y, int w, int h, int cells, int seed)
        {
            float fx = x / (float)w * cells, fy = y / (float)h * cells;
            int x0 = Mathf.FloorToInt(fx), y0 = Mathf.FloorToInt(fy);
            float tx = fx - x0, ty = fy - y0;
            tx = tx * tx * (3f - 2f * tx);
            ty = ty * ty * (3f - 2f * ty);
            float n00 = Hash(x0 % cells, y0 % cells, seed);
            float n10 = Hash((x0 + 1) % cells, y0 % cells, seed);
            float n01 = Hash(x0 % cells, (y0 + 1) % cells, seed);
            float n11 = Hash((x0 + 1) % cells, (y0 + 1) % cells, seed);
            return Mathf.Lerp(Mathf.Lerp(n00, n10, tx), Mathf.Lerp(n01, n11, tx), ty);
        }
    }
}
