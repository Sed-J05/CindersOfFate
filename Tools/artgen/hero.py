"""The protagonist — Phase 1 white-blue (char2 reference) and Phase 2 black-red (char1 reference)."""
import math
from core import hexc, mix, rot
from rig import vec
from humanoid import Style

# ---------------- Phase 1: white / blue ----------------
W1 = hexc("EAF6FF"); S1 = hexc("A9C6EE"); B1 = hexc("4FA3FF"); D1 = hexc("1B4F9C"); E1 = hexc("10224F")
B1d = hexc("2E6FD6")

HERO_DIMS = dict(thigh=14, shin=13, torso=19, uarm=11, larm=11, arm_r=2.1, leg_r=2.6, hip_w=3, neck=2,
                 shoulder_drop=2, margin=5, shade_w=2, head_shade_w=2)


def _scarf_tail(L, s, pose, t, col, dark, length=16, width=4, ragged=False):
    nx, ny = s.neck
    base = (nx - 2, ny + 2)
    pts_top, pts_bot = [], []
    ph = pose.get("scarf", 0.0) + t * math.tau
    lift = pose.get("cape", 0.0)      # >0 = tail streams more horizontally (running, falling)
    for i in range(7):
        u = i / 6
        x = base[0] - u * length
        y = base[1] + u * (length * (0.30 - 0.32 * lift)) + math.sin(ph + u * 3.2) * (1.2 + 2.6 * u)
        w = width * (1 - 0.55 * u)
        pts_top.append((x, y - w * 0.5))
        pts_bot.append((x, y + w * 0.5))
    pts = pts_top + pts_bot[::-1]
    L.poly(pts, col)
    if ragged:  # torn tips (dark version)
        tip = pts_top[-1]
        L.poly([tip, (tip[0] - 4, tip[1] - 2), (tip[0] - 1, tip[1] + 1)], col)
        L.poly([pts_bot[-1], (pts_bot[-1][0] - 5, pts_bot[-1][1] + 2), (pts_bot[-1][0] - 1, pts_bot[-1][1] - 1)], col)
        mid = pts_bot[3]
        L.poly([mid, (mid[0] - 3, mid[1] + 3), (mid[0] + 1, mid[1])], col)


def hero1_back(L, s, pose, t):
    _scarf_tail(L, s, pose, t, B1, B1d, length=24, width=7)


def hero1_torso(L, s, pose, t):
    (hx, hy), (nx, ny) = s.hip, s.neck
    lean = math.radians(pose["lean"])
    # slim torso: narrower at the hips
    ox, oy = math.cos(lean), math.sin(lean)
    L.poly([(nx - 4 * ox, ny - 4 * oy), (nx + 4 * ox, ny + 4 * oy), (hx + 3.5 * ox, hy + 3.5 * oy), (hx - 3.5 * ox, hy - 3.5 * oy)], W1)
    L.ellipse(hx, hy, 3.5, 2.5, W1)


def hero1_head(L, s, pose, t):
    cx, cy = s.head
    cy -= 9
    tilt = math.radians(pose["head"] + pose["lean"] * 0.4)
    def R(x, y):
        return rot(cx + x, cy + y, cx, cy, tilt)
    # skull: wide rounded, slight snout forward
    L.ellipse(cx, cy, 9, 8.5, W1)
    L.ellipse(*R(3, 2), 7, 6.5, W1)
    # tall back ear + front ear
    L.poly([R(-6, -5), R(-4, -21), R(1, -6)], W1)
    L.poly([R(2, -6), R(6, -18), R(8, -3)], W1)
    # inner ear shading
    L.poly([R(-4, -8), R(-3.5, -17), R(-1, -8)], S1)


def hero1_eye(img_layer_draw):
    pass


def hero1_face(L, s, pose, t):
    """Eye drawn after outlining the head (as part of the scarf/neck layer pass)."""
    cx, cy = s.head
    cy -= 9
    tilt = math.radians(pose["head"] + pose["lean"] * 0.4)
    ex, ey = rot(cx + 4, cy - 1, cx, cy, tilt)
    blink = pose.get("open", 0.0)  # >0.5 closed
    if blink > 0.5:
        L.line((ex - 2, ey), (ex + 2, ey), E1, 1)
    else:
        L.ellipse(ex, ey, 2.4, 2.9, E1)
        L.rect(ex + 0.5, ey - 2, ex + 1, ey - 1.5, W1)


def hero1_neck(L, s, pose, t):
    nx, ny = s.neck
    # scarf wrap around the neck
    L.ellipse(nx, ny + 1, 5.5, 2.6, B1)
    L.rect(nx - 4, ny + 1, nx + 4, ny + 2.5, B1)


def hero1_foot(L, foot, knee, pose, back=False):
    fx, fy = foot
    col = B1d if back else B1
    L.rect(fx - 2.5, fy - 4, fx + 2.5, fy, col)
    L.rect(fx - 2.5, fy - 1, fx + 4, fy, col)
    L.rect(fx - 2.8, fy - 5, fx + 2.8, fy - 4, B1d)


def hero1_hand(L, hand, pose, back=False):
    L.ellipse(hand[0], hand[1], 1.8, 1.8, S1 if back else W1)


def hero1_weapon(L, s, pose, t):
    hx, hy = s.hand_f
    a = pose["wpn"]
    tip = s.along((hx, hy), a, 13)
    butt = s.along((hx, hy), a + 180, 2.5)
    # blade (white with a blue edge) + blue wrapped grip
    L.capsule((hx, hy), tip, 1.6, 0.4, W1)
    L.capsule(s.along((hx, hy), a, 2), s.along((hx, hy), a, 11), 0.5, 0.2, S1)
    L.capsule(butt, s.along((hx, hy), a, 1.5), 1.4, 1.4, B1)
    gx, gy = s.along((hx, hy), a, 2)
    L.capsule(s.along((gx, gy), a + 90, 2.4), s.along((gx, gy), a - 90, 2.4), 0.8, 0.8, B1)


