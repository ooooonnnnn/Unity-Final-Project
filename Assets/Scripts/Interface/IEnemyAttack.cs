using UnityEngine;

    public interface IEnemyAttack // shared interface of all enemy attacks.
    {
        void Initialize(Transform owner, Transform target);
        void Tick(float dt);
        bool IsInRange();
    }

