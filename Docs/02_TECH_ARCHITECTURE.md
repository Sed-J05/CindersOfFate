# 02 — Tech Architecture & Unity Setup

## 1. Project setup (human does once)
1. Install **Unity 6 LTS** (any 6000.x LTS) — Unity 2022.3 LTS also works; Claude detects the version.
2. New project → template **Universal 2D** (URP + 2D Renderer). Name: `Shatranj`.
3. Package Manager must contain: Input System, Universal RP, 2D Tilemap Editor (usually included), TextMeshPro (`ugui`), Test Framework.
4. Project Settings → Player → *Active Input Handling* = **Input System Package (New)** (or **Both**).
5. Git: `git init`, Unity `.gitignore`, commit after every prompt. (Lifesaver in a competition.)
6. Copy `CLAUDE.md` to project root and `Docs/` next to `Assets/`.

## 2. Folder structure
```
Assets/
  Art/
    Characters/<Name>/<Anim>/<Name>_<Anim>_00.png      # see doc 06
    FX/  Tiles/  Backgrounds/  UI/  Cutscenes/  Props/
  Audio/                      # empty for now (hooks only)
  Data/
    Configs/                  # ScriptableObjects (Player, Enemies, Bosses, Themes, Flame)
    Levels/                   # ASCII room files  (*.txt)  + LevelManifest.json
    Cutscenes/                # *.json cutscene scripts
    AnimationManifest.json    # generated from doc 06
  Prefabs/  (Player, Enemies, Bosses, Projectiles, FX, UI, Level)
  Scenes/   MainMenu, Phase1_Cave, Phase2_Castle, (Cutscene, Ending are runtime-driven; see §8)
  Scripts/
    Core/ Player/ Enemies/ Bosses/ Level/ UI/ Cutscenes/ Visuals/ Data/ Utility/
  Editor/Shatranj/            # all editor tooling
  Settings/                   # URP assets, volume profiles
```

## 3. Scenes
| Scene | Purpose |
|---|---|
| `MainMenu` | Title (puppet-style), Start, Quit |
| `Phase1_Cave` | All Phase 1 rooms built from ASCII files |
| `Phase2_Castle` | All Phase 2 rooms |
| `Cutscene` | Generic stage that plays a JSON cutscene (loaded additively or as its own scene) |
| `Sandbox` | Dev-only test arena (movement/combat tests, enemy spawn keys) |

All scenes are created/registered in Build Settings by `Shatranj/Build All Scenes`.

