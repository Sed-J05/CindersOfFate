"""HUD / menu sprites (Docs/06 §7) in the same pixel style: drawn at half size, upscaled x2."""
import math, os
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter
from core import Layer, outlined, band, hexc
from bake import lowres_size
from env import glow, flame

OUT = hexc("0E0C0E"); PALE = hexc("EAF6FF"); PALE_S = hexc("A9C6EE"); BLUE = hexc("4FA3FF"); DBLUE = hexc("1B4F9C")
IRON = hexc("2E2B30"); IRON_L = hexc("5A5660"); EMBER = hexc("E2682A"); EMBER_L = hexc("FFB15C"); CRIM = hexc("C1272D")


def heart_pts(cx, cy, r):
    pts = []
    for k in range(40):
        t = k / 40 * math.tau
        x = 16 * math.sin(t) ** 3
        y = 13 * math.cos(t) - 5 * math.cos(2 * t) - 2 * math.cos(3 * t) - math.cos(4 * t)
        pts.append((cx + x * r / 16, cy - y * r / 16))
    return pts


def heart(state, k=0):
    W, H = lowres_size("Heart_Full")
    L = Layer(W, H)
    cx, cy = W / 2, H / 2 + 1
    if state == "empty":
        L.poly(heart_pts(cx, cy, 12), IRON)
        L.poly(heart_pts(cx, cy, 8), (0, 0, 0, 0))
        return outlined(L, OUT, 1)
    if state == "full":
        L.poly(heart_pts(cx, cy, 12), PALE)
        band(L, PALE_S, "left", 2)
        band(L, PALE_S, "bottom", 1)
        img = outlined(L, DBLUE, 1)
        hl = Layer(W, H)
        hl.rect(cx - 7, cy - 6, cx - 5, cy - 4, (255, 255, 255, 255))
        img.alpha_composite(hl.img)
        return img
    # break: halves split apart and fade, with a crack and shards
    t = k / 3
    left, right = Layer(W, H), Layer(W, H)
    pts = heart_pts(cx, cy, 12)
    crack = [(cx, cy - 8), (cx - 2, cy - 3), (cx + 2, cy + 1), (cx - 1, cy + 5), (cx, cy + 14)]
    lp = [p for p in pts if p[0] <= cx] ; rp = [p for p in pts if p[0] >= cx]
    off = t * 5
    left.poly([(x - off, y + off * 0.6) for x, y in lp + crack[::-1]], PALE)
    right.poly([(x + off, y + off * 0.6) for x, y in rp + crack], PALE_S)
    img = Image.new("RGBA", (W, H))
    img.alpha_composite(outlined(left, DBLUE, 1)); img.alpha_composite(outlined(right, DBLUE, 1))
    sh = Layer(W, H)
    for j in range(5):
        a = j * 1.3 + 0.4
        x, y = cx + math.cos(a) * (4 + t * 12), cy + math.sin(a) * (4 + t * 12)
        sh.rect(x, y, x + 1, y + 1, PALE)
    img.alpha_composite(sh.img)
    arr = np.array(img).astype(np.float32); arr[:, :, 3] *= (1 - t * 0.75)
    return Image.fromarray(arr.astype(np.uint8), "RGBA")


def ring(L, cx, cy, r0, r1, col):
    L.ellipse(cx, cy, r1, r1, col)
    L.ellipse(cx, cy, r0, r0, (0, 0, 0, 0))


def soul_frame():
    W, H = lowres_size("Soul_Frame")
    L = Layer(W, H)
    c = W / 2
    ring(L, c, c, 30, 38, IRON)
    for k in range(8):                             # notches + top flame finial
        a = k * math.tau / 8
        x, y = c + math.cos(a) * 38, c + math.sin(a) * 38
        L.poly([(x + math.cos(a + 1.57) * 3, y + math.sin(a + 1.57) * 3), (x + math.cos(a) * 5, y + math.sin(a) * 5),
                (x - math.cos(a + 1.57) * 3, y - math.sin(a + 1.57) * 3)], IRON)
    band(L, IRON_L, "top", 1)
    img = outlined(L, OUT, 1)
    fl = Layer(W, H)
    flame(fl, c, c - 38, 10, 0.2)
    img.alpha_composite(fl.img)
    return img


def soul_fill():
    W, H = lowres_size("Soul_Fill")
    L = Layer(W, H)
    c = W / 2
    L.ellipse(c, c, 30, 30, BLUE)
    L.ellipse(c - 3, c - 3, 23, 23, hexc("8CC8FF"))
    L.ellipse(c - 9, c - 10, 9, 7, PALE)
    return L.img


def soul_glow():
    W, H = 256, 256
    yy, xx = np.mgrid[0:H, 0:W]
    d = np.sqrt((xx - W / 2) ** 2 + (yy - H / 2) ** 2) / (W / 2)
    arr = np.zeros((H, W, 4), np.float32)
    arr[:, :, :3] = (180, 220, 255)
    arr[:, :, 3] = np.clip(1 - d, 0, 1) ** 1.8 * 230
    return Image.fromarray(arr.astype(np.uint8), "RGBA")


def bossbar(fill):
    W, H = lowres_size("BossBar_Frame")
    L = Layer(W, H)
    if fill:
        L.rect(16, 8, W - 16, H - 8, CRIM)
        L.rect(16, 8, W - 16, 10, hexc("FF5A50"))
        L.rect(16, H - 10, W - 16, H - 8, hexc("6E1014"))
        return L.img
    L.rect(10, 5, W - 10, H - 5, IRON)
    L.rect(15, 8, W - 15, H - 8, (0, 0, 0, 0))
    for x, s in ((8, 1), (W - 8, -1)):          # end caps: blades
        L.poly([(x, H / 2), (x + s * 12, 2), (x + s * 18, H / 2), (x + s * 12, H - 2)], IRON)
    band(L, IRON_L, "top", 1)
    return outlined(L, OUT, 1)


