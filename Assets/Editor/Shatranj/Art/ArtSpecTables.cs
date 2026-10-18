using System.Collections.Generic;
using Shatranj.Data;

namespace Shatranj.EditorTools.Art
{
    /// <summary>
    /// Docs/06 §3–§7 as data. <see cref="Build"/> produces the manifest that is written to
    /// Assets/Data/AnimationManifest.json once; after that the JSON file is the source of truth.
    /// </summary>
    public static class ArtSpecTables
    {
        private static AnimDef A(string name, int frames, float fps, bool loop = false, bool holdLast = false)
            => new AnimDef(name, frames, fps, loop, holdLast);

        private static SpriteSetDef Set(string name, string folder, string layout, string pivot, string style, int w, int h, params AnimDef[] anims)
        {
            var s = new SpriteSetDef { name = name, folder = folder, layout = layout, pivot = pivot, style = style, width = w, height = h };
            s.anims.AddRange(anims);
            return s;
        }

        private static SpriteSetDef Character(string name, string pivot, string style, int w, int h, params AnimDef[] anims)
            => Set(name, "Characters", "character", pivot, style, w, h, anims);

        private static SpriteSetDef Fx(string name, int w, int h, string style, string pivot, params AnimDef[] anims)
            => Set(name, "FX", "set", pivot, style, w, h, anims);

        private static SpriteSetDef Prop(string name, int w, int h, string style, string pivot, params AnimDef[] anims)
            => Set(name, "Props", "set", pivot, style, w, h, anims);

        private static SpriteSetDef Image(string name, string folder, int w, int h, string style, string pivot = ArtPivot.Center)
            => Set(name, folder, "flat", pivot, style, w, h, A("", 1, 0f));

        private static SpriteSetDef Strip(string name, string folder, int w, int h, string style, int frames, float fps, bool loop, string pivot = ArtPivot.Center)
            => Set(name, folder, "flat", pivot, style, w, h, A("", frames, fps, loop));

