# 09 — Build Prompts (paste these into Claude, in order)

**How to use**
1. Put `CLAUDE.md` in the Unity project root and the `Docs/` folder next to `Assets/`. Open Claude Code in the project root.
2. Paste **one prompt at a time**. Wait for it to finish → follow its "Unity steps for you" → press Play / test → commit to git → next prompt.
3. If Unity shows **red compile errors**, paste the **Fix Prompt** (bottom). If something feels bad, use the **Tune Prompt**.
4. Don't skip prompts; each builds on the last. You can run *Art-drop-in* any time your artists deliver sprites.
5. Each prompt already tells Claude which docs to read — don't paste the docs yourself if you use Claude Code. (If you use plain chat instead, attach the named docs with each prompt and ask for "complete files with full paths".)

Time guide (rough): Prompts 0–6 = core engine; 7–9 = Phase 1 playable; 10–11 = look + cutscenes; 12–15 = Phase 2 + bosses; 16–18 = finish. **Aim to have Phase 1 playable end-to-end before touching Phase 2.**

---

## PROMPT 0 — Kickoff & project setup
```text
You are the lead Unity engineer on "Shatranj", a 2D shadow-puppet action platformer for a game-dev competition. Read /CLAUDE.md fully, then read every file in /Docs (01 through 08) fully. Do not write gameplay code yet.

Step 1 — Reply with: (a) a 10-line summary of the game in your own words, (b) the Unity version you detected from ProjectSettings/ProjectVersion.txt and the API choices you'll use because of it, (c) any BLOCKING questions (max 5; skip if none — otherwise make decisions yourself and log them in Docs/DECISIONS.md).

Step 2 — Without waiting, build the foundation:
1. Create the folder structure from Docs/02 §2 (empty folders need a .gitkeep).
2. Create Docs/PROGRESS.md (checklist of Prompts 0-18 with status) and Docs/DECISIONS.md.
3. Write Editor script Assets/Editor/Shatranj/SetupProject.cs with menu "Shatranj/Setup Project" that: creates all Layers and Sorting Layers from the docs (edit TagManager.asset via SerializedObject), configures the Physics2D collision matrix, sets Time.fixedDeltaTime=0.01666, sets gravity to (0,-9.81) (we use custom gravity), makes sure the .gitignore is the standard Unity one (create if missing), and prints a summary.
4. Write Assets/Scripts/Core/GameLayers.cs (static class with layer indices + LayerMasks).
5. Write a short Assets/Editor/Shatranj/README_DEV.md describing every "Shatranj/..." menu item that will exist (planned ones marked "planned").
Then give me the standard end-of-task sections from CLAUDE.md §2.7.
```

## PROMPT 1 — Core framework
```text
Read /CLAUDE.md, Docs/02_TECH_ARCHITECTURE.md (§4, §5, §6), Docs/03_PLAYER_SPEC.md §7, and Docs/PROGRESS.md.
Implement the core framework in Assets/Scripts/Core and Assets/Scripts/Data:
- GameManager (singleton, DontDestroyOnLoad; GameState enum; Phase; Cycle; PlayerAbilities flags; PendingCutscene & NextSceneAfterCutscene), GameEvents (static events listed in doc 02), SaveSystem (JSON, per doc 02 §4), SceneFlow (async load with a persistent fade overlay Canvas created in code), HitStop, ScreenShake data hooks, ObjectPool, Tween utility, AudioManager (safe no-op), DebugHotkeys (CLAUDE.md §6; Editor/Development builds only).
- IDamageable, DamageInfo, DamageType, Health component (HP, i-frames, events, hit-flash hook).
- ScriptableObject configs with the exact default numbers from the docs: PlayerConfig (doc 03), EnemyConfig + BossConfig (doc 04, one field set that covers all enemy types), ThemeConfig + FlameConfig (doc 07), ArtStyleConfig (point/bilinear toggle, drop-shadow toggle).
- Editor menu "Shatranj/Generate Config Assets": creates every config asset (Player, all 7 regular enemies' EnemyConfigs, 4 BossConfigs, 4 ThemeConfigs, FlameConfig P1/P2, ArtStyleConfig) under Assets/Data/Configs with the doc defaults; skips existing.
- EditMode tests: Health i-frames, DamageInfo, SaveSystem round-trip.
Everything must compile. Don't build player/enemies yet.
```

