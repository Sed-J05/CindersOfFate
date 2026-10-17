namespace Shatranj.Core
{
    /// <summary>Anything that can receive a <see cref="DamageInfo"/>.</summary>
    public interface IDamageable
    {
        /// <summary>Applies the hit. Returns false when it was rejected (dead, invulnerable, zero damage).</summary>
        bool TakeDamage(DamageInfo info);

        bool IsAlive { get; }
    }
}