def _post_face(face_fn):
    def post(img, sk, pose, t):
        from core import Layer
        L = Layer(img.width, img.height)
        face_fn(L, sk, pose, t)
        img.alpha_composite(L.img)
        return img
    return post


HERO1 = Style(dims=HERO_DIMS, body=W1, body_back=S1, shade=S1, outline=D1, accent_dark=B1d,
              back=hero1_back, torso=hero1_torso, head=hero1_head, neck_wear=hero1_neck,
              foot=hero1_foot, hand=hero1_hand, weapon=hero1_weapon, weapon_outline=D1,
              post=_post_face(hero1_face))

# ---------------- Phase 2: black / red (char1 reference) ----------------
K2 = hexc("120A0C"); K2s = hexc("2B1D20"); R2 = hexc("9E1717"); R2d = hexc("5E0C0E"); EYE2 = hexc("FF3B30")
EMBER = hexc("E2682A"); STEEL = hexc("3A3236"); STEELd = hexc("1C1517")


def hero2_back(L, s, pose, t):
    _scarf_tail(L, s, pose, t, R2, R2d, length=26, width=9, ragged=True)
    # coat tails behind the legs
    hx, hy = s.hip
    sw = math.sin(t * math.tau + pose.get("cape", 0) * 2) * 1.5
    L.poly([(hx - 4, hy - 2), (hx + 3, hy - 2), (hx + 2 + sw, hy + 13), (hx - 2 + sw, hy + 15), (hx - 7 + sw, hy + 12)], K2)


def hero2_torso(L, s, pose, t):
    (hx, hy), (nx, ny) = s.hip, s.neck
    lean = math.radians(pose["lean"])
    ox, oy = math.cos(lean), math.sin(lean)
    L.poly([(nx - 5 * ox, ny - 5 * oy), (nx + 5 * ox, ny + 5 * oy), (hx + 4.5 * ox, hy + 4.5 * oy), (hx - 4.5 * ox, hy - 4.5 * oy)], K2)
    # red sash at the waist
    L.rect(hx - 4.5, hy - 2, hx + 4.5, hy, R2)


def hero2_head(L, s, pose, t):
    cx, cy = s.head
    cy -= 8
    tilt = math.radians(pose["head"] + pose["lean"] * 0.4)
    def R(x, y):
        return rot(cx + x, cy + y, cx, cy, tilt)
    L.ellipse(cx, cy, 8, 8, K2)
    # spiky swept-back hair
    spikes = [(-8, -1, -17, -4), (-7, -6, -16, -12), (-4, -8, -10, -18), (0, -9, -2, -19), (4, -8, 6, -16), (-8, 3, -15, 6), (6, -5, 11, -10)]
    for x0, y0, x1, y1 in spikes:
        L.poly([R(x0 - 2, y0 + 2), R(x1, y1), R(x0 + 3, y0 + 1)], K2)


def hero2_face(L, s, pose, t):
    cx, cy = s.head
    cy -= 8
    tilt = math.radians(pose["head"] + pose["lean"] * 0.4)
    for ox in (1.5, 5.5):
        ex, ey = rot(cx + ox, cy + 1, cx, cy, tilt)
        L.poly([(ex - 1.6, ey - 0.8), (ex + 1.6, ey - 0.2), (ex + 0.4, ey + 1.0)], EYE2)


def hero2_neck(L, s, pose, t):
    nx, ny = s.neck
    L.ellipse(nx, ny + 1, 6.5, 3.2, R2)


def hero2_foot(L, foot, knee, pose, back=False):
    fx, fy = foot
    col = K2s if back else K2
    L.rect(fx - 3, fy - 5, fx + 3, fy, col)
    L.rect(fx - 3, fy - 1.5, fx + 4.5, fy, col)


def hero2_hand(L, hand, pose, back=False):
    L.ellipse(hand[0], hand[1], 2.2, 2.2, K2s if back else K2)


def hero2_weapon(L, s, pose, t):
    """Big jagged dark sword with a red core (char1)."""
    hx, hy = s.hand_f
    a = pose["wpn"]
    tip = s.along((hx, hy), a, 28)
    L.capsule(s.along((hx, hy), a + 180, 4), s.along((hx, hy), a, 2), 1.4, 1.4, STEELd)   # grip
    base = s.along((hx, hy), a, 3)
    L.capsule(base, tip, 3.4, 0.6, STEEL)
    L.capsule(s.along((hx, hy), a, 5), s.along((hx, hy), a, 24), 1.2, 0.3, R2)        # red core
    # jagged guard + barbs
    for k, ln in ((3, 6), (8, 4), (13, 3)):
        p = s.along((hx, hy), a, k)
        for side in (90, -90):
            q = s.along(p, a + side * 0.8, ln)
            L.poly([s.along(p, a, -1), q, s.along(p, a, 2)], STEEL)


HERO2 = Style(dims=dict(HERO_DIMS, torso=18, arm_r=2.4, leg_r=3.0), body=K2, body_back=K2s, shade=K2s, outline=hexc("2A0A08"),
              accent_dark=R2d, back=hero2_back, torso=hero2_torso, head=hero2_head, neck_wear=hero2_neck,
              foot=hero2_foot, hand=hero2_hand, weapon=hero2_weapon, weapon_outline=hexc("2A0A08"),
              rim=EMBER, post=_post_face(hero2_face))
