# 04 — Enemies & Bosses

**Global rules**
- Every attack: **Telegraph → Active → Recovery** (durations in config). Telegraph = pose change + 0.1 s white/red flash + (for big ones) `!` marker.
- Contact damage 1 unless stated. Enemies flash white 0.08 s on hit; knockback small (regular) or none (bosses).
- Enemies only update AI when their room (or an adjacent room) is active.
- Phase 1 enemies: **red/black**, fixed size. Phase 2 enemies: **white/blue**, **scaled by the flame** (doc 07 §4), better AI.
- Soul is granted to the **player** on hit (doc 03), enemies drop nothing.
- Sizes below are *design sizes* (world units, at scale 1.0). Canvas = sprite file size (doc 06). Colliders are **slightly smaller** than visible body.
- All stats go in `EnemyConfig` / `BossConfig` SOs.

## A. Phase 1 — Cave (red/black, cube-ish "small black ninjas")

### A1. Shade Crawler (`c`)
| | |
|---|---|
| Look | Small black cube-ish ninja with red eyes/scarf, scuttles low |
| Body / collider | 0.8 × 0.7 u / BoxCollider2D 0.7 × 0.6 · **Canvas 128×128** |
| HP | **2** |
| Contact dmg | 1 |
| AI | Patrol 2.5 u/s; turns at walls/ledges. If player within **6 u** on same platform (±1.5 u vertical) → chase at **3.5 u/s** (still turns at ledges). No ranged. |
| Knockback | 3 u/s for 0.15 s |

### A2. Ninja Lunger (`l`)
| | |
|---|---|
| Look | Slightly taller ninja, crouches before pouncing |
| Body / collider | 0.9 × 1.0 u / 0.8 × 0.9 · **Canvas 160×160** |
| HP | **3** |
| AI | Patrol 2 u/s. Player within **7 u** with line-of-sight → **Telegraph 0.5 s** (crouch, `!` above) → **Lunge 14 u/s for 0.35 s** (≈ 5 u, horizontal toward where player *was*) → **Recovery 0.8 s** (vulnerable, no turn) → 1 s cooldown |
| Contact dmg | 1 (during lunge and body) |

### A3. Shadow Kite (`f`) — flyer, appears **after MB1**
| | |
|---|---|
| Look | Paper-cut kite/bird shape with tail, red/black |
| Body / collider | 1.0 × 0.8 u / CircleCollider 0.4 r · **Canvas 128×128** |
| HP | **2** |
| AI | Hovers with sine bob (amp 0.4 u, 1.5 Hz) near its spawn; if player within **9 u** → drifts toward player at 2.5 u/s keeping ~3 u above. Every **3 s**: **Telegraph 0.5 s** (flash, tail flares) → **Dive 9 u/s along the line to the player's position at telegraph start**, until hit ground / 6 u travelled → return to hover height at 3 u/s. |
| Knockback | 4 u/s |

## B. Phase 1 Mini-bosses (fixed size, don't scale)

### B1. MB1 — **Kage Warden** (the gate guard)
| | |
|---|---|
| Look | Tall black swordsman, red sash, big straight blade |
| Body / collider | 1.6 × 2.4 u / 1.2 × 2.2 · **Canvas 384×384** |
| HP | **20** (hit-flash only, no stagger) |
| Arena | 1 screen (40×22 tiles). Entry gate closes on engage; boss bar appears |
| Contact dmg | 1 · Slash dmg 1 |
| Behaviour (loop, pick weighted-random with no immediate repeat) | **1. Advance** (walk toward player 3 u/s for 0.8–1.4 s) · **2. Slash combo**: Telegraph 0.45 s → slash 1 (reach 2.2 u, active 0.1 s) → 0.25 s gap → slash 2 in the *opposite* direction if the player is behind it, else same direction → Recovery 0.6 s · **3. Back-step** (hop back 3 u, 0.35 s) then Advance · **4. Block**: raises blade for 1.5 s; **attacks against the front deal 0 dmg and show a "clink" spark; the player gets 2 u/s pushback.** Hits from behind still damage. After 1.5 s it ends with a **counter-slash** (telegraph 0.3 s) if player is within 2.5 u. |
| Rule | Never chain Block twice. Block only when player attacked it ≥ 2 times in the last 2 s or randomly 20 %. |
| On death | 1.2 s slow-mo death, gate opens |

