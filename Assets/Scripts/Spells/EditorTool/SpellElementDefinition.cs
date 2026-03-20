using UnityEngine;

[CreateAssetMenu(fileName = "NewElement", menuName = "Spells/Element Definition")]
public class SpellElementDefinition : ScriptableObject
{
    [Header("Presentation")]
    public Color elementColor = Color.white;
    [TextArea] public string description;

    [Header("Parameters")] 
    public float basePowerModifier = 1f;
    public float statusChance = 0f;
    
    [Header("Inference")]
    [TextArea] public string inferenceLabel;
}