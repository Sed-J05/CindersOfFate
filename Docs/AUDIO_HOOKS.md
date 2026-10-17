# AUDIO HOOKS (generated — run the generator again after adding sounds)

Every sound the game asks for. Drop a clip named exactly like the id into `Assets/Resources/Audio/` (sfx) or
`Assets/Resources/Audio/Music/` (music) and it plays — missing clips are silent no-ops (`AudioManager`).
Gamepad rumble and shake are separate (`Assets/Resources/FeelConfig.asset`).

**87 ids.**

## Music

| id | kind | when | source |
|---|---|---|---|
| `music_boss` | music | Mini-boss fight starts | Assets/Scripts/Level/LevelSceneSetup.cs |
| `music_ending` | music | Cutscene 3 starts | Assets/Data/Cutscenes/cs3_ending.json |
| `music_final_boss` | music | Zulmat fight starts | Assets/Scripts/Level/LevelSceneSetup.cs |
| `music_intro` | music | Cutscene 1 starts | Assets/Data/Cutscenes/cs1_intro.json |
| `music_memory` | music | Cutscene 2 starts | Assets/Data/Cutscenes/cs2_orb.json |
| `music_menu` | music | Main menu opens | Assets/Scripts/UI/MainMenuController.cs |
| `music_phase1` | music | Phase 1 scene starts / a boss fight ends in Phase 1 | Assets/Scripts/Level/LevelSceneSetup.cs |
| `music_phase2` | music | Phase 2 scene starts / a boss fight ends in Phase 2 | Assets/Scripts/Level/LevelSceneSetup.cs |

## UI & menus

| id | kind | when | source |
|---|---|---|---|
| `ui_card` | sfx | Title / phase / cycle card appears | Assets/Scripts/UI/ScreenCard.cs |
| `ui_confirm` | sfx | Menu button pressed | Assets/Scripts/UI/UIFactory.cs |
| `ui_move` | sfx | Menu selection moves / hover | Assets/Scripts/UI/MenuButtonFeedback.cs |
| `ui_pause` | sfx | Game paused | Assets/Scripts/Core/GameManager.cs |
| `ui_prompt` | sfx | Interact prompt appears (bench, sign, door) | Assets/Scripts/Level/InteractableBase.cs |
| `ui_resume` | sfx | Game resumed | Assets/Scripts/Core/GameManager.cs |
| `ui_text` | sfx | Cutscene caption line starts | Assets/Scripts/Cutscenes/CutsceneUI.cs |

## Player

| id | kind | when | source |
|---|---|---|---|
| `fireball_impact` | sfx | Fireball hits | Assets/Scripts/Player/PlayerFxPools.cs |
| `player_attack` | sfx | Side swing | Assets/Scripts/Player/PlayerCombat.cs |
| `player_attack_up` | sfx | Up swing | Assets/Scripts/Player/PlayerCombat.cs |
| `player_dash` | sfx | Time Slip dash | Assets/Scripts/Player/PlayerDash.cs |
| `player_death` | sfx | Player dies | Assets/Scripts/Player/PlayerHealth.cs |
| `player_fireball` | sfx | Fireball cast | Assets/Scripts/Player/PlayerFxPools.cs |
| `player_heal` | sfx | Heal completes | Assets/Scripts/Player/PlayerSpells.cs |
| `player_heal_start` | sfx | Heal begins | Assets/Scripts/Player/PlayerSpells.cs |
| `player_hit_boss` | sfx | Nail hits a boss | Assets/Scripts/Player/PlayerCombat.cs |
| `player_hit_enemy` | sfx | Nail hits an enemy | Assets/Scripts/Player/PlayerCombat.cs |
| `player_hurt` | sfx | Player takes damage | Assets/Scripts/Player/PlayerHealth.cs |
| `player_jump` | sfx | Jump | Assets/Scripts/Player/PlayerController.cs |
| `player_land` | sfx | Land | Assets/Scripts/Player/PlayerController.cs |
| `player_respawn` | sfx | Respawn (death, spikes, bench start) | Assets/Scripts/Player/PlayerController.cs |
| `soul_full` | sfx | Soul vessel becomes full | Assets/Scripts/Player/PlayerSoul.cs |
| `soul_gain` | sfx | Soul gained from a hit | Assets/Scripts/Player/PlayerSoul.cs |

## World & level

| id | kind | when | source |
|---|---|---|---|
| `ability_unlocked` | sfx | Ability popup | Assets/Scripts/UI/AbilityPopupUI.cs |
| `bench_rest` | sfx | Sit at a bench | Assets/Scripts/Level/Bench.cs |
| `bench_saved` | sfx | Bench rest finished (healed + saved) | Assets/Scripts/Level/Bench.cs |
| `door_open` | sfx | Door opens | Assets/Scripts/Level/Door.cs |
| `gate_close` | sfx | Boss gate closes | Assets/Scripts/Level/BossGate.cs |
| `gate_open` | sfx | Boss gate opens | Assets/Scripts/Level/BossGate.cs |
| `hazard_lava` | sfx | Lava hits the player | Assets/Scripts/Player/PlayerHealth.cs |
| `hazard_spikes` | sfx | Spikes hit the player | Assets/Scripts/Player/PlayerHealth.cs |
| `orb_touch` | sfx | Orb touched | Assets/Scripts/Level/Orb.cs |
| `room_transition` | sfx | Room-to-room paper sweep | Assets/Scripts/Visuals/ScreenWipe.cs |

