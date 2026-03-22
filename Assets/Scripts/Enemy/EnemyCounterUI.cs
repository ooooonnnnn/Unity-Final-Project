using TMPro;
using UnityEngine;


    public class EnemyCounterUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI enemyCountText;

        private void OnEnable()
        {
            EnemySpawner.OnEnemyCountChanged += UpdateEnemyCount;
        }

        private void OnDisable()
        {
            EnemySpawner.OnEnemyCountChanged -= UpdateEnemyCount;
        }

        private void UpdateEnemyCount(int alive, int spawnedSoFar, int totalInWave)
        {
            enemyCountText.text = $"Enemies Alive: {alive}\nSpawned: {spawnedSoFar}/{totalInWave}";
        }
    }
