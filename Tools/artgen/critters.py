"""Non-humanoid enemies: Shade Crawler, Shadow Kite (dark style) and Bat (light style)."""
import math
from core import Layer, outlined, band, stack, rim, hexc, rot
from enemies import DK, LT
from anims import dissolve, flash
from bake import lowres_size


def _finish(parts, W, H, pal):
    img = stack(W, H, parts)
    if pal["rim"]:
        img = rim(img, pal["rim"], 1)
    return img


def _part(W, H, fn, pal, shade=True):
    L = Layer(W, H)
    fn(L)
    if shade:
        band(L, pal["shade"], "left", 1)
        band(L, pal["shade"], "bottom", 1)
    return outlined(L, pal["outline"], 1)


# ------------------------------------------------------------------ Shade Crawler
def crawler_frame(ph=0.0, squash=0.0, lean=0.0, hurt=False, W=64, H=64):
    pal = DK
    gx, gy = W / 2, H - 5
    bw, bh = 20 * (1 + squash * 0.15), 16 * (1 - squash * 0.15)
    by = gy - 5 - bh / 2 + abs(math.sin(ph * math.tau)) * -1.2
    bx = gx + lean
    parts = []
    # scarf tail
    def scarf(L):
        pts_t, pts_b = [], []
        for i in range(6):
            u = i / 5
            x = bx - bw / 2 - u * 12
            y = by - 3 + u * 2 + math.sin(ph * math.tau * 2 + u * 3) * (1 + 2 * u)
            w = 3.5 * (1 - 0.6 * u)
            pts_t.append((x, y - w / 2)); pts_b.append((x, y + w / 2))
        L.poly(pts_t + pts_b[::-1], pal["accent"])
    parts.append(_part(W, H, scarf, pal, shade=False))
    # back legs
    def legs(L, front):
        for k in range(2):
            off = (k * 0.5 + (0.25 if front else 0)) * math.tau
            sx = bx - bw * 0.3 + k * bw * 0.6 + (2 if front else -2)
            step = math.sin(ph * math.tau * 2 + off) * 3
            L.capsule((sx, by + bh / 2 - 2), (sx + step, gy), 1.8, 1.6, pal["body"] if front else pal["back"])
    parts.append(_part(W, H, lambda L: legs(L, False), pal, shade=False))
    # body: rounded box
    def body(L):
        L.rect(bx - bw / 2, by - bh / 2 + 3, bx + bw / 2, by + bh / 2, pal["body"])
        L.ellipse(bx, by - bh / 2 + 4, bw / 2, 5, pal["body"])
        L.rect(bx - bw / 2, by + 1, bx + bw / 2, by + 3, pal["accent"])     # red band
    parts.append(_part(W, H, body, pal))
    parts.append(_part(W, H, lambda L: legs(L, True), pal, shade=False))
    img = _finish(parts, W, H, pal)
    eyes = Layer(W, H)
    for ex in (bx + bw * 0.12, bx + bw * 0.34):
        if hurt:
            eyes.line((ex - 1.5, by - 3), (ex + 1.5, by - 1), pal["eye"], 1)
        else:
            eyes.poly([(ex - 2, by - 3.5), (ex + 2, by - 2.5), (ex - 0.5, by - 1)], pal["eye"])
    img.alpha_composite(eyes.img)
    return img


def crawler():
    W, H = lowres_size("Crawler")
    A = {"Idle": [crawler_frame(i / 4 * 0.3, squash=0.25 * math.sin(i / 4 * math.tau), W=W, H=H) for i in range(4)],
         "Walk": [crawler_frame(i / 6, W=W, H=H) for i in range(6)],
         "Hurt": [crawler_frame(0, squash=0.4, lean=-3, hurt=True, W=W, H=H), crawler_frame(0, squash=0.2, lean=-2, hurt=True, W=W, H=H)]}
    A["Death"] = [dissolve(crawler_frame(0, squash=0.3 + 0.1 * i, lean=-2, hurt=True, W=W, H=H), max(0, (i - 1) / 5), DK["rim"], seed=70 + i) for i in range(6)]
    return A


# ------------------------------------------------------------------ Shadow Kite
def kite_frame(flap=0.0, tilt=0.0, tail_ph=0.0, flare=0.0, W=64, H=64, hurt=False):
    pal = DK
    cx, cy = W / 2, H / 2
    a = math.radians(tilt)
    R = lambda x, y: rot(cx + x, cy + y, cx, cy, a)
    parts = []
    def tail(L):
        pts = []
        for i in range(9):
            u = i / 8
            x = -8 - u * 22
            y = 3 + math.sin(tail_ph * math.tau + u * 5) * (2 + 3 * u) - flare * u * 8
            pts.append(R(x, y))
        for p0, p1 in zip(pts, pts[1:]):
            L.capsule(p0, p1, 1.4, 1.2, pal["accent"])
        for k in (3, 6):
            px, py = pts[k]
            L.poly([(px - 2, py - 2), (px + 2, py), (px - 2, py + 2)], pal["accent"])
    parts.append(_part(W, H, tail, pal, shade=False))
    wing_y = -9 * math.cos(flap * math.tau)
    def wings(L, back):
        s = -1 if back else 1
        L.poly([R(-4, -1), R(-8 + 2 * s, wing_y - 4 + s), R(6, wing_y - 2), R(4, 0)], pal["back"] if back else pal["body"])
    parts.append(_part(W, H, lambda L: wings(L, True), pal, shade=False))
    def body(L):
        L.poly([R(13, 0), R(0, -7), R(-11, 0), R(0, 7)], pal["body"])
        L.line(R(-11, 0), R(13, 0), pal["accent_d"], 1)
        L.line(R(0, -7), R(0, 7), pal["accent_d"], 1)
    parts.append(_part(W, H, body, pal))
    parts.append(_part(W, H, lambda L: wings(L, False), pal))
    img = _finish(parts, W, H, pal)
    eye = Layer(W, H)
    ex, ey = R(6, -1)
    eye.ellipse(ex, ey, 1.6 + flare, 1.6 + flare, pal["eye"])
    img.alpha_composite(eye.img)
    return img


