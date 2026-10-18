"""Every sound effect the game asks for (ids in Docs/AUDIO_HOOKS.md), designed from synthesis recipes.
Each recipe returns (signal, peak dB). Quiet, frequent sounds get lower peaks so the mix stays balanced."""
import numpy as np
from synth import (SR, noise, lowpass, highpass, band, env_exp, env_ar, sweep, saw, thump, whoosh, metal, bell, pluck,
                   taiko, braam, crackle, paper, roar, shimmer, reverb, mix, note, n_, t_)


def swish(dur=0.18, lo=800, hi=6000, peak=0.35):
    return whoosh(dur, lo, hi, peak)


def impact(f0=160, f1=55, dur=0.35, decay=0.09, noise_amt=0.7, bright=3500):
    n = n_(dur)
    return thump(f0, f1, dur, decay) + lowpass(noise(dur), bright) * env_exp(n, decay * 0.35) * noise_amt


def tear(dur=0.16, bright=3500):
    return paper(dur, density=900, bright=bright) * env_exp(n_(dur), dur * 0.35)


def clang(freq=900, dur=0.9, decay=0.35):
    return mix((metal(freq, dur, decay), 1.0, 0), (lowpass(noise(0.03), 9000), 0.6, 0))


def flame(dur=0.5, bright=2500):
    n = n_(dur)
    body = lowpass(noise(dur), bright) * env_ar(n, 0.02, dur * 0.8)
    return body + crackle(dur, 30) * 0.4


def chime(freqs, dur=1.5, gap=0.07):
    return mix(*[(bell(f, dur, 0.9), 1.0, i * gap) for i, f in enumerate(freqs)])


def hiss(dur=0.5):
    n = n_(dur)
    return highpass(noise(dur), 3000) * env_ar(n, dur * 0.6, dur * 0.3)


def step_down(dur=0.25):
    return lowpass(noise(dur), 900) * env_exp(n_(dur), 0.04) + thump(120, 60, dur, 0.05) * 0.6


R = {}


def sfx(name, peak=-1.0):
    def deco(fn):
        R[name] = (fn, peak)
        return fn
    return deco


# ------------------------------------------------------------------ UI
@sfx("ui_card", -4)
def _():
    return mix((whoosh(0.5, 300, 3000, 0.4), 0.6, 0), (reverb(metal(110, 2.5, 1.2, ratios=(1, 2.4, 3.9, 5.3)), 1.5, 0.3), 0.9, 0.12))


@sfx("ui_confirm", -6)
def _():
    return mix((tear(0.05, 4000), 0.6, 0), (thump(900, 400, 0.12, 0.03), 0.8, 0))


@sfx("ui_move", -12)
def _():
    return mix((tear(0.04, 5000), 1.0, 0), (thump(1400, 900, 0.05, 0.015), 0.4, 0))


@sfx("ui_pause", -7)
def _():
    return whoosh(0.35, 2000, 300, 0.15)


@sfx("ui_resume", -7)
def _():
    return whoosh(0.35, 300, 2500, 0.85)


@sfx("ui_prompt", -10)
def _():
    return reverb(bell(1760, 0.8, 0.5), 0.6, 0.2)


@sfx("ui_text", -16)
def _():
    return paper(0.14, 140, 3500) * env_ar(n_(0.14), 0.02, 0.08)


# ------------------------------------------------------------------ player
@sfx("player_attack", -3)
def _():
    return swish(0.17, 900, 7000, 0.4)


@sfx("player_attack_up", -3)
def _():
    return swish(0.17, 1300, 9000, 0.4)


@sfx("player_hit_enemy", -1)
def _():
    return mix((tear(0.12, 3000), 1.0, 0), (impact(220, 80, 0.2, 0.05, 0.4), 0.8, 0))


@sfx("player_hit_boss", -1)
def _():
    return mix((tear(0.14, 2600), 0.8, 0), (impact(170, 60, 0.3, 0.07, 0.5), 1.0, 0), (clang(1300, 0.4, 0.12), 0.25, 0))


@sfx("player_jump", -8)
def _():
    return mix((whoosh(0.16, 400, 2500, 0.3), 1.0, 0), (paper(0.08, 200, 2500), 0.5, 0))


@sfx("player_land", -7)
def _():
    return step_down(0.2)


