using UnityEngine;
using UnityEngine.InputSystem;

namespace Shatranj.Core
{
    /// <summary>
    /// Gamepad rumble hook (Prompt 17). <see cref="Pulse"/> sets the current gamepad's motors for a real-time duration;
    /// overlapping pulses keep the stronger motors and the longer remaining time. Motors stop on pause, on focus loss
    /// and when the runner is destroyed, so a controller is never left buzzing. No gamepad → no-op.
    /// </summary>
    public static class Rumble
    {
        public static bool Enabled = true;
        public static float Strength = 1f;

        private static RumbleRunner runner;

        public static void Pulse(float low, float high, float duration)
        {
            if (!Enabled || duration <= 0f || (low <= 0f && high <= 0f) || !Application.isPlaying) return;
            if (Gamepad.current == null) return;
            if (GameManager.HasInstance && GameManager.Instance.IsPaused) return;
            Runner.Request(Mathf.Clamp01(low * Strength), Mathf.Clamp01(high * Strength), duration);
        }

        public static void Stop()
        {
            if (runner != null) runner.StopMotors();
        }

        private static RumbleRunner Runner
        {
            get
            {
                if (runner != null) return runner;
                var go = new GameObject("[Rumble]");
                Object.DontDestroyOnLoad(go);
                runner = go.AddComponent<RumbleRunner>();
                return runner;
            }
        }

        private class RumbleRunner : MonoBehaviour
        {
            private float low, high, remaining;
            private Gamepad pad;

            public void Request(float l, float h, float duration)
            {
                low = remaining > 0f ? Mathf.Max(low, l) : l;
                high = remaining > 0f ? Mathf.Max(high, h) : h;
                remaining = Mathf.Max(remaining, duration);
                pad = Gamepad.current;
                if (pad != null) pad.SetMotorSpeeds(low, high);
            }

            private void Update()
            {
                if (remaining <= 0f) return;
                if (GameManager.HasInstance && GameManager.Instance.IsPaused) { StopMotors(); return; }
                remaining -= Time.unscaledDeltaTime;
                if (remaining <= 0f) StopMotors();
            }

            public void StopMotors()
            {
                remaining = 0f;
                low = high = 0f;
                if (pad != null) pad.SetMotorSpeeds(0f, 0f);
                else if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(0f, 0f);
            }

            private void OnApplicationFocus(bool focus) { if (!focus) StopMotors(); }
            private void OnDisable() => StopMotors();
        }
    }
}
