# AUDIO & VOICE — Cinders of Fate

Everything the game can play: **8 voice lines** and **87 sounds/music tracks**. 
**All of them are installed.** The voice clips are cut from the team's recordings; every sound effect and music track is generated from scratch (no samples, no third-party music) by `Tools/audiogen/` — run `python Tools/audiogen/build_audio.py` to rebuild them. To replace any sound, drop a file with the same id into the folder shown (it overrides the generated one when the generator isn't re-run).

## How to add a file

1. Export the clip (format notes below) and name it **exactly** like its id, e.g. `vo_cs1_02.wav`, `player_jump.wav`, `music_phase1.ogg`.
2. Put it in the folder for its kind:

   | Kind | Folder |
   |---|---|
   | Voice lines | `Assets/Resources/Audio/Voice/` |
   | Sound effects | `Assets/Resources/Audio/` |
   | Music | `Assets/Resources/Audio/Music/` |

   Create the folders if they don't exist yet (right-click in the Unity Project window → Create → Folder).
3. That's it — press Play. No code, no menu items.
4. Music loops by itself (no need to tick Loop on the file). For long tracks set *Load Type = Streaming* in the import settings to save memory.

**Format notes**

- WAV or OGG, 44.1 kHz or 48 kHz. Sound effects and voice: **mono**. Music: stereo.
- Trim silence at the start of every SFX so it lands on the hit.
- Keep levels consistent: voice around −16 LUFS, music a little quieter than voice, SFX peaks below −3 dB.
- Voice lines should end cleanly; the player can press to skip ahead and the line is cut off.

---

## Voice lines

Recorded takes: `cut1.wav` (all Narrator lines) and `cut2.wav` (the Gods), split per line by `Tools/audiogen/voices.py`. Captions stay on screen until their voice ends, and the music ducks under every voice line. 
Each cutscene caption line has its own id (stored in the `"voices"` list next to the `"lines"` in `Assets/Data/Cutscenes/*.json`). The voice starts with the caption and stops when the player advances. Skipping the cutscene stops it too.

**Cast**

| Voice | Who | Tone |
|---|---|---|
| Narrator | Storyteller of the shadow theatre | Warm, slow, older; reads like a fable. **Recorded.** |
| The Gods | Three hooded gods | Three voices in unison, cold and echoing (recorded once, doubled + hall in post). **Recorded.** |
| Protagonist, Zulmat | — | **Silent by design**: their lines stay text-only captions. |

### Cutscene 1 — Intro (village → cave)

Plays on New Game (and each new cycle). Script: `Assets/Data/Cutscenes/cs1_intro.json`.

| id | Speaker | Line | Direction | File |
|---|---|---|---|---|
| `vo_cs1_01` | Narrator | "…again." | Weary, almost a whisper — only heard on cycle 2+, the narrator realises it is happening again. | `Assets/Resources/Audio/Voice/vo_cs1_01.wav` |
| `vo_cs1_02` | Narrator | "Our village slept beneath the mountain's shadow." | Calm storyteller, warm, slow. Like opening a fairy tale. | `Assets/Resources/Audio/Voice/vo_cs1_02.wav` |
| `vo_cs1_03` | Narrator | "Then the shadows began to move." | Same voice, darker; slight pause after 'Then'. | `Assets/Resources/Audio/Voice/vo_cs1_03.wav` |

### Cutscene 2 — The Orb (end of Phase 1)

Plays when the player touches the Orb. Script: `Assets/Data/Cutscenes/cs2_orb.json`.

| id | Speaker | Line | Direction | File |
|---|---|---|---|---|
| `vo_cs2_05` | Narrator | "The light behind the screen changed. So did everything on it." | Narrator, hushed awe. | `Assets/Resources/Audio/Voice/vo_cs2_05.wav` |

### Cutscene 3 — Ending (after Zulmat)

Plays after the final boss. Script: `Assets/Data/Cutscenes/cs3_ending.json`.

