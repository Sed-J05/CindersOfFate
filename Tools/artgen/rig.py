"""2D humanoid rig: poses are a few joint angles, animations are keyframe lists interpolated to N frames.

Angles are in degrees, 0 = pointing straight DOWN, positive = rotated toward +x (the facing direction, right).
Coordinates are low-res pixels, y down; the root (between the feet on the ground) sits at the canvas bottom-centre.
"""
import math

BASE = dict(x=0.0, y=0.0, lean=0.0, head=0.0,
            ua_f=10.0, la_f=20.0, ua_b=-8.0, la_b=-4.0,      # upper/lower arm, front & back
            ul_f=6.0, ll_f=0.0, ul_b=-6.0, ll_b=0.0,         # thigh/shin, front & back
            wpn=60.0,                                         # weapon angle (absolute, 0 = down)
            scarf=0.0, cape=0.0, fx=0.0, squash=1.0, glow=0.0, open=0.0)


def P(**kw):
    p = dict(BASE)
    p.update(kw)
    return p


def interp(a, b, t):
    return {k: a[k] + (b.get(k, a[k]) - a[k]) * t for k in a}


def frames(keys, n, loop):
    """Spread keyframes across n frames. Loop: keys form a cycle and the last frame leads back into the first."""
    if n <= 1 or len(keys) == 1:
        return [dict(keys[0]) for _ in range(max(1, n))]
    out = []
    for i in range(n):
        if loop:
            u = i / n * len(keys)
            k = int(u) % len(keys)
            a, b = keys[k], keys[(k + 1) % len(keys)]
        else:
            segs = len(keys) - 1
            u = i / (n - 1) * segs
            k = min(int(u), segs - 1)
            a, b = keys[k], keys[k + 1]
        t = u - int(u) if (loop or int(u) <= len(keys) - 2) else 1.0
        if not loop and i == n - 1:
            t = 1.0
        t = t * t * (3 - 2 * t)
        out.append(interp(a, b, t))
    return out


def vec(ang_deg, length):
    a = math.radians(ang_deg)
    return math.sin(a) * length, math.cos(a) * length


class Skeleton:
    """Forward kinematics for one pose. `dims` holds lengths in low-res px."""

    def __init__(self, dims, pose, root):
        self.d = dims
        self.p = pose
        rx, ry = root
        leg = dims["thigh"] + dims["shin"]
        # hip height follows the legs: average vertical reach of both legs
        reach_f = math.cos(math.radians(pose["ul_f"])) * dims["thigh"] + math.cos(math.radians(pose["ll_f"])) * dims["shin"]
        reach_b = math.cos(math.radians(pose["ul_b"])) * dims["thigh"] + math.cos(math.radians(pose["ll_b"])) * dims["shin"]
        hip_h = max(reach_f, reach_b)
        self.hip = (rx + pose["x"], ry - hip_h + pose["y"])
        hx, hy = self.hip
        sp = dims.get("hip_w", 2)
        self.hip_f = (hx + sp * 0.5, hy)
        self.hip_b = (hx - sp * 0.5, hy)
        # legs
        self.knee_f = self._add(self.hip_f, vec(pose["ul_f"], dims["thigh"]))
        self.foot_f = self._add(self.knee_f, vec(pose["ll_f"], dims["shin"]))
        self.knee_b = self._add(self.hip_b, vec(pose["ul_b"], dims["thigh"]))
        self.foot_b = self._add(self.knee_b, vec(pose["ll_b"], dims["shin"]))
        # torso (lean: positive = forward)
        tl = dims["torso"] * pose.get("squash", 1.0)
        lx, ly = vec(180 - pose["lean"], tl)
        self.neck = (hx + lx, hy + ly)
        self.chest = (hx + lx * 0.5, hy + ly * 0.5)
        sh = dims.get("shoulder_drop", 2)
        nx, ny = self.neck
        self.sh_f = (nx + 1, ny + sh)
        self.sh_b = (nx - 1, ny + sh)
        self.elbow_f = self._add(self.sh_f, vec(pose["ua_f"], dims["uarm"]))
        self.hand_f = self._add(self.elbow_f, vec(pose["la_f"], dims["larm"]))
        self.elbow_b = self._add(self.sh_b, vec(pose["ua_b"], dims["uarm"]))
        self.hand_b = self._add(self.elbow_b, vec(pose["la_b"], dims["larm"]))
        hx2, hy2 = vec(180 - pose["lean"] - pose["head"] * 0.5, dims.get("neck", 2))
        self.head = (nx + hx2, ny + hy2)

    @staticmethod
    def _add(p, v):
        return p[0] + v[0], p[1] + v[1]

    def along(self, p0, ang_deg, length):
        return self._add(p0, vec(ang_deg, length))
