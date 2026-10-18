"""Enemy + boss styles. Two families: DARK (Phase 1 / Zulmat: black, red, ember rim) and LIGHT (Phase 2: white, blue, dark-blue outline)."""
import math
from core import hexc, rot, Layer
from humanoid import Style
from hero import _scarf_tail

# palettes
DK = dict(body=hexc("140C0E"), back=hexc("2A1C1F"), shade=hexc("2A1C1F"), accent=hexc("A8191B"), accent_d=hexc("5E0C0E"),
          eye=hexc("FF3B30"), outline=hexc("2A0A08"), rim=hexc("D85A24"), metal=hexc("3A3236"), metal_d=hexc("1C1517"))
LT = dict(body=hexc("F2F8FF"), back=hexc("BFD3EE"), shade=hexc("BFD3EE"), accent=hexc("6EC1FF"), accent_d=hexc("2C6FD1"),
          eye=hexc("1B4F9C"), outline=hexc("1E4A92"), rim=None, metal=hexc("DCE8F7"), metal_d=hexc("9DB4D6"))
ZU = dict(DK, body=hexc("0B0406"), back=hexc("1E0D10"), shade=hexc("1E0D10"), accent=hexc("C2182A"), eye=hexc("FF2A3A"), rim=hexc("FF2A3A"))


def dims_for(height, bulk=1.0, head=0.22, legs=0.36):
    h = height
    return dict(thigh=h * legs * 0.52, shin=h * legs * 0.48, torso=h * 0.30, uarm=h * 0.17, larm=h * 0.16,
                arm_r=max(1.8, h * 0.032 * bulk), leg_r=max(2.2, h * 0.04 * bulk), hip_w=max(2, h * 0.05 * bulk),
                neck=max(1.5, h * 0.02), shoulder_drop=max(1.5, h * 0.025), margin=5, shade_w=2, head_shade_w=2,
                head_r=h * head * 0.5, tw=h * 0.10 * bulk)


