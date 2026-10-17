using System;
using UnityEngine;

namespace Shatranj.Data
{
    /// <summary>One boss move: Telegraph → Active → Recovery plus reach/speed/damage/weight.</summary>
    [Serializable]
    public class BossMoveConfig
    {
        public string name = "Slash";
        public float telegraph = 0.4f;
        public float active = 0.1f;
        public float recovery = 0.6f;
        public float reach = 2f;
        public float speed = 0f;
        public float duration = 0f;
        public int damage = 1;
        public float cooldown = 0f;
        [Tooltip("Weighted-random selection weight.")]
        public float weight = 1f;
        [Tooltip("First stage (1-based) where this move is available.")]
        public int minStage = 1;
        public int hits = 1;
        public float hitGap = 0f;
        [Tooltip("Distance band bitmask: 0 = any, 1 = close, 2 = mid, 4 = far (Colossus selection).")]
        public int distanceBand = 0;
        [Tooltip("Move-specific extra value (jump apex, shockwave radius, vine count…). See the move's comment.")]
        public float extra = 0f;

        public BossMoveConfig() { }

        public BossMoveConfig(string name, float telegraph, float active, float recovery, float reach, int damage, float weight = 1f)
        {
            this.name = name;
            this.telegraph = telegraph;
            this.active = active;
            this.recovery = recovery;
            this.reach = reach;
            this.damage = damage;
            this.weight = weight;
        }
    }

    /// <summary>
    /// Every boss number from Docs/04 §B, §D, §E in one field set. Move lists are data so telegraph
    /// durations and reaches are tunable without code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Shatranj/Configs/Boss Config", fileName = "BossConfig")]
    public class BossConfig : ScriptableObject
    {
        [Header("Identity (Docs/08 §7 names)")]
        public string id = "kage_warden";
        public string displayName = "Kage Warden";
        public string epithet = "Keeper of the Gate";
        [Range(1, 2)] public int phase = 1;

        [Header("Size")]
        public Vector2 bodySize = new Vector2(1.6f, 2.4f);
        public Vector2 colliderSize = new Vector2(1.2f, 2.2f);
        public Vector2Int canvasSize = new Vector2Int(384, 384);

        [Header("Stats")]
        public int maxHp = 20;
        public int contactDamage = 1;
        [Tooltip("Default damage of attacks when a move doesn't override it (Colossus = 2).")]
        public int attackDamage = 1;
        public float moveSpeed = 3f;
        public bool neverStaggers = true;
        public float hitFlashDuration = 0.08f;
        public float deathSlowmoTime = 1.2f;
        [Range(0.05f, 1f)] public float deathSlowmoScale = 0.25f;
        public float introDuration = 1.5f;

        [Header("Stages (fractions of HP where a new stage begins, descending)")]
        public float[] stageThresholds = new float[0];
        public float stageTransitionInvulnerability = 1.5f;

        [Header("Enrage")]
        [Tooltip("HP fraction below which enrage applies (0 = none).")]
        public float enrageBelowFraction = 0f;
        public float enrageTelegraphReduction = 0f;
        public float enrageSpeedMultiplier = 1f;
        public float enrageCooldownMultiplier = 1f;

        [Header("Selection")]
        public bool noImmediateRepeat = true;
        [Tooltip("Max times the same move may be chosen in a row (Colossus 2 → never 3×).")]
        public int maxSameMoveInRow = 1;
        [Tooltip("Minimum gap between the end of one attack and the start of the next.")]
        public float fairnessGap = 0.6f;
        public float advanceTimeMin = 0.8f;
        public float advanceTimeMax = 1.4f;
        public float backstepDistance = 3f;
        public float backstepTime = 0.35f;
        [Tooltip("Distance-based selection (Colossus): player closer than this = close band.")]
        public float closeDistance = 3.5f;
        [Tooltip("Distance-based selection (Colossus): player farther than this = far band; between = mid.")]
        public float farDistance = 7f;
        [Range(0f, 1f), Tooltip("Colossus: chance to walk toward a mid-range player (advanceTimeMin–Max) before choosing an attack. Never twice in a row.")]
        public float walkChance = 0.35f;

        [Header("Telegraph (Docs/04 global rules)")]
        public float telegraphFlashTime = 0.1f;
        public Color telegraphFlashColor = new Color(1f, 0.35f, 0.3f, 1f);
        [Tooltip("Gravity scale of the boss body (Physics2D gravity is −9.81).")]
        public float gravityScale = 3f;

        [Header("Block")]
        public float blockDuration = 0f;
        public float blockPushback = 2f;
        [Tooltip("How long the 2 u/s pushback lasts when the player hits a raised guard.")]
        public float blockPushbackTime = 0.15f;
        public int blockMinHitsToTrigger = 2;
        public float blockHitWindow = 2f;
        [Range(0f, 1f)] public float blockRandomChance = 0.2f;
        public float counterSlashRange = 2.5f;

        [Header("Summons")]
        public int summonCount = 0;
        public int summonCountEnraged = 0;
        public int summonCap = 0;
        public float summonCooldown = 0f;
        public float summonCooldownEnraged = 0f;
        public string[] summonEnemyIds = new string[0];

        [Header("Projectile")]
        public float projectileSpeed = 0f;
        public Vector2 projectileSize = Vector2.zero;
        public Vector2Int projectileCanvas = Vector2Int.zero;
        public float projectileLifetime = 0f;
        [Tooltip("Below this HP fraction fire a second projectile after projectileSecondDelay (0 = never).")]
        public float projectileSecondBelowFraction = 0f;
        public float projectileSecondDelay = 0f;

        [Header("Transform (Zulmat)")]
        public float shrinkScale = 1f;
        public float giantScale = 1f;
        public float transformTime = 0f;
        public int maxLavaZones = 0;
        public float lavaDuration = 0f;

        [Header("Arena (tiles)")]
        public int arenaWidthTiles = 40;
        public int arenaHeightTiles = 22;
        public bool flameParksDuringFight = false;

        [Header("Moves")]
        public BossMoveConfig[] moves = new BossMoveConfig[0];

        /// <summary>Number of stages = thresholds + 1.</summary>
        public int StageCount => stageThresholds.Length + 1;

        /// <summary>1-based stage for a given HP fraction.</summary>
        public int StageForFraction(float fraction)
        {
            int stage = 1;
            for (int i = 0; i < stageThresholds.Length; i++)
                if (fraction <= stageThresholds[i]) stage = i + 2;
            return stage;
        }

        public BossMoveConfig FindMove(string moveName)
        {
            for (int i = 0; i < moves.Length; i++)
                if (moves[i] != null && moves[i].name == moveName) return moves[i];
            return null;
        }
    }
}
