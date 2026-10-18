# VOICE LINES — Cinders of Fate

All **18 voice lines** in the game: who says them, the exact words, how to perform them, and where the file goes.
(Sound effects and music are in `Docs/AUDIO_AND_VOICE.md`.)

## How to add a voice file

1. Record the line and export it as WAV or OGG, **mono**, 44.1 or 48 kHz, around −16 LUFS, with no silence at the start.
2. Name it **exactly** like its id, e.g. `vo_cs1_02.wav`.
3. Put it in `Assets/Resources/Audio/Voice/` (create the folder if it doesn't exist).
4. Press Play. It plays with its caption; the player pressing to advance (or skipping the cutscene) cuts it off, so end lines cleanly.

Missing files are silent, so you can add them one at a time.

Each cutscene caption line has its own id (stored in the `"voices"` list next to the `"lines"` in `Assets/Data/Cutscenes/*.json`). The voice starts with the caption and stops when the player advances. Skipping the cutscene stops it too.

## Cast

| Voice | Who | Tone |
|---|---|---|
| Narrator | Storyteller of the shadow theatre | Warm, slow, older; reads like a fable. |
| Protagonist (inner voice) | The white-blue hero — later revealed as the god's mercy | Young, quiet, thoughtful. Never shouts. |
| Zulmat | Final boss — the god's cruelty | Deep, slow, bitter. Can be pitched down / layered. |
| The Gods | Three hooded gods | Three voices in unison, cold and echoing. |

## Lines

### Cutscene 1 — Intro (village → cave)

Plays on New Game (and each new cycle). Script: `Assets/Data/Cutscenes/cs1_intro.json`.

| id | Speaker | Line | Direction | File |
|---|---|---|---|---|
| `vo_cs1_01` | Narrator | "…again." | Weary, almost a whisper — only heard on cycle 2+, the narrator realises it is happening again. | `Assets/Resources/Audio/Voice/vo_cs1_01.wav` |
| `vo_cs1_02` | Narrator | "Our village slept beneath the mountain's shadow." | Calm storyteller, warm, slow. Like opening a fairy tale. | `Assets/Resources/Audio/Voice/vo_cs1_02.wav` |
| `vo_cs1_03` | Narrator | "Then the shadows began to move." | Same voice, darker; slight pause after 'Then'. | `Assets/Resources/Audio/Voice/vo_cs1_03.wav` |
| `vo_cs1_04` | Protagonist (inner voice) | "The source of this lies in the cave." | Young, quiet determination, said to themselves. | `Assets/Resources/Audio/Voice/vo_cs1_04.wav` |

### Cutscene 2 — The Orb (end of Phase 1)

Plays when the player touches the Orb. Script: `Assets/Data/Cutscenes/cs2_orb.json`.

| id | Speaker | Line | Direction | File |
|---|---|---|---|---|
| `vo_cs2_01` | Protagonist (inner voice) | "I remember… a throne of light." | Dreamy, distant, remembering. Long pause on the '…'. | `Assets/Resources/Audio/Voice/vo_cs2_01.wav` |
| `vo_cs2_02` | Protagonist (inner voice) | "I remember… something I cast away." | Uneasy, softer — something they regret. | `Assets/Resources/Audio/Voice/vo_cs2_02.wav` |
| `vo_cs2_03` | Protagonist (inner voice) | "Its home is the castle beyond the cave." | Resolved, clearer: now they know where to go. | `Assets/Resources/Audio/Voice/vo_cs2_03.wav` |
| `vo_cs2_04` | Zulmat (unseen, shown as '???') | "…You remember. Then I am awake." | Deep, slow, amused menace. Unseen. Could be layered/pitched down. | `Assets/Resources/Audio/Voice/vo_cs2_04.wav` |
| `vo_cs2_05` | Narrator | "The light behind the screen changed. So did everything on it." | Narrator, hushed awe. | `Assets/Resources/Audio/Voice/vo_cs2_05.wav` |

### Cutscene 3 — Ending (after Zulmat)

Plays after the final boss. Script: `Assets/Data/Cutscenes/cs3_ending.json`.

| id | Speaker | Line | Direction | File |
|---|---|---|---|---|
| `vo_cs3_01` | Zulmat | "Look at me. You know my face." | Zulmat, tired and bitter rather than angry. | `Assets/Resources/Audio/Voice/vo_cs3_01.wav` |
| `vo_cs3_02` | Protagonist (inner voice) | "He is… me." | Protagonist, shocked whisper. Break on the '…'. | `Assets/Resources/Audio/Voice/vo_cs3_02.wav` |
| `vo_cs3_03` | Protagonist (inner voice) | "I was a god." | Protagonist, steady, a confession. | `Assets/Resources/Audio/Voice/vo_cs3_03.wav` |
| `vo_cs3_04` | Protagonist (inner voice) | "I tore out my cruelty, and it became him." | Harder, painful — 'him' with weight. | `Assets/Resources/Audio/Voice/vo_cs3_04.wav` |
| `vo_cs3_05` | Protagonist (inner voice) | "I tore out my mercy, and it became me." | Quiet, final. | `Assets/Resources/Audio/Voice/vo_cs3_05.wav` |
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
