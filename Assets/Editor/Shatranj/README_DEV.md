# Shatranj — Editor tooling (`Shatranj/…` menu)

All project content (scenes, prefabs, ScriptableObjects, placeholder art, levels) is **generated** by the
editor scripts in this folder. Nobody hand-edits `.unity` / `.prefab` / `.asset` YAML. Run a menu item once,
re-run it whenever you want; every tool is idempotent and never overwrites existing art.

Menu order (top → bottom) mirrors the build order. `Build Everything` runs them all in sequence.

| Menu item | Status | Script | What it does |
|---|---|---|---|
| `Shatranj/Setup Project` | **done** (Prompt 0) | `SetupProject.cs` | Creates the Docs/02 §2 folder structure (+ `.gitkeep`), the 12 physics layers (slots 8–19) and 4 tags, the 11 sorting layers back→front, the Physics2D collision matrix, `Fixed Timestep = 0.01666`, gravity `(0, −9.81)`, and checks/creates the standard Unity `.gitignore`. Reports Active Input Handling. Prints a summary. |
| `Shatranj/Generate Config Assets` | **done** (Prompt 1) | `GenerateConfigs.cs` + `ConfigDefaults.cs` | Creates every ScriptableObject config under `Assets/Data/Configs` with doc defaults: `PlayerConfig`, `ArtStyleConfig`, `Enemies/Enemy_*` ×7, `Bosses/Boss_*` ×4, `Themes/Theme_*` ×4, `Flame/Flame_Phase1/2`. Skips existing assets, so delete one to regenerate it. |
| `Shatranj/Build Sandbox Scene` | **done** (Prompt 2, grows through 12) | `BuildSandboxScene.cs` + `SceneBuildUtil.cs` + `PlayerFactory.cs` | Rebuilds `Assets/Scenes/Sandbox.unity` from scratch: floor with a pit, walls, 4 steps, wall-stick pillar, low ceiling, 5 one-way platforms, 4 u gap test, RoomBounds, camera rig, code-built player, global light, debug HUD. Creates `Assets/Art/Placeholders/Square_White.png` and `Assets/Data/Physics/ZeroFriction.physicsMaterial2D` once. Adds the scene to Build Settings. Later prompts add dummy, spikes, HUD, enemy-spawn keys. |
| `Shatranj/Generate Animation Manifest` | **done** (Prompt 4) | `Art/ManifestIO.cs` + `Art/ArtSpecTables.cs` | Writes `Assets/Data/AnimationManifest.json` from the Docs/06 tables (every character, FX, prop, tile, background, cutscene image, UI sprite with canvas, pivot, frames, fps, loop). Kept if it exists: the JSON is human-editable and the other two tools read it. |
| `Shatranj/Generate Placeholder Art` | **done** (Prompt 4) | `Art/GeneratePlaceholderArt.cs` + `Art/PlaceholderPainter.cs`, `PixelCanvas.cs`, `PixelFont.cs` | Writes a labelled placeholder PNG for every frame in the manifest at the exact size and path: silhouette by style, palette by character, name + anim + frame label in a 5×7 pixel font, 2 px edge outline, pivot marker, ±4 px motion per frame. Never overwrites an existing file. |
| `Shatranj/Import & Rebuild Animations` | **done** (Prompt 4) | `Art/RebuildAnimations.cs` + `Art/ArtImportSettings.cs` | Re-applies import settings to every PNG under `Assets/Art` (PPU 100, pivot from manifest, filter from ArtStyleConfig, no compression, no mipmaps, Full Rect; backgrounds repeat) and builds one `SpriteAnimationSet` per animated set under `Assets/Resources/Animations`, using the frames on disk. Updates in place, so GUIDs survive. The `ShatranjSpriteImporter` postprocessor applies the same rules to any future import. |
| `Shatranj/Build UI Prefabs` | **done** (Prompt 5) | `BuildUIPrefabs.cs` | Builds `Assets/Prefabs/UI/HUD.prefab` in code from the placeholder UI sprites: Canvas 1920×1080 (match 0.5), SoulVessel, Hearts, BossBar (+ stage ticks), FlameArc, AbilityPopup, CycleLabel, InteractPrompt, F10 HudDebugPanel. Needs TMP Essential Resources (Window → TextMeshPro → Import TMP Essential Resources) and says so if they are missing. Overwrites in place, so the GUID survives. |
| `Shatranj/Build Prefabs` | **done** for level props, Phase 1 + Phase 2 enemies, Kage Warden/Caller (Prompts 6–9, 12) | `BuildPrefabs.cs` + `EnemyPrefabFactory.cs` + `BossPrefabFactory.cs` | Builds `Assets/Prefabs/Level/{Bench,Door,BossGate,Orb,TutorialSign}.prefab` and `Assets/Prefabs/Enemies/{Crawler,Lunger,ShadowKite,Knight,Bat,Charger,Thrower}.prefab` (Phase 2 ones get a FlameScaler) from their EnemyConfigs (body collider, Hurtbox, Contact, Flash overlay, '!' marker, SpriteAnimator). Creates `Assets/Data/Materials/SpriteSilhouette.mat` once. Saved in place (GUIDs survive). |
| `Shatranj/Build Levels` | **done** (Prompt 6) | `Level/LevelBuilder.cs` + `TileAssets.cs` + `LevelOverviewWriter.cs` | Parses `Assets/Data/Levels/LevelManifest.json` + room `.txt` files into one scene per phase: Grid with Ground / OneWay / Spikes / Lava tilemaps (composite colliders, 16-tile bitmask styling from `Assets/Data/Tiles`), RoomBounds, EnemySpawners with stable ids, benches, doors, gates, orb, signs, safe points, boss arenas, plus camera, light, HUD, player, RoomManager. Prints the per-section PASS/FAIL table and writes `Docs/LEVEL_OVERVIEW.md`. Skips phases with no rooms yet. |
| `Shatranj/Build LevelTest Scene` | **done** (Prompt 6) | `Level/LevelBuilder.cs` | Same pipeline on `LevelManifest_Test.json` (rooms `TEST_A/B/C`) → `Assets/Scenes/LevelTest.unity`. Not part of the game. |
| `Shatranj/Validate Project` | **done** (Prompt 6, grows later) | `Level/ValidateProject.cs` | Layers; manifest + room parse errors; enemy counts per section vs Docs/01 §6 (PASS/FAIL/PENDING); room overlaps; opening alignment with neighbours and minimum opening height (6 / 8); enemies within 6 tiles of an entrance; benches in boss arenas; ground enemies on ground, bats directly under solid, Phase 2 ceiling ≥ 8 tiles over knight/charger/thrower patrol runs (Prompt 13); lava must sit on a floor (Prompt 14); horizontal gaps vs Docs/05 §2 (8 safe / 11 max, 12 / 16 with dash); missing prefabs; scenes in Build Settings. |
| `Shatranj/Build All Scenes` | **done** (Prompts 8, 11) | `BuildScenes.cs` | Ensures prefabs, HUD and the SpriteCatalog, builds MainMenu, Cutscene, every phase scene with rooms (Phase1_Cave, Phase2_Castle) and the Sandbox, orders Build Settings (MainMenu first), opens MainMenu. |
| `Shatranj/Build Cutscene Catalog` | **done** (Prompt 11) | `CutsceneCatalogBuilder.cs` | Rebuilds `Assets/Resources/SpriteCatalog.asset`: named sprites from Art/Cutscenes, Art/UI, Art/Backgrounds, the cutscene JSON TextAssets, the unlit material. |
| `Shatranj/Build Main Menu Scene` / `Build Cutscene Scene` | **done** (Prompt 11) | `MenuSceneBuilder.cs` | MainMenu: parchment stage, idle protagonist puppet on a ledge, MainMenuController. Cutscene: parchment stage, locked camera, CutsceneStage + CutsceneUI + CutsceneRunner. |
| `Shatranj/Balance Report` | planned (Prompt 17) | `BalanceReport.cs` | Prints every enemy/boss HP, hits-to-kill, damage and telegraph durations from the configs against Docs/04 §G targets. |
| `Shatranj/Build Windows Player` | planned (Prompt 18) | `BuildPlayer.cs` | Sets scene list, 1920×1080 windowed defaults, runs `BuildPipeline` into `/Builds`. |
| `Shatranj/Build Everything` | planned (after Prompt 8) | `BuildEverything.cs` | Runs Setup Project → Generate Config Assets → Generate Placeholder Art → Import & Rebuild Animations → Build Prefabs → Build UI Prefabs → Build Levels → Build All Scenes → Validate Project. |

