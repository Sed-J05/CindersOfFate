# QA CHECKLIST — Cinders of Fate

Tick each box in a **Windows build** (`Shatranj → Build Windows Player (Development)` for the debug keys, the release build for the final pass) and once in the Editor. Note the build/commit at the top of each pass. Anything that fails goes into `Docs/KNOWN_ISSUES.md` with steps to reproduce.

Debug keys (Editor / Development only): F1 god · F2 soul · F3 next room · F4 phase toggle · F5 kill room enemies · Shift+F5 kill boss · F6 next boss arena · Shift+F6 jump to Phase 2 · F7 hitboxes · F8 flame 0.9 · F9 reload · F10 HUD debug · F11 visuals · F12 flame scale.

Pass build: ________  commit: ________  tester: ________  date: ________

---

## 1. Rooms — every room traversable (no debug keys)
Walk every room entrance → exit, both directions where the route allows. Check: no falling through floors, no stuck spots on tile seams, every jump reachable without dash in Phase 1 and with normal jumps in Phase 2, camera clamps to the room, the paper sweep plays on room change.

**Phase 1 — Phase1_Cave**
- [ ] P1_A0 Cave Mouth (start, 3 tutorial signs readable, bench)
- [ ] P1_A1 · [ ] P1_A2 (spike pit: falling in → 1 heart + safe-ground respawn) · [ ] P1_A3 (shaft climb) · [ ] P1_A4 · [ ] P1_A5 (bench)
- [ ] P1_MB1 Kage Warden arena (both gates, platforms)
- [ ] P1_B1 · [ ] P1_B2 · [ ] P1_B3 (shaft) · [ ] P1_B4 (spike hall) · [ ] P1_B5 · [ ] P1_B6 (bench)
- [ ] P1_MB2 Kage Caller arena (entry gate, door to the orb)
- [ ] P1_ORB (orb pedestal)

**Phase 2 — Phase2_Castle**
- [ ] P2_C0 Gate Hall (start, bench, "PHASE II" card on first arrival)
- [ ] P2_C1 · [ ] P2_C2 (tower climb, 3 screens) · [ ] P2_C3 (low arches) · [ ] P2_C4 (beam shaft) · [ ] P2_C5 · [ ] P2_C6 (bench nook)
- [ ] P2_MB Iron Colossus arena (entry + exit gates, long platform)
- [ ] P2_D1 (charger lesson walls) · [ ] P2_D2 (knife tower) · [ ] P2_D3 · [ ] P2_D4 (high tower) · [ ] P2_D5 (gauntlet) · [ ] P2_D6 (lava moat: lava = 1 heart + bounce, no teleport) · [ ] P2_D7 · [ ] P2_D8 (bench)
- [ ] P2_THRONE (gate, torches, throne)
- [ ] Falling below the world anywhere → back to safe ground (out-of-world safety net), no soft-lock

