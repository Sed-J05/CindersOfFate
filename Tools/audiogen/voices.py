"""Cuts the recorded voice takes into one clip per line and installs them for the game.

    python Tools/audiogen/voices.py [source_folder]     (default: C:/Users/saadj/Downloads/vlines)

cut1.wav = every Narrator line in script order, cut2.wav = the Gods' two lines (matched by pauses and syllable counts,
D-185). Each clip is cut at the quietest point between lines, trimmed, faded, and levelled to the same loudness.
The Gods get a light doubled, echoing treatment ("three voices in unison"). Output: Assets/Resources/Audio/Voice/<id>.wav
"""
import os
import sys
import numpy as np
from scipy.io import wavfile
from scipy.signal import fftconvolve, resample

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "Resources", "Audio", "Voice")
SRC = sys.argv[1] if len(sys.argv) > 1 else r"C:\Users\saadj\Downloads\vlines"

# (source file, approximate start, approximate end, id, treatment). Boundaries sit inside the pauses found in the takes.
LINES = [
    ("cut1.wav", 0.00, 1.35, "vo_cs1_01", "narrator"),       # "…again."
    ("cut1.wav", 1.35, 4.54, "vo_cs1_02", "narrator"),       # "Our village slept beneath the mountain's shadow."
    ("cut1.wav", 4.54, 7.76, "vo_cs1_03", "narrator"),       # "Then the shadows began to move."
    ("cut1.wav", 7.76, 12.09, "vo_cs2_05", "narrator"),      # "The light behind the screen changed. So did everything on it."
    ("cut1.wav", 12.09, 17.48, "vo_cs3_08", "narrator"),     # "Two halves, each too weak to resist. Their memories locked away."
    ("cut1.wav", 17.48, None, "vo_extinguished", "narrator"),  # "You were extinguished"
    ("cut2.wav", 0.00, 2.70, "vo_cs3_06", "gods"),           # "Whole, you are too much."
    ("cut2.wav", 2.70, None, "vo_cs3_07", "gods"),           # "Be halved again."
]

TARGET_RMS_DB = -18.0      # loudness of the spoken part
PEAK_DB = -1.0


def load(path):
    sr, x = wavfile.read(path)
    x = x.astype(np.float64)
    if x.ndim > 1:
        x = x.mean(axis=1)
    x /= 32768.0 if np.abs(x).max() > 1.5 else 1.0
    return sr, x


def envelope(x, sr, win=0.01):
    hop = max(1, int(sr * win))
    n = len(x) // hop
    return np.sqrt((x[: n * hop].reshape(n, hop) ** 2).mean(1) + 1e-12), hop


def quietest(x, sr, t, radius=0.25):
    """Moves a cut point to the quietest 10 ms window within ±radius seconds."""
    env, hop = envelope(x, sr)
    a = max(0, int((t - radius) * sr / hop))
    b = min(len(env), int((t + radius) * sr / hop) + 1)
    if b <= a:
        return t
    return (a + int(np.argmin(env[a:b]))) * hop / sr


def trim(x, sr, pad=0.05):
    env, hop = envelope(x, sr)
    db = 20 * np.log10(env)
    thr = max(db.max() - 40, np.percentile(db, 5) + 10)
    on = np.where(db > thr)[0]
    if len(on) == 0:
        return x
    s = max(0, on[0] * hop - int(pad * sr))
    e = min(len(x), (on[-1] + 1) * hop + int(pad * 1.6 * sr))
    return x[s:e]


def fades(x, sr, fin=0.01, fout=0.06):
    a, b = int(fin * sr), int(fout * sr)
    x = x.copy()
    x[:a] *= np.linspace(0, 1, a)
    x[-b:] *= np.linspace(1, 0, b) ** 2
    return x


def level(x, sr):
    env, _ = envelope(x, sr, 0.05)
    speech = env[env > env.max() * 0.1]
    rms = np.sqrt((speech ** 2).mean()) if len(speech) else np.sqrt((x ** 2).mean())
    x = x * (10 ** (TARGET_RMS_DB / 20) / rms)
    peak = np.abs(x).max()
    lim = 10 ** (PEAK_DB / 20)
    if peak > lim:                                   # soft-knee limiter on the few loud syllables
        x = np.tanh(x / lim) * lim
    return x


def gods(x, sr):
    """Three voices in unison: two slightly detuned, delayed copies plus a cold hall."""
    out = x.copy()
    for ratio, delay, gain in ((1.025, 0.021, 0.38), (0.975, 0.034, 0.34)):
        y = resample(x, int(len(x) / ratio))
        d = int(delay * sr)
        buf = np.zeros(len(x) + d + len(y))
        buf[d:d + len(y)] = y
        out = np.pad(out, (0, max(0, len(buf) - len(out))))
        out[: len(buf)] += buf[: len(out)] * gain
    n = int(1.6 * sr)
    rng = np.random.default_rng(3)
    ir = rng.uniform(-1, 1, n) * np.exp(-np.arange(n) / (0.45 * sr))
    ir /= np.sqrt((ir ** 2).sum())
    wet = fftconvolve(out, ir)[: len(out) + int(1.0 * sr)]
    out = np.pad(out, (0, len(wet) - len(out)))
    return out + wet * 0.22


def main():
    os.makedirs(OUT, exist_ok=True)
    cache = {}
    for i, (src, t0, t1, vid, kind) in enumerate(LINES):
        if src not in cache:
            cache[src] = load(os.path.join(SRC, src))
        sr, x = cache[src]
        s = 0.0 if t0 == 0 else quietest(x, sr, t0)
        e = len(x) / sr if t1 is None else quietest(x, sr, t1)
        clip = trim(x[int(s * sr):int(e * sr)], sr)
        if kind == "gods":
            clip = gods(clip, sr)
            clip = trim(clip, sr, pad=0.05)
        clip = level(fades(clip, sr), sr)
        wavfile.write(os.path.join(OUT, vid + ".wav"), sr, (np.clip(clip, -1, 1) * 32767).astype(np.int16))
        print(f"{vid:16s} {len(clip) / sr:5.2f}s  (from {src} {s:5.2f}-{e:5.2f})")


if __name__ == "__main__":
    main()
