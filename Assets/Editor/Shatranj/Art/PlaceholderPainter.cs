using Shatranj.Data;
using UnityEngine;

namespace Shatranj.EditorTools.Art
{
    /// <summary>
    /// Draws one placeholder frame per Docs/06 §9: silhouette shape by style, palette by character,
    /// label (name + anim + frame), 2 px edge outline, pivot marker, subtle per-frame motion.
    /// </summary>
    public static class PlaceholderPainter
    {
        // Palettes (Docs/07 §2)
        private static readonly Color32 ProtWhite = PixelCanvas.Hex("#EAF6FF");
        private static readonly Color32 ProtBlue = PixelCanvas.Hex("#4FA3FF");
        private static readonly Color32 ProtDeep = PixelCanvas.Hex("#1B4F9C");
        private static readonly Color32 EnemyBlack = PixelCanvas.Hex("#16090A");
        private static readonly Color32 EnemyRed = PixelCanvas.Hex("#C1272D");
        private static readonly Color32 EnemyRedGlow = PixelCanvas.Hex("#FF4B3E");
        private static readonly Color32 EnemyWhite = PixelCanvas.Hex("#F2F8FF");
        private static readonly Color32 EnemyBlue = PixelCanvas.Hex("#6EC1FF");
        private static readonly Color32 EnemyDeepBlue = PixelCanvas.Hex("#2C6FD1");
        private static readonly Color32 ZulBlack = PixelCanvas.Hex("#0A0305");
        private static readonly Color32 ZulCrimson = PixelCanvas.Hex("#FF2A3A");
        private static readonly Color32 EnvDark = PixelCanvas.Hex("#26262A");
        private static readonly Color32 EnvMid = PixelCanvas.Hex("#4A4A52");
        private static readonly Color32 EnvAccent = PixelCanvas.Hex("#6B3A3A");
        private static readonly Color32 EnvLight = PixelCanvas.Hex("#D8DEE9");
        private static readonly Color32 EnvMidInv = PixelCanvas.Hex("#A7B0C4");
        private static readonly Color32 EnvAccentInv = PixelCanvas.Hex("#5E86A8");
        private static readonly Color32 ParchBeige = PixelCanvas.Hex("#E8D9B5");
        private static readonly Color32 ParchEdge = PixelCanvas.Hex("#9C8460");
        private static readonly Color32 ParchNavy = PixelCanvas.Hex("#0E1424");
        private static readonly Color32 FlameCore = PixelCanvas.Hex("#FFD27A");
        private static readonly Color32 FlameGlow = PixelCanvas.Hex("#FF8A2B");
        private static readonly Color32 Marker = new Color32(255, 0, 255, 255);
        private static readonly Color32 Label = new Color32(255, 255, 255, 255);

        public static byte[] Paint(SpriteSetDef set, AnimDef anim, int frame)
        {
            var c = new PixelCanvas(set.width, set.height);
            c.Clear(new Color32(0, 0, 0, 0));

            int frames = Mathf.Max(1, anim.frames);
            float t = frames > 1 ? frame / (float)frames : 0f;        // 0..1 across the animation
            float wave = Mathf.Sin(t * Mathf.PI * 2f);                  // -1..1
            int bob = Mathf.RoundToInt(wave * 4f);                      // ±4 px (Docs/06 §9)

            DrawShape(c, set, anim, frame, t, wave, bob);

            // Edge outline + pivot + label are debug aids; the artist's file replaces all of it.
            c.Border(2, PixelCanvas.WithAlpha(AccentFor(set.style), 140));
            Vector2 pv = set.PivotVector;
            c.PivotMarker(Mathf.Clamp(Mathf.RoundToInt(pv.x * (set.width - 1)), 0, set.width - 1),
                          Mathf.Clamp(Mathf.RoundToInt(pv.y * (set.height - 1)), 0, set.height - 1), 6, Marker);
            DrawLabel(c, set, anim, frame);
            return c.ToPng();
        }

        // ------------------------------------------------------------------
        // Labels
        // ------------------------------------------------------------------

        private static void DrawLabel(PixelCanvas c, SpriteSetDef set, AnimDef anim, int frame)
        {
            int scale = Mathf.Clamp(Mathf.Min(set.width, set.height) / 128, 1, 8);
            int lineH = (PixelFont.GlyphHeight + 2) * scale;
            int y = set.height - 4;
            string line1 = set.name.ToUpperInvariant();
            string line2 = string.IsNullOrEmpty(anim.name) ? (anim.IsStatic ? "" : $"{frame:00}") : $"{anim.name.ToUpperInvariant()}_{frame:00}";

            if (PixelFont.MeasureWidth(line1, scale) <= set.width - 8)
            {
                c.DrawText(line1, 4, y, scale, Label);
                y -= lineH;
            }
            if (!string.IsNullOrEmpty(line2) && PixelFont.MeasureWidth(line2, scale) <= set.width - 8)
                c.DrawText(line2, 4, y, scale, Label);
            else if (!string.IsNullOrEmpty(line2))
                c.DrawText($"{frame:00}", 4, y, scale, Label);
        }

