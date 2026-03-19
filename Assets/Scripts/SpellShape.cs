using UnityEngine;

public enum SpellShape
{
    /// <summary>
    /// A projectile that flies from the caster 
    /// </summary>
    Projectile,
    /// <summary>
    /// Creates an area of effect that persists for a time
    /// </summary>
    AOE,
    /// <summary>
    /// A targeted spell that deals damage to a single target.
    /// </summary>
    SingleStrike,
    /// <summary>
    /// Protects the caster from the same element fully and from other elements partially for a set time, also heals 
    /// </summary>
    Reinforce
}
