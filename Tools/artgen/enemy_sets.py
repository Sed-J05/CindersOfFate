"""Animations for every humanoid enemy and boss."""
import math
from rig import P, frames, Skeleton
from humanoid import render
from enemies import make, DK, LT, ZU
from anims import idle_keys, walk_keys, run_keys, hurt_keys, crouch, death_keys, dissolve, flash, glow_orb, aura, speed_lines, ghost
from core import Layer, hexc
from bake import lowres_size


class Builder:
    def __init__(self, name, st, base):
        self.name, self.st, self.base = name, st, base
        self.W, self.H = lowres_size(name)

    def R(self, p, t=0.0):
        return render(self.st, p, self.W, self.H, t)

    def seq(self, keys, n, loop, tele=False):
        fs = frames(keys, n, loop)
        out = [self.R(p, i / n) for i, p in enumerate(fs)]
        if tele:   # windup brightens toward the accent colour (reads as a warning)
            out = [flash(im, self.st.pal["accent"], 0.15 + 0.35 * i / max(1, n - 1)) for i, im in enumerate(out)]
        return out

    def skel(self, p):
        return Skeleton(self.st.dims, p, (self.W / 2 + self.st.dims.get("root_dx", 0), self.H - self.st.dims["margin"]))

    # common animations
    def idle(self, n):
        return self.seq(idle_keys(self.base), n, True)

    def walk(self, n, amp=1.0):
        return self.seq(walk_keys(self.base, amp), n, True)

    def hurt(self, n):
        return self.seq(hurt_keys(self.base), n, False)

    def death(self, n):
        ks = frames(death_keys(self.base), n, False)
        out = []
        start = n // 2
        for i, p in enumerate(ks):
            amt = 0 if i < start else min(0.98, (i - start + 1) / (n - start))
            out.append(dissolve(self.R(p, i / n), amt, self.st.pal["rim"] or self.st.pal["accent"], seed=31 + i))
        return out

    def intro(self, n):
        b = self.base
        keys = [dict(b, head=25, lean=8, ua_f=0, la_f=0, wpn=10), dict(b, head=10, lean=4), dict(b, head=-5, ua_f=60, la_f=110, wpn=140),
                dict(b, head=-10, ua_f=20, la_f=60, wpn=100)]
        out = self.seq(keys, n, False)
        return [flash(im, self.st.pal["accent"], 0.4) if i == n - 2 else im for i, im in enumerate(out)]


def swing(b, frm, to, n_extra=0, lean=8, x=2):
    """Attack keyframes: windup (frm), strike (to)."""
    return [dict(b, **frm), dict(b, ua_f=(frm.get("ua_f", 0) + to["ua_f"]) / 2, la_f=(frm.get("la_f", 0) + to["la_f"]) / 2,
                                 wpn=(frm.get("wpn", 0) + to["wpn"]) / 2, lean=lean * 0.5),
            dict(b, lean=lean, x=x, **to), dict(b, lean=lean * 0.4, x=x * 0.5, ua_f=to["ua_f"] * 0.7, la_f=to["la_f"] * 0.7, wpn=to["wpn"] * 0.85)]


# ------------------------------------------------------------------ Lunger
def lunger():
    st = make(DK, 50, head="ninja", torso="slim", weapon="none", scarf=True)
    b = P(ua_f=20, la_f=60, ua_b=-15, la_b=20)
    B = Builder("Lunger", st, b)
    A = {"Idle": B.idle(4), "Walk": B.walk(6), "Hurt": B.hurt(2), "Death": B.death(6)}
    A["Telegraph"] = B.seq([crouch(b, 0.6), crouch(dict(b, ua_f=60, la_f=90), 1.0), crouch(dict(b, ua_f=70, la_f=100, head=-5), 1.15)], 3, False, tele=True)
    lp = dict(b, lean=72, ul_f=-35, ll_f=-60, ul_b=-55, ll_b=-80, ua_f=100, la_f=100, ua_b=95, la_b=95, cape=1.3)
    A["Lunge"] = [B.R(dict(lp, lean=55), 0), speed_lines(B.R(lp, 0.33), DK["accent"], B.H * 0.5, B.H * 0.8, B.W * 0.4, 4, 5),
                  speed_lines(B.R(dict(lp, lean=75), 0.66), DK["accent"], B.H * 0.5, B.H * 0.8, B.W * 0.4, 4, 6)]
    A["Recover"] = B.seq([crouch(dict(b, lean=25), 0.9), crouch(b, 0.6), b], 3, False)
    return A


