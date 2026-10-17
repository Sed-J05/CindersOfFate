# 06 — Art & Animation Spec (for the artists AND for the placeholder generator)

The team draws the final art. Claude generates **placeholder PNGs with identical names/sizes** so the game is playable immediately; you overwrite them later.

## 1. Global rules
- Format: **PNG, RGBA, transparent background**, one file per frame.
- **PPU = 100** (100 px = 1 world unit). Canvas sizes below are **fixed per character** — every frame of every animation of that character uses the same canvas, so nothing jitters.
- **Pivot = bottom-centre (0.5, 0)** for ground characters (feet on the bottom edge, leave ≈ 8–10 px margin). **Pivot = centre (0.5, 0.5)** for flyers, projectiles, FX.
- Characters are drawn **facing right**. Unity flips them.
- Style-agnostic: hand-drawn or pixel art both work. Default filter: **Bilinear**, no compression, no mipmaps, `Mesh Type = Full Rect`. Pixel-art teams: set `ArtStyleConfig.usePointFilter = true`.
- Silhouette first! It's shadow puppetry: every character must read as a **clear solid silhouette** (limbs separated, readable pose). Use flat shapes, minimal inner detail, a bright **accent** (eyes/scarf/edge light).
- Palettes: see doc 07 §2.

## 2. Folder & file naming
```
Assets/Art/Characters/<Character>/<Anim>/<Character>_<Anim>_00.png
Assets/Art/Characters/Protagonist/Idle/Protagonist_Idle_00.png ... _05.png
Assets/Art/FX/<FxName>/<FxName>_00.png
Assets/Art/Tiles/<Theme>/Tile_<Theme>_00.png ... _15.png
Assets/Art/Backgrounds/..., Assets/Art/UI/..., Assets/Art/Cutscenes/..., Assets/Art/Props/...
```
Frame numbers are zero-padded 2 digits. Loop animations: last frame ≠ first frame (the game loops).
`Assets/Data/AnimationManifest.json` (generated from the tables below) stores: `character, anim, frames, fps, loop`. Re-run `Shatranj/Import & Rebuild Animations` after changing art.

## 3. Characters — canvas sizes, animations, frame counts, fps
Format: `Anim: frames @ fps (loop?)`.

### 3.1 Protagonist — canvas **256×256**, pivot bottom-centre. Total **65 frames**
Idle 6 @8 (loop) · Run 8 @14 (loop) · Jump_Rise 2 @10 (hold last) · Jump_Fall 2 @10 (loop) · Land 3 @16 · Attack_Side_A 4 @20 · Attack_Side_B 4 @20 · Attack_Up 4 @20 · Cast 5 @14 · Heal_Charge 6 @10 (loop) · Heal_Release 3 @12 · Hurt 2 @12 · Death 8 @10 · Dash 4 @20 *(Phase 2)* · Rest 4 @6 (loop, sitting at bench).
*Cutscene reuse:* Idle, Run (walk), Hurt, Death, Rest. Extra optional: `Look_Up` 1, `Look_Down` 1.

### 3.2 Phase 1 enemies (red/black)
| Character | Canvas | Pivot | Animations (frames @ fps) | Total |
|---|---|---|---|---|
| **Crawler** | 128×128 | bottom | Idle 4@6 loop · Walk 6@10 loop · Hurt 2@12 · Death 6@12 | **18** |
| **Lunger** | 160×160 | bottom | Idle 4@6 loop · Walk 6@10 loop · Telegraph 3@10 · Lunge 3@16 (hold last) · Recover 3@8 · Hurt 2@12 · Death 6@12 | **27** |
| **ShadowKite** | 128×128 | centre | Fly 6@12 loop · Telegraph 3@10 · Dive 2@12 loop · Hurt 2@12 · Death 6@12 | **19** |

### 3.3 Phase 1 mini-bosses
| Character | Canvas | Pivot | Animations | Total |
|---|---|---|---|---|
| **KageWarden** (MB1) | 384×384 | bottom | Intro 6@8 · Idle 6@8 loop · Walk 8@10 loop · Slash_Telegraph 3@8 · Slash_A 4@18 · Slash_B 4@18 · Block_Start 3@16 · Block_Hold 2@6 loop · Block_Impact 2@16 · Death 10@10 | **48** |
| **KageCaller** (MB2) | 384×384 | bottom | Intro 6@8 · Idle 6@8 loop · Walk 8@10 loop · Slash_Telegraph 3@8 · Slash_A 4@18 · Slash_B 4@18 · Cast_Telegraph 4@10 · Cast_Fire 4@16 · Summon_Raise 6@10 · Summon_Hold 2@6 loop · Death 10@10 | **57** |
(Boss "hurt" = code-driven white flash; no hurt frames.)

