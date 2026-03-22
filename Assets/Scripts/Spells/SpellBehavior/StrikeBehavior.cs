using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class StrikeBehavior : SpellBase
{
    [SerializeField] private Color fireColor;
    [SerializeField] private Color iceColor;
    [SerializeField] private Color lightColor;
    [SerializeField] private Color darkColor;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float arcHeight = 5f;

    private Vector3 startPosition;
    private float progress;

    protected override void Awake()
    {
        base.Awake();
        ActiveParticlePrefab.Pause();

        // Initialize startPosition at the start of the motion
        startPosition = transform.position;
    }

    public void Update()
    {
        // Stop updating if progress exceeds duration
        if (progress >= 2)
        {
            StartCoroutine(Exolode());
            enabled = false; // Disable Update
            return;
        }

        // Update progress
        progress = Mathf.Min(progress + Time.deltaTime, 2);

        // Calculate horizontal position
        Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPosition, progress / (spellCombo.duration - 1f));

        // Calculate sinusoidal height
        float height = Mathf.Sin(progress / 2 * Mathf.PI) * arcHeight;

        // Update transform position
        transform.position = new Vector3(horizontalPosition.x, horizontalPosition.y + height, horizontalPosition.z);
    }

    // Spawn the area of effect zone and apply spell effects to targets within the zone
    protected override void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(Exolode());
    }

    private IEnumerator Exolode()
    {
        ActiveParticlePrefab.transform.position = transform.position;
        ActiveParticlePrefab.Play();
        Collider[] targets = Physics.OverlapSphere(transform.position, spellCombo.radius / 2f);

        foreach (var target in targets)
        {
            target.TryGetComponent<ITakeSpellData>(out var taker);
            taker?.TakeSpellData(spellCombo);
        }
        
        yield return new WaitForSeconds(1f);

        SelfDestruct();
    }
}