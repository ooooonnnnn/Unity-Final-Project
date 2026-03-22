using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public enum SpellElement
{
    Fire,
    Ice,
    Light,
    Dark
}

public class SpellBase : MonoBehaviour
{
    public SpellComboDefinition spellCombo;
    [SerializeField] private ParticleSystem fireParticlePrefab;
    [SerializeField] private ParticleSystem iceParticlePrefab;
    [SerializeField] private ParticleSystem lightParticlePrefab;
    [SerializeField] private ParticleSystem darkParticlePrefab;

    public bool ignorePlayer = false;
    public bool ignoreEnemies = false;
    protected ParticleSystem ActiveParticlePrefab;
    

    protected virtual void Awake()
    {
        gameObject.transform.parent = null; // Detach from caster to allow independent movement

        switch (spellCombo.element.elementEnum)
        {
            case SpellElement.Fire:
                    ActiveParticlePrefab = Instantiate(fireParticlePrefab, transform.position, transform.rotation);
                    ActiveParticlePrefab.transform.parent = transform;
                break;
            case SpellElement.Ice:
                    ActiveParticlePrefab = Instantiate(iceParticlePrefab, transform.position, transform.rotation);
                    ActiveParticlePrefab.transform.parent = transform;
                break;
            case SpellElement.Light:
            case SpellElement.Dark:
                // Instantiate corresponding particle effects for Earth, Light, and Dark elements
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        StartCoroutine(DestroyAfterLifeTime());
    }
    IEnumerator DestroyAfterLifeTime()
    {
        yield return new WaitForSeconds(spellCombo.duration);
        SelfDestruct();
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (ignorePlayer && other.gameObject.CompareTag("Player")) return;
        if (ignoreEnemies && other.gameObject.CompareTag("Enemy")) return;
        other.gameObject.TryGetComponent<ITakeSpellData>(out var component);
        component?.TakeSpellData(spellCombo);
        SelfDestruct();
    }

    protected void SelfDestruct()
    {
        ActiveParticlePrefab.transform.parent = null;
        ActiveParticlePrefab.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(gameObject);
    }

    public void ChangeElement(SpellComboDefinition newCombo)
    {
        //if (spellCombo.spellType != newCombo.spellType) return;
        
        spellCombo = newCombo;
    }
}