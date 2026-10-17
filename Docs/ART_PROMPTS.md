# ART PROMPTS — Cinders of Fate

Every image the game loads, with the **exact file name, canvas size, frame count and timing**, plus a ready-to-paste prompt for an image AI.
Generated from `Assets/Data/AnimationManifest.json` (the importer's source of truth) — if a name or size here doesn't match, the game falls back to the placeholder.
**131 sprite sets · 741 PNG files in total.** Draw in this order if time is short: Protagonist → Phase 1 enemies → cave tiles + parchment → Phase 1 bosses → Phase 2 enemies + castle tiles → Colossus → Zulmat → FX → cutscenes.


## 0. How to use this

1. **Rules for every file:** PNG, RGBA, **transparent background**, **exact canvas size** listed (every frame of a character uses the same canvas so nothing jitters), characters drawn **facing right**, one PNG per frame, frame numbers zero-padded (`_00`, `_01`…). 100 px = 1 game unit.
2. **Keep the body size**: each character lists the size its body should occupy on the canvas (that is its hitbox). Weapons / effects may extend beyond it inside the canvas.
3. **Consistency workflow for an image AI:** first generate a **character reference sheet** with the identity prompt, pick the best result, then generate each animation using that image as the reference (same seed / image-to-image / character reference feature). Ask for a **horizontal strip of N frames, each exactly W×H**, then cut it into frames — or generate the key poses and draw in-betweens.
4. **Drop the files in** at the listed paths (overwrite the placeholders), then in Unity: **Shatranj → Import & Rebuild Animations → Build Prefabs → Build All Scenes**.
5. **Loop animations:** the last frame must lead back into the first (don't repeat frame 0 at the end). Non-loop animations hold their last frame.
6. **Protagonist colours matter:** paint him in exactly white `#EAF6FF`, blue `#4FA3FF`, deep blue `#1B4F9C`. A shader turns those into black / red in Phase 2.

### Style block — paste in front of every character prompt

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border
```

### Negative prompt (if your AI supports one)

```
background scenery, floor, drop shadow, gradient backdrop, text, watermark, signature, border, frame, 3D render, photorealistic, blurry, soft airbrush, anti-aliased halo on transparency, multiple characters, cropped limbs, character facing left, perspective / three-quarter view
```

### Style blocks for other categories

- **FX:** `2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border`
- **Environment / backgrounds / tiles:** `2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture`
- **Cutscene panels:** `full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text`

### World & palettes (for context)

A shadow-puppet theatre: a paper screen fills the display and a flame burns behind it; everything on stage is a cut-paper puppet pressed against the paper. **Phase 1 (cave):** beige paper, orange flame, enemies black with red. **Phase 2 (castle):** grey-blue paper, blue flame, enemies white with blue, the hero turns black/red (shader). **Final boss:** black with crimson rim light. **Ending:** purple flame.

| Group | Colours |
|---|---|
| Protagonist (paint these) | white `#EAF6FF` · blue `#4FA3FF` · deep blue `#1B4F9C` |
| Phase 1 enemies & mini-bosses | black `#16090A` · red `#C1272D` · glow `#FF4B3E` |
| Phase 2 enemies & Colossus | white `#F2F8FF` · light blue `#6EC1FF` · deep blue `#2C6FD1` |
| Zulmat | black `#0A0305` · crimson rim `#FF2A3A` |
| Cave environment | `#26262A` · `#4A4A52` · accent `#6B3A3A` |
| Castle environment | `#D8DEE9` · `#A7B0C4` · accent `#5E86A8` |
| Parchment | beige `#E8D9B5` · burnt edge `#9C8460` |

## Contents

1. Characters
2. FX & projectiles
3. Props
4. Tiles
5. Backgrounds & foreground overlays
6. Cutscene panels
7. UI

## 1. Characters

### Protagonist

- **Who:** The hero (player). A small, brave blue-white creature from a village; a compassionate half of a broken god (revealed at the end).
- **Canvas:** 256×256 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 110×170 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/Protagonist/<Anim>/Protagonist_<Anim>_NN.png` · **17 animations, 67 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Canvas 256x256 px, the body about 110x170 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Idle | 6 | 8 | loop | `Assets/Art/Characters/Protagonist/Idle/Protagonist_Idle_00.png` … `Protagonist_Idle_05.png` |
| Run | 8 | 14 | loop | `Assets/Art/Characters/Protagonist/Run/Protagonist_Run_00.png` … `Protagonist_Run_07.png` |
| Jump_Rise | 2 | 10 | hold last | `Assets/Art/Characters/Protagonist/Jump_Rise/Protagonist_Jump_Rise_00.png` … `Protagonist_Jump_Rise_01.png` |
| Jump_Fall | 2 | 10 | loop | `Assets/Art/Characters/Protagonist/Jump_Fall/Protagonist_Jump_Fall_00.png` … `Protagonist_Jump_Fall_01.png` |
| Land | 3 | 16 | once | `Assets/Art/Characters/Protagonist/Land/Protagonist_Land_00.png` … `Protagonist_Land_02.png` |
| Attack_Side_A | 4 | 20 | once | `Assets/Art/Characters/Protagonist/Attack_Side_A/Protagonist_Attack_Side_A_00.png` … `Protagonist_Attack_Side_A_03.png` |
| Attack_Side_B | 4 | 20 | once | `Assets/Art/Characters/Protagonist/Attack_Side_B/Protagonist_Attack_Side_B_00.png` … `Protagonist_Attack_Side_B_03.png` |
| Attack_Up | 4 | 20 | once | `Assets/Art/Characters/Protagonist/Attack_Up/Protagonist_Attack_Up_00.png` … `Protagonist_Attack_Up_03.png` |
| Cast | 5 | 14 | once | `Assets/Art/Characters/Protagonist/Cast/Protagonist_Cast_00.png` … `Protagonist_Cast_04.png` |
| Heal_Charge | 6 | 10 | loop | `Assets/Art/Characters/Protagonist/Heal_Charge/Protagonist_Heal_Charge_00.png` … `Protagonist_Heal_Charge_05.png` |
| Heal_Release | 3 | 12 | once | `Assets/Art/Characters/Protagonist/Heal_Release/Protagonist_Heal_Release_00.png` … `Protagonist_Heal_Release_02.png` |
| Hurt | 2 | 12 | once | `Assets/Art/Characters/Protagonist/Hurt/Protagonist_Hurt_00.png` … `Protagonist_Hurt_01.png` |
| Death | 8 | 10 | once | `Assets/Art/Characters/Protagonist/Death/Protagonist_Death_00.png` … `Protagonist_Death_07.png` |
| Dash | 4 | 20 | once | `Assets/Art/Characters/Protagonist/Dash/Protagonist_Dash_00.png` … `Protagonist_Dash_03.png` |
| Rest | 4 | 6 | loop | `Assets/Art/Characters/Protagonist/Rest/Protagonist_Rest_00.png` … `Protagonist_Rest_03.png` |
| Look_Up | 1 | 1 | once | `Assets/Art/Characters/Protagonist/Look_Up/Protagonist_Look_Up_00.png` |
| Look_Down | 1 | 1 | once | `Assets/Art/Characters/Protagonist/Look_Down/Protagonist_Look_Down_00.png` |

**Protagonist — Idle** (6 frames @ 8 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Idle', 6 frames, looping (last frame flows back into the first), each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: chest rising slightly, shoulders up 2 px. Frame 3: chest at top of breath. Frame 4: settling back down. Frame 5: shoulders lowest, slight sag. Frame 6: rising again toward neutral.
```

**Protagonist — Run** (8 frames @ 14 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Run', 8 frames, looping (last frame flows back into the first), each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: right foot contact, body low, leaning forward. Frame 2: pushing off, body rising. Frame 3: airborne passing pose, legs crossed. Frame 4: left foot reaching forward. Frame 5: left foot contact, body low. Frame 6: push off. Frame 7: airborne passing pose. Frame 8: right foot reaching forward.
```

**Protagonist — Jump_Rise** (2 frames @ 10 fps, plays once and holds the last frame)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Jump_Rise', 2 frames, plays once and holds the last frame, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: take-off: knees tucked, arms up. Frame 2: rising: body stretched upward, scarf trailing down.
```

**Protagonist — Jump_Fall** (2 frames @ 10 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Jump_Fall', 2 frames, looping (last frame flows back into the first), each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: falling: legs slightly apart, arms out for balance, scarf flowing up. Frame 2: falling: legs together, scarf fluttering.
```

**Protagonist — Land** (3 frames @ 16 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Land', 3 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: impact: deep crouch. Frame 2: half crouch rising. Frame 3: back to standing.
```

**Protagonist — Attack_Side_A** (4 frames @ 20 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Attack_Side_A', 4 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: blade drawn back over the shoulder. Frame 2: fast horizontal slash forward. Frame 3: blade fully extended in front. Frame 4: recovering, blade lowering.
```

**Protagonist — Attack_Side_B** (4 frames @ 20 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Attack_Side_B', 4 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: blade low behind (backhand). Frame 2: fast rising backhand slash. Frame 3: blade extended up-forward. Frame 4: recovering.
```

**Protagonist — Attack_Up** (4 frames @ 20 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Attack_Up', 4 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: crouching, blade held low. Frame 2: slashing straight upward. Frame 3: blade fully overhead. Frame 4: recovering.
```

**Protagonist — Cast** (5 frames @ 14 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Cast', 5 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: one hand pulled back, palm glowing. Frame 2: glow grows. Frame 3: thrusting the palm forward. Frame 4: release: arm extended, burst of light. Frame 5: recovering.
```

**Protagonist — Heal_Charge** (6 frames @ 10 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Heal_Charge', 6 frames, looping (last frame flows back into the first), each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: kneeling, hands together at the chest, soft glow. Frame 2: glow pulsing brighter. Frame 3: eyes closed, glow peak. Frame 4: glow dimming. Frame 5: glow pulsing. Frame 6: glow steady.
```

**Protagonist — Heal_Release** (3 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Heal_Release', 3 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: standing up, light bursting outward. Frame 2: arms spread, sparkles. Frame 3: back to neutral.
```

**Protagonist — Hurt** (2 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Hurt', 2 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: recoil: head and torso snapped backwards, limbs flung. Frame 2: stagger: bent backwards, wincing pose.
```

**Protagonist — Death** (8 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Death', 8 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: struck, recoiling. Frame 2: falling to the knees. Frame 3: kneeling, head bowed. Frame 4: falling forward. Frame 5: lying down. Frame 6: body crumbling into embers/paper. Frame 7: embers drifting up. Frame 8: nearly empty, last embers.
```

**Protagonist — Dash** (4 frames @ 20 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Dash', 4 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: lunging forward, body horizontal, scarf streaming back. Frame 2: stretched dash pose with speed lines. Frame 3: afterimage pose. Frame 4: coming out of the dash.
```

**Protagonist — Rest** (4 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Rest', 4 frames, looping (last frame flows back into the first), each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: sitting cross-legged facing right, head bowed. Frame 2: breathing in. Frame 3: breathing out, shoulders relaxed. Frame 4: breathing in.
```

**Protagonist — Look_Up** (1 frames @ 1 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Look_Up', 1 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: standing, head tilted up looking upward.
```

**Protagonist — Look_Down** (1 frames @ 1 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small slim humanoid creature with a rounded head, two short pointed ear-like horns, big bright eyes, a short flowing scarf/cape, slender limbs, carries a short slightly curved blade (a 'nail') in the right hand. Reads as kind, nimble and determined. Colours: EXACTLY three tones: white body #EAF6FF, blue accents #4FA3FF, deep blue shading #1B4F9C (keep these three flat colours — in Phase 2 a shader remaps them to black / red / deep red). Animation 'Look_Down', 1 frames, plays once, each frame 256x256 px, body about 110x170 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: standing, head tilted down looking at the ground.
```

### Crawler

- **Who:** Shade Crawler — the weakest Phase 1 cave enemy; scuttles along the floor.
- **Canvas:** 128×128 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 80×70 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/Crawler/<Anim>/Crawler_<Anim>_NN.png` · **4 animations, 18 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Small black cube-ish ninja, almost a box with stubby legs, hunched low to the ground, two glowing red eyes and a red scarf tail. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Canvas 128x128 px, the body about 80x70 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Idle | 4 | 6 | loop | `Assets/Art/Characters/Crawler/Idle/Crawler_Idle_00.png` … `Crawler_Idle_03.png` |
| Walk | 6 | 10 | loop | `Assets/Art/Characters/Crawler/Walk/Crawler_Walk_00.png` … `Crawler_Walk_05.png` |
| Hurt | 2 | 12 | once | `Assets/Art/Characters/Crawler/Hurt/Crawler_Hurt_00.png` … `Crawler_Hurt_01.png` |
| Death | 6 | 12 | once | `Assets/Art/Characters/Crawler/Death/Crawler_Death_00.png` … `Crawler_Death_05.png` |

**Crawler — Idle** (4 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small black cube-ish ninja, almost a box with stubby legs, hunched low to the ground, two glowing red eyes and a red scarf tail. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Idle', 4 frames, looping (last frame flows back into the first), each frame 128x128 px, body about 80x70 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: chest rising slightly, shoulders up 2 px. Frame 3: settling back down. Frame 4: shoulders lowest, slight sag.
```

**Crawler — Walk** (6 frames @ 10 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small black cube-ish ninja, almost a box with stubby legs, hunched low to the ground, two glowing red eyes and a red scarf tail. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Walk', 6 frames, looping (last frame flows back into the first), each frame 128x128 px, body about 80x70 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: scuttle: legs left. Frame 2: body dips. Frame 3: legs right. Frame 4: body bobs up. Frame 5: legs left. Frame 6: body dips.
```

**Crawler — Hurt** (2 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small black cube-ish ninja, almost a box with stubby legs, hunched low to the ground, two glowing red eyes and a red scarf tail. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Hurt', 2 frames, plays once, each frame 128x128 px, body about 80x70 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: recoil: head and torso snapped backwards, limbs flung. Frame 2: stagger: bent backwards, wincing pose.
```

**Crawler — Death** (6 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Small black cube-ish ninja, almost a box with stubby legs, hunched low to the ground, two glowing red eyes and a red scarf tail. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Death', 6 frames, plays once, each frame 128x128 px, body about 80x70 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: knees buckling. Frame 3: falling, body tilting. Frame 4: silhouette cracking into paper fragments. Frame 5: fragments drifting apart. Frame 6: only a few paper scraps left.
```

### Lunger

- **Who:** Ninja Lunger — Phase 1 enemy that crouches and pounces horizontally.
- **Canvas:** 160×160 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 90×100 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/Lunger/<Anim>/Lunger_<Anim>_NN.png` · **7 animations, 27 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Slightly taller black ninja than the crawler, lean, hood and red scarf, red eye slit, springy crouching legs. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Canvas 160x160 px, the body about 90x100 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Idle | 4 | 6 | loop | `Assets/Art/Characters/Lunger/Idle/Lunger_Idle_00.png` … `Lunger_Idle_03.png` |
| Walk | 6 | 10 | loop | `Assets/Art/Characters/Lunger/Walk/Lunger_Walk_00.png` … `Lunger_Walk_05.png` |
| Telegraph | 3 | 10 | once | `Assets/Art/Characters/Lunger/Telegraph/Lunger_Telegraph_00.png` … `Lunger_Telegraph_02.png` |
| Lunge | 3 | 16 | hold last | `Assets/Art/Characters/Lunger/Lunge/Lunger_Lunge_00.png` … `Lunger_Lunge_02.png` |
| Recover | 3 | 8 | once | `Assets/Art/Characters/Lunger/Recover/Lunger_Recover_00.png` … `Lunger_Recover_02.png` |
| Hurt | 2 | 12 | once | `Assets/Art/Characters/Lunger/Hurt/Lunger_Hurt_00.png` … `Lunger_Hurt_01.png` |
| Death | 6 | 12 | once | `Assets/Art/Characters/Lunger/Death/Lunger_Death_00.png` … `Lunger_Death_05.png` |

**Lunger — Idle** (4 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Slightly taller black ninja than the crawler, lean, hood and red scarf, red eye slit, springy crouching legs. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Idle', 4 frames, looping (last frame flows back into the first), each frame 160x160 px, body about 90x100 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: chest rising slightly, shoulders up 2 px. Frame 3: settling back down. Frame 4: shoulders lowest, slight sag.
```

**Lunger — Walk** (6 frames @ 10 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Slightly taller black ninja than the crawler, lean, hood and red scarf, red eye slit, springy crouching legs. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Walk', 6 frames, looping (last frame flows back into the first), each frame 160x160 px, body about 90x100 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: right foot forward contact. Frame 2: weight down on right leg. Frame 3: passing pose, left leg swinging. Frame 4: left foot forward contact. Frame 5: weight down on left leg. Frame 6: passing pose, right leg swinging.
```

**Lunger — Telegraph** (3 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Slightly taller black ninja than the crawler, lean, hood and red scarf, red eye slit, springy crouching legs. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Telegraph', 3 frames, plays once, each frame 160x160 px, body about 90x100 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: dropping into a crouch. Frame 2: deep crouch, coiled. Frame 3: coiled crouch held, eye glowing (pounce warning).
```

**Lunger — Lunge** (3 frames @ 16 fps, plays once and holds the last frame)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Slightly taller black ninja than the crawler, lean, hood and red scarf, red eye slit, springy crouching legs. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Lunge', 3 frames, plays once and holds the last frame, each frame 160x160 px, body about 90x100 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: launching forward horizontally, body stretched. Frame 2: flying forward, blade-like arms extended. Frame 3: fully stretched pounce (held while sliding).
```

**Lunger — Recover** (3 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Slightly taller black ninja than the crawler, lean, hood and red scarf, red eye slit, springy crouching legs. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Recover', 3 frames, plays once, each frame 160x160 px, body about 90x100 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: skidding to a stop, off balance. Frame 2: hunched, catching breath. Frame 3: straightening up.
```

**Lunger — Hurt** (2 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Slightly taller black ninja than the crawler, lean, hood and red scarf, red eye slit, springy crouching legs. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Hurt', 2 frames, plays once, each frame 160x160 px, body about 90x100 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: recoil: head and torso snapped backwards, limbs flung. Frame 2: stagger: bent backwards, wincing pose.
```

**Lunger — Death** (6 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Slightly taller black ninja than the crawler, lean, hood and red scarf, red eye slit, springy crouching legs. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Death', 6 frames, plays once, each frame 160x160 px, body about 90x100 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: knees buckling. Frame 3: falling, body tilting. Frame 4: silhouette cracking into paper fragments. Frame 5: fragments drifting apart. Frame 6: only a few paper scraps left.
```

### ShadowKite

- **Who:** Shadow Kite — Phase 1 flyer that hovers and dives.
- **Canvas:** 128×128 px · **pivot:** centre · **body on canvas:** about 100×80 px, centred in the canvas (it flies)
- **Folder:** `Assets/Art/Characters/ShadowKite/<Anim>/ShadowKite_<Anim>_NN.png` · **5 animations, 19 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Paper-cut kite / bird shape: a diamond-shaped body with angular wings and a long ribbon tail, single red eye. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Canvas 128x128 px, the body about 100x80 px, centred in the canvas (it flies). Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Fly | 6 | 12 | loop | `Assets/Art/Characters/ShadowKite/Fly/ShadowKite_Fly_00.png` … `ShadowKite_Fly_05.png` |
| Telegraph | 3 | 10 | once | `Assets/Art/Characters/ShadowKite/Telegraph/ShadowKite_Telegraph_00.png` … `ShadowKite_Telegraph_02.png` |
| Dive | 2 | 12 | loop | `Assets/Art/Characters/ShadowKite/Dive/ShadowKite_Dive_00.png` … `ShadowKite_Dive_01.png` |
| Hurt | 2 | 12 | once | `Assets/Art/Characters/ShadowKite/Hurt/ShadowKite_Hurt_00.png` … `ShadowKite_Hurt_01.png` |
| Death | 6 | 12 | once | `Assets/Art/Characters/ShadowKite/Death/ShadowKite_Death_00.png` … `ShadowKite_Death_05.png` |

**ShadowKite — Fly** (6 frames @ 12 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Paper-cut kite / bird shape: a diamond-shaped body with angular wings and a long ribbon tail, single red eye. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Fly', 6 frames, looping (last frame flows back into the first), each frame 128x128 px, body about 100x80 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: wings fully up. Frame 2: wings sweeping down. Frame 3: wings level. Frame 4: wings fully down. Frame 5: wings sweeping up. Frame 6: wings level, body bobbing.
```

**ShadowKite — Telegraph** (3 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Paper-cut kite / bird shape: a diamond-shaped body with angular wings and a long ribbon tail, single red eye. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Telegraph', 3 frames, plays once, each frame 128x128 px, body about 100x80 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: tail flaring upward. Frame 2: wings pulled back, tail fanned. Frame 3: body tilted nose-down, eye bright (dive warning).
```

**ShadowKite — Dive** (2 frames @ 12 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Paper-cut kite / bird shape: a diamond-shaped body with angular wings and a long ribbon tail, single red eye. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Dive', 2 frames, looping (last frame flows back into the first), each frame 128x128 px, body about 100x80 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: streamlined dive, wings folded back. Frame 2: dive with tail whipping.
```

**ShadowKite — Hurt** (2 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Paper-cut kite / bird shape: a diamond-shaped body with angular wings and a long ribbon tail, single red eye. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Hurt', 2 frames, plays once, each frame 128x128 px, body about 100x80 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: recoil: head and torso snapped backwards, limbs flung. Frame 2: stagger: bent backwards, wincing pose.
```

**ShadowKite — Death** (6 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Paper-cut kite / bird shape: a diamond-shaped body with angular wings and a long ribbon tail, single red eye. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf). Animation 'Death', 6 frames, plays once, each frame 128x128 px, body about 100x80 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: knees buckling. Frame 3: falling, body tilting. Frame 4: silhouette cracking into paper fragments. Frame 5: fragments drifting apart. Frame 6: only a few paper scraps left.
```

### KageWarden

- **Who:** Kage Warden — Phase 1 mini-boss #1, the gate guard. Swordsman who slashes, back-steps and blocks with his blade.
- **Canvas:** 384×384 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 160×240 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/KageWarden/<Anim>/KageWarden_<Anim>_NN.png` · **10 animations, 48 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Canvas 384x384 px, the body about 160x240 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Intro | 6 | 8 | once | `Assets/Art/Characters/KageWarden/Intro/KageWarden_Intro_00.png` … `KageWarden_Intro_05.png` |
| Idle | 6 | 8 | loop | `Assets/Art/Characters/KageWarden/Idle/KageWarden_Idle_00.png` … `KageWarden_Idle_05.png` |
| Walk | 8 | 10 | loop | `Assets/Art/Characters/KageWarden/Walk/KageWarden_Walk_00.png` … `KageWarden_Walk_07.png` |
| Slash_Telegraph | 3 | 8 | once | `Assets/Art/Characters/KageWarden/Slash_Telegraph/KageWarden_Slash_Telegraph_00.png` … `KageWarden_Slash_Telegraph_02.png` |
| Slash_A | 4 | 18 | once | `Assets/Art/Characters/KageWarden/Slash_A/KageWarden_Slash_A_00.png` … `KageWarden_Slash_A_03.png` |
| Slash_B | 4 | 18 | once | `Assets/Art/Characters/KageWarden/Slash_B/KageWarden_Slash_B_00.png` … `KageWarden_Slash_B_03.png` |
| Block_Start | 3 | 16 | once | `Assets/Art/Characters/KageWarden/Block_Start/KageWarden_Block_Start_00.png` … `KageWarden_Block_Start_02.png` |
| Block_Hold | 2 | 6 | loop | `Assets/Art/Characters/KageWarden/Block_Hold/KageWarden_Block_Hold_00.png` … `KageWarden_Block_Hold_01.png` |
| Block_Impact | 2 | 16 | once | `Assets/Art/Characters/KageWarden/Block_Impact/KageWarden_Block_Impact_00.png` … `KageWarden_Block_Impact_01.png` |
| Death | 10 | 10 | once | `Assets/Art/Characters/KageWarden/Death/KageWarden_Death_00.png` … `KageWarden_Death_09.png` |

**KageWarden — Intro** (6 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Intro', 6 frames, plays once, each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: standing tall, head down. Frame 2: head rising. Frame 3: weapon drawn / arms spreading. Frame 4: menacing pose. Frame 5: accent glow flares. Frame 6: battle stance.
```

**KageWarden — Idle** (6 frames @ 8 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Idle', 6 frames, looping (last frame flows back into the first), each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: chest rising slightly, shoulders up 2 px. Frame 3: chest at top of breath. Frame 4: settling back down. Frame 5: shoulders lowest, slight sag. Frame 6: rising again toward neutral.
```

**KageWarden — Walk** (8 frames @ 10 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Walk', 8 frames, looping (last frame flows back into the first), each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: right foot forward contact. Frame 2: weight down on right leg. Frame 3: passing pose, left leg swinging. Frame 4: left leg up and forward. Frame 5: left foot forward contact. Frame 6: weight down on left leg. Frame 7: passing pose, right leg swinging. Frame 8: right leg up and forward.
```

**KageWarden — Slash_Telegraph** (3 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Slash_Telegraph', 3 frames, plays once, each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: starting the wind-up, weight shifting back. Frame 2: deeper wind-up, weapon/limb pulled back. Frame 3: full wind-up held, accent glow brightest (clear warning pose).
```

**KageWarden — Slash_A** (4 frames @ 18 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Slash_A', 4 frames, plays once, each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: blade raised high over the right shoulder. Frame 2: huge diagonal downward slash. Frame 3: blade fully extended low-forward. Frame 4: follow-through.
```

**KageWarden — Slash_B** (4 frames @ 18 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Slash_B', 4 frames, plays once, each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: blade low on the left. Frame 2: rising reverse slash. Frame 3: blade high. Frame 4: recover to stance.
```

**KageWarden — Block_Start** (3 frames @ 16 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Block_Start', 3 frames, plays once, each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: raising the blade vertically in front. Frame 2: blade held across the body. Frame 3: solid guard pose.
```

**KageWarden — Block_Hold** (2 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Block_Hold', 2 frames, looping (last frame flows back into the first), each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: guard pose, blade vertical in front of the body. Frame 2: guard pose, slight tension shift.
```

**KageWarden — Block_Impact** (2 frames @ 16 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Block_Impact', 2 frames, plays once, each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: blade jolted by an impact, sparks. Frame 2: guard pose recovering.
```

**KageWarden — Death** (10 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall black swordsman in layered armour-cloth, wide straw-hat silhouette or topknot, red sash, a big straight blade held in both hands. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red sash. Animation 'Death', 10 frames, plays once, each frame 384x384 px, body about 160x240 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: hit hard, recoiling backwards. Frame 3: knees buckling. Frame 4: falling, body tilting. Frame 5: collapsing to the ground. Frame 6: silhouette cracking into paper fragments. Frame 7: silhouette cracking into paper fragments. Frame 8: fragments drifting apart. Frame 9: only a few paper scraps left. Frame 10: last faint scraps fading (nearly empty frame).
```

### KageCaller

- **Who:** Kage Caller — Phase 1 mini-boss #2, guards the orb. Summoner swordsman who fires crescent 'black slashes'.
- **Canvas:** 384×384 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 150×230 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/KageCaller/<Anim>/KageCaller_<Anim>_NN.png` · **11 animations, 57 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Canvas 384x384 px, the body about 150x230 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Intro | 6 | 8 | once | `Assets/Art/Characters/KageCaller/Intro/KageCaller_Intro_00.png` … `KageCaller_Intro_05.png` |
| Idle | 6 | 8 | loop | `Assets/Art/Characters/KageCaller/Idle/KageCaller_Idle_00.png` … `KageCaller_Idle_05.png` |
| Walk | 8 | 10 | loop | `Assets/Art/Characters/KageCaller/Walk/KageCaller_Walk_00.png` … `KageCaller_Walk_07.png` |
| Slash_Telegraph | 3 | 8 | once | `Assets/Art/Characters/KageCaller/Slash_Telegraph/KageCaller_Slash_Telegraph_00.png` … `KageCaller_Slash_Telegraph_02.png` |
| Slash_A | 4 | 18 | once | `Assets/Art/Characters/KageCaller/Slash_A/KageCaller_Slash_A_00.png` … `KageCaller_Slash_A_03.png` |
| Slash_B | 4 | 18 | once | `Assets/Art/Characters/KageCaller/Slash_B/KageCaller_Slash_B_00.png` … `KageCaller_Slash_B_03.png` |
| Cast_Telegraph | 4 | 10 | once | `Assets/Art/Characters/KageCaller/Cast_Telegraph/KageCaller_Cast_Telegraph_00.png` … `KageCaller_Cast_Telegraph_03.png` |
| Cast_Fire | 4 | 16 | once | `Assets/Art/Characters/KageCaller/Cast_Fire/KageCaller_Cast_Fire_00.png` … `KageCaller_Cast_Fire_03.png` |
| Summon_Raise | 6 | 10 | once | `Assets/Art/Characters/KageCaller/Summon_Raise/KageCaller_Summon_Raise_00.png` … `KageCaller_Summon_Raise_05.png` |
| Summon_Hold | 2 | 6 | loop | `Assets/Art/Characters/KageCaller/Summon_Hold/KageCaller_Summon_Hold_00.png` … `KageCaller_Summon_Hold_01.png` |
| Death | 10 | 10 | once | `Assets/Art/Characters/KageCaller/Death/KageCaller_Death_00.png` … `KageCaller_Death_09.png` |

**KageCaller — Intro** (6 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Intro', 6 frames, plays once, each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: standing tall, head down. Frame 2: head rising. Frame 3: weapon drawn / arms spreading. Frame 4: menacing pose. Frame 5: accent glow flares. Frame 6: battle stance.
```

**KageCaller — Idle** (6 frames @ 8 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Idle', 6 frames, looping (last frame flows back into the first), each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: chest rising slightly, shoulders up 2 px. Frame 3: chest at top of breath. Frame 4: settling back down. Frame 5: shoulders lowest, slight sag. Frame 6: rising again toward neutral.
```

**KageCaller — Walk** (8 frames @ 10 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Walk', 8 frames, looping (last frame flows back into the first), each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: right foot forward contact. Frame 2: weight down on right leg. Frame 3: passing pose, left leg swinging. Frame 4: left leg up and forward. Frame 5: left foot forward contact. Frame 6: weight down on left leg. Frame 7: passing pose, right leg swinging. Frame 8: right leg up and forward.
```

**KageCaller — Slash_Telegraph** (3 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Slash_Telegraph', 3 frames, plays once, each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: starting the wind-up, weight shifting back. Frame 2: deeper wind-up, weapon/limb pulled back. Frame 3: full wind-up held, accent glow brightest (clear warning pose).
```

**KageCaller — Slash_A** (4 frames @ 18 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Slash_A', 4 frames, plays once, each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: weapon raised behind, anticipation. Frame 2: fast swing, motion arc. Frame 3: impact: weapon fully extended forward. Frame 4: follow-through, recovering.
```

**KageCaller — Slash_B** (4 frames @ 18 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Slash_B', 4 frames, plays once, each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: weapon raised behind, anticipation. Frame 2: fast swing, motion arc. Frame 3: impact: weapon fully extended forward. Frame 4: follow-through, recovering.
```

**KageCaller — Cast_Telegraph** (4 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Cast_Telegraph', 4 frames, plays once, each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: raising the blade overhead. Frame 2: blade glowing red. Frame 3: red glow intensifies, aura flares. Frame 4: glow at maximum (warning).
```

**KageCaller — Cast_Fire** (4 frames @ 16 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Cast_Fire', 4 frames, plays once, each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: swinging the blade down-forward. Frame 2: releasing a red crescent. Frame 3: blade low, crescent leaving. Frame 4: recover.
```

**KageCaller — Summon_Raise** (6 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Summon_Raise', 6 frames, plays once, each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: raising the left arm. Frame 2: arm up, smoke gathering. Frame 3: smoke swirling. Frame 4: red sigil glowing above the hand. Frame 5: sigil bright. Frame 6: holding.
```

**KageCaller — Summon_Hold** (2 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Summon_Hold', 2 frames, looping (last frame flows back into the first), each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: arm raised, smoke swirling. Frame 2: arm raised, sigil pulsing.
```

**KageCaller — Death** (10 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Robed black swordsman with a deep hood, trailing smoke wisps at the hem, a thin curved blade, a red crescent aura behind the head. Colours: black #16090A body, red #C1272D details, glowing red accent #FF4B3E (eyes / scarf); red crescent aura. Animation 'Death', 10 frames, plays once, each frame 384x384 px, body about 150x230 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: hit hard, recoiling backwards. Frame 3: knees buckling. Frame 4: falling, body tilting. Frame 5: collapsing to the ground. Frame 6: silhouette cracking into paper fragments. Frame 7: silhouette cracking into paper fragments. Frame 8: fragments drifting apart. Frame 9: only a few paper scraps left. Frame 10: last faint scraps fading (nearly empty frame).
```

### Knight

- **Who:** Castle Knight — Phase 2 enemy with a 2-hit sword combo.
- **Canvas:** 256×256 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 110×190 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/Knight/<Anim>/Knight_<Anim>_NN.png` · **7 animations, 33 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: White-blue armoured knight with a tall plumed helmet, kite shield on the back, longsword. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Canvas 256x256 px, the body about 110x190 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Idle | 4 | 6 | loop | `Assets/Art/Characters/Knight/Idle/Knight_Idle_00.png` … `Knight_Idle_03.png` |
| Walk | 8 | 10 | loop | `Assets/Art/Characters/Knight/Walk/Knight_Walk_00.png` … `Knight_Walk_07.png` |
| Telegraph | 3 | 10 | once | `Assets/Art/Characters/Knight/Telegraph/Knight_Telegraph_00.png` … `Knight_Telegraph_02.png` |
| Attack_A | 4 | 18 | once | `Assets/Art/Characters/Knight/Attack_A/Knight_Attack_A_00.png` … `Knight_Attack_A_03.png` |
| Attack_B | 4 | 18 | once | `Assets/Art/Characters/Knight/Attack_B/Knight_Attack_B_00.png` … `Knight_Attack_B_03.png` |
| Hurt | 2 | 12 | once | `Assets/Art/Characters/Knight/Hurt/Knight_Hurt_00.png` … `Knight_Hurt_01.png` |
| Death | 8 | 12 | once | `Assets/Art/Characters/Knight/Death/Knight_Death_00.png` … `Knight_Death_07.png` |

**Knight — Idle** (4 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue armoured knight with a tall plumed helmet, kite shield on the back, longsword. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Idle', 4 frames, looping (last frame flows back into the first), each frame 256x256 px, body about 110x190 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: chest rising slightly, shoulders up 2 px. Frame 3: settling back down. Frame 4: shoulders lowest, slight sag.
```

**Knight — Walk** (8 frames @ 10 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue armoured knight with a tall plumed helmet, kite shield on the back, longsword. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Walk', 8 frames, looping (last frame flows back into the first), each frame 256x256 px, body about 110x190 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: right foot forward contact. Frame 2: weight down on right leg. Frame 3: passing pose, left leg swinging. Frame 4: left leg up and forward. Frame 5: left foot forward contact. Frame 6: weight down on left leg. Frame 7: passing pose, right leg swinging. Frame 8: right leg up and forward.
```

**Knight — Telegraph** (3 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue armoured knight with a tall plumed helmet, kite shield on the back, longsword. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Telegraph', 3 frames, plays once, each frame 256x256 px, body about 110x190 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: starting the wind-up, weight shifting back. Frame 2: deeper wind-up, weapon/limb pulled back. Frame 3: full wind-up held, accent glow brightest (clear warning pose).
```

**Knight — Attack_A** (4 frames @ 18 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue armoured knight with a tall plumed helmet, kite shield on the back, longsword. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Attack_A', 4 frames, plays once, each frame 256x256 px, body about 110x190 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: sword drawn back. Frame 2: horizontal slash. Frame 3: sword extended. Frame 4: follow-through.
```

**Knight — Attack_B** (4 frames @ 18 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue armoured knight with a tall plumed helmet, kite shield on the back, longsword. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Attack_B', 4 frames, plays once, each frame 256x256 px, body about 110x190 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: sword raised. Frame 2: overhead downward chop. Frame 3: sword low. Frame 4: recover to guard.
```

**Knight — Hurt** (2 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue armoured knight with a tall plumed helmet, kite shield on the back, longsword. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Hurt', 2 frames, plays once, each frame 256x256 px, body about 110x190 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: recoil: head and torso snapped backwards, limbs flung. Frame 2: stagger: bent backwards, wincing pose.
```

**Knight — Death** (8 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue armoured knight with a tall plumed helmet, kite shield on the back, longsword. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Death', 8 frames, plays once, each frame 256x256 px, body about 110x190 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: knees buckling. Frame 3: falling, body tilting. Frame 4: collapsing to the ground. Frame 5: silhouette cracking into paper fragments. Frame 6: fragments drifting apart. Frame 7: only a few paper scraps left. Frame 8: last faint scraps fading (nearly empty frame).
```

### Bat

- **Who:** Bat — Phase 2 swarm flyer that hangs from ceilings, wakes, circles and dive-bites.
- **Canvas:** 128×128 px · **pivot:** centre · **body on canvas:** about 90×60 px, centred in the canvas (it flies)
- **Folder:** `Assets/Art/Characters/Bat/<Anim>/Bat_<Anim>_NN.png` · **7 animations, 24 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: White-blue cartoon bat with big pointed ears, wide scalloped wings, small fangs, blue eyes. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Canvas 128x128 px, the body about 90x60 px, centred in the canvas (it flies). Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Sleep | 2 | 4 | loop | `Assets/Art/Characters/Bat/Sleep/Bat_Sleep_00.png` … `Bat_Sleep_01.png` |
| Wake | 3 | 12 | once | `Assets/Art/Characters/Bat/Wake/Bat_Wake_00.png` … `Bat_Wake_02.png` |
| Fly | 6 | 14 | loop | `Assets/Art/Characters/Bat/Fly/Bat_Fly_00.png` … `Bat_Fly_05.png` |
| Telegraph | 3 | 10 | once | `Assets/Art/Characters/Bat/Telegraph/Bat_Telegraph_00.png` … `Bat_Telegraph_02.png` |
| Dive | 2 | 12 | loop | `Assets/Art/Characters/Bat/Dive/Bat_Dive_00.png` … `Bat_Dive_01.png` |
| Hurt | 2 | 12 | once | `Assets/Art/Characters/Bat/Hurt/Bat_Hurt_00.png` … `Bat_Hurt_01.png` |
| Death | 6 | 12 | once | `Assets/Art/Characters/Bat/Death/Bat_Death_00.png` … `Bat_Death_05.png` |

**Bat — Sleep** (2 frames @ 4 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue cartoon bat with big pointed ears, wide scalloped wings, small fangs, blue eyes. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Sleep', 2 frames, looping (last frame flows back into the first), each frame 128x128 px, body about 90x60 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: hanging upside down, wings wrapped around the body. Frame 2: same, tiny breathing shift.
```

**Bat — Wake** (3 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue cartoon bat with big pointed ears, wide scalloped wings, small fangs, blue eyes. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Wake', 3 frames, plays once, each frame 128x128 px, body about 90x60 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: eyes opening, still hanging. Frame 2: wings unfolding. Frame 3: dropping and spreading wings.
```

**Bat — Fly** (6 frames @ 14 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue cartoon bat with big pointed ears, wide scalloped wings, small fangs, blue eyes. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Fly', 6 frames, looping (last frame flows back into the first), each frame 128x128 px, body about 90x60 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: wings fully up. Frame 2: wings sweeping down. Frame 3: wings level. Frame 4: wings fully down. Frame 5: wings sweeping up. Frame 6: wings level, body bobbing.
```

**Bat — Telegraph** (3 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue cartoon bat with big pointed ears, wide scalloped wings, small fangs, blue eyes. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Telegraph', 3 frames, plays once, each frame 128x128 px, body about 90x60 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: wings pulled up high. Frame 2: body tucked, fangs bared. Frame 3: aimed downward, eyes bright (dive warning).
```

**Bat — Dive** (2 frames @ 12 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue cartoon bat with big pointed ears, wide scalloped wings, small fangs, blue eyes. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Dive', 2 frames, looping (last frame flows back into the first), each frame 128x128 px, body about 90x60 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: streamlined dive, wings folded, fangs forward. Frame 2: dive, wings slightly open.
```

**Bat — Hurt** (2 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue cartoon bat with big pointed ears, wide scalloped wings, small fangs, blue eyes. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Hurt', 2 frames, plays once, each frame 128x128 px, body about 90x60 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: recoil: head and torso snapped backwards, limbs flung. Frame 2: stagger: bent backwards, wincing pose.
```

**Bat — Death** (6 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: White-blue cartoon bat with big pointed ears, wide scalloped wings, small fangs, blue eyes. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Death', 6 frames, plays once, each frame 128x128 px, body about 90x60 px, centred in the canvas (it flies), identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: knees buckling. Frame 3: falling, body tilting. Frame 4: silhouette cracking into paper fragments. Frame 5: fragments drifting apart. Frame 6: only a few paper scraps left.
```

### Charger

- **Who:** Siege Ram Knight (Charger) — Phase 2 brute that charges across the room and stuns itself on walls.
- **Canvas:** 320×256 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 180×150 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/Charger/<Anim>/Charger_<Anim>_NN.png` · **7 animations, 32 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Bulky low armoured knight, horned helm, a huge battering-ram shoulder plate pointing forward, short thick legs. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Canvas 320x256 px, the body about 180x150 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Idle | 4 | 6 | loop | `Assets/Art/Characters/Charger/Idle/Charger_Idle_00.png` … `Charger_Idle_03.png` |
| Walk | 6 | 8 | loop | `Assets/Art/Characters/Charger/Walk/Charger_Walk_00.png` … `Charger_Walk_05.png` |
| Telegraph | 4 | 8 | once | `Assets/Art/Characters/Charger/Telegraph/Charger_Telegraph_00.png` … `Charger_Telegraph_03.png` |
| Charge | 4 | 14 | loop | `Assets/Art/Characters/Charger/Charge/Charger_Charge_00.png` … `Charger_Charge_03.png` |
| Stunned | 4 | 6 | loop | `Assets/Art/Characters/Charger/Stunned/Charger_Stunned_00.png` … `Charger_Stunned_03.png` |
| Hurt | 2 | 12 | once | `Assets/Art/Characters/Charger/Hurt/Charger_Hurt_00.png` … `Charger_Hurt_01.png` |
| Death | 8 | 12 | once | `Assets/Art/Characters/Charger/Death/Charger_Death_00.png` … `Charger_Death_07.png` |

**Charger — Idle** (4 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Bulky low armoured knight, horned helm, a huge battering-ram shoulder plate pointing forward, short thick legs. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Idle', 4 frames, looping (last frame flows back into the first), each frame 320x256 px, body about 180x150 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: chest rising slightly, shoulders up 2 px. Frame 3: settling back down. Frame 4: shoulders lowest, slight sag.
```

**Charger — Walk** (6 frames @ 8 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Bulky low armoured knight, horned helm, a huge battering-ram shoulder plate pointing forward, short thick legs. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Walk', 6 frames, looping (last frame flows back into the first), each frame 320x256 px, body about 180x150 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: right foot forward contact. Frame 2: weight down on right leg. Frame 3: passing pose, left leg swinging. Frame 4: left foot forward contact. Frame 5: weight down on left leg. Frame 6: passing pose, right leg swinging.
```

**Charger — Telegraph** (4 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Bulky low armoured knight, horned helm, a huge battering-ram shoulder plate pointing forward, short thick legs. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Telegraph', 4 frames, plays once, each frame 320x256 px, body about 180x150 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: head lowered, pawing the ground. Frame 2: pawing again, dust. Frame 3: ram aimed forward. Frame 4: coiled, ready to charge (warning).
```

**Charger — Charge** (4 frames @ 14 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Bulky low armoured knight, horned helm, a huge battering-ram shoulder plate pointing forward, short thick legs. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Charge', 4 frames, looping (last frame flows back into the first), each frame 320x256 px, body about 180x150 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: galloping run, ram forward, leg A. Frame 2: body low, leg B. Frame 3: leg A. Frame 4: leg B.
```

**Charger — Stunned** (4 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Bulky low armoured knight, horned helm, a huge battering-ram shoulder plate pointing forward, short thick legs. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Stunned', 4 frames, looping (last frame flows back into the first), each frame 320x256 px, body about 180x150 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: dazed, head wobbling left, stars. Frame 2: head wobbling right. Frame 3: slumped. Frame 4: wobbling.
```

**Charger — Hurt** (2 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Bulky low armoured knight, horned helm, a huge battering-ram shoulder plate pointing forward, short thick legs. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Hurt', 2 frames, plays once, each frame 320x256 px, body about 180x150 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: recoil: head and torso snapped backwards, limbs flung. Frame 2: stagger: bent backwards, wincing pose.
```

**Charger — Death** (8 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Bulky low armoured knight, horned helm, a huge battering-ram shoulder plate pointing forward, short thick legs. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Death', 8 frames, plays once, each frame 320x256 px, body about 180x150 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: knees buckling. Frame 3: falling, body tilting. Frame 4: collapsing to the ground. Frame 5: silhouette cracking into paper fragments. Frame 6: fragments drifting apart. Frame 7: only a few paper scraps left. Frame 8: last faint scraps fading (nearly empty frame).
```

### Thrower

- **Who:** Thrower — Phase 2 ranged enemy that keeps distance and throws arcing knives.
- **Canvas:** 256×256 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 100×180 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/Thrower/<Anim>/Thrower_<Anim>_NN.png` · **6 animations, 26 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Lean white-blue hooded figure, long coat, a bandolier / bundle of knives across the chest, one knife ready in hand. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Canvas 256x256 px, the body about 100x180 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Idle | 4 | 6 | loop | `Assets/Art/Characters/Thrower/Idle/Thrower_Idle_00.png` … `Thrower_Idle_03.png` |
| Walk | 6 | 10 | loop | `Assets/Art/Characters/Thrower/Walk/Thrower_Walk_00.png` … `Thrower_Walk_05.png` |
| Throw_Telegraph | 3 | 10 | once | `Assets/Art/Characters/Thrower/Throw_Telegraph/Thrower_Throw_Telegraph_00.png` … `Thrower_Throw_Telegraph_02.png` |
| Throw | 3 | 16 | once | `Assets/Art/Characters/Thrower/Throw/Thrower_Throw_00.png` … `Thrower_Throw_02.png` |
| Hurt | 2 | 12 | once | `Assets/Art/Characters/Thrower/Hurt/Thrower_Hurt_00.png` … `Thrower_Hurt_01.png` |
| Death | 8 | 12 | once | `Assets/Art/Characters/Thrower/Death/Thrower_Death_00.png` … `Thrower_Death_07.png` |

**Thrower — Idle** (4 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Lean white-blue hooded figure, long coat, a bandolier / bundle of knives across the chest, one knife ready in hand. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Idle', 4 frames, looping (last frame flows back into the first), each frame 256x256 px, body about 100x180 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: chest rising slightly, shoulders up 2 px. Frame 3: settling back down. Frame 4: shoulders lowest, slight sag.
```

**Thrower — Walk** (6 frames @ 10 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Lean white-blue hooded figure, long coat, a bandolier / bundle of knives across the chest, one knife ready in hand. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Walk', 6 frames, looping (last frame flows back into the first), each frame 256x256 px, body about 100x180 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: right foot forward contact. Frame 2: weight down on right leg. Frame 3: passing pose, left leg swinging. Frame 4: left foot forward contact. Frame 5: weight down on left leg. Frame 6: passing pose, right leg swinging.
```

**Thrower — Throw_Telegraph** (3 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Lean white-blue hooded figure, long coat, a bandolier / bundle of knives across the chest, one knife ready in hand. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Throw_Telegraph', 3 frames, plays once, each frame 256x256 px, body about 100x180 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: drawing a knife from the bandolier. Frame 2: arm cocked back. Frame 3: arm fully back, knife glinting (warning).
```

**Thrower — Throw** (3 frames @ 16 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Lean white-blue hooded figure, long coat, a bandolier / bundle of knives across the chest, one knife ready in hand. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Throw', 3 frames, plays once, each frame 256x256 px, body about 100x180 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: arm whipping forward. Frame 2: knife released. Frame 3: follow-through.
```

**Thrower — Hurt** (2 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Lean white-blue hooded figure, long coat, a bandolier / bundle of knives across the chest, one knife ready in hand. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Hurt', 2 frames, plays once, each frame 256x256 px, body about 100x180 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: recoil: head and torso snapped backwards, limbs flung. Frame 2: stagger: bent backwards, wincing pose.
```

**Thrower — Death** (8 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Lean white-blue hooded figure, long coat, a bandolier / bundle of knives across the chest, one knife ready in hand. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Death', 8 frames, plays once, each frame 256x256 px, body about 100x180 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: knees buckling. Frame 3: falling, body tilting. Frame 4: collapsing to the ground. Frame 5: silhouette cracking into paper fragments. Frame 6: fragments drifting apart. Frame 7: only a few paper scraps left. Frame 8: last faint scraps fading (nearly empty frame).
```

### IronColossus

- **Who:** Iron Colossus — Phase 2 mini-boss: slow tanky brute with a huge sword and a tower shield.
- **Canvas:** 512×512 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 220×260 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/IronColossus/<Anim>/IronColossus_<Anim>_NN.png` · **15 animations, 66 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Canvas 512x512 px, the body about 220x260 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Intro | 6 | 8 | once | `Assets/Art/Characters/IronColossus/Intro/IronColossus_Intro_00.png` … `IronColossus_Intro_05.png` |
| Idle | 6 | 6 | loop | `Assets/Art/Characters/IronColossus/Idle/IronColossus_Idle_00.png` … `IronColossus_Idle_05.png` |
| Walk | 8 | 8 | loop | `Assets/Art/Characters/IronColossus/Walk/IronColossus_Walk_00.png` … `IronColossus_Walk_07.png` |
| Slash_Telegraph | 4 | 8 | once | `Assets/Art/Characters/IronColossus/Slash_Telegraph/IronColossus_Slash_Telegraph_00.png` … `IronColossus_Slash_Telegraph_03.png` |
| Slash | 5 | 16 | once | `Assets/Art/Characters/IronColossus/Slash/IronColossus_Slash_00.png` … `IronColossus_Slash_04.png` |
| Block_Start | 3 | 12 | once | `Assets/Art/Characters/IronColossus/Block_Start/IronColossus_Block_Start_00.png` … `IronColossus_Block_Start_02.png` |
| Block_Hold | 2 | 6 | loop | `Assets/Art/Characters/IronColossus/Block_Hold/IronColossus_Block_Hold_00.png` … `IronColossus_Block_Hold_01.png` |
| Block_Impact | 2 | 16 | once | `Assets/Art/Characters/IronColossus/Block_Impact/IronColossus_Block_Impact_00.png` … `IronColossus_Block_Impact_01.png` |
| Bash | 3 | 16 | once | `Assets/Art/Characters/IronColossus/Bash/IronColossus_Bash_00.png` … `IronColossus_Bash_02.png` |
| Jump_Telegraph | 3 | 10 | once | `Assets/Art/Characters/IronColossus/Jump_Telegraph/IronColossus_Jump_Telegraph_00.png` … `IronColossus_Jump_Telegraph_02.png` |
| Jump_Air | 2 | 6 | loop | `Assets/Art/Characters/IronColossus/Jump_Air/IronColossus_Jump_Air_00.png` … `IronColossus_Jump_Air_01.png` |
| Jump_Land | 4 | 14 | once | `Assets/Art/Characters/IronColossus/Jump_Land/IronColossus_Jump_Land_00.png` … `IronColossus_Jump_Land_03.png` |
| Dash_Telegraph | 3 | 10 | once | `Assets/Art/Characters/IronColossus/Dash_Telegraph/IronColossus_Dash_Telegraph_00.png` … `IronColossus_Dash_Telegraph_02.png` |
| Dash | 3 | 16 | loop | `Assets/Art/Characters/IronColossus/Dash/IronColossus_Dash_00.png` … `IronColossus_Dash_02.png` |
| Death | 12 | 10 | once | `Assets/Art/Characters/IronColossus/Death/IronColossus_Death_00.png` … `IronColossus_Death_11.png` |

**IronColossus — Intro** (6 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Intro', 6 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: standing tall, head down. Frame 2: head rising. Frame 3: weapon drawn / arms spreading. Frame 4: menacing pose. Frame 5: accent glow flares. Frame 6: battle stance.
```

**IronColossus — Idle** (6 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Idle', 6 frames, looping (last frame flows back into the first), each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: chest rising slightly, shoulders up 2 px. Frame 3: chest at top of breath. Frame 4: settling back down. Frame 5: shoulders lowest, slight sag. Frame 6: rising again toward neutral.
```

**IronColossus — Walk** (8 frames @ 8 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Walk', 8 frames, looping (last frame flows back into the first), each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: right foot forward contact. Frame 2: weight down on right leg. Frame 3: passing pose, left leg swinging. Frame 4: left leg up and forward. Frame 5: left foot forward contact. Frame 6: weight down on left leg. Frame 7: passing pose, right leg swinging. Frame 8: right leg up and forward.
```

**IronColossus — Slash_Telegraph** (4 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Slash_Telegraph', 4 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: greatsword lifting. Frame 2: greatsword raised high behind. Frame 3: sword overhead. Frame 4: held at the top (warning).
```

**IronColossus — Slash** (5 frames @ 16 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Slash', 5 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: starting the huge swing. Frame 2: wide horizontal arc. Frame 3: sword fully extended. Frame 4: momentum carrying through. Frame 5: recovering slowly.
```

**IronColossus — Block_Start** (3 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Block_Start', 3 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: raising the tower shield. Frame 2: shield moving in front. Frame 3: shield planted in front.
```

**IronColossus — Block_Hold** (2 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Block_Hold', 2 frames, looping (last frame flows back into the first), each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: crouched behind the tower shield. Frame 2: same, braced.
```

**IronColossus — Block_Impact** (2 frames @ 16 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Block_Impact', 2 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: shield jolted by a hit. Frame 2: recovering behind the shield.
```

**IronColossus — Bash** (3 frames @ 16 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Bash', 3 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: shield pulled back. Frame 2: shield slammed forward. Frame 3: shield extended.
```

**IronColossus — Jump_Telegraph** (3 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Jump_Telegraph', 3 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: crouching low. Frame 2: deeper crouch, sword raised. Frame 3: coiled to leap (warning).
```

**IronColossus — Jump_Air** (2 frames @ 6 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Jump_Air', 2 frames, looping (last frame flows back into the first), each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: airborne, sword raised overhead, legs tucked. Frame 2: airborne, coming down.
```

**IronColossus — Jump_Land** (4 frames @ 14 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Jump_Land', 4 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: slamming down, sword hitting the ground. Frame 2: impact crouch. Frame 3: rising. Frame 4: standing.
```

**IronColossus — Dash_Telegraph** (3 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Dash_Telegraph', 3 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: shoulder lowered behind the shield. Frame 2: leaning forward. Frame 3: about to charge (warning).
```

**IronColossus — Dash** (3 frames @ 16 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Dash', 3 frames, looping (last frame flows back into the first), each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: charging forward behind the shield, leg A. Frame 2: leg B. Frame 3: leg A.
```

**IronColossus — Death** (12 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Stocky giant in white-blue plate armour, small visored helm, enormous greatsword in the right hand, tall tower shield on the left arm. Colours: white #F2F8FF body, light blue #6EC1FF details, deep blue #2C6FD1 shading/accents. Animation 'Death', 12 frames, plays once, each frame 512x512 px, body about 220x260 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: hit hard, recoiling backwards. Frame 2: hit hard, recoiling backwards. Frame 3: knees buckling. Frame 4: falling, body tilting. Frame 5: falling, body tilting. Frame 6: collapsing to the ground. Frame 7: silhouette cracking into paper fragments. Frame 8: silhouette cracking into paper fragments. Frame 9: fragments drifting apart. Frame 10: only a few paper scraps left. Frame 11: only a few paper scraps left. Frame 12: last faint scraps fading (nearly empty frame).
```

### Zulmat

- **Who:** Zulmat, the Severed — final boss; the protagonist's cruel other half (Aku-like evil lord).
- **Canvas:** 512×512 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin) · **body on canvas:** about 200×320 px, centred horizontally, feet 10 px above the bottom edge
- **Folder:** `Assets/Art/Characters/Zulmat/<Anim>/Zulmat_<Anim>_NN.png` · **17 animations, 96 frames**

**Character reference prompt**

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Character: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Canvas 512x512 px, the body about 200x320 px, centred horizontally, feet 10 px above the bottom edge. Character reference sheet, neutral standing pose.
```

| Animation | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Intro | 8 | 8 | once | `Assets/Art/Characters/Zulmat/Intro/Zulmat_Intro_00.png` … `Zulmat_Intro_07.png` |
| Idle | 8 | 8 | loop | `Assets/Art/Characters/Zulmat/Idle/Zulmat_Idle_00.png` … `Zulmat_Idle_07.png` |
| Walk | 8 | 10 | loop | `Assets/Art/Characters/Zulmat/Walk/Zulmat_Walk_00.png` … `Zulmat_Walk_07.png` |
| Slash_Telegraph | 3 | 10 | once | `Assets/Art/Characters/Zulmat/Slash_Telegraph/Zulmat_Slash_Telegraph_00.png` … `Zulmat_Slash_Telegraph_02.png` |
| Slash_A | 4 | 18 | once | `Assets/Art/Characters/Zulmat/Slash_A/Zulmat_Slash_A_00.png` … `Zulmat_Slash_A_03.png` |
| Slash_B | 4 | 18 | once | `Assets/Art/Characters/Zulmat/Slash_B/Zulmat_Slash_B_00.png` … `Zulmat_Slash_B_03.png` |
| Vine_Cast | 6 | 12 | once | `Assets/Art/Characters/Zulmat/Vine_Cast/Zulmat_Vine_Cast_00.png` … `Zulmat_Vine_Cast_05.png` |
| Summon_Cast | 6 | 10 | once | `Assets/Art/Characters/Zulmat/Summon_Cast/Zulmat_Summon_Cast_00.png` … `Zulmat_Summon_Cast_05.png` |
| Shrink | 4 | 14 | once | `Assets/Art/Characters/Zulmat/Shrink/Zulmat_Shrink_00.png` … `Zulmat_Shrink_03.png` |
| Grow | 4 | 14 | once | `Assets/Art/Characters/Zulmat/Grow/Zulmat_Grow_00.png` … `Zulmat_Grow_03.png` |
| Lunge_Telegraph | 3 | 10 | once | `Assets/Art/Characters/Zulmat/Lunge_Telegraph/Zulmat_Lunge_Telegraph_00.png` … `Zulmat_Lunge_Telegraph_02.png` |
| Lunge | 3 | 16 | loop | `Assets/Art/Characters/Zulmat/Lunge/Zulmat_Lunge_00.png` … `Zulmat_Lunge_02.png` |
| Swipe_Telegraph | 4 | 8 | once | `Assets/Art/Characters/Zulmat/Swipe_Telegraph/Zulmat_Swipe_Telegraph_00.png` … `Zulmat_Swipe_Telegraph_03.png` |
| Swipe | 5 | 16 | once | `Assets/Art/Characters/Zulmat/Swipe/Zulmat_Swipe_00.png` … `Zulmat_Swipe_04.png` |
| Lava_Cast | 6 | 10 | once | `Assets/Art/Characters/Zulmat/Lava_Cast/Zulmat_Lava_Cast_00.png` … `Zulmat_Lava_Cast_05.png` |
| Roar | 6 | 10 | once | `Assets/Art/Characters/Zulmat/Roar/Zulmat_Roar_00.png` … `Zulmat_Roar_05.png` |
| Death | 14 | 8 | once | `Assets/Art/Characters/Zulmat/Death/Zulmat_Death_00.png` … `Zulmat_Death_13.png` |

**Zulmat — Intro** (8 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Intro', 8 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: standing tall, head down. Frame 2: standing tall, head down. Frame 3: head rising. Frame 4: weapon drawn / arms spreading. Frame 5: menacing pose. Frame 6: menacing pose. Frame 7: accent glow flares. Frame 8: battle stance.
```

**Zulmat — Idle** (8 frames @ 8 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Idle', 8 frames, looping (last frame flows back into the first), each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: neutral stance, chest at rest. Frame 2: neutral stance, chest at rest. Frame 3: chest rising slightly, shoulders up 2 px. Frame 4: chest at top of breath. Frame 5: settling back down. Frame 6: settling back down. Frame 7: shoulders lowest, slight sag. Frame 8: rising again toward neutral.
```

**Zulmat — Walk** (8 frames @ 10 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Walk', 8 frames, looping (last frame flows back into the first), each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: right foot forward contact. Frame 2: weight down on right leg. Frame 3: passing pose, left leg swinging. Frame 4: left leg up and forward. Frame 5: left foot forward contact. Frame 6: weight down on left leg. Frame 7: passing pose, right leg swinging. Frame 8: right leg up and forward.
```

**Zulmat — Slash_Telegraph** (3 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Slash_Telegraph', 3 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: starting the wind-up, weight shifting back. Frame 2: deeper wind-up, weapon/limb pulled back. Frame 3: full wind-up held, accent glow brightest (clear warning pose).
```

**Zulmat — Slash_A** (4 frames @ 18 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Slash_A', 4 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: weapon raised behind, anticipation. Frame 2: fast swing, motion arc. Frame 3: impact: weapon fully extended forward. Frame 4: follow-through, recovering.
```

**Zulmat — Slash_B** (4 frames @ 18 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Slash_B', 4 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: weapon raised behind, anticipation. Frame 2: fast swing, motion arc. Frame 3: impact: weapon fully extended forward. Frame 4: follow-through, recovering.
```

**Zulmat — Vine_Cast** (6 frames @ 12 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Vine_Cast', 6 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: clawed hand raised. Frame 2: hand slamming toward the ground. Frame 3: palm on the ground, red cracks. Frame 4: cracks glowing. Frame 5: rising. Frame 6: recover.
```

**Zulmat — Summon_Cast** (6 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Summon_Cast', 6 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: arms raised. Frame 2: dark energy gathering. Frame 3: energy swirling. Frame 4: energy bursting out to the sides. Frame 5: arms lowering. Frame 6: recover.
```

**Zulmat — Shrink** (4 frames @ 14 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Shrink', 4 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: (keep full size — the game scales him down in code) starting to curl inward, robe pulling in. Frame 2: hunching, arms wrapping the body. Frame 3: curled tight, robe wrapped around him, flames flaring. Frame 4: fully curled ball-like pose.
```

**Zulmat — Grow** (4 frames @ 14 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Grow', 4 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: (keep full size — the game scales him up in code) curled pose starting to unfurl. Frame 2: rising, arms opening. Frame 3: standing, arms spreading wide, flames flaring. Frame 4: towering pose, arms spread, head high.
```

**Zulmat — Lunge_Telegraph** (3 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Lunge_Telegraph', 3 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: crouched low, claws forward. Frame 2: coiled. Frame 3: eyes flaring (warning).
```

**Zulmat — Lunge** (3 frames @ 16 fps, looping (last frame flows back into the first))

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Lunge', 3 frames, looping (last frame flows back into the first), each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: streaking forward horizontally, claws first. Frame 2: stretched lunge, robe trailing. Frame 3: lunge.
```

**Zulmat — Swipe_Telegraph** (4 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Swipe_Telegraph', 4 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: huge arm pulled back. Frame 2: arm further back. Frame 3: arm fully back, claws glowing. Frame 4: held (warning).
```

**Zulmat — Swipe** (5 frames @ 16 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Swipe', 5 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: arm sweeping forward low. Frame 2: horizontal sweep across the ground. Frame 3: arm fully extended. Frame 4: follow-through. Frame 5: recover.
```

**Zulmat — Lava_Cast** (6 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Lava_Cast', 6 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: fists raised. Frame 2: fists slammed down. Frame 3: ground glowing red. Frame 4: lava light rising. Frame 5: stepping back. Frame 6: recover.
```

**Zulmat — Roar** (6 frames @ 10 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Roar', 6 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: head thrown back. Frame 2: roaring, mouth wide. Frame 3: roaring, arms spread. Frame 4: roar peak, flames flaring from the hair. Frame 5: roar fading. Frame 6: recover.
```

**Zulmat — Death** (14 frames @ 8 fps, plays once)

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Same character as the reference: Tall imposing demon-lord silhouette: flame-like swept-back hair / horns, long sharp jaw, glowing slit eyes, flowing robe that frays into flames at the hem, long clawed fingers, a long dark blade. Menacing, theatrical, mirror-image of the hero's proportions but taller. Colours: deep black #0A0305 body with a bright crimson rim light #FF2A3A and crimson eyes. Animation 'Death', 14 frames, plays once, each frame 512x512 px, body about 200x320 px, centred horizontally, feet 10 px above the bottom edge, identical scale and position in every frame. Frame 1: struck, recoiling. Frame 2: staggering. Frame 3: falling to one knee. Frame 4: kneeling, head bowed. Frame 5: kneeling, cracks of light. Frame 6: cracks spreading. Frame 7: light bursting through. Frame 8: breaking apart. Frame 9: fragments. Frame 10: fragments drifting. Frame 11: smoke. Frame 12: fading smoke. Frame 13: nearly gone. Frame 14: last wisps.
```

## 2. FX & projectiles

All FX: transparent background, pivot as listed, drawn so they look right when the game tints/scales them.

### Slash_Side

- **Canvas:** 320×256 px · **pivot:** leftCenter
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 3 | 24 | once | `Assets/Art/FX/Slash_Side/Slash_Side_00.png` … `Slash_Side_02.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. The protagonist's horizontal nail slash: a white-blue crescent arc sweeping left-to-right, thick at the start and tapering, with a bright edge. Frames: arc appearing, full crescent, crescent fading. Each frame 320x256 px, horizontal strip of frames.
```

### Slash_Up

- **Canvas:** 256×320 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin)
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 3 | 24 | once | `Assets/Art/FX/Slash_Up/Slash_Up_00.png` … `Slash_Up_02.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. The protagonist's upward slash: a white-blue crescent arc sweeping upward from the bottom of the canvas. Frames: appearing, full, fading. Each frame 256x320 px, horizontal strip of frames.
```

### Fireball_Fly

- **Canvas:** 192×128 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 14 | loop | `Assets/Art/FX/Fireball_Fly/Fireball_Fly_00.png` … `Fireball_Fly_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A blue-white spirit flame projectile flying right: round head with a flickering tail streaming left. Loop of 4 flickering frames. Each frame 192x128 px, horizontal strip of frames.
```

### Fireball_Impact

- **Canvas:** 192×192 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 5 | 16 | once | `Assets/Art/FX/Fireball_Impact/Fireball_Impact_00.png` … `Fireball_Impact_04.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. The fireball bursting: blue-white flame explosion expanding into sparks and smoke, then fading (5 frames). Each frame 192x192 px, horizontal strip of frames.
```

### SoulOrb

- **Canvas:** 48×48 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 10 | loop | `Assets/Art/FX/SoulOrb/SoulOrb_00.png` … `SoulOrb_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A small dark orb with a blue rim glow (soul gained from a hit), gently pulsing loop. Each frame 48x48 px, horizontal strip of frames.
```

### Heal_Aura

- **Canvas:** 256×256 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 6 | 10 | loop | `Assets/Art/FX/Heal_Aura/Heal_Aura_00.png` … `Heal_Aura_05.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A soft blue-white healing aura ring around a character (the character is NOT drawn): rising light motes, pulsing loop. Each frame 256x256 px, horizontal strip of frames.
```

### Hit_Spark

- **Canvas:** 128×128 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 20 | once | `Assets/Art/FX/Hit_Spark/Hit_Spark_00.png` … `Hit_Spark_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A white impact spark/starburst (tinted in code): small flash expanding into 4–6 shards, then gone. Each frame 128x128 px, horizontal strip of frames.
```

### Block_Clink

- **Canvas:** 128×128 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 3 | 20 | once | `Assets/Art/FX/Block_Clink/Block_Clink_00.png` … `Block_Clink_02.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A white-gold metal clink spark: a bright starburst with short lines, quick. Each frame 128x128 px, horizontal strip of frames.
```

### BlackSlash_Fly

- **Canvas:** 192×96 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 3 | 12 | loop | `Assets/Art/FX/BlackSlash_Fly/BlackSlash_Fly_00.png` … `BlackSlash_Fly_02.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. Kage Caller's projectile: a black crescent blade of energy with a red edge, flying right, flickering loop. Each frame 192x96 px, horizontal strip of frames.
```

### BlackSlash_Break

- **Canvas:** 192×96 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 16 | once | `Assets/Art/FX/BlackSlash_Break/BlackSlash_Break_00.png` … `BlackSlash_Break_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. The black crescent shattering into black and red shards (4 frames). Each frame 192x96 px, horizontal strip of frames.
```

### Exclamation

- **Canvas:** 64×64 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 12 | loop | `Assets/Art/FX/Exclamation/Exclamation_00.png` … `Exclamation_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A red '!' warning marker in cut-paper style above enemies, bouncing/pulsing loop. Each frame 64x64 px, horizontal strip of frames.
```

### Knife_Fly

- **Canvas:** 128×32 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 16 | loop | `Assets/Art/FX/Knife_Fly/Knife_Fly_00.png` … `Knife_Fly_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A thrown white-blue knife pointing right, slight spin blur, loop. Each frame 128x32 px, horizontal strip of frames.
```

### Vine_Warn

- **Canvas:** 192×64 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin)
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 10 | loop | `Assets/Art/FX/Vine_Warn/Vine_Warn_00.png` … `Vine_Warn_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A glowing red crack/glow on the ground (pivot bottom): where a vine will erupt. Pulsing loop. Each frame 192x64 px, horizontal strip of frames.
```

### Vine

- **Canvas:** 128×384 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin)
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Grow | 6 | 16 | once | `Assets/Art/FX/Vine/Vine_Grow_00.png` … `Vine_Grow_05.png` |
| Retract | 3 | 16 | once | `Assets/Art/FX/Vine/Vine_Retract_00.png` … `Vine_Retract_02.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. Zulmat's thorned vine bursting up from the ground (pivot bottom): black thorny tendril with crimson glow. Grow: sprouting from the ground to full height (6 frames). Retract: sinking back down (3 frames). Each frame 128x384 px, horizontal strip of frames.
```

### Lava_Zone

- **Canvas:** 100×50 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 6 | loop | `Assets/Art/FX/Lava_Zone/Lava_Zone_00.png` … `Lava_Zone_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A horizontally tileable strip of glowing red-orange lava with a bubbling surface, 4-frame loop. Left and right edges must tile seamlessly. Each frame 100x50 px, horizontal strip of frames.
```

### Giant_Swipe

- **Canvas:** 768×512 px · **pivot:** leftCenter
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 5 | 16 | once | `Assets/Art/FX/Giant_Swipe/Giant_Swipe_00.png` … `Giant_Swipe_04.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. Zulmat's giant swipe: a huge horizontal crimson claw-slash arc covering the ground in front (pivot left-centre), sweeping left-to-right over 5 frames. Each frame 768x512 px, horizontal strip of frames.
```

### Shockwave

- **Canvas:** 256×64 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 16 | once | `Assets/Art/FX/Shockwave/Shockwave_00.png` … `Shockwave_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. Iron Colossus' landing shockwave: a flat ground wave of white-blue energy and dust spreading outward to both sides (4 frames). Each frame 256x64 px, horizontal strip of frames.
```

### Shield_Arc

- **Canvas:** 256×384 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 2 | 6 | loop | `Assets/Art/FX/Shield_Arc/Shield_Arc_00.png` … `Shield_Arc_01.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A translucent white-blue shield arc / barrier curve (the Colossus' raised tower shield effect), shimmering 2-frame loop. Each frame 256x384 px, horizontal strip of frames.
```

### Dust_Puff

- **Canvas:** 96×96 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 4 | 14 | once | `Assets/Art/FX/Dust_Puff/Dust_Puff_00.png` … `Dust_Puff_03.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. A small grey dust puff (landing, charging), expanding and fading in 4 frames. Each frame 96x96 px, horizontal strip of frames.
```

### Death_Smoke

- **Canvas:** 192×192 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single clip) | 6 | 14 | once | `Assets/Art/FX/Death_Smoke/Death_Smoke_00.png` … `Death_Smoke_05.png` |

```
2D game visual effect sprite, shadow-puppet theatre style, flat cut-paper shapes with soft inner glow, crisp edges, centred on the canvas, fully transparent background (PNG with alpha), no text, no border. Enemy death poof: dark smoke burst with paper scraps, expanding then dissipating (6 frames). Each frame 192x192 px, horizontal strip of frames.
```

## 3. Props

### Bench

- **Canvas:** 192×192 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin)
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Unlit | 1 | 1 | once | `Assets/Art/Props/Bench/Bench_Unlit_00.png` |
| Lit | 4 | 8 | loop | `Assets/Art/Props/Bench/Bench_Lit_00.png` … `Bench_Lit_03.png` |
| Activate | 4 | 10 | once | `Assets/Art/Props/Bench/Bench_Activate_00.png` … `Bench_Activate_03.png` |

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, prop object, transparent background. The Candle Shrine (save bench): a small stone shrine with a stone seat and a single candle on top. Unlit: candle out. Lit: candle burning (4-frame flicker loop). Activate: candle igniting with a burst of light (4 frames). Canvas 192x192 px, every frame the same size and position.
```

### Door

- **Canvas:** 128×384 px · **pivot:** bottomLeft
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Closed | 1 | 1 | once | `Assets/Art/Props/Door/Door_Closed_00.png` |
| Open | 6 | 12 | once | `Assets/Art/Props/Door/Door_Open_00.png` … `Door_Open_05.png` |

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, prop object, transparent background. A tall locked dungeon door (pivot bottom-LEFT, 128×384): heavy wooden/iron door in a stone frame with a red seal. Closed: 1 frame. Open: the door sliding up or swinging open over 6 frames. Canvas 128x384 px, every frame the same size and position.
```

### Gate

- **Canvas:** 100×300 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin)
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Open | 1 | 1 | once | `Assets/Art/Props/Gate/Gate_Open_00.png` |
| Close | 5 | 14 | once | `Assets/Art/Props/Gate/Gate_Close_00.png` … `Gate_Close_04.png` |

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, prop object, transparent background. A boss-room portcullis gate (100×300, pivot bottom): iron bars. Open: bars raised (out of view / at the very top). Close: bars dropping down over 5 frames until fully closed. Canvas 100x300 px, every frame the same size and position.
```

### Orb

- **Canvas:** 128×128 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| Idle | 6 | 8 | loop | `Assets/Art/Props/Orb/Orb_Idle_00.png` … `Orb_Idle_05.png` |
| Touch | 6 | 12 | once | `Assets/Art/Props/Orb/Orb_Touch_00.png` … `Orb_Touch_05.png` |

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, prop object, transparent background. The memory orb on its pedestal: a glowing pale orb floating, with swirling light inside. Idle: gentle pulsing loop. Touch: flaring brightly and bursting with light. Canvas 128x128 px, every frame the same size and position.
```

### TutorialSign

- **Canvas:** 128×128 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin)
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single) | 1 | 1 | once | `Assets/Art/Props/TutorialSign/TutorialSign_00.png` |

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, prop object, transparent background. A small wooden signpost with a blank board (text is added in code). Canvas 128x128 px, every frame the same size and position.
```

### Torch

- **Canvas:** 96×192 px · **pivot:** bottom-centre (feet on the bottom edge, ~10 px margin)
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single) | 4 | 8 | loop | `Assets/Art/Props/Torch/Torch_00.png` … `Torch_03.png` |

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, prop object, transparent background. A castle wall torch (pivot bottom): iron sconce with a burning flame, 4-frame flicker loop. Canvas 96x192 px, every frame the same size and position.
```

### Chandelier

- **Canvas:** 300×150 px · **pivot:** centre
| Clip | Frames | FPS | Loop | Files |
|---|---|---|---|---|
| (single) | 1 | 1 | once | `Assets/Art/Props/Chandelier/Chandelier_00.png` |

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, prop object, transparent background. A wide iron chandelier platform with candles (300×150), seen from the side, flat top the player can stand on. Canvas 300x150 px, every frame the same size and position.
```

## 4. Tiles

50×50 px each, pivot centre, **seamless** (neighbouring tiles must join without visible seams). The level builder picks one of 16 tiles per cell by which neighbours are solid (index = top·1 + right·2 + bottom·4 + left·8). Floors (exposed top) get a slightly lighter lip so they read. Tip: draw one 200×200 tileable rock/stone patch first, then cut the 16 edge variants from it.

### Cave tiles

Material: cave rock: dark grey stone #26262A–#4A4A52 with a muted red accent #6B3A3A in cracks, rough rounded edges.

| File | Shape |
|---|---|
| `Assets/Art/Tiles/Cave/Tile_Cave_00.png` | exposed edge on the top, right, bottom, left sides (an isolated single block); the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Cave/Tile_Cave_01.png` | exposed edge on the right, bottom, left sides; continues seamlessly into neighbouring tiles on the top |
| `Assets/Art/Tiles/Cave/Tile_Cave_02.png` | exposed edge on the top, bottom, left sides; continues seamlessly into neighbouring tiles on the right; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Cave/Tile_Cave_03.png` | exposed edge on the bottom, left sides; continues seamlessly into neighbouring tiles on the top, right |
| `Assets/Art/Tiles/Cave/Tile_Cave_04.png` | exposed edge on the top, right, left sides; continues seamlessly into neighbouring tiles on the bottom; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Cave/Tile_Cave_05.png` | exposed edge on the right, left sides; continues seamlessly into neighbouring tiles on the top, bottom |
| `Assets/Art/Tiles/Cave/Tile_Cave_06.png` | exposed edge on the top, left sides; continues seamlessly into neighbouring tiles on the right, bottom; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Cave/Tile_Cave_07.png` | exposed edge on the left side; continues seamlessly into neighbouring tiles on the top, right, bottom |
| `Assets/Art/Tiles/Cave/Tile_Cave_08.png` | exposed edge on the top, right, bottom sides; continues seamlessly into neighbouring tiles on the left; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Cave/Tile_Cave_09.png` | exposed edge on the right, bottom sides; continues seamlessly into neighbouring tiles on the top, left |
| `Assets/Art/Tiles/Cave/Tile_Cave_10.png` | exposed edge on the top, bottom sides; continues seamlessly into neighbouring tiles on the right, left; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Cave/Tile_Cave_11.png` | exposed edge on the bottom side; continues seamlessly into neighbouring tiles on the top, right, left |
| `Assets/Art/Tiles/Cave/Tile_Cave_12.png` | exposed edge on the top, right sides; continues seamlessly into neighbouring tiles on the bottom, left; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Cave/Tile_Cave_13.png` | exposed edge on the right side; continues seamlessly into neighbouring tiles on the top, bottom, left |
| `Assets/Art/Tiles/Cave/Tile_Cave_14.png` | exposed edge on the top side; continues seamlessly into neighbouring tiles on the right, bottom, left; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Cave/Tile_Cave_15.png` | fully surrounded interior fill (no visible edges; must tile seamlessly on all four sides) |
| `Assets/Art/Tiles/Cave/Tile_Cave_OneWay_L.png` | one-way platform LEFT end cap (thin plank / ledge in the top third of the tile, the rest transparent) |
| `Assets/Art/Tiles/Cave/Tile_Cave_OneWay_M.png` | one-way platform MIDDLE section (thin plank in the top third, tiles left-right seamlessly) |
| `Assets/Art/Tiles/Cave/Tile_Cave_OneWay_R.png` | one-way platform RIGHT end cap |
| `Assets/Art/Tiles/Cave/Tile_Cave_Spikes.png` | a row of sharp spikes pointing up from the bottom of the tile, transparent above |
| `Assets/Art/Tiles/Cave/Decor_Cave_00.png` | 100×100 decoration with no collision (cave: hanging roots, crystals, bones, moss; castle: banner, chain, cracked brick, window) |
| `Assets/Art/Tiles/Cave/Decor_Cave_01.png` | 100×100 decoration with no collision (cave: hanging roots, crystals, bones, moss; castle: banner, chain, cracked brick, window) |
| `Assets/Art/Tiles/Cave/Decor_Cave_02.png` | 100×100 decoration with no collision (cave: hanging roots, crystals, bones, moss; castle: banner, chain, cracked brick, window) |
| `Assets/Art/Tiles/Cave/Decor_Cave_03.png` | 100×100 decoration with no collision (cave: hanging roots, crystals, bones, moss; castle: banner, chain, cracked brick, window) |

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. Seamless cave rock: dark grey stone #26262A–#4A4A52 with a muted red accent #6B3A3A in cracks, rough rounded edges tile set for a side-view platformer, 50x50 px tiles, flat silhouette style with a lighter top lip on floor surfaces.
```

### Castle tiles

Material: castle masonry: pale stone blocks #D8DEE9 with mid-grey mortar #A7B0C4 and blue-grey accent #5E86A8, clean cut edges.

| File | Shape |
|---|---|
| `Assets/Art/Tiles/Castle/Tile_Castle_00.png` | exposed edge on the top, right, bottom, left sides (an isolated single block); the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Castle/Tile_Castle_01.png` | exposed edge on the right, bottom, left sides; continues seamlessly into neighbouring tiles on the top |
| `Assets/Art/Tiles/Castle/Tile_Castle_02.png` | exposed edge on the top, bottom, left sides; continues seamlessly into neighbouring tiles on the right; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Castle/Tile_Castle_03.png` | exposed edge on the bottom, left sides; continues seamlessly into neighbouring tiles on the top, right |
| `Assets/Art/Tiles/Castle/Tile_Castle_04.png` | exposed edge on the top, right, left sides; continues seamlessly into neighbouring tiles on the bottom; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Castle/Tile_Castle_05.png` | exposed edge on the right, left sides; continues seamlessly into neighbouring tiles on the top, bottom |
| `Assets/Art/Tiles/Castle/Tile_Castle_06.png` | exposed edge on the top, left sides; continues seamlessly into neighbouring tiles on the right, bottom; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Castle/Tile_Castle_07.png` | exposed edge on the left side; continues seamlessly into neighbouring tiles on the top, right, bottom |
| `Assets/Art/Tiles/Castle/Tile_Castle_08.png` | exposed edge on the top, right, bottom sides; continues seamlessly into neighbouring tiles on the left; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Castle/Tile_Castle_09.png` | exposed edge on the right, bottom sides; continues seamlessly into neighbouring tiles on the top, left |
| `Assets/Art/Tiles/Castle/Tile_Castle_10.png` | exposed edge on the top, bottom sides; continues seamlessly into neighbouring tiles on the right, left; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Castle/Tile_Castle_11.png` | exposed edge on the bottom side; continues seamlessly into neighbouring tiles on the top, right, left |
| `Assets/Art/Tiles/Castle/Tile_Castle_12.png` | exposed edge on the top, right sides; continues seamlessly into neighbouring tiles on the bottom, left; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Castle/Tile_Castle_13.png` | exposed edge on the right side; continues seamlessly into neighbouring tiles on the top, bottom, left |
| `Assets/Art/Tiles/Castle/Tile_Castle_14.png` | exposed edge on the top side; continues seamlessly into neighbouring tiles on the right, bottom, left; the top edge is a walkable floor surface: draw a slightly lighter lip |
| `Assets/Art/Tiles/Castle/Tile_Castle_15.png` | fully surrounded interior fill (no visible edges; must tile seamlessly on all four sides) |
| `Assets/Art/Tiles/Castle/Tile_Castle_OneWay_L.png` | one-way platform LEFT end cap (thin plank / ledge in the top third of the tile, the rest transparent) |
| `Assets/Art/Tiles/Castle/Tile_Castle_OneWay_M.png` | one-way platform MIDDLE section (thin plank in the top third, tiles left-right seamlessly) |
| `Assets/Art/Tiles/Castle/Tile_Castle_OneWay_R.png` | one-way platform RIGHT end cap |
| `Assets/Art/Tiles/Castle/Tile_Castle_Spikes.png` | a row of sharp spikes pointing up from the bottom of the tile, transparent above |
| `Assets/Art/Tiles/Castle/Decor_Castle_00.png` | 100×100 decoration with no collision (cave: hanging roots, crystals, bones, moss; castle: banner, chain, cracked brick, window) |
| `Assets/Art/Tiles/Castle/Decor_Castle_01.png` | 100×100 decoration with no collision (cave: hanging roots, crystals, bones, moss; castle: banner, chain, cracked brick, window) |
| `Assets/Art/Tiles/Castle/Decor_Castle_02.png` | 100×100 decoration with no collision (cave: hanging roots, crystals, bones, moss; castle: banner, chain, cracked brick, window) |
| `Assets/Art/Tiles/Castle/Decor_Castle_03.png` | 100×100 decoration with no collision (cave: hanging roots, crystals, bones, moss; castle: banner, chain, cracked brick, window) |
| `Assets/Art/Tiles/Castle/Tile_Castle_Lava_00.png` … `Tile_Castle_Lava_03.png` | glowing lava surface, 4-frame bubbling loop (`Assets/Art/Tiles/Castle/Tile_Castle_Lava_00.png` … `Tile_Castle_Lava_03.png`), tiles left-right seamlessly |

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. Seamless castle masonry: pale stone blocks #D8DEE9 with mid-grey mortar #A7B0C4 and blue-grey accent #5E86A8, clean cut edges tile set for a side-view platformer, 50x50 px tiles, flat silhouette style with a lighter top lip on floor surfaces.
```

## 5. Backgrounds & foreground overlays

Render order back→front: parchment paper → flame glow → far background → mid background → level → characters → FX → **ParchmentFrame (foreground overlay)** → UI.

### Parchment_Beige

- **Size:** 2048×2048 px · **file(s):** `Assets/Art/Backgrounds/Parchment_Beige.png`

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. Seamless tileable cream-beige parchment paper texture #E8D9B5 with subtle fibres and faint stains, evenly lit, no vignette (the light comes from code). Must tile on all four edges. Size 2048x2048 px.
```

### Parchment_Inverted

- **Size:** 2048×2048 px · **file(s):** `Assets/Art/Backgrounds/Parchment_Inverted.png`

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. OPTIONAL / currently unused (Phase 2 tints the beige paper grey-blue instead): the same parchment texture in deep navy with pale fibres. Tileable. Size 2048x2048 px.
```

### ParchmentFrame

- **Size:** 1920×1080 px · **file(s):** `Assets/Art/Backgrounds/ParchmentFrame.png`

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. Foreground overlay (1920×1080, alpha): transparent centre, burnt / scorched paper edges darkening toward all four borders like an old theatre screen, irregular burn marks. Grey-scale (tinted per theme in code). Size 1920x1080 px.
```

### FlameGlow

- **Size:** 1024×1024 px · **file(s):** `Assets/Art/Backgrounds/FlameGlow.png`

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. Soft radial gradient: pure white centre fading smoothly to fully transparent at the edges, perfectly round, no texture (tinted in code). Size 1024x1024 px.
```

### FlameCore

- **Size:** 256×256 px · 6 frames @ 10 fps loop · **file(s):** `Assets/Art/Backgrounds/FlameCore_00.png` … `FlameCore_05.png`

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. The flame seen through the paper: a blurred, soft white-yellow candle-flame silhouette, 6-frame flicker loop, centred, transparent background (tinted in code). Size 256x256 px.
```

### BG_Cave_Far

- **Size:** 2048×1080 px · **file(s):** `Assets/Art/Backgrounds/BG_Cave_Far.png`

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. Far cave background layer (horizontally tileable, transparent sky area): distant cutout silhouettes of stalactites, rock columns and cave arches in dark grey #26262A with faint muted-red #6B3A3A highlights, low contrast. Left and right edges must tile. Size 2048x1080 px.
```

### BG_Cave_Mid

- **Size:** 2048×1080 px · **file(s):** `Assets/Art/Backgrounds/BG_Cave_Mid.png`

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. Mid cave background layer (horizontally tileable, mostly transparent): closer cutout silhouettes of stalactites, broken pillars and hanging roots in grey #4A4A52, a bit more contrast than the far layer. Left/right edges tile. Size 2048x1080 px.
```

### BG_Castle_Far

- **Size:** 2048×1080 px · **file(s):** `Assets/Art/Backgrounds/BG_Castle_Far.png`

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. Far castle background layer (horizontally tileable, transparent sky area): distant towers, spires and battlements in pale grey-blue #A7B0C4, low contrast. Left/right edges tile. Size 2048x1080 px.
```

### BG_Castle_Mid

- **Size:** 2048×1080 px · **file(s):** `Assets/Art/Backgrounds/BG_Castle_Mid.png`

```
2D game environment art, shadow-puppet theatre style: layered cut-paper silhouettes on a backlit paper screen, flat shapes, hard edges, limited palette, subtle paper fibre texture. Mid castle background layer (horizontally tileable, mostly transparent): arches, hanging chains, banners and window frames in pale stone #D8DEE9 with blue-grey #5E86A8 accents. Left/right edges tile. Size 2048x1080 px.
```

## 6. Cutscene panels

Full-screen panels (1920×1080, parallax layers 2400×1080) shown with slow pans; the God puppets are separate transparent sprites.

### CS1_Village_Far

- **Size:** 2400×1080 px · **file:** `Assets/Art/Cutscenes/CS1_Village_Far.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Intro, parallax layer 1 (far, 2400×1080): a mountain range silhouette with a cave mouth at its foot under a warm orange-lit paper sky. Size 2400x1080 px.
```

### CS1_Village_Mid

- **Size:** 2400×1080 px · **file:** `Assets/Art/Cutscenes/CS1_Village_Mid.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Intro, parallax layer 2 (mid, 2400×1080, transparent top): rooftops of a small village (curved roofs, lanterns) as black cutouts. Size 2400x1080 px.
```

### CS1_Village_Near

- **Size:** 2400×1080 px · **file:** `Assets/Art/Cutscenes/CS1_Village_Near.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Intro, parallax layer 3 (near, 2400×1080, transparent top): foreground fence posts, grass tufts and a lantern pole as black cutouts along the bottom. Size 2400x1080 px.
```

### CS1_CaveMouth

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS1_CaveMouth.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Intro end: the dark cave mouth at the mountain's foot, jagged rock teeth, the protagonist's tiny silhouette at the entrance. Size 1920x1080 px.
```

### CS2_Memory_01

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS2_Memory_01.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Memory flash: a figure made half of light and half of shadow, standing tall (a god, whole). Size 1920x1080 px.
```

### CS2_Memory_02

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS2_Memory_02.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Memory flash: a crown of fire floating in darkness. Size 1920x1080 px.
```

### CS2_Memory_03

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS2_Memory_03.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Memory flash: a vast great hall with pillars and a throne of light. Size 1920x1080 px.
```

### CS2_Memory_04

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS2_Memory_04.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Memory flash: two beings divided — a bright one and a dark one pulled apart. Size 1920x1080 px.
```

### CS2_Memory_05

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS2_Memory_05.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Memory flash: a pale hand reaching out toward the viewer. Size 1920x1080 px.
```

### CS2_Memory_06

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS2_Memory_06.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Memory flash: a castle on a cliff under a dark sky. Size 1920x1080 px.
```

### CS2_Zulmat_Eyes

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS2_Zulmat_Eyes.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Pitch darkness with two crimson slit eyes opening in the middle of the frame. Size 1920x1080 px.
```

### CS2_Castle_Silhouette

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS2_Castle_Silhouette.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. A red-black castle silhouette on a cliff (tall, used for an upward pan), a single window glowing. Size 1920x1080 px.
```

### CS3_Merge_01

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS3_Merge_01.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Ending memory: a god, whole and radiant, one figure of light and shadow combined. Size 1920x1080 px.
```

### CS3_Merge_02

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS3_Merge_02.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Ending memory: the god's body being cut down the middle by a line of light. Size 1920x1080 px.
```

### CS3_Merge_03

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS3_Merge_03.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Ending memory: the two halves separated — a bright small figure (the hero) and a tall dark one (Zulmat). Size 1920x1080 px.
```

### CS3_GodsStab

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS3_GodsStab.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. Three tall hooded gods surround a merged glowing figure; one drives a long blade into it. Size 1920x1080 px.
```

### CS3_Split

- **Size:** 1920×1080 px · **file:** `Assets/Art/Cutscenes/CS3_Split.png`

```
full-screen cutscene illustration, shadow-puppet theatre style: black and dark cutout silhouettes against a warm backlit parchment screen, strong silhouettes, limited palette (parchment beige, black, one accent colour), subtle paper fibre texture, cinematic composition, no text. The figure splitting into a light half and a dark half, falling apart in opposite directions, fading to black at the edges. Size 1920x1080 px.
```

### God_A

- **Size:** 512×768 px · **file:** `Assets/Art/Cutscenes/God_A.png`

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Hooded god puppet A (512×768, pivot bottom, transparent background): very tall robed figure, deep hood, no visible face, holding a long staff. Size 512x768 px.
```

### God_B

- **Size:** 512×768 px · **file:** `Assets/Art/Cutscenes/God_B.png`

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Hooded god puppet B: tall robed figure, hood with a pointed tip, hands folded in long sleeves. Size 512x768 px.
```

### God_C

- **Size:** 512×768 px · **file:** `Assets/Art/Cutscenes/God_C.png`

```
2D game sprite, shadow-puppet theatre style: a flat cut-paper puppet silhouette pressed against a backlit paper screen, bold clean solid shapes, crisp hard edges, minimal inner detail, limbs clearly separated so the pose reads as a silhouette, one bright accent colour (eyes / scarf / rim light), subtle paper-cut texture, side view, character facing RIGHT, fully transparent background (PNG with alpha), no ground, no cast shadow, no text, no frame, no border. Hooded god puppet C: tall robed figure, broad shoulders, holding a long thin blade pointed down. Size 512x768 px.
```

## 7. UI

1:1 pixels on a 1920×1080 screen, transparent backgrounds, cut-paper style matching the theatre.

### Heart_Full

- **Size:** 64×64 px · **file(s):** `Assets/Art/UI/Heart_Full.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. HUD health heart (mask-like, Hollow-Knight style but heart-shaped), filled, cut-paper, readable at small size. Size 64x64 px.
```

### Heart_Empty

- **Size:** 64×64 px · **file(s):** `Assets/Art/UI/Heart_Empty.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. The same heart shape, empty / hollow outline only. Size 64x64 px.
```

### Heart_Break

- **Size:** 64×64 px · 4 frames @ 16 fps · **file(s):** `Assets/Art/UI/Heart_Break_00.png` … `Heart_Break_03.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. The full heart cracking and breaking apart (4 frames): crack appears, splits, shards fall, empty. Size 64x64 px.
```

### Soul_Frame

- **Size:** 192×192 px · **file(s):** `Assets/Art/UI/Soul_Frame.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. Circular soul vessel frame (like a glass orb holder) with ornamental rim; the inside is transparent. Size 192x192 px.
```

### Soul_Fill

- **Size:** 192×192 px · **file(s):** `Assets/Art/UI/Soul_Fill.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. The soul liquid that fills the vessel: a circle of glowing blue-white liquid with a wavy top surface (drawn full; filled vertically in code). Size 192x192 px.
```

### Soul_Glow

- **Size:** 256×256 px · **file(s):** `Assets/Art/UI/Soul_Glow.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. A soft blue-white glow ring around the vessel, transparent centre (pulses when the soul is usable). Size 256x256 px.
```

### BossBar_Frame

- **Size:** 900×48 px · **file(s):** `Assets/Art/UI/BossBar_Frame.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. Boss health bar frame (900×48): long ornamental cut-paper bar border, transparent inside. Size 900x48 px.
```

### BossBar_Fill

- **Size:** 900×48 px · **file(s):** `Assets/Art/UI/BossBar_Fill.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. Boss health bar fill (900×48): a solid red bar with a subtle texture (cropped horizontally in code). Size 900x48 px.
```

### Button_Normal

- **Size:** 360×80 px · **file(s):** `Assets/Art/UI/Button_Normal.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. Menu button (360×80): a torn cut-paper strip, cream with a dark ink edge, no text. Size 360x80 px.
```

### Button_Hover

- **Size:** 360×80 px · **file(s):** `Assets/Art/UI/Button_Hover.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. Same button highlighted: brighter paper with a red ink underline / glow, no text. Size 360x80 px.
```

### Title_Logo

- **Size:** 1200×400 px · **file(s):** `Assets/Art/UI/Title_Logo.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. Game title logo 'CINDERS OF FATE' (1200×400, transparent background): cut-paper lettering, ember / flame motif, black and red with a warm glow. Size 1200x400 px.
```

### Cursor

- **Size:** 64×64 px · **file(s):** `Assets/Art/UI/Cursor.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. Menu selection marker (64×64): a small flame or arrow cut-paper icon. Size 64x64 px.
```

### Ability_Icon_Dash

- **Size:** 96×96 px · **file(s):** `Assets/Art/UI/Ability_Icon_Dash.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. Ability icon (96×96): 'Time Slip' dash — a running silhouette with motion streaks and a clock motif. Size 96x96 px.
```

### Ability_Icon_Fireball

- **Size:** 96×96 px · **file(s):** `Assets/Art/UI/Ability_Icon_Fireball.png`

```
2D game UI element, cut-paper shadow-theatre style, crisp edges, transparent background, no text unless stated. Ability icon (96×96): 'Flamecall' fireball — a blue-white flame orb in a circle. Size 96x96 px.
```

