using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Docs/03 §6 "Time Slip": 20 u/s for 0.18 s in the facing direction, no gravity, full i-frames,
    /// 0.5 s cooldown after it ends, one air dash per airtime, blue-white afterimage ghosts. Phase 2 only.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController), typeof(PlayerHealth))]
    public class PlayerDash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ghostSource;

        private PlayerController controller;
        private PlayerInput input;
        private PlayerHealth health;
        private PlayerFxPools fx;
        private PlayerConfig config;

        private float cooldownTimer;
        private int airDashesLeft;
        private float ghostTimer;
        private bool dashing;

        public bool IsDashing => dashing;
        public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
        public int AirDashesLeft => airDashesLeft;
        public bool Unlocked => GameManager.HasInstance && GameManager.Instance.Abilities.canDash;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            input = GetComponent<PlayerInput>();
            health = GetComponent<PlayerHealth>();
            fx = GetComponent<PlayerFxPools>();
            config = controller.Config;
            airDashesLeft = config.airDashesPerAirtime;
            if (ghostSource == null) ghostSource = GetComponentInChildren<SpriteRenderer>();
        }

        private void OnEnable()
        {
            controller.OnStateChanged += HandleStateChanged;
            controller.OnLanded += HandleLanded;
        }

        private void OnDisable()
        {
            controller.OnStateChanged -= HandleStateChanged;
            controller.OnLanded -= HandleLanded;
        }

        private void HandleLanded() => airDashesLeft = config.airDashesPerAirtime;

        private bool CanDash
        {
            get
            {
                if (!Unlocked || dashing || cooldownTimer > 0f) return false;
                if (!controller.IsGrounded && airDashesLeft <= 0) return false;
                switch (controller.State)
                {
                    case PlayerState.Idle:
                    case PlayerState.Run:
                    case PlayerState.JumpRise:
                    case PlayerState.Fall:
                    case PlayerState.Land:
                    case PlayerState.AttackSide:
                    case PlayerState.AttackUp:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            if (cooldownTimer > 0f) cooldownTimer -= dt;

            if (dashing)
            {
                ghostTimer -= dt;
                if (ghostTimer <= 0f)
                {
                    if (fx != null) fx.SpawnGhost(ghostSource, config.dashGhostFadeTime);
                    ghostTimer = config.dashDuration / Mathf.Max(1, config.dashGhostCount);
                }
                return;
            }

            if (!input.DashBuffered(config.dashInputBuffer)) return;
            if (!CanDash) return;
            StartDash();
        }

        private void StartDash()
        {
            input.ConsumeDash();
            if (!controller.IsGrounded) airDashesLeft--;
            dashing = true;
            ghostTimer = 0f;
            health.Health.GrantInvulnerability(config.dashDuration);
            controller.EnterTimedState(PlayerState.Dash, config.dashDuration);
            AudioManager.Play("player_dash");
            Feel.Play(FeelEvent.PlayerDash);
            if (controller.IsGrounded) controller.Dust(0.4f);
        }

        private void HandleStateChanged(PlayerState previous, PlayerState next)
        {
            if (previous == PlayerState.Dash && next != PlayerState.Dash)
            {
                dashing = false;
                cooldownTimer = config.dashCooldown;
            }
        }
    }
}
