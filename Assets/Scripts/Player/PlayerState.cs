namespace Shatranj.Player
{
    /// <summary>Docs/03 §8. Every state exists now; combat states get their behaviour in Prompt 3.</summary>
    public enum PlayerState
    {
        Idle,
        Run,
        JumpRise,
        Fall,
        Land,
        AttackSide,
        AttackUp,
        Cast,
        HealCharge,
        Dash,
        Hurt,
        Dead,
        Cutscene,
        Resting,
    }

    public static class PlayerStateExtensions
    {
        /// <summary>States where left/right input drives the motor.</summary>
        public static bool AllowsRunInput(this PlayerState s)
        {
            switch (s)
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

        /// <summary>States from which a jump may start.</summary>
        public static bool AllowsJump(this PlayerState s)
        {
            switch (s)
            {
                case PlayerState.Idle:
                case PlayerState.Run:
                case PlayerState.Fall:
                case PlayerState.Land:
                case PlayerState.AttackSide:
                case PlayerState.AttackUp:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>States where the body ignores gravity (dash) or is fully scripted.</summary>
        public static bool IgnoresGravity(this PlayerState s) => s == PlayerState.Dash;

        public static bool IsAirborne(this PlayerState s) => s == PlayerState.JumpRise || s == PlayerState.Fall;

        /// <summary>Docs/03 §8: Hurt interrupts everything except Dash, Dead and Cutscene.</summary>
        public static bool CanBeHurt(this PlayerState s) =>
            s != PlayerState.Dash && s != PlayerState.Dead && s != PlayerState.Cutscene && s != PlayerState.Hurt;
    }
}
