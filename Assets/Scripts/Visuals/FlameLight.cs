using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Shatranj.Visuals
{
    /// <summary>
    /// The flame's Point Light 2D (Docs/07 §3): follows the flame's world position, radius ≈ 1.6× screen width,
    /// falloff 0.6, intensity = theme intensity × flame flicker. Colour is set by ThemeManager. No shadows (perf, §8).
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class FlameLight : MonoBehaviour
    {
        [SerializeField] private FlameController flame;
        [SerializeField] private float innerRadiusFraction = 0.15f;

        private Light2D light2D;
        private float baseIntensity = 1.2f;
        private float radiusScreenMultiplier = 1.6f;

        public Light2D Light => light2D;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
            light2D.lightType = Light2D.LightType.Point;
            light2D.shadowsEnabled = false;
            if (flame == null) flame = FlameController.Instance;
        }

        /// <summary>ThemeManager pushes the current look here.</summary>
        public void ApplyTheme(Color color, float intensity, float radiusMultiplier, float falloff)
        {
            if (light2D == null) light2D = GetComponent<Light2D>();
            light2D.color = color;
            baseIntensity = intensity;
            radiusScreenMultiplier = radiusMultiplier;
            light2D.falloffIntensity = falloff;
        }

        private void LateUpdate()
        {
            if (flame == null) { flame = FlameController.Instance; if (flame == null) return; }
            transform.position = new Vector3(flame.WorldPosition.x, flame.WorldPosition.y, 0f);

            Camera cam = flame.Camera;
            float screenWidth = cam != null ? cam.orthographicSize * 2f * cam.aspect : 19.6f;
            float outer = screenWidth * radiusScreenMultiplier;
            light2D.pointLightOuterRadius = outer;
            light2D.pointLightInnerRadius = outer * innerRadiusFraction;
            light2D.intensity = baseIntensity * flame.Intensity;
        }
    }
}
