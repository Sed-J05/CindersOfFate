using System;
using Shatranj.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shatranj.Player
{
    /// <summary>
    /// New Input System actions built in code (CLAUDE.md §3, Docs/03 §2): keyboard + gamepad.
    /// Buffers jump/attack presses with timestamps, detects K tap (fireball) vs hold (heal).
    /// Polled in Update; consumers read the buffered state from FixedUpdate.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInput : MonoBehaviour
    {
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction attackAction;
        private InputAction spellAction;
        private InputAction dashAction;
        private InputAction pauseAction;
        private InputAction interactAction;

        private PlayerConfig config;
        private float deadzone = 0.3f;
        private float spellTapWindow = 0.18f;

        private float jumpPressedAt = float.NegativeInfinity;
        private float attackPressedAt = float.NegativeInfinity;
        private float dashPressedAt = float.NegativeInfinity;
        private float spellPressedAt = float.NegativeInfinity;
        private bool spellHeld;
        private bool spellHoldAnnounced;

        /// <summary>False while paused / in cutscenes / dead. All queries return neutral values.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Set by the controller from GameManager abilities. When false, K starts healing immediately.</summary>
        public bool FireballUnlocked { get; set; }

        // --- Movement ---
        public float MoveX { get; private set; }
        public float MoveY { get; private set; }
        public Vector2 MoveRaw { get; private set; }
        public bool UpHeld => Enabled && MoveY > 0.5f;
        public bool DownHeld => Enabled && MoveY < -0.5f;

        // --- Buttons ---
        public bool JumpHeld => Enabled && jumpAction != null && jumpAction.IsPressed();
        public bool JumpReleasedThisFrame { get; private set; }
        public bool AttackHeld => Enabled && attackAction != null && attackAction.IsPressed();
        public bool SpellHeld => Enabled && spellHeld;
        public bool DashHeld => Enabled && dashAction != null && dashAction.IsPressed();
        public bool InteractPressed { get; private set; }

        /// <summary>Fired once per Esc / Start press, even while disabled (pause menu needs it).</summary>
        public event Action OnPausePressed;
        /// <summary>K tapped and released inside the tap window (fireball).</summary>
        public event Action OnSpellTap;
        /// <summary>K held past the tap window (or pressed at all when fireball is locked): heal begins.</summary>
        public event Action OnHealHoldStart;
        /// <summary>K released after a heal hold started.</summary>
        public event Action OnHealHoldRelease;

        public void Initialize(PlayerConfig playerConfig)
        {
            config = playerConfig;
            if (config != null)
            {
                deadzone = config.moveDeadzone;
                spellTapWindow = config.spellTapWindow;
            }
        }

        private void Awake()
        {
            BuildActions();
        }

        private void BuildActions()
        {
            moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");
            moveAction.AddBinding("<Gamepad>/dpad");

            jumpAction = new InputAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");

            attackAction = new InputAction("Attack", InputActionType.Button);
            attackAction.AddBinding("<Keyboard>/j");
            attackAction.AddBinding("<Gamepad>/buttonWest");

            spellAction = new InputAction("Spell", InputActionType.Button);
            spellAction.AddBinding("<Keyboard>/k");
            spellAction.AddBinding("<Gamepad>/buttonEast");
            spellAction.AddBinding("<Gamepad>/buttonNorth");

            dashAction = new InputAction("Dash", InputActionType.Button);
            dashAction.AddBinding("<Keyboard>/l");
            dashAction.AddBinding("<Gamepad>/rightTrigger");
            dashAction.AddBinding("<Gamepad>/rightShoulder");

            pauseAction = new InputAction("Pause", InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Gamepad>/start");

            interactAction = new InputAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/w");
            interactAction.AddBinding("<Keyboard>/upArrow");
            interactAction.AddBinding("<Gamepad>/dpad/up");
        }

        private void OnEnable()
        {
            moveAction.Enable(); jumpAction.Enable(); attackAction.Enable(); spellAction.Enable();
            dashAction.Enable(); pauseAction.Enable(); interactAction.Enable();
        }

        private void OnDisable()
        {
            moveAction.Disable(); jumpAction.Disable(); attackAction.Disable(); spellAction.Disable();
            dashAction.Disable(); pauseAction.Disable(); interactAction.Disable();
            ClearBuffers();
        }

        private void OnDestroy()
        {
            moveAction?.Dispose(); jumpAction?.Dispose(); attackAction?.Dispose(); spellAction?.Dispose();
            dashAction?.Dispose(); pauseAction?.Dispose(); interactAction?.Dispose();
        }

        private void Update()
        {
            if (pauseAction.WasPressedThisFrame()) OnPausePressed?.Invoke();

            if (!Enabled)
            {
                MoveRaw = Vector2.zero; MoveX = 0f; MoveY = 0f;
                JumpReleasedThisFrame = false;
                InteractPressed = false;
                if (spellHeld) { spellHeld = false; if (spellHoldAnnounced) OnHealHoldRelease?.Invoke(); spellHoldAnnounced = false; }
                return;
            }

            float now = Time.time;

            Vector2 raw = moveAction.ReadValue<Vector2>();
            MoveRaw = raw;
            MoveX = Mathf.Abs(raw.x) < deadzone ? 0f : Mathf.Sign(raw.x);
            MoveY = Mathf.Abs(raw.y) < deadzone ? 0f : Mathf.Sign(raw.y);

            if (jumpAction.WasPressedThisFrame()) jumpPressedAt = now;
            JumpReleasedThisFrame = jumpAction.WasReleasedThisFrame();
            if (attackAction.WasPressedThisFrame()) attackPressedAt = now;
            if (dashAction.WasPressedThisFrame()) dashPressedAt = now;
            InteractPressed = interactAction.WasPressedThisFrame();

            UpdateSpell(now);
        }

        private void UpdateSpell(float now)
        {
            if (spellAction.WasPressedThisFrame())
            {
                spellHeld = true;
                spellPressedAt = now;
                spellHoldAnnounced = false;
                if (!FireballUnlocked)
                {
                    spellHoldAnnounced = true;
                    OnHealHoldStart?.Invoke();
                }
                return;
            }

            if (!spellHeld) return;

            if (spellAction.WasReleasedThisFrame() || !spellAction.IsPressed())
            {
                spellHeld = false;
                if (spellHoldAnnounced) OnHealHoldRelease?.Invoke();
                else OnSpellTap?.Invoke();
                spellHoldAnnounced = false;
                return;
            }

            if (!spellHoldAnnounced && now - spellPressedAt >= spellTapWindow)
            {
                spellHoldAnnounced = true;
                OnHealHoldStart?.Invoke();
            }
        }

        // --- Buffered queries (window in seconds) ---

        public bool JumpBuffered(float window) => Enabled && Time.time - jumpPressedAt <= window;
        public void ConsumeJump() => jumpPressedAt = float.NegativeInfinity;
        public float JumpBufferAge => Time.time - jumpPressedAt;

        public bool AttackBuffered(float window) => Enabled && Time.time - attackPressedAt <= window;
        public void ConsumeAttack() => attackPressedAt = float.NegativeInfinity;

        public bool DashBuffered(float window) => Enabled && Time.time - dashPressedAt <= window;
        public void ConsumeDash() => dashPressedAt = float.NegativeInfinity;

        public void ClearBuffers()
        {
            jumpPressedAt = attackPressedAt = dashPressedAt = float.NegativeInfinity;
        }
    }
}
