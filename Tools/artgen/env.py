"""Tiles, decor and props in the dark greyscale stone style of the environment concept sheet.
Drawn at half resolution (pixel look), upscaled x2 by bake.save like the characters."""
import math
import numpy as np
from PIL import Image
from core import Layer, outlined, band, stack, hexc, mix
from bake import lowres_size

OUT = hexc("0E0C0E")
EMBER = hexc("E2682A"); EMBER_L = hexc("FFB15C"); EMBER_W = hexc("FFF0C8")
GLOW_B = hexc("4FA3FF"); GLOW_W = hexc("EAF6FF")

THEMES = {
    "Cave": {"dark": hexc("1C1A1D"), "mid": hexc("2E2B30"), "light": hexc("4A464C"), "lip": hexc("8C8790"),
             "lip2": hexc("B7B2B8"), "crack": hexc("121013"), "accent": hexc("6B2A2E")},
    "Castle": {"dark": hexc("1B1E26"), "mid": hexc("2B303C"), "light": hexc("454C5C"), "lip": hexc("8F99AD"),
               "lip2": hexc("C2CAD8"), "crack": hexc("10121A"), "accent": hexc("7A1E22")},
}


def pnoise(w, h, cells, seed, octaves=3):
    """Periodic value noise in [0,1]: wraps on both axes with period (w, h)."""
    rng = np.random.default_rng(seed)
    out = np.zeros((h, w))
    amp, tot, c = 1.0, 0.0, cells
    for _ in range(octaves):
        g = rng.random((c, c))
        ys = np.arange(h) / h * c
        xs = np.arange(w) / w * c
        y0 = ys.astype(int); x0 = xs.astype(int)
        fy = (ys - y0)[:, None]; fx = (xs - x0)[None, :]
        fy = fy * fy * (3 - 2 * fy); fx = fx * fx * (3 - 2 * fx)
        y1 = (y0 + 1) % c; x1 = (x0 + 1) % c
        g00 = g[y0][:, x0]; g10 = g[y0][:, x1]; g01 = g[y1][:, x0]; g11 = g[y1][:, x1]
        out += amp * ((g00 * (1 - fx) + g10 * fx) * (1 - fy) + (g01 * (1 - fx) + g11 * fx) * fy)
        tot += amp; amp *= 0.5; c *= 2
    return out / tot


def quantize(n, cols, cuts):
    """Map a [0,1] field to a hard-edged palette (pixel-art bands)."""
    h, w = n.shape
    arr = np.zeros((h, w, 4), np.uint8)
    arr[:] = cols[0]
    for c, t in zip(cols[1:], cuts):
        arr[n > t] = c
    return arr


