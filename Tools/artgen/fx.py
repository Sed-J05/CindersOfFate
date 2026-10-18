"""FX & projectiles (Docs/06 §4): procedural pixel shapes, low-res then upscaled like the characters."""
import math
import numpy as np
from PIL import Image
from core import Layer, outlined, stack, hexc, rot, mix
from bake import lowres_size

WHITE = hexc("FFFFFF"); ICE = hexc("DDF0FF"); BLUE = hexc("4FA3FF"); DBLUE = hexc("1B4F9C")
RED = hexc("C1272D"); CRIM = hexc("FF2A3A"); DARK = hexc("140C0E"); EMB = hexc("E2682A"); GOLD = hexc("FFE08A")
GREY = hexc("8A8A90"); GREYL = hexc("BDBDC2"); SMOKE = hexc("2B2426")


def fade(img, a):
    arr = np.array(img).astype(np.float32)
    arr[:, :, 3] *= max(0.0, min(1.0, a))
    return Image.fromarray(arr.astype(np.uint8), "RGBA")


def crescent(L, cx, cy, r, thick, a0, a1, col, taper=True):
    """Thick arc from angle a0 to a1 (degrees, 0 = right, clockwise because y is down)."""
    n = 24
    outer, inner = [], []
    for i in range(n + 1):
        u = i / n
        a = math.radians(a0 + (a1 - a0) * u)
        th = thick * (math.sin(math.pi * u) if taper else 1.0)
        outer.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
        inner.append((cx + math.cos(a) * (r - th), cy + math.sin(a) * (r - th)))
    L.poly(outer + inner[::-1], col)


def slash_side():
    W, H = lowres_size("Slash_Side")
    out = []
    for i, (span, alpha) in enumerate(((0.55, 1.0), (1.0, 1.0), (1.0, 0.45))):
        L = Layer(W, H)
        a0, a1 = -70, -70 + 140 * span
        crescent(L, 20, H / 2, W * 0.42, 16, a0, a1, BLUE)
        crescent(L, 20, H / 2, W * 0.42, 9, a0 + 8, a1 - 4, ICE)
        crescent(L, 20, H / 2, W * 0.42, 3, a0 + 15, a1 - 10, WHITE)
        out.append(fade(L.img, alpha))
    return {"": out}


def slash_up():
    W, H = lowres_size("Slash_Up")
    out = []
    for span, alpha in ((0.55, 1.0), (1.0, 1.0), (1.0, 0.45)):
        L = Layer(W, H)
        a0, a1 = 200, 200 + 140 * span
        crescent(L, W / 2, H - 14, H * 0.42, 16, a0, a1, BLUE)
        crescent(L, W / 2, H - 14, H * 0.42, 9, a0 + 8, a1 - 4, ICE)
        crescent(L, W / 2, H - 14, H * 0.42, 3, a0 + 15, a1 - 10, WHITE)
        out.append(fade(L.img, alpha))
    return {"": out}


def flame_blob(L, cx, cy, r, ph, cols, tail=0):
    for k, col in enumerate(cols):
        rr = r * (1 - k * 0.3)
        L.ellipse(cx, cy, rr, rr * 0.85, col)
        for j in range(4):
            a = ph * math.tau + j * 1.7
            tx = cx - rr * (0.8 + 0.6 * math.sin(a)) - tail * (1 - k * 0.3)
            ty = cy + math.sin(a * 1.3) * rr * 0.5
            L.poly([(cx - rr * 0.2, cy - rr * 0.6), (tx, ty), (cx - rr * 0.2, cy + rr * 0.6)], col)


def fireball_fly():
    W, H = lowres_size("Fireball_Fly")
    return {"": [outlined(_draw(W, H, lambda L, i=i: flame_blob(L, W * 0.62, H / 2, 16, i / 4, [BLUE, ICE, WHITE], tail=20)), DBLUE) for i in range(4)]}


def _draw(W, H, fn):
    L = Layer(W, H)
    fn(L)
    return L


def burst(W, H, k, n, cols, rays=8, seed=1, maxr=None):
    rng = np.random.default_rng(seed)
    L = Layer(W, H)
    cx, cy = W / 2, H / 2
    maxr = maxr or min(W, H) * 0.45
    t = (k + 1) / n
    r = maxr * (0.35 + 0.65 * t)
    for j in range(rays):
        a = j * math.tau / rays + rng.random() * 0.3
        ln = r * (0.7 + 0.5 * rng.random())
        w = max(1.5, 5 * (1 - t))
        p1 = (cx + math.cos(a) * ln, cy + math.sin(a) * ln)
        L.capsule((cx + math.cos(a) * r * 0.2, cy + math.sin(a) * r * 0.2), p1, w, 0.5, cols[0])
    if t < 0.8:
        L.ellipse(cx, cy, r * 0.45 * (1 - t), r * 0.45 * (1 - t), cols[-1])
    return fade(L.img, 1.15 - t * 0.6)


