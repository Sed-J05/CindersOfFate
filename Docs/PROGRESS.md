# PROGRESS (Claude keeps this updated after every prompt)

Legend: ✅ done · 🔧 in progress · ⬜ not started · ⚠️ blocked

| # | Prompt | Status | Notes |
|---|---|---|---|
| 0 | Kickoff & project setup | ✅ done (2026-10-17) | Folders + `.gitkeep`, `.gitignore`, `GameLayers.cs`, `Shatranj/Setup Project`, `README_DEV.md`, DECISIONS D-001…D-013. Human must run the menu item once (see below). |
| 1 | Core framework | ⬜ not started | GameManager, GameEvents, SaveSystem, SceneFlow, HitStop, ObjectPool, Tween, AudioManager, DebugHotkeys, Health/DamageInfo, all configs + `Generate Config Assets`, EditMode tests |
| 2 | Player movement, input, camera, sandbox | ⬜ not started | |
| 3 | Player combat, soul, heal, dash, fireball | ⬜ not started | |
| 4 | Placeholder art + animation pipeline | ⬜ not started | |
| 5 | HUD | ⬜ not started | |
| 6 | Level pipeline | ⬜ not started | includes `Validate Project` |
| 7 | Enemy framework + Phase 1 enemies | ⬜ not started | |
| 8 | Phase 1 Section A + MB1 | ⬜ not started | |
| 9 | Phase 1 Section B + MB2 + orb | ⬜ not started | |
| 10 | Shadow puppetry visuals | ⬜ not started | also picks up URP/quality tweaks deferred from Prompt 0 (D-011) |
| 11 | Cutscene system + CS1/CS2 + menus | ⬜ not started | |
| 12 | Phase 2 systems + enemies | ⬜ not started | |
| 13 | Phase 2 Section C + MB3 | ⬜ not started | |
| 14 | Phase 2 Section D + throne room | ⬜ not started | |
| 15 | Final boss Zulmat | ⬜ not started | |
| 16 | Ending cutscene + loop | ⬜ not started | |
| 17 | Polish & balance | ⬜ not started | |
| 18 | QA & build | ⬜ not started | sets `productName = Shatranj` for builds (D-002) |

## Environment facts (verified 2026-10-17)
- Unity **6000.0.32f1**, URP 17.0.3 (2D Renderer, `Assets/Settings/Renderer2D.asset`), Input System 1.11.2, ugui 2.0.0 (TMP), Test Framework 1.4.5, 2D feature set 2.0.1.
- Active Input Handling = **Both** (OK). Default screen 1920×1080.
- Project folder/product name is `cinders of fate` (D-002). **Not a git repository yet.**

## Pending human steps (carry forward until done)
1. Open Unity, wait for compile, run **Shatranj → Setup Project** once; check the Console summary says `Validation: PASS`.
2. `git init` + first commit (the `.gitignore` is in place).

## Next up
Prompt 1 — Core framework (`Docs/09_BUILD_PROMPTS.md`).