@sfx("player_hurt", -1)
def _():
    breath = band(noise(0.25), 500, 1800) * env_ar(n_(0.25), 0.01, 0.2)
    return mix((impact(240, 70, 0.3, 0.06, 0.8), 1.0, 0), (tear(0.1, 2200), 0.7, 0), (breath, 0.5, 0.03))


@sfx("player_death", -2)
def _():
    snuff = whoosh(0.9, 3000, 300, 0.15)
    return reverb(mix((snuff, 1.0, 0), (crackle(1.6, 12), 0.6, 0.2), (thump(120, 40, 1.0, 0.3), 0.6, 0)), 1.5, 0.3)


@sfx("player_respawn", -6)
def _():
    return mix((flame(0.6, 2000), 0.7, 0), (whoosh(0.5, 300, 2500, 0.8), 0.6, 0))


@sfx("player_dash", -3)
def _():
    rev = whoosh(0.28, 600, 5000, 0.85)
    glass = shimmer(0.4, 3000, 8000, 10)
    return mix((rev, 1.0, 0), (glass, 0.6, 0.05))


@sfx("player_fireball", -2)
def _():
    return mix((thump(200, 70, 0.3, 0.08), 0.8, 0), (whoosh(0.4, 400, 4000, 0.2), 0.9, 0), (crackle(0.4, 30), 0.3, 0))


@sfx("fireball_impact", -2)
def _():
    return mix((impact(180, 50, 0.5, 0.12, 1.0, 5000), 1.0, 0), (crackle(0.5, 40), 0.5, 0.02))


@sfx("player_heal_start", -8)
def _():
    n = n_(0.9)
    return (sweep(220, 440, 0.9) + 0.5 * sweep(330, 660, 0.9)) * env_ar(n, 0.4, 0.2) * 0.6 + whoosh(0.9, 300, 1500, 0.9) * 0.5


@sfx("player_heal", -5)
def _():
    return reverb(chime([note(0, 5), note(4, 5), note(7, 5)], 1.4, 0.06), 1.0, 0.3)


@sfx("soul_gain", -14)
def _():
    return shimmer(0.25, 3500, 7000, 5)


@sfx("soul_full", -6)
def _():
    return reverb(chime([note(4, 5), note(7, 5), note(9, 5)], 1.2, 0.05), 0.8, 0.3)


# ------------------------------------------------------------------ world
@sfx("ability_unlocked", -2)
def _():
    return reverb(mix((braam(note(0, 2), 2.5), 0.6, 0), (chime([note(0, 5), note(4, 5), note(7, 5), note(9, 5)], 2.0, 0.09), 0.8, 0.05)), 1.5, 0.35)


@sfx("bench_rest", -6)
def _():
    exhale = band(noise(1.4), 300, 1500) * env_ar(n_(1.4), 0.3, 0.9)
    return mix((flame(0.5), 0.6, 0), (flame(0.5), 0.5, 0.25), (flame(0.5), 0.5, 0.5), (exhale, 0.4, 0.3))


@sfx("bench_saved", -4)
def _():
    return reverb(bell(note(0, 3), 3.0, 1.5), 1.5, 0.3)


@sfx("door_open", -3)
def _():
    n = n_(1.4)
    grind = band(noise(1.4), 80, 600) * (0.7 + 0.3 * np.sin(2 * np.pi * 9 * t_(n))) * env_ar(n, 0.1, 0.3)
    return mix((grind, 1.0, 0), (step_down(0.4), 0.8, 1.3))


@sfx("gate_close", -1)
def _():
    rattle = band(noise(0.5), 1500, 6000) * env_ar(n_(0.5), 0.3, 0.05) * 0.4
    return reverb(mix((rattle, 1.0, 0), (impact(150, 40, 0.8, 0.2, 0.6), 1.0, 0.45), (clang(220, 1.2, 0.5), 0.5, 0.45)), 1.2, 0.3)


@sfx("gate_open", -3)
def _():
    n = n_(1.1)
    chain = band(noise(1.1), 1200, 5000) * (0.6 + 0.4 * np.sign(np.sin(2 * np.pi * 14 * t_(n)))) * env_ar(n, 0.1, 0.2)
    return mix((chain, 0.6, 0), (band(noise(1.1), 80, 400) * env_ar(n, 0.1, 0.2), 0.8, 0))


@sfx("hazard_lava", -2)
def _():
    return mix((hiss(0.6), 1.0, 0), (impact(120, 50, 0.3, 0.08, 0.3), 0.6, 0), (crackle(0.6, 40), 0.4, 0))