## PROMPT 2 — Player movement, input, camera, sandbox
```text
Read /CLAUDE.md, Docs/03_PLAYER_SPEC.md (§1-3, §8, §11), Docs/02 §6-§7.
Implement: PlayerInput (new Input System, actions built in code, keyboard+gamepad per doc 03 §2, with buffered jump/attack/spell and tap-vs-hold detection for K), PlayerController (Rigidbody2D movement exactly to doc 03 §3: run, variable jump, coyote, buffer, gravity multipliers, max fall speed, ground check, one-way platform support, wall-stick prevention), PlayerState enum + state machine skeleton (all states from doc 03 §8 present even if not yet implemented), PlayerAnimator stub driven by state (uses placeholder-friendly fallback: if no animator clips exist, tint a SpriteRenderer by state), CameraController (doc 02 §7: smoothing, look-ahead, look up/down, bounds clamp from a RoomBounds component, shake, zoom, lock).
Create editor menu "Shatranj/Build Sandbox Scene" that builds scene Sandbox.unity: a flat floor, a few one-way platforms, steps, walls, spawn point, camera rig, player (built by code with a 0.7x1.5 capsule + coloured box sprite placeholder), a debug text showing state/velocity/grounded. Add it to Build Settings.
Acceptance: the doc 03 §11 feel checklist items for movement.
```

## PROMPT 3 — Player combat, soul, heal, dash, fireball
```text
Read /CLAUDE.md, Docs/03_PLAYER_SPEC.md (all), Docs/04 "Global rules".
Implement PlayerCombat (side/up attacks with timed hitboxes from PlayerConfig, alternating swings, recoil, hit-stop, buffered input, Slash FX spawn from pool with placeholder visuals if sprites are missing), PlayerHealth (hearts, hurt knockback, i-frames blink, hazards + safe-ground respawn, death flow signalled via GameEvents), PlayerSoul (gain on hit via flying SoulOrb FX, spend), Heal (hold K, rooted, cancel rules), Fireball (pierce, pooled projectile, cost 33, cast anim state), Dash "Time Slip" (i-frames, afterimage ghost pool, air-dash limit, cooldown) — fireball and dash must be gated by PlayerAbilities (locked in Phase 1). Add a dummy DamageableDummy target (training dummy with HP and hit-flash) and a spike hazard to the Sandbox. Debug hotkey F4 toggles abilities.
Update the Sandbox scene builder so rebuilding adds a dummy and spikes. Acceptance: all numbers match doc 03; hitboxes visible with F7 gizmos.
```

## PROMPT 4 — Placeholder art generator + animation pipeline
```text
Read /CLAUDE.md, Docs/06_ART_AND_ANIMATION_SPEC.md (all), Docs/02 §10-§11.
Build the art pipeline:
1. Assets/Data/AnimationManifest.json generated from Docs/06 tables (character, canvas, pivot, anims with frames/fps/loop). Keep it editable by humans.
2. Editor menu "Shatranj/Generate Placeholder Art": writes placeholder PNGs for EVERY sprite in doc 06 §3-§7 at the exact size/frame count/folder/name, per doc 06 §9 (silhouette shapes, labels, frame numbers, pivot marker, palette by character, tiny built-in pixel font coded by you). Skips existing files. Includes tiles Cave/Castle 16 bitmask tiles, one-way, spikes, backgrounds, props, cutscene images, UI sprites.
3. An AssetPostprocessor that applies import settings by path (PPU 100, pivot per manifest, filter from ArtStyleConfig, no compression, no mipmaps, Full Rect).
4. Editor menu "Shatranj/Import & Rebuild Animations": builds AnimationClips + one AnimatorController per character from the manifest (parameters/states named after anims; transitions are not needed because gameplay scripts will call Animator.Play(name) via a small SpriteAnimator helper — implement runtime SpriteAnimator that plays manifest-defined clips WITHOUT needing an Animator if that is simpler and more robust; choose and log the decision). Re-runnable; preserves GUIDs.
5. Wire the Protagonist's PlayerAnimator to real animations (Idle/Run/Jump/Land/Attack/Cast/Heal/Hurt/Death/Dash/Rest) with correct flip handling.
Acceptance: after running the two menu items the Sandbox player shows animated placeholders; replacing a PNG with another PNG of the same name updates it without code changes.
```

