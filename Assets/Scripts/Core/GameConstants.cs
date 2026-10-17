namespace Shatranj.Core
{
    /// <summary>
    /// Global constants from Docs/01 §12b and scene/cutscene identifiers from Docs/02 §3, §8.
    /// Tunable gameplay numbers live in ScriptableObject configs, not here.
    /// </summary>
    public static class GameConstants
    {
        // --- World scale (Docs/01 §12b) ---
        public const int   PixelsPerUnit      = 100;
        public const float CameraOrthoSize    = 5.5f;
        public const float TileSize           = 0.5f;
        public const int   TilePixels         = 50;
        public const int   ScreenWidthTiles   = 40;
        public const int   ScreenHeightTiles  = 22;
        public const float ScreenWidthUnits   = ScreenWidthTiles * TileSize;   // 20
        public const float ScreenHeightUnits  = ScreenHeightTiles * TileSize;  // 11
        public const int   FixedStepHz        = 60;

        // --- Phases / hearts (Docs/01 §5, Docs/03 §9) ---
        public const int Phase1       = 1;
        public const int Phase2       = 2;
        public const int HeartsPhase1 = 5;
        public const int HeartsPhase2 = 7;

        // --- Soul (Docs/03 §5) ---
        public const int SoulMax     = 99;
        public const int SoulPerHit  = 11;
        public const int SpellCost   = 33;

        // --- Scenes (Docs/02 §3) ---
        public const string SceneMainMenu = "MainMenu";
        public const string ScenePhase1   = "Phase1_Cave";
        public const string ScenePhase2   = "Phase2_Castle";
        public const string SceneCutscene = "Cutscene";
        public const string SceneSandbox  = "Sandbox";

        // --- Cutscene ids (Docs/08) ---
        public const string CutsceneIntro  = "cs1_intro";
        public const string CutsceneOrb    = "cs2_orb";
        public const string CutsceneEnding = "cs3_ending";

        // --- Save flags (Docs/05 legend) ---
        public const string FlagDoorOrb = "door_orb";
    }
}
