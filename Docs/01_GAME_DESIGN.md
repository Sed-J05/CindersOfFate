# 01 — Game Design: SHATRANJ

## 1. Pitch
A 2D shadow-puppet action platformer. Everything you see is a puppet casting a shadow onto a parchment screen, lit by a flame burning behind it. You are a blue-white creature who descends into a cave to find the source of the monsters terrorising your village — and uncovers that the cycle you're trapped in is of your own making.

**Pillars**
1. **Shadow puppetry** — the world is a paper screen lit by a living flame. The flame is a *gameplay actor*, not just a skybox.
2. **Hollow-Knight-feel combat** — tight movement, nail-style melee, soul-gathering, focus-healing, fireball.
3. **Short, authored, escalating** — two phases, three bosses, ~85 regular enemies, one big twist.

**Themes:** shadow · fire · time.
**Title note:** *Shatranj* = chess. Optional subtle motif: the two halves (black/white, good/cruel) are two sides of one board; the ending "resets the board". Use for UI flavour only — no chess mechanics.

## 2. Platform & scope
- PC (Windows), keyboard + optional gamepad. 1920×1080 target, 16:9, 60 fps.
- Single player, ~45–75 min first playthrough. No online, no shop, no map, no inventory.
- Sound/music: **skipped for now**, but code must expose `AudioManager.Play(id)` hooks (no-op safe if clip missing) so audio can be dropped in later.

## 3. Core loop
Explore room → fight enemies (hits fill Soul) → heal with Soul / use fireball → rest at benches → beat mini-boss → progress → boss → cutscene.

## 4. Story (spoiler-level, for cutscenes — details in doc 08)
- **Beginning:** A blue-white creature lives in a village. Monsters terrorise it. It sets out for the cave at the mountain's foot, the source.
- **Phase 1 (Cave):** ~35 regular enemies, 2 mini-bosses. At the end, behind a door guarded by mini-boss #2, is an **orb**. Touching it returns **memories** (the protagonist had amnesia): the source of the monsters is a **castle/lair** belonging to an evil lord who looks a bit like *Aku from Samurai Jack*. As the memories flood in, the lord **feels it** and **wakes up**.
- **Phase 2 (Castle):** The flame behind the screen turns **blue**, moves **faster**, enemies are **inverted**, tougher, and **grow larger as the flame approaches**. Protagonist keeps his size and gains **dash** and **fireball** (time-related powers returning from his past). ~50 regular enemies, 1 mini-boss, then the lord's throne room.
- **Ending:** Defeat the lord → the flame turns **purple** → revelation: the lord is *you*. You were a god who stripped out your compassion (that became the protagonist) and your cruelty became the lord. As the halves merge, the other gods stab the merged being: they feared the combined power and **split you again**, sealing the memories. Screen pans back to the cave village. **The cycle repeats.** (Game shows "Cycle 2" and may loop; see doc 02 §9.)

## 5. Phases at a glance
| | **Phase 1 — Cave** | **Phase 2 — Castle** |
|---|---|---|
| Environment | Simple cave, horizontal, a little vertical | Castlevania-style, **more upward** |
| Parchment | Beige | **Inverted** (deep navy/black) |
| Flame | Orange, slow drift | **Blue**, fast arc bottom-left → bottom-right |
| Protagonist | blue/white | blue/white (extra glow for contrast), **+dash +fireball** |
| Hearts | **5** | **7** |
| Enemies | red/black, **no size scaling**, mild HP | white/blue, more HP, **better AI**, **scale up with flame** |
| Regular enemy count | 15 → MB1 → 20 → MB2 | 20 → MB → 30 → Final Boss |
| Regular enemy types | Crawler, Lunger (+ Shadow Kite flyers after MB1) | Knight, Bat (+ Charger, Thrower after MB) |
| Bosses | MB1 *Kage Warden*, MB2 *Kage Caller* | MB *Iron Colossus*, Final *Zulmat* |

## 6. Progression counts (authoritative — validator must enforce)
- Phase 1: 15 regular → **MB1** → 20 regular → **MB2** (guards door) → **Orb room**. (35 regular total)
- Phase 2: 20 regular → **MB3** → 30 regular → **Throne room / Final Boss**. (50 regular total)
- Benches: one at the end of each regular section (before every boss arena) + one at start of each phase.