## Regular enemies

| id | kind | when | source |
|---|---|---|---|
| `bat_telegraph` | sfx | Telegraph (bat) | Assets/Scripts/Enemies/Bat.cs |
| `bat_wake` | sfx | Wake (bat) | Assets/Scripts/Enemies/Bat.cs |
| `charger_charge` | sfx | Charge (charger) | Assets/Scripts/Enemies/Charger.cs |
| `charger_stun` | sfx | Stun (charger) | Assets/Scripts/Enemies/Charger.cs |
| `charger_telegraph` | sfx | Telegraph (charger) | Assets/Scripts/Enemies/Charger.cs |
| `enemy_death` | sfx | Any enemy dies | Assets/Scripts/Enemies/EnemyBase.cs |
| `enemy_hit` | sfx | Any enemy takes damage | Assets/Scripts/Enemies/EnemyBase.cs |
| `kite_dive` | sfx | Dive (kite) | Assets/Scripts/Enemies/ShadowKite.cs |
| `kite_telegraph` | sfx | Telegraph (kite) | Assets/Scripts/Enemies/ShadowKite.cs |
| `knight_slash` | sfx | Slash (knight) | Assets/Scripts/Enemies/Knight.cs |
| `knight_telegraph` | sfx | Telegraph (knight) | Assets/Scripts/Enemies/Knight.cs |
| `lunger_lunge` | sfx | Lunge (lunger) | Assets/Scripts/Enemies/Lunger.cs |
| `lunger_telegraph` | sfx | Telegraph (lunger) | Assets/Scripts/Enemies/Lunger.cs |
| `thrower_hop` | sfx | Hop (thrower) | Assets/Scripts/Enemies/Thrower.cs |
| `thrower_telegraph` | sfx | Telegraph (thrower) | Assets/Scripts/Enemies/Thrower.cs |
| `thrower_throw` | sfx | Throw (thrower) | Assets/Scripts/Enemies/Thrower.cs |

## Bosses

| id | kind | when | source |
|---|---|---|---|
| `boss_death` | sfx | Boss death slow-mo starts | Assets/Scripts/Bosses/BossBase.cs |
| `boss_intro` | sfx | Boss intro card | Assets/Scripts/Bosses/BossBase.cs |
| `caller_blackslash` | sfx | Blackslash (caller) | Assets/Scripts/Bosses/KageCaller.cs |
| `caller_blackslash_telegraph` | sfx | Blackslash telegraph (caller) | Assets/Scripts/Bosses/KageCaller.cs |
| `caller_summon` | sfx | Summon (caller) | Assets/Scripts/Bosses/KageCaller.cs |
| `colossus_bash` | sfx | Bash (colossus) | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_clink` | sfx | Clink (colossus) | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_dash` | sfx | Dash (colossus) | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_jump` | sfx | Jump (colossus) | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_slam` | sfx | Slam (colossus) | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_slash` | sfx | Slash (colossus) | Assets/Scripts/Bosses/IronColossus.cs |
| `colossus_telegraph` | sfx | Telegraph (colossus) | Assets/Scripts/Bosses/IronColossus.cs |
| `kage_caller_slash` | sfx | Caller slash (kage) | Assets/Scripts/Bosses/SwordsmanBoss.cs (config.id + suffix) |
| `kage_caller_telegraph` | sfx | Caller telegraph (kage) | Assets/Scripts/Bosses/SwordsmanBoss.cs (config.id + suffix) |
| `kage_warden_slash` | sfx | Warden slash (kage) | Assets/Scripts/Bosses/SwordsmanBoss.cs (config.id + suffix) |
| `kage_warden_telegraph` | sfx | Warden telegraph (kage) | Assets/Scripts/Bosses/SwordsmanBoss.cs (config.id + suffix) |
| `warden_clink` | sfx | Clink (warden) | Assets/Scripts/Bosses/KageWarden.cs |
| `warden_slash` | sfx | Slash (warden) | Assets/Scripts/Bosses/KageWarden.cs |
| `zulmat_cast` | sfx | Cast (zulmat) | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_death` | sfx | Death (zulmat) | Assets/Scripts/Bosses/Zulmat.cs |
| `zulmat_grow` | sfx | Grow (zulmat) | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_lava` | sfx | Lava (zulmat) | Assets/Scripts/Bosses/LavaZoneHazard.cs |
| `zulmat_lunge` | sfx | Lunge (zulmat) | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_roar` | sfx | Roar (zulmat) | Assets/Scripts/Bosses/Zulmat.cs |
| `zulmat_shrink` | sfx | Shrink (zulmat) | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_slash` | sfx | Slash (zulmat) | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_summon` | sfx | Summon (zulmat) | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_swipe` | sfx | Swipe (zulmat) | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_telegraph` | sfx | Telegraph (zulmat) | Assets/Scripts/Bosses/Zulmat.Moves.cs |
| `zulmat_vine` | sfx | Vine (zulmat) | Assets/Scripts/Bosses/VineHazard.cs |