@sfx("hazard_spikes", -1)
def _():
    return mix((clang(1800, 0.25, 0.06), 0.6, 0), (impact(200, 60, 0.3, 0.06, 0.6), 1.0, 0))


@sfx("orb_touch", -2)
def _():
    n = n_(2.5)
    hum = (sweep(80, 160, 2.5) + 0.5 * sweep(120, 240, 2.5)) * env_ar(n, 1.6, 0.4)
    return reverb(mix((hum, 0.8, 0), (shimmer(1.5, 2000, 6000, 20), 0.6, 1.0), (braam(note(0, 2), 2.0), 0.5, 2.1)), 2.0, 0.35)


@sfx("room_transition", -9)
def _():
    return mix((whoosh(0.4, 500, 5000, 0.5), 1.0, 0), (paper(0.3, 120, 3000), 0.5, 0.05))


# ------------------------------------------------------------------ regular enemies
@sfx("enemy_hit", -2)
def _():
    return mix((tear(0.1, 2800), 1.0, 0), (thump(260, 110, 0.12, 0.03), 0.6, 0))


@sfx("enemy_death", -3)
def _():
    puff = lowpass(noise(0.6), 1500) * env_ar(n_(0.6), 0.02, 0.5)
    return mix((puff, 0.9, 0), (paper(0.4, 300, 2500) * env_exp(n_(0.4), 0.15), 0.7, 0.02))


@sfx("lunger_telegraph", -4)
def _():
    return band(noise(0.45), 2500, 7000) * env_ar(n_(0.45), 0.35, 0.08)


@sfx("lunger_lunge", -3)
def _():
    return whoosh(0.3, 500, 5000, 0.3)


@sfx("kite_telegraph", -5)
def _():
    n = n_(0.5)
    flutter = paper(0.5, 400, 3000) * (0.5 + 0.5 * np.sin(2 * np.pi * 22 * t_(n))) * np.linspace(0.4, 1, n)
    return flutter + sweep(700, 1400, 0.5) * np.linspace(0, 0.3, n)


@sfx("kite_dive", -4)
def _():
    n = n_(0.6)
    return sweep(1800, 600, 0.6) * env_ar(n, 0.05, 0.2) * 0.5 + whoosh(0.6, 800, 4000, 0.5)


@sfx("bat_wake", -5)
def _():
    n = n_(0.4)
    wings = paper(0.4, 300, 2500) * (0.5 + 0.5 * np.sin(2 * np.pi * 16 * t_(n)))
    squeak = sweep(3000, 4200, 0.12) * env_ar(n_(0.12), 0.01, 0.06)
    return mix((wings, 0.8, 0), (squeak, 0.5, 0.15))


@sfx("bat_telegraph", -4)
def _():
    n = n_(0.35)
    return (sweep(3500, 5200, 0.35) + 0.4 * sweep(5200, 6800, 0.35)) * env_ar(n, 0.02, 0.1) * (1 + 0.3 * np.sin(2 * np.pi * 40 * t_(n)))


@sfx("charger_telegraph", -3)
def _():
    snort = band(noise(0.3), 300, 1500) * env_ar(n_(0.3), 0.02, 0.2)
    scrape = band(noise(0.4), 1500, 5000) * env_ar(n_(0.4), 0.05, 0.2)
    return mix((snort, 1.0, 0), (scrape, 0.6, 0.25), (scrape, 0.6, 0.6))


@sfx("charger_charge", -2)
def _():
    n = n_(1.0)
    hooves = np.zeros(n)
    for k in range(8):
        i = int(k * 0.12 * SR)
        h = thump(140, 60, 0.12, 0.03)[: n - i]
        hooves[i:i + len(h)] += h
    return hooves + band(noise(1.0), 60, 300) * 0.4


@sfx("charger_stun", -2)
def _():
    return mix((impact(140, 40, 0.5, 0.12, 0.6), 1.0, 0), (reverb(bell(2200, 1.2, 0.6), 0.8, 0.3), 0.3, 0.15), (reverb(bell(2600, 1.2, 0.6), 0.8, 0.3), 0.25, 0.4))


@sfx("knight_telegraph", -4)
def _():
    return mix((clang(700, 0.4, 0.12), 0.4, 0), (clang(950, 0.4, 0.12), 0.3, 0.1), (band(noise(0.4), 2000, 8000) * env_ar(n_(0.4), 0.3, 0.05), 0.4, 0))


