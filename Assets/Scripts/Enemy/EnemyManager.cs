using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


    public class EnemyManager : MonoBehaviour
    {
        private static readonly List<EnemyBase> enemies = new();
        public static IReadOnlyList<EnemyBase> Enemies => enemies;

        public static void Register(EnemyBase enemy)
        {
            if (!enemies.Contains(enemy))
                enemies.Add(enemy);
        }

        public static void Unregister(EnemyBase enemy)
        {
            enemies.Remove(enemy);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Tick(dt);
            }
            //TODO: remove this
            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                KillAllEnemies();
            }
        
        
        }
        private void KillAllEnemies()
        {
            // Copy list because enemies will unregister themselves when disabled
            var enemiesCopy = new List<EnemyBase>(enemies);

            foreach (var enemy in enemiesCopy)
            {
                enemy.Die();
            }
            Debug.Log("All enemies killed");
        }
    }