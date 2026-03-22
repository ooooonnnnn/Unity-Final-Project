using UnityEngine;

public enum SpellDeliveryCategory
{
    Projectile,
    AOE,
    Strike,
    Shield
}

[CreateAssetMenu(fileName = "NewSpellType", menuName = "Spells/Spell Type Definition")]
public class SpellTypeDefinition : ScriptableObject, IInferenceLabel
{
    [Header("Behavior")]
    public SpellDeliveryCategory deliveryCategory = SpellDeliveryCategory.Projectile;
    [TextArea] public string description;

    [Header("Parameters")]
    public float baseCooldown = 1f;
    public float manaCost = 10f;

    [Header("Inference")] public string inferenceLabel;
    
    public string GetLabel() => inferenceLabel;
    public SpellDeliveryCategory spellTypeEnum;
}