"""Parchment, flame, parallax backgrounds and cutscene panels: soft painterly greyscale (environment concept sheet).
Painted at half resolution with soft edges, then resized up smoothly; written with lowres=False."""
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
from core import hexc, mix
from env import pnoise

EMBER = hexc("E2682A"); CRIMSON = hexc("C1272D"); BLUE = hexc("4FA3FF")


def rgba(arr):
    return Image.fromarray(np.clip(arr, 0, 255).astype(np.uint8), "RGBA")


def gradient(w, h, top, bottom):
    t = np.linspace(0, 1, h)[:, None, None]
    a = np.array(top, np.float32)[None, None, :]
    b = np.array(bottom, np.float32)[None, None, :]
    return np.repeat(a * (1 - t) + b * t, w, axis=1)


def texture(img, strength, seed, cells=24):
    """Multiply luminance by soft noise so flat fills read as painted stone / paper."""
    arr = np.array(img).astype(np.float32)
    h, w = arr.shape[:2]
    n = pnoise(w, h, cells, seed, 4)
    arr[:, :, :3] *= (1 - strength / 2 + n[:, :, None] * strength)
    return rgba(arr)


class Canvas:
    """Half-resolution paint canvas with x-wrapping shapes for tileable layers."""

    def __init__(self, w, h, wrap=False, bg=(0, 0, 0, 0)):
        self.w, self.h, self.wrap = w, h, wrap
        self.img = Image.new("RGBA", (w, h), bg)

    def layer(self):
        return Image.new("RGBA", (self.w, self.h), (0, 0, 0, 0))

    def put(self, lay, blur=0.0, tex=0.0, seed=1):
        if tex:
            lay = texture(lay, tex, seed)
        if blur:
            lay = lay.filter(ImageFilter.GaussianBlur(blur))
        self.img.alpha_composite(lay)

    def poly(self, d, pts, col):
        shifts = (-self.w, 0, self.w) if self.wrap else (0,)
        for s in shifts:
            d.polygon([(x + s, y) for x, y in pts], fill=tuple(col))

    def ellipse(self, d, cx, cy, rx, ry, col):
        shifts = (-self.w, 0, self.w) if self.wrap else (0,)
        for s in shifts:
            d.ellipse([cx - rx + s, cy - ry, cx + rx + s, cy + ry], fill=tuple(col))

    def rect(self, d, x0, y0, x1, y1, col):
        shifts = (-self.w, 0, self.w) if self.wrap else (0,)
        for s in shifts:
            d.rectangle([x0 + s, y0, x1 + s, y1], fill=tuple(col))

    def final(self, W, H):
        return self.img.resize((W, H), Image.BICUBIC)


def with_alpha(c, a):
    return (c[0], c[1], c[2], int(a))


