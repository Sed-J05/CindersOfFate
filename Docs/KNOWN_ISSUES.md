# KNOWN ISSUES — Cinders of Fate

Open issues after Prompt 18 (QA & build). Severity: **High** = blocks a playthrough, **Med** = noticeable, **Low** = cosmetic / content. Fixed during QA is listed at the bottom for reference. Add new findings from `Docs/QA_CHECKLIST.md` here with steps to reproduce.

## Open

| # | Severity | Area | Issue | Notes / workaround |
|---|---|---|---|---|
| 1 | Med | Audio | No sound, music or voice files exist yet — every hook is silent. | See `Docs/AUDIO_AND_VOICE.md` (all 18 voice lines + 87 sounds, folders and direction). Drop clips named after the ids into `Assets/Resources/Audio/` (sfx) or `Audio/Music/` (music); no code needed. |
| 2 | Med | Art | All art is procedurally generated (`Tools/artgen`, D-180): readable and on-style, but simpler than hand-drawn art (stiff rig poses, simple cutscene panels, repeating tile texture). | Team art replaces them by file name (Docs/06); the art-swap test from Prompt 4 is still unverified. |
| 3 | Med | Balance | Fight lengths are untested against Docs/04 §G. The Balance Report's punish-window estimates are well under target (Kage Caller ≈ 23 s vs 70–110 s is flagged). | Playtest with a stopwatch; tune HP / telegraphs in the configs and rerun `Shatranj → Balance Report`. |
| 4 | Med | Difficulty | The Phase 2 difficulty pass (D-162) was an analysis of the room files, not a hands-on playtest. | Play Section C/D and move enemies (placement only) if a room spikes. |
| 5 | Low | Save | Building the player sets company/product to "Team Cinders of Fate / Cinders of Fate", which moves `Application.persistentDataPath`: saves made in the Editor before the first build are no longer found. | Start a New Game once after the first build. |
| 6 | Low | Cutscene 3 | The "camera pans back" zoom shrinks the stage but doesn't reveal the parchment edges, because the paper rig re-fits to the camera every frame (D-171 said otherwise). | Reads fine with the rewind streaks; a frame reveal would need the rig to stop fitting during the beat. |
| 7 | Low | Visuals | Player FX (slash, fireball, heal aura) are white-blue in both phases; the Phase 2 protagonist art is black/red. | Recolour the FX art or extend the shader to them. |
| 8 | Low | Enemies | A sleeping bat hanging flush under a ceiling can't grow with the flame until it wakes (the wall-fit check blocks it). | By design of the safety check; invisible in play. |
| 9 | Low | Boss | The Iron Colossus can still be hit while standing on the arena platform by the Big Arc Slash (3.2 u tall); dash and shockwave miss it. | Intended (D-161); call out if it feels unfair. |
| 10 | Low | Levels | Validate Project doesn't check vertical reach automatically; climbs were checked by the room generator scripts (≤ 5 tiles up, ≤ 8 across). | Hand-check new rooms. |
| 11 | Low | Throne | Torches only decorate the throne room; no other castle props are placed. | Decoration pass later. |
| 12 | Low | Options | No options screen (volume, rumble, key rebinding, resolution) — not in the doc scope. Rumble can be turned off in `Assets/Resources/FeelConfig.asset`; resolution via Alt+Enter / window resize. | Post-competition. |
| 13 | Low | Resolutions | Only 16:9 sizes were designed for. Other aspect ratios show more/less width (camera keeps its height); UI scales by 1920×1080 match 0.5. | Check 16:10 / 21:9 in QA §12. |
| 14 | Low | Sandbox | Killing Zulmat in the Sandbox doesn't play the ending (intentional, D-169). | Use Phase2_Castle (Shift+F6, F6 ×2, Shift+F5) to test the ending. |

## Fixed during QA (Prompt 18)
- Enemy sword slashes were hard to see (thin blade, no trail) → every blade attack (Kage Warden, Kage Caller, Knight, Iron Colossus, Zulmat) has a sweep trail over the hit area (crimson in Phase 1 / Zulmat, blue-white in Phase 2); overhead cuts end low toward the floor; over-long blades shortened in the art (hit reach unchanged).
- Player attacks showed a white box: PlayerFxPools' real-sprite slots (slash, hit spark, soul orb, fireball, impact) were never filled → PlayerFactory wires them from Assets/Art/FX; slashes are fitted to the hitbox.
- Dark text was unreadable on the new dark stone UI art → all text is light: interact/sign prompts, ability popup, boss intro card, menu buttons, menu hint, cutscene captions/cards/skip label (UIFactory.Ink is now near-white; inner-voice and boss lines use light blue / light red).
- Dying in a boss arena locked the player out: the arena re-engaged on the body during the death slow-mo/wipe (gates shut, fresh boss spawned) before the respawn moved the player outside → BossArena never engages while the player is dead.
- Running a builder menu item (e.g. Build All Scenes) during Play mode threw `InvalidOperationException: This cannot be used during play mode` → every asset/scene builder is greyed out in Play mode (`PlayModeMenuGuards`).
- A boss destroyed during its slow-mo death (player died on the killing blow) left `Time.timeScale` at 0.25 → `TimeScaleGuard`: slow-mo owners are released on destroy, scene loads restore normal speed, a watchdog resets a stuck time scale; resuming from pause keeps a running slow-mo.
- Pooled FX and enemy projectiles still in flight kept flying into the next scene (pools live across loads) → released on every scene load.
- Bench rest finished for a player who had died during it (heal + save) → aborted.
- Alt-tab in a build kept the game running → losing focus pauses (builds only) and stops rumble.
