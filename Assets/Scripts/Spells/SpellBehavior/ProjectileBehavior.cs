using System;
using Interface;
using UnityEngine;

public class ProjectileBehavior : SpellBase
{
    public float projectileSpeed = 10f;
    private Transform target;
    private float damage;
    
    private void Update()
    {
        transform.position += transform.forward * (projectileSpeed * Time.deltaTime);
    }
    public void SetTarget(Transform target)
    {
        this.target = target;

        if (target)
        { Vector3 vector3 = new Vector3(target.localPosition.x, target.localPosition.y + 1, target.localPosition.z);
            
            Vector3 dir = (vector3 - transform.position).normalized;
            transform.forward = dir;
            
        }
    }
    
    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var damageable = other.GetComponent<IDamageable>();

        damageable?.TakeDamage(damage);

        Debug.Log("I AM ENEMY I HIT PLAYER");
        Destroy(gameObject);
    }
}