## PROMPT 5 — HUD
```text
Read /CLAUDE.md, Docs/08_CUTSCENES_AND_UI.md (Part 2 §6, §9), Docs/03 §5/§7, Docs/07 §4 (flame arc indicator).
Build the HUD: HUDController + HeartsUI (5 or 7, break/gain animations), SoulVesselUI (filled image, glow ≥33, smooth), BossBarUI (name, fill-in, stage ticks), FlameArcIndicator (Phase 2 only), AbilityPopupUI, CycleLabel, InteractPrompt. Created by an editor menu "Shatranj/Build UI Prefabs" using the generated placeholder UI sprites from Prompt 4. Subscribe to GameEvents only (no direct references to gameplay objects). Add HUD to the Sandbox scene builder. Include a debug panel (F10) that fakes damage/heal/soul to test the HUD.
```

## PROMPT 6 — Level pipeline (ASCII → scene), rooms, benches, doors
```text
Read /CLAUDE.md, Docs/05_LEVEL_DESIGN.md (all), Docs/02 §10, Docs/03 §10.
Implement: LevelParser (ASCII room files + LevelManifest.json per doc 05 §3 including headers, legend, validation with clear error messages), editor menu "Shatranj/Build Levels" producing a Tilemap (+CompositeCollider2D) per phase with automatic 16-tile bitmask styling (use the placeholder tiles), one-way tilemap, spikes/hazards, EnemySpawner markers with stable IDs, Bench (rest interaction: heal, save checkpoint), Door (flag-locked), BossGate, Orb, TutorialSign, RoomBounds, RoomManager (current room tracking, camera bounds switch with 0.4s blend, activate/deactivate enemies of current+adjacent rooms), Respawn system (death -> fade -> bench respawn, killed enemy IDs persisted, bosses reset).
Also "Shatranj/Validate Project": counts enemy glyphs per section and compares with Docs/01 §6, checks room size mismatches/overlaps, opening alignment between adjacent rooms, jump-limit violations where detectable, missing prefabs.
For testing, write 3 small TEST rooms (Assets/Data/Levels/TEST_*.txt) and build a scene "LevelTest" from them (not part of the game manifest). Do NOT write the real Phase 1/2 rooms yet.
```

## PROMPT 7 — Enemy framework + Phase 1 regular enemies
```text
Read /CLAUDE.md, Docs/04_ENEMIES_AND_BOSSES.md (Global rules + section A), Docs/06 §3.2, Docs/02 §5.
Implement EnemyBase (HP, hit-flash, knockback, death with Death_Smoke FX, contact damage via hitbox, room activation, soul orb trigger on player hits, stable-ID persistence, a FlameScaler hook that does nothing in Phase 1), shared AI helpers (ledge/wall check, line of sight, player distance, separation), and enemies: Crawler, Lunger, Shadow Kite — each with the exact numbers/behaviour/telegraphs in doc 04 §A. SpriteAnimator hookup with the Prompt-4 animations. Editor menu "Shatranj/Build Prefabs" creates prefabs for the 3 enemies (placeholder visuals, colliders, configs, hurtbox/hitbox layers). Add enemy-spawn keys to the Sandbox (1=Crawler, 2=Lunger, 3=Kite, 0=clear). Update the level builder so c/l/f glyphs spawn the prefabs. Tests: acceptance = every attack has a visible telegraph and enemies can be killed in the number of hits stated.
```