| id | Speaker | Line | Direction | File |
|---|---|---|---|---|
| `vo_cs3_06` | The Gods | "Whole, you are too much." | The Gods: three voices in unison, cold, echoing. | `Assets/Resources/Audio/Voice/vo_cs3_06.wav` |
| `vo_cs3_07` | The Gods | "Be halved again." | Same three voices, a verdict. Short. | `Assets/Resources/Audio/Voice/vo_cs3_07.wav` |
| `vo_cs3_08` | Narrator | "Two halves, each too weak to resist. Their memories locked away." | Narrator, sad and slow; let it trail off. | `Assets/Resources/Audio/Voice/vo_cs3_08.wav` |

### In-game

| id | Speaker | Line | Direction | File |
|---|---|---|---|---|
| `vo_extinguished` | Narrator | "You were extinguished" | Plays on the death screen. Quiet, matter-of-fact, ~1 s. | `Assets/Resources/Audio/Voice/vo_extinguished.wav` |

### Adding or changing a voice line

- To change wording, edit the `"lines"` text in the cutscene JSON and re-record the clip with the same id.
- To add a line, add the text to `"lines"` and a new id to `"voices"` **in the same position**, e.g.

  ```json
  { "type": "text", "voices": ["vo_cs1_02", "vo_cs1_02b"], "style": "narration",
    "lines": ["Our village slept beneath the mountain's shadow.", "A new line here."] }
  ```