## 2. Every enemy type
For each: idle/patrol, notices the player, **visible telegraph before every attack**, attack connects fairly, hit flash + spark + knockback on each nail hit, dies in the documented hits (Balance Report), death smoke, stays dead after leaving and re-entering the room, does not leave its home room, ignores the player in other rooms.
- [ ] Shade Crawler (2 hits; tracks only on the same level)
- [ ] Ninja Lunger (3 hits; lunge telegraph)
- [ ] Shadow Kite (2 hits; dive; never sticks to ceilings)
- [ ] Castle Knight (5 hits; 2-hit combo, retreats, feints, waits when you're high in the air)
- [ ] Bat (3 hits; hangs, wakes, circles, desynced dives, never sticks to ceilings)
- [ ] Siege Ram / Charger (7 hits; charge → wall stun 1.2 s; stops at ledges and lava)
- [ ] Thrower (4 hits; keeps distance, hop-dodges, lead-aimed knives scale with the flame)
- [ ] Phase 2 enemies grow with the flame (F12 slider / F8) and never clip into walls
- [ ] Enemies in other rooms are frozen and don't animate (performance)

## 3. Every boss — including death and respawn mid-fight
For each boss: gate closes only once you're inside, intro card + boss bar (stage ticks for Zulmat), camera push-in, every move telegraphed, then:
- [ ] **Die mid-fight** → burn wipe → respawn at the bench → the arena is open again, the boss is gone, the bar is hidden, time runs at normal speed, the camera is back to normal zoom
- [ ] **Re-enter** → a fresh boss at full HP
- [ ] **Win** → slow-mo death, gates open, the defeat is saved (re-entering the room doesn't start the fight again; after Continue too)

- [ ] Kage Warden (20 HP): advance, slash combo turns around, back-step, block (front clink + pushback, back hits land), counter-slash
- [ ] Kage Caller (30 HP): Black Slash only from range, flyer summons when camping platforms, minions die with him, orb door opens
- [ ] Iron Colossus (45 HP): arc, block → bash, jump slam + shockwave (jump it), dash, never lands on / snags under the platform, enrage < 40 % jump→arc, 2-heart hits
- [ ] Zulmat (90 HP): 3 stages with roar (invulnerable 1.5 s), slash, vines, giant swipe (dash or jump), reposition, shrink & lunge, summon regulars (cap 4), lava zones (stage 3), flame parked + pulsing, white flash → ending
- [ ] Die during a boss's slow-mo death (e.g. contact on the last hit) → no permanent slow motion (TimeScaleGuard)

## 4. Benches
- [ ] Rest: prompt, sit animation, healing aura, full heal, flame pulse, "bench_saved"
- [ ] Death respawns at the last bench used (correct scene)
- [ ] Phase 2: bench stores the flame progress; dying restores it (enemy sizes don't jump)
- [ ] Taking damage while walking up to a bench / resting can't heal a dead player

## 5. Doors & gates
- [ ] Caller door locked until he dies; opens with animation; stays open after Continue
- [ ] Boss gates can't be jumped over (doorway roof) and never close on the player
- [ ] Colossus exit gate opens only after the win

## 6. Cutscenes + skip
For Cutscene 1 (intro), 2 (orb → inversion), 3 (ending → loop):
- [ ] Plays fully; text advances with J / Enter / Space / gamepad A; auto-advances if idle
- [ ] Hold Esc / Enter / Start 1 s → skip ring → skipped, **flags still applied** (phase 2 after cs2; cycle + 1 after cs3)
- [ ] Cycle ≥ 2: Cutscene 1 skips with a single press; "…again." line shows
- [ ] Cards ("Thanks for playing", "Cycle N") readable on black
- [ ] Protagonist puppet colours match the theme (blue/white in cs1, black/red after the inversion and in the ending)

## 7. Pause during each state
Press Esc / Start in each and check the pause menu (Resume · Restart from bench · Quit to menu), unscaled UI, and that resuming restores the right speed:
- [ ] Walking / jumping / attacking · [ ] during hit-stop · [ ] while hurt (knockback) · [ ] while healing · [ ] mid-dash
- [ ] During a boss intro · [ ] during a boss slow-mo death (resume keeps the slow-mo, then normal speed)
- [ ] During the death burn wipe · [ ] during a room transition sweep
- [ ] In a cutscene → Esc means skip, not pause · [ ] main menu (no pause)
- [ ] Quit to menu from pause → main menu works, time scale 1, Continue available

## 8. Gamepad (Xbox / DualShock via Input System)
- [ ] Move (stick + d-pad), jump A, attack X, spell B/Y, dash RT/RB, pause Start
- [ ] Menus navigable with d-pad / stick + A; pause menu too
- [ ] Rumble on hits, hurt, deaths, boss impacts; **stops when paused and on alt-tab**
- [ ] Hot-plugging the pad mid-game keeps working

## 9. Phase transition
- [ ] Touch orb → Cutscene 2 → inversion wipe → Phase2_Castle: 7 hearts, full heal, dash + fireball, grey-blue paper, blue flame, protagonist black/red, "PHASE II" card once
- [ ] Phase 2 flame starts at 0 when Phase2_Castle appears and moves left → right over ~20 min; enemies visibly larger by ~60 % of the arc

## 10. Cycle loop (twice)
- [ ] Kill Zulmat → Cutscene 3 → (Cutscene 1) → Phase1_Cave with "CYCLE 2" card; Phase 1 abilities, 5 hearts
- [ ] All enemies alive again, both Phase 1 bosses alive, orb door locked, no bench saved, flame orange
- [ ] Repeat → "CYCLE 3"; no errors in the log (Development build: `%USERPROFILE%\AppData\LocalLow\Team Cinders of Fate\Cinders of Fate\Player.log`)

## 11. Save / Continue
- [ ] New Game with an existing save → starts fresh at cycle 1
- [ ] Rest at a bench, quit to menu → Continue → same bench, same hearts max / abilities, killed enemies stay dead
- [ ] Quit during Cutscene 2 → Continue resumes Cutscene 2 → Phase 2
- [ ] Quit after reaching Phase 2 (before any bench) → Continue → Phase2_Castle start, Phase 2 abilities
- [ ] Quit during Cutscene 3 before the "Cycle" card → Continue replays the ending; after the card → Continue → Phase 1, next cycle
- [ ] Corrupt / delete the save file → no crash; Continue hidden

## 12. Resolutions & window (release build)
Check camera framing (room fills the view, no gaps at the edges), UI scale (HUD hearts / soul / boss bar / captions readable, nothing cropped), parchment covering the screen:
- [ ] 1920 × 1080 windowed (default) and fullscreen (Alt+Enter)
- [ ] 1366 × 768
- [ ] 2560 × 1440
- [ ] **Resize the window** by dragging (narrow, wide, tall): parchment re-fits every frame, UI rescales, no errors
- [ ] **Alt-tab** away mid-fight → game pauses, rumble stops; alt-tab back → pause menu open, Resume works
- [ ] Minimise / restore → same as alt-tab

## 13. Performance (Development build, Profiler attached)
- [ ] 60 fps in the busiest rooms (P2_D5 gauntlet, P2_D6 lava, Zulmat stage 3 with lava + minions)
- [ ] No GC spikes per frame while walking / fighting (pooled FX and projectiles)
- [ ] Loading a phase scene < 3 s

## 14. Build sanity
- [ ] Window title / exe "Cinders of Fate", version 1.0.0; title logo and credits say Cinders of Fate
- [ ] Release build: debug keys do nothing, Sandbox not reachable
- [ ] No errors in Player.log after a full Phase 1 → Phase 2 → ending run