## PROMPT 8 — Phase 1 levels, Section A + Mini-boss 1
```text
Read /CLAUDE.md, Docs/05 §4 Section A, Docs/04 §B1, Docs/08 §6 (boss bar).
1. Author the REAL room files P1_A0..P1_A5 and P1_MB1 as ASCII in Assets/Data/Levels/ (exact sizes, enemy counts and notes in doc 05; obey the platforming limits in doc 05 §2; signs text in manifest) and add them to LevelManifest.json. Keep them fun but SIMPLE. Write Docs/LEVEL_OVERVIEW.md with an ASCII overview map.
2. Implement BossBase (stages, intro card, arena lock via BossGate, boss bar events, death hooks, reset on player death) and Kage Warden exactly per doc 04 §B1 (advance, combo with direction logic, back-step, block with clink + pushback, counter-slash, no repeated block, slow-mo death, gate opens).
3. Add Build Prefabs support for the boss, and a Sandbox key (4) to spawn it.
4. Build "Phase1_Cave" scene (Build All Scenes: scene with player, camera rig, HUD, RoomManager, level geometry) and run Validate Project: Section A must report PASS (15 enemies).
Acceptance: playable from the cave mouth through MB1 with benches, deaths, and respawns working.
```

## PROMPT 9 — Phase 1 Section B, Mini-boss 2, door, orb
```text
Read /CLAUDE.md, Docs/05 §4 Section B, Docs/04 §B2, Docs/08 §3 step 1.
Author rooms P1_B1..P1_B6, P1_MB2, P1_ORB (counts per doc 05: 20 enemies incl. Shadow Kites), update manifest/overview. Implement Kage Caller exactly per doc 04 §B2: regular slashes, BlackSlash projectile (pooled, 1 then 2 below 66%), Summon with Exclamation markers at the drop X positions and ceiling-drop minions (cap 3, die with boss), <50% enrage, door unlock on death (flag door_orb). Implement the Orb: touching it fires GameEvents.OnOrbTouched which (for now) just plays a placeholder fade and loads Phase2_Castle if it exists, else shows "To be continued" card.
Validate Project must report Section B PASS (20) and total Phase 1 = 35. Playtest: full Phase 1 from start to orb.
```

## PROMPT 10 — Shadow puppetry visuals + themes + flame
```text
Read /CLAUDE.md, Docs/07_VISUALS_SHADOW_PUPPETRY.md (all), Docs/06 §5.2, Docs/01 §12.
Implement the visual rig: ParchmentRig (camera child; parchment texture tiled to screen, tint, ParchmentFrame vignette, FlameGlow sprite), FlameController (arc math, Phase 1 slow ping-pong, Phase 2 one-way arc, parking for final boss, save/restore flameT at benches, flicker), FlameLight (Light2D position/colour/intensity/flicker), URP 2D setup (Global Light 2D + one Point Light 2D; a Volume with Bloom, Vignette, Film Grain, Color Adjustments), PuppetShadow component (drop shadows per doc 07 §5) auto-added to player/enemies, BG parallax layers, ThemeConfig + ThemeManager with cross-fade (Phase1_Beige, Phase2_Inverted, Ending_Purple, Cutscene), room-transition parchment wipe, death burn-wipe. Wire into Phase1_Cave and Sandbox scene builders.
Keep to Sprite-Lit/Unlit default shaders (doc 07 §8). Provide a debug panel (F11) to scrub flame t, swap themes, and toggle drop-shadows. Acceptance: doc 07 §9 checklist items for Phase 1.
```

## PROMPT 11 — Cutscene system + Cutscenes 1 & 2 + Phase transition
```text
Read /CLAUDE.md, Docs/08_CUTSCENES_AND_UI.md (Part 1 all), Docs/07 §7.
Implement CutsceneRunner + beat types (all in doc 08 §1) with JSON parsing (use JsonUtility-compatible classes or Newtonsoft only if already in the project; otherwise write a small custom parser — choose and log), puppet stage (spawn/anim/move/scale/tint/alpha/remove), full-screen image panels with Ken Burns, typewriter TMP caption box (cut-paper styling from placeholder UI), skip-hold ring, input advance, parallel beats, invertWipe, theme/flame beats, setFlag/loadScene. Create scene "Cutscene" via Build All Scenes. Write Assets/Data/Cutscenes/cs1_intro.json and cs2_orb.json exactly per doc 08 §2 and §3 (add timings and positions; placeholder images from Prompt 4). Hook: MainMenu Start -> cs1 -> Phase1_Cave; Orb touched -> cs2 -> Phase2_Castle (set phase=2, hearts 7, abilities on). Create the MainMenu scene + pause menu per doc 08 §7.
Acceptance: New Game -> Intro -> Phase 1 -> Orb -> Cutscene 2 -> blue-flame inversion wipe -> loads Phase2_Castle (use a stub scene with just the first room if it isn't built yet).
```