        private static Color32 AccentFor(string style)
        {
            switch (style)
            {
                case "protagonist": return ProtBlue;
                case "crawler": case "enemy_p1": case "flyer_p1": case "boss_p1": return EnemyRed;
                case "enemy_p2": case "flyer_p2": case "charger_p2": case "boss_p2": return EnemyBlue;
                case "zulmat": return ZulCrimson;
                default: return new Color32(255, 255, 255, 255);
            }
        }

        // ------------------------------------------------------------------
        // Shape dispatch
        // ------------------------------------------------------------------

        private static void DrawShape(PixelCanvas c, SpriteSetDef set, AnimDef anim, int frame, float t, float wave, int bob)
        {
            int w = set.width, h = set.height;
            int cx = w / 2, cy = h / 2;
            string a = anim.name;

            switch (set.style)
            {
                case "protagonist": Biped(c, w, h, a, t, bob, ProtWhite, ProtBlue, ProtDeep, 0.42f, false, false); break;
                case "enemy_p1": Biped(c, w, h, a, t, bob, EnemyBlack, EnemyRed, EnemyRedGlow, 0.5f, false, false); break;
                case "enemy_p2": Biped(c, w, h, a, t, bob, EnemyWhite, EnemyBlue, EnemyDeepBlue, 0.5f, true, false); break;
                case "charger_p2": Biped(c, w, h, a, t, bob, EnemyWhite, EnemyBlue, EnemyDeepBlue, 0.9f, true, false); break;
                case "boss_p1": Biped(c, w, h, a, t, bob, EnemyBlack, EnemyRed, EnemyRedGlow, 0.5f, true, true); break;
                case "boss_p2": Biped(c, w, h, a, t, bob, EnemyWhite, EnemyBlue, EnemyDeepBlue, 0.75f, true, true); break;
                case "zulmat": Zulmat(c, w, h, a, t, bob); break;
                case "crawler": Crawler(c, w, h, a, t, bob); break;
                case "flyer_p1": Flyer(c, w, h, a, t, wave, EnemyBlack, EnemyRed, true); break;
                case "flyer_p2": Flyer(c, w, h, a, t, wave, EnemyWhite, EnemyBlue, false); break;

                case "slash": c.FillCrescent(w / 2 + w / 8, cy, h / 2 - 12 - frame * 4, -w / 5, 0, PixelCanvas.WithAlpha(ProtWhite, 230)); c.FillCrescent(w / 2 + w / 8, cy, h / 2 - 20 - frame * 4, -w / 5, 0, PixelCanvas.WithAlpha(ProtBlue, 200)); break;
                case "slash_up": c.FillCrescent(cx, h / 2 + h / 8, w / 2 - 12 - frame * 4, 0, -h / 5, PixelCanvas.WithAlpha(ProtWhite, 230)); c.FillCrescent(cx, h / 2 + h / 8, w / 2 - 20 - frame * 4, 0, -h / 5, PixelCanvas.WithAlpha(ProtBlue, 200)); break;
                case "fireball": c.FillSoftCircle(cx + bob, cy, h / 2 - 8, PixelCanvas.WithAlpha(ProtBlue, 220), 0.5f); c.FillSoftCircle(cx + bob + 6, cy, h / 3 - 4, ProtWhite, 0.6f); break;
                case "fireball_impact": c.Ring(cx, cy, Mathf.Min(w, h) / 2 - 8 - (4 - frame) * 10, Mathf.Max(0, Mathf.Min(w, h) / 2 - 24 - (4 - frame) * 10), PixelCanvas.WithAlpha(ProtBlue, (byte)(230 - frame * 40))); c.FillSoftCircle(cx, cy, 20 + frame * 6, PixelCanvas.WithAlpha(ProtWhite, (byte)(220 - frame * 40)), 0.8f); break;
                case "soul_orb": c.FillCircle(cx, cy, Mathf.Min(w, h) / 2 - 4 - (frame % 2), ProtBlue); c.FillCircle(cx, cy, Mathf.Min(w, h) / 2 - 9, PixelCanvas.Hex("#0D0812")); break;
                case "aura": c.Ring(cx, cy, Mathf.Min(w, h) / 2 - 6 - frame * 3, Mathf.Min(w, h) / 2 - 18 - frame * 3, PixelCanvas.WithAlpha(PixelCanvas.Hex("#73FFA6"), 150)); c.FillSoftCircle(cx, cy, Mathf.Min(w, h) / 2 - 24, PixelCanvas.WithAlpha(PixelCanvas.Hex("#73FFA6"), 60), 1f); break;
                case "spark": Star(c, cx, cy, Mathf.Min(w, h) / 2 - 10 - frame * 8, PixelCanvas.WithAlpha(PixelCanvas.Hex("#FFF2A0"), (byte)(255 - frame * 50))); break;
                case "clink": Star(c, cx, cy, Mathf.Min(w, h) / 2 - 10 - frame * 10, PixelCanvas.WithAlpha(PixelCanvas.Hex("#FFE28A"), (byte)(255 - frame * 60))); break;
                case "black_slash": c.FillCrescent(cx, cy, h / 2 - 6, -w / 4, 0, EnemyBlack); c.FillCrescent(cx, cy, h / 2 - 12, -w / 4, 0, PixelCanvas.WithAlpha(EnemyRed, 200)); break;
                case "black_slash_break": for (int i = 0; i < 5; i++) c.FillRect(cx - 40 + i * 18 + frame * 4 * (i - 2), cy - 10 + (i % 2) * 12, 10, 10, PixelCanvas.WithAlpha(EnemyRed, (byte)(255 - frame * 55))); break;
                case "exclamation": c.FillRect(cx - w / 8, cy - h / 6 + bob, w / 4, h / 2, EnemyRed); c.FillRect(cx - w / 8, cy - h / 2 + 8 + bob, w / 4, w / 4, EnemyRed); break;
                case "knife": c.FillTriangle(new Vector2Int(8, cy), new Vector2Int(w / 2, cy + h / 2 - 4), new Vector2Int(w / 2, cy - h / 2 + 4), EnemyWhite); c.FillRect(w / 2, cy - 4, w / 2 - 8, 8, EnemyBlue); break;
                case "vine_warn": c.FillEllipse(cx, h / 4, w / 2 - 8 - frame * 6, h / 4, PixelCanvas.WithAlpha(ZulCrimson, (byte)(120 + frame * 30))); break;
                case "vine": Vine(c, w, h, a, frame, anim.frames); break;
                case "lava": for (int x = 0; x < w; x += 2) { int hh = h / 2 + Mathf.RoundToInt(Mathf.Sin((x / 10f) + frame * 1.2f) * 5f); c.FillRect(x, 0, 2, hh, PixelCanvas.Hex("#FF5A1F")); c.FillRect(x, hh - 4, 2, 4, FlameCore); } break;
                case "giant_swipe": c.FillCrescent(w / 2 + w / 6, cy, h / 2 - 12, -w / 4, 0, PixelCanvas.WithAlpha(ZulBlack, 230)); c.FillCrescent(w / 2 + w / 6, cy, h / 2 - 30, -w / 4, 0, PixelCanvas.WithAlpha(ZulCrimson, (byte)(200 - frame * 30))); break;
                case "shockwave": c.FillEllipse(cx, h / 3, w / 2 - 8 - (3 - frame) * 24, h / 3, PixelCanvas.WithAlpha(EnvMidInv, (byte)(230 - frame * 50))); break;
                case "shield": c.FillCrescent(cx - w / 6, cy, h / 2 - 8, -w / 2, 0, PixelCanvas.WithAlpha(EnemyBlue, (byte)(200 + frame * 40))); break;
                case "dust": for (int i = 0; i < 4; i++) c.FillSoftCircle(cx - 20 + i * 14, cy - 10 + (i % 2) * 8 + frame * 4, 14 + frame * 3, PixelCanvas.WithAlpha(PixelCanvas.Hex("#B8B0A0"), (byte)(200 - frame * 45)), 0.6f); break;
                case "smoke": c.FillSoftCircle(cx, cy + frame * 8, Mathf.Min(w, h) / 2 - 16 - frame * 6, PixelCanvas.WithAlpha(EnvDark, (byte)(220 - frame * 35)), 0.7f); break;

                case "tile_cave": Tile(c, w, h, set.name, EnvDark, EnvMid); break;
                case "tile_castle": Tile(c, w, h, set.name, EnvLight, EnvMidInv); break;
                case "tile_oneway": c.FillRect(0, h / 2 - 4, w, 10, set.folder.Contains("Castle") ? EnvAccentInv : EnvAccent); c.FillRect(0, h / 2 + 2, w, 4, set.folder.Contains("Castle") ? EnvMidInv : EnvMid); break;
                case "tile_spikes": for (int i = 0; i < 3; i++) c.FillTriangle(new Vector2Int(i * 17, 4), new Vector2Int(i * 17 + 16, 4), new Vector2Int(i * 17 + 8, h - 6), set.folder.Contains("Castle") ? EnvMidInv : EnvMid); break;
                case "tile_lava": for (int x = 0; x < w; x += 2) { int hh = h - 10 + Mathf.RoundToInt(Mathf.Sin((x / 8f) + frame * 1.5f) * 4f); c.FillRect(x, 0, 2, hh, PixelCanvas.Hex("#FF5A1F")); c.FillRect(x, hh - 3, 2, 3, FlameCore); } break;
                case "decor": c.FillSoftCircle(cx, cy, Mathf.Min(w, h) / 2 - 10, PixelCanvas.WithAlpha(set.folder.Contains("Castle") ? EnvMidInv : EnvMid, 200), 0.3f); c.FillTriangle(new Vector2Int(cx - 30, 10), new Vector2Int(cx + 30, 10), new Vector2Int(cx, h - 10), PixelCanvas.WithAlpha(set.folder.Contains("Castle") ? EnvAccentInv : EnvAccent, 180)); break;

                case "parchment": Parchment(c, w, h, ParchBeige, PixelCanvas.Hex("#D8C79E"), 11); break;
                case "parchment_inv": Parchment(c, w, h, ParchNavy, PixelCanvas.Hex("#1B2440"), 23); break;
                case "frame": Vignette(c, w, h); break;
                case "flame_glow": c.FillSoftCircle(cx, cy, Mathf.Min(w, h) / 2 - 2, new Color32(255, 255, 255, 255), 1f); break;
                case "flame_core": c.FillSoftCircle(cx, cy - h / 8, h / 3 + bob, PixelCanvas.WithAlpha(FlameGlow, 200), 0.6f); c.FillTriangle(new Vector2Int(cx - w / 5, cy - h / 8), new Vector2Int(cx + w / 5, cy - h / 8), new Vector2Int(cx + bob, cy + h / 3 + bob), PixelCanvas.WithAlpha(FlameCore, 230)); break;
                case "bg_cave_far": Stalactites(c, w, h, PixelCanvas.WithAlpha(EnvMid, 160), 9, 0.55f); break;
                case "bg_cave_mid": Stalactites(c, w, h, PixelCanvas.WithAlpha(EnvDark, 200), 13, 0.75f); break;
                case "bg_castle_far": Towers(c, w, h, PixelCanvas.WithAlpha(EnvMidInv, 160), 7, 0.5f); break;
                case "bg_castle_mid": Towers(c, w, h, PixelCanvas.WithAlpha(EnvLight, 200), 11, 0.7f); break;

                case "prop_bench": c.FillRect(w / 4, 8, w / 2, h / 4, EnvAccent); c.FillRect(cx - 6, h / 4 + 8, 12, h / 4, ParchEdge); if (a != "Unlit") c.FillSoftCircle(cx, h / 2 + 8 + bob, 14 + frame * 2, FlameCore, 0.7f); break;
                case "prop_door": { int open = a == "Open" ? frame * (w / 6) : 0; c.FillRect(0, 0, w, h, PixelCanvas.WithAlpha(EnvDark, 60)); c.FillRoundRect(open, 4, w - 8, h - 8, 12, EnvDark); c.FillRect(open + w / 2, h / 2, 8, 8, EnemyRed); break; }
                case "prop_gate": { int drop = a == "Close" ? (int)((h - 20) * (1f - frame / 4f)) : h - 20; for (int i = 0; i < 4; i++) c.FillRect(10 + i * 22, drop, 8, h - drop - 4, EnvMid); c.FillRect(0, h - 12, w, 12, EnvDark); break; }
                case "prop_orb": c.FillSoftCircle(cx, cy, Mathf.Min(w, h) / 2 - 10 - (a == "Touch" ? frame * 3 : 0), PixelCanvas.WithAlpha(PixelCanvas.Hex("#9B4DFF"), 200), 0.5f); c.FillCircle(cx, cy, w / 4 + bob / 2, PixelCanvas.Hex("#E5CCFF")); c.FillRect(cx - w / 4, 4, w / 2, 14, EnvMid); break;
                case "prop_sign": c.FillRect(cx - 4, 0, 8, h / 3, ParchEdge); c.FillRoundRect(8, h / 3, w - 16, h / 2, 8, ParchBeige); c.FillRect(16, h / 3 + 14, w - 32, 4, EnvMid); c.FillRect(16, h / 3 + 26, w - 44, 4, EnvMid); break;
                case "prop_torch": c.FillRect(cx - 6, 0, 12, h / 2, EnvDark); c.FillSoftCircle(cx, h / 2 + h / 6 + bob, w / 3, PixelCanvas.WithAlpha(FlameGlow, 220), 0.6f); c.FillSoftCircle(cx, h / 2 + h / 6 + bob, w / 6, FlameCore, 0.5f); break;
                case "prop_chandelier": c.FillRect(cx - 2, h / 2, 4, h / 2, EnvMid); c.FillRect(10, h / 3, w - 20, 16, EnvDark); for (int i = 0; i < 5; i++) c.FillSoftCircle(20 + i * (w - 40) / 4, h / 3 + 26, 10, FlameCore, 0.6f); break;

                case "cutscene": c.Clear(PixelCanvas.Hex("#777777")); c.FillRect(0, 0, w, h / 3, PixelCanvas.WithAlpha(EnvDark, 90)); break;
                case "cutscene_eyes": c.Clear(PixelCanvas.Hex("#08060A")); c.FillEllipse(w / 2 - 160, h / 2, 90, 30, ZulCrimson); c.FillEllipse(w / 2 + 160, h / 2, 90, 30, ZulCrimson); break;
                case "god": c.FillTriangle(new Vector2Int(cx - w / 3, h / 3), new Vector2Int(cx + w / 3, h / 3), new Vector2Int(cx, h - 20), EnvDark); c.FillRoundRect(cx - w / 3, 10, 2 * w / 3, h / 3 + 10, 30, EnvDark); c.FillEllipse(cx, h * 2 / 3, 18, 8, EnemyRedGlow); break;

                case "ui_heart_full": Heart(c, w, h, PixelCanvas.Hex("#E23A4A"), true); break;
                case "ui_heart_empty": Heart(c, w, h, PixelCanvas.Hex("#5A2A30"), false); break;
                case "ui_heart_break": Heart(c, w, h, PixelCanvas.Lerp(PixelCanvas.Hex("#E23A4A"), PixelCanvas.Hex("#5A2A30"), frame / 3f), true); c.Line(cx - 6 + frame * 2, h - 10, cx + 4 - frame * 2, 8, new Color32(0, 0, 0, 220), 3); break;
                case "ui_soul_frame": c.Ring(cx, cy, Mathf.Min(w, h) / 2 - 2, Mathf.Min(w, h) / 2 - 14, ParchEdge); break;
                case "ui_soul_fill": c.FillCircle(cx, cy, Mathf.Min(w, h) / 2 - 16, PixelCanvas.Hex("#0D0812")); c.Ring(cx, cy, Mathf.Min(w, h) / 2 - 16, Mathf.Min(w, h) / 2 - 24, ProtBlue); break;
                case "ui_soul_glow": c.FillSoftCircle(cx, cy, Mathf.Min(w, h) / 2 - 2, PixelCanvas.WithAlpha(ProtBlue, 200), 1f); break;
                case "ui_bar_frame": c.FillRoundRect(0, 0, w, h, 10, PixelCanvas.WithAlpha(ParchEdge, 255)); c.FillRoundRect(6, 6, w - 12, h - 12, 6, new Color32(0, 0, 0, 0)); for (int y = 6; y < h - 6; y++) for (int x = 6; x < w - 6; x++) c.Set(x, y, new Color32(0, 0, 0, 0)); break;
                case "ui_bar_fill": c.FillRoundRect(0, 0, w, h, 8, EnemyRed); c.FillRect(0, h - 10, w, 6, PixelCanvas.WithAlpha(EnemyRedGlow, 180)); break;
                case "ui_button": c.FillRoundRect(0, 0, w, h, 14, ParchBeige); c.FillRoundRect(4, 4, w - 8, h - 8, 12, PixelCanvas.WithAlpha(ParchEdge, 60)); break;
                case "ui_button_hover": c.FillRoundRect(0, 0, w, h, 14, ParchEdge); c.FillRoundRect(4, 4, w - 8, h - 8, 12, ParchBeige); break;
                case "ui_logo": { int sc = 10; string title = "CINDERS OF FATE"; int tw = PixelFont.MeasureWidth(title, sc); c.DrawText(title, cx - tw / 2, cy + (PixelFont.GlyphHeight * sc) / 2, sc, EnvDark, false); c.FillRect(cx - tw / 2, cy - PixelFont.GlyphHeight * sc / 2 - 24, tw, 8, EnemyRed); break; }
                case "ui_cursor": c.FillTriangle(new Vector2Int(10, 10), new Vector2Int(10, h - 10), new Vector2Int(w - 10, cy), ParchBeige); break;
                case "ui_icon_dash": c.FillRoundRect(6, 6, w - 12, h - 12, 12, EnvDark); for (int i = 0; i < 3; i++) c.FillRect(16 + i * 10, cy - 12 + i * 4, 40, 8, PixelCanvas.WithAlpha(ProtBlue, (byte)(120 + i * 60))); break;
                case "ui_icon_fireball": c.FillRoundRect(6, 6, w - 12, h - 12, 12, EnvDark); c.FillSoftCircle(cx, cy, w / 3, ProtBlue, 0.5f); c.FillSoftCircle(cx + 6, cy, w / 6, ProtWhite, 0.5f); break;

                default: c.FillRoundRect(w / 6, h / 6, w * 2 / 3, h * 2 / 3, 10, PixelCanvas.WithAlpha(EnvMid, 200)); break;
            }
        }

