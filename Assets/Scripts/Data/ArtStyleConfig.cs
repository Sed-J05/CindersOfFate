using UnityEngine;

namespace Shatranj.Data
{
    /// <summary>Art-pipeline switches (Docs/06 §1, Docs/07 §5-6). One asset for the whole project.</summary>
    [CreateAssetMenu(menuName = "Shatranj/Configs/Art Style Config", fileName = "ArtStyleConfig")]
    public class ArtStyleConfig : ScriptableObject
    {
        [Header("Import (Docs/06 §1)")]
        public int pixelsPerUnit = 100;
        [Tooltip("Pixel-art teams set true (Point filter); hand-drawn stays Bilinear.")]
        public bool usePointFilter = false;
        public bool generateMipMaps = false;

        [Header("Puppet feel (Docs/07 §5-6)")]
        public bool dropShadows = true;
        public bool shadowBlurCopy = true;
        public bool puppetJitter = true;
        public float jitterFrequency = 8f;
        public float jitterAmplitude = 0.01f;
        public bool puppetRods = false;
    }
}
