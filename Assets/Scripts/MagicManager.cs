using UnityEngine;

public class MagicManager : MonoBehaviour
{
    [SerializeField] private SpellDatabase spellDatabase;
    public SpellDatabase SpellDatabase => spellDatabase;
    
    public SpellComboDefinition GetComboDefinition(SpellElementDefinition element, SpellTypeDefinition spellType) => spellDatabase.GetComboEntry(element, spellType).combo;
}
