# 03 — Protagonist Spec ("the Shade-Child" — working name)

Blue/white shadow creature. Small blade-like claw/nail made of white light. Hollow-Knight movement feel, no pogo.

## 1. Dimensions
| Item | Value |
|---|---|
| Sprite canvas | **256 × 256 px**, pivot **bottom-centre (0.5, 0)**, PPU 100 (= 2.56 u) |
| Visible body | ≈ 110 × 170 px (≈ 1.1 × 1.7 u) centred in canvas, feet on the canvas bottom edge (leave ~10 px margin) |
| Body collider | `CapsuleCollider2D` size **0.7 × 1.5 u**, offset (0, 0.75), zero-friction material |
| Hurtbox | Same as body collider (trigger on `PlayerHurtbox` layer) |
| Facing | Sprite drawn facing **right**; flip via `localScale.x` |

## 2. Controls
| Action | Keyboard | Gamepad |
|---|---|---|
| Move | A / D (or ←/→) | Left stick / D-pad |
| Jump (variable height) | **Space** | A / South |
| Look up / Up-attack modifier | **W** (↑) | Stick up |
| Look down | **S** (↓) | Stick down |
| Attack | **J** | X / West |
| Spell: **tap** = Fireball, **hold** = Heal | **K** | B / East (Y optional) |
| Dash (Phase 2) | **L** | RT / R1 |
| Pause | Esc | Start |

Tap vs hold: pressing K starts a **0.18 s** window; released before it ends → fireball (if unlocked & ≥33 soul); still held → enters Heal. In Phase 1 (fireball locked) K just begins healing immediately.

## 3. Movement numbers (defaults → `PlayerConfig` SO)
| Parameter | Value |
|---|---|
| Run speed | **7.5 u/s** (accel 0.05 s, decel 0.04 s; air control 90 %) |
| Jump apex height (full hold) | **3.2 u** (apex time 0.40 s → jump velocity ≈ 16 u/s, rise gravity ≈ 40 u/s²) |
| Variable jump | On release while rising: `vy *= 0.4` (min height ≈ 1.2 u) |
| Fall gravity multiplier | **1.5×** (≈ 60 u/s²) |
| Max fall speed | **22 u/s** |
| Coyote time | **0.10 s** |
| Jump buffer | **0.12 s** |
| Air jumps | **0** |
| Land | 0.08 s landing squash anim (no movement lock) |

## 4. Combat (Hollow-Knight style, **no pogo**)
| Parameter | Value |
|---|---|
| Attack damage | **1** (flat) |
| Attack cooldown | **0.40 s** (can't queue more than one buffered press, 0.15 s buffer) |
| Side attack hitbox | box **1.6 w × 1.2 h**, offset (1.1, 0.95), active **0.08 s** starting 0.06 s after press |
| Up attack (W held) | box **1.4 w × 1.6 h**, offset (0, 2.0) |
| Down attack | **does not exist** (S+J = side attack) |
| Alternating swings | Attack_Side_A / Attack_Side_B alternate |
| Self-recoil on hit | Horizontal push of **2 u/s for 0.08 s** away from enemy (grounded only) |
| Hit-stop | 0.05 s on every connecting hit; 0.10 s on boss hit |
| Slash FX | `Slash_Side` (320×256) / `Slash_Up` (256×320) spawned at hitbox, 3 frames, 0.12 s |
| Cannot attack while | healing, dashing, hurt-stunned, in cutscene |

## 5. Soul ("shadow power")
- Max **99**. **+11 per connecting attack** (regular enemies & bosses). Fireball hits **don't** grant soul.
- On hit, a **shadow-orb FX** (48×48, dark with blue rim) flies from enemy to player chest (0.25 s, eased) then adds the soul (so soul rises when orb arrives).
- **Heal:** cost **33**, hold K **0.9 s** (rooted, 6-frame charge loop with aura), restores **1 heart**. Cancelled if the player takes damage or releases K early (soul not consumed unless the heal completes). Can chain while soul ≥ 33.
- **Fireball (Phase 2):** cost **33**; 0.25 s cast (rooted on ground, free in air); projectile 1.2 × 0.8 u (canvas 192×128), speed **14 u/s**, lifetime 1.2 s, **pierces**, damage **3**, spawns at (0.8, 1.0) from feet in facing direction. Impact FX at wall.

## 6. Dash — "Time Slip" (Phase 2 unlock)
| Parameter | Value |
|---|---|
| Speed / duration | **20 u/s for 0.18 s** (≈ 3.6 u), horizontal in facing direction |
| Cooldown | **0.5 s** after dash ends |
| I-frames | Full dash duration (0.18 s) |
| Gravity | **0 during dash**, velocity restored after |
| Air dash | **One** per airtime (reset on landing) |
| Visual | 3–4 blue-white **afterimage ghosts** (sprite copies fading in 0.3 s) + tiny time-warp ripple (optional) |

## 7. Health
| Item | Value |
|---|---|
| Hearts | **5** (Phase 1) → **7** (Phase 2). 1 damage = 1 heart (Colossus & some Phase 2 hits = 2) |
| Hurt | Knockback **6 u/s horizontally + 5 u/s up** for 0.2 s, stun 0.25 s, **i-frames 1.2 s** (sprite blinks at 10 Hz) |
| Hit-stop on being hit | 0.12 s + screenshake (0.3 amp, 0.2 s) |
| Hazards | Spikes: 1 dmg + respawn at last safe-ground position (stored every time player stands grounded for 0.5 s outside hazards). Lava (P2): 1 dmg per touch with i-frames, no teleport |
| Death | 0.6 s slow-mo death anim → fade → respawn at last bench with full hearts & soul = 0 |
| Full heal | At benches and on phase change |

## 8. State machine (enum)
`Idle, Run, JumpRise, Fall, Land, AttackSide, AttackUp, Cast, HealCharge, Dash, Hurt, Dead, Cutscene, Resting`
Transitions: standard; Hurt interrupts everything except Dash/Dead/Cutscene; Dash interrupts everything except Hurt-from-hazard? (no — i-frames make Dash immune); Cutscene overrides all (input disabled, can be script-driven: `PlayerController.PlayCutsceneMove(target, speed)`).

## 9. Abilities flags
`PlayerAbilities { bool canDash, canFireball, int maxHearts }`. Phase 1 = `{false,false,5}`; Phase 2 = `{true,true,7}`. Set by `GameManager` on phase change, persisted in save.

## 10. Bench ("Candle Shrine")
Interact: stand in trigger and press **W** (or Interact). Plays 0.8 s rest animation, full hearts, saves checkpoint (room, position, flame progress, flags). **Does not respawn enemies.**

## 11. Feel checklist (acceptance)
- [ ] Jump feels floaty-free and snappy; buffer + coyote work.
- [ ] Attack hits feel punchy (hit-stop, flash, shake, small recoil, soul orb).
- [ ] Can't get stuck on tile seams (use composite collider on tilemaps or edge radius).
- [ ] No sliding on slopes (no slopes in game).
- [ ] Running into walls while holding jump doesn't stick.