### 3.4 Phase 2 enemies (white/blue)
| Character | Canvas | Pivot | Animations | Total |
|---|---|---|---|---|
| **Knight** | 256×256 | bottom | Idle 4@6 loop · Walk 8@10 loop · Telegraph 3@10 · Attack_A 4@18 · Attack_B 4@18 · Hurt 2@12 · Death 8@12 | **33** |
| **Bat** | 128×128 | centre | Sleep 2@4 loop · Wake 3@12 · Fly 6@14 loop · Telegraph 3@10 · Dive 2@12 loop · Hurt 2@12 · Death 6@12 | **24** |
| **Charger** | 320×256 | bottom | Idle 4@6 loop · Walk 6@8 loop · Telegraph 4@8 · Charge 4@14 loop · Stunned 4@6 loop · Hurt 2@12 · Death 8@12 | **32** |
| **Thrower** | 256×256 | bottom | Idle 4@6 loop · Walk 6@10 loop · Throw_Telegraph 3@10 · Throw 3@16 · Hurt 2@12 · Death 8@12 | **26** |

### 3.5 Phase 2 mini-boss & final boss
| Character | Canvas | Pivot | Animations | Total |
|---|---|---|---|---|
| **IronColossus** (MB3) | 512×512 | bottom | Intro 6@8 · Idle 6@6 loop · Walk 8@8 loop · Slash_Telegraph 4@8 · Slash 5@16 · Block_Start 3@12 · Block_Hold 2@6 loop · Block_Impact 2@16 · Bash 3@16 · Jump_Telegraph 3@10 · Jump_Air 2@6 loop · Jump_Land 4@14 · Dash_Telegraph 3@10 · Dash 3@16 loop · Death 12@10 | **66** |
| **Zulmat** (final) | 512×512 | bottom | Intro 8@8 · Idle 8@8 loop · Walk 8@10 loop · Slash_Telegraph 3@10 · Slash_A 4@18 · Slash_B 4@18 · Vine_Cast 6@12 · Summon_Cast 6@10 · Shrink 4@14 · Grow 4@14 · Lunge_Telegraph 3@10 · Lunge 3@16 loop · Swipe_Telegraph 4@8 · Swipe 5@16 · Lava_Cast 6@10 · Roar 6@10 · Death 14@8 | **96** |
*Zulmat Shrink/Giant forms are the same sprite scaled ×0.5 / ×1.8 by code — no extra art.*

### 3.6 Sprite totals (characters only)
Protagonist 65 · Crawler 18 · Lunger 27 · Kite 19 · KageWarden 48 · KageCaller 57 · Knight 33 · Bat 24 · Charger 32 · Thrower 26 · Colossus 66 · Zulmat 96 → **≈ 511 frames** (Minimum-Viable-Art tier in §6 cuts this to ≈ 300).

## 4. FX & projectiles (pivot centre unless noted)
| Name | Canvas | Frames @ fps | Notes |
|---|---|---|---|
| Slash_Side | 320×256 | 3 @24 | white-blue crescent, pivot left-centre |
| Slash_Up | 256×320 | 3 @24 | pivot bottom-centre |
| Fireball_Fly | 192×128 | 4 @14 loop | blue-white flame (phase 2) |
| Fireball_Impact | 192×192 | 5 @16 | |
| SoulOrb | 48×48 | 4 @10 loop | dark orb, blue rim |
| Heal_Aura | 256×256 | 6 @10 loop | around protagonist |
| Hit_Spark | 128×128 | 4 @20 | enemy hit (colour-tinted by code) |
| Block_Clink | 128×128 | 3 @20 | white-gold spark |
| Dash_Ghost | — | code | afterimage sprite copies |
| BlackSlash_Fly | 192×96 | 3 @12 loop | Kage Caller projectile (red/black) |
| BlackSlash_Break | 192×96 | 4 @16 | |
| Exclamation | 64×64 | 4 @12 loop | `!` marker for drops/telegraphs, red |
| Knife_Fly | 128×32 | 4 @16 loop | Thrower projectile |
| Vine_Warn | 192×64 | 4 @10 loop | ground glow (pivot bottom) |
| Vine | 128×384 | Grow 6 @16 · Retract 3 @16 | pivot bottom |
| Lava_Zone | 100×50 | 4 @6 loop | tileable horizontally |
| Giant_Swipe | 768×512 | 5 @16 | pivot left-centre |
| Shockwave | 256×64 | 4 @16 | Colossus landing |
| Shield_Arc | 256×384 | 2 @6 loop | Colossus shield |
| Dust_Puff | 96×96 | 4 @14 | landing/charge dust |
| Death_Smoke | 192×192 | 6 @14 | enemies' death poof |

## 5. Environment
### 5.1 Tiles (50×50 px, pivot centre, `Tile_<Theme>_00..15`, themes `Cave`, `Castle`)
16 tiles indexed by 4-neighbour bitmask: **index = N·1 + E·2 + S·4 + W·8** where each term is 1 if the neighbour is solid.
Examples: `00` = isolated block · `15` = fully surrounded fill · `07` (N+E+S, no W) = left wall edge · `13` (N,S,W… etc.). Provide additionally **per theme**: `Tile_<Theme>_OneWay_L/M/R` (50×50), `Tile_<Theme>_Spikes` (50×50), `Tile_Castle_Lava` (50×50, 4 frames) and 4 decorative `Decor_<Theme>_0..3` (any size, no collider).
Tiles must be **seamless**. Top surfaces: draw a slightly lighter lip so floors read.

