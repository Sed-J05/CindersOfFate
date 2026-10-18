"""The game's eight music tracks (D Hijaz, 90 BPM, same instruments as the trailer score). Every track loops seamlessly:
it is rendered with extra tail time, and that tail is folded back onto the start so notes and reverb carry over the loop."""
import numpy as np
from synth import (SR, lowpass, pad, choir, pluck, bell, taiko, braam, crackle, noise, place, note, reverb, n_, env_ar,
                   rng, fftconvolve, t_)

BEAT = 60.0 / 90.0
BAR = BEAT * 4
TAIL = 4.0


class Track:
    def __init__(self, bars):
        self.length = bars * BAR
        self.dry = np.zeros(n_(self.length + TAIL))
        self.wet = np.zeros_like(self.dry)

    def add(self, sig, t0, gain=1.0, send=0.3):
        place(self.dry, sig, t0, gain)
        place(self.wet, sig, t0, gain * send)

    def render(self, rms_db=-22.0, room=2.4):
        n = n_(room)
        ir = lowpass(rng.uniform(-1, 1, n) * np.exp(-t_(n) / (room / 4)), 5000)
        ir /= np.sqrt((ir ** 2).sum())
        full = self.dry + fftconvolve(self.wet, ir)[: len(self.dry)] * 0.9
        L = n_(self.length)
        out = full[:L].copy()
        tail = full[L:]
        out[: len(tail)] += tail[: L]                       # wrap the tail: seamless loop
        out *= 10 ** (rms_db / 20) / (np.sqrt((out ** 2).mean()) + 1e-12)
        peak = np.abs(out).max()
        if peak > 0.89:
            out = np.tanh(out / 0.89) * 0.89
        return out


def ostinato(tr, start, end, pattern, octave, root, step, gain, bright=0.4, length=0.8):
    t, i = start, 0
    while t < end - 1e-6:
        tr.add(pluck(note(pattern[i % len(pattern)], octave, root), length, bright), t, gain, 0.15)
        t += step
        i += 1


def drums(tr, start, end, pattern, gain=0.7):
    """pattern: list of (beat offset in bar, pitch, level) repeated every bar."""
    bar = start
    while bar < end - 1e-6:
        for off, pitch, lvl in pattern:
            if bar + off * BEAT < end:
                tr.add(taiko(pitch, 0.9), bar + off * BEAT, gain * lvl, 0.2)
        bar += BAR


def theme(tr, t0, degrees_beats, octave, root, gain, beat=BEAT * 2):
    for b, deg in degrees_beats:
        tr.add(pluck(note(deg, octave, root), 2.5, 0.65), t0 + b * beat, gain, 0.45)


VILLAGE = [(0, 4), (0.5, 5), (1, 4), (2, 6), (3, 4), (4, 7), (5, 6), (6, 5), (8, 4)]


def music_menu():
    tr = Track(18)
    tr.add(crackle(tr.length, 4), 0, 0.35, 0.05)
    for k in range(3):
        tr.add(pad([note(0, 2), note(4, 2)], 6 * BAR, attack=4, release=4, cutoff=500), k * 6 * BAR, 0.8)
    theme(tr, 2 * BAR, VILLAGE, 5, 0, 0.35)
    theme(tr, 10 * BAR, [(b, d - 1) for b, d in VILLAGE], 5, 0, 0.3)
    return tr.render(-24)


def music_intro():
    tr = Track(12)
    tr.add(crackle(tr.length, 4), 0, 0.3, 0.05)
    tr.add(pad([note(0, 2), note(4, 2)], 6 * BAR, attack=3, release=3, cutoff=550), 0, 0.8)
    tr.add(pad([note(0, 2), note(1, 2), note(4, 2)], 6 * BAR, attack=3, release=3, cutoff=450), 6 * BAR, 0.8)
    theme(tr, 0.5 * BAR, VILLAGE, 5, 0, 0.35)
    for k, deg in enumerate((7, 8, 7, 5, 4, 5, 4, 1, 0)):
        tr.add(pluck(note(deg, 4), 2.5, 0.4), 6.5 * BAR + k * BEAT, 0.35, 0.4)
    ostinato(tr, 8 * BAR, 12 * BAR, [0, 0, 4, 0], 3, 0, BEAT, 0.22, 0.3)
    return tr.render(-23)


def music_phase1():
    tr = Track(24)
    tr.add(crackle(tr.length, 3), 0, 0.25, 0.05)
    for k in range(4):
        chord = [note(0, 2), note(4, 2)] if k % 2 == 0 else [note(5, 1), note(1, 2)]
        tr.add(pad(chord, 6 * BAR, attack=3, release=3, cutoff=450), k * 6 * BAR, 0.75)
    arp = [0, 4, 7, 4, 8, 7, 4, 1]
    ostinato(tr, 4 * BAR, 20 * BAR, arp, 3, 0, BEAT, 0.2, 0.35, 1.2)
    drums(tr, 8 * BAR, 24 * BAR, [(0, 0.8, 0.8), (2.5, 0.9, 0.4)], 0.55)
    for k in range(10):                                         # cave drips
        tr.add(bell(rng.uniform(1800, 3200), 1.5, 0.5), rng.uniform(0, tr.length - 1), 0.08, 0.7)
    theme(tr, 14 * BAR, [(0, 7), (1, 8), (2, 7), (3, 4), (4, 5), (5, 4), (6, 1), (7, 0)], 4, 0, 0.25)
    return tr.render(-23)