## 7. Protagonist summary (full numbers in doc 03)
HK-style movement; **WASD + J**. After Phase 1 unlocks **Dash** (time-slip, i-frames) and **Fireball** (spell, costs Soul). Hits on enemies gather **Soul** (dark shadow-orb flies into you instead of HK's white). Hold to **Heal** (HK focus). **No pogo** (no down-attack bounce). Hearts instead of masks; Soul meter replaces HK's soul vessel with "shadow power".

## 8. Bosses summary (full in doc 04)
- **MB1 Kage Warden:** simple back-and-forth slashes + blocking.
- **MB2 Kage Caller:** ranged black slashes + melee + summons crawlers/lungers that drop from the ceiling (`!` marks warn where).
- **MB3 Iron Colossus:** slow, tanky, stocky, hits for double; jumps and dashes; big-arc slashes and big-shield blocks.
- **Final Zulmat:** Aku-like. Summons vines, shrinks and lunges, grows huge and swipes, summons regular enemies while small, creates lava zones. Always the inverse palette of the current enemies (red/black).

## 9. Cutscenes
Three: **(1) Intro**, **(2) After Phase 1 (orb memories → boss wakes → blue flame → Phase 2)**, **(3) Ending (purple flame → truth → merge → stabbed → split → loop)**. Implementation in doc 08.

## 10. Win / lose
- Die → fade → respawn at last bench, full hearts. Bosses reset. Regular enemies **stay dead** (counts are fixed, progress is guaranteed).
- "Win" = finish cutscene 3; the cycle restarts (Cycle counter +1, keep nothing except a "Cycle N" label — NG+ is out of scope).

## 11. Out of scope (do not build)
Map, shops, currency, charms, multiple endings, pogo, wall-jump, swimming, online, save slots (single auto-save only), localisation, audio (hooks only).

## 12. Assumptions I made (the team's notes were ambiguous — change anything here and tell Claude)
1. "Room for 20 minutes in castle" is a typo for **20 enemies**.
2. "Protag health +2 after phase 1 (was 5)" → **7 hearts** in Phase 2.
3. Fireball and dash unlock **at the start of Phase 2** (after cutscene 2). Healing exists from the start.
4. "Final boss always has inverted colours" → the boss is always the **inverse palette of the current regular enemies** — i.e. red/black in Phase 2 (where regular enemies are white/blue). In cutscene 2 he appears as a red-black silhouette with glowing eyes.
5. ~~In Phase 2 the protagonist stays blue/white with an outline/glow.~~ **Changed by the team (D-154): in Phase 2 the protagonist turns black/red** (reads clearly against white/blue enemies on grey-blue paper).
6. Phase 1 flame is orange, **slow and cosmetic** (enemies don't scale). Phase 2 flame is the **blue fast arc** that scales enemies.
7. Phase 2 enemy "size increase" applies to sprite **and** collider (fair hitboxes), max ×1.4, pivot at feet. Protagonist never scales. Bosses never scale with flame.
8. Phase 2 flame progress is saved at benches; **dying restores bench's flame progress** (deaths don't compound the size penalty).
9. Regular enemies keep their kills across deaths; benches do **not** respawn enemies.
10. "Dash" is themed as a **Time Slip** (afterimage ghosts + brief i-frames). Fireball is a flame projectile that pierces.
11. Controls: **A/D move, Space jump, W look-up / up-attack, S look-down, J attack, K spell (tap=fireball) / hold=heal, L dash**, Esc pause. Arrow keys + gamepad also bound.
12. Mini-boss #1 and #2 and #3 do not use flame scaling; they are fixed size.

## 12b. Global numeric constants (use everywhere)
| Constant | Value |
|---|---|
| Pixels Per Unit (PPU) | **100** |
| Camera | Orthographic, **size 5.5** (view = 11 u tall × ~19.6 u wide at 16:9) |
| Tile | **50×50 px = 0.5 u** |
| 1 "screen" | **40 × 22 tiles = 20 × 11 u** |
| Protagonist visual height | ~1.7 u (collider 0.7 × 1.5 u) |
| Fixed timestep | 0.01666 (60 Hz) |
| Sorting layers (back→front) | Parchment, BG_Far, BG_Mid, Tiles, Props, Enemies, Player, FX, Foreground, Light/Overlay, UI |