Debug-only panels (runtime hotkeys, not menu items): F1–F9 per `CLAUDE.md` §6, F10 HUD fake-events (Prompt 5),
F11 visuals: `,`/`.` scrub flame t, M freeze, P park, G spike, T next theme, Y drop-shadows (Prompt 10), F12 flame-scale slider: drag or `[`/`]` (Shift fine) to set flameT, readout of target scale and scaling enemies; F4 for Phase 2 (Prompt 12).
Sandbox spawn keys at the cursor: 1 Crawler, 2 Lunger, 3 Kite, 4 Kage Warden, 5 Knight, 6 Bat, 7 Charger, 8 Thrower, Shift+1 Kage Warden, Shift+2 Kage Caller, Shift+3 Iron Colossus, 0 clear.

Scene builders (Sandbox, LevelTest, phase scenes) call `VisualRigFactory.Build` for the parchment stage, flame, lights, PostFX volume, themes and wipes.

## Assemblies
- `Assets/Scripts/Shatranj.Runtime.asmdef` — all gameplay code (references Input System, TMP, UI, URP).
- `Assets/Editor/Shatranj.Editor.asmdef` — this folder; references the runtime assembly.
- `Assets/Tests/EditMode/Shatranj.Tests.EditMode.asmdef` — NUnit EditMode tests; run them from Window → General → Test Runner.

## Conventions
- Namespace `Shatranj.EditorTools`; one static class per menu item; `[MenuItem("Shatranj/<Name>", false, <priority>)]`.
- Read all layer indices, masks, sorting-layer names and tags from `Shatranj.Core.GameLayers` — never hard-code them.
- Write settings through `SerializedObject` on the ProjectSettings singletons so changes persist to disk.
- Every tool prints one summary block to the Console and must be safe to run twice.