def music_boss():
    tr = Track(16)
    ostinato(tr, 0, tr.length, [0, 0, 4, 0, 5, 4, 1, 0], 3, 0, BEAT / 2, 0.35, 0.5, 0.6)
    drums(tr, 0, tr.length, [(0, 1.0, 1.0), (1, 1.3, 0.6), (2, 1.0, 0.9), (3, 1.3, 0.6), (3.5, 1.6, 0.45)], 0.75)
    tr.add(pad([note(0, 2), note(4, 2), note(7, 2)], tr.length, attack=1, release=1, cutoff=650), 0, 0.6)
    tr.add(choir([note(0, 3), note(4, 3)], 8 * BAR, attack=3, release=2), 8 * BAR, 0.5, 0.5)
    for bar in range(0, 16, 4):
        tr.add(braam(note(0, 2), 2.6), bar * BAR, 0.4, 0.3)
    return tr.render(-19)


def music_memory():
    tr = Track(12)
    tr.add(pad([note(0, 3), note(1, 3), note(4, 3)], tr.length, attack=4, release=4, cutoff=900), 0, 0.5, 0.6)
    for k, deg in enumerate((7, 9, 11, 9, 7, 8, 7, 4, 7, 9, 12, 11, 9, 8, 7, 4)):
        tr.add(bell(note(deg, 5), 4, 1.4), k * BAR * 0.75, 0.2, 0.8)
    tr.add(braam(note(1, 1), 4), 9 * BAR, 0.35, 0.4)
    return tr.render(-23, room=3.2)


def music_phase2():
    R = 2                                                       # E — a whole step above Phase 1
    tr = Track(24)
    for k in range(4):
        chord = [note(0, 3, R), note(4, 3, R), note(7, 3, R)] if k % 2 == 0 else [note(5, 2, R), note(1, 3, R), note(5, 3, R)]
        tr.add(choir(chord, 6 * BAR, attack=3, release=2.5), k * 6 * BAR, 0.45, 0.5)
        tr.add(pad(chord[:2], 6 * BAR, attack=2, release=2, cutoff=600), k * 6 * BAR, 0.5)
    ostinato(tr, 4 * BAR, 24 * BAR, [0, 4, 0, 5, 0, 4, 1, 4], 3, R, BEAT / 2, 0.2, 0.5, 0.5)
    drums(tr, 8 * BAR, 24 * BAR, [(0, 1.0, 0.8), (2, 1.2, 0.5), (3.5, 1.4, 0.3)], 0.55)
    return tr.render(-22)


def music_final_boss():
    R = 2
    tr = Track(16)
    ostinato(tr, 0, tr.length, [0, 4, 7, 4, 8, 7, 4, 1], 4, R, BEAT / 4, 0.25, 0.75, 0.45)
    drums(tr, 0, tr.length, [(o, 1.0 if int(o * 2) % 2 == 0 else 1.6, 1.0 if o == int(o) else 0.5) for o in np.arange(0, 4, 0.5)], 0.75)
    tr.add(choir([note(0, 3, R), note(5, 3, R), note(8, 3, R)], tr.length, attack=1, release=1), 0, 0.6, 0.5)
    tr.add(pad([note(0, 2, R), note(1, 2, R)], tr.length, attack=1, release=1, cutoff=700), 0, 0.5)
    for bar in range(0, 16, 2):
        tr.add(braam(note(0, 2, R), 2.6), bar * BAR, 0.45, 0.3)
    return tr.render(-18)


def music_ending():
    tr = Track(15)
    tr.add(crackle(tr.length, 3), 0, 0.25, 0.05)
    tr.add(choir([note(0, 3), note(4, 3), note(7, 3)], 7 * BAR, attack=3, release=3), 0, 0.45, 0.6)
    tr.add(choir([note(5, 2), note(0, 3), note(4, 3)], 8 * BAR, attack=3, release=3), 7 * BAR, 0.45, 0.6)
    tr.add(pad([note(0, 2), note(4, 2)], tr.length, attack=3, release=3, cutoff=500), 0, 0.6)
    theme(tr, 1 * BAR, VILLAGE, 4, 0, 0.32, beat=BEAT * 3)
    theme(tr, 9 * BAR, [(0, 7), (1, 8), (2, 7), (3, 4), (4.5, 7)], 5, 0, 0.3, beat=BEAT * 2)
    return tr.render(-23, room=3.0)


TRACKS = {name: fn for name, fn in globals().items() if name.startswith("music_") and callable(fn)}
