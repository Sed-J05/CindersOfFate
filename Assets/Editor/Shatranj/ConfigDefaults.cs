using Shatranj.Data;
using UnityEngine;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Doc defaults for every config asset (Docs/04, Docs/07). PlayerConfig and ArtStyleConfig carry their
    /// defaults as field initializers; enemies, bosses, themes and flames differ per asset, so they live here.
    /// </summary>
    public static class ConfigDefaults
    {
        // ------------------------------------------------------------------
        // Phase 1 enemies (Docs/04 §A) — red/black, fixed size
        // ------------------------------------------------------------------

        public static void Crawler(EnemyConfig c)
        {
            c.id = "crawler"; c.displayName = "Shade Crawler"; c.glyph = "c"; c.phase = 1;
            c.locomotion = EnemyLocomotion.Ground;
            c.bodySize = new Vector2(0.8f, 0.7f); c.colliderShape = EnemyColliderShape.Box; c.colliderSize = new Vector2(0.7f, 0.6f);
            c.canvasSize = new Vector2Int(128, 128); c.pivotAtFeet = true;
            c.maxHp = 2; c.contactDamage = 1; c.attackDamage = 1;
            c.knockbackSpeed = 3f; c.knockbackDuration = 0.15f;
            c.detectRange = 6f; c.verticalTolerance = 1.5f; c.requiresLineOfSight = false;
            c.patrolSpeed = 2.5f; c.chaseSpeed = 3.5f; c.turnsAtLedges = true;
            c.attackName = "";
        }

        public static void Lunger(EnemyConfig c)
        {
            c.id = "lunger"; c.displayName = "Ninja Lunger"; c.glyph = "l"; c.phase = 1;
            c.locomotion = EnemyLocomotion.Ground;
            c.bodySize = new Vector2(0.9f, 1.0f); c.colliderShape = EnemyColliderShape.Box; c.colliderSize = new Vector2(0.8f, 0.9f);
            c.canvasSize = new Vector2Int(160, 160); c.pivotAtFeet = true;
            c.maxHp = 3; c.contactDamage = 1; c.attackDamage = 1;
            c.knockbackSpeed = 3f; c.knockbackDuration = 0.15f;
            c.detectRange = 7f; c.verticalTolerance = 1.5f; c.requiresLineOfSight = true;
            c.patrolSpeed = 2f; c.chaseSpeed = 2f; c.turnsAtLedges = true;
            c.attackName = "Lunge"; c.telegraphTime = 0.5f; c.attackSpeed = 14f; c.attackActiveTime = 0.35f;
            c.attackMaxDistance = 5f; c.attackRecoveryTime = 0.8f; c.attackCooldown = 1f; c.showExclamation = true;
        }

        public static void ShadowKite(EnemyConfig c)
        {
            c.id = "shadow_kite"; c.displayName = "Shadow Kite"; c.glyph = "f"; c.phase = 1;
            c.locomotion = EnemyLocomotion.Flyer;
            c.bodySize = new Vector2(1.0f, 0.8f); c.colliderShape = EnemyColliderShape.Circle; c.colliderRadius = 0.4f;
            c.canvasSize = new Vector2Int(128, 128); c.pivotAtFeet = false;
            c.maxHp = 2; c.contactDamage = 1; c.attackDamage = 1;
            c.knockbackSpeed = 4f; c.knockbackDuration = 0.15f;
            c.detectRange = 9f; c.requiresLineOfSight = false;
            c.patrolSpeed = 0f; c.chaseSpeed = 2.5f; c.turnsAtLedges = false;
            c.hoverAmplitude = 0.4f; c.hoverFrequency = 1.5f; c.hoverHeightAbovePlayer = 3f; c.returnSpeed = 3f;
            c.attackName = "Dive"; c.attackInterval = 3f; c.telegraphTime = 0.5f; c.attackSpeed = 9f;
            c.attackMaxDistance = 6f; c.attackActiveTime = 0.7f; c.attackRecoveryTime = 0.3f;
        }

        // ------------------------------------------------------------------
        // Phase 2 enemies (Docs/04 §C) — white/blue, flame-scaled, better AI
        // ------------------------------------------------------------------

        private static void Phase2Common(EnemyConfig c)
        {
            c.phase = 2;
            c.loseTrackTime = 3f;
            c.separationRadius = 1.0f;
            c.scalesWithFlame = true;
            c.knockbackSpeed = 3f; c.knockbackDuration = 0.15f;
        }

        public static void Knight(EnemyConfig c)
        {
            Phase2Common(c);
            c.id = "knight"; c.displayName = "Castle Knight"; c.glyph = "k";
            c.locomotion = EnemyLocomotion.Ground;
            c.bodySize = new Vector2(1.1f, 1.9f); c.colliderShape = EnemyColliderShape.Box; c.colliderSize = new Vector2(0.8f, 1.7f);
            c.canvasSize = new Vector2Int(256, 256); c.pivotAtFeet = true;
            c.maxHp = 5; c.contactDamage = 1; c.attackDamage = 1;
            c.detectRange = 9f; c.requiresLineOfSight = true;
            c.patrolSpeed = 2f; c.chaseSpeed = 3f; c.turnsAtLedges = true;
            c.preferredDistance = 2.0f; c.retreatDistance = 2.5f; c.retreatTime = 0.5f;
            c.feintChance = 0.2f; c.waitIfPlayerAboveHeight = 1.5f;
            c.attackName = "Combo"; c.telegraphTime = 0.4f; c.attackReach = 1.6f; c.attackActiveTime = 0.1f;
            c.comboHits = 2; c.comboGap = 0.3f; c.attackRecoveryTime = 0.4f; c.attackCooldown = 0.5f;
        }

        public static void Bat(EnemyConfig c)
        {
            Phase2Common(c);
            c.id = "bat"; c.displayName = "Bat"; c.glyph = "b";
            c.locomotion = EnemyLocomotion.Flyer;
            c.bodySize = new Vector2(0.9f, 0.6f); c.colliderShape = EnemyColliderShape.Circle; c.colliderRadius = 0.3f;
            c.canvasSize = new Vector2Int(128, 128); c.pivotAtFeet = false; c.scalePivotCenter = true;
            c.maxHp = 3; c.contactDamage = 1; c.attackDamage = 1;
            c.knockbackSpeed = 4f;
            c.detectRange = 8f; c.requiresLineOfSight = false;
            c.startsAsleep = true; c.wakeTime = 0.4f;
            c.wanderSpeed = 3.5f; c.chaseSpeed = 3.5f; c.circleRadiusMin = 3f; c.circleRadiusMax = 4f; c.returnSpeed = 3f;
            c.attackName = "DiveBite"; c.attackInterval = 2.5f; c.telegraphTime = 0.35f; c.attackSpeed = 8f;
            c.attackMaxDistance = 5f; c.attackActiveTime = 0.6f; c.attackRecoveryTime = 0.4f; c.desyncOffset = 0.6f;
        }

        public static void Charger(EnemyConfig c)
        {
            Phase2Common(c);
            c.id = "charger"; c.displayName = "Siege Ram Knight"; c.glyph = "h";
            c.locomotion = EnemyLocomotion.Ground;
            c.bodySize = new Vector2(1.8f, 1.5f); c.colliderShape = EnemyColliderShape.Box; c.colliderSize = new Vector2(1.6f, 1.3f);
            c.canvasSize = new Vector2Int(320, 256); c.pivotAtFeet = true;
            c.maxHp = 7; c.contactDamage = 1; c.attackDamage = 1;
            c.detectRange = 10f; c.verticalTolerance = 1.5f; c.requiresLineOfSight = true;
            c.patrolSpeed = 1.5f; c.chaseSpeed = 1.5f; c.turnsAtLedges = true;
            c.attackName = "Charge"; c.telegraphTime = 0.6f; c.attackSpeed = 13f; c.attackMaxDistance = 10f;
            c.attackActiveTime = 0.8f; c.attackRecoveryTime = 0.5f; c.attackCooldown = 1f;
            c.wallStunTime = 1.2f; c.knockbackImmuneDuringAttack = true; c.showExclamation = true;
        }

        public static void Thrower(EnemyConfig c)
        {
            Phase2Common(c);
            c.id = "thrower"; c.displayName = "Thrower"; c.glyph = "t";
            c.locomotion = EnemyLocomotion.Ground;
            c.bodySize = new Vector2(1.0f, 1.8f); c.colliderShape = EnemyColliderShape.Box; c.colliderSize = new Vector2(0.7f, 1.6f);
            c.canvasSize = new Vector2Int(256, 256); c.pivotAtFeet = true;
            c.maxHp = 4; c.contactDamage = 1; c.attackDamage = 1;
            c.detectRange = 12f; c.requiresLineOfSight = true;
            c.patrolSpeed = 2.5f; c.chaseSpeed = 2.5f; c.turnsAtLedges = true;
            c.preferredDistance = 7.5f; c.retreatIfCloserThan = 5f; c.advanceIfFartherThan = 10f; c.hopDodgeDistance = 1.5f;
            c.attackName = "Throw"; c.attackInterval = 2.2f; c.telegraphTime = 0.4f; c.attackActiveTime = 0.1f; c.attackRecoveryTime = 0.3f;
            c.projectileSpeed = 10f; c.projectileSize = new Vector2(0.9f, 0.2f); c.projectileCanvas = new Vector2Int(128, 32);
            c.projectileLifetime = 3f; c.projectileLeadTime = 0.3f; c.projectileArcs = true; c.projectileDamage = 1;
            c.projectileScaleFactor = 0.5f;
        }

        // ------------------------------------------------------------------
        // Bosses (Docs/04 §B, §D, §E)
        // ------------------------------------------------------------------

        public static void KageWarden(BossConfig b)
        {
            b.id = "kage_warden"; b.displayName = "Kage Warden"; b.epithet = "Keeper of the Gate"; b.phase = 1;
            b.bodySize = new Vector2(1.6f, 2.4f); b.colliderSize = new Vector2(1.2f, 2.2f); b.canvasSize = new Vector2Int(384, 384);
            b.maxHp = 20; b.contactDamage = 1; b.attackDamage = 1; b.moveSpeed = 3f; b.neverStaggers = true;
            b.deathSlowmoTime = 1.2f; b.arenaWidthTiles = 40; b.arenaHeightTiles = 22;
            b.advanceTimeMin = 0.8f; b.advanceTimeMax = 1.4f; b.backstepDistance = 3f; b.backstepTime = 0.35f;
            b.blockDuration = 1.5f; b.blockPushback = 2f; b.blockMinHitsToTrigger = 2; b.blockHitWindow = 2f;
            b.blockRandomChance = 0.2f; b.counterSlashRange = 2.5f;
            b.noImmediateRepeat = true; b.maxSameMoveInRow = 1; b.fairnessGap = 0.4f;
            b.moves = new[]
            {
                new BossMoveConfig("Advance",      0f,    0f,   0f,   0f,   0, 1f) { duration = 1.1f, speed = 3f },
                new BossMoveConfig("SlashCombo",   0.45f, 0.1f, 0.6f, 2.2f, 1, 2f) { hits = 2, hitGap = 0.25f },
                new BossMoveConfig("BackStep",     0f,    0f,   0f,   3f,   0, 1f) { duration = 0.35f },
                new BossMoveConfig("Block",        0f,    1.5f, 0f,   0f,   0, 1f) { duration = 1.5f },
                new BossMoveConfig("CounterSlash", 0.3f,  0.1f, 0.5f, 2.5f, 1, 0f),
            };
        }

        public static void KageCaller(BossConfig b)
        {
            b.id = "kage_caller"; b.displayName = "Kage Caller"; b.epithet = "Summoner of Shadows"; b.phase = 1;
            b.bodySize = new Vector2(1.5f, 2.3f); b.colliderSize = new Vector2(1.1f, 2.1f); b.canvasSize = new Vector2Int(384, 384);
            b.maxHp = 30; b.contactDamage = 1; b.attackDamage = 1; b.moveSpeed = 3.5f; b.neverStaggers = true;
            b.deathSlowmoTime = 1.2f; b.arenaWidthTiles = 50; b.arenaHeightTiles = 22;
            b.enrageBelowFraction = 0.5f; b.enrageTelegraphReduction = 0.1f; b.enrageSpeedMultiplier = 1.25f; b.enrageCooldownMultiplier = 1f;
            b.summonCount = 2; b.summonCountEnraged = 3; b.summonCap = 3; b.summonCooldown = 12f; b.summonCooldownEnraged = 8f;
            b.summonEnemyIds = new[] { "crawler", "lunger" };
            b.projectileSpeed = 11f; b.projectileSize = new Vector2(1.4f, 0.6f); b.projectileCanvas = new Vector2Int(192, 96);
            b.projectileLifetime = 2f; b.projectileSecondBelowFraction = 0.66f; b.projectileSecondDelay = 0.5f;
            b.advanceTimeMin = 0.8f; b.advanceTimeMax = 1.4f; b.backstepDistance = 3f; b.backstepTime = 0.35f;
            b.noImmediateRepeat = true; b.maxSameMoveInRow = 1; b.fairnessGap = 0.4f;
            b.moves = new[]
            {
                new BossMoveConfig("Advance",    0f,    0f,   0f,   0f,   0, 1f) { duration = 1.1f, speed = 3.5f },
                new BossMoveConfig("SlashCombo", 0.35f, 0.1f, 0.6f, 2.2f, 1, 2f) { hits = 2, hitGap = 0.25f },
                new BossMoveConfig("BlackSlash", 0.5f,  0.1f, 0.6f, 0f,   1, 2f) { speed = 11f },
                new BossMoveConfig("Summon",     0.8f,  0.1f, 0.8f, 0f,   0, 1f) { cooldown = 12f },
                new BossMoveConfig("BackStep",   0f,    0f,   0f,   3f,   0, 1f) { duration = 0.35f },
            };
        }

        public static void IronColossus(BossConfig b)
        {
            b.id = "iron_colossus"; b.displayName = "Iron Colossus"; b.epithet = "The Unmoving"; b.phase = 2;
            b.bodySize = new Vector2(2.2f, 2.6f); b.colliderSize = new Vector2(1.8f, 2.4f); b.canvasSize = new Vector2Int(512, 512);
            b.maxHp = 45; b.contactDamage = 1; b.attackDamage = 2; b.moveSpeed = 1.8f; b.neverStaggers = true;
            b.deathSlowmoTime = 1.5f; b.arenaWidthTiles = 50; b.arenaHeightTiles = 22;
            b.enrageBelowFraction = 0.4f; b.enrageTelegraphReduction = 0.15f; b.enrageSpeedMultiplier = 1f; b.enrageCooldownMultiplier = 1f;
            b.blockDuration = 2f; b.blockPushback = 2f;
            b.noImmediateRepeat = false; b.maxSameMoveInRow = 2; b.fairnessGap = 0.6f;
            const int close = 1, mid = 2, far = 4;
            b.moves = new[]
            {
                new BossMoveConfig("BigArcSlash", 0.7f, 0.15f, 0.9f, 3.2f, 2, 2f) { distanceBand = close | mid },
                new BossMoveConfig("ShieldBlock", 0f,   2f,    0f,   0f,   0, 1f) { duration = 2f, distanceBand = close },
                new BossMoveConfig("ShieldBash",  0.3f, 0.1f,  0.6f, 2f,   2, 1f) { distanceBand = close },
                new BossMoveConfig("JumpSlam",    0.6f, 0.7f,  0.6f, 2f,   2, 2f) { distanceBand = mid | far, extra = 3.5f },
                new BossMoveConfig("Dash",        0.5f, 0.4f,  0.8f, 0f,   2, 2f) { speed = 12f, duration = 0.4f, distanceBand = far },
            };
        }

        public static void Zulmat(BossConfig b)
        {
            b.id = "zulmat"; b.displayName = "Zulmat"; b.epithet = "The Severed"; b.phase = 2;
            b.bodySize = new Vector2(2.0f, 3.2f); b.colliderSize = new Vector2(1.3f, 3.0f); b.canvasSize = new Vector2Int(512, 512);
            b.maxHp = 90; b.contactDamage = 1; b.attackDamage = 1; b.moveSpeed = 3f; b.neverStaggers = true;
            b.deathSlowmoTime = 2.5f; b.arenaWidthTiles = 60; b.arenaHeightTiles = 22; b.flameParksDuringFight = true;
            b.stageThresholds = new[] { 0.67f, 0.34f }; b.stageTransitionInvulnerability = 1.5f;
            // Stage 3 "combos" act like an enrage: telegraphs −0.15 s, cooldowns −20 %.
            b.enrageBelowFraction = 0.34f; b.enrageTelegraphReduction = 0.15f; b.enrageSpeedMultiplier = 1f; b.enrageCooldownMultiplier = 0.8f;
            b.summonCount = 2; b.summonCountEnraged = 2; b.summonCap = 4; b.summonCooldown = 20f; b.summonCooldownEnraged = 20f;
            b.summonEnemyIds = new[] { "knight", "bat" };
            b.shrinkScale = 0.5f; b.giantScale = 1.8f; b.transformTime = 0.5f;
            b.maxLavaZones = 3; b.lavaDuration = 6f;
            b.noImmediateRepeat = true; b.maxSameMoveInRow = 1; b.fairnessGap = 0.6f;
            b.moves = new[]
            {
                new BossMoveConfig("SwordSlash",     0.4f,  0.1f,  0.6f, 2.8f, 1, 2f) { hits = 2, hitGap = 0.25f, minStage = 1 },
                new BossMoveConfig("VineSummon",     0.8f,  1.75f, 0.6f, 3f,   1, 2f) { minStage = 1, extra = 3f, hitGap = 0.35f, duration = 0.7f },
                new BossMoveConfig("GiantSwipe",     0.8f,  0.15f, 0.8f, 6f,   1, 1.5f) { minStage = 1, extra = 2.5f, duration = 0.5f },
                new BossMoveConfig("ShrinkLunge",    0.35f, 0.4f,  0.5f, 0f,   1, 2f) { minStage = 2, speed = 16f, hits = 2, duration = 0.4f },
                new BossMoveConfig("SummonRegulars", 0.8f,  1.0f,  0.5f, 0f,   0, 1f) { minStage = 2, cooldown = 20f },
                new BossMoveConfig("LavaZones",      1.0f,  0.1f,  0.5f, 5f,   1, 1.5f) { minStage = 3, extra = 6f },
            };
        }

        // ------------------------------------------------------------------
        // Themes (Docs/07 §2)
        // ------------------------------------------------------------------

        public static void ThemePhase1Beige(ThemeConfig t)
        {
            t.id = "Phase1_Beige"; t.tileTheme = "Cave";
            t.parchmentBase = ThemeConfig.Hex("#E8D9B5"); t.parchmentEdge = ThemeConfig.Hex("#9C8460"); t.frameTint = ThemeConfig.Hex("#9C8460");
            t.environmentDark = ThemeConfig.Hex("#26262A"); t.environmentMid = ThemeConfig.Hex("#4A4A52"); t.environmentAccent = ThemeConfig.Hex("#6B3A3A");
            t.enemyBase = ThemeConfig.Hex("#16090A"); t.enemyPrimary = ThemeConfig.Hex("#C1272D"); t.enemyAccent = ThemeConfig.Hex("#FF4B3E");
            t.protagonistWhite = ThemeConfig.Hex("#EAF6FF"); t.protagonistBlue = ThemeConfig.Hex("#4FA3FF"); t.protagonistDeepBlue = ThemeConfig.Hex("#1B4F9C");
            t.protagonistOutline = false;
            t.flameCore = ThemeConfig.Hex("#FFD27A"); t.flameGlow = ThemeConfig.Hex("#FF8A2B");
            t.flameLightIntensity = 1.2f; t.globalLightColor = ThemeConfig.Hex("#FFF4E0"); t.globalLightIntensity = 0.55f;
            t.shadowAlpha = 0.30f; t.contrastBoost = 0f;
        }

        public static void ThemePhase2Inverted(ThemeConfig t)
        {
            t.id = "Phase2_Inverted"; t.tileTheme = "Castle";
            t.parchmentBase = ThemeConfig.Hex("#0E1424"); t.parchmentEdge = ThemeConfig.Hex("#03050A"); t.frameTint = ThemeConfig.Hex("#03050A");
            t.environmentDark = ThemeConfig.Hex("#D8DEE9"); t.environmentMid = ThemeConfig.Hex("#A7B0C4"); t.environmentAccent = ThemeConfig.Hex("#5E86A8");
            t.enemyBase = ThemeConfig.Hex("#F2F8FF"); t.enemyPrimary = ThemeConfig.Hex("#6EC1FF"); t.enemyAccent = ThemeConfig.Hex("#2C6FD1");
            t.protagonistWhite = ThemeConfig.Hex("#EAF6FF"); t.protagonistBlue = ThemeConfig.Hex("#4FA3FF"); t.protagonistDeepBlue = ThemeConfig.Hex("#1B4F9C");
            t.protagonistOutline = true; t.protagonistOutlineColor = Color.white; t.protagonistGlowColor = new Color(0.4f, 1f, 1f, 0.6f);
            t.flameCore = ThemeConfig.Hex("#CFEAFF"); t.flameGlow = ThemeConfig.Hex("#3EA0FF");
            t.flameLightIntensity = 1.3f; t.globalLightColor = ThemeConfig.Hex("#C9D8FF"); t.globalLightIntensity = 0.55f;
            t.shadowAlpha = 0.35f; t.contrastBoost = 0.1f;
        }

        public static void ThemeEndingPurple(ThemeConfig t)
        {
            ThemePhase2Inverted(t);
            t.id = "Ending_Purple";
            t.parchmentBase = ThemeConfig.Hex("#1A1030"); t.parchmentEdge = ThemeConfig.Hex("#07030F"); t.frameTint = ThemeConfig.Hex("#07030F");
            t.flameCore = ThemeConfig.Hex("#E5CCFF"); t.flameGlow = ThemeConfig.Hex("#9B4DFF");
            t.globalLightColor = ThemeConfig.Hex("#D8C6FF");
        }

        public static void ThemeCutscene(ThemeConfig t)
        {
            ThemePhase1Beige(t);
            t.id = "Cutscene"; t.tileTheme = "Cave";
            t.parchmentBase = ThemeConfig.Hex("#CFC8B8"); t.parchmentEdge = ThemeConfig.Hex("#6E6657"); t.frameTint = ThemeConfig.Hex("#6E6657");
            t.flameCore = ThemeConfig.Hex("#FFF1D6"); t.flameGlow = ThemeConfig.Hex("#FFC07A");
            t.globalLightColor = Color.white; t.globalLightIntensity = 0.5f;
            t.shadowAlpha = 0.30f;
        }

        // ------------------------------------------------------------------
        // Flame (Docs/07 §3-4)
        // ------------------------------------------------------------------

        public static void FlamePhase1(FlameConfig f)
        {
            f.id = "Flame_Phase1"; f.phase = 1;
            f.motion = FlameMotion.PingPong; f.sweepDuration = 480f;
            f.arcStartX = 0.05f; f.arcEndX = 0.95f; f.arcBaseY = 0.05f; f.arcHeight = 0.85f;
            f.parkedPosition = new Vector2(0.5f, 0.85f); f.parkedIntensityMultiplier = 1.2f;
            f.coreColor = ThemeConfig.Hex("#FFD27A"); f.glowColor = ThemeConfig.Hex("#FF8A2B");
            f.baseIntensity = 1.2f; f.flickerAmplitude = 0.08f; f.flickerFrequency = 6f;
            f.hitSpikeAmount = 0.3f; f.hitSpikeDuration = 0.1f;
            f.affectsEnemyScale = false; f.scaleMax = 1.4f; f.scaleMode = FlameScaleMode.Monotonic; f.scaleLerpSpeed = 2f;
        }

        public static void FlamePhase2(FlameConfig f)
        {
            FlamePhase1(f);
            f.id = "Flame_Phase2"; f.phase = 2;
            f.motion = FlameMotion.OneWay; f.sweepDuration = 1200f;
            f.coreColor = ThemeConfig.Hex("#CFEAFF"); f.glowColor = ThemeConfig.Hex("#3EA0FF");
            f.baseIntensity = 1.3f; f.flickerAmplitude = 0.12f; f.flickerFrequency = 9f;
            f.affectsEnemyScale = true;
        }
    }
}
