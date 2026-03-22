using UnityEngine;

[CreateAssetMenu(fileName = "NewElement", menuName = "Spells/Element Definition")]
public class SpellElementDefinition : ScriptableObject, IInferenceLabel
{
    [Header("Presentation")]
    public Color elementColor = Color.white;
    [TextArea] public string description;

    [Header("Parameters")]
    public SpellElement elementEnum;
    public float basePowerModifier = 1f;
    public float statusChance = 0f;
    [TextArea] public string inferenceLabel;
    public string GetLabel() => inferenceLabel;
    
}