        public static ArtManifest Build()
        {
            var m = new ArtManifest { version = 1, pixelsPerUnit = 100 };
            var s = m.sets;

            // ---------------- §3.1 Protagonist (65 frames + 2 optional) ----------------
            s.Add(Character("Protagonist", ArtPivot.Bottom, "protagonist", 256, 256,
                A("Idle", 6, 8, true), A("Run", 8, 14, true), A("Jump_Rise", 2, 10, false, true), A("Jump_Fall", 2, 10, true),
                A("Land", 3, 16), A("Attack_Side_A", 4, 20), A("Attack_Side_B", 4, 20), A("Attack_Up", 4, 20),
                A("Cast", 5, 14), A("Heal_Charge", 6, 10, true), A("Heal_Release", 3, 12), A("Hurt", 2, 12),
                A("Death", 8, 10), A("Dash", 4, 20), A("Rest", 4, 6, true), A("Look_Up", 1, 1), A("Look_Down", 1, 1)));
            // Phase 2 look (black/red, D-181): same clips, swapped in by PlayerAnimator on the phase change.
            s.Add(Character("Protagonist_P2", ArtPivot.Bottom, "protagonist", 256, 256,
                A("Idle", 6, 8, true), A("Run", 8, 14, true), A("Jump_Rise", 2, 10, false, true), A("Jump_Fall", 2, 10, true),
                A("Land", 3, 16), A("Attack_Side_A", 4, 20), A("Attack_Side_B", 4, 20), A("Attack_Up", 4, 20),
                A("Cast", 5, 14), A("Heal_Charge", 6, 10, true), A("Heal_Release", 3, 12), A("Hurt", 2, 12),
                A("Death", 8, 10), A("Dash", 4, 20), A("Rest", 4, 6, true), A("Look_Up", 1, 1), A("Look_Down", 1, 1)));

            // ---------------- §3.2 Phase 1 enemies ----------------
            s.Add(Character("Crawler", ArtPivot.Bottom, "crawler", 128, 128,
                A("Idle", 4, 6, true), A("Walk", 6, 10, true), A("Hurt", 2, 12), A("Death", 6, 12)));
            s.Add(Character("Lunger", ArtPivot.Bottom, "enemy_p1", 160, 160,
                A("Idle", 4, 6, true), A("Walk", 6, 10, true), A("Telegraph", 3, 10), A("Lunge", 3, 16, false, true),
                A("Recover", 3, 8), A("Hurt", 2, 12), A("Death", 6, 12)));
            s.Add(Character("ShadowKite", ArtPivot.Center, "flyer_p1", 128, 128,
                A("Fly", 6, 12, true), A("Telegraph", 3, 10), A("Dive", 2, 12, true), A("Hurt", 2, 12), A("Death", 6, 12)));

            // ---------------- §3.3 Phase 1 mini-bosses ----------------
            s.Add(Character("KageWarden", ArtPivot.Bottom, "boss_p1", 384, 384,
                A("Intro", 6, 8), A("Idle", 6, 8, true), A("Walk", 8, 10, true), A("Slash_Telegraph", 3, 8),
                A("Slash_A", 4, 18), A("Slash_B", 4, 18), A("Block_Start", 3, 16), A("Block_Hold", 2, 6, true),
                A("Block_Impact", 2, 16), A("Death", 10, 10)));
            s.Add(Character("KageCaller", ArtPivot.Bottom, "boss_p1", 384, 384,
                A("Intro", 6, 8), A("Idle", 6, 8, true), A("Walk", 8, 10, true), A("Slash_Telegraph", 3, 8),
                A("Slash_A", 4, 18), A("Slash_B", 4, 18), A("Cast_Telegraph", 4, 10), A("Cast_Fire", 4, 16),
                A("Summon_Raise", 6, 10), A("Summon_Hold", 2, 6, true), A("Death", 10, 10)));

            // ---------------- §3.4 Phase 2 enemies ----------------
            s.Add(Character("Knight", ArtPivot.Bottom, "enemy_p2", 256, 256,
                A("Idle", 4, 6, true), A("Walk", 8, 10, true), A("Telegraph", 3, 10), A("Attack_A", 4, 18), A("Attack_B", 4, 18),
                A("Hurt", 2, 12), A("Death", 8, 12)));
            s.Add(Character("Bat", ArtPivot.Center, "flyer_p2", 128, 128,
                A("Sleep", 2, 4, true), A("Wake", 3, 12), A("Fly", 6, 14, true), A("Telegraph", 3, 10), A("Dive", 2, 12, true),
                A("Hurt", 2, 12), A("Death", 6, 12)));
            s.Add(Character("Charger", ArtPivot.Bottom, "charger_p2", 320, 256,
                A("Idle", 4, 6, true), A("Walk", 6, 8, true), A("Telegraph", 4, 8), A("Charge", 4, 14, true), A("Stunned", 4, 6, true),
                A("Hurt", 2, 12), A("Death", 8, 12)));
            s.Add(Character("Thrower", ArtPivot.Bottom, "enemy_p2", 256, 256,
                A("Idle", 4, 6, true), A("Walk", 6, 10, true), A("Throw_Telegraph", 3, 10), A("Throw", 3, 16),
                A("Hurt", 2, 12), A("Death", 8, 12)));

            // ---------------- §3.5 Phase 2 bosses ----------------
            s.Add(Character("IronColossus", ArtPivot.Bottom, "boss_p2", 512, 512,
                A("Intro", 6, 8), A("Idle", 6, 6, true), A("Walk", 8, 8, true), A("Slash_Telegraph", 4, 8), A("Slash", 5, 16),
                A("Block_Start", 3, 12), A("Block_Hold", 2, 6, true), A("Block_Impact", 2, 16), A("Bash", 3, 16),
                A("Jump_Telegraph", 3, 10), A("Jump_Air", 2, 6, true), A("Jump_Land", 4, 14), A("Dash_Telegraph", 3, 10),
                A("Dash", 3, 16, true), A("Death", 12, 10)));
            s.Add(Character("Zulmat", ArtPivot.Bottom, "zulmat", 512, 512,
                A("Intro", 8, 8), A("Idle", 8, 8, true), A("Walk", 8, 10, true), A("Slash_Telegraph", 3, 10), A("Slash_A", 4, 18),
                A("Slash_B", 4, 18), A("Vine_Cast", 6, 12), A("Summon_Cast", 6, 10), A("Shrink", 4, 14), A("Grow", 4, 14),
                A("Lunge_Telegraph", 3, 10), A("Lunge", 3, 16, true), A("Swipe_Telegraph", 4, 8), A("Swipe", 5, 16),
                A("Lava_Cast", 6, 10), A("Roar", 6, 10), A("Death", 14, 8)));

            // ---------------- §4 FX & projectiles ----------------
            s.Add(Fx("Slash_Side", 320, 256, "slash", ArtPivot.LeftCenter, A("", 3, 24)));
            s.Add(Fx("Slash_Up", 256, 320, "slash_up", ArtPivot.Bottom, A("", 3, 24)));
            s.Add(Fx("Fireball_Fly", 192, 128, "fireball", ArtPivot.Center, A("", 4, 14, true)));
            s.Add(Fx("Fireball_Impact", 192, 192, "fireball_impact", ArtPivot.Center, A("", 5, 16)));
            s.Add(Fx("SoulOrb", 48, 48, "soul_orb", ArtPivot.Center, A("", 4, 10, true)));
            s.Add(Fx("Heal_Aura", 256, 256, "aura", ArtPivot.Center, A("", 6, 10, true)));
            s.Add(Fx("Hit_Spark", 128, 128, "spark", ArtPivot.Center, A("", 4, 20)));
            s.Add(Fx("Block_Clink", 128, 128, "clink", ArtPivot.Center, A("", 3, 20)));
            s.Add(Fx("BlackSlash_Fly", 192, 96, "black_slash", ArtPivot.Center, A("", 3, 12, true)));
            s.Add(Fx("BlackSlash_Break", 192, 96, "black_slash_break", ArtPivot.Center, A("", 4, 16)));
            s.Add(Fx("Exclamation", 64, 64, "exclamation", ArtPivot.Center, A("", 4, 12, true)));
            s.Add(Fx("Knife_Fly", 128, 32, "knife", ArtPivot.Center, A("", 4, 16, true)));
            s.Add(Fx("Vine_Warn", 192, 64, "vine_warn", ArtPivot.Bottom, A("", 4, 10, true)));
            s.Add(Fx("Vine", 128, 384, "vine", ArtPivot.Bottom, A("Grow", 6, 16), A("Retract", 3, 16)));
            s.Add(Fx("Lava_Zone", 100, 50, "lava", ArtPivot.Center, A("", 4, 6, true)));
            s.Add(Fx("Giant_Swipe", 768, 512, "giant_swipe", ArtPivot.LeftCenter, A("", 5, 16)));
            s.Add(Fx("Shockwave", 256, 64, "shockwave", ArtPivot.Center, A("", 4, 16)));
            s.Add(Fx("Shield_Arc", 256, 384, "shield", ArtPivot.Center, A("", 2, 6, true)));
            s.Add(Fx("Dust_Puff", 96, 96, "dust", ArtPivot.Center, A("", 4, 14)));
            s.Add(Fx("Death_Smoke", 192, 192, "smoke", ArtPivot.Center, A("", 6, 14)));

            // ---------------- §5.1 Tiles ----------------
            foreach (string theme in new[] { "Cave", "Castle" })
            {
                string folder = "Tiles/" + theme;
                string style = theme == "Cave" ? "tile_cave" : "tile_castle";
                for (int i = 0; i < 16; i++) s.Add(Image($"Tile_{theme}_{i:00}", folder, 50, 50, style));
                s.Add(Image($"Tile_{theme}_OneWay_L", folder, 50, 50, "tile_oneway"));
                s.Add(Image($"Tile_{theme}_OneWay_M", folder, 50, 50, "tile_oneway"));
                s.Add(Image($"Tile_{theme}_OneWay_R", folder, 50, 50, "tile_oneway"));
                s.Add(Image($"Tile_{theme}_Spikes", folder, 50, 50, "tile_spikes"));
                for (int i = 0; i < 4; i++) s.Add(Image($"Decor_{theme}_{i:00}", folder, 100, 100, "decor"));
            }
            s.Add(Strip("Tile_Castle_Lava", "Tiles/Castle", 50, 50, "tile_lava", 4, 6, true));

            // ---------------- §5.2 Backgrounds ----------------
            s.Add(Image("Parchment_Beige", "Backgrounds", 2048, 2048, "parchment"));
            s.Add(Image("Parchment_Inverted", "Backgrounds", 2048, 2048, "parchment_inv"));
            s.Add(Image("ParchmentFrame", "Backgrounds", 1920, 1080, "frame"));
            s.Add(Image("FlameGlow", "Backgrounds", 1024, 1024, "flame_glow"));
            s.Add(Strip("FlameCore", "Backgrounds", 256, 256, "flame_core", 6, 10, true));
            s.Add(Image("BG_Cave_Far", "Backgrounds", 2048, 1080, "bg_cave_far"));
            s.Add(Image("BG_Cave_Mid", "Backgrounds", 2048, 1080, "bg_cave_mid"));
            s.Add(Image("BG_Castle_Far", "Backgrounds", 2048, 1080, "bg_castle_far"));
            s.Add(Image("BG_Castle_Mid", "Backgrounds", 2048, 1080, "bg_castle_mid"));

            // ---------------- §5.3 Props ----------------
            s.Add(Prop("Bench", 192, 192, "prop_bench", ArtPivot.Bottom, A("Unlit", 1, 1), A("Lit", 4, 8, true), A("Activate", 4, 10)));
            s.Add(Prop("Door", 128, 384, "prop_door", ArtPivot.BottomLeft, A("Closed", 1, 1), A("Open", 6, 12)));
            s.Add(Prop("Gate", 100, 300, "prop_gate", ArtPivot.Bottom, A("Open", 1, 1), A("Close", 5, 14)));
            s.Add(Prop("Orb", 128, 128, "prop_orb", ArtPivot.Center, A("Idle", 6, 8, true), A("Touch", 6, 12)));
            s.Add(Prop("TutorialSign", 128, 128, "prop_sign", ArtPivot.Bottom, A("", 1, 1)));
            s.Add(Prop("Torch", 96, 192, "prop_torch", ArtPivot.Bottom, A("", 4, 8, true)));
            s.Add(Prop("Chandelier", 300, 150, "prop_chandelier", ArtPivot.Center, A("", 1, 1)));

            // ---------------- §6 Cutscene art ----------------
            s.Add(Image("CS1_Village_Far", "Cutscenes", 2400, 1080, "cutscene"));
            s.Add(Image("CS1_Village_Mid", "Cutscenes", 2400, 1080, "cutscene"));
            s.Add(Image("CS1_Village_Near", "Cutscenes", 2400, 1080, "cutscene"));
            s.Add(Image("CS1_CaveMouth", "Cutscenes", 1920, 1080, "cutscene"));
            for (int i = 1; i <= 6; i++) s.Add(Image($"CS2_Memory_{i:00}", "Cutscenes", 1920, 1080, "cutscene"));
            s.Add(Image("CS2_Zulmat_Eyes", "Cutscenes", 1920, 1080, "cutscene_eyes"));
            s.Add(Image("CS2_Castle_Silhouette", "Cutscenes", 1920, 1080, "cutscene"));
            for (int i = 1; i <= 3; i++) s.Add(Image($"CS3_Merge_{i:00}", "Cutscenes", 1920, 1080, "cutscene"));
            s.Add(Image("CS3_GodsStab", "Cutscenes", 1920, 1080, "cutscene"));
            s.Add(Image("CS3_Split", "Cutscenes", 1920, 1080, "cutscene"));
            s.Add(Image("God_A", "Cutscenes", 512, 768, "god", ArtPivot.Bottom));
            s.Add(Image("God_B", "Cutscenes", 512, 768, "god", ArtPivot.Bottom));
            s.Add(Image("God_C", "Cutscenes", 512, 768, "god", ArtPivot.Bottom));

            // ---------------- §7 UI ----------------
            s.Add(Image("Heart_Full", "UI", 64, 64, "ui_heart_full"));
            s.Add(Image("Heart_Empty", "UI", 64, 64, "ui_heart_empty"));
            s.Add(Strip("Heart_Break", "UI", 64, 64, "ui_heart_break", 4, 16, false));
            s.Add(Image("Soul_Frame", "UI", 192, 192, "ui_soul_frame"));
            s.Add(Image("Soul_Fill", "UI", 192, 192, "ui_soul_fill"));
            s.Add(Image("Soul_Glow", "UI", 256, 256, "ui_soul_glow"));
            s.Add(Image("BossBar_Frame", "UI", 900, 48, "ui_bar_frame"));
            s.Add(Image("BossBar_Fill", "UI", 900, 48, "ui_bar_fill"));
            s.Add(Image("Button_Normal", "UI", 360, 80, "ui_button"));
            s.Add(Image("Button_Hover", "UI", 360, 80, "ui_button_hover"));
            s.Add(Image("Title_Logo", "UI", 1200, 400, "ui_logo"));
            s.Add(Image("Cursor", "UI", 64, 64, "ui_cursor"));
            s.Add(Image("Ability_Icon_Dash", "UI", 96, 96, "ui_icon_dash"));
            s.Add(Image("Ability_Icon_Fireball", "UI", 96, 96, "ui_icon_fireball"));

            return m;
        }
    }
}
