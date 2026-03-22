using UnityEngine;

namespace Wave
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "Game/WaveData")]
    public class WaveData : ScriptableObject
    {
        [Header("Enemy")]
        public EnemyBase[] enemyPrefabs;

        [Header("Wave Settings")]
        public int totalEnemies = 20;
        public int batchSize = 3;
        public float spawnInterval = 2f;
        public float delayAfterWave = 10f;
    }
}