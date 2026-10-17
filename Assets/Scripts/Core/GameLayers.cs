using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Single source of truth for physics layers, layer masks, the collision matrix,
    /// sorting layers and tags. <c>Shatranj/Setup Project</c> writes this table into
    /// TagManager / Physics2D settings; gameplay code only ever reads from here
    /// (no string lookups, no <see cref="LayerMask.NameToLayer"/> in hot paths).
    /// </summary>
    public static class GameLayers
    {
        // ------------------------------------------------------------------
        // Built-in Unity layers (never written by us, listed for completeness)
        // ------------------------------------------------------------------
        public const int Default       = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Water         = 4;
        public const int UI            = 5;

        // ------------------------------------------------------------------
        // Shatranj layers (user slots 8..19) — see Docs/02 §6
        // ------------------------------------------------------------------
        public const int Ground        = 8;   // solid tiles, walls, floors (CompositeCollider2D)
        public const int OneWay        = 9;   // '=' platforms, jump-through from below
        public const int Player        = 10;  // player body (dynamic Rigidbody2D + capsule)
        public const int PlayerHurtbox = 11;  // trigger that receives EnemyAttack / Hazard
        public const int Enemy         = 12;  // enemy / boss bodies
        public const int EnemyHurtbox  = 13;  // trigger that receives PlayerAttack
        public const int PlayerAttack  = 14;  // nail hitboxes, fireball
        public const int EnemyAttack   = 15;  // enemy hitboxes, contact damage, projectiles
        public const int Hazard        = 16;  // spikes, lava
        public const int Interactable  = 17;  // benches, orb, signs, doors (triggers)
        public const int RoomBounds    = 18;  // room switch triggers
        public const int Ignore        = 19;  // purely visual: FX, shadows, puppets (collides with nothing)

        public const int FirstUserLayer = 8;
        public const int LayerCount     = 32;

        /// <summary>Name + index of one layer we own.</summary>
        public readonly struct LayerDef
        {
            public readonly int Index;
            public readonly string Name;
            public LayerDef(int index, string name) { Index = index; Name = name; }
        }

        /// <summary>Every layer Setup Project must create, in slot order.</summary>
        public static readonly LayerDef[] All =
        {
            new LayerDef(Ground,        "Ground"),
            new LayerDef(OneWay,        "OneWay"),
            new LayerDef(Player,        "Player"),
            new LayerDef(PlayerHurtbox, "PlayerHurtbox"),
            new LayerDef(Enemy,         "Enemy"),
            new LayerDef(EnemyHurtbox,  "EnemyHurtbox"),
            new LayerDef(PlayerAttack,  "PlayerAttack"),
            new LayerDef(EnemyAttack,   "EnemyAttack"),
            new LayerDef(Hazard,        "Hazard"),
            new LayerDef(Interactable,  "Interactable"),
            new LayerDef(RoomBounds,    "RoomBounds"),
            new LayerDef(Ignore,        "Ignore"),
        };

        // ------------------------------------------------------------------
        // Bit masks (compile-time constants; assign directly to a LayerMask)
        // ------------------------------------------------------------------
        public const int DefaultMask       = 1 << Default;
        public const int GroundMask        = 1 << Ground;
        public const int OneWayMask        = 1 << OneWay;
        public const int PlayerMask        = 1 << Player;
        public const int PlayerHurtboxMask = 1 << PlayerHurtbox;
        public const int EnemyMask         = 1 << Enemy;
        public const int EnemyHurtboxMask  = 1 << EnemyHurtbox;
        public const int PlayerAttackMask  = 1 << PlayerAttack;
        public const int EnemyAttackMask   = 1 << EnemyAttack;
        public const int HazardMask        = 1 << Hazard;
        public const int InteractableMask  = 1 << Interactable;
        public const int RoomBoundsMask    = 1 << RoomBounds;
        public const int IgnoreMask        = 1 << Ignore;

        /// <summary>What the player can stand on (ground checks).</summary>
        public const int WalkableMask = GroundMask | OneWayMask;
        /// <summary>What stops horizontal movement / wall checks (one-way platforms don't block sideways).</summary>
        public const int WallMask = GroundMask;
        /// <summary>What a player attack hitbox is allowed to damage.</summary>
        public const int PlayerAttackTargetsMask = EnemyHurtboxMask;
        /// <summary>What an enemy attack hitbox is allowed to damage.</summary>
        public const int EnemyAttackTargetsMask = PlayerHurtboxMask;
        /// <summary>What destroys a projectile on contact.</summary>
        public const int ProjectileBlockersMask = GroundMask;
        /// <summary>What blocks enemy line-of-sight raycasts.</summary>
        public const int LineOfSightBlockersMask = GroundMask;
        /// <summary>Everything that hurts the player on touch (hazards + enemy attacks).</summary>
        public const int PlayerDangerMask = EnemyAttackMask | HazardMask;

        // Ready-made LayerMask instances for inspector-free assignment.
        public static readonly LayerMask Walkable             = WalkableMask;
        public static readonly LayerMask Wall                 = WallMask;
        public static readonly LayerMask PlayerAttackTargets  = PlayerAttackTargetsMask;
        public static readonly LayerMask EnemyAttackTargets   = EnemyAttackTargetsMask;
        public static readonly LayerMask ProjectileBlockers   = ProjectileBlockersMask;
        public static readonly LayerMask LineOfSightBlockers  = LineOfSightBlockersMask;
        public static readonly LayerMask PlayerDanger         = PlayerDangerMask;
        public static readonly LayerMask PlayerOnly           = PlayerMask;
        public static readonly LayerMask EnemyOnly            = EnemyMask;
        public static readonly LayerMask HazardOnly           = HazardMask;
        public static readonly LayerMask InteractableOnly     = InteractableMask;

        /// <summary>True if <paramref name="layer"/> (0..31) is inside <paramref name="mask"/>.</summary>
        public static bool Contains(int mask, int layer) => (mask & (1 << layer)) != 0;

        /// <summary>True if <paramref name="layer"/> (0..31) is inside <paramref name="mask"/>.</summary>
        public static bool Contains(LayerMask mask, int layer) => (mask.value & (1 << layer)) != 0;

        // ------------------------------------------------------------------
        // Collision matrix (Docs/02 §6, decisions D-005). Pairs NOT listed here
        // do not collide. Only the layers in CollisionManagedLayers are touched.
        // ------------------------------------------------------------------

        /// <summary>Layers whose matrix rows Setup Project owns (built-in FX/Water/UI are left alone).</summary>
        public static readonly int[] CollisionManagedLayers =
        {
            Default, Ground, OneWay, Player, PlayerHurtbox, Enemy, EnemyHurtbox,
            PlayerAttack, EnemyAttack, Hazard, Interactable, RoomBounds, Ignore,
        };

        /// <summary>Every pair that IS allowed to collide. Order inside a pair does not matter.</summary>
        public static readonly (int a, int b)[] CollisionPairs =
        {
            // Default = "unassigned" objects: forgiving, but never touches hit/hurt/trigger layers.
            (Default, Default),
            (Default, Ground),
            (Default, Player),
            (Default, Enemy),

            // World
            (Ground, Player),
            (Ground, Enemy),
            (Ground, PlayerAttack),      // fireball hits walls
            (Ground, EnemyAttack),       // black slashes / knives hit walls
            (OneWay, Player),
            (OneWay, Enemy),

            // Player body triggers
            (Player, Interactable),      // benches, orb, signs, doors
            (Player, RoomBounds),        // room switching

            // Damage
            (PlayerHurtbox, EnemyAttack),
            (PlayerHurtbox, Hazard),
            (EnemyHurtbox, PlayerAttack),
        };

        /// <summary>Editor/validation helper: should these two layers collide per the table above?</summary>
        public static bool ShouldCollide(int a, int b)
        {
            for (int i = 0; i < CollisionPairs.Length; i++)
            {
                var p = CollisionPairs[i];
                if ((p.a == a && p.b == b) || (p.a == b && p.b == a)) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------
        // Sorting layers (Docs/01 §12b, back → front). "Default" stays at the back, unused by the game.
        // ------------------------------------------------------------------
        public static class Sorting
        {
            public const string Default      = "Default";
            public const string Parchment    = "Parchment";
            public const string BG_Far       = "BG_Far";
            public const string BG_Mid       = "BG_Mid";
            public const string Tiles        = "Tiles";
            public const string Props        = "Props";
            public const string Enemies      = "Enemies";
            public const string Player       = "Player";
            public const string FX           = "FX";
            public const string Foreground   = "Foreground";
            public const string LightOverlay = "LightOverlay"; // doc says "Light/Overlay"; see D-004
            public const string UI           = "UI";

            /// <summary>Game sorting layers, back to front, excluding Unity's Default.</summary>
            public static readonly string[] OrderBackToFront =
            {
                Parchment, BG_Far, BG_Mid, Tiles, Props, Enemies, Player, FX, Foreground, LightOverlay, UI,
            };
        }

        // ------------------------------------------------------------------
        // Tags — for editor validation and debug tooling only (D-006). Never compare tags in gameplay hot paths.
        // ------------------------------------------------------------------
        public static class Tags
        {
            public const string Player       = "Player";
            public const string Enemy        = "Enemy";
            public const string Boss         = "Boss";
            public const string Interactable = "Interactable";

            public static readonly string[] All = { Player, Enemy, Boss, Interactable };
        }

        // ------------------------------------------------------------------
        // Validation (used by Setup Project summary and, later, Shatranj/Validate Project)
        // ------------------------------------------------------------------

        /// <summary>
        /// Checks that every layer in <see cref="All"/> exists at its expected slot and every
        /// sorting layer in <see cref="Sorting.OrderBackToFront"/> exists. Returns true when all OK.
        /// </summary>
        public static bool Validate(out string report)
        {
            var sb = new System.Text.StringBuilder();
            bool ok = true;

            for (int i = 0; i < All.Length; i++)
            {
                string actual = LayerMask.LayerToName(All[i].Index);
                if (actual != All[i].Name)
                {
                    ok = false;
                    sb.Append("Layer slot ").Append(All[i].Index).Append(" is '").Append(actual)
                      .Append("' but GameLayers expects '").Append(All[i].Name).Append("'.\n");
                }
            }

            for (int i = 0; i < Sorting.OrderBackToFront.Length; i++)
            {
                string name = Sorting.OrderBackToFront[i];
                if (SortingLayer.NameToID(name) == 0)
                {
                    ok = false;
                    sb.Append("Sorting layer '").Append(name).Append("' is missing.\n");
                }
            }

            report = ok ? "GameLayers: all layers and sorting layers present." : sb.ToString();
            return ok;
        }
    }
}
