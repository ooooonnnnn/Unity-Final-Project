using System;
using System.Linq;
using UnityEngine;

public class MagicManager : MonoBehaviour
{
    [SerializeField] private SpellDatabase spellDatabase;
    [SerializeField, Tooltip("scores under this threshold yield no result")] private float classificationThreshold;
    public SpellDatabase SpellDatabase => spellDatabase;
    
    public SpellComboDefinition GetComboDefinition(SpellElementDefinition element, SpellTypeDefinition spellType) => spellDatabase.GetComboEntry(element, spellType).combo;

    public string[] GetInferenceLabels()
    {
        return spellDatabase.elements.Select(ele => ele.GetLabel()).Concat(
            spellDatabase.spellTypes.Select(typ => typ.GetLabel())).ToArray();
    }

    public void GetBestFitElementAndType(float[] scores, out ElementType? element, out SpellShape? type)
    {
        var numElements = spellDatabase.elements.Count;
        var numTypes = spellDatabase.spellTypes.Count;

        var indexBestElem = scores.Take(numElements).ToList().MaxIndex(out var bestElemScore);
        var indexBestType = scores.Skip(numElements).Take(numTypes).ToList().MaxIndex(out var bestTypeScore);

        element = bestElemScore > classificationThreshold ? spellDatabase.elements[indexBestElem].elementType : null;
        type = bestTypeScore > classificationThreshold ? spellDatabase.spellTypes[indexBestType].SpellShape : null;
    }
}