        // ------------------------------------------------------------------
        // Characters
        // ------------------------------------------------------------------

        private static void Biped(PixelCanvas c, int w, int h, string anim, float t, int bob, Color32 body, Color32 accent, Color32 outline, float widthRatio, bool armoured, bool weapon)
        {
            int margin = Mathf.Max(8, h / 24);
            int visH = h - margin * 2;
            int cx = w / 2;
            int feet = margin;

            float squash = 1f, lean = 0f;
            switch (anim)
            {
                case "Land": case "Telegraph": case "Slash_Telegraph": case "Jump_Telegraph": case "Dash_Telegraph": case "Throw_Telegraph": case "Cast_Telegraph": case "Lunge_Telegraph": case "Swipe_Telegraph": squash = 0.85f; break;
                case "Jump_Rise": squash = 1.1f; break;
                case "Jump_Fall": case "Jump_Air": squash = 1.05f; break;
                case "Hurt": lean = -0.12f; break;
                case "Death": squash = Mathf.Lerp(1f, 0.25f, t); break;
                case "Lunge": case "Dive": case "Charge": case "Dash": lean = 0.18f; break;
                case "Rest": squash = 0.7f; break;
                case "Stunned": lean = Mathf.Sin(t * Mathf.PI * 2f) * 0.1f; break;
            }

            int bodyH = Mathf.RoundToInt(visH * 0.55f * squash);
            int bodyW = Mathf.RoundToInt(visH * widthRatio * 0.5f);
            int legH = Mathf.RoundToInt(visH * 0.22f * squash);
            int headR = Mathf.RoundToInt(visH * 0.12f);
            int shift = Mathf.RoundToInt(lean * visH);
            int bobY = anim == "Idle" || anim == "Run" || anim == "Walk" || anim == "Fly" ? bob : 0;

            // Legs (alternate ±4 px while running).
            int legGap = bodyW / 3;
            int legW = Mathf.Max(6, bodyW / 4);
            int legSwing = anim == "Run" || anim == "Walk" ? bob : 0;
            c.FillRect(cx - legGap - legW / 2 + legSwing, feet, legW, legH, body);
            c.FillRect(cx + legGap - legW / 2 - legSwing, feet, legW, legH, body);

            // Torso
            int torsoY = feet + legH - 2;
            c.FillRoundRect(cx - bodyW / 2 + shift, torsoY + bobY, bodyW, bodyH, Mathf.Max(6, bodyW / 4), body);
            if (armoured) c.FillRect(cx - bodyW / 2 + shift, torsoY + bobY + bodyH / 2, bodyW, Mathf.Max(4, bodyH / 10), accent);

            // Head + eye/accent (faces right)
            int headY = torsoY + bodyH + headR - 2 + bobY;
            c.FillCircle(cx + shift + shift / 2, headY, headR, body);
            c.FillRect(cx + shift + headR / 3, headY, Mathf.Max(3, headR / 2), Mathf.Max(3, headR / 3), accent);

            // Scarf / sash
            c.FillRect(cx - bodyW / 2 + shift, torsoY + bodyH - Mathf.Max(4, bodyH / 8) + bobY, bodyW, Mathf.Max(4, bodyH / 8), accent);

            // Arm / weapon
            bool attacking = anim.StartsWith("Attack") || anim.StartsWith("Slash") || anim == "Bash" || anim == "Throw" || anim == "Swipe" || anim == "Cast" || anim == "Cast_Fire";
            if (attacking)
            {
                int reach = Mathf.RoundToInt(visH * (0.35f + 0.25f * t));
                if (anim == "Attack_Up") c.FillRect(cx - legW / 2, torsoY + bodyH, legW, reach, accent);
                else c.FillRect(cx + bodyW / 2 + shift, torsoY + bodyH * 2 / 3 + bobY, reach, Mathf.Max(6, legW), weapon ? outline : accent);
            }
            else if (anim.StartsWith("Block"))
            {
                c.FillRect(cx + bodyW / 2 + 4 + shift, torsoY + bobY, Mathf.Max(8, bodyW / 4), bodyH + headR, outline);
            }
            else if (weapon)
            {
                c.FillRect(cx - bodyW / 2 - Mathf.Max(6, legW) + shift, torsoY + bobY, Mathf.Max(6, legW), bodyH + headR * 2, outline);
            }

            if (anim == "Heal_Charge" || anim == "Heal_Release")
                c.Ring(cx, torsoY + bodyH / 2, bodyW + 6 + bob, bodyW + 2 + bob, PixelCanvas.WithAlpha(PixelCanvas.Hex("#73FFA6"), 160));
        }

