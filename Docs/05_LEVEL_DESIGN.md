# 05 — Level Design

Keep it **simple and readable** (the team said: don't make the cave too complex). A linear, room-by-room path with a few vertical rooms and small detours. No map, no backtracking requirement, no secret rooms.

## 1. Units
- 1 tile = **0.5 u = 50 px**. 1 screen = **40 × 22 tiles** (20 × 11 u). Room sizes are multiples of 20 tiles wide / 22 tiles tall where possible.
- Ceiling clearance in every room that has Phase 2 enemies: **≥ 8 tiles (4 u)** so ×1.4 knights (≈ 2.7 u) fit.
- Minimum corridor height: **6 tiles (3 u)** in Phase 1, **8 tiles (4 u)** in Phase 2.

## 2. Platforming limits (design every gap/ledge within these)
Derived from doc 03 (run 7.5 u/s, apex 3.2 u, air time ≈ 0.73 s).
| | Max (hard) | Safe (use this) |
|---|---|---|
| Jump height up | 3.2 u (6 tiles) | **2.5 u (5 tiles)** |
| Horizontal gap, no dash | 5.5 u (11 tiles) | **4.0 u (8 tiles)** |
| Horizontal gap, with dash (P2) | 8 u | **6 u (12 tiles)** |
| One-way platforms | stack every ≤ 5 tiles vertically for climbs |

## 3. Room manifest
`Assets/Data/Levels/LevelManifest.json` lists rooms with world placement (so openings line up).  Each room is a text file `Assets/Data/Levels/<ROOM_ID>.txt`.

### Room file format
```
# id: P1_A1
# size: 60x22            (width x height in tiles — must match the grid exactly)
# origin: 40,0           (world tile coord of the bottom-left corner)
# theme: cave            (cave | castle)
# music: none
# name: The First Crawlers
<22 rows of exactly 60 characters, TOP row first>
```
### Legend
| Char | Meaning | Char | Meaning |
|---|---|---|---|
| `#` | Solid terrain | `.` or space | Empty |
| `=` | One-way platform (jump through from below) | `x` | Spikes (hazard) |
| `P` | Player start (only in first room of a phase) | `B` | Bench (candle shrine) |
| `c` | Shade Crawler | `l` | Ninja Lunger |
| `f` | Shadow Kite | `k` | Castle Knight |
| `b` | Bat (ceiling-hang spawn; place under a `#` ceiling) | `h` | Charger |
| `t` | Thrower | `M` | Mini-boss spawn |
| `Z` | Final boss spawn | `D` | Door (locked until flag `door_<id>`) — 2 tiles wide × 6 tall; mark its bottom-left |
| `O` | Orb pedestal | `G` | Boss gate (closes on boss engage; 2×6 tiles, place at arena entrance and exit) |
| `L` | Lava (static, P2) | `T` | Tutorial sign (shows text from manifest) |
| `1`–`9` | Spawn point for minion-drop slots (boss arenas) | `S` | Safe-ground respawn point (after spikes/pits) |
Tiles are auto-styled: the builder chooses one of 16 tiles by a 4-neighbour bitmask (N=1, E=2, S=4, W=8 → `Tile_<Theme>_00..15`, doc 06). Room edges that are open (a corridor leading out) must be **empty** on the shared boundary and line up with the neighbouring room's opening (same Y rows).

### Room rules
1. Boundaries: left/right/top/bottom outer ring is `#` **except** openings to neighbouring rooms.
2. Rooms are placed in world space adjacent to each other; the camera clamps to the current room and the `RoomManager` switches when the player crosses a boundary trigger.
3. Every opening must be **≥ 6 tiles tall** (P1) / **8 tiles** (P2).
4. Enemies placed on ground tiles (`c l k h t`) must stand on solid or `=`. Flyers (`f`) and bats (`b`) in open air / under ceiling.
5. No enemy within 6 tiles of a room entrance (no ambush on entry).
6. Benches are in a calm nook, never in a boss arena.
7. Each section's total enemy count must match the tables below — the **validator** (`Shatranj/Validate Project`) counts glyphs per section and fails on mismatch.

## 4. PHASE 1 — Cave (scene `Phase1_Cave`, theme `cave`)
Cave system, rooms for 15 enemies before MB1, then 20 enemies before MB2, then orb. Mostly horizontal; vertical shafts A3 and B3 only.

### Section A — 15 enemies (Crawlers + Lungers)
| Room ID | Size (tiles) | Enemies | Notes |
|---|---|---|---|
| `P1_A0` Cave Mouth | 40×22 | 0 | `P` start, 3 tutorial signs (move/jump/attack), village-glow backdrop visible left, `B` bench |
| `P1_A1` | 60×22 | 3 (`c c c`) | Flat with two low platforms; teaches attack/soul |
| `P1_A2` | 40×22 | 3 (`c c l`) | Small spike pit (8 tiles) with `S`; first lunger on a ledge |
| `P1_A3` | 40×44 | 4 (`c c l l`) | **Vertical shaft**, ledges every 5 tiles, climbing up then exit to the right |
| `P1_A4` | 80×22 | 3 (`c l l`) | Two-tier cave; upper route optional |
| `P1_A5` | 60×22 | 2 (`l l`) | Pre-boss gauntlet; `B` bench at east end |
| `P1_MB1` | 40×22 | boss `M` | Flat arena, `G` gates at entry (west), exit (east) |
**Total A = 15 ✔**

### Section B — 20 enemies (Crawlers + Lungers + **Shadow Kites**)
| Room ID | Size | Enemies | Notes |
|---|---|---|---|
| `P1_B1` | 60×22 | 3 (`c l f`) | First kite, high ceiling |
| `P1_B2` | 40×22 | 4 (`f f c l`) | Stalactite ceiling, narrow floor |
| `P1_B3` | 40×44 | 3 (`f c l`) | **Vertical**, platforms; kites circle the shaft |
| `P1_B4` | 80×22 | 4 (`c c l f`) | Long hall, spikes (`x`) between 2 platforms |
| `P1_B5` | 60×22 | 3 (`l f f`) | Pillars (`#` columns) to hide behind |
| `P1_B6` | 60×22 | 3 (`c l f`) | Final stretch, `B` bench east end |
| `P1_MB2` | 50×22 | boss `M` | Ceiling drop slots `1 2 3`, `G` entry gate, `D` **door** at east end (flag `door_orb`) |
| `P1_ORB` Orb Room | 30×22 | 0 | `O` pedestal, shrine-like, no enemies; touching starts Cutscene 2 |
**Total B = 20 ✔**   **Phase 1 regular enemies = 35 ✔**

## 5. PHASE 2 — Castle (scene `Phase2_Castle`, theme `castle`)
Castlevania-style: stone halls, tall towers, stairs-like platform stacks. **A bit of upward progression**: rooms C2, C4, D2, D4 are tall; overall the route climbs to a throne room at the top.

### Section C — 20 enemies (Knights + Bats)
| Room ID | Size | Enemies | Notes |
|---|---|---|---|
| `P2_C0` Gate Hall | 40×22 | 0 | `P` start (arrives from Cutscene 2), `B` bench, shows "Phase II" card |
| `P2_C1` | 60×22 | 3 (`k b b`) | Hall with hanging chandelier platforms |
| `P2_C2` | 40×66 | 4 (`k k b b`) | **Tower climb**, 3 screens tall, one-way platforms |
| `P2_C3` | 60×22 | 3 (`k k b`) | Arches, low ceiling bats |
| `P2_C4` | 40×44 | 4 (`k b b b`) | Vertical, bats on ceiling beams |
| `P2_C5` | 80×22 | 3 (`k k b`) | Long gallery |
| `P2_C6` | 60×22 | 3 (`k k b`) | `B` bench east end |
| `P2_MB` | 50×22 | boss `M` | Iron Colossus arena, `G` gates |
**Total C = 20 ✔** (3+4+3+4+3+3)

### Section D — 30 enemies (adds **Chargers** + **Throwers**)
| Room ID | Size | Enemies | Notes |
|---|---|---|---|
| `P2_D1` | 60×22 | 4 (`k h b b`) | Long flat hall to introduce the charger (wall at one end) |
| `P2_D2` | 40×44 | 4 (`t k b b`) | Vertical; thrower on high ledge |
| `P2_D3` | 80×22 | 4 (`h t k b`) | Two chargers' worth of room, pillars |
| `P2_D4` | 40×66 | 4 (`t t k b`) | **Tall tower**, throwers across the shaft |
| `P2_D5` | 60×22 | 4 (`h h k t`) | Gauntlet |
| `P2_D6` | 60×22 | 4 (`h t b b`) | Lava pit (static `L`) between platforms |
| `P2_D7` | 80×22 | 3 (`h t k`) | Long corridor |
| `P2_D8` | 60×22 | 3 (`t k h`) | `B` bench east end |
| `P2_THRONE` | 60×22 | boss `Z` | Throne at east wall; no ceiling hazards; torch props; `G` gate at entrance |
**Total D = 4+4+4+4+4+4+3+3 = 30 ✔**   **Phase 2 regular enemies = 50 ✔**

## 6. Flow diagrams (for Claude's reference)
Phase 1: `A0 → A1 → A2 → A3(↑) → A4 → A5 → [B]MB1 → B1 → B2 → B3(↑) → B4 → B5 → B6 → MB2 → (door) → ORB`
Phase 2: `C0 → C1 → C2(↑↑) → C3 → C4(↑) → C5 → C6 → MB → D1 → D2(↑) → D3 → D4(↑↑) → D5 → D6 → D7 → D8 → THRONE`
World layout: place Phase rooms left→right with vertical rooms stepping the next rooms up/down so openings line up; `origin` fields in the manifest decide this. Claude should lay it out and print an ASCII overview map in `Docs/LEVEL_OVERVIEW.md`.

## 7. Tutorial & signposting
- `P1_A0` has three `T` signs: "A / D — move", "Space — jump", "J — attack". Text pulled from `LevelManifest.json` so the team can edit.
- Enemies are introduced **one at a time**: crawlers (A1) → lunger (A2) → kite (B1). Phase 2: knight+bat (C1) → charger (D1) → thrower (D2).
- Each boss arena has a bench **within 2 rooms** before it.

## 8. Builder requirements (`Shatranj/Build Levels`)
- Reads all room files + manifest, validates dimensions, places a **single Tilemap per phase** (+ `CompositeCollider2D` on `Ground`), a one-way `Tilemap` for `=`, hazards for `x`/`L`.
- Spawns an `EnemySpawner` marker per enemy glyph with a **stable ID** (`<roomId>_<glyph>_<index>`) used by the kill-save.
- Creates `RoomBounds` (trigger + bounds rect) per room, `BossArena` objects, `Door`, `Bench`, `Orb`, `TutorialSign` prefabs at glyphs.
- Counts glyphs per section and prints a **PASS/FAIL table** vs doc 01 §6.
- Auto-creates the overview doc `Docs/LEVEL_OVERVIEW.md`.
- A **graybox mode**: uses flat-colour placeholder tiles so the levels are playable before any art exists.
