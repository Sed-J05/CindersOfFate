using System;

namespace Shatranj.Core
{
    /// <summary>Ability flags per Docs/03 §9. Phase 1 = {false,false,5}; Phase 2 = {true,true,7}.</summary>
    [Serializable]
    public class PlayerAbilities
    {
        public bool canDash;
        public bool canFireball;
        public int maxHearts = GameConstants.HeartsPhase1;

        public PlayerAbilities() { }

        public PlayerAbilities(bool canDash, bool canFireball, int maxHearts)
        {
            this.canDash = canDash;
            this.canFireball = canFireball;
            this.maxHearts = maxHearts;
        }

        /// <summary>The ability set a phase grants by default.</summary>
        public static PlayerAbilities ForPhase(int phase)
        {
            return phase >= GameConstants.Phase2
                ? new PlayerAbilities(true, true, GameConstants.HeartsPhase2)
                : new PlayerAbilities(false, false, GameConstants.HeartsPhase1);
        }

        public PlayerAbilities Clone() => new PlayerAbilities(canDash, canFireball, maxHearts);

        public override string ToString() => $"dash={canDash} fireball={canFireball} hearts={maxHearts}";
    }
}