@sfx("knight_slash", -2)
def _():
    return mix((swish(0.25, 600, 6000, 0.4), 1.0, 0), (clang(1100, 0.3, 0.08), 0.15, 0.08))


@sfx("thrower_telegraph", -5)
def _():
    n = n_(0.3)
    return band(noise(0.3), 3000, 9000) * env_ar(n, 0.25, 0.04) + metal(2400, 0.3, 0.1) * 0.1


@sfx("thrower_throw", -4)
def _():
    n = n_(0.35)
    return sweep(2400, 1600, 0.35) * env_ar(n, 0.01, 0.2) * 0.3 + whoosh(0.35, 1500, 8000, 0.3)


@sfx("thrower_hop", -9)
def _():
    return mix((whoosh(0.12, 500, 2000, 0.3), 1.0, 0), (thump(300, 150, 0.08, 0.02), 0.5, 0))


# ------------------------------------------------------------------ bosses
@sfx("boss_intro", -1)
def _():
    return reverb(mix((taiko(0.8, 1.5), 1.0, 0), (braam(note(0, 1), 3.5), 0.9, 0)), 1.8, 0.35)


@sfx("boss_death", -1)
def _():
    return reverb(mix((braam(note(1, 1), 4.0), 1.0, 0), (lowpass(noise(3.0), 1200) * env_ar(n_(3.0), 0.3, 2.0), 0.4, 0.3), (crackle(3.0, 15), 0.4, 0.5)), 2.0, 0.35)


def _sword(base=1.0, heavy=0.0):
    return mix((whoosh(0.3 + heavy * 0.15, 500 * base, 6000 * base, 0.4), 1.0, 0), (clang(1500 * base, 0.4, 0.1), 0.08 + 0.1 * heavy, 0.1))


def _draw(freq=1800, dur=0.5):
    n = n_(dur)
    scrape = band(noise(dur), freq, freq * 3) * env_ar(n, dur * 0.7, dur * 0.2)
    return scrape + metal(freq, dur, dur * 0.5) * 0.15


@sfx("kage_warden_telegraph", -3)
def _():
    return _draw(1600, 0.5)


@sfx("kage_warden_slash", -2)
def _():
    return _sword(1.0, 0.3)


@sfx("warden_slash", -3)
def _():
    return mix((thump(140, 60, 0.25, 0.07), 0.8, 0), (swish(0.25, 400, 3000, 0.4), 0.6, 0))


@sfx("warden_clink", -1)
def _():
    return reverb(clang(1250, 0.8, 0.25), 0.6, 0.2)


@sfx("kage_caller_telegraph", -4)
def _():
    breath = band(noise(0.6), 400, 2500) * env_ar(n_(0.6), 0.4, 0.15)
    return mix((breath, 0.8, 0), (shimmer(0.6, 800, 2500, 8), 0.4, 0))


@sfx("kage_caller_slash", -3)
def _():
    return swish(0.26, 500, 5000, 0.45)


@sfx("caller_blackslash_telegraph", -3)
def _():
    n = n_(0.8)
    rumble = band(noise(0.8), 40, 250) * env_ar(n, 0.6, 0.1)
    ink = sweep(90, 60, 0.8) * env_ar(n, 0.6, 0.1)
    return rumble + ink * 0.6


@sfx("caller_blackslash", -2)
def _():
    return mix((whoosh(0.5, 300, 4500, 0.25), 1.0, 0), (tear(0.25, 1800), 0.6, 0.05))


@sfx("caller_summon", -3)
def _():
    chant = sum(roar(1.2, f, 0.2, (450, 800, 2400)) * 0.3 for f in (98, 147, 196))
    return reverb(mix((chant, 0.7, 0), (lowpass(noise(0.5), 1200) * env_ar(n_(0.5), 0.02, 0.4), 0.8, 0.9)), 1.5, 0.35)


@sfx("colossus_telegraph", -3)
def _():
    n = n_(0.7)
    creak = band(saw(60 + 15 * np.sin(2 * np.pi * 3 * t_(n)), n), 300, 2000) * env_ar(n, 0.3, 0.2)
    return creak * 0.6 + clang(300, 0.7, 0.3) * 0.2


@sfx("colossus_slash", -1)
def _():
    return mix((whoosh(0.55, 200, 3000, 0.45), 1.0, 0), (thump(100, 45, 0.5, 0.15), 0.5, 0.2))


