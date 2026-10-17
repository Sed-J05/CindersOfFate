using System;
using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Hit points + invulnerability window + events (Docs/02 §4). Shared by player, enemies, bosses, dummies.
    /// Rules: a rejected hit (dead / invulnerable / zero) returns false and fires nothing.
    /// The clock is injectable so EditMode tests can advance time without Play mode.
    /// </summary>
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHp = 1;
        [SerializeField, Min(0f)] private float invulnerabilityDuration = 0f;
        [SerializeField, Min(0f)] private float hitFlashDuration = 0.08f;
        [SerializeField] private bool destroyOnDeath = false;

        private int currentHp;
        private float invulnerableUntil = float.NegativeInfinity;
        private bool initialized;
        private bool dead;
        private Func<float> clock;

        /// <summary>Fired after HP dropped. Argument is the hit that was applied.</summary>
        public event Action<DamageInfo> OnDamaged;
        /// <summary>Fired once when HP reaches zero.</summary>
        public event Action<DamageInfo> OnDied;
        /// <summary>Fired after a heal; argument is the amount actually restored.</summary>
        public event Action<int> OnHealed;
        /// <summary>Visual hook: play a white flash for this many seconds.</summary>
        public event Action<float> OnHitFlash;
        /// <summary>(current, max) after any change.</summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>External override (god mode, dash i-frames, boss stage transition).</summary>
        public bool Invulnerable { get; set; }

        /// <summary>Optional gate run on every hit attempt that passed the i-frame check. Return false to block it (boss guard).</summary>
        public Func<DamageInfo, bool> DamageFilter { get; set; }

        /// <summary>Fired when <see cref="DamageFilter"/> rejected a hit.</summary>
        public event Action<DamageInfo> OnBlocked;

        public int MaxHp => maxHp;
        public int CurrentHp { get { EnsureInit(); return currentHp; } }
        public bool IsAlive => !dead;
        public bool IsFull => CurrentHp >= maxHp;
        public float Fraction => maxHp > 0 ? (float)CurrentHp / maxHp : 0f;
        public float HitFlashDuration => hitFlashDuration;
        public float InvulnerabilityDuration => invulnerabilityDuration;

        /// <summary>True while inside the post-hit window or while <see cref="Invulnerable"/> is set.</summary>
        public bool IsInvulnerable => Invulnerable || Now < invulnerableUntil;
        public float InvulnerabilityRemaining => Mathf.Max(0f, invulnerableUntil - Now);

        private float Now => clock != null ? clock() : Time.time;

        private void Awake() => EnsureInit();

        private void EnsureInit()
        {
            if (initialized) return;
            initialized = true;
            currentHp = maxHp;
        }

        /// <summary>Replaces the time source (tests, or a GameTime that respects hit-stop). Null restores Time.time.</summary>
        public void SetClock(Func<float> timeSource) => clock = timeSource;

        /// <summary>Sets max HP and i-frame duration, refills to full, clears death and invulnerability.</summary>
        public void Configure(int newMaxHp, float newInvulnerabilityDuration, float newHitFlashDuration = 0.08f)
        {
            initialized = true;
            maxHp = Mathf.Max(1, newMaxHp);
            invulnerabilityDuration = Mathf.Max(0f, newInvulnerabilityDuration);
            hitFlashDuration = Mathf.Max(0f, newHitFlashDuration);
            ResetToFull();
        }

        /// <summary>Changes max HP (phase change). Current HP is clamped, or refilled when <paramref name="refill"/>.</summary>
        public void SetMax(int newMaxHp, bool refill)
        {
            EnsureInit();
            maxHp = Mathf.Max(1, newMaxHp);
            currentHp = refill ? maxHp : Mathf.Min(currentHp, maxHp);
            if (currentHp > 0) dead = false;
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        public void ResetToFull()
        {
            EnsureInit();
            dead = false;
            currentHp = maxHp;
            invulnerableUntil = float.NegativeInfinity;
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        public bool TakeDamage(DamageInfo info)
        {
            EnsureInit();
            if (dead || info.amount <= 0) return false;
            if (!info.ignoreInvulnerability && IsInvulnerable) return false;
            if (!info.ignoreInvulnerability && DamageFilter != null && !DamageFilter(info))
            {
                OnBlocked?.Invoke(info);
                return false;
            }

            currentHp = Mathf.Max(0, currentHp - info.amount);
            if (invulnerabilityDuration > 0f)
                invulnerableUntil = Now + invulnerabilityDuration;

            OnHitFlash?.Invoke(hitFlashDuration);
            OnDamaged?.Invoke(info);
            OnHealthChanged?.Invoke(currentHp, maxHp);

            if (currentHp == 0)
            {
                dead = true;
                OnDied?.Invoke(info);
                if (destroyOnDeath && this != null) Destroy(gameObject);
            }
            return true;
        }

        /// <summary>Restores up to <paramref name="amount"/>. Returns the amount actually restored (0 if dead or full).</summary>
        public int Heal(int amount)
        {
            EnsureInit();
            if (dead || amount <= 0) return 0;
            int before = currentHp;
            currentHp = Mathf.Min(maxHp, currentHp + amount);
            int restored = currentHp - before;
            if (restored > 0)
            {
                OnHealed?.Invoke(restored);
                OnHealthChanged?.Invoke(currentHp, maxHp);
            }
            return restored;
        }

        /// <summary>Extends the invulnerability window to at least <paramref name="seconds"/> from now.</summary>
        public void GrantInvulnerability(float seconds)
        {
            invulnerableUntil = Mathf.Max(invulnerableUntil, Now + Mathf.Max(0f, seconds));
        }

        public void ClearInvulnerability() => invulnerableUntil = float.NegativeInfinity;

        /// <summary>Kills outright, bypassing i-frames.</summary>
        public void Kill(GameObject source = null, DamageType type = DamageType.Debug)
        {
            EnsureInit();
            if (dead) return;
            TakeDamage(new DamageInfo(currentHp, Vector2.zero, 0f, source, type, ignoreInvulnerability: true));
        }
    }
}