def make(pal, height, head="hood", torso="slim", weapon="katana", bulk=1.0, scarf=None, shield=None, extra=None, wlen=None):
    d = dims_for(height, bulk)
    hr = d["head_r"]
    tw = d["tw"]
    P_ = pal

    def T(L, s, pose, t):
        (hx, hy), (nx, ny) = s.hip, s.neck
        lean = math.radians(pose["lean"])
        ox, oy = math.cos(lean), math.sin(lean)
        top = tw * (1.25 if torso in ("armor", "bulky") else 1.0)
        bot = tw * (0.85 if torso == "slim" else 1.0)
        L.poly([(nx - top * ox, ny - top * oy), (nx + top * ox, ny + top * oy), (hx + bot * ox, hy + bot * oy), (hx - bot * ox, hy - bot * oy)], P_["body"])
        L.ellipse(hx, hy, bot, bot * 0.7, P_["body"])
        if torso == "robe":   # long robe to the ankles
            sw = math.sin(t * math.tau) * 1.5
            L.poly([(hx - bot * 1.1, hy - 2), (hx + bot * 1.1, hy - 2), (hx + bot * 1.9 + sw, hy + d["thigh"] + d["shin"] - 2),
                    (hx - bot * 2.2 + sw, hy + d["thigh"] + d["shin"] - 1)], P_["body"])
        if torso in ("armor", "bulky"):
            L.ellipse(nx + 1, ny + 2, top * 1.15, top * 0.75, P_["metal"] if pal is LT else P_["body"])   # pauldrons
            L.rect(hx - bot, hy - 2, hx + bot, hy, P_["accent_d"])                                   # belt
        if torso == "slim" and pal is not LT:
            L.rect(hx - bot, hy - 2, hx + bot, hy - 0.5, P_["accent"])                                  # red sash

    def Hd(L, s, pose, t):
        cx, cy = s.head
        cy -= hr * 0.9
        tilt = math.radians(pose["head"] + pose["lean"] * 0.4)
        R = lambda x, y: rot(cx + x, cy + y, cx, cy, tilt)
        if head in ("hood", "ninja"):
            L.ellipse(cx, cy, hr, hr, P_["body"])
            L.poly([R(-hr * 0.9, -hr * 0.2), R(-hr * 1.8, hr * 0.6), R(-hr * 0.6, hr * 0.9)], P_["body"])   # hood tail
        elif head == "deep_hood":
            L.ellipse(cx, cy, hr * 1.05, hr * 1.1, P_["body"])
            L.poly([R(-hr, -hr * 0.4), R(-hr * 0.2, -hr * 1.7), R(hr * 0.7, -hr * 0.6)], P_["body"])
        elif head == "straw_hat":
            L.ellipse(cx, cy, hr, hr, P_["body"])
            L.poly([R(-hr * 2.3, -hr * 0.5), R(0, -hr * 1.6), R(hr * 2.3, -hr * 0.5)], P_["body"])
            L.poly([R(-hr * 2.3, -hr * 0.75), R(hr * 2.3, -hr * 0.75), R(hr * 2.3, -hr * 0.35), R(-hr * 2.3, -hr * 0.35)], P_["accent_d"])
        elif head == "knight":
            L.ellipse(cx, cy, hr, hr * 1.1, P_["metal"])
            L.poly([R(-hr, -hr * 0.2), R(hr, -hr * 0.2), R(hr, hr), R(-hr, hr)], P_["metal"])
            plume = [R(-hr * 0.2, -hr * 1.0), R(-hr * 2.2, -hr * 2.0 + math.sin(t * math.tau)), R(-hr * 1.8, -hr * 0.6)]
            L.poly(plume, P_["accent"])
        elif head == "bucket":
            L.poly([R(-hr * 0.9, -hr), R(hr * 0.9, -hr), R(hr * 0.9, hr * 0.9), R(-hr * 0.9, hr * 0.9)], P_["metal"])
            L.ellipse(cx, cy - hr, hr * 0.9, hr * 0.4, P_["metal"])
        elif head == "horned":
            L.ellipse(cx, cy, hr, hr, P_["metal"])
            L.poly([R(hr * 0.3, -hr * 0.7), R(hr * 1.6, -hr * 1.9), R(hr * 0.9, -hr * 0.2)], P_["metal_d"])
            L.poly([R(-hr * 0.5, -hr * 0.8), R(-hr * 1.5, -hr * 2.0), R(-hr * 0.9, -hr * 0.1)], P_["metal_d"])
        elif head == "zulmat":
            L.ellipse(cx, cy, hr * 0.9, hr, P_["body"])
            L.poly([R(hr * 0.2, hr * 0.2), R(hr * 1.3, hr * 1.4), R(-hr * 0.1, hr * 0.9)], P_["body"])          # long jaw
            ph = t * math.tau
            for k, (x1, y1) in enumerate([(-2.6, -1.4), (-2.2, -2.4), (-1.0, -2.9), (0.4, -2.6), (-2.8, -0.2)]):
                wob = math.sin(ph + k) * 0.25
                L.poly([R(-hr * 0.4, -hr * 0.6), R(hr * (x1 + wob), hr * y1), R(hr * 0.5, -hr * 0.7)], P_["body"])  # flame hair / horns

    def face(L, s, pose, t):
        cx, cy = s.head
        cy -= hr * 0.9
        tilt = math.radians(pose["head"] + pose["lean"] * 0.4)
        if head in ("knight", "bucket"):
            a, b = rot(cx + hr * 0.1, cy - hr * 0.1, cx, cy, tilt), rot(cx + hr * 0.95, cy - hr * 0.1, cx, cy, tilt)
            L.line(a, b, P_["outline"], max(1, int(hr * 0.25)))          # visor slit
            return
        n = 2 if head in ("zulmat", "ninja", "hood") else 1
        for k in range(n):
            ex, ey = rot(cx + hr * (0.25 + 0.45 * k), cy + hr * 0.05, cx, cy, tilt)
            w = max(1.2, hr * 0.28)
            L.poly([(ex - w, ey - w * 0.4), (ex + w, ey), (ex - w * 0.2, ey + w * 0.55)], P_["eye"])

    def Wp(L, s, pose, t):
        hx, hy = s.hand_f
        a = pose["wpn"]
        ln = wlen or {"katana": height * 0.42, "longblade": height * 0.55, "curved": height * 0.45, "longsword": height * 0.5,
                      "knife": height * 0.14, "greatsword": height * 0.75, "claw_blade": height * 0.5}.get(weapon, 0)
        if weapon in ("none", "ram") or ln == 0:
            return
        blade = P_["metal"] if pal is LT else P_["metal"]
        w0 = {"greatsword": 3.6, "longblade": 1.8, "longsword": 1.8, "katana": 1.4, "curved": 1.5, "knife": 1.2, "claw_blade": 2.2}[weapon]
        L.capsule(s.along((hx, hy), a + 180, max(2, ln * 0.12)), (hx, hy), 1.2, 1.2, P_["accent_d"])    # grip
        if weapon == "curved":
            pts = [s.along((hx, hy), a + 6 * k, ln * k / 4) for k in range(5)]
            for p0, p1 in zip(pts, pts[1:]):
                L.capsule(p0, p1, w0, w0 * 0.8, blade)
        else:
            L.capsule((hx, hy), s.along((hx, hy), a, ln), w0, 0.5, blade)
        if weapon in ("claw_blade",) and pal is not LT:
            L.capsule(s.along((hx, hy), a, 2), s.along((hx, hy), a, ln * 0.85), 0.8, 0.2, P_["accent"])
        gx, gy = s.along((hx, hy), a, 1)
        L.capsule(s.along((gx, gy), a + 90, w0 + 2), s.along((gx, gy), a - 90, w0 + 2), 0.8, 0.8, P_["accent_d"])

    def back(L, s, pose, t):
        if scarf:
            _scarf_tail(L, s, pose, t, P_["accent"], P_["accent_d"], length=height * 0.25, width=height * 0.07, ragged=pal is not LT)
        if shield == "kite_back":
            nx, ny = s.neck
            L.poly([(nx - 7, ny + 1), (nx + 1, ny + 1), (nx, ny + 14), (nx - 4, ny + 18), (nx - 8, ny + 12)], P_["metal"])
        if "aura" in (extra or ""):   # Kage Caller crescent
            cx, cy = s.head
            L.ellipse(cx - 2, cy - hr, hr * 2.2, hr * 2.2, P_["accent_d"])
            L.ellipse(cx - 0.5, cy - hr * 1.15, hr * 1.9, hr * 1.9, (0, 0, 0, 0))
        if "smoke" in (extra or ""):
            hx, hy = s.hip
            for k in range(4):
                x = hx - 6 - k * 5 + math.sin(t * math.tau + k) * 2
                L.ellipse(x, hy + d["thigh"] + d["shin"] - 3 - k, 3 + k * 0.6, 2 + k * 0.4, P_["back"])

    def front(L, s, pose, t):
        if "ram" in (extra or ""):   # Siege Ram shoulder plate pointing forward
            nx, ny = s.neck
            L.poly([(nx - 4, ny - 2), (nx + height * 0.36, ny + 3), (nx + height * 0.40, ny + 7), (nx + height * 0.30, ny + 11), (nx - 4, ny + 10)], P_["metal"])
            L.poly([(nx + height * 0.26, ny + 2), (nx + height * 0.46, ny - 3), (nx + height * 0.34, ny + 6)], P_["metal_d"])
        if shield == "tower":
            hx, hy = s.hand_b
            L.rect(hx - 4, hy - height * 0.30, hx + 7, hy + height * 0.18, P_["metal"])
            L.rect(hx + 1, hy - height * 0.28, hx + 3, hy + height * 0.16, P_["metal_d"])

    def foot(L, f, k, pose, back=False):
        fx, fy = f
        c = P_["back"] if back else P_["body"]
        r = d["leg_r"]
        if pal is LT and torso in ("armor", "bulky"):
            c = P_["metal_d"] if back else P_["metal"]
        L.rect(fx - r, fy - r * 1.5, fx + r * 1.1, fy, c)
        L.rect(fx - r, fy - 1.2, fx + r * 2.0, fy, c)

    def hand(L, h, pose, back=False):
        L.ellipse(h[0], h[1], d["arm_r"] * 1.05, d["arm_r"] * 1.05, P_["back"] if back else P_["body"])

    def post(img, sk, pose, t):
        L = Layer(img.width, img.height)
        face(L, sk, pose, t)
        img.alpha_composite(L.img)
        return img

    st = Style(dims=d, body=P_["body"], body_back=P_["back"], shade=P_["shade"], outline=P_["outline"], accent_dark=P_["accent_d"],
               back=back, torso=T, head=Hd, foot=foot, hand=hand, weapon=Wp, weapon_outline=P_["outline"], front=front, rim=P_["rim"], post=post)
    st.height = height
    st.pal = pal
    return st
