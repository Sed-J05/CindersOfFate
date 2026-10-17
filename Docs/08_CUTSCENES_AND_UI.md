# 08 — Cutscenes & UI

# PART 1 — CUTSCENES

## 1. How we implement them (decision)
**Data-driven "puppet theatre" sequencer** — no Timeline, no video.
- A cutscene is a **JSON script** in `Assets/Data/Cutscenes/<id>.json`: an ordered list of **beats**. A `CutsceneRunner` (MonoBehaviour in the `Cutscene` scene) executes them with coroutines.
- The stage is the same **parchment + flame** rig as gameplay, so cutscenes look native. Characters are **puppets**: the in-game sprites (Idle/Run/Death…) or full-screen silhouette images from doc 06 §6.
- Text is shown with **TextMeshPro typewriter** (~35 chars/s) in a cut-paper caption box at the bottom. Press **J/Enter/Space** to advance/finish the line, **hold Esc/Enter 1 s to skip** the whole cutscene (shows a "Skipping…" ring).
- Why: team can edit text/timings in a text editor without Unity; art is reused; no heavy tooling; Claude can generate and tweak scripts easily.

### Beat types
| `type` | Fields | Effect |
|---|---|---|
| `fade` | `to: black\|clear\|parchment`, `time` | Fade overlay |
| `theme` | `theme: Phase1_Beige\|Phase2_Inverted\|Ending_Purple\|Cutscene`, `time` | Switch theme/flame colours (cross-fade) |
| `flame` | `t`, `colour`, `intensity`, `time` | Move/recolour flame |
| `spawn` | `id`, `sprite` or `character+anim`, `pos[x,y]`, `scale`, `layer`, `flip` | Create a puppet |
| `anim` | `id`, `anim`, `loop` | Play animation on puppet |
| `move` | `id`, `to[x,y]`, `time`, `ease` | Tween position |
| `scale` / `rotate` / `tint` / `alpha` | `id`, value, `time` | Tweens |
| `image` | `id`, `sprite`, `alpha`, `kenburns:{from,to,time}` | Full-screen panel with slow pan/zoom |
| `remove` | `id`, `fade` | Remove puppet |
| `text` | `speaker`, `lines[]`, `style: narration\|inner\|boss` | Caption(s) |
| `wait` | `time` | Pause |
| `shake` | `amp`, `time` | Camera shake |
| `flash` | `colour`, `time` | Screen flash |
| `parallel` | `beats[]` | Run beats concurrently (wait for all) |
| `sfx` / `music` | `id` | `AudioManager.Play(id)` (no-op for now) |
| `invertWipe` | `time` | The Phase-2 inversion circle wipe (doc 07 §7) |
| `loadScene` | `scene`, `fade` | End: load the next scene, apply `GameManager` flags |
| `setFlag` | `flag`, `value` | e.g. `phase=2`, `cycle++` |
Coordinates: **world units relative to the screen centre**, screen = 19.6 × 11 u.
Runner API: `CutsceneRunner.Play(id, onComplete)`. Player input is disabled via `GameManager.State = Cutscene`.

## 2. Cutscene 1 — INTRO (≈ 40 s)
Theme: Phase1_Beige, flame orange slow.
| # | Beat | Details |
|---|---|---|
| 1 | fade black→parchment 1.5 s | Title card "SHATRANJ" cut-paper logo for 2.5 s, fade |
| 2 | `image` CS1_Village parallax (3 layers) | Ken-Burns slow pan right; flame flickers behind |
| 3 | text (narration) | "Our village slept beneath the mountain's shadow." |
| 4 | spawn **Crawler / Lunger puppets** crossing the village silhouette | shake small, flame flares |
| 5 | text | "Then the shadows began to move." |
| 6 | spawn **Protagonist** puppet (Idle) at village, looks at the mountain | text (inner): "The source of this lies in the cave." |
| 7 | protagonist `Run` walks right across screen 3 s; BG slides to cave mouth image | |
| 8 | fade to black → `loadScene Phase1_Cave` | New game spawns at `P` in `P1_A0` |
On later cycles (Cycle ≥ 2) add a first line "…again." and make it skippable instantly.

## 3. Cutscene 2 — THE ORB / PHASE CHANGE (≈ 70 s)
Triggered on touching the orb at the end of Phase 1. Theme starts Phase1.
| # | Beat | Details |
|---|---|---|
| 1 | Player freeze, orb pulses; `flash white 0.3` | Orb `Touch` anim |
| 2 | `theme Cutscene`, background dims; fast-cut `image` CS2_Memory_01..06, each 1.8 s with shake/flash | Silhouette memories: a figure of light and shadow; a crown of fire; a great hall; two beings divided; a pale hand reaching; a castle on a cliff |
| 3 | text (inner) over memories | "I remember… a throne of light." / "I remember… something I cast away." / "Its home is the castle beyond the cave." |
| 4 | `image` CS2_Castle_Silhouette (Ken Burns up) | Red-black castle, one window glowing |
| 5 | `image` CS2_Zulmat_Eyes: two crimson eyes open in the dark, `shake 0.6`, flame light flares | text (boss): "…You remember. Then I am awake." |
| 6 | back to stage: `theme Phase2_Inverted 0` **via `invertWipe 1.5 s`**, `flame colour → blue`, `flame t = 0` | Flame turns **blue**, faster |
| 7 | text (narration) | "The light behind the screen changed. So did everything on it." |
| 8 | ability popup: "TIME SLIP (L)" and "FLAMECALL (K)" unlocked, protagonist glows | `setFlag phase=2`, hearts max 7, full heal |
| 9 | `loadScene Phase2_Castle` | Spawn at `P` in `P2_C0`, "Phase II" card |

