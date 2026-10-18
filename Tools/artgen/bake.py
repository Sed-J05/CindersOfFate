"""Writes generated frames to the exact paths / sizes the game expects (Assets/Data/AnimationManifest.json)."""
import io, json, os
from PIL import Image
from core import upscale, SCALE

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
MANIFEST = json.load(io.open(os.path.join(ROOT, "Assets", "Data", "AnimationManifest.json"), encoding="utf-8"))
SETS = {s["name"]: s for s in MANIFEST["sets"]}
WRITTEN = []


def spec(name):
    return SETS[name]


def anim_spec(name, anim):
    for a in SETS[name]["anims"]:
        if a["name"] == anim:
            return a
    raise KeyError(f"{name}/{anim} not in the manifest")


def path_for(name, anim, i):
    s = SETS[name]
    folder, layout = s["folder"], s["layout"]
    a = anim_spec(name, anim)
    if layout == "character":
        return os.path.join(ROOT, "Assets", "Art", folder, name, anim, f"{name}_{anim}_{i:02d}.png")
    if layout == "set":
        mid = f"{name}_{anim}_" if anim else f"{name}_"
        return os.path.join(ROOT, "Assets", "Art", folder, name, f"{mid}{i:02d}.png")
    if a["frames"] <= 1:
        return os.path.join(ROOT, "Assets", "Art", folder, f"{name}.png")
    return os.path.join(ROOT, "Assets", "Art", folder, f"{name}_{i:02d}.png")


def save(name, anim, images, lowres=True):
    """images: list of PIL RGBA, low-res (upscaled here) unless lowres=False. Checks count and canvas size."""
    s = SETS[name]
    a = anim_spec(name, anim)
    if len(images) != a["frames"]:
        raise ValueError(f"{name}/{anim}: {len(images)} frames, manifest wants {a['frames']}")
    for i, im in enumerate(images):
        out = upscale(im) if lowres else im
        if out.size != (s["width"], s["height"]):
            out = out.resize((s["width"], s["height"]), Image.NEAREST)
        p = path_for(name, anim, i)
        os.makedirs(os.path.dirname(p), exist_ok=True)
        out.save(p)
        WRITTEN.append(p)


def lowres_size(name):
    s = SETS[name]
    return s["width"] // SCALE, s["height"] // SCALE
