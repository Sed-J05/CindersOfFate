using System;
using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// One hit. Docs/02 §4: amount, knockback direction/force, source, type.
    /// Plain struct so it can be built and passed with no allocations.
    /// </summary>
    [Serializable]
    public struct DamageInfo
    {
        public int amount;
        public Vector2 knockbackDir;
        public float knockbackForce;
        public GameObject source;
        public DamageType type;
        /// <summary>Bypasses i-frames and the external Invulnerable flag (debug kills, scripted deaths).</summary>
        public bool ignoreInvulnerability;

        public DamageInfo(int amount, Vector2 knockbackDir, float knockbackForce, GameObject source, DamageType type, bool ignoreInvulnerability = false)
        {
            this.amount = amount;
            this.knockbackDir = knockbackDir;
            this.knockbackForce = knockbackForce;
            this.source = source;
            this.type = type;
            this.ignoreInvulnerability = ignoreInvulnerability;
        }

        /// <summary>A hit with no knockback.</summary>
        public static DamageInfo Simple(int amount, DamageType type, GameObject source = null)
        {
            return new DamageInfo(amount, Vector2.zero, 0f, source, type);
        }

        /// <summary>
        /// A hit pushing the victim away from <paramref name="from"/> toward <paramref name="to"/>.
        /// If the two points coincide the push goes to +X so knockback is never NaN.
        /// </summary>
        public static DamageInfo Directional(int amount, Vector2 from, Vector2 to, float force, DamageType type, GameObject source = null)
        {
            return new DamageInfo(amount, DirectionBetween(from, to), force, source, type);
        }

        /// <summary>
        /// A hit pushing purely horizontally away from <paramref name="from"/> (regular enemy hits, Docs/03 §7).
        /// </summary>
        public static DamageInfo Horizontal(int amount, float fromX, float toX, float force, DamageType type, GameObject source = null)
        {
            Vector2 dir = toX >= fromX ? Vector2.right : Vector2.left;
            return new DamageInfo(amount, dir, force, source, type);
        }

        /// <summary>Unit vector from a to b, or +X when they coincide.</summary>
        public static Vector2 DirectionBetween(Vector2 from, Vector2 to)
        {
            Vector2 d = to - from;
            return d.sqrMagnitude > 1e-6f ? d.normalized : Vector2.right;
        }

        public readonly bool HasKnockback => knockbackForce > 0f && knockbackDir.sqrMagnitude > 0f;

        /// <summary>Velocity to apply to the victim (direction × force).</summary>
        public readonly Vector2 KnockbackVelocity => HasKnockback ? knockbackDir * knockbackForce : Vector2.zero;

        /// <summary>Hazards and spells don't feed the soul meter; melee does (Docs/03 §5).</summary>
        public readonly bool GrantsSoul => type == DamageType.Melee;

        public readonly DamageInfo WithAmount(int newAmount)
        {
            DamageInfo copy = this;
            copy.amount = newAmount;
            return copy;
        }

        public readonly DamageInfo WithKnockback(Vector2 dir, float force)
        {
            DamageInfo copy = this;
            copy.knockbackDir = dir;
            copy.knockbackForce = force;
            return copy;
        }

        public override readonly string ToString() => $"{type} x{amount} kb={knockbackDir}*{knockbackForce}";
    }
}