### B2. MB2 — **Kage Caller** (guards the orb door)
| | |
|---|---|
| Look | Robed black swordsman with a hood and trailing smoke, red crescent aura |
| Body / collider | 1.5 × 2.3 u / 1.1 × 2.1 · **Canvas 384×384** |
| HP | **30** |
| Arena | 50 × 22 tiles (25 × 11 u), ceiling has 3 "drop slots"; **door** at the east end (locked until boss dies) |
| Moves | **Regular slash** (like MB1 but faster: telegraph 0.35 s, 2-hit combo) · **Black Slash** (ranged): Telegraph 0.5 s (raises blade, red glow) → fires crescent **1.4 × 0.6 u** (canvas 192×96) at **11 u/s** along the ground (jump to avoid), dmg 1, lifetime 2 s, destroyed on wall. Fires **1, then 2 with 0.5 s spacing** below 66 % HP · **Summon**: Telegraph 0.8 s (raises arm, `!` markers appear **at the X positions where minions will fall** for 0.8 s) → 2 minions drop from ceiling (Crawlers/Lungers, 50 % each, normal stats). **Cap: 3 minions alive.** Cooldown 12 s (8 s below 50 %) · Movement: advance/back-step like MB1, 3.5 u/s. |
| Phases | 100–50 %: as above. **<50 %:** move speed ×1.25, all telegraphs −0.1 s, summons 3 minions |
| On death | Minions die with it; door unlocks (door anim opens), 1.2 s slow-mo |

## C. Phase 2 — Castle (white/blue, **scaled by flame**, better AI)
**"Better AI" means:** enemies keep **spacing**, **retreat after attacking**, **avoid ledges**, **react to the player's jump** (a knight won't swing at a player who is mid-air above 1.5 u; it waits), **don't stack on each other** (separation steering), and **use cooldown-based attack choice with 20 % feints**. They notice the player at larger ranges and **lose track after 3 s** without line-of-sight.

### C1. Castle Knight (`k`)
| | |
|---|---|
| Look | White-blue armoured knight, plumed helmet, longsword |
| Base size / collider | 1.1 × 1.9 u / 0.8 × 1.7 · **Canvas 256×256** |
| HP | **5** · Contact dmg 1 · Sword dmg 1 |
| AI | Patrol 2 u/s. See player within **9 u** → approach to **~2.0 u**, then Telegraph 0.4 s → **2-hit combo** (reach 1.6 u, hit 1 at 0.1 s, hit 2 after 0.3 s gap) → **retreats 2.5 u** (0.5 s) → repeats. 20 % feint: telegraph, cancel, back off. Won't swing if player airborne above 1.5 u. |
| Scaling | **Yes** (doc 07 §4) |

### C2. Bat (`b`) — swarm flyer
| | |
|---|---|
| Look | White-blue cartoon bat, pointed ears |
| Base size / collider | 0.9 × 0.6 u / CircleCollider 0.3 r · **Canvas 128×128** |
| HP | **3** · Contact dmg 1 |
| AI | Spawns **hanging** from ceiling (Sleep anim) until player within **8 u** → Wake (0.4 s) → erratic flight (Perlin wander, 3.5 u/s) circling the player at 3–4 u radius → every **2.5 s** dive-bite (Telegraph 0.35 s, 8 u/s) → flutters away. Bats in same room **desync** attack timers (+0.6 s each) so they don't dive together. |
| Scaling | Yes (scale pivot = centre) |

### C3. Charger / Siege Ram Knight (`h`) — appears **after MB3**
| | |
|---|---|
| Look | Bulky armoured knight with a horned helm and shoulder-ram |
| Base size / collider | 1.8 × 1.5 u / 1.6 × 1.3 · **Canvas 320×256** |
| HP | **7** · Charge dmg 1 (2 if scaled ≥ ×1.3?) → **keep 1** |
| AI | Patrol 1.5 u/s. Player within **10 u** in line, same level → **Telegraph 0.6 s** (paws ground, dust, flash) → **Charge 13 u/s** up to 10 u or until wall → on wall hit **stunned 1.2 s** (big opening, takes normal damage) → turn and retreat/repeat. Can't be knocked back during charge. If player jumps over it, it overshoots and stuns on the wall as well. |
| Scaling | Yes |

### C4. Thrower (`t`) — appears **after MB3**
| | |
|---|---|
| Look | Lean white-blue hooded figure with a bundle of knives |
| Base size / collider | 1.0 × 1.8 u / 0.7 × 1.6 · **Canvas 256×256** |
| HP | **4** |
| AI | Keeps **6–9 u** from the player: retreats if closer than 5 u, advances if farther than 10 u (stops at ledges/walls; if cornered it does a short hop-dodge). Every **2.2 s**: Telegraph 0.4 s → throws a **knife** (canvas 128×32, 0.9 × 0.2 u) in a slight arc, speed **10 u/s**, with **first-order lead** (aims at player pos + vel × 0.3 s), dmg 1, destroyed on wall. |
| Scaling | Yes (projectile size scales ×(1 + 0.5·(s−1))) |

