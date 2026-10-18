"""Original trailer score for Cinders of Fate, synthesized from scratch (no samples, no third-party music).

    python Trailer/tools/make_score.py      ->  Trailer/music/cinders_trailer_score.wav  (2:48, mono, 44.1 kHz)

Cues line up with the scene times in Trailer/trailer.html (SCENES). Scale: D Hijaz (D Eb F# G A Bb C), 90 BPM.
"""
import os
import numpy as np
from scipy.signal import lfilter, fftconvolve
from scipy.io import wavfile

SR = 44100
LENGTH = 168.0
BEAT = 60.0 / 90.0
rng = np.random.default_rng(7)

dry = np.zeros(int(SR * LENGTH) + SR * 6)
wet = np.zeros_like(dry)            # reverb send


def add(sig, t0, gain=1.0, send=0.3):
    i = int(t0 * SR)
    if i >= len(dry):
        return
    sig = sig[: len(dry) - i]
    dry[i:i + len(sig)] += sig * gain
    wet[i:i + len(sig)] += sig * gain * send


def tl(n):
    return np.arange(n) / SR


def lowpass(x, cutoff):
    a = np.exp(-2 * np.pi * cutoff / SR)
    return lfilter([1 - a], [1, -a], x)


def highpass(x, cutoff):
    return x - lowpass(x, cutoff)


def saw(freq, n, phase=0.0):
    t = tl(n)
    return 2 * ((freq * t + phase) % 1.0) - 1


HIJAZ = [0, 1, 4, 5, 7, 8, 10]


def note(degree, octave=3, root=0):
    """Scale degree (can exceed 7) -> Hz. D3 = 146.83 Hz; root shifts the key in semitones."""
    o, d = divmod(degree, 7)
    semis = HIJAZ[d] + 12 * (o + octave - 3) + root
    return 146.83 * 2 ** (semis / 12)


# ------------------------------------------------------------------ instruments
def pluck(freq, dur=2.5, bright=0.5, decay=0.996):
    """Karplus-Strong string (koto / oud-like)."""
    n = int(dur * SR)
    N = max(2, int(SR / freq))
    burst = rng.uniform(-1, 1, N)
    burst = lowpass(burst, 1500 + 6000 * bright)
    x = np.zeros(n)
    x[:N] = burst
    a = np.zeros(N + 2)
    a[0] = 1
    a[N] = -decay * 0.5
    a[N + 1] = -decay * 0.5
    y = lfilter([1.0], a, x)
    y *= np.minimum(1, (n - np.arange(n)) / (0.05 * SR))
    return y / (np.abs(y).max() + 1e-9)


def bell(freq, dur=4.0):
    t = tl(int(dur * SR))
    y = np.zeros_like(t)
    for ratio, amp, dec in ((1, 1, 1.6), (2.76, 0.5, 0.9), (5.4, 0.25, 0.5), (8.93, 0.12, 0.3)):
        y += amp * np.sin(2 * np.pi * freq * ratio * t) * np.exp(-t / dec)
    return y * 0.5


def taiko(pitch=1.0, dur=1.2):
    t = tl(int(dur * SR))
    f = 55 * pitch + 90 * pitch * np.exp(-t * 18)
    body = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t * 4.5)
    click = lowpass(rng.uniform(-1, 1, len(t)), 2500) * np.exp(-t * 60)
    return body + 0.5 * click


def braam(freq, dur=5.0):
    """Big low brass-like hit with a sub drop."""
    n = int(dur * SR)
    t = tl(n)
    y = np.zeros(n)
    for mult in (1, 1.5, 2, 3):
        for det in (-0.4, 0.0, 0.5):
            y += saw(freq * mult + det, n, rng.random()) / mult
    y = lowpass(lowpass(y, 900), 1400)
    env = np.minimum(1, t / 0.03) * np.exp(-t / 1.8)
    sub_f = freq / 2 * (1 + 0.6 * np.exp(-t * 6))
    sub = np.sin(2 * np.pi * np.cumsum(sub_f) / SR) * np.exp(-t / 2.2)
    hit = lowpass(rng.uniform(-1, 1, n), 4000) * np.exp(-t * 25)
    return 0.35 * y * env + 0.9 * sub + 0.4 * hit