# ------------------------------------------------------------------ parchment, frame, flame
def parchment(W, H, base, light, dark, fibre, seed):
    n1 = pnoise(W // 2, H // 2, 6, seed, 4)
    n2 = pnoise(W // 2, H // 2, 40, seed + 1, 2)
    arr = np.zeros((H // 2, W // 2, 4), np.float32)
    b, l, d = (np.array(c[:3], np.float32) for c in (base, light, dark))
    t = n1[:, :, None]
    arr[:, :, :3] = np.where(t > 0.5, b + (l - b) * (t - 0.5) * 2, d + (b - d) * t * 2)
    arr[:, :, :3] *= (0.94 + n2[:, :, None] * 0.12)
    arr[:, :, 3] = 255
    img = rgba(arr)
    d2 = ImageDraw.Draw(img)
    rng = np.random.default_rng(seed + 2)
    w, h = W // 2, H // 2
    for _ in range(int(w * h / 2500)):                  # paper fibres, wrapped so the texture stays seamless
        x, y = rng.uniform(0, w), rng.uniform(0, h)
        a = rng.uniform(0, math.pi)
        ln = rng.uniform(4, 14)
        x2, y2 = x + math.cos(a) * ln, y + math.sin(a) * ln
        for sx in (-w, 0, w):
            for sy in (-h, 0, h):
                d2.line([(x + sx, y + sy), (x2 + sx, y2 + sy)], fill=with_alpha(fibre, 28), width=1)
    return img.resize((W, H), Image.BICUBIC)


def parchment_frame(W, H):
    """Burnt, ragged-edged vignette overlay (alpha), tinted per theme by code."""
    w, h = W // 4, H // 4
    yy, xx = np.mgrid[0:h, 0:w]
    dx = np.minimum(xx, w - 1 - xx) / (w * 0.5)
    dy = np.minimum(yy, h - 1 - yy) / (h * 0.5)
    edge = np.minimum(dx * 1.6, dy * 1.9)
    n = pnoise(w, h, 10, 31, 4)
    v = edge + (n - 0.5) * 0.25
    a = np.clip(1 - v * 2.2, 0, 1) ** 1.6
    arr = np.zeros((h, w, 4), np.float32)
    arr[:, :, :3] = 12
    arr[:, :, 3] = a * 245
    burn = (v > 0.04) & (v < 0.07)                      # thin ember line along the burnt edge
    arr[burn, 0], arr[burn, 1], arr[burn, 2], arr[burn, 3] = 90, 40, 24, 180
    return rgba(arr).filter(ImageFilter.GaussianBlur(0.6)).resize((W, H), Image.BICUBIC)


def flame_glow(W, H):
    yy, xx = np.mgrid[0:H, 0:W]
    d = np.sqrt((xx - W / 2) ** 2 + (yy - H / 2) ** 2) / (W / 2)
    arr = np.zeros((H, W, 4), np.float32)
    arr[:, :, :3] = 255
    arr[:, :, 3] = np.clip(1 - d, 0, 1) ** 2.2 * 255
    return rgba(arr)


def flame_core(W, H, i):
    c = Canvas(W, H)
    lay = c.layer(); d = ImageDraw.Draw(lay)
    ph = i / 6 * math.tau
    for k, (s, a) in enumerate(((1.0, 120), (0.7, 190), (0.42, 255))):
        sw = math.sin(ph + k) * 12 * s
        pts = [(W / 2 - 56 * s, H * 0.86), (W / 2 + 56 * s, H * 0.86), (W / 2 + 30 * s + sw * 0.5, H * 0.55),
               (W / 2 + sw + math.sin(ph * 2) * 6, H * (0.86 - 0.7 * s)), (W / 2 - 26 * s + sw * 0.3, H * 0.5)]
        d.polygon(pts, fill=(255, 255, 255, a))
        d.ellipse([W / 2 - 56 * s, H * 0.86 - 50 * s, W / 2 + 56 * s, H * 0.86 + 20 * s], fill=(255, 255, 255, a))
    c.put(lay, blur=9)
    return c.img


# ------------------------------------------------------------------ cave / castle parallax
def stalactite(c, d, x, y0, w, ln, col):
    c.poly(d, [(x - w, y0), (x + w, y0), (x + w * 0.35, y0 + ln * 0.6), (x + 2, y0 + ln), (x - w * 0.4, y0 + ln * 0.55)], col)


def stalagmite(c, d, x, y0, w, ln, col):
    c.poly(d, [(x - w, y0), (x + w, y0), (x + w * 0.3, y0 - ln * 0.6), (x, y0 - ln), (x - w * 0.35, y0 - ln * 0.55)], col)


def ragged_band(c, d, y, amp, col, seed, top=True, step=14):
    rng = np.random.default_rng(seed)
    pts = []
    for x in range(0, c.w + step, step):
        pts.append((x, y + rng.uniform(-amp, amp) + math.sin(x / c.w * math.tau * 3) * amp))
    pts[-1] = (c.w + step, pts[0][1])
    if top:
        poly = [(0, -5)] + pts + [(c.w + step, -5)]
    else:
        poly = [(0, c.h + 5)] + pts + [(c.w + step, c.h + 5)]
    c.poly(d, poly, col)


def bg_cave_far(W, H):
    c = Canvas(W // 2, H // 2, wrap=True)
    w, h = c.w, c.h
    col = (70, 66, 70, 150)
    lay = c.layer(); d = ImageDraw.Draw(lay)
    ragged_band(c, d, h * 0.12, 10, col, 1)
    rng = np.random.default_rng(3)
    for k in range(18):
        stalactite(c, d, rng.uniform(0, w), h * 0.1, rng.uniform(10, 26), rng.uniform(50, 170), col)
    for k in range(6):                                  # distant pillars
        x = (k + rng.uniform(0, 0.6)) * w / 6
        pw = rng.uniform(16, 30)
        c.poly(d, [(x - pw, h), (x + pw, h), (x + pw * 0.6, h * 0.2), (x - pw * 0.6, h * 0.2)], col)
    ragged_band(c, d, h * 0.82, 12, col, 2, top=False)
    c.put(lay, blur=3, tex=0.25, seed=5)
    fog = c.layer(); d = ImageDraw.Draw(fog)
    for k in range(5):
        c.ellipse(d, k * w / 5 + 60, h * 0.72, w * 0.18, 26, (190, 186, 182, 46))
    c.put(fog, blur=18)
    return c.final(W, H)


def bg_cave_mid(W, H):
    c = Canvas(W // 2, H // 2, wrap=True)
    w, h = c.w, c.h
    col = (38, 35, 39, 235)
    lay = c.layer(); d = ImageDraw.Draw(lay)
    ragged_band(c, d, h * 0.06, 8, col, 11)
    rng = np.random.default_rng(13)
    for k in range(10):
        stalactite(c, d, rng.uniform(0, w), h * 0.05, rng.uniform(14, 34), rng.uniform(60, 200), col)
    for k in range(3):                                  # ruined arches like the concept sheet
        x = k * w / 3 + rng.uniform(40, 120)
        aw, ah = rng.uniform(70, 110), rng.uniform(150, 210)
        broken = rng.uniform(0.55, 1.0)
        c.rect(d, x - aw, h - ah, x - aw + 26, h, col)
        if broken > 0.9:
            c.rect(d, x + aw - 26, h - ah, x + aw, h, col)
        else:
            c.rect(d, x + aw - 26, h - ah * 0.45, x + aw, h, col)
        a_end = math.pi * (1 - broken)
        outer = [(x + math.cos(a) * aw, h - ah - math.sin(a) * aw * 0.7) for a in np.linspace(math.pi, a_end, 16)]
        inner = [(x + math.cos(a) * (aw - 26), h - ah - math.sin(a) * (aw - 26) * 0.7 + 4) for a in np.linspace(a_end, math.pi, 16)]
        c.poly(d, outer + inner, col)
    for k in range(4):                                  # floating rocks with drips
        x, y = rng.uniform(0, w), rng.uniform(h * 0.35, h * 0.55)
        rw = rng.uniform(30, 55)
        c.poly(d, [(x - rw, y), (x + rw, y), (x + rw * 0.7, y + 14), (x + 8, y + 34), (x - rw * 0.6, y + 16)], col)
        for j in range(3):
            dx = rng.uniform(-rw * 0.6, rw * 0.6)
            c.poly(d, [(x + dx - 3, y + 12), (x + dx + 3, y + 12), (x + dx, y + 12 + rng.uniform(10, 30))], col)
    ragged_band(c, d, h * 0.9, 8, col, 12, top=False)
    for k in range(12):
        stalagmite(c, d, rng.uniform(0, w), h * 0.92, rng.uniform(8, 18), rng.uniform(20, 60), col)
    c.put(lay, blur=0.8, tex=0.35, seed=15)
    rim = c.layer(); d = ImageDraw.Draw(rim)          # faint crimson glints on a few rocks
    for k in range(8):
        c.ellipse(d, rng.uniform(0, w), rng.uniform(h * 0.6, h * 0.9), 2, 2, with_alpha(CRIMSON, 160))
    c.put(rim, blur=1.5)
    return c.final(W, H)


def bg_castle_far(W, H):
    c = Canvas(W // 2, H // 2, wrap=True)
    w, h = c.w, c.h
    col = (58, 64, 82, 150)
    lay = c.layer(); d = ImageDraw.Draw(lay)
    rng = np.random.default_rng(21)
    ragged_band(c, d, h * 0.86, 6, col, 22, top=False)
    for k in range(9):                                  # towers with pointed roofs
        x = (k + rng.uniform(0, 0.5)) * w / 9
        tw, th = rng.uniform(16, 30), rng.uniform(140, 330)
        top = h * 0.88 - th
        c.rect(d, x - tw, top, x + tw, h, col)
        c.poly(d, [(x - tw - 6, top), (x + tw + 6, top), (x, top - tw * 2.4)], col)
        for j in range(int(th / 40)):
            c.rect(d, x - 3, top + 20 + j * 40, x + 3, top + 30 + j * 40, (150, 165, 200, 70))
    for k in range(4):                                  # connecting walls
        x = k * w / 4
        c.rect(d, x, h * 0.66, x + w / 6, h, col)
        for j in range(8):
            c.rect(d, x + j * w / 48, h * 0.66 - 8, x + j * w / 48 + 6, h * 0.66, col)
    c.put(lay, blur=2.5, tex=0.2, seed=23)
    fog = c.layer(); d = ImageDraw.Draw(fog)
    for k in range(5):
        c.ellipse(d, k * w / 5 + 40, h * 0.8, w * 0.2, 24, (175, 186, 210, 40))
    c.put(fog, blur=18)
    return c.final(W, H)


def bg_castle_mid(W, H):
    c = Canvas(W // 2, H // 2, wrap=True)
    w, h = c.w, c.h
    col = (30, 33, 43, 235)
    lay = c.layer(); d = ImageDraw.Draw(lay)
    rng = np.random.default_rng(31)
    c.rect(d, 0, 0, w, h * 0.07, col)
    for k in range(4):                                  # gothic arcade: pillars + pointed arches
        x = k * w / 4
        pw = 24
        c.rect(d, x - pw, 0, x + pw, h, col)
        c.rect(d, x - pw - 8, h * 0.86, x + pw + 8, h, col)
        span = w / 4
        top = h * 0.07
        c.poly(d, [(x + pw, top), (x + pw, top + 90), (x + span * 0.3, top + 40), (x + span / 2, top + 20),
                   (x + span * 0.7, top + 40), (x + span - pw, top + 90), (x + span - pw, top)], col)
    for k in range(6):                                  # hanging chains and torn banners
        x = rng.uniform(0, w)
        ln = rng.uniform(60, 180)
        for y in range(int(h * 0.07), int(h * 0.07 + ln), 7):
            c.ellipse(d, x, y, 2.2, 3.2, col)
        if k % 2 == 0:
            by = h * 0.07 + ln
            c.poly(d, [(x - 18, by), (x + 18, by), (x + 18, by + 70), (x + 6, by + 58), (x - 4, by + 80), (x - 18, by + 62)], (80, 22, 28, 230))
    ragged_band(c, d, h * 0.93, 3, col, 32, top=False, step=40)
    c.put(lay, blur=0.8, tex=0.3, seed=35)
    return c.final(W, H)


# ------------------------------------------------------------------ cutscene panels (opaque, painterly)
def panel(W, H, top=(52, 50, 54), bottom=(118, 112, 106)):
    c = Canvas(W // 2, H // 2)
    c.img = texture(rgba(np.concatenate([gradient(c.w, c.h, top, bottom), np.full((c.h, c.w, 1), 255, np.float32)], axis=2)), 0.18, 41)
    return c


def silhouette_figure(c, d, x, ground, s, col, hood=False, ears=False, sword=0):
    """Generic puppet figure (head, body, legs) used in memory panels."""
    c.poly(d, [(x - 14 * s, ground - 50 * s), (x + 14 * s, ground - 50 * s), (x + 18 * s, ground - 110 * s), (x - 18 * s, ground - 110 * s)], col)
    c.poly(d, [(x - 12 * s, ground), (x - 4 * s, ground), (x - 2 * s, ground - 52 * s), (x - 14 * s, ground - 52 * s)], col)
    c.poly(d, [(x + 4 * s, ground), (x + 12 * s, ground), (x + 14 * s, ground - 52 * s), (x + 2 * s, ground - 52 * s)], col)
    c.ellipse(d, x, ground - 128 * s, 17 * s, 18 * s, col)
    if hood:
        c.poly(d, [(x - 22 * s, ground - 118 * s), (x, ground - 165 * s), (x + 22 * s, ground - 118 * s)], col)
        c.poly(d, [(x - 26 * s, ground - 110 * s), (x + 26 * s, ground - 110 * s), (x + 36 * s, ground), (x - 36 * s, ground)], col)
    if ears:
        c.poly(d, [(x - 14 * s, ground - 138 * s), (x - 8 * s, ground - 170 * s), (x - 2 * s, ground - 142 * s)], col)
        c.poly(d, [(x + 2 * s, ground - 142 * s), (x + 9 * s, ground - 172 * s), (x + 14 * s, ground - 138 * s)], col)
    if sword:
        c.poly(d, [(x + 16 * s, ground - 90 * s), (x + 22 * s, ground - 94 * s), (x + 22 * s + sword * 90 * s, ground - 150 * s), (x + 16 * s + sword * 86 * s, ground - 146 * s)], col)


def cs_village(W, H, layer):
    c = Canvas(W // 2, H // 2)
    w, h = c.w, c.h
    rng = np.random.default_rng(51 + layer)
    if layer == 0:
        c.img = rgba(np.concatenate([gradient(w, h, (60, 58, 64), (150, 140, 128)), np.full((h, w, 1), 255, np.float32)], axis=2))
        lay = c.layer(); d = ImageDraw.Draw(lay)
        c.ellipse(d, w * 0.7, h * 0.28, 46, 46, (225, 215, 196, 255))
        pts = [(0, h)] + [(x, h * 0.55 - 70 * math.sin(x / w * 5.2) - 40 * math.sin(x / w * 13 + 1)) for x in np.linspace(0, w, 40)] + [(w, h)]
        c.poly(d, pts, (88, 84, 88, 255))
        c.put(lay, blur=2, tex=0.2, seed=52)
        return c.final(W, H)
    lay = c.layer(); d = ImageDraw.Draw(lay)
    if layer == 1:
        col = (36, 33, 37, 255)
        c.rect(d, 0, h * 0.86, w, h, col)
        x = 20
        while x < w:
            hw, hh = rng.uniform(60, 110), rng.uniform(80, 150)
            base = h * 0.88
            c.rect(d, x, base - hh, x + hw, base, col)
            c.poly(d, [(x - 22, base - hh), (x + hw + 22, base - hh), (x + hw * 0.75, base - hh - 46), (x + hw * 0.25, base - hh - 46)], col)
            c.poly(d, [(x - 22, base - hh), (x - 30, base - hh - 14), (x - 4, base - hh - 8)], col)
            c.poly(d, [(x + hw + 22, base - hh), (x + hw + 30, base - hh - 14), (x + hw + 4, base - hh - 8)], col)
            c.rect(d, x + hw * 0.4, base - hh * 0.55, x + hw * 0.6, base - hh * 0.3, with_alpha(EMBER, 220))
            x += hw + rng.uniform(40, 110)
        c.put(lay, blur=0.6, tex=0.2, seed=53)
    else:
        col = (16, 14, 16, 255)
        c.rect(d, 0, h * 0.94, w, h, col)
        for k in range(int(w / 26)):
            x = k * 26 + rng.uniform(-4, 4)
            c.rect(d, x, h * 0.84, x + 6, h, col)
        c.rect(d, 0, h * 0.86, w, h * 0.875, col)
        for k in range(40):
            x = rng.uniform(0, w)
            c.poly(d, [(x - 6, h * 0.95), (x, h * 0.95 - rng.uniform(14, 34)), (x + 6, h * 0.95)], col)
        lx = w * 0.62
        c.rect(d, lx, h * 0.3, lx + 8, h, col)
        c.rect(d, lx, h * 0.3, lx + 50, h * 0.32, col)
        c.rect(d, lx + 38, h * 0.32, lx + 52, h * 0.4, with_alpha(EMBER, 255))
        c.put(lay)
        glow = c.layer(); d = ImageDraw.Draw(glow)
        c.ellipse(d, lx + 45, h * 0.36, 40, 40, with_alpha(EMBER, 90))
        c.put(glow, blur=12)
    return c.final(W, H)


def cs_cave_mouth(W, H):
    c = panel(W, H, (40, 38, 42), (110, 104, 98))
    w, h = c.w, c.h
    lay = c.layer(); d = ImageDraw.Draw(lay)
    col = (20, 18, 21, 255)
    c.rect(d, 0, 0, w, h, col)
    pts = [(w * 0.5 + math.cos(a) * w * 0.26, h * 0.85 - math.sin(a) * h * 0.62 + math.sin(a * 9) * 10) for a in np.linspace(0, math.pi, 30)]
    hole = Image.new("L", (w, h), 0)
    ImageDraw.Draw(hole).polygon(pts + [(w * 0.76, h), (w * 0.24, h)], fill=255)
    arr = np.array(lay); arr[np.array(hole) > 0, 3] = 0
    lay = Image.fromarray(arr, "RGBA"); d = ImageDraw.Draw(lay)
    rng = np.random.default_rng(61)
    for k in range(9):
        stalactite(c, d, w * 0.28 + k * w * 0.055, h * 0.24 + abs(k - 4) * 14, rng.uniform(8, 16), rng.uniform(30, 90), col)
    c.rect(d, 0, h * 0.86, w, h, col)
    c.put(lay, blur=0.8, tex=0.25, seed=62)
    fig = c.layer(); d = ImageDraw.Draw(fig)
    silhouette_figure(c, d, w * 0.5, h * 0.86, 0.55, (10, 9, 11, 255), ears=True)
    c.put(fig)
    eye = c.layer(); d = ImageDraw.Draw(eye)
    c.ellipse(d, w * 0.5 + 4, h * 0.86 - 70, 3, 3, with_alpha(BLUE, 255))
    c.put(eye, blur=0.5)
    return c.final(W, H)


def cs_memory(W, H, k):
    c = panel(W, H, (34, 32, 36), (96, 92, 90))
    w, h = c.w, c.h
    lay = c.layer(); d = ImageDraw.Draw(lay)
    blk = (12, 11, 13, 255)
    g = h * 0.85
    c.rect(d, 0, g, w, h, blk)
    glow_at = None
    if k == 1:                                         # a figure of light and shadow
        silhouette_figure(c, d, w * 0.5, g, 1.6, blk)
        glow_at = (w * 0.5, g - 180, 160, (230, 236, 255))
    elif k == 2:                                       # a crown of fire
        cx, cy = w * 0.5, h * 0.45
        c.ellipse(d, cx, cy + 40, 110, 26, blk)
        for j in range(7):
            x = cx - 96 + j * 32
            c.poly(d, [(x - 14, cy + 30), (x + 14, cy + 30), (x, cy - 50 - (j % 2) * 30)], blk)
        glow_at = (cx, cy - 20, 200, EMBER[:3])
    elif k == 3:                                       # a great hall
        for j in range(6):
            x = w * 0.1 + j * w * 0.16
            c.rect(d, x - 22, h * 0.12, x + 22, g, blk)
            c.poly(d, [(x + 22, h * 0.12), (x + w * 0.16 - 22, h * 0.12), (x + w * 0.08, h * 0.3)], blk) if j < 5 else None
        c.rect(d, 0, 0, w, h * 0.12, blk)
        glow_at = (w * 0.5, h * 0.55, 180, (210, 205, 195))
    elif k == 4:                                       # two beings divided
        silhouette_figure(c, d, w * 0.32, g, 1.4, (232, 238, 248, 255), ears=True)
        silhouette_figure(c, d, w * 0.68, g, 1.4, blk, sword=1)
        c.rect(d, w * 0.5 - 3, 0, w * 0.5 + 3, g, with_alpha(CRIMSON, 255))
    elif k == 5:                                       # a pale hand reaching
        c.poly(d, [(w * 0.1, h * 0.95), (w * 0.2, h), (w * 0.48, h * 0.55), (w * 0.44, h * 0.5)], (220, 224, 232, 255))
        for j in range(4):
            a = -0.9 + j * 0.3
            bx, by = w * 0.46, h * 0.52
            c.poly(d, [(bx - 6, by), (bx + 6, by), (bx + math.cos(a) * 80, by + math.sin(a) * 80 - 20)], (220, 224, 232, 255))
        glow_at = (w * 0.62, h * 0.32, 120, (255, 255, 255))
    else:                                              # a castle on a cliff
        c.poly(d, [(w * 0.4, h), (w * 0.52, h * 0.55), (w * 0.8, h * 0.5), (w * 0.95, h)], blk)
        for x, th in ((0.58, 220), (0.66, 300), (0.74, 180)):
            c.rect(d, w * x - 18, h * 0.52 - th, w * x + 18, h * 0.55, blk)
            c.poly(d, [(w * x - 26, h * 0.52 - th), (w * x + 26, h * 0.52 - th), (w * x, h * 0.52 - th - 60)], blk)
        glow_at = (w * 0.66, h * 0.52 - 240, 30, CRIMSON[:3])
    c.put(lay, blur=0.6, tex=0.2, seed=70 + k)
    if glow_at:
        gx, gy, r, gc = glow_at
        gl = c.layer(); d = ImageDraw.Draw(gl)
        d.ellipse([gx - r, gy - r, gx + r, gy + r], fill=(gc[0], gc[1], gc[2], 120))
        c.img.alpha_composite(gl.filter(ImageFilter.GaussianBlur(r * 0.45)))
    return c.final(W, H)


def cs_zulmat_eyes(W, H):
    c = Canvas(W // 2, H // 2, bg=(8, 6, 8, 255))
    w, h = c.w, c.h
    gl = c.layer(); d = ImageDraw.Draw(gl)
    for ex in (w * 0.4, w * 0.6):
        d.ellipse([ex - 70, h * 0.45 - 40, ex + 70, h * 0.45 + 40], fill=(193, 39, 45, 120))
    c.put(gl, blur=30)
    ey = c.layer(); d = ImageDraw.Draw(ey)
    for ex, s in ((w * 0.4, -1), (w * 0.6, 1)):
        d.polygon([(ex - 44 * s, h * 0.45 + 6), (ex + 40 * s, h * 0.45 - 12), (ex + 30 * s, h * 0.45 + 8)], fill=(255, 59, 48, 255))
        d.polygon([(ex - 20 * s, h * 0.45 + 2), (ex + 26 * s, h * 0.45 - 6), (ex + 18 * s, h * 0.45 + 4)], fill=(255, 210, 190, 255))
    c.put(ey, blur=0.6)
    return c.final(W, H)


def cs_castle(W, H):
    c = panel(W, H, (30, 18, 22), (120, 52, 44))
    w, h = c.w, c.h
    lay = c.layer(); d = ImageDraw.Draw(lay)
    blk = (10, 8, 10, 255)
    c.poly(d, [(0, h), (0, h * 0.82), (w * 0.3, h * 0.7), (w * 0.7, h * 0.68), (w, h * 0.8), (w, h)], blk)
    for x, th, tw in ((0.3, 260, 24), (0.4, 340, 30), (0.5, 430, 36), (0.6, 320, 28), (0.7, 240, 22)):
        c.rect(d, w * x - tw, h * 0.7 - th, w * x + tw, h * 0.72, blk)
        c.poly(d, [(w * x - tw - 8, h * 0.7 - th), (w * x + tw + 8, h * 0.7 - th), (w * x, h * 0.7 - th - tw * 2.6)], blk)
    c.rect(d, w * 0.3, h * 0.5, w * 0.7, h * 0.72, blk)
    c.put(lay, blur=0.5, tex=0.15, seed=81)
    win = c.layer(); d = ImageDraw.Draw(win)
    d.rectangle([w * 0.5 - 6, h * 0.7 - 380, w * 0.5 + 6, h * 0.7 - 356], fill=(255, 90, 60, 255))
    c.put(win)
    gl = c.layer(); d = ImageDraw.Draw(gl)
    d.ellipse([w * 0.5 - 40, h * 0.7 - 408, w * 0.5 + 40, h * 0.7 - 328], fill=(255, 80, 50, 110))
    c.put(gl, blur=14)
    return c.final(W, H)


def cs_merge(W, H, k):
    c = panel(W, H, (28, 26, 30), (90, 86, 84))
    w, h = c.w, c.h
    g = h * 0.88
    lay = c.layer(); d = ImageDraw.Draw(lay)
    c.rect(d, 0, g, w, h, (12, 11, 13, 255))
    if k == 1:                                         # the god whole: half pale, half dark, one figure
        silhouette_figure(c, d, w * 0.5, g, 2.0, (40, 38, 44, 255), hood=True)
        glow = (w * 0.5, g - 220, 240, (235, 235, 240))
    elif k == 2:                                       # torn apart along the middle
        silhouette_figure(c, d, w * 0.47, g, 2.0, (232, 238, 248, 255), hood=True)
        silhouette_figure(c, d, w * 0.53, g, 2.0, (14, 12, 15, 255), hood=True)
        c.rect(d, w * 0.5 - 2, h * 0.1, w * 0.5 + 2, g, with_alpha(CRIMSON, 255))
        glow = (w * 0.5, g - 220, 200, CRIMSON[:3])
    else:                                              # mercy and cruelty walk away from each other
        silhouette_figure(c, d, w * 0.3, g, 1.5, (232, 238, 248, 255), ears=True)
        silhouette_figure(c, d, w * 0.7, g, 1.7, (14, 12, 15, 255), hood=True, sword=1)
        glow = (w * 0.3, g - 150, 140, (180, 210, 255))
    c.put(lay, blur=0.5, tex=0.15, seed=90 + k)
    gx, gy, r, gc = glow
    gl = c.layer(); d = ImageDraw.Draw(gl)
    d.ellipse([gx - r, gy - r, gx + r, gy + r], fill=(gc[0], gc[1], gc[2], 90))
    c.img.alpha_composite(gl.filter(ImageFilter.GaussianBlur(r * 0.4)))
    return c.final(W, H)


def cs_gods_stab(W, H):
    c = panel(W, H, (24, 20, 24), (84, 72, 72))
    w, h = c.w, c.h
    g = h * 0.88
    lay = c.layer(); d = ImageDraw.Draw(lay)
    blk = (12, 11, 13, 255)
    c.rect(d, 0, g, w, h, blk)
    for x, s in ((0.2, 2.2), (0.8, 2.2), (0.5, 2.6)):
        silhouette_figure(c, d, w * x, g if x != 0.5 else g - 40, s * 0.9, (30, 28, 32, 255), hood=True)
    silhouette_figure(c, d, w * 0.5, g, 1.6, (200, 200, 210, 255), ears=True)
    c.poly(d, [(w * 0.24, g - 200), (w * 0.27, g - 210), (w * 0.5, g - 160), (w * 0.49, g - 150)], (220, 220, 230, 255))
    c.put(lay, blur=0.5, tex=0.15, seed=95)
    gl = c.layer(); d = ImageDraw.Draw(gl)
    d.ellipse([w * 0.5 - 60, g - 220, w * 0.5 + 60, g - 100], fill=(193, 39, 45, 150))
    c.img.alpha_composite(gl.filter(ImageFilter.GaussianBlur(24)))
    return c.final(W, H)


def cs_split(W, H):
    c = Canvas(W // 2, H // 2, bg=(10, 9, 11, 255))
    w, h = c.w, c.h
    left = c.layer(); d = ImageDraw.Draw(left)
    d.rectangle([0, 0, w * 0.5, h], fill=(150, 160, 178, 255))
    c.put(left, blur=40)
    lay = c.layer(); d = ImageDraw.Draw(lay)
    g = h * 0.86
    silhouette_figure(c, d, w * 0.38, g, 1.5, (245, 248, 255, 255), ears=True)
    silhouette_figure(c, d, w * 0.62, g, 1.5, (6, 5, 7, 255), sword=1)
    c.rect(d, w * 0.5 - 2, 0, w * 0.5 + 2, h, with_alpha(CRIMSON, 255))
    c.put(lay, blur=0.5)
    return c.final(W, H)


def god(W, H, k):
    """Hooded god puppet (transparent), three variants: staff, scales, blade."""
    c = Canvas(W // 2, H // 2)
    w, h = c.w, c.h
    lay = c.layer(); d = ImageDraw.Draw(lay)
    col = (22, 20, 24, 255)
    cx, g = w / 2, h - 4
    d.polygon([(cx - 70, g), (cx + 70, g), (cx + 44, h * 0.35), (cx - 44, h * 0.35)], fill=col)
    d.polygon([(cx - 48, h * 0.38), (cx + 48, h * 0.38), (cx + 30, h * 0.18), (cx + 6, h * 0.06), (cx - 30, h * 0.2)], fill=col)
    if k == 0:
        d.rectangle([cx + 56, h * 0.08, cx + 64, g], fill=col)
        d.ellipse([cx + 48, h * 0.04, cx + 72, h * 0.1], fill=col)
    elif k == 1:
        d.rectangle([cx + 48, h * 0.4, cx + 110, h * 0.42], fill=col)
        for x in (cx + 52, cx + 106):
            d.line([(x, h * 0.42), (x, h * 0.5)], fill=col, width=2)
            d.chord([x - 16, h * 0.46, x + 16, h * 0.56], 0, 180, fill=col)
        d.rectangle([cx + 77, h * 0.36, cx + 81, h * 0.5], fill=col)
    else:
        d.polygon([(cx + 40, h * 0.5), (cx + 50, h * 0.48), (cx + 120, h * 0.1), (cx + 112, h * 0.08)], fill=(70, 66, 74, 255))
    c.put(lay)
    eye = c.layer(); d = ImageDraw.Draw(eye)
    accent = [(235, 225, 190), (180, 210, 255), (255, 90, 70)][k]
    d.ellipse([cx + 6, h * 0.2, cx + 16, h * 0.23], fill=(accent[0], accent[1], accent[2], 255))
    c.put(eye)
    rimg = c.layer(); d = ImageDraw.Draw(rimg)
    d.line([(cx + 44, h * 0.35), (cx + 70, g)], fill=(accent[0], accent[1], accent[2], 160), width=2)
    c.put(rimg)
    return c.img.resize((W, H), Image.NEAREST)


def build():
    P1 = (138, 131, 121); P1L = (170, 163, 152); P1D = (104, 98, 90)
    out = {
        "Parchment_Beige": parchment(2048, 2048, P1, P1L, P1D, (200, 194, 184), 101),
        "Parchment_Inverted": parchment(2048, 2048, (34, 40, 56), (52, 60, 80), (22, 26, 38), (150, 162, 190), 102),
        "ParchmentFrame": parchment_frame(1920, 1080),
        "FlameGlow": flame_glow(1024, 1024),
        "BG_Cave_Far": bg_cave_far(2048, 1080), "BG_Cave_Mid": bg_cave_mid(2048, 1080),
        "BG_Castle_Far": bg_castle_far(2048, 1080), "BG_Castle_Mid": bg_castle_mid(2048, 1080),
        "CS1_Village_Far": cs_village(2400, 1080, 0), "CS1_Village_Mid": cs_village(2400, 1080, 1),
        "CS1_Village_Near": cs_village(2400, 1080, 2), "CS1_CaveMouth": cs_cave_mouth(1920, 1080),
        "CS2_Zulmat_Eyes": cs_zulmat_eyes(1920, 1080), "CS2_Castle_Silhouette": cs_castle(1920, 1080),
        "CS3_GodsStab": cs_gods_stab(1920, 1080), "CS3_Split": cs_split(1920, 1080),
    }
    out = {k: {"": [v]} for k, v in out.items()}
    out["FlameCore"] = {"": [flame_core(256, 256, i) for i in range(6)]}
    for k in range(1, 7):
        out[f"CS2_Memory_{k:02d}"] = {"": [cs_memory(1920, 1080, k)]}
    for k in range(1, 4):
        out[f"CS3_Merge_{k:02d}"] = {"": [cs_merge(1920, 1080, k)]}
    for k, n in enumerate("ABC"):
        out[f"God_{n}"] = {"": [god(512, 768, k)]}
    return out
