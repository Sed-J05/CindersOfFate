"""Tiny synthesis toolkit shared by the SFX and music generators (numpy + scipy, mono, 44.1 kHz)."""
import numpy as np
from scipy.signal import lfilter, butter, sosfilt, fftconvolve

SR = 44100
rng = np.random.default_rng(11)


def n_(dur):
    return max(1, int(dur * SR))


def t_(n):
    return np.arange(n) / SR


def noise(dur):
    return rng.uniform(-1, 1, n_(dur))


def lowpass(x, cutoff):
    a = np.exp(-2 * np.pi * cutoff / SR)
    return lfilter([1 - a], [1, -a], x)


def highpass(x, cutoff):
    return x - lowpass(x, cutoff)


def band(x, lo, hi, order=2):
    sos = butter(order, [max(20, lo), min(SR / 2 - 100, hi)], btype="band", fs=SR, output="sos")
    return sosfilt(sos, x)


def env_exp(n, decay, attack=0.002):
    t = t_(n)
    return np.minimum(1, t / max(attack, 1e-4)) * np.exp(-t / decay)


def env_ar(n, attack, release):
    t = t_(n)
    dur = n / SR
    return np.clip(np.minimum(t / max(attack, 1e-4), (dur - t) / max(release, 1e-4)), 0, 1)


def sweep(f0, f1, dur, shape=1.0):
    n = n_(dur)
    u = np.linspace(0, 1, n) ** shape
    f = f0 + (f1 - f0) * u
    return np.sin(2 * np.pi * np.cumsum(f) / SR)


def saw(freq, n, phase=0.0):
    if np.isscalar(freq):
        return 2 * ((freq * t_(n) + phase) % 1.0) - 1
    return 2 * ((np.cumsum(freq) / SR + phase) % 1.0) - 1


def thump(f0, f1, dur, decay):
    n = n_(dur)
    t = t_(n)
    f = f1 + (f0 - f1) * np.exp(-t * 25)
    return np.sin(2 * np.pi * np.cumsum(f) / SR) * env_exp(n, decay)


def whoosh(dur, f_a, f_b, peak=0.5, bands=7):
    """Filtered noise whose brightness glides from f_a to f_b (either direction); loudest at `peak` (0..1)."""
    n = n_(dur)
    u = np.linspace(0, 1, n)
    centre = np.exp(np.log(f_a) + (np.log(f_b) - np.log(f_a)) * u)
    x = noise(dur)
    out = np.zeros(n)
    lo_f, hi_f = min(f_a, f_b) * 0.7, max(f_a, f_b) * 1.4
    edges = np.geomspace(lo_f, min(hi_f, SR / 2 - 200), bands + 1)
    for lo, hi in zip(edges, edges[1:]):
        mid = np.sqrt(lo * hi)
        w = np.exp(-((np.log(centre) - np.log(mid)) ** 2) / 0.3)
        out += band(x, lo, hi) * w
    p = min(max(peak, 0.02), 0.98)
    amp = np.where(u < p, (u / p) ** 1.2, ((1 - u) / (1 - p)) ** 1.5)
    return out * amp


def metal(freq, dur, decay, ratios=(1, 2.76, 5.4, 8.93, 13.3), bright=1.0):
    t = t_(n_(dur))
    y = np.zeros_like(t)
    for k, r in enumerate(ratios):
        y += (0.8 ** k) * np.sin(2 * np.pi * freq * r * t + rng.random() * 6) * np.exp(-t / (decay / (1 + k * 0.6 / bright)))
    return y


def bell(freq, dur=3.0, decay=1.2):
    t = t_(n_(dur))
    y = np.zeros_like(t)
    for ratio, amp, dec in ((1, 1, 1.0), (2.76, 0.5, 0.55), (5.4, 0.25, 0.3), (8.93, 0.12, 0.18)):
        y += amp * np.sin(2 * np.pi * freq * ratio * t) * np.exp(-t / (decay * dec))
    return y * 0.5


def pluck(freq, dur=1.5, bright=0.5, decay=0.996):
    n = n_(dur)
    N = max(2, int(SR / freq))
    x = np.zeros(n)
    x[:N] = lowpass(rng.uniform(-1, 1, N), 1500 + 6000 * bright)
    a = np.zeros(N + 2)
    a[0] = 1
    a[N] = -decay * 0.5
    a[N + 1] = -decay * 0.5
    y = lfilter([1.0], a, x)
    y *= np.minimum(1, (n - np.arange(n)) / (0.03 * SR))
    return y / (np.abs(y).max() + 1e-9)


def taiko(pitch=1.0, dur=1.0):
    n = n_(dur)
    t = t_(n)
    f = 55 * pitch + 90 * pitch * np.exp(-t * 18)
    body = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t * 4.5)
    click = lowpass(rng.uniform(-1, 1, n), 2500) * np.exp(-t * 60)
    return body + 0.5 * click


def braam(freq, dur=4.0):
    n = n_(dur)
    t = t_(n)
    y = np.zeros(n)
    for mult in (1, 1.5, 2, 3):
        for det in (-0.4, 0.0, 0.5):
            y += saw(freq * mult + det, n, rng.random()) / mult
    y = lowpass(lowpass(y, 900), 1400)
    env = np.minimum(1, t / 0.03) * np.exp(-t / 1.6)
    sub_f = freq / 2 * (1 + 0.6 * np.exp(-t * 6))
    sub = np.sin(2 * np.pi * np.cumsum(sub_f) / SR) * np.exp(-t / 2.0)
    hit = lowpass(rng.uniform(-1, 1, n), 4000) * np.exp(-t * 25)
    return 0.35 * y * env + 0.9 * sub + 0.4 * hit


