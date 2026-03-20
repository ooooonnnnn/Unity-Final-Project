using UnityEngine;

public enum SpellDeliveryCategory
{
    Projectile,
    Shield,
    Heal,
    Area
}

[CreateAssetMenu(fileName = "NewSpellType", menuName = "Spells/Spell Type Definition")]
public class SpellTypeDefinition : ScriptableObject
{
    [Header("Behavior")] 
    public SpellShape SpellShape;
    [TextArea] public string description;

    [Header("Parameters")]
    public float baseCooldown = 1f;
    public float manaCost = 10f;
    
    [Header("Inference")]
    [TextArea] public string inferenceLabel;
}