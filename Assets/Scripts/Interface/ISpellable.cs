
using UnityEngine;

namespace Interface
{
    public interface ISpellable
    {
        void Initialize(SpellComboDefinition combo, Transform caster, Transform target);
    }
}
