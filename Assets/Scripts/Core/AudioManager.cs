using System.Collections.Generic;
using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Audio hooks (Docs/01 §2, Docs/02 §4). <see cref="Play"/> is a safe no-op when no clip is registered,
    /// so gameplay code can call it everywhere now and audio can be dropped in later via <see cref="Register"/>.
    /// </summary>
    public static class AudioManager
    {
        private static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private static readonly HashSet<string> requested = new HashSet<string>();
        private static AudioRunner runner;

        /// <summary>When true (editor only), each missing id is logged once to help build the hook list.</summary>
        public static bool LogMissingIds = false;

        public static float MasterVolume { get; private set; } = 1f;

        public static void Register(string id, AudioClip clip)
        {
            if (string.IsNullOrEmpty(id) || clip == null) return;
            clips[id] = clip;
        }

        public static bool Has(string id) => !string.IsNullOrEmpty(id) && clips.ContainsKey(id);

        /// <summary>Drop-in clips (Docs/AUDIO_HOOKS.md): Resources/Audio/&lt;id&gt; for sfx, Resources/Audio/Music/&lt;id&gt; for music.</summary>
        public const string SfxFolder = "Audio/", MusicFolder = "Audio/Music/";

        private static readonly HashSet<string> notInResources = new HashSet<string>();

        /// <summary>Registered clip, else a one-time Resources lookup (misses are remembered, so no repeated loads).</summary>
        private static AudioClip Find(string id, string folder)
        {
            if (clips.TryGetValue(id, out AudioClip clip) && clip != null) return clip;
            if (!Application.isPlaying || notInResources.Contains(id)) return null;
            clip = Resources.Load<AudioClip>(folder + id);
            if (clip != null) clips[id] = clip;
            else notInResources.Add(id);
            return clip;
        }

        /// <summary>Plays a one-shot SFX by id. No-op if nothing is registered under that id.</summary>
        public static void Play(string id, float volume = 1f)
        {
            if (string.IsNullOrEmpty(id)) return;
            AudioClip clip = Find(id, SfxFolder);
            if (clip == null)
            {
                NoteMissing(id);
                return;
            }
            if (!Application.isPlaying) return;
            Runner.PlayOneShot(clip, Mathf.Clamp01(volume) * MasterVolume);
        }

        /// <summary>Starts looping music by id (replaces current). No-op if missing.</summary>
        public static void PlayMusic(string id, float volume = 1f, bool loop = true)
        {
            if (string.IsNullOrEmpty(id)) return;
            AudioClip clip = Find(id, MusicFolder);
            if (clip == null)
            {
                NoteMissing(id);
                return;
            }
            if (!Application.isPlaying) return;
            Runner.PlayMusic(clip, Mathf.Clamp01(volume) * MasterVolume, loop);
        }

        public static void StopMusic()
        {
            if (runner != null) runner.StopMusic();
        }

        public static void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            if (runner != null) runner.ApplyMusicVolume(MasterVolume);
        }

        /// <summary>Every id the game asked for so far (registered or not) — useful for Docs/AUDIO_HOOKS.md.</summary>
        public static IReadOnlyCollection<string> RequestedIds => requested;

        private static void NoteMissing(string id)
        {
            if (requested.Add(id) && LogMissingIds)
                Debug.Log($"AudioManager: no clip registered for '{id}' (hook only).");
        }

        private static AudioRunner Runner
        {
            get
            {
                if (runner != null) return runner;
                var go = new GameObject("[AudioManager]");
                Object.DontDestroyOnLoad(go);
                runner = go.AddComponent<AudioRunner>();
                return runner;
            }
        }

        private class AudioRunner : MonoBehaviour
        {
            private AudioSource sfx;
            private AudioSource music;
            private float musicBaseVolume = 1f;

            private void Awake()
            {
                sfx = gameObject.AddComponent<AudioSource>();
                sfx.playOnAwake = false;
                music = gameObject.AddComponent<AudioSource>();
                music.playOnAwake = false;
                music.loop = true;
            }

            public void PlayOneShot(AudioClip clip, float volume) => sfx.PlayOneShot(clip, volume);

            public void PlayMusic(AudioClip clip, float volume, bool loop)
            {
                musicBaseVolume = volume;
                music.clip = clip;
                music.loop = loop;
                music.volume = volume;
                music.Play();
            }

            public void StopMusic() => music.Stop();

            public void ApplyMusicVolume(float master) => music.volume = musicBaseVolume * master;

            private void OnDestroy()
            {
                if (runner == this) runner = null;
            }
        }
    }
}
