using System;
using Interface;
using UnityEngine;
using UnityEngine.AI;

namespace Camera
{
    public class CharacterComponents : MonoBehaviour, IDamageable
    {
        public const float MAX_HEALTH = 100f;
        public event Action OnPlayerDied;
        public event Action<float> OnHealthChanged;

        public NavMeshAgent navMeshAgent => _navMeshAgent;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        public Transform cameraFollowTarget => _cameraFollowTarget;
        [SerializeField] private Transform _cameraFollowTarget;

        public Collider playerCollider => _PlayerCollider;
        [SerializeField] private Collider _PlayerCollider;


        private float health;

        private void Awake()
        {
            if (!_navMeshAgent)
                _navMeshAgent =
                    GetComponent<NavMeshAgent>(); // called only in the rare case it wasn't set in the editor 


            health = MAX_HEALTH;
            OnHealthChanged?.Invoke(health);
        }

        private void OnValidate()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>(); //editor time
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0)
            {
                OnPlayerDied?.Invoke();
                return;
            }

            OnHealthChanged?.Invoke(health);
        }
    }
}