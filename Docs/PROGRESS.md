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
| 6 | Level pipeline | ⬜ not started | includes `Validate Project` |
| 7 | Enemy framework + Phase 1 enemies | ⬜ not started | |
| 8 | Phase 1 Section A + MB1 | ⬜ not started | |
| 9 | Phase 1 Section B + MB2 + orb | ⬜ not started | |
| 10 | Shadow puppetry visuals | ⬜ not started | also picks up URP/quality tweaks deferred from Prompt 0 (D-011) |
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
1. **Window → TextMeshPro → Import TMP Essential Resources** (one-time; creates `Assets/TextMesh Pro`).
2. **Shatranj → Build UI Prefabs**, then **Shatranj → Build Sandbox Scene**, press Play. F10 opens the HUD debug panel.
3. Art-swap test from Prompt 4 is still unverified (copy a PNG over `Protagonist_Idle_00.png`, expect the idle to update with no further steps).

## Next up
Prompt 6 — Level pipeline (`Docs/09_BUILD_PROMPTS.md`).
