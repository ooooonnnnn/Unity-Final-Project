using Managers;
using Player;
using UnityEngine;

namespace Enemy
{
    public class AttackMelee : MonoBehaviour, IEnemyAttack
    {
        [SerializeField] private float attackRate = 1f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private Animator animator;
        [SerializeField] private SpellCaster spellCaster;
        [SerializeField] private SpellComboDefinition combo;

        private Transform _target;
        private Transform _owner;
        private float _timer;

        private static readonly int AttackHash = Animator.StringToHash("Attack");

        public void Initialize(Transform owner, Transform target)
        {
            _owner = owner;
            _target = target;
        }

        public void Tick(float dt)
        {
            _timer += dt;

            if (_timer < attackRate)
                return;

            _timer = 0f;

            PerformAttack();
        }

        public bool IsInRange()
        {
            Vector3 ownerPos = transform.position;
            Vector3 targetPos = CharacterComponents.Instance.transform.position;

            ownerPos.y = 0f;
            targetPos.y = 0f;

            float distSqr = (ownerPos - targetPos).sqrMagnitude;
            return distSqr <= attackRange * attackRange;
        }

        private void PerformAttack()
        {
            animator.SetTrigger(AttackHash);

            if (spellCaster)
            {
                spellCaster.CastSpell(); 
            }
            
            CharacterComponents.Instance.TakeDamage(combo.damage);
        }
    }
}