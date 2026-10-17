using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>What the camera needs from whatever it follows (the player, a cutscene puppet…).</summary>
    public interface ICameraTarget
    {
        Vector2 Position { get; }
        /// <summary>+1 or −1; drives horizontal look-ahead.</summary>
        int FacingDirection { get; }
        /// <summary>+1 looking up, −1 looking down, 0 neutral. Only non-zero when the target is idle on the ground.</summary>
        float VerticalLook { get; }
        bool CameraTargetGrounded { get; }
    }
}