        private static void Zulmat(PixelCanvas c, int w, int h, string anim, float t, int bob)
        {
            Biped(c, w, h, anim, t, bob, ZulBlack, ZulCrimson, ZulCrimson, 0.55f, false, false);
            int margin = Mathf.Max(8, h / 24);
            int visH = h - margin * 2;
            int top = margin + visH;
            int cx = w / 2;
            // Flame hair / horns swept back-left.
            for (int i = 0; i < 4; i++)
            {
                int bx = cx - visH / 8 + i * visH / 16;
                c.FillTriangle(new Vector2Int(bx - 10, top - visH / 5), new Vector2Int(bx + 10, top - visH / 5), new Vector2Int(bx - visH / 10 - bob, Mathf.Min(h - 4, top - i * 6)), ZulCrimson);
            }
        }

        private static void Crawler(PixelCanvas c, int w, int h, string anim, float t, int bob)
        {
            int margin = Mathf.Max(8, h / 16);
            int cx = w / 2;
            int bodyW = w * 3 / 5, bodyH = h * 2 / 5;
            int squash = anim == "Death" ? Mathf.RoundToInt(bodyH * t * 0.6f) : 0;
            c.FillRoundRect(cx - bodyW / 2, margin + 6, bodyW, bodyH - squash, 10, EnemyBlack);
            c.FillCircle(cx + bodyW / 3, margin + bodyH - squash, h / 8, EnemyBlack);
            c.FillRect(cx + bodyW / 3 + 2, margin + bodyH - squash, 6, 4, EnemyRed);
            int legSwing = anim == "Walk" ? bob : 0;
            for (int i = 0; i < 3; i++)
            {
                int lx = cx - bodyW / 2 + 6 + i * (bodyW / 3);
                c.FillRect(lx + (i % 2 == 0 ? legSwing : -legSwing), margin, 5, 8, EnemyBlack);
            }
            c.FillRect(cx - bodyW / 2, margin + 6 + bodyH / 2 - squash, bodyW, 4, EnemyRed);
        }

