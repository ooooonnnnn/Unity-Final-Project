using Extra;
using Interface;
using UnityEngine;

namespace Enemy
{
    public class EnemySpellExecutor : MonoBehaviour
    {
        [SerializeField] private SpellComboDefinition spellCombo;

        public SpellComboDefinition SpellCombo => spellCombo;

        public void Execute(Transform caster, Transform target, Transform firePoint = null)
        {
            if (!spellCombo)
            {
                Debug.LogWarning($"{name}: No SpellComboDefinition assigned.");
                return;
            }
        
            if (spellCombo.prefab)
            {
                Vector3 spawnPosition = firePoint ? firePoint.position : caster.position;
                Quaternion spawnRotation = firePoint ? firePoint.rotation : caster.rotation;

                GameObject spellObject = ProjectilePool.Instance.Get(spellCombo.prefab);
                spellObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

                if (spellObject.TryGetComponent<ISpellable>(out var spellBehaviour))
                {
                    spellBehaviour.Initialize(spellCombo, caster, target);
                }
            }
            
        }

     }
}