# ------------------------------------------------------------------ Kage Warden
def warden():
    st = make(DK, 120, head="straw_hat", torso="slim", weapon="longblade", scarf=False)
    b = P(ua_f=30, la_f=70, ua_b=20, la_b=70, wpn=120, ul_f=14, ul_b=-14)
    B = Builder("KageWarden", st, b)
    A = {"Intro": B.intro(6), "Idle": B.idle(6), "Walk": B.walk(8, 0.8), "Death": B.death(10)}
    A["Slash_Telegraph"] = B.seq([dict(b, ua_f=130, la_f=165, wpn=175), dict(b, ua_f=175, la_f=205, wpn=195, lean=-5),
                                  dict(b, ua_f=200, la_f=225, wpn=205, lean=-8)], 3, False, tele=True)
    A["Slash_A"] = B.seq(swing(b, dict(ua_f=200, la_f=225, wpn=205), dict(ua_f=95, la_f=100, wpn=100)), 4, False)
    A["Slash_B"] = B.seq(swing(b, dict(ua_f=40, la_f=10, wpn=-40), dict(ua_f=140, la_f=160, wpn=160)), 4, False)
    guard = dict(b, ua_f=60, la_f=150, ua_b=50, la_b=150, wpn=175, lean=-3)
    A["Block_Start"] = B.seq([b, dict(guard, wpn=150), guard], 3, False)
    A["Block_Hold"] = [B.R(guard, 0), B.R(dict(guard, y=-0.5), 0.5)]
    A["Block_Impact"] = [flash(B.R(dict(guard, x=-2, lean=-8), 0), hexc("FFFFFF"), 0.45), B.R(guard, 0.5)]
    return A


# ------------------------------------------------------------------ Kage Caller
def caller():
    st = make(DK, 115, head="deep_hood", torso="robe", weapon="curved", extra="aura smoke")
    b = P(ua_f=25, la_f=65, ua_b=-10, la_b=10, wpn=110)
    B = Builder("KageCaller", st, b)
    A = {"Intro": B.intro(6), "Idle": B.idle(6), "Walk": B.walk(8, 0.6), "Death": B.death(10)}
    A["Slash_Telegraph"] = B.seq([dict(b, ua_f=130, la_f=165, wpn=175), dict(b, ua_f=175, la_f=205, wpn=195), dict(b, ua_f=200, la_f=225, wpn=205)], 3, False, tele=True)
    A["Slash_A"] = B.seq(swing(b, dict(ua_f=200, la_f=225, wpn=205), dict(ua_f=95, la_f=100, wpn=100)), 4, False)
    A["Slash_B"] = B.seq(swing(b, dict(ua_f=40, la_f=10, wpn=-40), dict(ua_f=140, la_f=160, wpn=160)), 4, False)

    def glowing(p, r, i, n):
        img = B.R(p, i / n)
        sk = B.skel(p)
        tip = sk.along(sk.hand_f, p["wpn"], 40)
        return glow_orb(img, tip[0], tip[1], r, hexc("FFB3A8"), DK["accent"]) if r > 0 else img
    up = dict(b, ua_f=170, la_f=175, wpn=180)
    A["Cast_Telegraph"] = [flash(glowing(dict(up, ua_f=120 + 15 * i, la_f=140 + 10 * i), 1.5 + i, i, 4), DK["accent"], 0.1 * i) for i in range(4)]
    A["Cast_Fire"] = B.seq(swing(b, dict(ua_f=170, la_f=175, wpn=180), dict(ua_f=95, la_f=95, wpn=95)), 4, False)
    raise_p = dict(b, ua_b=-150, la_b=-170)
    A["Summon_Raise"] = [aura(B.R(dict(b, ua_b=-20 - 25 * i, la_b=-30 - 25 * i), i / 6), *B.skel(raise_p).hand_b, 8, 6, DK["accent"], i / 6) for i in range(6)]
    A["Summon_Hold"] = [aura(B.R(raise_p, k / 2), *B.skel(raise_p).hand_b, 9, 7, DK["accent"], k / 2) for k in range(2)]
    return A


