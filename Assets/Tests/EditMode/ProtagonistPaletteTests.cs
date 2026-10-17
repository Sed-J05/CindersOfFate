using NUnit.Framework;
using Shatranj.Data;
using Shatranj.EditorTools;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Tests
{
    /// <summary>D-154: Phase 2 protagonist reads black/red; Phase 1 is drawn as painted; Phase 2 paper is grey-blue.</summary>
    public class ProtagonistPaletteTests
    {
        private const float Eps = 0.01f;

        private static ThemeConfig Theme(System.Action<ThemeConfig> fill)
        {
            var t = ScriptableObject.CreateInstance<ThemeConfig>();
            fill(t);
            return t;
        }

        private static void AssertColor(Color expected, Color actual, string label)
        {
            Assert.AreEqual(expected.r, actual.r, Eps, label + " r");
            Assert.AreEqual(expected.g, actual.g, Eps, label + " g");
            Assert.AreEqual(expected.b, actual.b, Eps, label + " b");
        }

        [Test]
        public void Phase2_PaintedPalette_BecomesBlackAndRed()
        {
            ThemeConfig t = Theme(ConfigDefaults.ThemePhase2Inverted);
            Assert.AreEqual(1f, t.protagonistRecolor);
            AssertColor(t.protagonistRecolorLight, ProtagonistPalette.Recolor(t.protagonistWhite, t), "white body -> black");
            AssertColor(t.protagonistRecolorMid, ProtagonistPalette.Recolor(t.protagonistBlue, t), "blue accent -> red");
            AssertColor(t.protagonistRecolorDark, ProtagonistPalette.Recolor(t.protagonistDeepBlue, t), "deep blue -> deep red");

            Color body = ProtagonistPalette.Recolor(t.protagonistWhite, t);
            Assert.Less(ProtagonistPalette.Luminance(body), 0.1f, "body is near black");
            Color accent = ProtagonistPalette.Recolor(t.protagonistBlue, t);
            Assert.Greater(accent.r, accent.g * 3f, "accent is red");
            Assert.Greater(accent.r, accent.b * 3f, "accent is red");
            Object.DestroyImmediate(t);
        }

        [Test]
        public void Recolor_KeepsAlpha_AndPhase1IsUnchanged()
        {
            ThemeConfig p1 = Theme(ConfigDefaults.ThemePhase1Beige);
            Assert.AreEqual(0f, p1.protagonistRecolor);
            Color painted = new Color(0.31f, 0.64f, 1f, 0.5f);
            Assert.AreEqual(painted, ProtagonistPalette.Recolor(painted, p1));

            ThemeConfig p2 = Theme(ConfigDefaults.ThemePhase2Inverted);
            Assert.AreEqual(0.5f, ProtagonistPalette.Recolor(painted, p2).a, 1e-5f);
            Object.DestroyImmediate(p1);
            Object.DestroyImmediate(p2);
        }

        [Test]
        public void SourceLuminance_IsStrictlyIncreasing()
        {
            ThemeConfig t = Theme(ConfigDefaults.ThemePhase2Inverted);
            Vector4 s = ProtagonistPalette.SourceLuminance(t);
            Assert.Less(s.x, s.y);
            Assert.Less(s.y, s.z);
            t.protagonistBlue = t.protagonistDeepBlue;   // degenerate palette still yields a valid ramp
            s = ProtagonistPalette.SourceLuminance(t);
            Assert.Less(s.x, s.y);
            Object.DestroyImmediate(t);
        }

        [Test]
        public void Phase2Paper_IsGreyBlue_NotDarkNavy()
        {
            ThemeConfig t = Theme(ConfigDefaults.ThemePhase2Inverted);
            Color paper = t.parchmentBase;
            float lum = ProtagonistPalette.Luminance(paper);
            Assert.Greater(lum, 0.4f, "mid-tone, not dark navy");
            Assert.Less(lum, 0.7f, "still darker than the white/blue enemies");
            Assert.Greater(paper.b, paper.r, "blue-ish");
            Assert.Less(paper.b - paper.r, 0.2f, "grey, not saturated blue");

            // The beige texture times the tint should land near the same grey-blue.
            Color beige = ThemeConfig.Hex("#E8D9B5");
            Color tinted = beige * t.parchmentSpriteTint;
            AssertColor(paper, tinted, "tinted beige texture");
            Assert.AreEqual(ConfigDefaults.ThemeRevision, t.defaultsRevision);
            Object.DestroyImmediate(t);
        }
    }
}
