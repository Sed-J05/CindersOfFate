using UnityEngine;

namespace Shatranj.Data
{
    public enum EnemyColliderShape { Box, Circle }
    public enum EnemyLocomotion { Ground, Flyer }

    /// <summary>
    /// One field set that covers every regular enemy in Docs/04 §A and §C. Fields a type doesn't use are
    /// left at 0/false and ignored by that enemy's AI. Design sizes are world units at scale 1.
    /// </summary>
    [CreateAssetMenu(menuName = "Shatranj/Configs/Enemy Config", fileName = "EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Identity")]
        public string id = "crawler";
        public string displayName = "Shade Crawler";
        [Tooltip("Level glyph from Docs/05 legend (c l f k b h t).")]
        public string glyph = "c";
        [Range(1, 2)] public int phase = 1;
        public EnemyLocomotion locomotion = EnemyLocomotion.Ground;

        [Header("Size (Docs/04 'Body / collider', Docs/06 canvas)")]
        public Vector2 bodySize = new Vector2(0.8f, 0.7f);
        public EnemyColliderShape colliderShape = EnemyColliderShape.Box;
        public Vector2 colliderSize = new Vector2(0.7f, 0.6f);
        public float colliderRadius = 0f;
        public Vector2Int canvasSize = new Vector2Int(128, 128);
        public bool pivotAtFeet = true;

        [Header("Stats")]
        public int maxHp = 2;
        public int contactDamage = 1;
        public int attackDamage = 1;
        public float hitFlashDuration = 0.08f;
        public float knockbackSpeed = 3f;
        public float knockbackDuration = 0.15f;
        public bool knockbackImmuneDuringAttack = false;
        public float deathAnimTime = 0.5f;
        [Tooltip("Hurt clip override length after a hit (2 frames @ 12 fps).")]
        public float hurtAnimTime = 0.17f;
        [Tooltip("Gravity scale for ground enemies (Physics2D gravity is −9.81).")]
        public float gravityScale = 3f;
        [Tooltip("How far below the front foot the ledge probe looks for ground.")]
        public float ledgeProbeDistance = 0.6f;

        [Header("Telegraph (Docs/04 global rules)")]
        [Tooltip("Flash shown when an attack telegraph starts.")]
        public float telegraphFlashTime = 0.1f;
        public Color telegraphFlashColor = new Color(1f, 0.35f, 0.3f, 1f);

        [Header("Perception")]
        public float detectRange = 6f;
        [Tooltip("Vertical window for 'same platform' checks (Crawler ±1.5 u).")]
        public float verticalTolerance = 1.5f;
        public bool requiresLineOfSight = false;
        [Tooltip("Phase 2: seconds without line-of-sight before losing the player (0 = never).")]
        public float loseTrackTime = 0f;
        [Tooltip("Keeps chasing this long after the player leaves its level (so a jump doesn't end the chase).")]
        public float trackMemoryTime = 0.6f;

        [Header("Movement")]
        public float patrolSpeed = 2.5f;
        public float chaseSpeed = 3.5f;
        public bool turnsAtLedges = true;
        [Tooltip("Phase 2 spacing: distance the enemy tries to hold before attacking (Knight 2.0).")]
        public float preferredDistance = 0f;
        [Tooltip("Thrower band: retreat if closer than this.")]
        public float retreatIfCloserThan = 0f;
        [Tooltip("Thrower band: advance if farther than this.")]
        public float advanceIfFartherThan = 0f;
        public float retreatDistance = 0f;
        public float retreatTime = 0f;
        [Range(0f, 1f)] public float feintChance = 0f;
        [Tooltip("Won't attack while the player is airborne above this height (0 = ignore).")]
        public float waitIfPlayerAboveHeight = 0f;
        public float separationRadius = 1.0f;
        public float hopDodgeDistance = 0f;

        [Header("Attack (Telegraph → Active → Recovery)")]
        public string attackName = "";
        public float telegraphTime = 0f;
        public float attackActiveTime = 0f;
        public float attackRecoveryTime = 0f;
        public float attackCooldown = 0f;
        [Tooltip("Lunge / dive / charge speed.")]
        public float attackSpeed = 0f;
        [Tooltip("Max distance an attack travels (Kite dive 6 u, Charger 10 u).")]
        public float attackMaxDistance = 0f;
        [Tooltip("Melee reach for sword swings.")]
        public float attackReach = 0f;
        public int comboHits = 1;
        public float comboGap = 0f;
        [Tooltip("Periodic attacks: seconds between attempts (Kite 3, Bat 2.5, Thrower 2.2).")]
        public float attackInterval = 0f;
        [Tooltip("Per-enemy timer offset so groups don't attack together (Bat +0.6 s each).")]
        public float desyncOffset = 0f;
        public bool showExclamation = false;

        [Header("Flyer")]
        public float hoverAmplitude = 0f;
        public float hoverFrequency = 0f;
        public float hoverHeightAbovePlayer = 0f;
        public float returnSpeed = 0f;
        public float wanderSpeed = 0f;
        public float circleRadiusMin = 0f;
        public float circleRadiusMax = 0f;
        public bool startsAsleep = false;
        public float wakeTime = 0f;

        [Header("Charger")]
        public float wallStunTime = 0f;

        [Header("Projectile")]
        public float projectileSpeed = 0f;
        public Vector2 projectileSize = Vector2.zero;
        public Vector2Int projectileCanvas = Vector2Int.zero;
        public float projectileLifetime = 0f;
        [Tooltip("First-order lead: aim at player pos + vel × this.")]
        public float projectileLeadTime = 0f;
        public bool projectileArcs = false;
        public int projectileDamage = 1;

        [Header("Flame scaling (Docs/07 §4)")]
        public bool scalesWithFlame = false;
        public bool scalePivotCenter = false;
        [Tooltip("Projectile size × (1 + this × (scale − 1)).")]
        public float projectileScaleFactor = 0.5f;

        /// <summary>Hits needed to kill with a given damage per hit (player nail = 1, fireball = 3).</summary>
        public int HitsToKill(int damagePerHit) => damagePerHit <= 0 ? int.MaxValue : Mathf.CeilToInt(maxHp / (float)damagePerHit);

        public bool HasAttack => !string.IsNullOrEmpty(attackName);
    }
}