def fireball_impact():
    W, H = lowres_size("Fireball_Impact")
    return {"": [burst(W, H, k, 5, [BLUE, ICE, WHITE], rays=10, seed=3) for k in range(5)]}


def soul_orb():
    W, H = lowres_size("SoulOrb")
    out = []
    for i in range(4):
        L = Layer(W, H)
        r = 8 + math.sin(i / 4 * math.tau) * 1
        L.ellipse(W / 2, H / 2, r + 2, r + 2, BLUE)
        L.ellipse(W / 2, H / 2, r, r, DARK)
        L.rect(W / 2 - 3, H / 2 - 4, W / 2 - 2, H / 2 - 3, ICE)
        out.append(L.img)
    return {"": out}


def heal_aura():
    W, H = lowres_size("Heal_Aura")
    out = []
    for i in range(6):
        L = Layer(W, H)
        ph = i / 6
        L.ellipse(W / 2, H * 0.62, W * 0.34, H * 0.12, (79, 163, 255, 70))
        for k in range(14):
            a = k * math.tau / 14 + ph * 1.5
            x = W / 2 + math.cos(a) * W * 0.32
            y = H * 0.75 - ((ph * 40 + k * 7) % 60)
            s = 1 + (k % 2)
            L.rect(x, y, x + s, y + s, ICE if k % 3 else WHITE)
        out.append(L.img)
    return {"": out}


def star(L, cx, cy, r, col, pts=4, inner=0.3, rot0=0):
    p = []
    for k in range(pts * 2):
        a = rot0 + k * math.pi / pts
        rr = r if k % 2 == 0 else r * inner
        p.append((cx + math.cos(a) * rr, cy + math.sin(a) * rr))
    L.poly(p, col)


def hit_spark():
    W, H = lowres_size("Hit_Spark")
    out = []
    for k in range(4):
        L = Layer(W, H)
        r = 8 + k * 7
        star(L, W / 2, H / 2, r, WHITE, 4, 0.25, k * 0.3)
        if k < 2:
            star(L, W / 2, H / 2, r * 0.6, ICE, 4, 0.3, 0.78)
        out.append(fade(L.img, 1 - k * 0.22))
    return {"": out}


def block_clink():
    W, H = lowres_size("Block_Clink")
    out = []
    for k in range(3):
        L = Layer(W, H)
        r = 10 + k * 8
        star(L, W / 2, H / 2, r, GOLD, 6, 0.22, k * 0.2)
        star(L, W / 2, H / 2, r * 0.5, WHITE, 4, 0.3, 0.4)
        out.append(fade(L.img, 1 - k * 0.25))
    return {"": out}


def black_slash():
    W, H = lowres_size("BlackSlash_Fly")
    out = []
    for i in range(3):
        L = Layer(W, H)
        crescent(L, W * 0.25, H / 2, H * 0.62, 12, -65, 65, CRIM)
        crescent(L, W * 0.25 - 2, H / 2, H * 0.62, 9, -58, 58, DARK)
        for j in range(3):
            y = H / 2 + (j - 1) * 8 + math.sin(i + j) * 2
            L.line((W * 0.2 - 10 - j * 4, y), (W * 0.45, y), (193, 39, 45, 160), 1)
        out.append(L.img)
    return {"": out}


def black_slash_break():
    W, H = lowres_size("BlackSlash_Break")
    rng = np.random.default_rng(9)
    out = []
    shards = [(rng.random() * W * 0.4 + W * 0.3, rng.random() * H * 0.8 + H * 0.1, rng.random() * math.tau) for _ in range(9)]
    for k in range(4):
        L = Layer(W, H)
        t = k / 3
        for j, (x, y, a) in enumerate(shards):
            x2, y2 = x + math.cos(a) * t * 24, y + math.sin(a) * t * 14
            L.poly([(x2, y2 - 3), (x2 + 4, y2), (x2, y2 + 3)], CRIM if j % 3 == 0 else DARK)
        out.append(fade(L.img, 1 - t * 0.7))
    return {"": out}


