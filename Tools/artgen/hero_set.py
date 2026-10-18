"""All protagonist animations (both phases)."""
from rig import P, frames
from humanoid import render
from hero import HERO1, HERO2, B1, W1, D1, R2, EYE2, EMBER
from anims import idle_keys, run_keys, hurt_keys, crouch, death_keys, dissolve, glow_orb, aura, speed_lines, ghost
from rig import Skeleton
from core import hexc


def _two_hand(p):
    """Phase 2 holds the big sword with both hands: back arm follows the front arm."""
    q = dict(p)
    q["ua_b"] = p["ua_f"] - 10
    q["la_b"] = p["la_f"] - 5
    return q


def build(p2=False):
    st = HERO2 if p2 else HERO1
    W, H = 128, 128
    glow_core = hexc("FFD6CC") if p2 else hexc("FFFFFF")
    glow_edge = R2 if p2 else B1
    ember = EMBER if p2 else B1
    base = P(ua_f=35, la_f=80, ua_b=25, la_b=75, wpn=155) if p2 else P(ua_f=10, la_f=32, ua_b=-10, la_b=-4, wpn=105)
    fix = _two_hand if p2 else (lambda p: p)

    def R(p, t=0.0):
        return render(st, fix(p), W, H, t)

    def seq(keys, n, loop):
        fs = frames(keys, n, loop)
        return [R(p, i / n) for i, p in enumerate(fs)]

    A = {}
    A["Idle"] = seq(idle_keys(base), 6, True)
    A["Run"] = seq(run_keys(base), 8, True)
    A["Jump_Rise"] = [R(dict(base, ul_f=45, ll_f=-25, ul_b=25, ll_b=-45, ua_f=120, la_f=150, ua_b=-60, la_b=-30, cape=0.2)),
                      R(dict(base, ul_f=20, ll_f=-10, ul_b=5, ll_b=-25, ua_f=140, la_f=160, ua_b=-80, la_b=-60, cape=0.1), 0.3)]
    A["Jump_Fall"] = [R(dict(base, ul_f=18, ll_f=8, ul_b=-12, ll_b=-4, ua_f=95, la_f=110, ua_b=-95, la_b=-110, cape=1.0), 0.0),
                      R(dict(base, ul_f=22, ll_f=10, ul_b=-16, ll_b=-6, ua_f=105, la_f=120, ua_b=-105, la_b=-120, cape=1.1), 0.5)]
    A["Land"] = [R(crouch(base, 1.0)), R(crouch(base, 0.5), 0.3), R(base, 0.6)]
    A["Attack_Side_A"] = [R(dict(base, ua_f=195, la_f=220, wpn=210, lean=-4)), R(dict(base, ua_f=40, la_f=80, wpn=60, lean=8), 0.25),
                          R(dict(base, ua_f=85, la_f=92, wpn=95, lean=12, x=2), 0.5), R(dict(base, ua_f=60, la_f=72, wpn=82, lean=6, x=1), 0.75)]
    A["Attack_Side_B"] = [R(dict(base, ua_f=20, la_f=-20, wpn=-60, lean=-2)), R(dict(base, ua_f=110, la_f=150, wpn=150, lean=8), 0.25),
                          R(dict(base, ua_f=130, la_f=160, wpn=165, lean=10, x=2), 0.5), R(dict(base, ua_f=80, la_f=100, wpn=110, lean=5, x=1), 0.75)]
    A["Attack_Up"] = [R(dict(base, ua_f=20, la_f=0, wpn=10, ul_f=25, ll_f=-10, ul_b=10, ll_b=-20)),
                      R(dict(base, ua_f=140, la_f=165, wpn=175, head=-10), 0.25),
                      R(dict(base, ua_f=175, la_f=180, wpn=180, y=-2, head=-15), 0.5),
                      R(dict(base, ua_f=120, la_f=140, wpn=150, head=-5), 0.75)]

    def cast_frame(p, r, i):
        img = R(p, i / 5)
        sk = Skeleton(st.dims, fix(p), (W / 2, H - st.dims["margin"]))
        if r > 0:
            img = glow_orb(img, sk.hand_f[0] + 2, sk.hand_f[1] - 1, r, glow_core, glow_edge)
        return img
    cast_poses = [dict(base, ua_f=-30, la_f=-60, lean=-3), dict(base, ua_f=-20, la_f=-40, lean=-2),
                  dict(base, ua_f=80, la_f=90, lean=10, x=1), dict(base, ua_f=88, la_f=92, lean=12, x=2), dict(base, ua_f=60, la_f=70, lean=5)]
    A["Cast"] = [cast_frame(p, r, i) for i, (p, r) in enumerate(zip(cast_poses, (1.5, 2.5, 3.5, 5.0, 0)))]

    heal_base = dict(crouch(base, 0.55), ua_f=40, la_f=125, ua_b=30, la_b=125, open=1.0)
    hc = []
    for i in range(6):
        img = R(dict(heal_base, y=-0.5 * (i % 2)), i / 6)
        sk = Skeleton(st.dims, heal_base, (W / 2, H - st.dims["margin"]))
        img = aura(img, sk.chest[0], sk.chest[1], 14, 18, glow_edge, i / 6)
        img = glow_orb(img, sk.hand_f[0], sk.hand_f[1], 1.5 + (i % 3) * 0.8, glow_core, glow_edge)
        hc.append(img)
    A["Heal_Charge"] = hc
    A["Heal_Release"] = [aura(R(dict(base, ua_f=150, la_f=160, ua_b=-150, la_b=-160, head=-10)), W / 2, H / 2 + 10, 22, 26, glow_edge, 0.2),
                         aura(R(dict(base, ua_f=100, la_f=100, ua_b=-100, la_b=-100), 0.3), W / 2, H / 2 + 10, 28, 30, glow_edge, 0.6),
                         R(base, 0.6)]
    A["Hurt"] = [R(p, i / 2) for i, p in enumerate(hurt_keys(base))]
    dk = frames(death_keys(base), 8, False)
    amounts = [0, 0, 0, 0, 0.25, 0.5, 0.75, 0.97]
    A["Death"] = [dissolve(R(p, i / 8), amounts[i], ember, seed=11 + i) for i, p in enumerate(dk)]
    dash_p = dict(base, lean=62, ul_f=-25, ll_f=-55, ul_b=-45, ll_b=-75, ua_f=-95, la_f=-90, ua_b=-110, la_b=-100, cape=1.4, wpn=-90)
    dash = []
    for i in range(4):
        img = R(dict(dash_p, y=-4 + (i % 2)), i / 4)
        if i in (1, 2):
            img = ghost(img, 0.35, -10)
        img = speed_lines(img, glow_edge, H * 0.45, H * 0.8, W * 0.35, n=6, seed=20 + i)
        dash.append(img)
    A["Dash"] = dash
    rest = dict(base, ul_f=85, ll_f=-60, ul_b=80, ll_b=-75, lean=8, head=20, ua_f=50, la_f=80, ua_b=40, la_b=75, open=1.0, wpn=80)
    A["Rest"] = [R(dict(rest, y=-0.4 * k), i / 4) for i, k in enumerate((0, 1, 2, 1))]
    A["Look_Up"] = [R(dict(base, head=-25))]
    A["Look_Down"] = [R(dict(base, head=25))]
    return A
