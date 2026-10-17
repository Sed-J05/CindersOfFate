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
| `Shatranj/Generate Placeholder Art` | planned (Prompt 4) | `GeneratePlaceholderArt.cs` | Writes placeholder PNGs for every sprite in Docs/06 (characters, FX, tiles, backgrounds, props, cutscene images, UI) at exact canvas size / frame count, labelled with a built-in pixel font. Never overwrites existing files. |
| `Shatranj/Import & Rebuild Animations` | planned (Prompt 4) | `RebuildAnimations.cs` | Applies import settings by folder (PPU 100, pivot, filter, no compression, Full Rect) and builds animation clips/controllers (or SpriteAnimator data) from `Assets/Data/AnimationManifest.json`. Re-runnable, preserves GUIDs. |
| `Shatranj/Build UI Prefabs` | planned (Prompt 5) | `BuildUIPrefabs.cs` | HUD prefab (hearts, soul vessel, boss bar, flame arc, ability popup, cycle label, interact prompt) from placeholder UI sprites. |
| `Shatranj/Build Levels` | planned (Prompt 6) | `BuildLevels.cs` | Parses `Assets/Data/Levels/*.txt` + `LevelManifest.json` into tilemaps (16-tile bitmask), one-way tilemap, hazards, EnemySpawners with stable IDs, benches, doors, gates, orb, signs, RoomBounds. Prints PASS/FAIL enemy-count table and writes `Docs/LEVEL_OVERVIEW.md`. |
| `Shatranj/Validate Project` | planned (Prompt 6) | `ValidateProject.cs` | Checks layer/sorting setup (`GameLayers.Validate`), missing sprites/clips, enemy counts per section vs Docs/01 §6, room overlaps/size mismatches, opening alignment, missing prefab refs. Readable Console report. |
| `Shatranj/Build Prefabs` | planned (Prompt 7, grows through 15) | `BuildPrefabs.cs` | Creates/updates prefabs for player, enemies, bosses, projectiles, FX, benches, doors, gates, orb, signs — wired to configs and animations. |
| `Shatranj/Build All Scenes` | planned (Prompt 8 / 11) | `BuildScenes.cs` | Builds `MainMenu`, `Phase1_Cave`, `Phase2_Castle`, `Cutscene`, `Sandbox`; registers them in Build Settings. |
| `Shatranj/Balance Report` | planned (Prompt 17) | `BalanceReport.cs` | Prints every enemy/boss HP, hits-to-kill, damage and telegraph durations from the configs against Docs/04 §G targets. |
| `Shatranj/Build Windows Player` | planned (Prompt 18) | `BuildPlayer.cs` | Sets scene list, 1920×1080 windowed defaults, runs `BuildPipeline` into `/Builds`. |
| `Shatranj/Build Everything` | planned (after Prompt 8) | `BuildEverything.cs` | Runs Setup Project → Generate Config Assets → Generate Placeholder Art → Import & Rebuild Animations → Build Prefabs → Build UI Prefabs → Build Levels → Build All Scenes → Validate Project. |

Debug-only panels (runtime hotkeys, not menu items): F1–F9 per `CLAUDE.md` §6, F10 HUD fake-events (Prompt 5),
F11 flame/theme scrub (Prompt 10), F12 flame-scale slider (Prompt 12).

## Assemblies
- `Assets/Scripts/Shatranj.Runtime.asmdef` — all gameplay code (references Input System, TMP, UI, URP).
- `Assets/Editor/Shatranj.Editor.asmdef` — this folder; references the runtime assembly.
- `Assets/Tests/EditMode/Shatranj.Tests.EditMode.asmdef` — NUnit EditMode tests; run them from Window → General → Test Runner.

## Conventions
- Namespace `Shatranj.EditorTools`; one static class per menu item; `[MenuItem("Shatranj/<Name>", false, <priority>)]`.
- Read all layer indices, masks, sorting-layer names and tags from `Shatranj.Core.GameLayers` — never hard-code them.
- Write settings through `SerializedObject` on the ProjectSettings singletons so changes persist to disk.
- Every tool prints one summary block to the Console and must be safe to run twice.
