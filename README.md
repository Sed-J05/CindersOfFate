# Cinders of Fate

A 2D shadow-puppet action platformer made in Unity 6. Every creature is a cut-paper puppet and every shadow is cast by a
living flame behind the screen. Descend into the cave beneath your village, defeat what waits there, and find out
what you cast away.

## Play it (no Unity needed)

Download from the [`Release/`](Release/) folder of this repo:

| Platform | File | How to run |
|---|---|---|
| **Windows** 10/11 (64-bit) | [`Release/CindersOfFate_Windows.zip`](Release/CindersOfFate_Windows.zip) | Unzip the whole folder, double-click **Cinders of Fate.exe**. If SmartScreen appears: *More info → Run anyway*. |
| **macOS** 11+ (Intel and Apple silicon) | [`Release/CindersOfFate_macOS.zip`](Release/CindersOfFate_macOS.zip) | Unzip, then **right-click Cinders of Fate.app → Open → Open** (the app isn't Apple-signed). If macOS says it "is damaged", run `xattr -cr "/path/to/Cinders of Fate.app"` in Terminal and open it again. |

Each zip also contains a `HOW TO RUN.txt`.

## Controls

| Action | Keyboard | Gamepad |
|---|---|---|
| Move | A / D or ← / → | Left stick / D-pad |
| Jump | Space | A / Cross |
| Attack (hold W to aim up) | J | X / Square |
| Heal (hold) · Fireball (tap, Phase 2) | K | B / Circle |
| Dash (Phase 2) | L | RT / RB |
| Interact (benches, signs, orb) | W / ↑ | D-pad up |
| Pause | Esc | Start |

## Build from source

1. Unity **6000.0.32f1** with Windows Build Support (and **Mac Build Support (Mono)** for the Mac player).
2. Open the project, then run **Shatranj → Build for Submission (Windows + macOS)**. It builds both players and writes the
   zips to `Release/`.

Design docs are in [`Docs/`](Docs/). All art, sound effects and music were made for this game (see `Tools/artgen` and
`Tools/audiogen`); the voice lines were recorded by the team.
