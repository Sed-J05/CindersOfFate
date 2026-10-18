"""Bakes every sprite in Assets/Data/AnimationManifest.json and checks that nothing is missing.

    python Tools/artgen/build_all.py            # everything
    python Tools/artgen/build_all.py fx ui      # only some groups

Groups: hero, enemies, critters, fx, tiles, props, backgrounds, ui.
Overwrites the PNGs at the manifest paths (art lives in git, so any earlier version can be restored).
Then in Unity: Shatranj > Import & Rebuild Animations, Build Prefabs, Build All Scenes."""
import os, sys, time
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bake


def save_all(groups, lowres=True):
    for name, anims in groups.items():
        for anim, frames in anims.items():
            bake.save(name, anim, frames, lowres=lowres)


def hero():
    import hero_set
    save_all({"Protagonist": hero_set.build(False)})
    save_all({"Protagonist_P2": hero_set.build(True)})


def enemies():
    import enemy_sets
    save_all({n: enemy_sets.build(n) for n in enemy_sets.ALL})


def critters():
    import critters as c
    save_all({n: c.build(n) for n in c.ALL})


def fx():
    import fx as f
    save_all({n: fn() for n, fn in f.ALL.items()})


def tiles():
    import env
    save_all(env.build_tiles())


def props():
    import env
    save_all(env.build_props())


def backgrounds():
    import backgrounds as b
    save_all(b.build(), lowres=False)


def ui():
    import ui as u
    lo, hi = u.build()
    save_all(lo)
    save_all(hi, lowres=False)


GROUPS = {"hero": hero, "enemies": enemies, "critters": critters, "fx": fx, "tiles": tiles, "props": props,
          "backgrounds": backgrounds, "ui": ui}


def verify():
    """Every manifest frame must exist at the right canvas size."""
    missing, wrong = [], []
    total = 0
    for s in bake.MANIFEST["sets"]:
        for a in s["anims"]:
            for i in range(a["frames"]):
                total += 1
                p = bake.path_for(s["name"], a["name"], i)
                if not os.path.exists(p):
                    missing.append(p)
                    continue
                with Image.open(p) as im:
                    if im.size != (s["width"], s["height"]):
                        wrong.append(f"{p} {im.size}")
    return total, missing, wrong


def main(argv):
    names = argv or list(GROUPS)
    for n in names:
        t = time.time()
        before = len(bake.WRITTEN)
        GROUPS[n]()
        print(f"{n:12s} {len(bake.WRITTEN) - before:4d} files  {time.time() - t:5.1f}s")
    total, missing, wrong = verify()
    print(f"manifest frames: {total}, missing: {len(missing)}, wrong size: {len(wrong)}")
    for m in missing[:20]:
        print("  missing", os.path.relpath(m, bake.ROOT))
    for w in wrong[:20]:
        print("  size", w)
    return 1 if (missing or wrong) else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
