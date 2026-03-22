using UnityEngine;


    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Movement")] 
        public float moveSpeed = 3f;

        public float minAttackRange; 
        public float maxAttackRange; 

        [Header("Attack")] 
        public float fireRate = 2f;

        public float projectileSpeed = 12f;
        public float projectileDamage = 10f;

        [Header("References")]
        public GameObject projectilePrefab;
    }