# ------------------------------------------------------------------ Castle Knight
def knight():
    st = make(LT, 95, head="knight", torso="armor", weapon="longsword", shield="kite_back")
    b = P(ua_f=30, la_f=70, ua_b=-10, la_b=10, wpn=125)
    B = Builder("Knight", st, b)
    A = {"Idle": B.idle(4), "Walk": B.walk(8, 0.8), "Hurt": B.hurt(2), "Death": B.death(8)}
    A["Telegraph"] = B.seq([dict(b, ua_f=130, la_f=165, wpn=175), dict(b, ua_f=175, la_f=205, wpn=195, lean=-4), dict(b, ua_f=200, la_f=225, wpn=205, lean=-6)], 3, False, tele=True)
    A["Attack_A"] = B.seq(swing(b, dict(ua_f=200, la_f=225, wpn=205), dict(ua_f=92, la_f=95, wpn=95)), 4, False)
    A["Attack_B"] = B.seq(swing(b, dict(ua_f=160, la_f=170, wpn=175), dict(ua_f=60, la_f=40, wpn=40)), 4, False)
    return A


# ------------------------------------------------------------------ Thrower
def thrower():
    st = make(LT, 90, head="hood", torso="slim", weapon="knife", scarf=True)
    b = P(ua_f=15, la_f=40, ua_b=-10, la_b=10, wpn=60)
    B = Builder("Thrower", st, b)
    A = {"Idle": B.idle(4), "Walk": B.walk(6, 0.8), "Hurt": B.hurt(2), "Death": B.death(8)}
    A["Throw_Telegraph"] = B.seq([dict(b, ua_f=130, la_f=160, wpn=170), dict(b, ua_f=190, la_f=215, wpn=200, lean=-5), dict(b, ua_f=205, la_f=230, wpn=210, lean=-8)], 3, False, tele=True)
    A["Throw"] = B.seq([dict(b, ua_f=40, la_f=60, wpn=70, lean=6), dict(b, ua_f=95, la_f=95, wpn=95, lean=10, x=2), dict(b, ua_f=70, la_f=60, wpn=60, lean=5)], 3, False)
    return A


# ------------------------------------------------------------------ Siege Ram (Charger)
def charger():
    st = make(LT, 75, head="horned", torso="bulky", weapon="ram", bulk=1.9, extra="ram")
    b = P(ua_f=10, la_f=30, ua_b=-10, la_b=10, lean=10)
    B = Builder("Charger", st, b)
    A = {"Idle": B.idle(4), "Walk": B.walk(6, 0.6), "Hurt": B.hurt(2), "Death": B.death(8)}
    A["Telegraph"] = B.seq([dict(b, lean=25, head=15, ul_f=30, ll_f=-10), dict(b, lean=30, head=18, ul_f=10, ll_f=10), dict(b, lean=30, head=18, ul_f=30, ll_f=-10),
                            crouch(dict(b, lean=35, head=20), 0.7)], 4, False, tele=True)
    A["Charge"] = [speed_lines(im, LT["accent"], B.H * 0.45, B.H * 0.85, B.W * 0.3, 5, 40 + i) for i, im in enumerate(B.seq(run_keys(dict(b, lean=30), 0.9, lean=30), 4, True))]

    def stars(img, ph):
        L = Layer(img.width, img.height)
        sk = B.skel(b)
        cx, cy = sk.head
        for k in range(3):
            a = ph * math.tau + k * math.tau / 3
            x, y = cx + math.cos(a) * 9, cy - 14 + math.sin(a) * 3
            L.poly([(x, y - 2), (x + 0.7, y - 0.5), (x + 2, y), (x + 0.7, y + 0.5), (x, y + 2), (x - 0.7, y + 0.5), (x - 2, y), (x - 0.7, y - 0.5)], hexc("FFE680"))
        img.alpha_composite(L.img)
        return img
    A["Stunned"] = [stars(B.R(dict(b, lean=-5, head=-15 + 30 * (i % 2), ua_f=5, ua_b=-5), i / 4), i / 4) for i in range(4)]
    return A