        private static void Flyer(PixelCanvas c, int w, int h, string anim, float t, float wave, Color32 body, Color32 accent, bool kite)
        {
            int cx = w / 2, cy = h / 2;
            int flap = Mathf.RoundToInt(wave * 6f);
            int span = w / 2 - 10;
            if (anim == "Dive") flap = -8;
            if (anim == "Sleep") { span = w / 5; flap = 10; }

            c.FillTriangle(new Vector2Int(cx, cy + 4), new Vector2Int(cx - span, cy + flap), new Vector2Int(cx, cy - h / 5), body);
            c.FillTriangle(new Vector2Int(cx, cy + 4), new Vector2Int(cx + span, cy + flap), new Vector2Int(cx, cy - h / 5), body);
            c.FillCircle(cx, cy, h / 8, body);
            c.FillRect(cx + 2, cy + 2, 5, 3, accent);
            if (kite) c.Line(cx, cy - h / 5, cx - 10 + flap, cy - h / 2 + 6, accent, 2);
            else { c.FillTriangle(new Vector2Int(cx - 6, cy + h / 8), new Vector2Int(cx - 2, cy + h / 8 + 10), new Vector2Int(cx - 12, cy + h / 8), body); c.FillTriangle(new Vector2Int(cx + 6, cy + h / 8), new Vector2Int(cx + 2, cy + h / 8 + 10), new Vector2Int(cx + 12, cy + h / 8), body); }
        }