def button(hover):
    W, H = lowres_size("Button_Normal")
    L = Layer(W, H)
    rng = np.random.default_rng(4)
    top = [(x, 4 + rng.uniform(-1.5, 1.5)) for x in np.linspace(6, W - 6, 30)]
    bot = [(x, H - 4 + rng.uniform(-1.5, 1.5)) for x in np.linspace(W - 6, 6, 30)]
    L.poly(top + bot, hexc("3A363C") if hover else hexc("26232A"))
    band(L, hexc("1C1A1D"), "bottom", 2)
    img = outlined(L, EMBER if hover else OUT, 1)
    if hover:
        img = glow(img, W / 2, H / 2, W * 0.45, EMBER, 0.18)
    return img


def cursor():
    W, H = lowres_size("Cursor")
    L = Layer(W, H)
    flame(L, W / 2, H - 6, 22, 0.15)
    return outlined(L, OUT, 1)


def icon(kind):
    W, H = lowres_size("Ability_Icon_Dash")
    L = Layer(W, H)
    L.poly([(W / 2, 3), (W - 3, H / 2), (W / 2, H - 3), (3, H / 2)], IRON)
    band(L, IRON_L, "top", 1)
    img = outlined(L, OUT, 1)
    f = Layer(W, H)
    if kind == "dash":
        for j, y in enumerate((18, 24, 30)):
            f.rect(10 + j * 2, y, 26 + j * 2, y + 2, PALE_S)
        f.poly([(26, 16), (40, 24), (26, 34), (30, 24)], PALE)
        img.alpha_composite(outlined(f, DBLUE, 1))
    else:
        f.ellipse(W / 2 + 4, H / 2, 8, 7, BLUE)
        f.poly([(W / 2 + 2, H / 2 - 6), (12, H / 2 - 2), (18, H / 2), (12, H / 2 + 3), (W / 2 + 2, H / 2 + 6)], BLUE)
        f.ellipse(W / 2 + 5, H / 2, 4, 4, PALE)
        img.alpha_composite(outlined(f, DBLUE, 1))
        img = glow(img, W / 2 + 4, H / 2, 20, BLUE, 0.3)
    return img


def _font(size):
    for name in ("georgiab.ttf", "BOOKOSB.TTF", "timesbd.ttf"):
        p = os.path.join(os.environ.get("WINDIR", "C:/Windows"), "Fonts", name)
        if os.path.exists(p):
            return ImageFont.truetype(p, size)
    return ImageFont.load_default()


def title_logo():
    """'CINDERS OF FATE' as chunky cut-paper letters with an ember rim and drifting cinders."""
    W, H = 600, 200
    text = Image.new("L", (W, H), 0)
    d = ImageDraw.Draw(text)
    big, small = _font(78), _font(34)
    for s, y, f in (("CINDERS", 18, big), ("OF FATE", 118, small)):
        bb = d.textbbox((0, 0), s, font=f)
        d.text(((W - (bb[2] - bb[0])) / 2 - bb[0], y), s, font=f, fill=255)
    m = np.array(text) > 110
    out = np.zeros((H, W, 4), np.uint8)
    dil = m.copy()
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1), (2, 0), (-2, 0), (0, 2), (0, -2), (1, 1), (-1, -1), (1, -1), (-1, 1)):
        dil |= np.roll(np.roll(m, dy, 0), dx, 1)
    out[dil] = EMBER
    out[m] = hexc("161214")
    lower = np.roll(m, -2, 0) & ~m              # ember-lit bottom edge inside letters
    out[m & ~np.roll(m, -3, 0)] = hexc("6E2A1E")
    img = Image.fromarray(out, "RGBA")
    line = Layer(W, H)
    line.rect(120, 108, 230, 110, EMBER)
    line.rect(370, 108, 480, 110, EMBER)
    line.poly([(300, 102), (306, 109), (300, 116), (294, 109)], EMBER_L)
    rng = np.random.default_rng(8)
    for _ in range(26):
        x, y = rng.uniform(40, W - 40), rng.uniform(10, H - 10)
        if not dil[int(y), int(x)]:
            line.rect(x, y, x + 1, y + 1, EMBER_L if rng.random() > 0.4 else EMBER)
    img.alpha_composite(line.img)
    g = Image.new("RGBA", (W, H))
    gl = Image.fromarray(np.where(dil[:, :, None], np.array([226, 104, 42, 120], np.uint8), 0).astype(np.uint8), "RGBA").filter(ImageFilter.GaussianBlur(6))
    g.alpha_composite(gl)
    g.alpha_composite(img)
    return g


def build():
    out = {
        "Heart_Full": {"": [heart("full")]}, "Heart_Empty": {"": [heart("empty")]},
        "Heart_Break": {"": [heart("break", k) for k in range(4)]},
        "Soul_Frame": {"": [soul_frame()]}, "Soul_Fill": {"": [soul_fill()]},
        "BossBar_Frame": {"": [bossbar(False)]}, "BossBar_Fill": {"": [bossbar(True)]},
        "Button_Normal": {"": [button(False)]}, "Button_Hover": {"": [button(True)]},
        "Title_Logo": {"": [title_logo()]}, "Cursor": {"": [cursor()]},
        "Ability_Icon_Dash": {"": [icon("dash")]}, "Ability_Icon_Fireball": {"": [icon("fire")]},
    }
    return out, {"Soul_Glow": {"": [soul_glow()]}}       # second dict: already full resolution
