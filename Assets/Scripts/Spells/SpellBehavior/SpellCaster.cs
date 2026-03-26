using System;
using Managers;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject areaOfEffectPrefab;
    [SerializeField] private GameObject strikePrefab;
    //[SerializeField] private GameObject shieldPrefab;
    [SerializeField] private SpellComboDefinition[] spellCombos;
    
    [SerializeField] private bool ignorePlayer = false;
    [SerializeField] private bool ignoreEnemies = false;

    [SerializeField] private Transform projectileOrigin;
    
    public UnityEvent OnSpellCast;
    public UnityEvent OnSpellCastFailed;

    public Vector3 targetPosition; 
    private EnemyData enemyData;

    // private ProjectileBehavior projectileBehavior;
    // private AreaOfEffectBehavior areaOfEffectCombo;
    // private StrikeBehavior strikeBehavior;
    //TODO: Remove this debug action
    [SerializeField] private InputActionReference debugAction;

    
    public void Initialize(EnemyData data)
    {
        enemyData = data;
    }
    
    private void Start()
    {
        //TODO: Remove this debug action
        if (debugAction)
        {
            debugAction.action.performed += DebugCastSpell;
            // ManagersMaster.Instance.PlayerInput.actions[debugAction.name].performed += DebugCastSpell;
        }
    }

    private void OnDestroy()
    {
        if (debugAction && ManagersMaster.Instance)
        {
            debugAction.action.performed -= DebugCastSpell;
            // ManagersMaster.Instance.PlayerInput.actions[debugAction.name].performed -= DebugCastSpell;
        }
    }
    
    private void DebugCastSpell(InputAction.CallbackContext ctx)
    {
        print("Casting Spell");
        CastSpell();
    }

    // public void SetTarget(Transform target)
    // {
    //     _target = target;
    // }

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
                var spawnPosition = projectileOrigin ? projectileOrigin.position : transform.position;
                
                GameObject proj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

                var behavior = proj.GetComponent<ProjectileBehavior>();

                if (behavior)
                {
                    behavior.ChangeElement(combo);
                    behavior.ignorePlayer = ignorePlayer;
                    behavior.ignoreEnemies = ignoreEnemies;
                    
                    behavior.SetTarget(targetPosition);
                    
                    behavior.SetDamage(enemyData ? enemyData.projectileDamage : combo.damage);
                    
                    // print($"Created projectile with damage {behavior.GetDamage()}");
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
                var spawnPosition = projectileOrigin ? projectileOrigin.position : transform.position;
                
                GameObject strike = Instantiate(strikePrefab, spawnPosition, Quaternion.identity);

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