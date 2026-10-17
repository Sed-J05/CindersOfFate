using System;
using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>One shake request: amplitude in world units, duration in seconds, noise frequency in Hz.</summary>
    public readonly struct ShakeRequest
    {
        public readonly float amplitude;
        public readonly float duration;
        public readonly float frequency;

        public ShakeRequest(float amplitude, float duration, float frequency)
        {
            this.amplitude = amplitude;
            this.duration = duration;
            this.frequency = frequency;
        }
    }

    /// <summary>
    /// Data hook for camera shake (Docs/02 §4). Gameplay calls <see cref="Shake"/>; the camera (Prompt 2)
    /// subscribes to <see cref="OnShakeRequested"/> and feeds a <see cref="ShakeAccumulator"/>.
    /// </summary>
    public static class ScreenShake
    {
        public const float DefaultFrequency = 25f;

        public static event Action<ShakeRequest> OnShakeRequested;

        public static void Shake(float amplitude, float duration, float frequency = DefaultFrequency)
        {
            if (amplitude <= 0f || duration <= 0f) return;
            OnShakeRequested?.Invoke(new ShakeRequest(amplitude, duration, frequency));
        }

        /// <summary>Tests / resets.</summary>
        public static void ClearSubscribers() => OnShakeRequested = null;
    }

    /// <summary>
    /// Sums active shakes into a camera offset. Fixed slot array, no allocations.
    /// Each shake decays linearly and samples Perlin noise so motion is smooth, not jittery.
    /// </summary>
    public class ShakeAccumulator
    {
        private struct Slot
        {
            public bool active;
            public float amplitude;
            public float duration;
            public float remaining;
            public float frequency;
            public float seedX;
            public float seedY;
        }

        private const int MaxSlots = 8;
        private readonly Slot[] slots = new Slot[MaxSlots];
        private float time;

        public bool IsShaking
        {
            get
            {
                for (int i = 0; i < MaxSlots; i++) if (slots[i].active) return true;
                return false;
            }
        }

        public void Add(ShakeRequest request)
        {
            int index = -1;
            float weakest = float.MaxValue;
            for (int i = 0; i < MaxSlots; i++)
            {
                if (!slots[i].active) { index = i; break; }
                float strength = slots[i].amplitude * (slots[i].remaining / slots[i].duration);
                if (strength < weakest) { weakest = strength; index = i; }
            }

            ref Slot s = ref slots[index];
            s.active = true;
            s.amplitude = request.amplitude;
            s.duration = Mathf.Max(0.0001f, request.duration);
            s.remaining = s.duration;
            s.frequency = request.frequency;
            s.seedX = UnityEngine.Random.value * 100f;
            s.seedY = UnityEngine.Random.value * 100f + 50f;
        }

        /// <summary>Advances by <paramref name="dt"/> and returns this frame's offset.</summary>
        public Vector2 Evaluate(float dt)
        {
            time += dt;
            Vector2 offset = Vector2.zero;
            for (int i = 0; i < MaxSlots; i++)
            {
                ref Slot s = ref slots[i];
                if (!s.active) continue;
                s.remaining -= dt;
                if (s.remaining <= 0f) { s.active = false; continue; }

                float falloff = s.remaining / s.duration;
                float t = time * s.frequency;
                float nx = Mathf.PerlinNoise(s.seedX, t) * 2f - 1f;
                float ny = Mathf.PerlinNoise(s.seedY, t) * 2f - 1f;
                offset += new Vector2(nx, ny) * (s.amplitude * falloff);
            }
            return offset;
        }

        public void Clear()
        {
            for (int i = 0; i < MaxSlots; i++) slots[i].active = false;
        }
    }
}
