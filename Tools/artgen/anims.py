"""Shared humanoid animation library (keyframes) + post effects (death dissolve, glows, speed lines)."""
import math
import numpy as np
from PIL import Image
from rig import P, frames
from core import Layer, hexc

# ---------------------------------------------------------------- keyframes
def idle_keys(base):
    return [dict(base, y=0.0, ua_f=base["ua_f"], scarf=0.0),
            dict(base, y=-0.7, ua_f=base["ua_f"] + 3, ua_b=base["ua_b"] - 2, scarf=1.0),
            dict(base, y=-1.0, ua_f=base["ua_f"] + 4, ua_b=base["ua_b"] - 3, scarf=2.0),
            dict(base, y=-0.3, ua_f=base["ua_f"] + 1, scarf=3.0)]


def run_keys(base, amp=1.0, lean=12.0):
    a = amp
    return [
        dict(base, lean=lean, y=1.0, ul_f=35 * a, ll_f=12 * a, ul_b=-28 * a, ll_b=-60 * a, ua_f=-40 * a, la_f=-10, ua_b=40 * a, la_b=70 * a, cape=0.6, scarf=0),
        dict(base, lean=lean, y=-1.5, ul_f=6 * a, ll_f=-35 * a, ul_b=18 * a, ll_b=6 * a, ua_f=-5, la_f=20, ua_b=10, la_b=30, cape=0.7, scarf=1),
        dict(base, lean=lean, y=1.0, ul_f=-28 * a, ll_f=-60 * a, ul_b=35 * a, ll_b=12 * a, ua_f=40 * a, la_f=70 * a, ua_b=-40 * a, la_b=-10, cape=0.6, scarf=2),
        dict(base, lean=lean, y=-1.5, ul_f=18 * a, ll_f=6 * a, ul_b=6 * a, ll_b=-35 * a, ua_f=10, la_f=30, ua_b=-5, la_b=20, cape=0.7, scarf=3),
    ]


def walk_keys(base, amp=1.0):
    return run_keys(base, amp * 0.55, lean=base.get("lean", 0) + 3)


def hurt_keys(base):
    return [dict(base, lean=-22, head=-15, x=-2, ua_f=60, la_f=110, ua_b=-70, la_b=-120, ul_f=20, ul_b=-15),
            dict(base, lean=-14, head=-8, x=-1, ua_f=30, la_f=60, ua_b=-40, la_b=-60, ul_f=12, ul_b=-10)]


def crouch(base, depth=1.0):
    d = depth
    return dict(base, ul_f=55 * d, ll_f=-25 * d, ul_b=40 * d, ll_b=-40 * d, lean=base.get("lean", 0) + 10 * d)


def death_keys(base):
    return [dict(base, lean=-18, head=-10, ua_f=40, la_f=80, ua_b=-50, la_b=-80),
            crouch(dict(base, lean=-5, head=10), 0.8),
            crouch(dict(base, lean=25, head=25, ua_f=10, la_f=0, ua_b=-10), 1.2),
            crouch(dict(base, lean=40, head=30, ua_f=5, la_f=-5, ua_b=-5), 1.3)]


# ---------------------------------------------------------------- post effects
def dissolve(img, amount, ember, seed=7):
    """Death: the silhouette breaks into paper scraps / embers drifting up. amount 0..1."""
    if amount <= 0:
        return img
    arr = np.array(img)
    a = arr[:, :, 3] > 0
    rng = np.random.default_rng(seed)
    h, w = a.shape
    # block-wise dropout so it reads as pixel scraps
    blocks = rng.random(((h + 1) // 2, (w + 1) // 2))
    bmask = np.kron(blocks, np.ones((2, 2)))[:h, :w]
    # dissolve from the top-left toward bottom so it reads as crumbling
    yy = np.linspace(0, 1, h)[:, None]
    thresh = amount * 1.25 - (1 - yy) * 0.25
    kill = a & (bmask < thresh)
    out = arr.copy()
    out[kill] = 0
    # ember specks drifting up from killed pixels
    ys, xs = np.nonzero(kill)
    if len(ys):
        pick = rng.choice(len(ys), size=min(len(ys), 18), replace=False)
        for k in pick:
            ny = int(ys[k] - amount * 14 - rng.random() * 6)
            nx = int(xs[k] + rng.integers(-2, 3))
            if 0 <= ny < h and 0 <= nx < w:
                out[ny, nx] = ember
    return Image.fromarray(out, "RGBA")


def glow_orb(img, cx, cy, r, core, edge):
    L = Layer(img.width, img.height)
    L.ellipse(cx, cy, r + 1.5, r + 1.5, edge)
    L.ellipse(cx, cy, r, r, core)
    img.alpha_composite(L.img)
    return img


def aura(img, cx, cy, rx, ry, col, phase):
    L = Layer(img.width, img.height)
    for k in range(10):
        a = phase * math.tau + k * math.tau / 10
        x = cx + math.cos(a) * rx
        y = cy + math.sin(a) * ry - (phase * 6 + k) % 8
        L.rect(x, y, x + 1, y + 1, col)
    img.alpha_composite(L.img)
    return img


def speed_lines(img, col, y0, y1, x_end, n=5, seed=3):
    rng = np.random.default_rng(seed)
    L = Layer(img.width, img.height)
    for _ in range(n):
        y = rng.integers(int(y0), int(y1))
        ln = rng.integers(8, 20)
        L.line((x_end - ln, y), (x_end, y), col, 1)
    out = Image.new("RGBA", img.size, (0, 0, 0, 0))
    out.alpha_composite(L.img)
    out.alpha_composite(img)
    return out


def ghost(img, alpha, dx):
    """Afterimage copy behind (dash)."""
    arr = np.array(img).astype(np.float32)
    arr[:, :, 3] *= alpha
    g = Image.fromarray(arr.astype(np.uint8), "RGBA")
    out = Image.new("RGBA", img.size, (0, 0, 0, 0))
    out.alpha_composite(g, (int(dx), 0)) if dx >= 0 else out.alpha_composite(g.crop((-int(dx), 0, img.width, img.height)), (0, 0))
    out.alpha_composite(img)
    return out


def flash(img, col, amount):
    if amount <= 0:
        return img
    arr = np.array(img).astype(np.float32)
    m = arr[:, :, 3] > 0
    for c in range(3):
        arr[:, :, c][m] = arr[:, :, c][m] * (1 - amount) + col[c] * amount
    return Image.fromarray(arr.astype(np.uint8), "RGBA")