def kite():
    W, H = lowres_size("ShadowKite")
    A = {"Fly": [kite_frame(i / 6, tilt=math.sin(i / 6 * math.tau) * 4, tail_ph=i / 6, W=W, H=H) for i in range(6)]}
    A["Telegraph"] = [flash(kite_frame(0.25, tilt=-8 - 6 * i, tail_ph=i / 3, flare=0.5 * i, W=W, H=H), DK["accent"], 0.15 + 0.2 * i) for i in range(3)]
    A["Dive"] = [kite_frame(0.5, tilt=35, tail_ph=i / 2, W=W, H=H) for i in range(2)]
    A["Hurt"] = [kite_frame(0.1, tilt=-20, tail_ph=0.3, W=W, H=H), kite_frame(0.3, tilt=-10, tail_ph=0.6, W=W, H=H)]
    A["Death"] = [dissolve(kite_frame(0.4, tilt=-20 - 15 * i, tail_ph=i / 6, W=W, H=H), max(0, (i - 1) / 5), DK["rim"], seed=80 + i) for i in range(6)]
    return A


# ------------------------------------------------------------------ Bat
def bat_frame(flap=0.0, tilt=0.0, hang=False, fold=0.0, fangs=False, W=64, H=64, eyes_open=True):
    pal = LT
    cx, cy = W / 2, H / 2
    a = math.radians(tilt + (180 if hang else 0))
    R = lambda x, y: rot(cx + x, cy + y, cx, cy, a)
    parts = []
    span = 22 * (1 - fold * 0.75)
    up = -8 * math.cos(flap * math.tau) * (1 - fold)

    def wing(L, side, back):
        s = side
        tip = (s * span, up - 3)
        pts = [R(s * 3, -2), R(s * span * 0.55, up - 6), R(*tip), R(s * span * 0.8, up + 4), R(s * span * 0.55, up + 1),
               R(s * span * 0.35, up + 5), R(s * span * 0.2, 2), R(s * 3, 3)]
        L.poly(pts, pal["back"] if back else pal["body"])
    parts.append(_part(W, H, lambda L: wing(L, -1, True), pal, shade=False))
    def body(L):
        L.ellipse(*R(0, 1), 6, 7, pal["body"])
        L.ellipse(*R(1, -6), 5, 4.5, pal["body"])
        L.poly([R(-3, -8), R(-4, -15), R(0, -9)], pal["body"])   # ears
        L.poly([R(2, -9), R(5, -15), R(5, -7)], pal["body"])
        L.poly([R(-2, 7), R(0, 10), R(2, 7)], pal["body"])        # feet
    parts.append(_part(W, H, body, pal))
    parts.append(_part(W, H, lambda L: wing(L, 1, False), pal))
    img = _finish(parts, W, H, pal)
    f = Layer(W, H)
    for ex in (0.0, 3.5):
        x, y = R(ex, -6.5)
        if eyes_open:
            f.ellipse(x, y, 1.1, 1.3, pal["eye"])
        else:
            f.line((x - 1, y), (x + 1, y), pal["eye"], 1)
    if fangs:
        x, y = R(2, -3)
        f.poly([(x - 1, y), (x, y + 2), (x + 1, y)], hexc("FFFFFF"))
    img.alpha_composite(f.img)
    return img


def bat():
    W, H = lowres_size("Bat")
    A = {"Sleep": [bat_frame(hang=True, fold=1.0, eyes_open=False, W=W, H=H), bat_frame(hang=True, fold=0.95, eyes_open=False, tilt=2, W=W, H=H)],
         "Wake": [bat_frame(hang=True, fold=1.0, W=W, H=H), bat_frame(hang=True, fold=0.5, W=W, H=H), bat_frame(flap=0.3, fold=0.1, tilt=20, W=W, H=H)],
         "Fly": [bat_frame(i / 6, tilt=math.sin(i / 6 * math.tau) * 5, W=W, H=H) for i in range(6)],
         "Telegraph": [flash(bat_frame(0.0 + 0.1 * i, tilt=-10 - 5 * i, fangs=True, W=W, H=H), LT["accent"], 0.15 + 0.2 * i) for i in range(3)],
         "Dive": [bat_frame(0.5, tilt=40, fold=0.7, fangs=True, W=W, H=H), bat_frame(0.6, tilt=42, fold=0.6, fangs=True, W=W, H=H)],
         "Hurt": [bat_frame(0.2, tilt=-25, W=W, H=H, eyes_open=False), bat_frame(0.4, tilt=-12, W=W, H=H)]}
    A["Death"] = [dissolve(bat_frame(0.3, tilt=-20 - 20 * i, fold=0.2 * i, eyes_open=False, W=W, H=H), max(0, (i - 1) / 5), LT["accent"], seed=90 + i) for i in range(6)]
    return A


ALL = {"Crawler": crawler, "ShadowKite": kite, "Bat": bat}


def build(name):
    return ALL[name]()
