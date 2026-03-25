using System;
using Managers;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject areaOfEffectPrefab;
    [SerializeField] private GameObject strikePrefab;
    //[SerializeField] private GameObject shieldPrefab;
    [SerializeField] private SpellComboDefinition[] spellCombos;
    
    [SerializeField] private bool ignorePlayer = false;
    [SerializeField] private bool ignoreEnemies = false;
    
    public UnityEvent OnSpellCast;
    public UnityEvent OnSpellCastFailed;

    private Transform _target; 
    private EnemyData enemyData;

    // private ProjectileBehavior projectileBehavior;
    // private AreaOfEffectBehavior areaOfEffectCombo;
    // private StrikeBehavior strikeBehavior;
    //TODO: Remove this debug action
    [SerializeField] private InputActionReference debugAction;
    private Action<InputAction.CallbackContext> callbackCastSpell;

    
    public void Initialize(EnemyData data)
    {
        enemyData = data;
    }
    
    private void Start()
    {
        _target = CharacterComponents.Instance.transform;
        
        //TODO: Remove this debug action
        callbackCastSpell = ctx => CastSpell();
        debugAction.action.performed += callbackCastSpell;
    }

    private void OnDestroy()
    {
        debugAction.action.performed -= callbackCastSpell;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void CastSpell()
    {
        CastSpellFromCombo(spellCombos[0]);
    }
    
    public void CastSpellFromCombo(SpellComboDefinition combo)
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
                    behavior.SetDamage(enemyData.projectileDamage);

                }

                break;
            }

            case SpellDeliveryCategory.AOE:
            {
                
                var proj = Instantiate(areaOfEffectPrefab, transform.position, Quaternion.identity);

                var behavior = proj.GetComponent<AreaOfEffectBehavior>();

                if (behavior)
                {
                    behavior.ChangeElement(combo);
                    behavior.ignorePlayer = ignorePlayer;
                    behavior.ignoreEnemies = ignoreEnemies;
                    behavior.CastSpell();
                }

                break;
            }

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

    public void CastSpellFromParameters(SpellElement? element, SpellDeliveryCategory? type, string incantation)
    {
        if (element == null || type == null)
        {
            print("Invalid spell parameters");
            OnSpellCastFailed?.Invoke();
            return;
        }
        var combo = ManagersMaster.Instance.MagicManager.GetComboDefinition(element.Value, type.Value);
        print(combo.element.GetLabel());
        
        CastSpellFromCombo(combo);
    }
}