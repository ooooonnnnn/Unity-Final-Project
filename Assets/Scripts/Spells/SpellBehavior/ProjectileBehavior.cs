using System;
using UnityEngine;

public class ProjectileBehavior : SpellBase
{
    public float projectileSpeed = 10f;
    private Transform target;
    
    private void Update()
    {
        transform.position += transform.forward * (projectileSpeed * Time.deltaTime);
    }
    public void SetTarget(Transform target)
    {
        this.target = target;

        if (target)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            transform.forward = dir;
        }
    }
}