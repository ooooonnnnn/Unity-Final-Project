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

        InstantiateActiveParticle();
        StartCoroutine(DestroyAfterLifeTime());
    }

    protected void InstantiateActiveParticle(bool paused = false)
    {
        ActiveParticlePrefab = spellCombo.element.elementEnum switch
        {
            SpellElement.Fire => Instantiate(fireParticlePrefab, transform.position, transform.rotation),
            SpellElement.Ice => Instantiate(iceParticlePrefab, transform.position, transform.rotation),
            SpellElement.Light => Instantiate(lightParticlePrefab, transform.position, transform.rotation),
            SpellElement.Dark => Instantiate(darkParticlePrefab, transform.position, transform.rotation),
            _ => throw new ArgumentOutOfRangeException()
        };

        ActiveParticlePrefab.transform.parent = transform;
        if (paused) ActiveParticlePrefab.Pause();
    }

    IEnumerator DestroyAfterLifeTime()
    {
        yield return new WaitForSeconds(spellCombo.duration);
        SelfDestruct();
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Spell")) return; // Ignore collisions with other spells
        if (ignorePlayer && other.gameObject.CompareTag("Player")) return;
        if (ignoreEnemies && other.gameObject.CompareTag("Enemy")) return;
        other.gameObject.TryGetComponent<ITakeSpellData>(out var component);
        component?.TakeSpellData(spellCombo);
        Debug.Log("Hit" + other.gameObject.name);
        SelfDestruct();
    }

    protected void SelfDestruct()
    {
        ActiveParticlePrefab.transform.parent = null;
        ActiveParticlePrefab.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(gameObject);
    }

    public virtual void ChangeElement(SpellComboDefinition newCombo)
    {
        if (spellCombo.spellType != newCombo.spellType) return;

        spellCombo = newCombo;
        if (ActiveParticlePrefab) Destroy(ActiveParticlePrefab);
        InstantiateActiveParticle();
    }
}