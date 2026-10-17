namespace Shatranj.Core
{
    /// <summary>Source category of a hit. Used for FX choice, soul rules and hazard handling.</summary>
    public enum DamageType
    {
        Melee,       // nail / sword swings
        Spell,       // fireball (grants no soul, Docs/03 §5)
        Projectile,  // black slash, knives
        Contact,     // touching an enemy body
        Hazard,      // spikes: damage + respawn at safe ground
        Lava,        // lava: damage, no teleport
        Debug,       // debug kills, never shown to the player
    }
}
