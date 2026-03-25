using System.Linq;
using Interface;
using Player;
using UnityEngine;

public class AreaOfEffectBehavior : SpellBase
{
    public void CastSpell()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, spellCombo.radius);

        foreach (var target in targets)
        {
            if (!ignorePlayer)
            {
                target.TryGetComponent<CharacterComponents>(out var taker);
                taker?.TakeDamage(spellCombo.damage);
            }
            else if (!ignoreEnemies)
            {
                target.TryGetComponent<EnemyBase>(out var taker);
                taker?.TakeDamage(spellCombo.damage);
            }
            else
            {
                target.TryGetComponent<IDamageable>(out var damageable);
                damageable?.TakeDamage(spellCombo.damage);
            }
        }
    }
    
}