# PROGRESS (Claude keeps this updated after every prompt)

Legend: ✅ done · 🔧 in progress · ⬜ not started · ⚠️ blocked

| # | Prompt | Status | Notes |
|---|---|---|---|
| 0 | Kickoff & project setup | ✅ done (2026-10-17) | Folders + `.gitkeep`, `.gitignore`, `GameLayers.cs`, `Shatranj/Setup Project`, `README_DEV.md`, DECISIONS D-001…D-013. Human must run the menu item once (see below). |
| 1 | Core framework | ✅ done (2026-10-17) | `Scripts/Core`: GameManager, GameEvents, SaveSystem, SceneFlow, HitStop, ScreenShake, ObjectPool, Tween, AudioManager, DebugHotkeys/DebugCommands, Health/DamageInfo/DamageType/IDamageable. `Scripts/Data`: Player/Enemy/Boss/Theme/Flame/ArtStyle configs. `Shatranj/Generate Config Assets` + ConfigDefaults. 3 asmdefs. EditMode tests (Health 16, DamageInfo 10, SaveSystem 9). All compiled offline against Unity 6000.0.32f1. |
| 2 | Player movement, input, camera, sandbox | ✅ done (2026-10-17) | `Scripts/Player`: PlayerState, PlayerInput (code-built actions, buffers, K tap/hold), PlayerMotor (custom gravity, checks), PlayerController (state machine, all 14 states), PlayerAnimator (tint fallback, squash/stretch), PlayerDebugHud. `Scripts/Visuals`: ICameraTarget, CameraController. `Scripts/Level`: RoomBounds. `Scripts/Core`: SceneBootstrap. `Data`: CameraConfig + check fields on PlayerConfig. Editor: `Shatranj/Build Sandbox Scene`, SceneBuildUtil, PlayerFactory. Compiled offline. |
| 3 | Player combat, soul, heal, dash, fireball | ✅ done (2026-10-17) | `Scripts/Player`: PlayerCombat (side/up, A/B, timed hitbox, recoil, hit-stop, soul), PlayerHealth (hearts, hurt, blink, spikes → safe ground, death flow), PlayerHurtbox, PlayerSoul (+orb FX), PlayerSpells (heal hold / fireball tap), PlayerDash (Time Slip, ghosts), PlayerFxPools, SoulOrbFx, FireballProjectile. `Core`: DamageDealer, IBossTarget. `Visuals`: PlaceholderSprites, SpriteFx. `Enemies`: DamageableDummy. Sandbox: 4 dummies + 2 spike strips. F4 toggles phase abilities; F7 gizmos. |
| 4 | Placeholder art + animation pipeline | ✅ done (2026-10-17) | `Data`: ArtManifest classes, SpriteAnimationSet. `Visuals`: SpriteAnimator (no Unity Animator, D-057). Editor `Art/`: manifest tables + IO, PixelFont, PixelCanvas, PlaceholderPainter, `Generate Animation Manifest`, `Generate Placeholder Art`, `Import & Rebuild Animations`, ShatranjSpriteImporter postprocessor. PlayerAnimator plays Docs/06 clips per state with tint fallback; PlayerFactory wires SpriteAnimator. |
| 5 | HUD | ✅ done (2026-10-17) | `Scripts/UI`: HUDController, HeartsUI, SoulVesselUI, BossBarUI, FlameArcIndicator, AbilityPopupUI, CycleLabelUI, InteractPromptUI, HudDebugPanel (F10). GameEvents: OnBossStarted now carries stage thresholds; new OnFlameProgressChanged, OnAbilityUnlocked, OnInteractPromptShown/Hidden. Editor: `Shatranj/Build UI Prefabs` → `Assets/Prefabs/UI/HUD.prefab`; Sandbox builder instantiates it. Needs TMP Essentials. |
| 6 | Level pipeline | ✅ done (2026-10-17) | `Scripts/Level`: LevelData (glyphs, RoomData, manifest), LevelParser, RoomBounds (extended), RoomManager (current + adjacent activation, F3/F5/F6, safe points), EnemySpawner (stable ids, kill-save), InteractableBase, Bench (rest + checkpoint), Orb, TutorialSign, Door (flag-locked), BossGate, BossArena, SafeGroundPoint/PlayerSpawnPoint, LevelSceneSetup (respawn at bench on load). Core: GameManager.SetFlag + OnFlagChanged, LastFlameProgress. Editor: `Build Prefabs` (level props), `Build Levels`, `Build LevelTest Scene`, `Validate Project`, TileAssets, LEVEL_OVERVIEW writer. Data: TEST_A/B/C rooms, `LevelManifest.json` skeleton (sections + expected counts), `LevelManifest_Test.json`. Tests: LevelParserTests (13). |
| 7 | Enemy framework + Phase 1 enemies | ✅ done (2026-10-17) | `Scripts/Enemies`: EnemyBase (Health, white flash via silhouette shader, knockback, Hurt/Death clips, Death_Smoke, contact DamageDealer, room activation, spawner kill-save, telegraph flash + '!', flame-scale hook), EnemySensors (ground/ledge/wall/LOS/separation/sweep), Crawler, Lunger, ShadowKite, SandboxEnemySpawner (1/2/3 at cursor, 0 clear). `Visuals`: FxPool + PooledFx. Shader `Shatranj/SpriteSilhouette`. Editor: EnemyPrefabFactory, Build Prefabs builds the 3 enemies; Sandbox + Level builders ensure them. Tests: EnemyAcceptanceTests (6). |
| 8 | Phase 1 Section A + MB1 | ✅ done (2026-10-18) | Rooms P1_A0–A5 + P1_MB1 (15 enemies, all jumps checked) in `LevelManifest.json`; `Docs/LEVEL_OVERVIEW.md`. `Scripts/Bosses`: BossBase (intro + card, bar events, stages/enrage, hit windows, guard filter, slow-mo death, reset), KageWarden (advance, combo with turn-around, back-step, block + clink + pushback, counter-slash). BossArena spawns/despawns the boss. Health.DamageFilter; GameEvents OnBossReset/OnBossIntroCard; BossIntroCardUI. Editor: BossPrefabFactory, Build Prefabs boss, Sandbox key 4, `Build All Scenes`. Validate: hazard-only pits, PENDING sections. Tests: BossAcceptanceTests (2). |
| 9 | Phase 1 Section B + MB2 + orb | ✅ done (2026-10-18) | Rooms P1_B1–B6, P1_MB2, P1_ORB (20 enemies incl. 8 kites; Phase 1 = 35); MB1 east opened; every gate/door now in a 6-tile doorway. `Scripts/Bosses`: SwordsmanBoss (shared advance / slash combo / back-step), KageWarden refactored onto it, KageCaller (faster combo, Black Slash 1→2 shots < 66 %, summon with '!' floor markers + ceiling-slot drops, cap 3, die with boss, enrage < 50 %, sets door_orb). `Enemies`: EnemyProjectile + ProjectilePool (runtime-built pools). Orb placeholder sequence + ToBeContinuedCard. Sandbox key 5. Tests: Kage Caller numbers. |
| 10 | Shadow puppetry visuals | ✅ done (2026-10-18) | `Scripts/Visuals`: FlameController (P1 ping-pong 480 s, P2 one-way 1200 s with save/death restore, park, flicker, hit spikes, publishes t), FlameLight (Point Light 2D), ParchmentRig (tiled parchment + cross-fade, FlameGlow, FlameCore, ParchmentFrame), ParallaxLayer (0.1 / 0.3), PuppetShadow (2 black copies offset from the flame), ThemeManager (4 themes, cross-fade, follows phase), ScreenWipe (room sweep, death burn), VisualDebugPanel (F11). Editor: VisualRigFactory (rig, lights, PostFX volume + profile, theme sprite slots) in Phase1/Sandbox/LevelTest builders; PuppetShadow on player/enemies/bosses. Tests: FlameMathTests (6). |
| 11 | Cutscene system + CS1/CS2 + menus | ⬜ not started | |
| 12 | Phase 2 systems + enemies | ⬜ not started | |
| 13 | Phase 2 Section C + MB3 | ⬜ not started | |
| 14 | Phase 2 Section D + throne room | ⬜ not started | |
| 15 | Final boss Zulmat | ⬜ not started | |
| 16 | Ending cutscene + loop | ⬜ not started | |
| 17 | Polish & balance | ⬜ not started | |
| 18 | QA & build | ⬜ not started | sets `productName = Shatranj` for builds (D-002) |

## Environment facts (verified 2026-10-17)
- Unity **6000.0.32f1**, URP 17.0.3 (2D Renderer, `Assets/Settings/Renderer2D.asset`), Input System 1.11.2, ugui 2.0.0 (TMP), Test Framework 1.4.5, 2D feature set 2.0.1.
- Active Input Handling = **Both** (OK). Default screen 1920×1080.
- Project folder/product name is `cinders of fate` (D-002). Git repo initialised on branch `main`, first commit 2026-10-17.

## Pending human steps (carry forward until done)
1. **Shatranj → Build Prefabs** (drop shadows on enemies/bosses), then **Shatranj → Build All Scenes**.
2. Play Phase1_Cave; F11 opens the visuals panel. Test Runner → EditMode → Run All (63 tests).
3. Art-swap test from Prompt 4 is still unverified.

## Next up
Prompt 11 — Cutscene system + Cutscenes 1 & 2 + phase transition + menus (`Docs/09_BUILD_PROMPTS.md`).