## 4. Cutscene 3 — ENDING / THE LOOP (≈ 100 s)
Triggered when Zulmat's death anim ends.
| # | Beat | Details |
|---|---|---|
| 1 | Zulmat kneels (death pose), flame **flickers**, `theme Ending_Purple 3 s` | Flame turns **purple** |
| 2 | text (boss) | "Look at me. You know my face." |
| 3 | spawn Protagonist + Zulmat puppets facing each other, they **mirror** each other's pose; Zulmat smiles | text (inner): "He is… me." |
| 4 | `image` memory flashes CS3_Merge_01..03: a god whole, then cut in two (a bright half and a dark half) | text: "I was a god. I tore out my cruelty, and it became him. I tore out my mercy, and it became me." |
| 5 | puppets walk toward each other and **merge** (cross-fade into one glowing puppet), `flash white` | |
| 6 | `image` CS3_GodsStab: 3 hooded gods surround, a blade enters the merged figure; `shake`, red flash | text (boss/gods): "Whole, you are too much." / "Be halved again." |
| 7 | `image` CS3_Split: the figure splits into light/dark; memory fades to black | text (narration): "Two halves, each too weak to resist. Their memories locked away." |
| 8 | **Pan back:** camera zooms out of the stage, the paper "rewinds" (flame goes from purple → orange, t = 0), the village returns | `setFlag cycle++, phase=1, clear progress` |
| 9 | text: "Cycle 2" caption, `fade` | `loadScene Phase1_Cave` (or Cutscene 1 if not skipped) |
End credits: out of scope; show "Thanks for playing — Team Shatranj" for 4 s before the loop (editable text).

## 5. Cutscene assets checklist (doc 06 §6)
`CS1_Village_Far/Mid/Near` · `CS1_CaveMouth` · `CS2_Memory_01..06` · `CS2_Castle_Silhouette` · `CS2_Zulmat_Eyes` · `CS3_Merge_01..03` · `CS3_GodsStab` · `CS3_Split` · `God_A/B/C`. Missing images → generator makes a labelled grey placeholder at 1920×1080.

---

# PART 2 — UI

## 6. HUD (Screen Space – Overlay, Canvas Scaler 1920×1080, match 0.5) — Hollow-Knight-like
```
[Soul vessel]  [♥ ♥ ♥ ♥ ♥ (♥ ♥)]                         [flame arc indicator (P2)]
 top-left          top-left, right of soul                         top-centre

                                    [Boss name + bar, bottom-centre during boss]
```
| Element | Spec |
|---|---|
| Soul vessel | 192×192 at (60, −60) from top-left. `Soul_Frame` + `Soul_Fill` (`Image.Filled`, vertical, **fill = soul/99**, smooth 0.2 s) + `Soul_Glow` pulsing when ≥ 33. Fill is **dark shadow liquid with blue rim** (HK's white soul replaced by shadow power). Phase 2: fill colour inverted tint |
| Hearts | 64×64 each, spacing 8 px, start at (270, −70). Full/Empty; **Heart_Break** anim (0.25 s) when lost; **gain pulse** when healed. 5 hearts P1, 7 P2 (new ones fade in at phase change) |
| Boss bar | `BossBar_Frame/Fill` 900×48 at bottom-centre (y = 90). Boss name above in a cut-paper font. Fills in on engage (0.8 s), shows stage ticks for Zulmat (at 67 %, 34 %) |
| Flame arc indicator (P2) | Thin arc (200×60) top-centre with a glowing dot moving along it with flameT; turns red-tinged above 0.75 |
| Ability popup | When unlocked: icon + name, 3 s, fades |
| Cycle label | "Cycle N" small, bottom-left, only if N ≥ 2 |
| Interact prompt | "W — Rest" over benches, "W — Touch" over orb |
Fonts: TextMeshPro default (swap later). Use `Resources/` or a TMP asset in `Assets/Art/UI/Fonts/`.

## 7. Menus
| Screen | Content |
|---|---|
| **Main Menu** (scene) | Parchment stage; Title_Logo; buttons: **Start**, **Continue** (if save), **Quit**; flame flickers; a puppet of the protagonist idles on a ledge |
| **Pause** (Esc) | Dim overlay; **Resume · Restart from bench · Quit to menu**; mouse + keyboard + gamepad navigation |
| **Death** | Burn-wipe → 1.2 s "You were extinguished" (TMP, fade) → respawn |
| **Boss intro card** | Boss name + 1-line epithet slide in for 1.5 s ("KAGE WARDEN — Keeper of the Gate") |
| **Phase card** | "PHASE II — THE INVERTED THEATRE" at start of Phase 2 |
| **Ending** | "Thanks for playing" + restart loop |

### Boss names & epithets
MB1 *Kage Warden — Keeper of the Gate* · MB2 *Kage Caller — Summoner of Shadows* · MB3 *Iron Colossus — The Unmoving* · Final *Zulmat — The Severed*.

## 8. Input handling in UI
Navigation via arrows/WASD + Enter/Space + mouse; gamepad D-pad + A. Use UnityEngine.UI `Button` with `Navigation = Explicit` or a tiny custom selector; Event System uses `InputSystemUIInputModule`.

## 9. Acceptance checklist
- [ ] Hearts/soul update instantly and correctly in all situations (heal, damage, phase change).
- [ ] Boss bar shows only in boss rooms.
- [ ] Pause works in all states except cutscenes (where Esc = skip hold).
- [ ] Cutscene text can be edited in JSON and picked up without recompiling.
- [ ] Cutscenes 1–3 play, skip, and end at the right scene with correct flags.