## D. Phase 2 Mini-boss — **Iron Colossus** (MB3)
| | |
|---|---|
| Look | Slow, tanky, **stocky** armoured brute in white/blue steel, huge sword and a tower shield |
| Body / collider | 2.2 × 2.6 u / 1.8 × 2.4 · **Canvas 512×512** (fixed size, no scaling) |
| HP | **45** · **All its hits deal 2 hearts** (contact deals 1) |
| Arena | 50 × 22 tiles, 1 gate |
| Move speed | **1.8 u/s** (slow). Poise: never staggers. |
| Moves | **Big Arc Slash**: Telegraph 0.7 s → wide arc, reach 3.2 u in front, active 0.15 s, Recovery 0.9 s (dmg 2) · **Big Shield Block**: raises tower shield for 2 s (big arc in front, blocks frontal attacks, shield FX arc), hits from behind do damage; ends with **shield bash** (reach 2 u, dmg 2, telegraph 0.3 s) · **Jump Slam**: Telegraph 0.6 s (crouch) → leaps toward the player's X (apex 3.5 u, 0.7 s) → lands with **shockwave** radius 2 u on the ground (dmg 2) · **Dash**: Telegraph 0.5 s → charges **12 u/s for 0.4 s** (≈ 5 u), dmg 2, recovery 0.8 s. |
| Selection | Distance-based: far → Dash or Jump; mid → Jump or Arc; close → Arc, Block, Bash. No move repeated 3× in a row. |
| Enrage | <40 % HP: telegraphs −0.15 s, adds a jump → arc combo |
| On death | 1.5 s slow-mo; gate opens; flame resumes |

## E. Final Boss — **Zulmat, the Severed** (Aku-like)
| | |
|---|---|
| Look | Tall, imposing figure, flame-like swept-back hair / horns, long jaw, flowing robe. **Palette: red/black with bright crimson rim light** (inverse of Phase 2's white/blue enemies). |
| Sprite set | **One set at 512×512** (design size 2.0 × 3.2 u; collider 1.3 × 3.0). Shrink = scale ×0.5; Giant = scale ×1.8 (tweened); transforms use dedicated 4-frame Shrink/Grow anims |
| HP | **90**, 3 stages: **100–67 %, 67–34 %, 34–0 %** (stage change plays roar anim + screenshake; 1.5 s invulnerable, summons nothing) |
| Arena | **Throne room**: 60 × 22 tiles (30 × 11 u). Flame is **parked** above (no scaling during this fight) |
| Contact dmg | 1 |
| Moves | See below |

**Move list**
| Move | Details | Stage |
|---|---|---|
| **Sword Slash** | Telegraph 0.4 s → 2-hit combo reach 2.8 u, dmg 1 | 1+ |
| **Vine Summon** | Telegraph 0.8 s: ground glow (`Vine_Warn`, 192×64) at the player's X, then vines (128×384, 1.0 × 3.0 u) erupt **sequentially**: 3 vines spaced 3 u apart marching toward the far wall, 0.35 s between. Vine stays 0.7 s. dmg 1. | 1+ |
| **Giant Swipe** | Zulmat grows to ×1.8 (0.5 s, rooted, can't be damaged? **he CAN be damaged**), Telegraph 0.8 s, horizontal sweep covering the full floor width in front (reach 6 u, height 0–2.5 u) → **dodge with Dash through it or jump above 2.6 u**. FX sprite `Giant_Swipe` 768×512. Then shrinks back. dmg 1 | 1+ |
| **Shrink & Lunge** | Shrinks to ×0.5 (0.4 s), vanishes in smoke, re-appears at arena edge at player height, Telegraph 0.35 s, **lunges 16 u/s** across the room (hit-box follows), twice in a row (the second from the opposite side), ends in a small slash. dmg 1 | 2+ |
| **Summon Regulars (while small)** | Shrunk, flies up to ceiling-centre, summons **2 enemies** (Knight, Bat) on both sides; cap **4 alive**; he is **vulnerable** while small and summoning (reward for fighting him). Cooldown 20 s | 2+ |
| **Lava Zones** | Telegraph 1.0 s (cracks glowing on floor, 4–6 u wide) → **lava** (`Lava_Zone` 100×50 tile, 4-frame loop) rises for **6 s**: standing in it = 1 dmg per touch (i-frames 1.2 s); max 3 zones. Aim: split the arena, forcing platform/jump play. | 3 |
| **Combos (Stage 3)** | Slash → Vine; Lunge → Giant Swipe; all telegraphs −0.15 s, move cooldowns −20 % | 3 |
Selection: weighted random with **no immediate repeat**, **max one summon active at a time**, a **"fairness gap"** of ≥ 0.6 s between the end of one attack and the start of the next.
On death: 2.5 s slow-mo + white flash → triggers Cutscene 3.

## F. Hazards (non-enemy)
| | |
|---|---|
| Spikes (P1) | tile 50×50, 1 dmg + respawn at last safe ground |
| Lava (P2 final boss only, plus 2 static lava pits in late castle) | 100×50 anim tile, 1 dmg, no teleport |

## G. Balance targets (use for tuning pass)
| Fight | Target duration (skilled) | Target damage taken |
|---|---|---|
| Regular enemy | 2–6 s | 0 |
| MB1 | 45–70 s | 1–2 hearts |
| MB2 | 70–110 s | 2–3 hearts |
| MB3 | 90–140 s | 3–4 hearts |
| Final boss | 150–240 s | 4–6 hearts (heals allowed) |
Section pacing: Phase 1 ≈ 18–25 min, Phase 2 ≈ 20–30 min (flame total arc = 20 min; see doc 07).
