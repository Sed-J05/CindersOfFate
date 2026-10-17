# CLAUDE.md — Standing Rules for "Shatranj"

> Place this file in the ROOT of the Unity project (same level as `Assets/`). Claude Code reads it automatically.
> Place the `Docs/` folder next to it. Every task starts by re-reading this file.

## 1. What we are building
**Shatranj** — a 2D shadow-puppet action platformer in **Unity (C#)**, Hollow-Knight-style combat/movement, made for a vibe-coding competition.
The full design is in `/Docs`. Read the relevant doc(s) named in each task before writing code.

| Doc | Contents |
|---|---|
| `Docs/01_GAME_DESIGN.md` | Story, phases, loop, assumptions, global numbers |
| `Docs/02_TECH_ARCHITECTURE.md` | Unity setup, folders, systems, editor tooling, code rules |
| `Docs/03_PLAYER_SPEC.md` | Protagonist movement/combat numbers, state machine |
| `Docs/04_ENEMIES_AND_BOSSES.md` | Every enemy + boss: stats, AI, sizes |
| `Docs/05_LEVEL_DESIGN.md` | Room lists, ASCII level format, jump limits |
| `Docs/06_ART_AND_ANIMATION_SPEC.md` | Sprite dimensions, frame counts, naming, import rules |
| `Docs/07_VISUALS_SHADOW_PUPPETRY.md` | Parchment, flame, themes, inversion, lighting |
| `Docs/08_CUTSCENES_AND_UI.md` | Cutscene system + 3 cutscene scripts, HUD, menus |
| `Docs/09_BUILD_PROMPTS.md` | (for the human) the prompt sequence |
| `Docs/PROGRESS.md` | **You maintain this.** What's done / what's next |
| `Docs/DECISIONS.md` | **You maintain this.** Every judgment call you made |

## 2. How you work (non-negotiable)
1. **No stubs, no `// TODO implement`, no pseudo-code.** Every script must compile and do its job.
2. **Never hand-write `.unity`, `.prefab`, `.asset` YAML.** Generate scenes/prefabs/ScriptableObjects through **Editor scripts** (`Assets/Editor/Shatranj/`) exposed under the menu `Shatranj/…`. The human runs the menu item once; everything gets built.
3. **Minimise manual Editor work.** If the human must click something, list it as numbered steps at the end of your reply.
4. **Data-driven.** All numbers (speeds, HP, damage, timings) live in ScriptableObject configs, never magic numbers in logic. Defaults come from the docs.
5. **No paid / third-party assets.** Allowed packages ONLY: Input System, Universal RP (2D Renderer), 2D Tilemap, TextMeshPro, Unity Test Framework. **Do NOT use Cinemachine, DOTween, Odin, or Asset Store code.** Write our own camera and tweening.
6. **Placeholder art rule.** The team draws final art. Until it arrives, the generator creates placeholder PNGs at the exact sizes/names in doc 06. **Never overwrite an art file that already exists.**
7. **After every task reply with exactly these sections:**
   - `Files created/changed` (paths)
   - `Unity steps for you` (numbered; "none" if none)
   - `How to test` (a short checklist)
   - `Known limitations`
   Then update `Docs/PROGRESS.md` and, if you made any judgment call, `Docs/DECISIONS.md`.
8. **Don't stall.** If something is ambiguous, pick the most reasonable option, log it in `DECISIONS.md`, and continue. Only ask a question if it truly blocks progress.
9. **Don't regress.** Before finishing, re-read your own code for compile errors (missing usings, wrong API names, typos), and run `Shatranj/Validate Project` logic if it exists.
10. **Stay in scope.** Do only what the current task asks. Don't refactor unrelated systems.

## 3. Unity version / API care
- Read `ProjectSettings/ProjectVersion.txt` first and record the version in `DECISIONS.md`.
- Unity 6 (6000.x): use `Rigidbody2D.linearVelocity` (NOT `.velocity`), `FindFirstObjectByType` / `FindObjectsByType` (NOT `FindObjectOfType`), `Rigidbody2D.linearDamping`.
- Unity 2022.3: use `.velocity`, `FindObjectOfType`.
- Input: new Input System package only (`using UnityEngine.InputSystem;`). Build `InputAction`s **in code** (no `.inputactions` asset editing needed) with keyboard + gamepad bindings.
- 2D lights: `UnityEngine.Rendering.Universal.Light2D` (URP 2D Renderer). Sprites use `Sprite-Lit-Default` unless a doc says unlit.
- Text: TextMeshPro (`using TMPro;`). If TMP Essentials are missing, tell the human to run *Window → TextMeshPro → Import TMP Essential Resources*.
- When using an API you are not 100% sure exists in the detected version, use the safest alternative and say so.

## 4. Code style
- Namespaces: `Shatranj.Core`, `.Player`, `.Enemies`, `.Bosses`, `.Level`, `.UI`, `.Cutscenes`, `.Visuals`, `.Data`, `.EditorTools` (editor-only, inside `#if UNITY_EDITOR` or in an `Editor` folder).
- `[SerializeField] private` fields, `[RequireComponent]` where needed, short `/// <summary>` on public classes.
- Small single-responsibility classes. State machines = enum + `switch` or small state classes. No god-classes > ~400 lines.
- No `Find*` calls in `Update`. No per-frame allocations in gameplay (no LINQ, no `new` lists in Update). Pool projectiles, FX, damage numbers.
- Physics via layers + `LayerMask`s from a central `GameLayers` static class. No string-tag comparisons in hot paths.
- Time-based logic uses `Time.deltaTime` or a unified `GameTime` (so hitstop and pause work). Pause = `Time.timeScale = 0` + unscaled UI.

## 5. Game-feel priorities (when in doubt, favour these)
Responsive input (jump buffer, coyote time) → clear telegraphs on every enemy attack → hit-stop + screenshake + flash on hit → readable silhouettes → fair damage. Every enemy attack must have a visible telegraph (flash/pose/`!`) before it can hurt.

## 6. Debug hotkeys (only in Editor / Development builds)
`F1` god mode · `F2` +full soul · `F3` skip to next room · `F4` jump to Phase 2 (applies Phase 2 abilities/HP) · `F5` kill all enemies in room · `F6` teleport to current boss arena · `F7` toggle hitbox gizmos · `F8` set flame progress to 0.9 · `F9` reload scene.

## 7. Resuming in a new session
If context was lost: read this file → `Docs/PROGRESS.md` → `Docs/DECISIONS.md` → only the docs relevant to the next task. Do not re-read the whole repo.
