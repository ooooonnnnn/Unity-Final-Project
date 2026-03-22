using UnityEngine;

[CreateAssetMenu(fileName = "NewSpellCombo", menuName = "Spells/Spell Combo Definition")]
public class SpellComboDefinition : ScriptableObject
{
    [Header("Links")]
    public SpellElementDefinition element;
    public SpellTypeDefinition spellType;

    [Header("Description")]
    [TextArea] public string description;

    [Header("Gameplay")]
    public bool isUnlockedByDefault = true;
    public float powerMultiplier = 1f;
    public float duration = 5f;
    public float speed = 10f;
    public float radius = 1f;
    public GameObject prefab;
    
}