using UnityEngine;

namespace Shatranj.Data
{
    /// <summary>
    /// Every protagonist number from Docs/03. Defaults ARE the doc values; the asset is the tuning surface.
    /// Config assets expose public fields on purpose (see DECISIONS D-017).
    /// </summary>
    [CreateAssetMenu(menuName = "Shatranj/Configs/Player Config", fileName = "PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Body (Docs/03 §1)")]
        public Vector2 colliderSize = new Vector2(0.7f, 1.5f);
        public Vector2 colliderOffset = new Vector2(0f, 0.75f);
        public Vector2Int canvasSize = new Vector2Int(256, 256);

        [Header("Run (Docs/03 §3)")]
        public float runSpeed = 7.5f;
        public float accelTime = 0.05f;
        public float decelTime = 0.04f;
        [Range(0f, 1f)] public float airControl = 0.9f;

        [Header("Jump (Docs/03 §3)")]
        [Tooltip("Apex height with a full hold, world units.")]
        public float jumpApexHeight = 3.2f;
        [Tooltip("Time to reach the apex, seconds.")]
        public float jumpApexTime = 0.40f;
        [Tooltip("vy *= this when jump is released while rising.")]
        public float jumpReleaseMultiplier = 0.4f;
        public float fallGravityMultiplier = 1.5f;
        public float maxFallSpeed = 22f;
        public float coyoteTime = 0.10f;
        public float jumpBufferTime = 0.12f;
        public int airJumps = 0;
        public float landSquashTime = 0.08f;

        [Header("Physics checks (Docs/02 §6)")]
        [Tooltip("BoxCast distance below the feet that counts as grounded.")]
        public float groundCheckDistance = 0.06f;
        [Tooltip("How much narrower than the collider the ground check box is (keeps edges honest).")]
        public float groundCheckInset = 0.06f;
        [Tooltip("Sideways cast distance for wall contact (wall-stick prevention).")]
        public float wallCheckDistance = 0.05f;
        [Tooltip("Upward cast distance for head bumps.")]
        public float ceilingCheckDistance = 0.05f;
        [Tooltip("Small downward speed kept while grounded so the body stays in contact.")]
        public float groundStickSpeed = 1f;
        [Tooltip("Analog stick / key threshold for digital movement.")]
        [Range(0.1f, 0.9f)] public float moveDeadzone = 0.3f;

        [Header("Attack (Docs/03 §4)")]
        public int attackDamage = 1;
        public float attackCooldown = 0.40f;
        public float attackBufferTime = 0.15f;
        public Vector2 sideHitboxSize = new Vector2(1.6f, 1.2f);
        public Vector2 sideHitboxOffset = new Vector2(1.1f, 0.95f);
        public Vector2 upHitboxSize = new Vector2(1.4f, 1.6f);
        public Vector2 upHitboxOffset = new Vector2(0f, 2.0f);
        [Tooltip("Seconds after the press before the hitbox turns on.")]
        public float hitboxStartDelay = 0.06f;
        public float hitboxActiveTime = 0.08f;
        public float recoilSpeed = 2f;
        public float recoilTime = 0.08f;
        public float hitStopOnHit = 0.05f;
        public float hitStopOnBossHit = 0.10f;
        public float slashFxLifetime = 0.12f;

        [Header("Soul & heal (Docs/03 §5)")]
        public int soulMax = 99;
        public int soulPerHit = 11;
        public float soulOrbTravelTime = 0.25f;
        public int healCost = 33;
        public float healHoldTime = 0.9f;
        public int healAmount = 1;

        [Header("Spell / fireball (Docs/03 §2, §5)")]
        [Tooltip("K released before this → fireball; still held → heal.")]
        public float spellTapWindow = 0.18f;
        public int fireballCost = 33;
        public float fireballCastTime = 0.25f;
        public Vector2 fireballSize = new Vector2(1.2f, 0.8f);
        public float fireballSpeed = 14f;
        public float fireballLifetime = 1.2f;
        public int fireballDamage = 3;
        public bool fireballPierces = true;
        public Vector2 fireballSpawnOffset = new Vector2(0.8f, 1.0f);

        [Header("Dash / Time Slip (Docs/03 §6)")]
        public float dashSpeed = 20f;
        public float dashDuration = 0.18f;
        public float dashCooldown = 0.5f;
        public int airDashesPerAirtime = 1;
        public int dashGhostCount = 4;
        public float dashGhostFadeTime = 0.3f;

        [Header("Health (Docs/03 §7)")]
        public int heartsPhase1 = 5;
        public int heartsPhase2 = 7;
        public float hurtKnockbackX = 6f;
        public float hurtKnockbackY = 5f;
        public float hurtKnockbackTime = 0.2f;
        public float hurtStunTime = 0.25f;
        public float hurtInvulnerabilityTime = 1.2f;
        public float hurtBlinkHz = 10f;
        public float hurtHitStop = 0.12f;
        public float hurtShakeAmplitude = 0.3f;
        public float hurtShakeDuration = 0.2f;
        [Tooltip("Seconds standing on safe ground before the position is stored as the spike respawn point.")]
        public float safeGroundRecordTime = 0.5f;
        public float deathSlowmoDuration = 0.6f;
        [Range(0.05f, 1f)] public float deathSlowmoScale = 0.3f;
        public float deathFadeTime = 0.6f;

        [Header("Bench (Docs/03 §10)")]
        public float benchRestTime = 0.8f;

        // --- Derived (Docs/03 §3): v = 2h/t, g = 2h/t² ---
        public float JumpVelocity => 2f * jumpApexHeight / Mathf.Max(0.0001f, jumpApexTime);          // ≈ 16
        public float RiseGravity => 2f * jumpApexHeight / Mathf.Max(0.0001f, jumpApexTime * jumpApexTime); // ≈ 40
        public float FallGravity => RiseGravity * fallGravityMultiplier;                                // ≈ 60
        public float AirTime => 2f * jumpApexTime;                                                      // ≈ 0.8 (full jump)
    }
}