def exclamation():
    W, H = lowres_size("Exclamation")
    out = []
    for i in range(4):
        L = Layer(W, H)
        dy = -abs(math.sin(i / 4 * math.pi)) * 3
        L.rect(W / 2 - 3, 6 + dy, W / 2 + 3, 20 + dy, CRIM)
        L.rect(W / 2 - 3, 23 + dy, W / 2 + 3, 28 + dy, CRIM)
        out.append(outlined(L, DARK, 1))
    return {"": out}


def knife():
    W, H = lowres_size("Knife_Fly")
    out = []
    for i in range(4):
        L = Layer(W, H)
        cy = H / 2 + (1 if i % 2 else 0) * 0.5
        L.poly([(W * 0.85, cy), (W * 0.45, cy - 3), (W * 0.45, cy + 3)], hexc("DCE8F7"))
        L.rect(W * 0.3, cy - 1.5, W * 0.45, cy + 1.5, DBLUE)
        L.rect(W * 0.43, cy - 4, W * 0.46, cy + 4, BLUE)
        if i % 2:
            L.line((W * 0.1, cy), (W * 0.28, cy), (110, 193, 255, 150), 1)
        out.append(outlined(L, hexc("1E4A92"), 1))
    return {"": out}


def vine_warn():
    W, H = lowres_size("Vine_Warn")
    out = []
    for i in range(4):
        L = Layer(W, H)
        g = 0.55 + 0.45 * math.sin(i / 4 * math.tau)
        L.ellipse(W / 2, H - 3, W * 0.42, 5, (255, 42, 58, int(80 * g)))
        L.ellipse(W / 2, H - 3, W * 0.25, 3, (255, 90, 60, int(170 * g)))
        for j in range(5):
            x = W * (0.25 + j * 0.12)
            L.line((x, H - 2), (x + (2 if j % 2 else -2), H - 8 - 3 * g), CRIM, 1)
        out.append(L.img)
    return {"": out}


def vine_frame(h_frac, W, H, sway=0.0):
    L = Layer(W, H)
    base = (W / 2, H - 2)
    top_h = (H - 6) * h_frac
    pts = [(base[0] + math.sin(u * 3 + sway) * 4 * u, base[1] - top_h * u) for u in np.linspace(0, 1, 10)]
    for k, (p0, p1) in enumerate(zip(pts, pts[1:])):
        r0 = 7 * (1 - k / 10) + 1.5
        L.capsule(p0, p1, r0, r0 * 0.9, DARK)
        if k % 2 == 0:
            side = 1 if k % 4 == 0 else -1
            L.poly([p1, (p1[0] + side * (r0 + 5), p1[1] + 2), (p1[0], p1[1] + 4)], DARK)
    for p0, p1 in zip(pts, pts[1:]):
        L.line(p0, p1, CRIM, 1)
    return outlined(L, CRIM, 1)


def vine():
    W, H = lowres_size("Vine")
    return {"Grow": [vine_frame(f, W, H, i * 0.4) for i, f in enumerate((0.15, 0.4, 0.7, 0.95, 1.05, 1.0))],
            "Retract": [vine_frame(f, W, H, 2.4 + i * 0.4) for i, f in enumerate((0.7, 0.4, 0.12))]}


def lava_strip(W, H, ph, frames=4):
    """Horizontally tileable bubbling lava (period = W)."""
    arr = np.zeros((H, W, 4), np.uint8)
    for x in range(W):
        u = x / W * math.tau
        surf = H * 0.38 + math.sin(u * 2 + ph * math.tau) * 1.5 + math.sin(u * 3 - ph * math.tau * 2) * 1.0
        for y in range(H):
            if y >= surf:
                depth = (y - surf) / (H - surf + 1e-6)
                c = mix(hexc("FFD27A"), hexc("C1272D"), min(1, depth * 1.6)) if y > surf + 1 else hexc("FFF0B0")
                arr[y, x] = c
    img = Image.fromarray(arr, "RGBA")
    L = Layer(W, H)
    for k in range(2):
        bx = (W * (0.25 + 0.5 * k) + ph * W * 0.3) % W
        by = H * 0.6 - ph * 4
        L.ellipse(bx, by, 1.5, 1.5, hexc("FFF0B0"))
    img.alpha_composite(L.img)
    return img


def lava_zone():
    W, H = lowres_size("Lava_Zone")
    return {"": [lava_strip(W, H, i / 4) for i in range(4)]}


