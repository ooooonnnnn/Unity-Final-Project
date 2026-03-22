using UnityEngine;

public class AreaOfEffectBehavior : SpellBase
{
    protected override void Awake()
    {
        base.Awake();
        spellCombo.spellType.spellTypeEnum = SpellDeliveryCategory.AOE;
        
        Collider[] targets = Physics.OverlapSphere(transform.position, spellCombo.radius / 2f);

        foreach (var target in targets)
        {
            target.TryGetComponent<ITakeSpellData>(out var taker);
            taker?.TakeSpellData(spellCombo);
        }
    }
    
}
