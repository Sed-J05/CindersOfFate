using System;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Plays <see cref="SpriteClip"/>s from a <see cref="SpriteAnimationSet"/> straight into a SpriteRenderer.
    /// No Animator, no transitions: gameplay calls <see cref="Play"/>. Uses scaled time, so hit-stop freezes it.
    /// Non-looping clips hold their last frame and raise <see cref="OnClipFinished"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteAnimationSet set;
        [SerializeField] private string defaultClip = "Idle";
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private float speed = 1f;

        private SpriteRenderer sr;
        private SpriteClip current;
        private int frame;
        private float timer;
        private bool playing;
        private bool finished;

        /// <summary>Clip name that just ended (non-looping clips only).</summary>
        public event Action<string> OnClipFinished;
        /// <summary>(clip name, frame index) each time the frame changes.</summary>
        public event Action<string, int> OnFrameChanged;

        public SpriteAnimationSet Set
        {
            get => set;
            set { this.set = value; current = null; }
        }

        public string CurrentClip => current != null ? current.name : null;
        public int Frame => frame;
        public bool IsPlaying => playing;
        public bool IsFinished => finished;
        public float Speed { get => speed; set => speed = Mathf.Max(0f, value); }
        public SpriteRenderer Renderer => sr != null ? sr : (sr = GetComponent<SpriteRenderer>());

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (playOnEnable && current == null && set != null && !string.IsNullOrEmpty(defaultClip))
                Play(defaultClip);
        }

        public bool HasClip(string clipName) => set != null && set.Has(clipName);

        /// <summary>
        /// Starts a clip. Returns false if the set has no valid clip of that name. Re-requesting the clip that is
        /// already playing is a no-op unless <paramref name="restart"/> is true.
        /// </summary>
        public bool Play(string clipName, bool restart = false)
        {
            if (set == null) return false;
            SpriteClip clip = set.Find(clipName);
            if (clip == null || !clip.IsValid) return false;

            if (current == clip && !restart && (playing || clip.loop)) return true;

            current = clip;
            frame = 0;
            timer = 0f;
            finished = false;
            playing = clip.FrameCount > 1 && clip.fps > 0f;
            Apply();
            return true;
        }

        /// <summary>Shows a single frame of a clip without playing it.</summary>
        public bool ShowFrame(string clipName, int frameIndex)
        {
            if (set == null) return false;
            SpriteClip clip = set.Find(clipName);
            if (clip == null || !clip.IsValid) return false;
            current = clip;
            frame = Mathf.Clamp(frameIndex, 0, clip.FrameCount - 1);
            playing = false;
            finished = false;
            Apply();
            return true;
        }

        public void Stop()
        {
            playing = false;
        }

        private void Update()
        {
            if (!playing || current == null) return;

            float frameDuration = 1f / current.fps;
            timer += Time.deltaTime * speed;

            while (timer >= frameDuration)
            {
                timer -= frameDuration;
                frame++;
                if (frame >= current.FrameCount)
                {
                    if (current.loop)
                    {
                        frame = 0;
                    }
                    else
                    {
                        frame = current.FrameCount - 1;
                        playing = false;
                        finished = true;
                        Apply();
                        OnClipFinished?.Invoke(current.name);
                        return;
                    }
                }
                Apply();
            }
        }

        private void Apply()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            Sprite s = current.frames[frame];
            if (s != null) sr.sprite = s;
            OnFrameChanged?.Invoke(current.name, frame);
        }
    }
}