def giant_swipe():
    W, H = lowres_size("Giant_Swipe")
    out = []
    for k in range(5):
        L = Layer(W, H)
        t = (k + 1) / 5
        span = min(1, t * 1.6)
        crescent(L, 10, H * 0.15, W * 0.9, 40, 20, 20 + 70 * span, CRIM)
        crescent(L, 10, H * 0.15, W * 0.9, 26, 26, 20 + 70 * span - 4, DARK)
        crescent(L, 10, H * 0.15, W * 0.9, 6, 30, 20 + 70 * span - 8, hexc("FF6A5A"))
        out.append(fade(L.img, 1.2 - max(0, t - 0.6) * 1.8))
    return {"": out}


def shockwave():
    W, H = lowres_size("Shockwave")
    out = []
    for k in range(4):
        L = Layer(W, H)
        t = (k + 1) / 4
        w = W * 0.5 * t
        for side in (-1, 1):
            x0 = W / 2
            L.poly([(x0, H - 2), (x0 + side * w, H - 2), (x0 + side * w * 0.9, H - 2 - 10 * (1 - t) - 3), (x0 + side * w * 0.4, H - 4)], ICE)
            L.poly([(x0 + side * w * 0.2, H - 2), (x0 + side * w, H - 2), (x0 + side * w * 0.85, H - 6)], BLUE)
        for j in range(6):
            x = W / 2 + (j - 2.5) * w * 0.35
            L.ellipse(x, H - 5 - (1 - t) * 6, 2 + t * 2, 2, GREYL)
        out.append(fade(L.img, 1.1 - t * 0.5))
    return {"": out}


def shield_arc():
    W, H = lowres_size("Shield_Arc")
    out = []
    for i in range(2):
        L = Layer(W, H)
        crescent(L, W * 0.1, H / 2, H * 0.48, 10 + i, -60, 60, (110, 193, 255, 130), taper=False)
        crescent(L, W * 0.1, H / 2, H * 0.48, 3, -55, 55, (240, 248, 255, 200), taper=False)
        out.append(L.img)
    return {"": out}


def dust_puff():
    W, H = lowres_size("Dust_Puff")
    out = []
    for k in range(4):
        L = Layer(W, H)
        t = (k + 1) / 4
        for j, (dx, dy) in enumerate(((-8, 0), (0, -4), (8, 0), (-3, 3), (4, 3))):
            r = 5 + t * 6 - j * 0.5
            L.ellipse(W / 2 + dx * (0.6 + t), H * 0.65 + dy - t * 6, r, r * 0.8, GREYL if j % 2 else GREY)
        out.append(fade(L.img, 1.1 - t * 0.7))
    return {"": out}


def death_smoke():
    W, H = lowres_size("Death_Smoke")
    rng = np.random.default_rng(5)
    scraps = [(rng.random() * math.tau, rng.random()) for _ in range(10)]
    out = []
    for k in range(6):
        L = Layer(W, H)
        t = (k + 1) / 6
        for j in range(7):
            a = j * math.tau / 7
            r = 8 + t * 22
            L.ellipse(W / 2 + math.cos(a) * r * 0.6, H / 2 + math.sin(a) * r * 0.5 - t * 8, 9 + t * 8, 8 + t * 6, SMOKE if j % 2 else hexc("3D3436"))
        for a, s in scraps:
            r = 10 + t * 40 * (0.5 + s)
            x, y = W / 2 + math.cos(a) * r, H / 2 + math.sin(a) * r - t * 10
            L.poly([(x, y), (x + 3, y + 1), (x + 1, y + 3)], hexc("E8D9B5") if s > 0.5 else EMB)
        out.append(fade(L.img, 1.15 - t * 0.8))
    return {"": out}


ALL = {"Slash_Side": slash_side, "Slash_Up": slash_up, "Fireball_Fly": fireball_fly, "Fireball_Impact": fireball_impact,
       "SoulOrb": soul_orb, "Heal_Aura": heal_aura, "Hit_Spark": hit_spark, "Block_Clink": block_clink,
       "BlackSlash_Fly": black_slash, "BlackSlash_Break": black_slash_break, "Exclamation": exclamation, "Knife_Fly": knife,
       "Vine_Warn": vine_warn, "Vine": vine, "Lava_Zone": lava_zone, "Giant_Swipe": giant_swipe, "Shockwave": shockwave,
       "Shield_Arc": shield_arc, "Dust_Puff": dust_puff, "Death_Smoke": death_smoke}