        private static void Vine(PixelCanvas c, int w, int h, string anim, int frame, int frames)
        {
            float k = anim == "Retract" ? 1f - (frame + 1) / (float)frames : (frame + 1) / (float)frames;
            int vh = Mathf.RoundToInt((h - 12) * k);
            int cx = w / 2;
            c.FillRoundRect(cx - w / 6, 4, w / 3, vh, w / 8, ZulBlack);
            for (int y = 20; y < vh; y += 36) c.FillEllipse(cx + ((y / 36) % 2 == 0 ? w / 4 : -w / 4), y + 4, w / 5, 10, ZulBlack);
            c.FillTriangle(new Vector2Int(cx - w / 6, vh - 4), new Vector2Int(cx + w / 6, vh - 4), new Vector2Int(cx, Mathf.Min(h - 4, vh + 30)), ZulCrimson);
        }

        private static void Star(PixelCanvas c, int cx, int cy, int r, Color32 col)
        {
            if (r <= 2) return;
            c.FillTriangle(new Vector2Int(cx - r, cy), new Vector2Int(cx + r, cy), new Vector2Int(cx, cy + r / 3), col);
            c.FillTriangle(new Vector2Int(cx - r, cy), new Vector2Int(cx + r, cy), new Vector2Int(cx, cy - r / 3), col);
            c.FillTriangle(new Vector2Int(cx, cy - r), new Vector2Int(cx, cy + r), new Vector2Int(cx + r / 3, cy), col);
            c.FillTriangle(new Vector2Int(cx, cy - r), new Vector2Int(cx, cy + r), new Vector2Int(cx - r / 3, cy), col);
        }