## PROMPT 12 — Phase 2 systems + Phase 2 regular enemies
```text
Read /CLAUDE.md, Docs/04 §C, Docs/07 §3-§4, Docs/03 §6/§9, Docs/01 §5, §12.
Implement: Phase 2 state (7 hearts, dash+fireball unlocked, full heal), FlameScaler (monotonic/peak modes, lerp smoothing, collider scaling, wall-nudge safety, flameT restore on death), the "better AI" toolkit (spacing, retreat-after-attack, ledge avoidance, reacts to player jump, separation, feints, lose-track timer) as reusable components, and enemies Knight, Bat (ceiling hang, wake, wander, desynced dives), Charger (telegraph, charge, wall stun), Thrower (spacing, lead-aimed arcing knives with scaling) — all per doc 04 §C incl. exact numbers. Build Prefabs for them; Sandbox keys 5-8; Sandbox slider (F12) to set flameT and see scaling. Add Theme_Phase2 usage when Phase2 scenes load. Write unit tests for the flame-scale curve and spacing logic.
```

## PROMPT 13 — Phase 2 Section C + Mini-boss 3 (Iron Colossus)
```text
Read /CLAUDE.md, Docs/05 §5 Section C, Docs/04 §D, Docs/06 §3.5.
Author rooms P2_C0..P2_C6 and P2_MB (counts per doc 05, castle theme, vertical rooms C2 and C4 with one-way platform stacks, ceiling clearance rule, bats under ceiling beams). Implement Iron Colossus per doc 04 §D: Big Arc Slash, Big Shield Block + bash, Jump Slam with shockwave, Dash, distance-based selection with no 3x repeat, enrage <40%, 2-heart damage, no poise, bar + intro card, gate logic. Build Phase2_Castle scene (with the Phase 2 theme and flame arc running from the moment the player controls Phase 2). Validate Project: Section C PASS (20).
```

## PROMPT 14 — Phase 2 Section D + throne room
```text
Read /CLAUDE.md, Docs/05 §5 Section D, Docs/07 §3.
Author rooms P2_D1..P2_D8 and P2_THRONE (counts per doc 05 — 30 enemies; introduce charger in D1 and thrower in D2; static lava in D6; tall towers D2/D4), update manifest + LEVEL_OVERVIEW.md. Add static Lava hazard behaviour. Validate must show Section D PASS (30) and Phase 2 total 50. Full playtest of Phase 2 up to the throne-room door; list any difficulty spikes and fix by adjusting enemy placement only.
```

## PROMPT 15 — Final boss Zulmat
```text
Read /CLAUDE.md, Docs/04 §E, Docs/07 §3 (boss parking), Docs/06 §3.5 and §4 (Vine, Lava_Zone, Giant_Swipe).
Implement Zulmat exactly per doc 04 §E: 90 HP, 3 stages with roar + invulnerable transition, Sword Slash, Vine Summon (sequential vines with ground warn), Giant Swipe (grow x1.8 tween, big sweep dodgeable with dash/jump), Shrink & Lunge (x0.5, smoke, double lunge), Summon Regulars while small (cap 4, vulnerable while summoning), Lava Zones (telegraph, 6 s, max 3), stage-3 combos, weighted selection with no immediate repeat + 0.6 s fairness gap. Flame parks during the fight; flame light pulses on moves. Boss bar with stage ticks. Death: 2.5 s slow-mo + white flash then GameEvents.OnFinalBossDefeated. Sandbox key 9 spawns him. Add tuning fields so I can change HP/telegraph durations in the BossConfig only.
```

## PROMPT 16 — Ending cutscene, loop, cycles
```text
Read /CLAUDE.md, Docs/08 §4, Docs/02 §8-§9.
Write cs3_ending.json per doc 08 §4 and hook OnFinalBossDefeated -> cs3 -> cycle reset (Cycle++, phase=1, clear progress, keep cycle count) -> optional cs1 (skippable on cycles >=2) -> Phase1_Cave with a "Cycle N" card. Implement the purple flame theme, merge cross-fade, "camera pans back" rewind effect, credits card. Make sure SaveSystem handles Continue properly after each phase and that finishing the game never leaves stale flags (door_orb, killed enemies, boss flags, flameT).
Acceptance: play from boss death through the loop twice without errors.
```