@sfx("colossus_clink", -1)
def _():
    return reverb(mix((clang(420, 1.2, 0.45), 1.0, 0), (impact(160, 70, 0.3, 0.06, 0.4), 0.6, 0)), 1.0, 0.25)


@sfx("colossus_bash", -1)
def _():
    return impact(130, 45, 0.6, 0.12, 0.9, 2500)


@sfx("colossus_jump", -3)
def _():
    return whoosh(0.6, 150, 1500, 0.4)


@sfx("colossus_slam", -0.5)
def _():
    return reverb(mix((thump(90, 30, 1.6, 0.45), 1.0, 0), (lowpass(noise(1.2), 900) * env_exp(n_(1.2), 0.3), 0.8, 0), (crackle(0.8, 30), 0.3, 0.05)), 1.5, 0.3)


@sfx("colossus_dash", -2)
def _():
    n = n_(0.9)
    return band(noise(0.9), 40, 400) * env_ar(n, 0.1, 0.3) + clang(250, 0.9, 0.4) * 0.15


@sfx("zulmat_telegraph", -2)
def _():
    return mix((roar(0.6, 70, 0.6, (350, 700, 1800)), 0.5, 0), (_draw(1300, 0.6), 0.8, 0))


@sfx("zulmat_slash", -1)
def _():
    return mix((_sword(0.8, 1.0), 1.0, 0), (thump(110, 45, 0.4, 0.12), 0.5, 0.12))


@sfx("zulmat_cast", -3)
def _():
    n = n_(1.0)
    return reverb((sweep(60, 180, 1.0) + band(noise(1.0), 200, 1200) * 0.6) * env_ar(n, 0.8, 0.15), 1.2, 0.3)


@sfx("zulmat_vine", -1)
def _():
    burst = band(noise(0.5), 200, 2500) * env_exp(n_(0.5), 0.15)
    return mix((impact(120, 50, 0.4, 0.1, 0.5), 0.9, 0), (burst, 0.8, 0), (paper(0.5, 500, 1500), 0.5, 0.02))


@sfx("zulmat_summon", -2)
def _():
    chant = sum(roar(1.6, f, 0.3, (400, 750, 2200)) * 0.3 for f in (73, 110, 147))
    return reverb(mix((chant, 0.8, 0), (whoosh(0.8, 2000, 200, 0.2), 0.6, 1.0)), 2.0, 0.4)


@sfx("zulmat_swipe", -1)
def _():
    return reverb(mix((whoosh(0.8, 150, 2500, 0.45), 1.0, 0), (thump(80, 35, 0.8, 0.25), 0.6, 0.3)), 1.0, 0.25)


@sfx("zulmat_lunge", -2)
def _():
    return whoosh(0.35, 400, 5000, 0.25)


@sfx("zulmat_lava", -1)
def _():
    n = n_(1.4)
    rumble = band(noise(1.4), 30, 200) * env_ar(n, 0.3, 0.6)
    bubbles = sum(sweep(rng_f, rng_f * 1.6, 0.08) * env_exp(n_(0.08), 0.03) for rng_f in (300, 420, 360))
    return mix((rumble, 1.0, 0), (hiss(1.0), 0.4, 0.3), (bubbles, 0.3, 0.5), (bubbles, 0.3, 0.8))


@sfx("zulmat_shrink", -3)
def _():
    return whoosh(0.6, 4000, 300, 0.9)


@sfx("zulmat_grow", -1)
def _():
    return reverb(mix((whoosh(0.6, 200, 3000, 0.9), 0.7, 0), (braam(note(0, 1), 1.8), 0.7, 0.5)), 1.2, 0.3)


@sfx("zulmat_roar", -0.5)
def _():
    return reverb(roar(2.2, 55, 1.2, (300, 650, 1700)) + roar(2.2, 82, 0.9, (420, 900, 2300)) * 0.6, 2.0, 0.35)


@sfx("zulmat_death", -0.5)
def _():
    n = n_(3.5)
    fall = roar(2.5, 60, 1.0, (300, 650, 1700)) * np.linspace(1, 0, n_(2.5)) ** 0.5
    return reverb(mix((fall, 1.0, 0), (crackle(3.5, 25), 0.6, 1.0), (braam(note(1, 1), 3.0), 0.6, 0.2)), 2.5, 0.4)