        // ------------------------------------------------------------------
        // Tiles / environment
        // ------------------------------------------------------------------

        private static void Tile(PixelCanvas c, int w, int h, string name, Color32 fill, Color32 lip)
        {
            int index = 15;
            int us = name.LastIndexOf('_');
            if (us >= 0) int.TryParse(name.Substring(us + 1), out index);
            bool n = (index & 1) != 0, e = (index & 2) != 0, s = (index & 4) != 0, wst = (index & 8) != 0;

            c.Clear(fill);
            // Texture speckle.
            for (int y = 0; y < h; y += 3) for (int x = 0; x < w; x += 3)
                if (PixelCanvas.Hash(x, y, index) > 0.85f) c.Blend(x, y, PixelCanvas.WithAlpha(lip, 60));
            // Open edges get a lighter lip (top surface strongest).
            if (!n) c.FillRect(0, h - 5, w, 5, lip);
            if (!s) c.FillRect(0, 0, w, 3, PixelCanvas.WithAlpha(lip, 160));
            if (!e) c.FillRect(w - 3, 0, 3, h, PixelCanvas.WithAlpha(lip, 160));
            if (!wst) c.FillRect(0, 0, 3, h, PixelCanvas.WithAlpha(lip, 160));
        }

        private static void Parchment(PixelCanvas c, int w, int h, Color32 baseCol, Color32 fibre, int seed)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float n = PixelCanvas.TileNoise(x, y, w, h, 64, seed) * 0.6f + PixelCanvas.TileNoise(x, y, w, h, 8, seed + 1) * 0.4f;
                    c.Set(x, y, PixelCanvas.Lerp(baseCol, fibre, n * 0.5f));
                }
            }
            // Fibres that wrap so the texture stays seamless.
            for (int i = 0; i < 900; i++)
            {
                int x0 = (int)(PixelCanvas.Hash(i, 1, seed) * w), y0 = (int)(PixelCanvas.Hash(i, 2, seed) * h);
                int len = 20 + (int)(PixelCanvas.Hash(i, 3, seed) * 120);
                int dir = PixelCanvas.Hash(i, 4, seed) > 0.5f ? 1 : -1;
                for (int k = 0; k < len; k++) c.Blend((x0 + k) % w, (y0 + dir * (k / 9)) % h < 0 ? h - 1 : (y0 + dir * (k / 9)) % h, PixelCanvas.WithAlpha(fibre, 70));
            }
        }

        private static void Vignette(PixelCanvas c, int w, int h)
        {
            int cx = w / 2, cy = h / 2;
            float maxD = Mathf.Sqrt(cx * cx + cy * cy);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / maxD;
                    float a = Mathf.Clamp01((d - 0.55f) / 0.45f);
                    a = a * a;
                    float edge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y)) / 60f;
                    a = Mathf.Max(a, 1f - Mathf.Clamp01(edge));
                    c.Set(x, y, new Color32(0, 0, 0, (byte)(a * 235)));
                }
            }
        }

        private static void Stalactites(PixelCanvas c, int w, int h, Color32 col, int count, float heightRatio)
        {
            for (int i = 0; i < count; i++)
            {
                int x = (int)(PixelCanvas.Hash(i, 7, count) * w);
                int hw = 40 + (int)(PixelCanvas.Hash(i, 8, count) * 90);
                int len = (int)(h * heightRatio * (0.4f + PixelCanvas.Hash(i, 9, count) * 0.6f));
                c.FillTriangle(new Vector2Int(x - hw, h), new Vector2Int(x + hw, h), new Vector2Int(x, h - len), col);
                c.FillTriangle(new Vector2Int(x - hw + w, h), new Vector2Int(x + hw + w, h), new Vector2Int(x + w, h - len), col);
                c.FillTriangle(new Vector2Int(x - hw - w, h), new Vector2Int(x + hw - w, h), new Vector2Int(x - w, h - len), col);
                int px = (int)(PixelCanvas.Hash(i, 10, count) * w);
                c.FillRect(px, 0, 30 + (int)(PixelCanvas.Hash(i, 11, count) * 50), (int)(h * 0.3f * PixelCanvas.Hash(i, 12, count)) + 40, col);
            }
        }

        private static void Towers(PixelCanvas c, int w, int h, Color32 col, int count, float heightRatio)
        {
            for (int i = 0; i < count; i++)
            {
                int x = (int)(PixelCanvas.Hash(i, 17, count) * w);
                int tw = 60 + (int)(PixelCanvas.Hash(i, 18, count) * 120);
                int th = (int)(h * heightRatio * (0.5f + PixelCanvas.Hash(i, 19, count) * 0.5f));
                for (int wrap = -1; wrap <= 1; wrap++)
                {
                    int bx = x + wrap * w;
                    c.FillRect(bx - tw / 2, 0, tw, th, col);
                    c.FillTriangle(new Vector2Int(bx - tw / 2 - 10, th), new Vector2Int(bx + tw / 2 + 10, th), new Vector2Int(bx, th + tw / 2), col);
                    for (int b = 0; b < tw; b += 24) c.FillRect(bx - tw / 2 + b, th, 12, 14, col);
                }
            }
        }

        private static void Heart(PixelCanvas c, int w, int h, Color32 col, bool filled)
        {
            int cx = w / 2;
            int r = w / 4 - 2;
            if (filled)
            {
                c.FillCircle(cx - r + 2, h * 2 / 3 - 4, r, col);
                c.FillCircle(cx + r - 2, h * 2 / 3 - 4, r, col);
                c.FillTriangle(new Vector2Int(cx - 2 * r + 2, h * 2 / 3 - 6), new Vector2Int(cx + 2 * r - 2, h * 2 / 3 - 6), new Vector2Int(cx, 6), col);
            }
            else
            {
                c.Ring(cx - r + 2, h * 2 / 3 - 4, r, r - 4, col);
                c.Ring(cx + r - 2, h * 2 / 3 - 4, r, r - 4, col);
                c.Line(cx - 2 * r + 3, h * 2 / 3 - 8, cx, 6, col, 4);
                c.Line(cx + 2 * r - 3, h * 2 / 3 - 8, cx, 6, col, 4);
            }
        }
    }
}
