"""Generates every sound effect and music track the game asks for and installs them where AudioManager loads them.

    python Tools/audiogen/build_audio.py            # everything
    python Tools/audiogen/build_audio.py sfx        # or: music, voices

sfx   -> Assets/Resources/Audio/<id>.wav          (all ids listed in Docs/AUDIO_HOOKS.md except music_*)
music -> Assets/Resources/Audio/Music/<id>.wav    (seamless loops)
voices-> Assets/Resources/Audio/Voice/<id>.wav    (cut from the recorded takes, see voices.py)
Fails if any id in Docs/AUDIO_HOOKS.md has no file afterwards.
"""
import os
import re
import sys
import time
import numpy as np
from scipy.io import wavfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from synth import SR, finish  # noqa: E402

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
AUDIO = os.path.join(ROOT, "Assets", "Resources", "Audio")


def write(path, x):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    wavfile.write(path, SR, (np.clip(x, -1, 1) * 32767).astype(np.int16))


def hook_ids():
    text = open(os.path.join(ROOT, "Docs", "AUDIO_HOOKS.md"), encoding="utf-8").read()
    return set(re.findall(r"^\| `([a-z0-9_]+)` \|", text, re.M))


def build_sfx():
    import sfx
    for name, (fn, peak) in sorted(sfx.R.items()):
        write(os.path.join(AUDIO, name + ".wav"), finish(fn(), peak))
    return len(sfx.R)


def build_music():
    import music
    for name, fn in sorted(music.TRACKS.items()):
        x = fn()
        write(os.path.join(AUDIO, "Music", name + ".wav"), x)
        print(f"  {name:18s} {len(x) / SR:5.1f}s loop")
    return len(music.TRACKS)


def build_voices():
    import voices
    voices.main()
    return len(voices.LINES)


def verify():
    missing = []
    for i in sorted(hook_ids()):
        folder = os.path.join(AUDIO, "Music") if i.startswith("music_") else AUDIO
        if not os.path.exists(os.path.join(folder, i + ".wav")):
            missing.append(i)
    return missing


def main(argv):
    jobs = {"sfx": build_sfx, "music": build_music, "voices": build_voices}
    for name in argv or ["sfx", "music", "voices"]:
        t = time.time()
        count = jobs[name]()
        print(f"{name:7s} {count:3d} files  {time.time() - t:5.1f}s")
    missing = verify()
    print(f"hooks: {len(hook_ids())}, missing: {len(missing)} {missing}")
    return 1 if missing else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
