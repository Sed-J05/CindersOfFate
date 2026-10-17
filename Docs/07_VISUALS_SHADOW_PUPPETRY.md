# 07 — Visuals: Shadow Puppetry, Flame, Themes

## 1. Concept
The game is a **shadow-puppet theatre**. A paper screen (parchment) fills the display. A **flame burns behind it**; everything on stage is a puppet pressed against the paper. The flame is a real actor: it moves, changes colour, lights the paper, offsets shadows, and in Phase 2 makes enemies **grow as it gets closer**.

Render stack (back → front):
1. **Parchment** (screen-space, child of camera, never scrolls) — texture + tint
2. **FlameGlow** (screen-space sprite, additive/soft-light) — radial glow at the flame's screen position
3. `BG_Far`, `BG_Mid` parallax cutouts
4. Tiles / props / enemies / player (world)
5. **Soft drop-shadows** (offset away from flame)
6. FX / foreground
7. `ParchmentFrame` vignette overlay (burnt edges), film grain / bloom (URP Volume)
8. UI (Screen Space – Overlay)

## 2. Palettes (authoritative; exposed in `ThemeConfig`)
### Phase 1 — "Beige theatre" (flame = orange)
| Role | Hex |
|---|---|
| Parchment base | `#E8D9B5` |
| Parchment burnt edge | `#9C8460` |
| Environment dark | `#26262A` |
| Environment mid (grey) | `#4A4A52` |
| Environment accent (muted red) | `#6B3A3A` — **different shading from enemy reds** |
| Enemy base (black) | `#16090A` |
| Enemy red | `#C1272D` · accent glow `#FF4B3E` |
| Protagonist white | `#EAF6FF` · blue `#4FA3FF` · deep blue `#1B4F9C` |
| Flame core / glow | `#FFD27A` / `#FF8A2B` |
### Phase 2 — "Inverted theatre" (flame = blue) — *literal inversion of Phase 1*
| Role | Hex |
|---|---|
| Parchment base | `#7D8A9C` grey-blue (beige paper texture × tint `#8AA2DB`) — changed from navy `#0E1424` by the team, D-154 |
| Parchment burnt edge | `#3A4350` |
| Environment light (inverted dark) | `#D8DEE9` |
| Environment mid | `#A7B0C4` |
| Environment accent (inverted muted red) | `#5E86A8` |
| Enemy base (white) | `#F2F8FF` · blue `#6EC1FF` · deep blue `#2C6FD1` |
| Protagonist | **black/red** (team decision, D-154): painted white body → black `#0D0809`, blue → red `#C8232C`, deep blue → deep red `#5A0D12`, recoloured by shader |
| **Zulmat (final boss)** | black `#0A0305` + crimson rim `#FF2A3A` (inverse of current enemies) |
| Flame core / glow | `#CFEAFF` / `#3EA0FF` |
### Ending — flame = purple `#9B4DFF` core `#E5CCFF`; parchment base shifts to `#1A1030`.

## 3. The Flame (`FlameController`, `FlameConfig` SO)
Screen-space position in normalised viewport coords (0–1). Parametric arc `t ∈ [0,1]`:
`pos(t) = (lerp(0.05, 0.95, t), 0.05 + 0.85 · sin(π·t))` → starts **bottom-left**, rises, ends **bottom-right** (like the Minecraft sun but 2D).
| | Phase 1 | Phase 2 |
|---|---|---|
| Colour | Orange | Blue |
| Motion | Slow **ping-pong** arc, **480 s** per sweep (cosmetic) | **One-way** arc, **1200 s (20 min)** from the moment Phase 2 gameplay starts. Persisted in save (`flameT`). At t = 1 it parks bottom-right (enemies stay max size) |
| Flicker | Perlin noise on intensity (±8 % at 6 Hz) + `FlameCore` sprite animation | same, ±12 % at 9 Hz, a bit faster |
| Effect on gameplay | none | **Enemy scaling** (below) |
| Boss fights | n/a | **MB3:** flame keeps moving but bosses do not scale. **Final boss:** flame **parks** at top-centre `(0.5, 0.85)`, intensity ×1.2, and pulses on boss moves |
`Light2D` (Point, URP 2D): sits at the flame's **world-projected** position (camera-relative, behind the world plane), radius ~ 1.6× screen, falloff 0.6, intensity ~1.0–1.4. A dim **Global Light 2D** (intensity 0.55) sets the ambient tint per theme.
**Benches** save `flameT`; **dying restores it** (doc 01 §12.8).

