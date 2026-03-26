using Interface;
using Player;
using UnityEngine;

public class ProjectileBehavior : SpellBase
{
    public float projectileSpeed = 10f;
    private Transform target;
    private float damage;
    
    private void Update()
    {
        transform.position += transform.forward * (projectileSpeed * Time.deltaTime);
        // print(transform.position);
    }
    public void SetTarget(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        transform.forward = dir;
    }
    
    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public float GetDamage() => damage;

    protected override void OnCollisionEnter(Collision other)
    {
        //Do nothing
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable;
        
        if (!ignorePlayer)
        {
            damageable = other.GetComponent<CharacterComponents>();
            if (damageable != null)
            {
                damageable?.TakeDamage(damage);
                // print("Self Destructing because player hit");
                SelfDestruct();
                return;
            }
        }

        if (!ignoreEnemies)
        {
            damageable = other.GetComponent<EnemyBase>();
            if (damageable != null)
            {
                damageable?.TakeDamage(damage);
                print("Self Destructing because enemy hit");
                SelfDestruct();
                return;
            }
        }
        
        // damageable = other.GetComponent<IDamageable>();
        // if (damageable == null) return;
        // damageable?.TakeDamage(damage);
        // print("Self Destructing because object hit");
        // SelfDestruct();
    }
}