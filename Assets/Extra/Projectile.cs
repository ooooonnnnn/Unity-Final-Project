using Interface;
using UnityEngine;

namespace Extra
{
    public class Projectile : MonoBehaviour, ISpellable
    {
        private SpellComboDefinition _combo;
        private Transform _target;
        private float _speed;
        private float _damage;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public void Initialize(SpellComboDefinition combo, Transform caster, Transform target)
        {
            _combo = combo;
            _target = target;
            _speed = combo.speed;
            _damage = combo.powerMultiplier;
        
            if (_target)
            {
                Vector3 dir = (_target.position - _transform.position).normalized;
                _transform.forward = dir;
            }
        }
        private void Update()
        {
            if (!_target)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 direction = (_target.position - _transform.position).normalized;
            _transform.position += direction * (_speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_target == null)
                return;

            if (other.transform != _target)
                return;

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_damage);
                
            }

            ProjectilePool.Instance.Return(_combo.prefab, gameObject);
        }
    }
}