- A single line can use `"voice": "vo_id"` instead of a list.
- Caption styles: `narration` (white), `inner` (light blue, the hero's thoughts), `boss` (red).

---

## Sound effects & music

The "Plays when" column says exactly when the game asks for the sound; "Code" is where it's triggered (for reference). The ids are also listed in `Docs/AUDIO_HOOKS.md`.

### Music

| id | Plays when | What it should sound like | File | Code |
|---|---|---|---|---|
| `music_boss` | Mini-boss fight starts | Mini-boss fight: driving drums, taiko-style hits, tense strings. 1–2 min loop. | `Assets/Resources/Audio/Music/music_boss.ogg` | Assets/Scripts/Level/LevelSceneSetup.cs |
| `music_ending` | Cutscene 3 starts | Bittersweet: the menu theme slowed down, swelling then fading to silence. | `Assets/Resources/Audio/Music/music_ending.ogg` | Assets/Data/Cutscenes/cs3_ending.json |
| `music_final_boss` | Zulmat fight starts | Zulmat: the heaviest track — choir, pounding drums, the Phase 1 melody twisted. Loops. | `Assets/Resources/Audio/Music/music_final_boss.ogg` | Assets/Scripts/Level/LevelSceneSetup.cs |
| `music_intro` | Cutscene 1 starts | Gentle village theme turning uneasy: warm flute/strings, then a darker low note when the shadows move. | `Assets/Resources/Audio/Music/music_intro.ogg` | Assets/Data/Cutscenes/cs1_intro.json |
| `music_memory` | Cutscene 2 starts | Cutscene 2: dreamy, reversed/echoing piano or bells, swelling into dread when the eyes open. | `Assets/Resources/Audio/Music/music_memory.ogg` | Assets/Data/Cutscenes/cs2_orb.json |
| `music_menu` | Main menu opens | Slow, quiet theme: low drone, plucked strings, a soft crackle of fire. Loops. | `Assets/Resources/Audio/Music/music_menu.ogg` | Assets/Scripts/UI/MainMenuController.cs |
| `music_phase1` | Phase 1 scene starts / a boss fight ends in Phase 1 | Cave exploration: sparse, echoing, low percussion, dripping-water ambience, minor key. Loops. | `Assets/Resources/Audio/Music/music_phase1.ogg` | Assets/Scripts/Level/LevelSceneSetup.cs |
| `music_phase2` | Phase 2 scene starts / a boss fight ends in Phase 2 | Castle: colder, bigger, organ or choir pad, faster pulse than Phase 1. Loops. | `Assets/Resources/Audio/Music/music_phase2.ogg` | Assets/Scripts/Level/LevelSceneSetup.cs |

### UI & menus

| id | Plays when | What it should sound like | File | Code |
|---|---|---|---|---|
| `ui_card` | Title / phase / cycle card appears | Soft paper swish + low gong for title / phase cards. | `Assets/Resources/Audio/ui_card.wav` | Assets/Scripts/UI/ScreenCard.cs |
| `ui_confirm` | Menu button pressed | Short wooden click or paper snap. | `Assets/Resources/Audio/ui_confirm.wav` | Assets/Scripts/UI/UIFactory.cs |
| `ui_move` | Menu selection moves / hover | Very quiet paper tick. | `Assets/Resources/Audio/ui_move.wav` | Assets/Scripts/UI/MenuButtonFeedback.cs |
| `ui_pause` | Game paused | Muffled whoosh down. | `Assets/Resources/Audio/ui_pause.wav` | Assets/Scripts/Core/GameManager.cs |
| `ui_prompt` | Interact prompt appears (bench, sign, door) | Tiny chime when an interact prompt pops up. | `Assets/Resources/Audio/ui_prompt.wav` | Assets/Scripts/Level/InteractableBase.cs |
| `ui_resume` | Game resumed | Muffled whoosh up. | `Assets/Resources/Audio/ui_resume.wav` | Assets/Scripts/Core/GameManager.cs |
| `ui_text` | Cutscene caption line starts | Soft paper rustle each time a caption line appears (keep it quiet; it plays under voice lines). | `Assets/Resources/Audio/ui_text.wav` | Assets/Scripts/Cutscenes/CutsceneUI.cs |

### Player

| id | Plays when | What it should sound like | File | Code |
|---|---|---|---|---|
| `fireball_impact` | Fireball hits | Small fiery burst. | `Assets/Resources/Audio/fireball_impact.wav` | Assets/Scripts/Player/PlayerFxPools.cs |
| `player_attack` | Side swing | Quick light blade swish. | `Assets/Resources/Audio/player_attack.wav` | Assets/Scripts/Player/PlayerCombat.cs |
| `player_attack_up` | Up swing | Same swish, slightly higher pitch. | `Assets/Resources/Audio/player_attack_up.wav` | Assets/Scripts/Player/PlayerCombat.cs |
| `player_dash` | Time Slip dash | Time Slip: quick reversed whoosh with a glassy shimmer. | `Assets/Resources/Audio/player_dash.wav` | Assets/Scripts/Player/PlayerDash.cs |
| `player_death` | Player dies | Fire snuffing out: whoosh then a crackling fade. | `Assets/Resources/Audio/player_death.wav` | Assets/Scripts/Player/PlayerHealth.cs |
| `player_fireball` | Fireball cast | Blue flame launch: whoomp + crackle. | `Assets/Resources/Audio/player_fireball.wav` | Assets/Scripts/Player/PlayerFxPools.cs |
| `player_heal` | Heal completes | Warm chime as the heart returns. | `Assets/Resources/Audio/player_heal.wav` | Assets/Scripts/Player/PlayerSpells.cs |
| `player_heal_start` | Heal begins | Rising hum / breath in. | `Assets/Resources/Audio/player_heal_start.wav` | Assets/Scripts/Player/PlayerSpells.cs |
| `player_hit_boss` | Nail hits a boss | Heavier impact with a metallic edge. | `Assets/Resources/Audio/player_hit_boss.wav` | Assets/Scripts/Player/PlayerCombat.cs |
| `player_hit_enemy` | Nail hits an enemy | Crisp slash-into-paper impact. | `Assets/Resources/Audio/player_hit_enemy.wav` | Assets/Scripts/Player/PlayerCombat.cs |
| `player_hurt` | Player takes damage | Sharp hit + short pained breath (non-verbal). | `Assets/Resources/Audio/player_hurt.wav` | Assets/Scripts/Player/PlayerHealth.cs |
| `player_jump` | Jump | Light cloth flap / soft hop. | `Assets/Resources/Audio/player_jump.wav` | Assets/Scripts/Player/PlayerController.cs |
| `player_land` | Land | Soft thud on stone. | `Assets/Resources/Audio/player_land.wav` | Assets/Scripts/Player/PlayerController.cs |
| `player_respawn` | Respawn (death, spikes, bench start) | Small flame igniting. | `Assets/Resources/Audio/player_respawn.wav` | Assets/Scripts/Player/PlayerController.cs |
| `soul_full` | Soul vessel becomes full | Brighter chime: the vessel is full. | `Assets/Resources/Audio/soul_full.wav` | Assets/Scripts/Player/PlayerSoul.cs |
| `soul_gain` | Soul gained from a hit | Tiny soft sparkle (plays a lot — keep subtle). | `Assets/Resources/Audio/soul_gain.wav` | Assets/Scripts/Player/PlayerSoul.cs |

### World & level

| id | Plays when | What it should sound like | File | Code |
|---|---|---|---|---|
| `ability_unlocked` | Ability popup | Triumphant short sting. | `Assets/Resources/Audio/ability_unlocked.wav` | Assets/Scripts/UI/AbilityPopupUI.cs |
| `bench_rest` | Sit at a bench | Candles igniting, a long calm exhale. | `Assets/Resources/Audio/bench_rest.wav` | Assets/Scripts/Level/Bench.cs |
| `bench_saved` | Bench rest finished (healed + saved) | Low warm bell. | `Assets/Resources/Audio/bench_saved.wav` | Assets/Scripts/Level/Bench.cs |
| `door_open` | Door opens | Heavy stone sliding up. | `Assets/Resources/Audio/door_open.wav` | Assets/Scripts/Level/Door.cs |
| `gate_close` | Boss gate closes | Iron portcullis slamming down. | `Assets/Resources/Audio/gate_close.wav` | Assets/Scripts/Level/BossGate.cs |
| `gate_open` | Boss gate opens | Iron portcullis grinding up. | `Assets/Resources/Audio/gate_open.wav` | Assets/Scripts/Level/BossGate.cs |
| `hazard_lava` | Lava hits the player | Sizzle + hiss. | `Assets/Resources/Audio/hazard_lava.wav` | Assets/Scripts/Player/PlayerHealth.cs |
| `hazard_spikes` | Spikes hit the player | Sharp stab + thud. | `Assets/Resources/Audio/hazard_spikes.wav` | Assets/Scripts/Player/PlayerHealth.cs |
| `orb_touch` | Orb touched | Deep resonant hum building into a flash. | `Assets/Resources/Audio/orb_touch.wav` | Assets/Scripts/Level/Orb.cs |
| `room_transition` | Room-to-room paper sweep | Quick paper sweep. | `Assets/Resources/Audio/room_transition.wav` | Assets/Scripts/Visuals/ScreenWipe.cs |

### Regular enemies

| id | Plays when | What it should sound like | File | Code |
|---|---|---|---|---|
| `bat_telegraph` | Telegraph (bat) | High screech. | `Assets/Resources/Audio/bat_telegraph.wav` | Assets/Scripts/Enemies/Bat.cs |
| `bat_wake` | Wake (bat) | Leathery wing flutter + squeak. | `Assets/Resources/Audio/bat_wake.wav` | Assets/Scripts/Enemies/Bat.cs |
| `charger_charge` | Charge (charger) | Heavy thundering run. | `Assets/Resources/Audio/charger_charge.wav` | Assets/Scripts/Enemies/Charger.cs |
| `charger_stun` | Stun (charger) | Wall thud + dizzy ring. | `Assets/Resources/Audio/charger_stun.wav` | Assets/Scripts/Enemies/Charger.cs |
| `charger_telegraph` | Telegraph (charger) | Snort + hoof scrape. | `Assets/Resources/Audio/charger_telegraph.wav` | Assets/Scripts/Enemies/Charger.cs |
| `enemy_death` | Any enemy dies | Puff of smoke + paper crumple. | `Assets/Resources/Audio/enemy_death.wav` | Assets/Scripts/Enemies/EnemyBase.cs |
| `enemy_hit` | Any enemy takes damage | Generic paper-tear hit for any enemy. | `Assets/Resources/Audio/enemy_hit.wav` | Assets/Scripts/Enemies/EnemyBase.cs |
| `kite_dive` | Dive (kite) | Diving whistle. | `Assets/Resources/Audio/kite_dive.wav` | Assets/Scripts/Enemies/ShadowKite.cs |
| `kite_telegraph` | Telegraph (kite) | Paper flutter rising. | `Assets/Resources/Audio/kite_telegraph.wav` | Assets/Scripts/Enemies/ShadowKite.cs |
| `knight_slash` | Slash (knight) | Heavy metal sword swing. | `Assets/Resources/Audio/knight_slash.wav` | Assets/Scripts/Enemies/Knight.cs |
| `knight_telegraph` | Telegraph (knight) | Armour clank as the sword rises. | `Assets/Resources/Audio/knight_telegraph.wav` | Assets/Scripts/Enemies/Knight.cs |
| `lunger_lunge` | Lunge (lunger) | Fast lunge whoosh. | `Assets/Resources/Audio/lunger_lunge.wav` | Assets/Scripts/Enemies/Lunger.cs |
| `lunger_telegraph` | Telegraph (lunger) | Hiss / intake. | `Assets/Resources/Audio/lunger_telegraph.wav` | Assets/Scripts/Enemies/Lunger.cs |
| `thrower_hop` | Hop (thrower) | Light hop. | `Assets/Resources/Audio/thrower_hop.wav` | Assets/Scripts/Enemies/Thrower.cs |
| `thrower_telegraph` | Telegraph (thrower) | Blade draw. | `Assets/Resources/Audio/thrower_telegraph.wav` | Assets/Scripts/Enemies/Thrower.cs |
| `thrower_throw` | Throw (thrower) | Knife whizz. | `Assets/Resources/Audio/thrower_throw.wav` | Assets/Scripts/Enemies/Thrower.cs |

### Bosses

| id | Plays when | What it should sound like | File | Code |
|---|---|---|---|---|
| `boss_death` | Boss death slow-mo starts | Slow-motion boom, then a long smoky fade. | `Assets/Resources/Audio/boss_death.wav` | Assets/Scripts/Bosses/BossBase.cs |
| `boss_intro` | Boss intro card | Big drum hit + low brass when the boss name appears. | `Assets/Resources/Audio/boss_intro.wav` | Assets/Scripts/Bosses/BossBase.cs |
| `caller_blackslash` | Blackslash (caller) | Caller: dark crescent flying — whoosh + tear. | `Assets/Resources/Audio/caller_blackslash.wav` | Assets/Scripts/Bosses/KageCaller.cs |
| `caller_blackslash_telegraph` | Blackslash telegraph (caller) | Caller: ink gathering, low rumble. | `Assets/Resources/Audio/caller_blackslash_telegraph.wav` | Assets/Scripts/Bosses/KageCaller.cs |
| `caller_summon` | Summon (caller) | Caller: hollow chant / smoke burst. | `Assets/Resources/Audio/caller_summon.wav` | Assets/Scripts/Bosses/KageCaller.cs |
| `colossus_bash` | Bash (colossus) | Colossus shield bash: blunt slam. | `Assets/Resources/Audio/colossus_bash.wav` | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_clink` | Clink (colossus) | Colossus shield block: deep metal clang. | `Assets/Resources/Audio/colossus_clink.wav` | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_dash` | Dash (colossus) | Colossus: armoured charge rumble. | `Assets/Resources/Audio/colossus_dash.wav` | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_jump` | Jump (colossus) | Colossus: grunt-less heavy leap whoosh. | `Assets/Resources/Audio/colossus_jump.wav` | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_slam` | Slam (colossus) | Colossus landing: ground-shaking boom. | `Assets/Resources/Audio/colossus_slam.wav` | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_slash` | Slash (colossus) | Colossus: huge greatsword swing. | `Assets/Resources/Audio/colossus_slash.wav` | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_telegraph` | Telegraph (colossus) | Colossus: heavy armour creak. | `Assets/Resources/Audio/colossus_telegraph.wav` | Assets/Scripts/Bosses/IronColossus.cs |
| `kage_caller_slash` | Caller slash (kage) | Caller: curved blade swish. | `Assets/Resources/Audio/kage_caller_slash.wav` | Assets/Scripts/Bosses/SwordsmanBoss.cs (config.id + suffix) |
| `kage_caller_telegraph` | Caller telegraph (kage) | Caller: whispery inhale + dark shimmer. | `Assets/Resources/Audio/kage_caller_telegraph.wav` | Assets/Scripts/Bosses/SwordsmanBoss.cs (config.id + suffix) |
| `kage_warden_slash` | Warden slash (kage) | Warden: long sharp katana slash. | `Assets/Resources/Audio/kage_warden_slash.wav` | Assets/Scripts/Bosses/SwordsmanBoss.cs (config.id + suffix) |
| `kage_warden_telegraph` | Warden telegraph (kage) | Warden: blade drawn back with a ringing scrape. | `Assets/Resources/Audio/kage_warden_telegraph.wav` | Assets/Scripts/Bosses/SwordsmanBoss.cs (config.id + suffix) |
| `warden_clink` | Clink (warden) | Warden blocks: bright metal clink. | `Assets/Resources/Audio/warden_clink.wav` | Assets/Scripts/Bosses/KageWarden.cs |
| `warden_slash` | Slash (warden) | Warden: extra slash layer (heavier). | `Assets/Resources/Audio/warden_slash.wav` | Assets/Scripts/Bosses/KageWarden.cs |
| `zulmat_cast` | Cast (zulmat) | Zulmat: dark magic building. | `Assets/Resources/Audio/zulmat_cast.wav` | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_death` | Death (zulmat) | Zulmat dies: roar collapsing into fire crackle and silence. | `Assets/Resources/Audio/zulmat_death.wav` | Assets/Scripts/Bosses/Zulmat.cs |
| `zulmat_grow` | Grow (zulmat) | Zulmat grows: swelling boom. | `Assets/Resources/Audio/zulmat_grow.wav` | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_lava` | Lava (zulmat) | Lava bursting up: rumble + bubbling. | `Assets/Resources/Audio/zulmat_lava.wav` | Assets/Scripts/Bosses/LavaZoneHazard.cs |
| `zulmat_lunge` | Lunge (zulmat) | Zulmat: fast dash whoosh. | `Assets/Resources/Audio/zulmat_lunge.wav` | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_roar` | Roar (zulmat) | Zulmat roar: huge, distorted, echoing (non-verbal). | `Assets/Resources/Audio/zulmat_roar.wav` | Assets/Scripts/Bosses/Zulmat.cs |
| `zulmat_shrink` | Shrink (zulmat) | Zulmat shrinks: sucking reversed whoosh. | `Assets/Resources/Audio/zulmat_shrink.wav` | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_slash` | Slash (zulmat) | Zulmat: dark heavy sword slash. | `Assets/Resources/Audio/zulmat_slash.wav` | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_summon` | Summon (zulmat) | Zulmat: deep chant, shadows tearing open. | `Assets/Resources/Audio/zulmat_summon.wav` | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_swipe` | Swipe (zulmat) | Zulmat giant swipe: massive whoosh. | `Assets/Resources/Audio/zulmat_swipe.wav` | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_telegraph` | Telegraph (zulmat) | Zulmat: low growl + blade ring. | `Assets/Resources/Audio/zulmat_telegraph.wav` | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_vine` | Vine (zulmat) | Thorny vines erupting from the floor. | `Assets/Resources/Audio/zulmat_vine.wav` | Assets/Scripts/Bosses/VineHazard.cs |

---

## Checklist (priority order)

1. **Core feel:** `player_attack`, `player_hit_enemy`, `enemy_hit`, `enemy_death`, `player_hurt`, `player_jump`, `player_land`, `player_death`.
2. **Telegraphs** (they warn the player): every `*_telegraph` id.
3. **Music:** `music_menu`, `music_phase1`, `music_boss`, `music_phase2`, `music_final_boss`.
4. **Voice:** done (7 cutscene lines + `vo_extinguished`).
5. Everything else (UI, world, boss move layers, cutscene music).