# ------------------------------------------------------------------ tiles
def rock_fill(theme, size, seed):
    t = THEMES[theme]
    if theme == "Cave":
        n = pnoise(size, size, 3, seed, 3)
        arr = quantize(n, [t["dark"], t["mid"], t["light"]], [0.42, 0.68])
        rng = np.random.default_rng(seed + 5)
        for _ in range(1):                     # one short crack (more reads as a repeating streak)
            x, y = rng.integers(0, size, 2)
            for k in range(rng.integers(3, 5)):
                arr[y % size, x % size] = t["crack"]
                x += rng.integers(-1, 2); y += 1
        return arr
    # castle: cut masonry, 2 courses per tile, offset bricks (period = tile)
    arr = np.zeros((size, size, 4), np.uint8)
    n = pnoise(size, size, 4, seed, 2)
    arr[:] = t["mid"]
    arr[n > 0.62] = t["light"]
    arr[n < 0.3] = t["dark"]
    course = size // 2
    for row in range(2):
        y = row * course
        arr[y, :] = t["crack"]
        off = 0 if row == 0 else size // 2
        for bx in (off % size, (off + size // 2) % size):
            arr[y:y + course, bx] = t["crack"]
        arr[min(size - 1, y + 1), :] = mix(t["light"], t["mid"], 0.5)
    return arr


def tile(theme, idx, size=25):
    """4-neighbour bitmask N1 E2 S4 W8 (Docs/06 §5.1): open sides get an outline, open top a light lip."""
    t = THEMES[theme]
    N, E, S, W = bool(idx & 1), bool(idx & 2), bool(idx & 4), bool(idx & 8)
    arr = rock_fill(theme, size, 11 + (0 if theme == "Cave" else 50))
    alpha = np.full((size, size), 255, np.uint8)
    yy, xx = np.mgrid[0:size, 0:size]
    rr = 4
    for open_a, open_b, cx, cy in ((not N, not W, rr, rr), (not N, not E, size - 1 - rr, rr),
                                   (not S, not W, rr, size - 1 - rr), (not S, not E, size - 1 - rr, size - 1 - rr)):
        if open_a and open_b:                  # round outside corners
            qx = (xx < rr) if cx == rr else (xx > size - 1 - rr)
            qy = (yy < rr) if cy == rr else (yy > size - 1 - rr)
            far = (xx - cx) ** 2 + (yy - cy) ** 2 > rr * rr
            alpha[qx & qy & far] = 0
    if theme == "Cave":                        # ragged rock edges
        rng = np.random.default_rng(idx + 3)
        for side, open_ in (("N", not N), ("S", not S), ("W", not W), ("E", not E)):
            if not open_:
                continue
            for k in range(size):
                d = rng.integers(0, 2)
                if side == "N": alpha[0:d, k] = 0
                if side == "S": alpha[size - d:, k] = 0
                if side == "W": alpha[k, 0:d] = 0
                if side == "E": alpha[k, size - d:] = 0
    arr[:, :, 3] = alpha
    img = Image.fromarray(arr, "RGBA")
    a = alpha > 0
    edge = np.zeros_like(a)
    edge[1:, :] |= a[1:, :] & ~a[:-1, :]
    edge[:-1, :] |= a[:-1, :] & ~a[1:, :]
    edge[:, 1:] |= a[:, 1:] & ~a[:, :-1]
    edge[:, :-1] |= a[:, :-1] & ~a[:, 1:]
    if not N: edge[0, :] |= a[0, :]
    if not S: edge[size - 1, :] |= a[size - 1, :]
    if not W: edge[:, 0] |= a[:, 0]
    if not E: edge[:, size - 1] |= a[:, size - 1]
    out = np.array(img)
    if not N:                                  # light lip just below the top outline
        top = np.argmax(a, axis=0)
        for x in range(size):
            if a[:, x].any():
                y = top[x]
                out[min(size - 1, y + 1), x] = t["lip2"]
                out[min(size - 1, y + 2), x] = t["lip"]
                if theme == "Cave" and (x * 7 + idx) % 5 == 0:
                    out[min(size - 1, y + 3), x] = t["lip"]
    out[edge] = OUT
    return Image.fromarray(out, "RGBA")


def oneway(theme, part, size=25):
    t = THEMES[theme]
    L = Layer(size, size)
    x0 = 3 if part == "L" else -2
    x1 = size - 3 if part == "R" else size + 2
    if theme == "Cave":                         # floating slab with drips (concept sheet)
        L.rect(x0, 1, x1, 8, t["mid"])
        L.ellipse((x0 + x1) / 2, 8, (x1 - x0) / 2, 4, t["mid"])
        rng = np.random.default_rng({"L": 1, "M": 2, "R": 3}[part])
        for _ in range(4):
            x = rng.uniform(max(x0, 2) + 2, min(x1, size - 2) - 2)
            ln = rng.uniform(3, 9)
            L.poly([(x - 2, 9), (x + 2, 9), (x, 9 + ln)], t["dark"])
    else:                                       # iron-bound stone beam
        L.rect(x0, 1, x1, 7, t["mid"])
        for bx in range(0, size, 12):
            L.rect(bx + 4, 1, bx + 6, 7, t["dark"])
        if part != "M":
            bx = 5 if part == "L" else size - 6
            L.poly([(bx - 2, 7), (bx + 2, 7), (bx + 1, 14), (bx - 1, 14)], t["dark"])
    band(L, t["dark"], "bottom", 1)
    img = outlined(L, OUT, 1)
    arr = np.array(img)
    for x in range(size):
        col = arr[:, x, 3] > 0
        if col.any():
            y = int(np.argmax(col))
            if arr[y + 1, x, 3] > 0:
                arr[y + 1, x] = t["lip2"]
            if arr[y + 2, x, 3] > 0:
                arr[y + 2, x] = t["lip"]
    if part == "M":                             # seamless across M tiles: no side outline
        arr[:, 0] = arr[:, 1]; arr[:, -1] = arr[:, -2]
    if part == "L":
        arr[:, -1] = arr[:, -2]
    if part == "R":
        arr[:, 0] = arr[:, 1]
    return Image.fromarray(arr, "RGBA")


def spikes(theme, size=25):
    t = THEMES[theme]
    L = Layer(size, size)
    L.rect(0, size - 5, size, size, t["mid"])
    for k in range(4):
        x = 3 + k * 6.3
        h = 13 + (k % 2) * 4
        L.poly([(x - 3, size - 4), (x + 3, size - 4), (x, size - 4 - h)], hexc("C9C2BC") if theme == "Cave" else hexc("C8D0DE"))
        L.poly([(x, size - 4), (x + 3, size - 4), (x, size - 4 - h)], hexc("7E7873") if theme == "Cave" else hexc("7C8496"))
    img = outlined(L, OUT, 1)
    arr = np.array(img)
    for x in range(size):                      # red tips: danger reads at a glance
        col = arr[:, x, 3] > 0
        if col.any():
            y = int(np.argmax(col))
            if y < size - 8:
                arr[y + 1:y + 3, x] = hexc("A3262B")
    arr[:, 0] = arr[:, 1]; arr[:, -1] = arr[:, -2]
    return Image.fromarray(arr, "RGBA")


def lava_tile(ph, size=25):
    arr = np.zeros((size, size, 4), np.uint8)
    n = pnoise(size, size, 3, 77, 2)
    for y in range(size):
        for x in range(size):
            u = x / size * math.tau
            surf = 4 + math.sin(u * 2 + ph * math.tau) * 1.2 + math.sin(u - ph * math.tau * 2) * 0.8
            if y < surf:
                continue
            v = (n[(y + int(ph * 6)) % size, x] + (y - surf) / size * 0.6)
            c = hexc("FFF0B0") if y < surf + 1.2 else (hexc("FFB24A") if v < 0.55 else (hexc("E2682A") if v < 0.8 else hexc("A3262B")))
            arr[y, x] = c
    return Image.fromarray(arr, "RGBA")


# ------------------------------------------------------------------ decor (no collider)
def decor(theme, k, W=50, H=50):
    t = THEMES[theme]
    L = Layer(W, H)
    if theme == "Cave":
        if k == 0:                              # stalactite cluster hanging from the top
            for x, ln, w in ((14, 30, 6), (25, 42, 8), (35, 22, 5), (42, 14, 4)):
                L.poly([(x - w, 0), (x + w, 0), (x + 1, ln), (x - 1, ln)], t["mid"])
            band(L, t["dark"], "left", 1)
        elif k == 1:                            # mushrooms
            for x, h, r in ((15, 18, 7), (27, 26, 9), (37, 12, 5)):
                L.rect(x - 1.5, H - h, x + 1.5, H - 1, t["lip"])
                L.ellipse(x, H - h, r, r * 0.55, t["light"])
                L.rect(x - r, H - h, x + r, H - h + r * 0.5, (0, 0, 0, 0))
            band(L, t["dark"], "bottom", 1)
        elif k == 2:                            # crystals
            for x, h, w, a in ((18, 22, 4, -0.25), (26, 32, 5, 0.0), (34, 18, 4, 0.3)):
                tx = x + math.sin(a) * h
                L.poly([(x - w, H - 1), (x + w, H - 1), (tx + w * 0.6, H - h * 0.85), (tx, H - h), (tx - w * 0.6, H - h * 0.85)], hexc("9FB4C8"))
                L.poly([(x, H - 1), (x + w, H - 1), (tx + w * 0.6, H - h * 0.85), (tx, H - h)], hexc("6C7F93"))
        else:                                   # broken stone arch fragment
            L.rect(8, 14, 16, H - 1, t["mid"])
            L.rect(34, 24, 42, H - 1, t["mid"])
            L.poly([(8, 14), (16, 14), (26, 8), (30, 10), (22, 18), (16, 20)], t["mid"])
            for y in range(18, H, 8):
                L.line((8, y), (16, y), t["dark"], 1)
                L.line((34, y + 4), (42, y + 4), t["dark"], 1)
            band(L, t["dark"], "left", 1)
    else:
        if k == 0:                              # torn red banner
            L.rect(10, 3, 40, 5, t["light"])
            L.poly([(13, 5), (37, 5), (37, 38), (31, 33), (25, 44), (19, 34), (13, 40)], t["accent"])
            L.poly([(21, 14), (29, 14), (25, 24)], hexc("1A0A0C"))
        elif k == 1:                            # hanging chains
            for x, ln in ((16, 40), (30, 28)):
                for y in range(0, ln, 4):
                    L.ellipse(x, y + 2, 1.6, 2.2, t["lip"])
            L.poly([(28, 28), (32, 28), (34, 33), (30, 36), (26, 33)], t["lip"])
        elif k == 2:                            # broken pillar
            L.rect(16, 16, 34, H - 1, t["mid"])
            L.rect(13, H - 6, 37, H - 1, t["light"])
            L.poly([(16, 16), (22, 11), (27, 15), (34, 9), (34, 16)], t["mid"])
            for x in (21, 26, 31):
                L.line((x, 17), (x, H - 7), t["dark"], 1)
        else:                                   # gothic window with cold light
            L.rect(14, 18, 36, H - 4, t["mid"])
            L.ellipse(25, 18, 11, 12, t["mid"])
            L.rect(17, 20, 33, H - 6, hexc("9AA8C4"))
            L.ellipse(25, 20, 8, 9, hexc("9AA8C4"))
            L.line((25, 11), (25, H - 6), t["dark"], 1)
            L.line((17, 30), (33, 30), t["dark"], 1)
    return outlined(L, OUT, 1)


# ------------------------------------------------------------------ props
def flame(L, x, y, h, ph, cols=(EMBER, EMBER_L, EMBER_W)):
    sway = math.sin(ph * math.tau) * h * 0.12
    for k, c in enumerate(cols):
        s = 1 - k * 0.32
        L.poly([(x - h * 0.28 * s, y), (x + h * 0.28 * s, y), (x + h * 0.12 * s + sway * 0.5, y - h * 0.55 * s),
                (x + sway, y - h * s), (x - h * 0.15 * s + sway * 0.3, y - h * 0.5 * s)], c)
        L.ellipse(x, y - h * 0.08, h * 0.28 * s, h * 0.2 * s, c)


def glow(img, x, y, r, col, strength=0.5):
    arr = np.array(img).astype(np.float32)
    h, w = arr.shape[:2]
    yy, xx = np.mgrid[0:h, 0:w]
    d = np.sqrt((xx - x) ** 2 + (yy - y) ** 2) / r
    g = np.clip(1 - d, 0, 1) ** 2 * strength * 255
    glow_layer = np.zeros_like(arr)
    glow_layer[:, :, :3] = col[:3]
    glow_layer[:, :, 3] = g
    base = Image.new("RGBA", (w, h))
    base.alpha_composite(Image.fromarray(glow_layer.astype(np.uint8), "RGBA"))
    base.alpha_composite(img)
    return base


def bench_frame(state, i):
    """Candle shrine: stone altar with three candles."""
    W, H = lowres_size("Bench")
    t = THEMES["Cave"]
    L = Layer(W, H)
    L.rect(20, H - 22, W - 20, H - 2, t["mid"])
    L.rect(16, H - 26, W - 16, H - 20, t["light"])
    L.rect(26, H - 18, W - 26, H - 6, t["dark"])
    L.poly([(W / 2 - 6, H - 8), (W / 2, H - 16), (W / 2 + 6, H - 8)], t["accent"])
    for cx, ch in ((W / 2 - 16, 12), (W / 2, 18), (W / 2 + 16, 10)):
        L.rect(cx - 3, H - 26 - ch, cx + 3, H - 26, hexc("D8CFC2"))
    band(L, t["dark"], "left", 1)
    img = outlined(L, OUT, 1)
    if state == "Unlit":
        return img
    fl = Layer(W, H)
    grow = 1.0 if state == "Lit" else (0.3 + i * 0.35)
    for k, (cx, ch) in enumerate(((W / 2 - 16, 12), (W / 2, 18), (W / 2 + 16, 10))):
        if state == "Activate" and k != 1 and i < 1:
            continue
        flame(fl, cx, H - 26 - ch, (9 + (2 if k == 1 else 0)) * min(1.6, grow) + math.sin(i * 1.7 + k) * 1, i / 4 + k * 0.3)
    img.alpha_composite(fl.img)
    r = 30 if state == "Lit" else 20 + i * 12
    return glow(img, W / 2, H - 50, r, EMBER, 0.35 + (0.15 if state == "Activate" and i == 2 else 0))


def door_frame(open_amt):
    """Stone slab door in a carved frame; opens by sliding up into the ceiling (pivot bottom-left)."""
    W, H = lowres_size("Door")
    t = THEMES["Cave"]
    frame = Layer(W, H)
    frame.rect(4, 0, 12, H, t["light"]); frame.rect(W - 12, 0, W - 4, H, t["light"])
    for y in range(6, H, 14):
        frame.line((4, y), (12, y), t["dark"], 1); frame.line((W - 12, y + 7), (W - 4, y + 7), t["dark"], 1)
    band(frame, t["dark"], "left", 1)
    slab = Layer(W, H)
    dy = -open_amt * (H - 8)
    slab.rect(12, dy, W - 12, H + dy, t["mid"])
    for y in range(int(dy) + 10, int(H + dy), 22):
        slab.line((12, y), (W - 12, y), t["dark"], 1)
    slab.poly([(W / 2 - 8, H * 0.5 + dy), (W / 2, H * 0.44 + dy), (W / 2 + 8, H * 0.5 + dy), (W / 2, H * 0.56 + dy)], t["accent"])
    band(slab, t["dark"], "left", 2)
    return stack(W, H, [outlined(slab, OUT, 1), outlined(frame, OUT, 1)])


def gate_frame(down):
    """Iron portcullis; `down` 0 = raised (open), 1 = shut."""
    W, H = lowres_size("Gate")
    L = Layer(W, H)
    iron, iron_d = hexc("3A3E48"), hexc("23262E")
    bottom = 10 + down * (H - 12)
    for x in range(6, W - 4, 9):
        L.rect(x, 0, x + 3, bottom, iron)
        L.poly([(x - 1, bottom), (x + 4, bottom), (x + 1.5, bottom + 6)], iron)
    for y in range(12, int(bottom), 26):
        L.rect(3, y, W - 3, y + 3, iron_d)
    L.rect(0, 0, W, 6, iron_d)
    return outlined(L, OUT, 1)


def orb_frame(i, touch=False):
    W, H = lowres_size("Orb")
    L = Layer(W, H)
    t = THEMES["Cave"]
    L.poly([(W / 2 - 14, H - 2), (W / 2 + 14, H - 2), (W / 2 + 8, H - 12), (W / 2 - 8, H - 12)], t["mid"])
    L.rect(W / 2 - 10, H - 15, W / 2 + 10, H - 12, t["light"])
    band(L, t["dark"], "left", 1)
    img = outlined(L, OUT, 1)
    bob = math.sin(i / 6 * math.tau) * 2
    cy = H / 2 - 4 + bob
    r = 10 + (i * 2.5 if touch else math.sin(i / 6 * math.tau) * 0.8)
    o = Layer(W, H)
    o.ellipse(W / 2, cy, r + 2, r + 2, hexc("1B4F9C"))
    o.ellipse(W / 2, cy, r, r, GLOW_B)
    o.ellipse(W / 2 - r * 0.15, cy - r * 0.15, r * 0.6, r * 0.6, GLOW_W)
    oi = o.img
    if touch and i >= 3:
        arr = np.array(oi).astype(np.float32)
        arr[:, :, 3] *= max(0, 1 - (i - 2) * 0.3)
        oi = Image.fromarray(arr.astype(np.uint8), "RGBA")
    img.alpha_composite(oi)
    return glow(img, W / 2, cy, 26 + (i * 5 if touch else 0), GLOW_B, 0.45)


def sign():
    W, H = lowres_size("TutorialSign")
    t = THEMES["Cave"]
    L = Layer(W, H)
    L.rect(W / 2 - 3, H - 24, W / 2 + 3, H - 1, t["mid"])
    L.poly([(12, 10), (W - 12, 8), (W - 10, H - 24), (10, H - 22)], t["light"])
    band(L, t["mid"], "left", 2)
    img = outlined(L, OUT, 1)
    g = Layer(W, H)
    for y in (20, 28, 36):
        g.line((20, y), (W - 22 - (y % 3) * 4, y), hexc("1C1A1D"), 1)
    g.poly([(W / 2 - 4, 44), (W / 2 + 4, 44), (W / 2, 50)], EMBER)
    img.alpha_composite(g.img)
    return img


def torch_frame(i):
    W, H = lowres_size("Torch")
    t = THEMES["Castle"]
    L = Layer(W, H)
    L.rect(W / 2 - 4, H - 50, W / 2 + 4, H - 10, hexc("3A3E48"))
    L.poly([(W / 2 - 12, H - 56), (W / 2 + 12, H - 56), (W / 2 + 7, H - 46), (W / 2 - 7, H - 46)], hexc("3A3E48"))
    L.rect(W / 2 - 8, H - 12, W / 2 + 8, H - 6, t["light"])
    band(L, hexc("23262E"), "left", 1)
    img = outlined(L, OUT, 1)
    fl = Layer(W, H)
    flame(fl, W / 2, H - 55, 22 + math.sin(i / 4 * math.tau) * 3, i / 4)
    img.alpha_composite(fl.img)
    return glow(img, W / 2, H - 66, 40, EMBER, 0.35)


def chandelier():
    W, H = lowres_size("Chandelier")
    iron = hexc("3A3E48")
    L = Layer(W, H)
    for x in (20, W / 2, W - 20):               # chains up to the ceiling
        for y in range(0, 26, 4):
            L.ellipse(x, y + 2, 1.4, 2, iron)
    L.rect(6, 26, W - 6, 32, iron)               # standing surface (one-way collider on top)
    L.poly([(6, 32), (W - 6, 32), (W - 24, 44), (24, 44)], hexc("23262E"))
    for x in range(18, W - 10, 24):
        L.rect(x - 2, 44, x + 2, 52, iron)
        L.rect(x - 2.5, 52, x + 2.5, 60, hexc("D8CFC2"))
    img = outlined(L, OUT, 1)
    fl = Layer(W, H)
    for k, x in enumerate(range(18, W - 10, 24)):
        flame(fl, x, 72, 9, k * 0.31)
    img.alpha_composite(fl.img)
    arr = np.array(img)
    arr[26, 6:W - 6] = hexc("8F99AD")
    return Image.fromarray(arr, "RGBA")


def build_tiles():
    out = {}
    for th in ("Cave", "Castle"):
        for i in range(16):
            out[f"Tile_{th}_{i:02d}"] = {"": [tile(th, i)]}
        for part in "LMR":
            out[f"Tile_{th}_OneWay_{part}"] = {"": [oneway(th, part)]}
        out[f"Tile_{th}_Spikes"] = {"": [spikes(th)]}
        for k in range(4):
            out[f"Decor_{th}_{k:02d}"] = {"": [decor(th, k)]}
    out["Tile_Castle_Lava"] = {"": [lava_tile(i / 4) for i in range(4)]}
    return out


def build_props():
    return {
        "Bench": {"Unlit": [bench_frame("Unlit", 0)], "Lit": [bench_frame("Lit", i) for i in range(4)],
                  "Activate": [bench_frame("Activate", i) for i in range(4)]},
        "Door": {"Closed": [door_frame(0)], "Open": [door_frame((i + 1) / 6) for i in range(6)]},
        "Gate": {"Open": [gate_frame(0)], "Close": [gate_frame((i + 1) / 5) for i in range(5)]},
        "Orb": {"Idle": [orb_frame(i) for i in range(6)], "Touch": [orb_frame(i, True) for i in range(6)]},
        "TutorialSign": {"": [sign()]},
        "Torch": {"": [torch_frame(i) for i in range(4)]},
        "Chandelier": {"": [chandelier()]},
    }