# ------------------------------------------------------------------ Iron Colossus
def colossus():
    st = make(LT, 130, head="bucket", torso="bulky", weapon="greatsword", shield="tower", bulk=1.9)
    b = P(ua_f=25, la_f=60, ua_b=10, la_b=40, wpn=150, ul_f=12, ul_b=-12)
    B = Builder("IronColossus", st, b)
    A = {"Intro": B.intro(6), "Idle": B.idle(6), "Walk": B.walk(8, 0.5), "Death": B.death(12)}
    A["Slash_Telegraph"] = B.seq([dict(b, ua_f=120, la_f=155, wpn=170), dict(b, ua_f=160, la_f=190, wpn=185), dict(b, ua_f=200, la_f=225, wpn=205, lean=-5),
                                  dict(b, ua_f=205, la_f=230, wpn=210, lean=-7)], 4, False, tele=True)
    sw = swing(b, dict(ua_f=205, la_f=230, wpn=210), dict(ua_f=95, la_f=95, wpn=95), lean=10, x=3)
    A["Slash"] = B.seq(sw + [dict(b, lean=3)], 5, False)
    guard = dict(b, ua_b=70, la_b=90, lean=-2)
    A["Block_Start"] = B.seq([b, dict(guard, ua_b=40, la_b=60), guard], 3, False)
    A["Block_Hold"] = [B.R(guard, 0), B.R(dict(guard, y=-0.5), 0.5)]
    A["Block_Impact"] = [flash(B.R(dict(guard, x=-2), 0), hexc("FFFFFF"), 0.45), B.R(guard, 0.5)]
    A["Bash"] = B.seq([dict(guard, ua_b=30, la_b=50, x=-1), dict(guard, ua_b=95, la_b=95, x=4, lean=10), dict(guard, ua_b=80, la_b=90, x=2, lean=5)], 3, False)
    A["Jump_Telegraph"] = B.seq([crouch(b, 0.5), crouch(dict(b, ua_f=150, la_f=170, wpn=175), 0.9), crouch(dict(b, ua_f=160, la_f=175, wpn=180), 1.1)], 3, False, tele=True)
    air = dict(b, ul_f=40, ll_f=-20, ul_b=20, ll_b=-35, ua_f=165, la_f=175, wpn=180)
    A["Jump_Air"] = [B.R(air, 0), B.R(dict(air, ul_f=30, ll_f=-10), 0.5)]
    A["Jump_Land"] = B.seq([crouch(dict(b, ua_f=95, la_f=95, wpn=95, lean=20), 1.2), crouch(dict(b, ua_f=90, la_f=90, wpn=90, lean=15), 0.9), crouch(b, 0.4), b], 4, False)
    A["Dash_Telegraph"] = B.seq([dict(guard, lean=10), dict(guard, lean=20, x=-1), dict(guard, lean=28, x=-2)], 3, False, tele=True)
    A["Dash"] = [speed_lines(im, LT["accent"], B.H * 0.45, B.H * 0.85, B.W * 0.3, 6, 50 + i)
                 for i, im in enumerate(B.seq(run_keys(dict(guard, lean=25), 0.7, lean=25)[:3], 3, True))]
    return A