def pad(freqs, dur, attack=2.0, release=2.0, cutoff=700):
    n = int(dur * SR)
    t = tl(n)
    y = np.zeros(n)
    for f in freqs:
        for det in (-0.25, 0.0, 0.3):
            vib = 1 + 0.002 * np.sin(2 * np.pi * 4.8 * t + rng.random() * 6)
            y += saw(f * vib + det, n, rng.random())
    y = lowpass(lowpass(y, cutoff), cutoff * 1.6)
    env = np.minimum(1, t / attack) * np.minimum(1, (dur - t) / release)
    return y * np.clip(env, 0, 1) / (len(freqs) * 3)


def choir(freqs, dur, attack=1.5, release=2.0):
    """Additive 'ah' voices: harmonics shaped by vowel formants, with vibrato."""
    n = int(dur * SR)
    t = tl(n)
    y = np.zeros(n)
    formants = ((700, 1.0, 130), (1220, 0.5, 120), (2600, 0.25, 160))
    for f0 in freqs:
        for voice in range(3):
            vib = 1 + 0.006 * np.sin(2 * np.pi * (5 + voice * 0.3) * t + voice)
            ph = 2 * np.pi * np.cumsum(f0 * (1 + (voice - 1) * 0.003) * vib) / SR
            for h in range(1, 18):
                fh = f0 * h
                amp = sum(a * np.exp(-((fh - fc) / bw) ** 2 / 2) for fc, a, bw in formants) + 0.08 / h
                y += amp * np.sin(h * ph)
    env = np.minimum(1, t / attack) * np.minimum(1, (dur - t) / release)
    return y * np.clip(env, 0, 1) / (len(freqs) * 6)


def riser(dur):
    n = int(dur * SR)
    t = tl(n)
    u = t / dur
    noise = highpass(rng.uniform(-1, 1, n), 1500 + 3000 * 0) * u ** 3
    tone = np.sin(2 * np.pi * np.cumsum(180 + 900 * u ** 2) / SR) * u ** 2 * 0.4
    return noise * 0.6 + tone


def crackle(dur, density=6.0):
    """Fire crackle: sparse random pops over a low rumble."""
    n = int(dur * SR)
    y = np.zeros(n)
    count = int(dur * density)
    for i in rng.integers(0, n - 2000, count):
        k = rng.integers(60, 400)
        y[i:i + k] += rng.uniform(-1, 1, k) * np.exp(-np.arange(k) / (k / 5)) * rng.uniform(0.2, 1)
    y = highpass(y, 1800)
    rumble = lowpass(rng.uniform(-1, 1, n), 120) * 3
    return y * 0.5 + rumble * 0.25


def heartbeat(t0, count, gap=1.0):
    for k in range(count):
        add(taiko(0.55, 0.8), t0 + k * gap, 0.55, 0.1)
        add(taiko(0.5, 0.8), t0 + k * gap + 0.28, 0.4, 0.1)


# ------------------------------------------------------------------ the cue sheet (times match SCENES in trailer.html)
# 0–30  Fire, drone, music-box village theme
add(crackle(LENGTH - 2, 5), 0, 0.3, 0.05)
add(pad([note(0, 2), note(4, 2)], 30, attack=6, release=3, cutoff=500), 0, 0.4)
theme = [(0, 4), (0.5, 5), (1, 4), (2, 6), (3, 4), (4, 7), (5, 6), (6, 5), (8, 4)]
for start in (14.0,):
    for k, (b, deg) in enumerate(theme):
        add(pluck(note(deg, 5), 2.5, 0.7), start + b * BEAT * 2, 0.35, 0.45)