## PROMPT 17 — Polish, juice, balance
```text
Read /CLAUDE.md, Docs/04 §G, Docs/03 §11, Docs/07 §9.
Do a polish pass WITHOUT adding scope: hit-stop/shake/flash consistency, dust puffs, damage feedback, enemy death smoke, boss intro/outro, menu polish, ability popup, bench rest FX, screen-edge room transitions, rumble hooks (gamepad), audio hook calls at every event (AudioManager.Play("player_hit") etc. — list all ids in Docs/AUDIO_HOOKS.md for later), performance check (pooling, active-room culling). Then a BALANCE pass using Docs/04 §G targets: expose any needed values in configs; add a "Shatranj/Balance Report" tool that simulates nothing but prints each enemy/boss's HP, hits-to-kill, damage, telegraph times. Fix anything that violates doc targets.
```

## PROMPT 18 — QA, bug-bash, build
```text
Read /CLAUDE.md and Docs/PROGRESS.md.
1. Run "Shatranj/Validate Project" and fix everything it reports.
2. Write Docs/QA_CHECKLIST.md covering: every room traversable, every enemy type, every boss (incl. death + respawn mid-fight), benches, doors, cutscenes + skip, pause during each state, gamepad, phase transition, cycle loop, save/continue, resolution 1920x1080 & 1366x768 & 2560x1440 (camera/UI scale), alt-tab, window resize.
3. Review the code for: null refs on scene load, event unsubscription leaks, pooled objects not reset, coroutines continuing after death, timeScale left at 0 after pause/hit-stop (add a safety reset), double-registered DontDestroyOnLoad singletons.
4. Add "Shatranj/Build Windows Player" (script that sets scenes in Build Settings, resolution defaults, windowed 1920x1080 default, runs BuildPipeline into /Builds). List remaining known bugs in Docs/KNOWN_ISSUES.md.
```

---

## UTILITY PROMPTS

### FIX — compile errors / exceptions
```text
Unity shows errors. Read /CLAUDE.md. Fix ALL of them at the root cause (don't hide with try/catch). Here are the Console messages exactly as shown:

<PASTE CONSOLE ERRORS HERE>

Reply with the cause of each error, the files changed, and any Unity steps. Don't change unrelated code.
```

### TUNE — feel / balance
```text
Read /CLAUDE.md and the relevant doc. Problem: <describe, e.g. "jump feels too floaty", "Kage Warden block is unfair">. Propose a change with numbers from the config (not code logic), apply it to the config defaults, and explain what to expect. Keep the old value in a comment in DECISIONS.md.
```

### ART DROP-IN — artists delivered sprites
```text
Read /CLAUDE.md and Docs/06. Artists added/replaced sprites in Assets/Art. Run the import + animation rebuild logic, then check every character in Assets/Data/AnimationManifest.json for: wrong canvas size, wrong frame count, missing frames, wrong pivot, non-transparent backgrounds, inconsistent sizes within one character. Print a table of problems and fix whatever can be fixed automatically (e.g. import settings, manifest frame counts if I changed the animation lengths on purpose: <list changes>). Never overwrite my art.
```

### ADD FEATURE (safe template)
```text
Read /CLAUDE.md and Docs/PROGRESS.md. New request: <feature>. First list which existing files you will touch and any risk of regression, then implement it data-driven, update the docs that this changes (only the relevant section), and give me the standard end-of-task sections.
```

### CONTEXT REFRESH (new chat / "you forgot things")
```text
Read /CLAUDE.md, Docs/PROGRESS.md, Docs/DECISIONS.md. Then tell me in 8 lines where we are and what the next prompt should be. Don't change any code.
```

### BIG BUG HUNT
```text
Read /CLAUDE.md. Do a static code review of Assets/Scripts for bugs that would appear in a long play session: event leaks, null refs after scene reload, coroutines that survive death/scene change, pooled objects not reset, timeScale stuck, save/load edge cases, race conditions between cutscene/scene load/state changes. Produce a prioritised list, then fix the top issues. Show a diff summary.
```
