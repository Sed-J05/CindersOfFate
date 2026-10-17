using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shatranj.Data
{
    /// <summary>One named clip: ordered sprite frames + timing.</summary>
    [Serializable]
    public class SpriteClip
    {
        public string name = "Idle";
        public Sprite[] frames = new Sprite[0];
        public float fps = 10f;
        public bool loop = false;
        public bool holdLast = true;

        public int FrameCount => frames != null ? frames.Length : 0;
        public float Duration => fps > 0f && FrameCount > 0 ? FrameCount / fps : 0f;
        public bool IsValid => FrameCount > 0 && frames[0] != null;
    }

    /// <summary>
    /// All clips of one character / FX / prop, built by Shatranj/Import &amp; Rebuild Animations from the
    /// PNG folders and the manifest. Lives in Assets/Resources/Animations so it can be found at runtime by name.
    /// </summary>
    [CreateAssetMenu(menuName = "Shatranj/Sprite Animation Set", fileName = "SpriteAnimationSet")]
    public class SpriteAnimationSet : ScriptableObject
    {
        public string setName = "";
        public List<SpriteClip> clips = new List<SpriteClip>();

        public const string ResourcesFolder = "Animations";

        public SpriteClip Find(string clipName)
        {
            for (int i = 0; i < clips.Count; i++)
                if (clips[i].name == clipName) return clips[i];
            return null;
        }

        public bool Has(string clipName)
        {
            SpriteClip c = Find(clipName);
            return c != null && c.IsValid;
        }

        public bool HasAnyFrames
        {
            get
            {
                for (int i = 0; i < clips.Count; i++) if (clips[i].IsValid) return true;
                return false;
            }
        }

        /// <summary>Runtime lookup: Resources/Animations/{setName}. Null when the set was never built.</summary>
        public static SpriteAnimationSet Load(string setName)
        {
            return Resources.Load<SpriteAnimationSet>(ResourcesFolder + "/" + setName);
        }
    }
}
