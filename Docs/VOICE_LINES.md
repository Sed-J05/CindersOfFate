# VOICE LINES — Cinders of Fate

All **8 voice lines** in the game: the Narrator and the three Gods. The hero and Zulmat are silent by design (their lines
stay text-only). Every clip below is recorded and installed in `Assets/Resources/Audio/Voice/`.
(Sound effects and music are in `Docs/AUDIO_AND_VOICE.md`.)

## How to replace a voice file

1. Export WAV, **mono**, 44.1 kHz, around −18 LUFS, no silence at the start.
2. Name it **exactly** like its id, e.g. `vo_cs1_02.wav`, and put it in `Assets/Resources/Audio/Voice/`.
3. Or put new full takes in a folder and re-run `python Tools/audiogen/voices.py <folder>` (update the cut times in `voices.py` if the takes change).

The voice plays with its caption; the caption stays up until the voice ends, the music ducks underneath, and pressing to
advance (or skipping the cutscene) cuts the line off.

Recorded takes: `cut1.wav` (all Narrator lines) and `cut2.wav` (the Gods), split per line by `Tools/audiogen/voices.py`. Captions stay on screen until their voice ends, and the music ducks under every voice line. 
Each cutscene caption line has its own id (stored in the `"voices"` list next to the `"lines"` in `Assets/Data/Cutscenes/*.json`). The voice starts with the caption and stops when the player advances. Skipping the cutscene stops it too.

## Cast

| Voice | Who | Tone |
|---|---|---|
| Narrator | Storyteller of the shadow theatre | Warm, slow, older; reads like a fable. **Recorded.** |
| The Gods | Three hooded gods | Three voices in unison, cold and echoing (recorded once, doubled + hall in post). **Recorded.** |
| Protagonist, Zulmat | — | **Silent by design**: their lines stay text-only captions. |

## Lines

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