for k, deg in enumerate((7, 8, 7, 5, 4, 5, 4, 1, 0)):            # 'Then the shadows began to move' — lower, darker
    add(pluck(note(deg, 4), 2.5, 0.4), 22.0 + k * BEAT, 0.35, 0.4)
add(riser(3.0), 27.0, 0.35, 0.3)

# 30–36  Title tease: hit, ring, silence
add(braam(note(0, 2)), 30.0, 0.9, 0.35)
add(bell(note(0, 5), 5), 30.0, 0.25, 0.6)

# 36–90  Build: ostinato -> taiko at 52 -> full at 70 -> riser into the memory
ost = [0, 0, 4, 0, 5, 4, 1, 0]
t = 36.0
i = 0
while t < 89.8:
    oct_ = 3 if t < 70 else 3
    add(pluck(note(ost[i % 8], oct_), 0.9, 0.35 + 0.3 * (t > 70)), t, 0.32 if t < 52 else 0.4, 0.15)
    t += BEAT / 2
    i += 1
add(pad([note(0, 2), note(4, 2), note(7, 2)], 54, attack=8, release=1, cutoff=600), 36.0, 0.6)
beat_t = 52.0
k = 0
while beat_t < 89.8:
    if beat_t < 70:
        if k % 4 in (0, 2):
            add(taiko(1.0), beat_t, 0.7, 0.2)
    else:
        add(taiko(1.0 if k % 2 == 0 else 1.3), beat_t, 0.75, 0.2)
        if k % 4 == 3:
            add(taiko(1.6, 0.6), beat_t + BEAT / 2, 0.45, 0.2)
    beat_t += BEAT
    k += 1
add(choir([note(0, 3), note(4, 3)], 20, attack=6, release=1.5), 70.0, 0.55, 0.5)
for bar in np.arange(70.0, 86.0, BEAT * 4):
    add(braam(note(0, 2), 2.5), bar, 0.35, 0.3)
add(riser(4.0), 86.0, 0.5, 0.3)

# 90–104  Memory: drums gone, bells in a big room, dread note at 100, white flash at 104
for k, deg in enumerate((7, 9, 11, 9, 7, 8, 7, 4)):
    add(bell(note(deg, 5), 5), 90.0 + k * 1.15, 0.22, 0.8)
add(pad([note(0, 3), note(1, 3), note(4, 3)], 14, attack=4, release=1, cutoff=900), 90.0, 0.5, 0.6)
add(braam(note(1, 1), 4), 100.0, 0.45, 0.4)
add(riser(3.5), 100.5, 0.6, 0.35)

# 104–132  Inversion: huge hit, key moves up a whole step (E), 16th ostinato, heavier drums, choir
add(braam(note(0, 2, 2), 6), 104.0, 1.0, 0.4)
add(bell(note(0, 5, 2), 5), 104.0, 0.3, 0.7)
t = 106.0
i = 0
while t < 131.8:
    add(pluck(note([0, 4, 0, 5, 0, 4, 1, 4][i % 8], 3, 2), 0.6, 0.6), t, 0.3, 0.12)
    t += BEAT / 4
    i += 1
beat_t = 106.0
k = 0
while beat_t < 131.8:
    add(taiko(1.0 if k % 2 == 0 else 1.25), beat_t, 0.8, 0.2)
    if k % 2 == 1:
        add(taiko(1.5, 0.5), beat_t + BEAT / 2, 0.4, 0.2)
    beat_t += BEAT
    k += 1
add(choir([note(0, 3, 2), note(4, 3, 2), note(7, 3, 2)], 26, attack=3, release=2), 106.0, 0.6, 0.5)
for bar in np.arange(106.0, 130.0, BEAT * 8):
    add(braam(note(0, 2, 2), 3), bar, 0.4, 0.3)

