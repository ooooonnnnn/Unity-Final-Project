using System;
using Managers;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    //[SerializeField] private GameObject areaOfEffectPrefab;
    [SerializeField] private GameObject strikePrefab;
    //[SerializeField] private GameObject shieldPrefab;
    [SerializeField] private SpellComboDefinition[] spellCombos;
    
    [SerializeField] private bool ignorePlayer = false;
    [SerializeField] private bool ignoreEnemies = false;

    private Transform _target; 
    
    private ProjectileBehavior projectileBehavior;
    private AreaOfEffectBehavior areaOfEffectCombo;
    private StrikeBehavior strikeBehavior;

    private void Awake()
    {
        projectileBehavior = projectilePrefab.GetComponent<ProjectileBehavior>();
        strikeBehavior = strikePrefab.GetComponent<StrikeBehavior>();
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void CastSpell()
    {
        CastSpell(spellCombos[0]);
    }
    
    public void CastSpell(SpellComboDefinition combo)
    {
        if (!combo) return;

        switch (combo.spellType.spellTypeEnum)
        {
            case SpellDeliveryCategory.Projectile:
            {
                GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

                var behavior = proj.GetComponent<ProjectileBehavior>();

                if (behavior)
                {
                    behavior.ChangeElement(combo);
                    behavior.ignorePlayer = ignorePlayer;
                    behavior.ignoreEnemies = ignoreEnemies;
                    
                    behavior.SetTarget(_target);
                }

                break;
            }

            case SpellDeliveryCategory.AOE:
                break;

            case SpellDeliveryCategory.Strike:
            {
                GameObject strike = Instantiate(strikePrefab, transform.position, Quaternion.identity);

                var behavior = strike.GetComponent<StrikeBehavior>();

                if (behavior)
                {
                    behavior.ChangeElement(combo);
                    behavior.ignorePlayer = ignorePlayer;
                    behavior.ignoreEnemies = ignoreEnemies;
                }

                break;
            }

            case SpellDeliveryCategory.Shield:
                break;

            default:
                print("Invalid spell type");
                break;
        }
    }
}