# ------------------------------------------------------------------ Zulmat
def zulmat():
    st = make(ZU, 160, head="zulmat", torso="robe", weapon="claw_blade", bulk=1.15)
    b = P(ua_f=30, la_f=70, ua_b=-20, la_b=0, wpn=135, ul_f=10, ul_b=-10)
    B = Builder("Zulmat", st, b)
    A = {"Intro": B.intro(8), "Idle": B.idle(8), "Walk": B.walk(8, 0.6), "Death": B.death(14)}
    A["Slash_Telegraph"] = B.seq([dict(b, ua_f=130, la_f=165, wpn=175), dict(b, ua_f=175, la_f=205, wpn=195), dict(b, ua_f=200, la_f=225, wpn=205, lean=-6)], 3, False, tele=True)
    A["Slash_A"] = B.seq(swing(b, dict(ua_f=200, la_f=225, wpn=205), dict(ua_f=95, la_f=98, wpn=98)), 4, False)
    A["Slash_B"] = B.seq(swing(b, dict(ua_f=40, la_f=10, wpn=-40), dict(ua_f=140, la_f=160, wpn=160)), 4, False)
    down = dict(b, ua_b=90, la_b=40, lean=18)
    A["Vine_Cast"] = B.seq([dict(b, ua_b=-150, la_b=-170), dict(b, ua_b=-90, la_b=-60), crouch(down, 0.6), crouch(down, 0.8), crouch(down, 0.5), b], 6, False)
    A["Vine_Cast"] = [aura(im, B.W * 0.65, B.H - 6, 10, 2, ZU["accent"], i / 6) if 2 <= i <= 4 else im for i, im in enumerate(A["Vine_Cast"])]
    arms_up = dict(b, ua_f=150, la_f=165, ua_b=-150, la_b=-165, wpn=170)
    A["Summon_Cast"] = [aura(im, B.W / 2, B.H * 0.35, 30, 14, ZU["accent"], i / 6) for i, im in enumerate(B.seq([b, dict(arms_up, ua_f=100, ua_b=-100), arms_up, arms_up, dict(arms_up, ua_f=110, ua_b=-110), b], 6, False))]
    curled = crouch(dict(b, lean=40, head=35, ua_f=60, la_f=150, ua_b=50, la_b=150), 1.3)
    A["Shrink"] = B.seq([b, dict(crouch(b, 0.6), lean=20, head=15), crouch(dict(b, lean=32, head=28, ua_f=50, la_f=140), 1.0), curled], 4, False)
    A["Grow"] = B.seq([curled, crouch(dict(b, lean=20, head=15), 0.7), dict(arms_up, ua_f=100, ua_b=-100), dict(arms_up, ua_f=90, ua_b=-90, head=-10)], 4, False)
    lunge = dict(b, lean=70, ul_f=-30, ll_f=-60, ul_b=-50, ll_b=-80, ua_f=100, la_f=100, wpn=95, cape=1.3)
    A["Lunge_Telegraph"] = B.seq([crouch(dict(b, ua_f=80, la_f=90), 0.6), crouch(dict(b, ua_f=90, la_f=95), 1.0), crouch(dict(b, ua_f=95, la_f=100), 1.2)], 3, False, tele=True)
    A["Lunge"] = [speed_lines(B.R(dict(lunge, lean=68 + 4 * i), i / 3), ZU["accent"], B.H * 0.4, B.H * 0.8, B.W * 0.35, 6, 60 + i) for i in range(3)]
    A["Swipe_Telegraph"] = B.seq([dict(b, ua_f=140, la_f=175, wpn=185), dict(b, ua_f=175, la_f=210, wpn=200, lean=-6), dict(b, ua_f=200, la_f=225, wpn=210, lean=-10),
                                  dict(b, ua_f=205, la_f=230, wpn=215, lean=-12)], 4, False, tele=True)
    A["Swipe"] = B.seq([dict(b, ua_f=200, la_f=220, wpn=205), dict(b, ua_f=40, la_f=70, wpn=80, lean=15, x=3), dict(b, ua_f=88, la_f=88, wpn=88, lean=22, x=5),
                        dict(b, ua_f=100, la_f=100, wpn=100, lean=15, x=4), dict(b, lean=5)], 5, False)
    fists = dict(b, ua_f=160, la_f=170, ua_b=-160, la_b=-170)
    lava = B.seq([b, fists, crouch(dict(b, ua_f=90, la_f=30, ua_b=60, la_b=20, lean=20), 1.0), crouch(dict(b, ua_f=90, la_f=30, ua_b=60, la_b=20, lean=18), 0.9),
                  dict(b, x=-2), b], 6, False)
    A["Lava_Cast"] = [aura(im, B.W / 2, B.H - 5, 26, 3, hexc("FF7A2A"), i / 6) if i in (2, 3, 4) else im for i, im in enumerate(lava)]
    roar = dict(b, head=-30, lean=-10, ua_f=120, la_f=150, ua_b=-120, la_b=-150)
    A["Roar"] = [flash(im, ZU["accent"], 0.35) if i == 3 else im for i, im in enumerate(B.seq([b, dict(roar, head=-15), roar, dict(roar, head=-35), dict(roar, head=-20), b], 6, False))]
    return A


ALL = {"Lunger": lunger, "KageWarden": warden, "KageCaller": caller, "Knight": knight, "Thrower": thrower,
       "Charger": charger, "IronColossus": colossus, "Zulmat": zulmat}


def build(name):
    return ALL[name]()