# 132–140  Drop: rumble + heartbeat, Zulmat's eyes open at 136
heartbeat(132.0, 8, 1.0)
add(braam(note(1, 1, 2), 5), 136.0, 0.7, 0.4)
add(riser(2.5), 137.5, 0.55, 0.3)

# 140–152  Climax: double-time drums, choir, a hit every bar
beat_t = 140.0
k = 0
while beat_t < 151.8:
    add(taiko(1.0 if k % 2 == 0 else 1.35), beat_t, 0.85, 0.2)
    add(taiko(1.7, 0.4), beat_t + BEAT / 2, 0.45, 0.15)
    beat_t += BEAT
    k += 1
t = 140.0
i = 0
while t < 151.8:
    add(pluck(note([0, 4, 7, 4, 8, 7, 4, 1][i % 8], 4, 2), 0.5, 0.8), t, 0.28, 0.12)
    t += BEAT / 4
    i += 1
add(choir([note(0, 3, 2), note(5, 3, 2), note(8, 3, 2)], 12, attack=0.8, release=0.6), 140.0, 0.7, 0.5)
for bar in np.arange(140.0, 150.0, BEAT * 4):
    add(braam(note(0, 2, 2), 2.6), bar, 0.5, 0.3)
add(riser(2.0), 150.0, 0.6, 0.3)

# 152–158  'He is… me.' — the mix is cut to near silence below; its one low note and string are added after that cut
quiet_cue = [(pad([note(0, 2)], 6, attack=0.5, release=2.5, cutoff=300), 152.0, 0.15),
             (pluck(note(4, 4), 3.5, 0.3), 154.5, 0.22)]

# 158–168  Title: final hit, bells, theme fragment, fade into fire
add(braam(note(0, 2), 8), 158.0, 1.0, 0.45)
add(choir([note(0, 3), note(4, 3), note(7, 3)], 9, attack=0.3, release=5), 158.0, 0.5, 0.6)
for k, (b, deg) in enumerate([(0, 7), (1, 8), (2, 7), (3, 4), (4.5, 7)]):
    add(pluck(note(deg, 5), 3, 0.7), 159.5 + b * BEAT, 0.3, 0.6)

# ------------------------------------------------------------------ mix
ir_n = int(2.8 * SR)
ir = rng.uniform(-1, 1, ir_n) * np.exp(-tl(ir_n) / 0.7)
ir = lowpass(ir, 5000)
ir /= np.sqrt((ir ** 2).sum())
rev = fftconvolve(wet, ir)[: len(dry)]
mix = dry + rev * 0.9
mix = mix[: int(SR * LENGTH)]
# Hard cut for the 'He is… me.' beat: climax tails drop to 8 % at 152.0 s and recover just before the 158 s hit.
duck = np.ones(len(mix))
a, b = int(152.0 * SR), int(157.9 * SR)
duck[a:b] = 0.08
duck[a - int(0.05 * SR):a] = np.linspace(1, 0.08, int(0.05 * SR))
mix *= duck
for sig, t0, g in quiet_cue:
    i = int(t0 * SR)
    seg = sig[: len(mix) - i] * g
    tail = fftconvolve(seg, ir)[: len(seg) + int(2 * SR)][: len(mix) - i] * 0.6
    mix[i:i + len(seg)] += seg
    mix[i:i + len(tail)] += tail
fade = int(3.5 * SR)                                    # fade out under the end card
mix[-fade:] *= np.linspace(1, 0, fade) ** 1.5
mix[: int(0.5 * SR)] *= np.linspace(0, 1, int(0.5 * SR))
mix /= np.abs(mix).max()
mix = np.tanh(mix * 1.6) / np.tanh(1.6) * 0.89          # gentle limiter, peak ≈ −1 dBFS

out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "music", "cinders_trailer_score.wav")
os.makedirs(os.path.dirname(out), exist_ok=True)
wavfile.write(out, SR, (mix * 32767).astype(np.int16))
print("wrote", os.path.normpath(out), f"{len(mix) / SR:.1f}s")