### 5.2 Backgrounds
| File | Size | Notes |
|---|---|---|
| `Parchment_Beige.png` | 2048×2048 | seamless paper texture, cream-beige, subtle fibres |
| `Parchment_Inverted.png` | 2048×2048 | same texture, deep navy/black with pale fibres (Phase 2) |
| `ParchmentFrame.png` | 1920×1080 | alpha overlay: darkened burnt edges / vignette (tinted per theme in code) |
| `FlameGlow.png` | 1024×1024 | soft radial gradient, white centre → transparent (tinted by code) |
| `FlameCore_00..05` | 256×256 | 6-frame flicker, blurred flame silhouette "seen through paper" |
| `BG_Cave_Far`, `BG_Cave_Mid` | 2048×1080 | horizontally tileable, cutout silhouettes (stalactites, pillars) in grey/black/muted red |
| `BG_Castle_Far`, `BG_Castle_Mid` | 2048×1080 | towers, arches, chains, inverted palette |
The parchment never moves with the world; BG layers parallax at 0.1 and 0.3.

### 5.3 Props
| Name | Canvas | Frames |
|---|---|---|
| Bench (Candle Shrine) | 192×192 | `Unlit` 1 · `Lit` 4 @8 loop · `Activate` 4 @10 |
| Door | 128×384 | `Closed` 1 · `Open` 6 @12 (pivot bottom-left) |
| Gate (boss gate) | 100×300 | `Open` 1 · `Close` 5 @14 |
| Orb | 128×128 | `Idle` 6 @8 loop · `Touch` 6 @12 |
| Tutorial sign | 128×128 | 1 |
| Torch (castle) | 96×192 | 4 @8 loop |
| Chandelier platform | 300×150 | 1 (one-way collider added in code) |

## 6. Cutscene art (see doc 08 for scripts)
- Cutscenes reuse in-game puppet sprites on the parchment stage (cheap + on-theme).
- Extra full-screen images (**1920×1080**, PNG, silhouette style): `CS1_Village_Far/Mid/Near` (2400×1080 parallax), `CS2_Memory_01..06`, `CS2_Zulmat_Eyes`, `CS2_Castle_Silhouette`, `CS3_Merge_01..03`, `CS3_GodsStab` (hooded gods ×3), `CS3_Split`.
- Hooded gods puppets: 3 × **512×768** (`God_A/B/C`, single pose).

## 7. UI sprites (sized for 1920×1080 canvas, 1:1 pixel)
| Name | Size | Notes |
|---|---|---|
| Heart_Full / Heart_Empty / Heart_Break (4 f) | 64×64 | masks→hearts (HK style) |
| Soul_Frame | 192×192 | circular vessel frame |
| Soul_Fill | 192×192 | liquid fill, drawn as `Image.Filled` (vertical) |
| Soul_Glow | 256×256 | pulses when ≥ 33 |
| BossBar_Frame / BossBar_Fill | 900×48 | bottom-centre |
| Button_Normal/Hover | 360×80 | menu |
| Title_Logo | 1200×400 | "SHATRANJ" cut-paper style |
| Cursor/Selector | 64×64 | menu marker |
| Ability_Icon_Dash / Fireball | 96×96 | "ability unlocked" popup |

## 8. Minimum Viable Art (MVA) — what to draw first
**Tier 1 (playable):** Protagonist (all except Dash/Cast) · Crawler · Lunger · Kite · tiles Cave 16 · Parchment_Beige · Flame glow · UI hearts/soul.
**Tier 2 (Phase 1 complete):** KageWarden · KageCaller · Slash FX · Bench · Door · Orb · BlackSlash · Exclamation · BG_Cave.
**Tier 3 (Phase 2):** Protagonist Dash/Cast · Fireball · Knight · Bat · Charger · Thrower · Colossus · tiles Castle · Parchment_Inverted · BG_Castle.
**Tier 4 (finale):** Zulmat (cut to Idle/Walk/Slash/Vine/Shrink/Grow/Lunge/Swipe/Lava/Death if time is short) · Vine · Lava · Giant_Swipe · Cutscene art.
Anything missing falls back to the generated placeholder, so the game never breaks.

## 9. Placeholder generator rules
- For each entry above generate PNGs at the exact canvas size, coloured: P1 enemies red/black; P2 enemies white/blue; protagonist blue/white; bosses red/black (Zulmat) etc.
- Draw a simple **silhouette shape** (rounded box for body, circle head, small eye/accent pixel block), the **animation name + frame number** as text (use a tiny built-in 5×7 pixel font written in code), and a 2 px bright outline at the canvas edge for size debugging. Show the **pivot** as a 6 px marker.
- Animate subtly between frames (shift limbs/body ±4 px per frame) so playback is visible.
- Skip files that already exist.
