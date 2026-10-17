using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// The K button (Docs/03 §2, §5). Hold = Heal: costs 33 soul, 0.9 s rooted on the ground, restores one heart,
    /// cancelled by damage or early release (soul only spent on completion), chains while held.
    /// Tap = Fireball (Phase 2 only): costs 33, 0.25 s cast rooted on the ground / free in the air.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController), typeof(PlayerSoul), typeof(PlayerHealth))]
    public class PlayerSpells : MonoBehaviour
    {
        private PlayerController controller;
        private PlayerInput input;
        private PlayerSoul soul;
        private PlayerHealth health;
        private PlayerAnimator animator;
        private PlayerFxPools fx;
        private PlayerConfig config;

        private bool healing;
        private bool healHeld;
        private float healTimer;

        public bool IsHealing => healing;
        public float HealProgress => healing ? Mathf.Clamp01(healTimer / config.healHoldTime) : 0f;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            input = GetComponent<PlayerInput>();
            soul = GetComponent<PlayerSoul>();
            health = GetComponent<PlayerHealth>();
            animator = GetComponent<PlayerAnimator>();
            fx = GetComponent<PlayerFxPools>();
            config = controller.Config;
        }

        private void OnEnable()
        {
            input.OnHealHoldStart += HandleHoldStart;
            input.OnHealHoldRelease += HandleHoldRelease;
            input.OnSpellTap += HandleTap;
            controller.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            input.OnHealHoldStart -= HandleHoldStart;
            input.OnHealHoldRelease -= HandleHoldRelease;
            input.OnSpellTap -= HandleTap;
            controller.OnStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            if (!healing) return;
            healTimer += Time.deltaTime;
            if (healTimer >= config.healHoldTime) CompleteHeal();
        }

        // ------------------------------------------------------------------
        // Heal
        // ------------------------------------------------------------------

        private void HandleHoldStart()
        {
            healHeld = true;
            TryStartHeal();
        }

        private void HandleHoldRelease()
        {
            healHeld = false;
            if (healing) CancelHeal(); // released early: soul not consumed
        }

        private bool TryStartHeal()
        {
            if (healing) return false;
            if (!soul.CanSpend(config.healCost)) return false;
            if (health.IsFull) return false;
            if (!controller.IsGrounded) return false;
            PlayerState s = controller.State;
            if (s != PlayerState.Idle && s != PlayerState.Run && s != PlayerState.Land) return false;

            healing = true;
            healTimer = 0f;
            controller.SetState(PlayerState.HealCharge);
            if (animator != null) animator.SetHealAura(true);
            AudioManager.Play("player_heal_start");
            return true;
        }

        private void CancelHeal()
        {
            healing = false;
            if (animator != null) animator.SetHealAura(false);
            if (controller.State == PlayerState.HealCharge) controller.ReturnToNeutral();
        }

        private void CompleteHeal()
        {
            healing = false;
            soul.Spend(config.healCost);
            health.HealHearts(config.healAmount);
            AudioManager.Play("player_heal");
            if (animator != null) animator.SetHealAura(false);

            // Chain while still held and affordable; otherwise return to neutral.
            if (healHeld && TryStartHeal()) return;
            if (controller.State == PlayerState.HealCharge) controller.ReturnToNeutral();
        }

        private void HandleStateChanged(PlayerState previous, PlayerState next)
        {
            // Damage (Hurt), death, dash or cutscene interrupt the charge: nothing is spent.
            if (healing && previous == PlayerState.HealCharge && next != PlayerState.HealCharge)
            {
                healing = false;
                if (animator != null) animator.SetHealAura(false);
            }
        }

        // ------------------------------------------------------------------
        // Fireball
        // ------------------------------------------------------------------

        private void HandleTap()
        {
            TryFireball();
        }

        public bool CanFireball
        {
            get
            {
                bool unlocked = GameManager.HasInstance ? GameManager.Instance.Abilities.canFireball : false;
                if (!unlocked || !soul.CanSpend(config.fireballCost)) return false;
                switch (controller.State)
                {
                    case PlayerState.Idle:
                    case PlayerState.Run:
                    case PlayerState.JumpRise:
                    case PlayerState.Fall:
                    case PlayerState.Land:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private bool TryFireball()
        {
            if (!CanFireball) return false;
            soul.Spend(config.fireballCost);
            controller.EnterTimedState(PlayerState.Cast, config.fireballCastTime);

            Vector2 spawn = (Vector2)transform.position + new Vector2(config.fireballSpawnOffset.x * controller.Facing, config.fireballSpawnOffset.y);
            if (fx != null) fx.SpawnFireball(spawn, controller.Facing, config, gameObject);
            return true;
        }
    }
}
