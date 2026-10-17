using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Publishes the protagonist recolour (D-154) as global shader properties read by Shatranj/SpriteProtagonistLit.
    /// ThemeManager calls <see cref="Apply"/> with the two themes it is blending, so the protagonist changes colour
    /// together with the rest of the stage (inversion wipe, F11 theme cycling).
    /// </summary>
    public static class ProtagonistPalette
    {
        public static readonly int RecolorId = Shader.PropertyToID("_ShatranjProtagRecolor");
        public static readonly int SourceLumId = Shader.PropertyToID("_ShatranjProtagSrcLum");
        public static readonly int DarkId = Shader.PropertyToID("_ShatranjProtagDark");
        public static readonly int MidId = Shader.PropertyToID("_ShatranjProtagMid");
        public static readonly int LightId = Shader.PropertyToID("_ShatranjProtagLight");

        /// <summary>Rec. 601 luminance, the same weights the shader uses.</summary>
        public static float Luminance(Color c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;

        /// <summary>Painted-palette luminances (deep blue, blue, white), kept strictly increasing so the ramp is valid.</summary>
        public static Vector4 SourceLuminance(ThemeConfig t)
        {
            float dark = Luminance(t.protagonistDeepBlue);
            float mid = Mathf.Max(Luminance(t.protagonistBlue), dark + 0.01f);
            float light = Mathf.Max(Luminance(t.protagonistWhite), mid + 0.01f);
            return new Vector4(dark, mid, light, 0f);
        }

        /// <summary>Recolour target for a painted colour, mirroring the shader (used by tests and tools).</summary>
        public static Color Recolor(Color painted, ThemeConfig t)
        {
            if (t.protagonistRecolor <= 0f) return painted;
            Vector4 s = SourceLuminance(t);
            float lum = Luminance(painted);
            Color r = lum <= s.y
                ? Color.Lerp(t.protagonistRecolorDark, t.protagonistRecolorMid, Mathf.InverseLerp(s.x, s.y, lum))
                : Color.Lerp(t.protagonistRecolorMid, t.protagonistRecolorLight, Mathf.InverseLerp(s.y, s.z, lum));
            Color result = Color.Lerp(painted, r, Mathf.Clamp01(t.protagonistRecolor));
            result.a = painted.a;
            return result;
        }

        /// <summary>Back to the painted colours (no theme active).</summary>
        public static void Reset() => Shader.SetGlobalFloat(RecolorId, 0f);

        public static void Apply(ThemeConfig a, ThemeConfig b, float k)
        {
            if (a == null || b == null) return;
            // Hold the target palette for the whole fade so only the amount blends (no muddy colour in between).
            ThemeConfig palette = b.protagonistRecolor > 0f || a.protagonistRecolor <= 0f ? b : a;
            Shader.SetGlobalFloat(RecolorId, Mathf.Lerp(a.protagonistRecolor, b.protagonistRecolor, k));
            Shader.SetGlobalVector(SourceLumId, SourceLuminance(palette));
            Shader.SetGlobalColor(DarkId, palette.protagonistRecolorDark);
            Shader.SetGlobalColor(MidId, palette.protagonistRecolorMid);
            Shader.SetGlobalColor(LightId, palette.protagonistRecolorLight);
        }
    }
}
