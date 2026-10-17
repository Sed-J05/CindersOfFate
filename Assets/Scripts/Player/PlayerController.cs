using System;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Player state machine (Docs/03 §8) and movement rules (Docs/03 §3). Reads <see cref="PlayerInput"/>,
    /// drives <see cref="PlayerMotor"/>. Combat states exist as timed states; Prompt 3 gives them hitboxes.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput), typeof(PlayerMotor))]
    public class PlayerController : MonoBehaviour, ICameraTarget
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private PlayerInput input;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerAnimator animator;

        public PlayerState State { get; private set; } = PlayerState.Idle;
        public PlayerState PreviousState { get; private set; } = PlayerState.Idle;
        /// <summary>+1 right, −1 left. Sprite is drawn facing right.</summary>
        public int Facing { get; private set; } = 1;
        public PlayerConfig Config => config;
        public PlayerInput Input => input;
        public PlayerMotor Motor => motor;

        public bool IsGrounded => motor.IsGrounded;
        public Vector2 Velocity => motor.Velocity;
        public float CoyoteRemaining => coyoteTimer;
        public float StateTime => stateTimer;
        public bool ControlsEnabled { get; private set; } = true;

        /// <summary>(previous, next)</summary>
        public event Action<PlayerState, PlayerState> OnStateChanged;
        public event Action OnJumped;
        public event Action OnLanded;

        private float coyoteTimer;
        private float stateTimer;
        private float stateDuration;
        private int airJumpsLeft;
        private bool jumpCutDone;
        private Vector2 cutsceneTarget;
        private float cutsceneSpeed;
        private bool cutsceneMoving;
        private float recoilTimer;
        private float recoilVelocityX;

        // --- ICameraTarget ---
        public Vector2 Position => transform.position;
        public int FacingDirection => Facing;
        public float VerticalLook => (State == PlayerState.Idle || State == PlayerState.Run) && IsGrounded ? input.MoveY : 0f;
        public bool CameraTargetGrounded => IsGrounded;

        private void Awake()
        {
            if (input == null) input = GetComponent<PlayerInput>();
            if (motor == null) motor = GetComponent<PlayerMotor>();
            if (animator == null) animator = GetComponent<PlayerAnimator>();
            if (config == null)
            {
                Debug.LogError("PlayerController: no PlayerConfig assigned — using runtime defaults.", this);
                config = ScriptableObject.CreateInstance<PlayerConfig>();
            }
            motor.Initialize(config);
            input.Initialize(config);
            airJumpsLeft = config.airJumps;
        }

        /// <summary>The live player, for enemy AI (set in OnEnable, so no Find calls in Update).</summary>
        public static PlayerController Active { get; private set; }

        private void OnEnable()
        {
            Active = this;
            GameEvents.OnAbilitiesChanged += HandleAbilitiesChanged;
            input.OnPausePressed += HandlePausePressed;
            if (GameManager.HasInstance) HandleAbilitiesChanged(GameManager.Instance.Abilities);
        }

        private void OnDisable()
        {
            if (Active == this) Active = null;
            GameEvents.OnAbilitiesChanged -= HandleAbilitiesChanged;
            input.OnPausePressed -= HandlePausePressed;
        }

        /// <summary>Esc / Start: freeze or resume (Docs/08 §7). The pause menu itself arrives in Prompt 11.</summary>
        private void HandlePausePressed()
        {
            if (!GameManager.HasInstance) return;
            GameManager gm = GameManager.Instance;
            if (gm.State == GameState.Playing || gm.State == GameState.Paused) gm.TogglePause();
        }

        private void HandleAbilitiesChanged(PlayerAbilities abilities)
        {
            input.FireballUnlocked = abilities.canFireball;
        }

        private void Update()
        {
            bool gameAllows = !GameManager.HasInstance || GameManager.Instance.InputEnabled;
            input.Enabled = ControlsEnabled && gameAllows && State != PlayerState.Dead && State != PlayerState.Cutscene;

            // Facing follows input except mid-swing (the slash direction is committed, Hollow-Knight style).
            bool swinging = State == PlayerState.AttackSide || State == PlayerState.AttackUp;
            if (State.AllowsRunInput() && !swinging && input.MoveX != 0f)
                SetFacing(input.MoveX > 0f ? 1 : -1);
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            motor.UpdateChecks();

            if (motor.IsGrounded)
            {
                coyoteTimer = config.coyoteTime;
                airJumpsLeft = config.airJumps;
            }
            else
            {
                coyoteTimer -= dt;
                if (-motor.Velocity.y > airFallSpeed) airFallSpeed = -motor.Velocity.y;
            }

            stateTimer += dt;
            TickState(dt);

            motor.GravityMultiplier = State.IgnoresGravity() ? 0f : 1f;
            motor.ApplyGravity(dt);
        }

        // ------------------------------------------------------------------
        // State machine
        // ------------------------------------------------------------------

        private void TickState(float dt)
        {
            switch (State)
            {
                case PlayerState.Idle:
                case PlayerState.Run:
                    motor.ApplyRun(input.MoveX, dt);
                    if (TryJump()) return;
                    if (!motor.IsGrounded) { SetState(PlayerState.Fall); return; }
                    SetState(input.MoveX != 0f ? PlayerState.Run : PlayerState.Idle);
                    break;

                case PlayerState.JumpRise:
                    motor.ApplyRun(input.MoveX, dt);
                    if (!jumpCutDone && !input.JumpHeld) { motor.CutJump(); jumpCutDone = true; }
                    if (motor.HitCeiling) { motor.SetVelocityY(0f); SetState(PlayerState.Fall); return; }
                    if (motor.Velocity.y <= 0f) SetState(PlayerState.Fall);
                    break;

                case PlayerState.Fall:
                    motor.ApplyRun(input.MoveX, dt);
                    if (TryJump()) return;
                    if (motor.IsGrounded) Land();
                    break;

                case PlayerState.Land:
                    motor.ApplyRun(input.MoveX, dt);
                    if (TryJump()) return;
                    if (!motor.IsGrounded) { SetState(PlayerState.Fall); return; }
                    if (stateTimer >= config.landSquashTime)
                        SetState(input.MoveX != 0f ? PlayerState.Run : PlayerState.Idle);
                    break;

                case PlayerState.AttackSide:
                case PlayerState.AttackUp:
                    if (recoilTimer > 0f)
                    {
                        // Docs/03 §4 self-recoil: brief push away from the enemy, grounded only.
                        recoilTimer -= dt;
                        motor.SetVelocityX(recoilVelocityX);
                    }
                    else motor.ApplyRun(input.MoveX, dt);
                    if (TryJump()) return;
                    if (stateTimer >= stateDuration) ExitToNeutral();
                    break;

                case PlayerState.Cast:
                    // Rooted on the ground, free in the air (Docs/03 §5).
                    if (motor.IsGrounded) motor.ApplyBrake(dt); else motor.ApplyRun(input.MoveX, dt);
                    if (stateTimer >= stateDuration) ExitToNeutral();
                    break;

                case PlayerState.HealCharge:
                case PlayerState.Resting:
                    motor.ApplyBrake(dt);
                    if (stateDuration > 0f && stateTimer >= stateDuration) ExitToNeutral();
                    break;

                case PlayerState.Dash:
                    motor.SetVelocity(new Vector2(Facing * config.dashSpeed, 0f));
                    if (stateTimer >= stateDuration) { motor.SetVelocity(Vector2.zero); ExitToNeutral(); }
                    break;

                case PlayerState.Hurt:
                    // Knockback velocity was set on entry; let it decay while stunned.
                    motor.ApplyBrake(dt, config.hurtKnockbackTime);
                    if (stateTimer >= stateDuration) ExitToNeutral();
                    break;

                case PlayerState.Dead:
                    motor.ApplyBrake(dt);
                    break;

                case PlayerState.Cutscene:
                    TickCutscene(dt);
                    break;
            }
        }

        private bool TryJump()
        {
            if (!State.AllowsJump()) return false;
            if (!input.JumpBuffered(config.jumpBufferTime)) return false;

            bool canGroundJump = motor.IsGrounded || coyoteTimer > 0f;
            if (!canGroundJump)
            {
                if (airJumpsLeft <= 0) return false;
                airJumpsLeft--;
            }

            input.ConsumeJump();
            coyoteTimer = 0f;
            jumpCutDone = false;
            motor.Jump();
            SetState(PlayerState.JumpRise);
            OnJumped?.Invoke();
            AudioManager.Play("player_jump");
            if (canGroundJump) Dust(0f);   // off the ground only, not air jumps
            return true;
        }

        private void Land()
        {
            SetState(PlayerState.Land);
            OnLanded?.Invoke();
            AudioManager.Play("player_land");
            if (airFallSpeed >= config.landDustMinFallSpeed) Dust(0f);
            airFallSpeed = 0f;
        }

        private void ExitToNeutral()
        {
            if (!motor.IsGrounded) SetState(PlayerState.Fall);
            else SetState(input.MoveX != 0f ? PlayerState.Run : PlayerState.Idle);
        }

        /// <summary>Leaves any special state for Idle / Run / Fall depending on ground and input.</summary>
        public void ReturnToNeutral() => ExitToNeutral();

        /// <summary>Horizontal push for <paramref name="time"/> seconds during an attack (self-recoil on hit).</summary>
        private float airFallSpeed;

        /// <summary>Dust puff at the feet (jump off the ground, hard landing, dash start). <paramref name="behind"/> u behind the facing.</summary>
        public void Dust(float behind)
        {
            Vector2 feet = (Vector2)transform.position + new Vector2(-Facing * behind, 0.25f);
            FxPool.Play("Dust_Puff", feet, -Facing, config.dustScale);
        }

        public void ApplyRecoil(float velocityX, float time)
        {
            recoilVelocityX = velocityX;
            recoilTimer = time;
        }

        private void TickCutscene(float dt)
        {
            if (!cutsceneMoving) { motor.ApplyBrake(dt); return; }
            float dx = cutsceneTarget.x - transform.position.x;
            if (Mathf.Abs(dx) <= 0.05f)
            {
                cutsceneMoving = false;
                motor.SetVelocityX(0f);
                return;
            }
            SetFacing(dx > 0f ? 1 : -1);
            motor.SetVelocityX(Mathf.Sign(dx) * cutsceneSpeed);
        }

        public void SetState(PlayerState next)
        {
            if (State == next) return;
            PreviousState = State;
            State = next;
            stateTimer = 0f;
            stateDuration = 0f;
            recoilTimer = 0f;
            OnStateChanged?.Invoke(PreviousState, next);
        }

        /// <summary>Enters a state that ends automatically after <paramref name="duration"/> (attacks, cast, dash, hurt).</summary>
        public void EnterTimedState(PlayerState next, float duration)
        {
            if (State == next) { stateTimer = 0f; stateDuration = duration; return; }
            SetState(next);
            stateDuration = duration;
        }

        public void SetFacing(int direction)
        {
            if (direction == 0 || direction == Facing) return;
            Facing = direction > 0 ? 1 : -1;
            if (animator != null) animator.SetFacing(Facing);
        }

        // ------------------------------------------------------------------
        // External control (cutscenes, hurt, death, respawn)
        // ------------------------------------------------------------------

        /// <summary>Docs/03 §8: cutscene override. Input is disabled; call <see cref="PlayCutsceneMove"/> to walk.</summary>
        public void SetCutsceneControl(bool on)
        {
            if (on) { cutsceneMoving = false; SetState(PlayerState.Cutscene); }
            else if (State == PlayerState.Cutscene) { cutsceneMoving = false; ExitToNeutral(); }
        }

        public void PlayCutsceneMove(Vector2 target, float speed)
        {
            if (State != PlayerState.Cutscene) SetState(PlayerState.Cutscene);
            cutsceneTarget = target;
            cutsceneSpeed = Mathf.Max(0.1f, speed);
            cutsceneMoving = true;
        }

        public bool CutsceneMoveDone => !cutsceneMoving;

        /// <summary>Applies knockback velocity and enters Hurt for the stun time (Docs/03 §7).</summary>
        public void ApplyHurt(Vector2 knockbackVelocity, float stunTime)
        {
            if (!State.CanBeHurt()) return;
            motor.SetVelocity(knockbackVelocity);
            EnterTimedState(PlayerState.Hurt, stunTime);
        }

        public void Die()
        {
            if (State == PlayerState.Dead) return;
            SetState(PlayerState.Dead);
        }

        /// <summary>Respawn helper: teleport, zero velocity, back to Idle.</summary>
        public void Respawn(Vector2 position)
        {
            AudioManager.Play("player_respawn");
            motor.Teleport(position);
            coyoteTimer = 0f;
            jumpCutDone = true;
            input.ClearBuffers();
            SetState(PlayerState.Idle);
        }

        /// <summary>Disables all player control without changing state (menus).</summary>
        public void SetControlsEnabled(bool enabled)
        {
            ControlsEnabled = enabled;
            if (!enabled) input.ClearBuffers();
        }
    }
}
