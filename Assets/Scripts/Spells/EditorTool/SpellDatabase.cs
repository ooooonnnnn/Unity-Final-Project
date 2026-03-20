using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellDatabase", menuName = "Spells/Spell Database")]
public class SpellDatabase : ScriptableObject
{
    public List<SpellElementDefinition> elements = new List<SpellElementDefinition>();
    public List<SpellTypeDefinition> spellTypes = new List<SpellTypeDefinition>();
    public List<SpellComboEntry> combos = new List<SpellComboEntry>();

    [Serializable]
    public class SpellComboEntry
    {
        public SpellElementDefinition element;
        public SpellTypeDefinition spellType;
        public SpellComboDefinition combo;
    }

    public SpellComboEntry GetComboEntry(SpellElementDefinition element, SpellTypeDefinition spellType)
    {
        for (int i = 0; i < combos.Count; i++)
        {
            SpellComboEntry entry = combos[i];

            if (entry != null &&
                entry.element == element &&
                entry.spellType == spellType)
            {
                return entry;
            }
        }

        return null;
    }

    public SpellComboDefinition GetCombo(SpellElementDefinition element, SpellTypeDefinition spellType)
    {
        SpellComboEntry entry = GetComboEntry(element, spellType);
        return entry != null ? entry.combo : null;
    }

    public void SetCombo(SpellElementDefinition element, SpellTypeDefinition spellType, SpellComboDefinition combo)
    {
        if (element == null || spellType == null)
        {
            return;
        }

        if (combo == null)
        {
            RemoveComboLink(element, spellType);
            return;
        }

        RemoveAllLinksToCombo(combo);

        SpellComboEntry existing = GetComboEntry(element, spellType);
        if (existing != null)
        {
            existing.combo = combo;
            return;
        }

        SpellComboEntry newEntry = new SpellComboEntry
        {
            element = element,
            spellType = spellType,
            combo = combo
        };

        combos.Add(newEntry);
    }

    public void RemoveComboLink(SpellElementDefinition element, SpellTypeDefinition spellType)
    {
        for (int i = combos.Count - 1; i >= 0; i--)
        {
            SpellComboEntry entry = combos[i];

            if (entry != null &&
                entry.element == element &&
                entry.spellType == spellType)
            {
                combos.RemoveAt(i);
            }
        }
    }

    public void RemoveAllLinksToCombo(SpellComboDefinition combo)
    {
        if (combo == null)
        {
            return;
        }

        for (int i = combos.Count - 1; i >= 0; i--)
        {
            SpellComboEntry entry = combos[i];

            if (entry != null && entry.combo == combo)
            {
                combos.RemoveAt(i);
            }
        }
    }

    public int GetTotalCellCount()
    {
        return elements.Count * spellTypes.Count;
    }

    public int GetAssignedCellCount()
    {
        int count = 0;

        for (int i = 0; i < combos.Count; i++)
        {
            SpellComboEntry entry = combos[i];

            if (entry != null &&
                entry.element != null &&
                entry.spellType != null &&
                entry.combo != null)
            {
                count++;
            }
        }

        return count;
    }

    public int GetEmptyCellCount()
    {
        return Mathf.Max(0, GetTotalCellCount() - GetAssignedCellCount());
    }

    public int GetInvalidComboReferenceCount()
    {
        int count = 0;

        for (int i = 0; i < combos.Count; i++)
        {
            SpellComboEntry entry = combos[i];

            if (entry == null || entry.combo == null)
            {
                continue;
            }

            if (entry.combo.element != entry.element || entry.combo.spellType != entry.spellType)
            {
                count++;
            }
        }

        return count;
    }

    public void Normalize()
    {
        elements.RemoveAll(item => item == null);
        spellTypes.RemoveAll(item => item == null);

        for (int i = combos.Count - 1; i >= 0; i--)
        {
            SpellComboEntry entry = combos[i];

            if (entry == null ||
                entry.element == null ||
                entry.spellType == null ||
                entry.combo == null)
            {
                combos.RemoveAt(i);
                continue;
            }

            if (!elements.Contains(entry.element) || !spellTypes.Contains(entry.spellType))
            {
                combos.RemoveAt(i);
            }
        }

        HashSet<string> seenPairs = new HashSet<string>();

        for (int i = combos.Count - 1; i >= 0; i--)
        {
            SpellComboEntry entry = combos[i];
            string key = entry.element.GetInstanceID() + "_" + entry.spellType.GetInstanceID();

            if (seenPairs.Contains(key))
            {
                combos.RemoveAt(i);
                continue;
            }

            seenPairs.Add(key);
        }
    }
}