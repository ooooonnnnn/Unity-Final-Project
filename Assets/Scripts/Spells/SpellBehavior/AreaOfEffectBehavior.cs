using Interface;
using UnityEngine;

public class AreaOfEffectBehavior : SpellBase
{
    public void CastSpell()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, spellCombo.radius);

        foreach (var target in targets)
        {
            target.TryGetComponent<IDamageable>(out var taker);
            taker?.TakeDamage(spellCombo.damage);
        }
    }
    
}