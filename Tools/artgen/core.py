"""Pixel-art primitives for the Cinders of Fate sprite generator.

Everything is drawn on a LOW-RES canvas (the final canvas / SCALE) with hard-edged shapes, then upscaled with
nearest-neighbour so the result reads as chunky pixel art (the reference art uses ~2 screen px per art pixel).
Parts are drawn on their own layer, outlined, shaded and then stacked, which gives the inner outlines between
limbs that the references have.
"""
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

SCALE = 2


def hexc(h, a=255):
    h = h.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


def mix(c1, c2, t):
    return tuple(int(round(c1[i] + (c2[i] - c1[i]) * t)) for i in range(4))


class Layer:
    """One part: a low-res RGBA image plus a draw handle."""

    def __init__(self, w, h):
        self.img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.img)

    # ---- shapes (all aliased, hard edges) ----
    def poly(self, pts, fill):
        if len(pts) >= 3:
            self.d.polygon([(round(x), round(y)) for x, y in pts], fill=fill)

    def ellipse(self, cx, cy, rx, ry, fill):
        self.d.ellipse([round(cx - rx), round(cy - ry), round(cx + rx), round(cy + ry)], fill=fill)

    def rect(self, x0, y0, x1, y1, fill):
        self.d.rectangle([round(min(x0, x1)), round(min(y0, y1)), round(max(x0, x1)), round(max(y0, y1))], fill=fill)

    def capsule(self, p0, p1, r0, r1=None, fill=(255, 255, 255, 255)):
        """Tapered limb from p0 (radius r0) to p1 (radius r1)."""
        r1 = r0 if r1 is None else r1
        (x0, y0), (x1, y1) = p0, p1
        dx, dy = x1 - x0, y1 - y0
        ln = math.hypot(dx, dy) or 1e-6
        nx, ny = -dy / ln, dx / ln
        self.poly([(x0 + nx * r0, y0 + ny * r0), (x1 + nx * r1, y1 + ny * r1),
                   (x1 - nx * r1, y1 - ny * r1), (x0 - nx * r0, y0 - ny * r0)], fill)
        self.ellipse(x0, y0, r0, r0, fill)
        self.ellipse(x1, y1, r1, r1, fill)

    def line(self, p0, p1, fill, width=1):
        self.d.line([(round(p0[0]), round(p0[1])), (round(p1[0]), round(p1[1]))], fill=fill, width=width)

    def alpha(self):
        return np.array(self.img)[:, :, 3] > 0


def _shift(mask, dx, dy):
    out = np.zeros_like(mask)
    h, w = mask.shape
    xs0, xs1 = max(0, -dx), min(w, w - dx)
    ys0, ys1 = max(0, -dy), min(h, h - dy)
    out[ys0 + dy:ys1 + dy, xs0 + dx:xs1 + dx] = mask[ys0:ys1, xs0:xs1]
    return out


def band(layer, color, side="left", width=2):
    """Shading band along one side of the part (left = behind for a right-facing character)."""
    a = layer.alpha()
    off = {"left": (1, 0), "right": (-1, 0), "bottom": (0, -1), "top": (0, 1)}[side]
    edge = np.zeros_like(a)
    for k in range(1, width + 1):
        edge |= a & ~_shift(a, off[0] * k, off[1] * k)
    arr = np.array(layer.img)
    arr[edge] = color
    layer.img = Image.fromarray(arr, "RGBA")
    layer.d = ImageDraw.Draw(layer.img)
    return layer


def outlined(layer, color, thickness=1):
    """Returns a new image: the part plus a hard outline around it."""
    a = Image.fromarray((layer.alpha() * 255).astype(np.uint8), "L")
    grown = a
    for _ in range(thickness):
        grown = grown.filter(ImageFilter.MaxFilter(3))
    g = np.array(grown) > 0
    base = np.zeros((a.size[1], a.size[0], 4), np.uint8)
    base[g] = color
    out = Image.fromarray(base, "RGBA")
    out.alpha_composite(layer.img)
    return out


def stack(w, h, images):
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    for im in images:
        if im is not None:
            out.alpha_composite(im)
    return out


def rim(img, color, thickness=1):
    """Outer glow-ish rim (ember edge of the dark style): colour the outermost ring of the silhouette."""
    arr = np.array(img)
    a = arr[:, :, 3] > 0
    inner = a.copy()
    for _ in range(thickness):
        inner = inner & _shift(inner, 1, 0) & _shift(inner, -1, 0) & _shift(inner, 0, 1) & _shift(inner, 0, -1)
    edge = a & ~inner
    arr[edge] = color
    return Image.fromarray(arr, "RGBA")


def upscale(img, s=SCALE):
    return img.resize((img.width * s, img.height * s), Image.NEAREST)


def rot(px, py, cx, cy, ang):
    c, s = math.cos(ang), math.sin(ang)
    x, y = px - cx, py - cy
    return cx + x * c - y * s, cy + x * s + y * c


def lerp(a, b, t):
    return a + (b - a) * t


def noise2(w, h, scale, seed, octaves=3):
    """Cheap value noise in [0,1] (numpy), for rock / paper textures."""
    rng = np.random.default_rng(seed)
    out = np.zeros((h, w))
    amp, tot = 1.0, 0.0
    s = scale
    for _ in range(octaves):
        gw, gh = max(2, int(w / s) + 2), max(2, int(h / s) + 2)
        grid = rng.random((gh, gw))
        ys = np.linspace(0, gh - 1.001, h)
        xs = np.linspace(0, gw - 1.001, w)
        y0 = ys.astype(int); x0 = xs.astype(int)
        fy = (ys - y0)[:, None]; fx = (xs - x0)[None, :]
        fy = fy * fy * (3 - 2 * fy); fx = fx * fx * (3 - 2 * fx)
        g00 = grid[y0][:, x0]; g10 = grid[y0][:, x0 + 1]; g01 = grid[y0 + 1][:, x0]; g11 = grid[y0 + 1][:, x0 + 1]
        out += amp * ((g00 * (1 - fx) + g10 * fx) * (1 - fy) + (g01 * (1 - fx) + g11 * fx) * fy)
        tot += amp
        amp *= 0.5
        s /= 2
    return out / tot


def tileable_noise(w, h, scale, seed, octaves=3):
    """Noise that wraps on both axes (blend of 4 offset copies)."""
    n = noise2(w * 2, h * 2, scale, seed, octaves)
    a = n[:h, :w]; b = n[:h, w:]; c = n[h:, :w]; d = n[h:, w:]
    yy = np.linspace(0, 1, h)[:, None]; xx = np.linspace(0, 1, w)[None, :]
    top = a * (1 - xx) + b * xx
    bot = c * (1 - xx) + d * xx
    return top * (1 - yy) + bot * yy
