"""Generic humanoid renderer driven by a style spec; each character customises head / extras / weapon."""
import math
from core import Layer, outlined, band, stack, rim, hexc, mix, rot
from rig import Skeleton, vec


class Style:
    def __init__(self, **kw):
        self.__dict__.update(kw)


def limb(layer, p0, p1, r0, r1, col):
    layer.capsule(p0, p1, r0, r1, col)


def render(spec, pose, W, H, t=0.0):
    """spec: Style with dims, colours and part painters. Returns the low-res RGBA image."""
    root = (W / 2 + spec.dims.get("root_dx", 0), H - spec.dims.get("margin", 5))
    sk = Skeleton(spec.dims, pose, root)
    parts = []

    def part(fn, shade_side="left", shade_w=1, outline=None, shade_col=None, z=0):
        L = Layer(W, H)
        fn(L, sk)
        if shade_col is not None and shade_w > 0:
            band(L, shade_col, shade_side, shade_w)
        parts.append((z, outlined(L, outline or spec.outline, 1)))

    d = spec.dims
    # ---- back layers ----
    if getattr(spec, "back", None):
        part(lambda L, s: spec.back(L, s, pose, t), shade_col=None, z=-5)
    # back arm + leg (slightly darker body colour)
    back_col = spec.body_back
    part(lambda L, s: (limb(L, s.sh_b, s.elbow_b, d["arm_r"], d["arm_r"] * 0.9, back_col),
                       limb(L, s.elbow_b, s.hand_b, d["arm_r"] * 0.9, d["arm_r"] * 0.8, back_col),
                       spec.hand(L, s.hand_b, pose, back=True) if getattr(spec, "hand", None) else None), shade_col=None, z=-3)
    part(lambda L, s: (limb(L, s.hip_b, s.knee_b, d["leg_r"], d["leg_r"] * 0.85, back_col),
                       limb(L, s.knee_b, s.foot_b, d["leg_r"] * 0.85, d["leg_r"] * 0.75, back_col),
                       spec.foot(L, s.foot_b, s.knee_b, pose, back=True)), shade_col=None, z=-2)
    if getattr(spec, "weapon_back", False):
        part(lambda L, s: spec.weapon(L, s, pose, t), shade_col=None, z=-1, outline=getattr(spec, "weapon_outline", None))
    # torso
    part(lambda L, s: spec.torso(L, s, pose, t), shade_col=spec.shade, shade_w=d.get("shade_w", 2), z=0)
    # front leg
    part(lambda L, s: (limb(L, s.hip_f, s.knee_f, d["leg_r"], d["leg_r"] * 0.85, spec.body),
                       limb(L, s.knee_f, s.foot_f, d["leg_r"] * 0.85, d["leg_r"] * 0.75, spec.body),
                       spec.foot(L, s.foot_f, s.knee_f, pose, back=False)), shade_col=spec.shade, shade_w=1, z=1)
    # head (+ hair / ears / eyes)
    part(lambda L, s: spec.head(L, s, pose, t), shade_col=spec.shade, shade_w=d.get("head_shade_w", 2), z=2)
    if getattr(spec, "neck_wear", None):
        part(lambda L, s: spec.neck_wear(L, s, pose, t), shade_col=getattr(spec, "accent_dark", None), shade_w=1, z=3)
    # front arm
    part(lambda L, s: (limb(L, s.sh_f, s.elbow_f, d["arm_r"], d["arm_r"] * 0.9, spec.body),
                       limb(L, s.elbow_f, s.hand_f, d["arm_r"] * 0.9, d["arm_r"] * 0.8, spec.body),
                       spec.hand(L, s.hand_f, pose, back=False) if getattr(spec, "hand", None) else None),
         shade_col=spec.shade, shade_w=1, z=5)
    if getattr(spec, "weapon", None) and not getattr(spec, "weapon_back", False):
        part(lambda L, s: spec.weapon(L, s, pose, t), shade_col=None, z=6, outline=getattr(spec, "weapon_outline", None))
    if getattr(spec, "front", None):
        part(lambda L, s: spec.front(L, s, pose, t), shade_col=None, z=7)
    parts.sort(key=lambda p: p[0])
    img = stack(W, H, [p[1] for p in parts])
    if getattr(spec, "rim", None):
        img = rim(img, spec.rim, 1)
    if getattr(spec, "post", None):
        img = spec.post(img, sk, pose, t)
    return img
