using System;
using Managers;
using UnityEngine;
using UnityEngine.Events;

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
    
    // private ProjectileBehavior projectileBehavior;
    // private AreaOfEffectBehavior areaOfEffectCombo;
    // private StrikeBehavior strikeBehavior;

    // private void Awake()
    // {
    //     projectileBehavior = projectilePrefab.GetComponent<ProjectileBehavior>();
    //     strikeBehavior = strikePrefab.GetComponent<StrikeBehavior>();
    // }

    private void Start()
    {
        //Subscribe to voice input pipeline
        ManagersMaster.Instance.VoiceInputPipeline.OnPipelineDone.AddListener(CastSpellFromParameters);
        ManagersMaster.Instance.VoiceInputPipeline.OnPipelineDone.AddListener((_,_,_) => print("Recieved command"));
    }

    private void OnDestroy()
    {
        ManagersMaster.Instance.VoiceInputPipeline.OnPipelineDone.RemoveListener(CastSpellFromParameters);
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