def pad(freqs, dur, attack=2.0, release=2.0, cutoff=700):
    n = n_(dur)
    t = t_(n)
    y = np.zeros(n)
    for f in freqs:
        for det in (-0.25, 0.0, 0.3):
            vib = 1 + 0.002 * np.sin(2 * np.pi * 4.8 * t + rng.random() * 6)
            y += saw(f * vib + det, n, rng.random())
    y = lowpass(lowpass(y, cutoff), cutoff * 1.6)
    return y * env_ar(n, attack, release) / (len(freqs) * 3)


def choir(freqs, dur, attack=1.5, release=2.0):
    n = n_(dur)
    t = t_(n)
    y = np.zeros(n)
    formants = ((700, 1.0, 130), (1220, 0.5, 120), (2600, 0.25, 160))
    for f0 in freqs:
        for voice in range(3):
            vib = 1 + 0.006 * np.sin(2 * np.pi * (5 + voice * 0.3) * t + voice)
            ph = 2 * np.pi * np.cumsum(f0 * (1 + (voice - 1) * 0.003) * vib) / SR
            for h in range(1, 16):
                fh = f0 * h
                amp = sum(a * np.exp(-((fh - fc) / bw) ** 2 / 2) for fc, a, bw in formants) + 0.08 / h
                y += amp * np.sin(h * ph)
    return y * env_ar(n, attack, release) / (len(freqs) * 6)


def crackle(dur, density=6.0):
    n = n_(dur)
    y = np.zeros(n)
    for i in rng.integers(0, max(1, n - 2000), int(dur * density)):
        k = int(rng.integers(60, 400))
        y[i:i + k] += rng.uniform(-1, 1, k) * np.exp(-np.arange(k) / (k / 5)) * rng.uniform(0.2, 1)
    return highpass(y, 1800) * 0.5 + lowpass(rng.uniform(-1, 1, n), 120) * 0.6


def paper(dur, density=60, bright=3000):
    """Paper rustle / tear: dense tiny clicks through a bandpass."""
    n = n_(dur)
    y = np.zeros(n)
    for i in rng.integers(0, max(1, n - 300), int(dur * density)):
        k = int(rng.integers(40, 200))
        y[i:i + k] += rng.uniform(-1, 1, k) * np.exp(-np.arange(k) / (k / 4)) * rng.uniform(0.3, 1)
    return band(y, bright * 0.5, min(15000, bright * 3))


def roar(dur, f0, growl=1.0, formant=(500, 900, 2200)):
    n = n_(dur)
    t = t_(n)
    f = f0 * (1 + 0.08 * np.sin(2 * np.pi * 5.5 * t) * growl + 0.05 * rng.standard_normal(n).cumsum() / np.sqrt(n))
    src = saw(f, n) + 0.6 * lowpass(noise(dur), 3000) * growl
    out = sum(band(src, fc * 0.7, fc * 1.4) * g for fc, g in zip(formant, (1.0, 0.6, 0.3)))
    out = np.tanh(out * 3 * growl)
    return out * env_ar(n, 0.08, dur * 0.4)


def shimmer(dur, f_lo=2000, f_hi=7000, count=14):
    y = np.zeros(n_(dur))
    for k in range(count):
        f = rng.uniform(f_lo, f_hi)
        s = int(rng.uniform(0, 0.6) * len(y))
        tone = np.sin(2 * np.pi * f * t_(len(y) - s)) * np.exp(-t_(len(y) - s) / (dur * 0.25))
        y[s:] += tone * rng.uniform(0.3, 1)
    return y / count


def reverb(x, size=1.2, wet=0.25, bright=6000):
    n = n_(size)
    ir = lowpass(rng.uniform(-1, 1, n) * np.exp(-t_(n) / (size / 4)), bright)
    ir /= np.sqrt((ir ** 2).sum())
    w = fftconvolve(x, ir)
    out = np.pad(x, (0, len(w) - len(x)))
    return out + w * wet


def place(buf, sig, t0, gain=1.0):
    i = int(t0 * SR)
    if i >= len(buf):
        return
    sig = sig[: len(buf) - i]
    buf[i:i + len(sig)] += sig * gain


def mix(*parts):
    """Sum (signal, gain, start) tuples into one buffer."""
    end = max(int(s * SR) + len(x) for x, g, s in parts)
    out = np.zeros(end)
    for x, g, s in parts:
        place(out, x, s, g)
    return out


def finish(x, peak_db=-1.0, fade=0.01, tail_trim=True):
    x = np.asarray(x, dtype=np.float64)
    if tail_trim:
        a = np.abs(x)
        thr = a.max() * 10 ** (-60 / 20)
        idx = np.where(a > thr)[0]
        if len(idx):
            x = x[: idx[-1] + int(0.02 * SR)]
    f = min(len(x) // 4, int(fade * SR))
    if f > 0:
        x[-f:] *= np.linspace(1, 0, f)
    peak = np.abs(x).max() + 1e-12
    return x * (10 ** (peak_db / 20) / peak)


HIJAZ = [0, 1, 4, 5, 7, 8, 10]


def note(degree, octave=3, root=0):
    o, d = divmod(degree, 7)
    return 146.83 * 2 ** ((HIJAZ[d] + 12 * (o + octave - 3) + root) / 12)
