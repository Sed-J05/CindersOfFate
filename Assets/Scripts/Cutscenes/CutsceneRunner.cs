using System;
using System.Collections;
using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shatranj.Cutscenes
{
    /// <summary>
    /// Plays a JSON cutscene in the Cutscene scene (Docs/08 §1). Reads GameManager.PendingCutscene, runs the beats in
    /// order (or concurrently for <c>parallel</c> / <c>async</c>), and ends on a <c>loadScene</c> beat — or loads
    /// GameManager.NextSceneAfterCutscene if the script has none. J / Enter / Space (gamepad A) advance text; hold
    /// Esc or Enter (gamepad Start) for 1 s to skip, which fast-forwards the rest so flags and scene loads still happen.
    /// </summary>
    [RequireComponent(typeof(CutsceneStage), typeof(CutsceneUI))]
    public class CutsceneRunner : MonoBehaviour
    {
        [SerializeField] private string fallbackCutscene = GameConstants.CutsceneIntro;
        [SerializeField] private string fallbackNextScene = GameConstants.ScenePhase1;

        private CutsceneStage stage;
        private CutsceneUI ui;
        private CutsceneScript script;
        private InputAction advanceAction;
        private InputAction skipAction;
        private bool advanceQueued;
        private float skipHold;
        private bool skipping;
        private bool finished;

        public bool Skipping => skipping;
        public string CurrentId => script != null ? script.id : "";

        private void Awake()
        {
            stage = GetComponent<CutsceneStage>();
            ui = GetComponent<CutsceneUI>();

            advanceAction = new InputAction("CutsceneAdvance", InputActionType.Button);
            advanceAction.AddBinding("<Keyboard>/j");
            advanceAction.AddBinding("<Keyboard>/enter");
            advanceAction.AddBinding("<Keyboard>/space");
            advanceAction.AddBinding("<Gamepad>/buttonSouth");
            skipAction = new InputAction("CutsceneSkip", InputActionType.Button);
            skipAction.AddBinding("<Keyboard>/escape");
            skipAction.AddBinding("<Keyboard>/enter");
            skipAction.AddBinding("<Gamepad>/start");
        }

        private void OnEnable() { advanceAction.Enable(); skipAction.Enable(); }
        private void OnDisable() { advanceAction.Disable(); skipAction.Disable(); }
        private void OnDestroy() { advanceAction.Dispose(); skipAction.Dispose(); }

        private void Start()
        {
            GameManager gm = GameManager.Ensure();
            string id = !string.IsNullOrEmpty(gm.PendingCutscene) ? gm.PendingCutscene : fallbackCutscene;
            if (string.IsNullOrEmpty(gm.NextSceneAfterCutscene)) gm.NextSceneAfterCutscene = fallbackNextScene;
            if (gm.State != GameState.Cutscene) gm.BeginCutscene(id, gm.NextSceneAfterCutscene);

            SpriteCatalog cat = SpriteCatalog.Instance;
            string json = cat != null ? cat.LoadCutsceneText(id) : null;
            if (json == null)
            {
                Debug.LogError($"Cutscene '{id}' not found (Assets/Data/Cutscenes/{id}.json / SpriteCatalog). Skipping to {gm.NextSceneAfterCutscene}.");
                Finish(gm.NextSceneAfterCutscene, 0.5f);
                return;
            }
            try { script = CutsceneScript.Parse(json, id); }
            catch (Exception e)
            {
                Debug.LogError($"Cutscene '{id}': {e.Message}");
                Finish(gm.NextSceneAfterCutscene, 0.5f);
                return;
            }

            if (!string.IsNullOrEmpty(script.theme) && ThemeManager.Instance != null) ThemeManager.Instance.Apply(script.theme, 0f);
            StartCoroutine(Play());
        }

        private void Update()
        {
            if (finished) return;
            if (advanceAction.WasPressedThisFrame()) advanceQueued = true;

            float hold = script != null ? script.skipHoldTime : 1f;
            bool instantSkip = script != null && GameManager.HasInstance && GameManager.Instance.Cycle >= script.instantSkipFromCycle;
            if (skipAction.IsPressed())
            {
                skipHold += Time.unscaledDeltaTime;
                if (instantSkip || skipHold >= hold) skipping = true;
            }
            else skipHold = 0f;
            ui.SetSkipProgress(skipping ? 0f : (instantSkip ? 0f : skipHold / Mathf.Max(0.01f, hold)));
        }

        private bool ConsumeAdvance()
        {
            if (!advanceQueued) return false;
            advanceQueued = false;
            return true;
        }

        private bool IsSkipping() => skipping;

        private IEnumerator Play()
        {
            foreach (Beat b in script.beats)
            {
                if (finished) yield break;
                if (!PassesCycle(b)) continue;
                if (b.Async && !skipping) StartCoroutine(Execute(b));
                else yield return Execute(b);
            }
            if (!finished) Finish(GameManager.Instance.NextSceneAfterCutscene, 0.6f);
        }

        private static bool PassesCycle(Beat b)
        {
            int cycle = GameManager.HasInstance ? GameManager.Instance.Cycle : 1;
            return cycle >= b.MinCycle && cycle <= b.MaxCycle;
        }

        private IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds && !skipping; t += Time.deltaTime) yield return null;
        }

        private float Dur(Beat b, string key = "time", float fallback = 0f) => skipping ? 0f : b.Float(key, fallback);

        // ------------------------------------------------------------------
        // Beats
        // ------------------------------------------------------------------

        private IEnumerator Execute(Beat b)
        {
            switch (b.Type)
            {
                case "fade": yield return DoFade(b); break;
                case "theme":
                    if (ThemeManager.Instance != null) ThemeManager.Instance.Apply(b.Str("theme", ThemeManager.Phase1Beige), Dur(b));
                    yield return Wait(Dur(b));
                    break;
                case "flame": yield return DoFlame(b); break;
                case "spawn": stage.Spawn(b); break;
                case "anim": stage.PlayAnim(b.Str("id"), b.Str("anim", "Idle")); break;
                case "move": yield return DoMove(b); break;
                case "scale": yield return DoScale(b); break;
                case "rotate": yield return DoRotate(b); break;
                case "tint": yield return DoTint(b); break;
                case "alpha": yield return DoAlpha(b); break;
                case "image": yield return stage.ShowImage(b, skipping); break;
                case "remove":
                    if (skipping) stage.Remove(b.Str("id"));
                    else yield return stage.RemoveAfterFade(b.Str("id"), b.Float("fade", 0.3f));
                    break;
                case "text": yield return DoText(b); break;
                case "wait": yield return Wait(Dur(b)); break;
                case "shake":
                    if (!skipping) ScreenShake.Shake(b.Float("amp", 0.3f), b.Float("time", 0.3f));
                    break;
                case "flash":
                    if (!skipping) yield return ui.Flash(b.Colour("colour", Color.white), b.Float("time", 0.3f));
                    break;
                case "parallel": yield return DoParallel(b); break;
                case "sequence":
                    foreach (Beat child in b.Beats("beats"))
                        if (PassesCycle(child)) yield return Execute(child);
                    break;
                case "sfx": if (!skipping) AudioManager.Play(b.Str("id")); break;
                case "music": AudioManager.PlayMusic(b.Str("id")); break;
                case "card": yield return ui.Card(Expand(b.Str("text", "")), Expand(b.Str("sub", "")), Dur(b, "time", 2f), b.Colour("colour", UI.UIFactory.Ink), IsSkipping); break;
                case "zoom": yield return DoZoom(b); break;
                case "rewind": yield return ui.Rewind(Dur(b, "time", 2f), b.Colour("colour", new Color(0.24f, 0.18f, 0.12f, 0.55f)), IsSkipping); break;
                case "invertwipe": yield return DoInvertWipe(b); break;
                case "setflag": DoSetFlag(b); break;
                case "loadscene":
                    // Optional chain: "cutscene" plays another script first (cs3 → cs1), then "next" (default: Phase 1).
                    Finish(b.Str("scene", GameManager.Instance.NextSceneAfterCutscene), skipping ? 0.3f : b.Float("fade", 0.8f),
                           b.Str("cutscene", ""), b.Str("next", ""));
                    break;
                default:
                    Debug.LogWarning($"Cutscene '{CurrentId}': unknown beat type '{b.Type}'");
                    break;
            }
        }

        private IEnumerator DoFade(Beat b)
        {
            string to = b.Str("to", "clear").ToLowerInvariant();
            SceneFlow flow = SceneFlow.Instance;
            float alpha = to == "clear" ? 0f : 1f;
            if (to == "black") flow.FadeColor = Color.black;
            else if (to == "parchment") flow.FadeColor = new Color(0.91f, 0.85f, 0.71f);
            else if (to == "white") flow.FadeColor = Color.white;
            float time = Dur(b);
            if (time <= 0f) flow.SetOverlayImmediate(alpha);
            else flow.FadeTo(alpha, time);
            yield return Wait(time);
            if (skipping) flow.SetOverlayImmediate(alpha);
        }

        private IEnumerator DoFlame(Beat b)
        {
            FlameController flame = FlameController.Instance;
            float time = Dur(b);
            if (b.Has("colour") && ThemeManager.Instance != null) ThemeManager.Instance.TintFlame(b.Colour("colour", Color.white), time);
            if (flame != null)
            {
                if (b.Float("intensity", 1f) > 1f) flame.Spike();
                if (b.Has("t"))
                {
                    flame.ManualOverride = true;
                    float from = flame.Progress, to = b.Float("t", 0f);
                    for (float t = 0f; t < time && !skipping; t += Time.deltaTime) { flame.SetProgress(Mathf.Lerp(from, to, t / time)); yield return null; }
                    flame.SetProgress(to);
                    flame.ManualOverride = b.Bool("hold", false);
                    yield break;
                }
            }
            yield return Wait(time);
        }

        private static Ease ParseEase(Beat b)
        {
            return Enum.TryParse(b.Str("ease", "InOutQuad"), true, out Ease e) ? e : Ease.InOutQuad;
        }

        private IEnumerator DoMove(Beat b)
        {
            GameObject go = stage.Get(b.Str("id"));
            if (go == null) yield break;
            Vector3 target = stage.StagePoint(b.Vec2("to"));
            float time = Dur(b, "time", 1f);
            if (time <= 0f) { go.transform.position = target; yield break; }
            Tween.Move(go.transform, target, time, ParseEase(b));
            yield return Wait(time);
            if (go != null) go.transform.position = target;
        }

        private IEnumerator DoScale(Beat b)
        {
            GameObject go = stage.Get(b.Str("id"));
            if (go == null) yield break;
            float v = b.Float("value", 1f);
            float sign = go.transform.localScale.x < 0f ? -1f : 1f;
            var target = new Vector3(v * sign, v, 1f);
            float time = Dur(b, "time", 0.5f);
            if (time <= 0f) { go.transform.localScale = target; yield break; }
            Tween.Scale(go.transform, target, time, ParseEase(b));
            yield return Wait(time);
            if (go != null) go.transform.localScale = target;
        }

        private IEnumerator DoRotate(Beat b)
        {
            GameObject go = stage.Get(b.Str("id"));
            if (go == null) yield break;
            float time = Dur(b, "time", 0.5f);
            float v = b.Float("value", 0f);
            if (time <= 0f) { go.transform.localRotation = Quaternion.Euler(0f, 0f, v); yield break; }
            Tween.Rotate(go.transform, v, time, ParseEase(b));
            yield return Wait(time);
        }

        private IEnumerator DoTint(Beat b)
        {
            SpriteRenderer sr = stage.RendererOf(b.Str("id"));
            if (sr == null) yield break;
            Color target = b.Colour("value", Color.white);
            target.a = sr.color.a;
            float time = Dur(b, "time", 0.5f);
            if (time <= 0f) { sr.color = target; yield break; }
            Tween.Color(sr, target, time);
            yield return Wait(time);
        }

        private IEnumerator DoAlpha(Beat b)
        {
            SpriteRenderer sr = stage.RendererOf(b.Str("id"));
            if (sr == null) yield break;
            yield return CutsceneStage.FadeRenderer(sr, sr.color.a, b.Float("value", 1f), Dur(b, "time", 0.5f));
        }

        private IEnumerator DoText(Beat b)
        {
            List<string> lines = b.Strings("lines");
            if (lines.Count == 0 && b.Has("line")) lines.Add(b.Str("line"));
            string speaker = b.Str("speaker", "");
            string style = b.Str("style", "narration");
            // Optional voice acting (Docs/AUDIO_AND_VOICE.md): "voices" holds one clip id per line ("voice" for a single line).
            List<string> voices = b.Strings("voices");
            if (voices.Count == 0 && b.Has("voice")) voices.Add(b.Str("voice"));
            advanceQueued = false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (skipping) break;
                if (i < voices.Count) AudioManager.PlayVoice(voices[i]);
                yield return ui.ShowLine(Expand(speaker), Expand(lines[i]), style, ConsumeAdvance, IsSkipping);
                AudioManager.StopVoice();   // advancing early cuts the line off
            }
            if (!b.Bool("keep", false) || skipping) ui.HideCaption();
        }

        private IEnumerator DoParallel(Beat b)
        {
            List<Beat> children = b.Beats("beats");
            int running = 0;
            foreach (Beat child in children)
            {
                if (!PassesCycle(child)) continue;
                running++;
                StartCoroutine(RunAndCount(child, () => running--));
            }
            while (running > 0) yield return null;
        }

        private IEnumerator RunAndCount(Beat b, Action done)
        {
            yield return Execute(b);
            done();
        }

        private IEnumerator DoInvertWipe(Beat b)
        {
            float time = Dur(b, "time", 1.5f);
            ThemeManager themes = ThemeManager.Instance;
            string theme = b.Str("theme", ThemeManager.Phase2Inverted);
            ThemeConfig target = themes != null ? themes.Find(theme) : null;
            Vector2 origin = FlameController.Instance != null ? FlameController.Instance.WorldPosition : Vector2.zero;

            if (themes != null) themes.Apply(theme, time);   // world colours (and the flame orange → blue) follow the circle
            if (!skipping)
            {
                Sprite inside = target != null ? target.parchmentSprite : null;
                Color insideColor = target != null ? (inside != null ? target.parchmentSpriteTint : target.parchmentBase) : Color.black;
                ScreenShake.Shake(0.25f, time);
                yield return ui.InvertWipe(origin, inside, insideColor, time, IsSkipping);
            }
            if (themes != null) themes.Apply(theme, 0f);
            ui.HideInvertWipe();
            if (b.Has("flameT") && FlameController.Instance != null) FlameController.Instance.SetProgress(b.Float("flameT", 0f));
        }

        private void DoSetFlag(Beat b)
        {
            GameManager gm = GameManager.Ensure();
            string flag = b.Str("flag", "");
            switch (flag)
            {
                case "phase":
                    gm.SetPhase(b.Int("value", GameConstants.Phase2));
                    gm.Save.flameT = 0f;
                    gm.Save.lastBenchId = "";
                    gm.Save.lastBenchScene = "";
                    break;
                case "cycle":
                    gm.AdvanceCycle();
                    break;
                case "titleCard":
                    gm.PendingTitleCard = Expand(b.Str("value", ""));
                    break;
                case "ability":
                    GameEvents.RaiseAbilityUnlocked(b.Str("value", ""));
                    break;
                default:
                    gm.SetFlag(flag, b.Bool("value", true));
                    break;
            }
        }

        // ------------------------------------------------------------------

        private void Finish(string scene, float fade, string chainCutscene = null, string chainNext = null)
        {
            if (finished) return;
            finished = true;
            ui.SetSkipProgress(0f);
            if (CameraController.Main != null) CameraController.Main.ResetZoom(0f);
            GameManager gm = GameManager.Ensure();
            gm.ClearPendingCutscene(CurrentId);
            string next = string.IsNullOrEmpty(scene) ? fallbackNextScene : scene;
            gm.NextSceneAfterCutscene = next;
            gm.EndCutscene();

            if (!string.IsNullOrEmpty(chainCutscene))
            {
                // Another cutscene follows (e.g. the post-loop Intro): it becomes the pending story beat.
                string after = !string.IsNullOrEmpty(chainNext) ? chainNext : GameConstants.ScenePhase1;
                gm.QueueStoryCutscene(chainCutscene, after);
                next = GameConstants.SceneCutscene;
            }
            else gm.WriteSave();

            SceneFlow.Instance.FadeColor = Color.black;
            SceneFlow.Load(next, fade);
        }

        /// <summary>Text tokens: {cycle} → the current cycle number (Docs/08 §4 "Cycle 2" card).</summary>
        public static string Expand(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0) return text;
            int cycle = GameManager.HasInstance ? GameManager.Instance.Cycle : 1;
            return text.Replace("{cycle}", cycle.ToString());
        }

        /// <summary>zoom: value = multiple of the normal camera size (> 1 pulls back from the stage), time.</summary>
        private IEnumerator DoZoom(Beat b)
        {
            CameraController cam = CameraController.Main;
            float time = Dur(b, "time", 1f);
            if (cam != null)
            {
                Camera c = Camera.main;
                float baseSize = c != null && cam.Config != null ? cam.Config.orthographicSize : 5.5f;
                cam.SetZoom(baseSize * Mathf.Max(0.1f, b.Float("value", 1f)), time);
            }
            yield return Wait(time);
            if (skipping && cam != null && cam.Config != null) cam.SetZoom(cam.Config.orthographicSize * Mathf.Max(0.1f, b.Float("value", 1f)), 0f);
        }
    }
}