## 4. Core systems (all in `Shatranj.Core` unless noted)
| System | Responsibility |
|---|---|
| `GameManager` (singleton, DontDestroyOnLoad) | Game state (`Menu, Playing, Cutscene, Paused, Dead`), current **Phase** (1/2), **Cycle** counter, ability flags, scene flow |
| `GameEvents` (static C# events) | `OnPlayerDamaged, OnPlayerDied, OnEnemyKilled, OnBossStarted/Defeated, OnRoomEntered, OnPhaseChanged, OnSoulChanged, OnHealthChanged, OnCutsceneStart/End` |
| `SaveSystem` | JSON in `Application.persistentDataPath`: phase, cycle, last bench id, flame progress at bench, killed enemy ids, defeated boss ids, hearts max, abilities |
| `GameLayers` | Static layer indices + `LayerMask`s |
| `IDamageable` / `DamageInfo` | `DamageInfo { int amount; Vector2 knockbackDir; float knockbackForce; GameObject source; DamageType type }` |
| `Health` component | HP, invulnerability, `OnDamaged/OnDied`, hit-flash hook |
| `HitStop` | Brief `timeScale` freeze (0.05–0.12 s) with unscaled timing |
| `ScreenShake` | Called by camera; amplitude/duration/falloff |
| `ObjectPool` | Generic prefab pool |
| `Tween` (utility) | Tiny coroutine/Update-free tween helpers: move, scale, color, fade, with easing curves |
| `AudioManager` | `Play(string id)` stub — safe no-op if no clip |
| `DebugHotkeys` | Per CLAUDE.md §6 |

## 5. Player/Enemy/Boss architecture
- Player: `PlayerController` (physics, input), `PlayerCombat`, `PlayerAbilities` (unlock flags), `PlayerSoul`, `PlayerHealth`, `PlayerAnimator`. States via enum state machine (doc 03).
- Enemies: `EnemyBase` (health, knockback, contact damage, hit-flash, death, soul-on-hit, room activation, scale hook) + one subclass per type; AI as small state machines; stats from `EnemyConfig` SO.
- Bosses: `BossBase` (health stages, intro, arena lock, boss-bar events, death → cutscene/door) + subclass per boss; moves as coroutines or state classes; stats from `BossConfig` SO.
- **Telegraph rule:** every damaging action goes `Telegraph → Active → Recovery`, durations in config.

## 6. Physics setup
- Player: dynamic `Rigidbody2D` (gravity scale controlled in code, `Freeze Rotation Z`, Interpolate, Continuous). `CapsuleCollider2D` 0.7×1.5.
- Ground checks via `BoxCast`/`OverlapBox` on `Ground` + `OneWay` layers. One-way via `PlatformEffector2D` **or** (preferred) custom: ignore collision while moving up / holding S+Space? (No drop-through needed; S+Space optional.)
- Layers: `Ground, OneWay, Player, PlayerHurtbox, Enemy, EnemyHurtbox, PlayerAttack, EnemyAttack, Hazard, Interactable, RoomBounds, Ignore`.
- Collision matrix configured by editor script `Shatranj/Setup Project` (creates layers via `TagManager` SerializedObject + sets `Physics2D.IgnoreLayerCollision`).
- Hit detection: attack hitboxes are `Collider2D` triggers activated on animation-timed frames (timed in code from config, not animation events, so placeholder art works).

## 7. Camera (custom, no Cinemachine)
`CameraController`: follows player with smoothing (~0.15 s), horizontal look-ahead 1.5 u, hold W/S 0.5 s to look ±3 u, **clamped to the current room's bounds**, blends smoothly between rooms (0.4 s), `Shake(amplitude, duration)`, `SetZoom(size, time)` for boss intros, `Lock(Vector2 pos)` for cutscenes/boss arenas. Camera also parents the **Parchment + FlameLight** rig (doc 07).

## 8. Scene flow
```
MainMenu → [Cutscene 1: Intro] → Phase1_Cave → (orb touched) → [Cutscene 2] → Phase2_Castle → (boss dead) → [Cutscene 3] → Cycle++ → [Cutscene 1 skip-able] → Phase1_Cave
```
`SceneFlow.LoadScene(name, fadeTime)` with a black/parchment-coloured fade overlay that persists across loads. Cutscenes run in the `Cutscene` scene, which receives a cutscene id (`GameManager.PendingCutscene`) and, when done, loads `GameManager.NextSceneAfterCutscene`.

## 9. Cycle / loop
`GameManager.Cycle` starts at 1. After Cutscene 3: `Cycle++`, reset phase to 1, clear killed-enemy and boss flags, **keep Cycle number**, show "Cycle N" card on Phase1 start. Cutscene 1 is skippable on cycles ≥ 2.

## 10. Editor tooling (build these; menu `Shatranj/…`)
| Menu item | Does |
|---|---|
| `Setup Project` | Creates layers, sorting layers, tags, physics matrix, time settings, quality, URP 2D renderer tweaks, folder structure, `.gitignore` check |
| `Generate Config Assets` | Creates every ScriptableObject config with doc defaults (skips existing) |
| `Generate Placeholder Art` | Writes placeholder PNGs for **every** sprite listed in doc 06 at the exact size/frame count (skips existing). Phase 1 = red/black enemies, Phase 2 = white/blue. Each frame has the entity name + frame number drawn on it |
| `Import & Rebuild Animations` | Sets texture import settings by folder; builds `AnimationClip`s + `AnimatorController`s from `AnimationManifest.json` & folders; safe to re-run after art changes |
| `Build Prefabs` | Creates all prefabs (Player, enemies, bosses, projectiles, FX, UI, benches, doors) wired to configs & animators |
| `Build Levels` | Parses `Assets/Data/Levels/*.txt` + `LevelManifest.json` → tilemaps, spawners, benches, doors, room bounds, into the Phase scenes |
| `Build All Scenes` | Builds MainMenu, Phase1_Cave, Phase2_Castle, Cutscene, Sandbox; updates Build Settings |
| `Validate Project` | Checks: layer setup, missing sprites/anim clips, enemy counts per section match doc 01 §6, room overlaps/size mismatches, missing prefab refs, unreachable benches. Prints a readable report in Console |
| `Build Everything` | Runs all of the above in order |

## 11. Art swap workflow (so artists never need programmers)
Artist overwrites a PNG (same path/name) → Unity reimports, sprite GUID kept → animations keep working. If frame count changed → run `Shatranj/Import & Rebuild Animations`.

## 12. Performance budget
60 fps on a mid laptop. Only the **current room + adjacent rooms** have active enemies/AI. Pool projectiles/FX. Max ~12 live enemies. Max 2 `Light2D`s visible (flame + global). One post-processing volume.

## 13. Testing
- `Sandbox` scene: keys `1–9` spawn enemies/bosses at the cursor, `0` clears. Debug hotkeys from CLAUDE.md §6.
- Add EditMode tests for: `DamageInfo` maths, `Health` invulnerability window, ASCII level parser (rooms/enemy counts), flame arc/scale curve, soul gain/spend.
