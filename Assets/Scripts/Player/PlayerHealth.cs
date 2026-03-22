using Interface;
using UnityEngine;

namespace Extra
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public float health = 100;

        public void TakeDamage(float damage)
        {
            health -= damage;
            Debug.Log("Player hit. HP: " + health);
        }
    }
}