## 4. Enemy size scaling — Phase 2 only (`FlameScaler` on `EnemyBase`)
- `closeness = t` (monotonic, default) — option `ScaleMode.Peak` uses `sin(π·t)` instead.
- `scale = lerp(1.0, 1.4, easeInQuad(closeness))` → **max ×1.4**.
- Applied to **visual and collider** (so hitboxes are honest). Scale pivot = feet (ground enemies) / centre (flyers). Updates smoothly (lerp 2 s^-1), not per-frame jitter.
- **Stats do not change** (HP/damage same). Optional: attack reach ×scale; knife projectile size ×(1 + 0.5·(scale−1)).
- Player **never** scales. Bosses never scale with flame.
- HUD shows a small **flame-arc indicator** at the top-centre (a thin arc with a dot) so players understand the pressure.
- Level design guarantee: ≥ 4 u ceiling clearance (doc 05 §1).
- Safety: if scaled enemy would overlap a wall on scale-up, nudge it away (use `Physics2D.OverlapBox` before applying).

## 5. Soft drop-shadows (`PuppetShadow` component)
Sells the "light behind paper" feel and flame position.
- Each character/enemy gets a child `SpriteRenderer` using the same sprite (synced each frame), tinted black, alpha **0.30** (P1) / **0.35** (P2), sorting just behind the parent, material `Sprite-Unlit`.
- Offset = `normalize(entityScreenPos − flameScreenPos) × k`, with `k = 0.10 u` base, up to `0.35 u` when closer to the flame; slight scale ×1.04 and a **blur approximation** (second copy with alpha 0.15, scale ×1.08).
- Skip on projectiles/small FX. Toggle in `ArtStyleConfig`.

## 6. Puppet feel (cheap, on-theme polish)
- **Puppet jitter:** enemy sprites have an optional 8 Hz ±0.01 u micro-jitter (disabled for player).
- **Rods:** optional thin vertical stick sprite under each enemy (off by default).
- **Paper edge:** `ParchmentFrame` darkens edges; slight **film grain** + **bloom** (Volume). Phase 2: higher contrast, cooler grade.
- **Paper tear transitions:** room-to-room fade is a quick 0.25 s parchment-coloured wipe; death uses a **burn** wipe (orange→black circle).
- **Light flicker on hit:** the flame light spikes +30 % for 0.1 s on boss hits.

## 7. Theme system (`ThemeConfig` SO, `ThemeManager`)
Fields: parchment sprite + tint, frame tint, global light colour/intensity, flame colour/glow colour/intensity/radius/flicker, volume profile, UI tint, BG sprites, tile theme name, enemy accent tint, `shadowAlpha`.
`ThemeManager.Apply(theme, duration)` cross-fades everything (colour lerps, sprite swap with fade). Themes: `Theme_Phase1_Beige`, `Theme_Phase2_Inverted`, `Theme_Ending_Purple`, `Theme_Cutscene` (neutral).
**Phase transition effect (cutscene 2 end):** an **inversion wipe**: a circular mask expands from the flame's position over **1.5 s**; inside the circle the parchment/theme is Phase 2 colours, outside Phase 1; flame colour tweens orange → blue; screenshake; the world is reloaded behind a full-screen cover at the end of the wipe.
Implementation hint: a full-screen UI `Image` with a **UI mask (circle sprite)** scaled up, child `Image` = Phase-2 coloured parchment texture. Fallback: simple crossfade.

## 8. Lighting/shader constraints
- Only **Sprite-Lit-Default** and **Sprite-Unlit-Default**; no custom shader graphs required. If a custom unlit tint shader is needed, write plain HLSL/ShaderLab for URP 2D, keep it < 60 lines.
- Colliders are not affected by lighting; flame is purely visual except scaling.
- Max **2 Light2D** active simultaneously; no shadow-casting on lights (perf).

## 9. Acceptance checklist
- [ ] Parchment stays fixed while world scrolls; texture seamless.
- [ ] Flame glow visibly moves and drifts the brightness across the paper.
- [ ] Drop-shadows shift as the flame moves.
- [ ] Phase 2: flame blue, arcs left→right in ~20 min, enemies noticeably larger by 60 %.
- [ ] Inversion wipe transition works; Phase 2 palette readable, protagonist is clearly distinguishable.
- [ ] Final boss fight: flame parked; Zulmat reads red/black.
- [ ] Ending: purple flame.
