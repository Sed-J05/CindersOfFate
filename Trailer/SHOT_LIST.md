# Trailer — shot list & how to finish it

**Length: 2:48** (168 s). A timed presentation with an original score. Seven slots need gameplay footage; until a clip exists,
its slot shows an orange dashed box with these instructions.

## Files

| Path | What |
|---|---|
| `Trailer/index.html` | The trailer. Open it in Chrome or Edge and press **Play**. |
| `Trailer/trailer.html` | Same page without the HTML wrapper (the source; `index.html` is built from it). |
| `Trailer/music/cinders_trailer_score.wav` | The score: 2:48, made from scratch by `Trailer/tools/make_score.py`, so there's no copyright issue. |
| `Trailer/assets/` | Game art used by the slides (copied from `Assets/Art`). |
| `Trailer/clips/` | **Put your gameplay clips here.** |

After editing `trailer.html`, run `python Trailer/tools/make_local.py` to rebuild `index.html`.
To change the music, edit and run `python Trailer/tools/make_score.py`.

## The 7 clips

Record at **1920×1080, 60 fps**, MP4 (H.264). The trailer mutes clips, so sound doesn't matter. Trim each file to exactly the
length shown and name it as listed. Turn on **F1 god mode** so nothing interrupts a take, and keep **F7 hitbox gizmos off**.

| File | Plays at | Length | Where to record | What to show |
|---|---|---|---|---|
| `clip01.mp4` | 0:44–0:52 | **8 s** | Phase1_Cave · P1_A0 *Cave Mouth* → P1_A1 *The First Crawlers* | Run right, jump between ledges and one-way platforms, the flame glow on the paper. No fighting. |
| `clip02.mp4` | 0:58–1:08 | **10 s** | Phase1_Cave · P1_A4 *Two Tiers* or P1_B1 *Paper Wings* | Side and up slashes on Crawlers, Lungers and Shadow Kites: hit sparks, soul filling up, finish with a heal (**F2** refills soul). |
| `clip03.mp4` | 1:16–1:24 | **8 s** | Phase1_Cave · P1_MB1 *The Warden Gate* (**F6** goes to the current boss arena) | Kage Warden's red telegraph, a dodge, a punish, one blocked hit (clink), then a combo. |
| `clip04.mp4` | 1:24–1:30 | **6 s** | Phase1_Cave · P1_MB2 *The Caller Hall* | Kage Caller's black slashes flying, a summon, end on a clean hit. |
| `clip05.mp4` | 1:58–2:06 | **8 s** | Phase2_Castle · P2_C1 *Chandelier Hall* → P2_C2 *The Tower Climb* (open Phase2_Castle, or **F4** in Phase 1) | Time Slip dash (**L**) through an enemy, a Flamecall fireball (**K**) into a Knight or Bat, fast climbing. |
| `clip06.mp4` | 2:06–2:12 | **6 s** | Phase2_Castle · P2_MB *The Colossus Hall* (**F6**) | Jump-slam shockwave, a shield clink, the player dashing over it. |
| `clip07.mp4` | 2:20–2:32 | **12 s** | Phase2_Castle · P2_THRONE *The Throne of Zulmat* (**F6** after the Colossus) | Three 4-second moments cut together: vines + giant swipe, shrink & reposition, lava + the final hits. |

**How to record:** OBS Studio (free): Display Capture, 1920×1080, 60 fps, MP4. Or the Windows Game Bar: **Win + Alt + R** to
start and stop, which saves to `Videos\Captures`. Record a longer take, then trim it to the exact length in the Windows
Photos/Clipchamp editor.

## Making the final video

1. Put the 7 clips in `Trailer/clips/`.
2. Open `Trailer/index.html` in Chrome, press **F** for fullscreen, then **Play**. The controls fade out after 2 seconds
   (**H** hides them completely).
3. Record the screen with OBS at 1920×1080 60 fps, **including desktop audio** (that's the score).
4. Trim the start and end. That recording is the trailer.

Keys: **Space** play/pause · **← / →** ±5 s · **R** restart · **G** timecode guide (shows the time and scene, handy while
lining up clips) · **F** fullscreen · **H** hide controls. A link ending in `#t95` opens paused at 95 s.

## Timeline

| Time | Scene | Music |
|---|---|---|
| 0:00 | "Every shadow is cast by a flame." | Fire crackle, low drone |
| 0:08 | Team Cinders of Fate presents | |
| 0:14 | Village parallax: "Our village slept beneath the mountain's shadow." | Music-box village theme |
| 0:22 | Crawler and Lunger cross the village: "Then the shadows began to move." | Darker theme, riser |
| 0:30 | **Title tease** (logo + flash) | Big hit |
| 0:36 | Slide: *A world of paper and flame* | String ostinato starts |
| 0:44 | **Clip 01**: Phase I, the cave (8 s) | |
| 0:52 | Slide: *Read it. Dodge it. Cut through it.* | Drums join |
| 0:58 | **Clip 02**: combat (10 s) | |
| 1:08 | Roster: Crawler, Lunger, Shadow Kite, Kage Caller, Kage Warden | Full drums, choir |
| 1:16 | **Clip 03**: Kage Warden (8 s) | |
| 1:24 | **Clip 04**: Kage Caller (6 s) | Riser |
| 1:30 | Memory flashes: "I remember… a throne of light." | Drums drop out, bells |
| 1:40 | Orb glow builds to white | Dread note, riser |
| 1:44 | **Inversion**: *The light changed.* Black-red hero | Huge hit, key change |
| 1:52 | Slide: *New powers*: Time Slip, Flamecall | Driving 16ths |
| 1:58 | **Clip 05**: Phase II, the castle (8 s) | |
| 2:06 | **Clip 06**: Iron Colossus (6 s) | |
| 2:12 | Zulmat's eyes: "…You remember. Then I am awake." | Heartbeat, sting at 2:16 |
| 2:20 | **Clip 07**: Zulmat (12 s) | Climax |
| 2:32 | "He is… me." | Near silence |
| 2:38 | **End card**: logo, *Two halves. One flame.*, Windows PC | Final hit, fade |
| 2:48 | End | |

Edit any text in `trailer.html` (search for the line), then rebuild `index.html`.
