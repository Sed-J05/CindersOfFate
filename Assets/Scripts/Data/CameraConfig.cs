using UnityEngine;

namespace Shatranj.Data
{
    /// <summary>Camera numbers from Docs/02 §7 and Docs/01 §12b.</summary>
    [CreateAssetMenu(menuName = "Shatranj/Configs/Camera Config", fileName = "CameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        [Header("Framing")]
        public float orthographicSize = 5.5f;
        [Tooltip("World-unit offset above the player's feet that the camera centres on.")]
        public float verticalOffset = 1.2f;

        [Header("Follow")]
        public float followSmoothTime = 0.15f;
        public float maxFollowSpeed = 60f;

        [Header("Look-ahead")]
        public float lookAheadDistance = 1.5f;
        public float lookAheadSmoothTime = 0.35f;

        [Header("Look up / down (hold W or S)")]
        public float verticalLookHoldTime = 0.5f;
        public float verticalLookDistance = 3f;
        public float verticalLookSmoothTime = 0.3f;

        [Header("Rooms")]
        public float roomBlendTime = 0.4f;

        [Header("Zoom")]
        public float defaultZoomTime = 0.5f;

        [Header("Shake")]
        public float shakeMultiplier = 1f;
        public float shakeFrequency = 25f;
    }
}
