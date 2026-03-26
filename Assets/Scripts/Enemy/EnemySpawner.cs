using System;
using System.Collections;
using Level;
using Player;
using Save;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using Wave;
using Random = UnityEngine.Random;


public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private LevelData waves;
    [SerializeField] private Transform[] spawnPoints;
    private Transform playerTarget;
    [SerializeField] private GameObject levelCompleteUI;

    private int enemiesSpawned;
    private int enemiesAlive;
    private WaveData wave;
    private bool waitingForWaveEnd;


    public static event Action<float> OnWaveDelayStarted;

    public static event Action<float> OnWaveDelayUpdated;

    public static event Action OnWaveCompleted;

    public static event Action<int, int, int> OnEnemyCountChanged; // for the ui

    public int EnemiesAlive => enemiesAlive;
    public int EnemiesSpawned => enemiesSpawned;
    public int TotalEnemies => wave.totalEnemies;

    private void Start()
    {
        playerTarget = CharacterComponents.Instance.transform;
        print(playerTarget.position);
        EnemyBase.OnEnemyKilled += HandleEnemyKilled;
        OnWaveCompleted += HandleLevelComplete;

        StartCoroutine(SpawnRoutine(waves));
        
        //TODO: Delete this
        //levelCompleteUI.SetActive(true);
    }

    private IEnumerator SpawnRoutine(LevelData level)
    {
        for (int i = 0; i < level.waves.Length; i++)
        {
            wave = level.waves[i];
            // Debug.Log("Wave " + wave);

            enemiesSpawned = 0;
            enemiesAlive = 0;

            NotifyEnemyCountChanged();

            while (enemiesSpawned < wave.totalEnemies)
            {
                SpawnBatch();
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            waitingForWaveEnd = true;

            while (waitingForWaveEnd)
            {
                if (enemiesAlive <= 0)
                {
                    waitingForWaveEnd = false;
                    break;
                }

                yield return null;
            }

            bool isLastWave = (i + 1 == level.waves.Length);
            Debug.Log("Is last wave:" + isLastWave);

            if (!isLastWave)
            {
                if (wave.delayAfterWave > 0)
                {
                    yield return StartCoroutine(RunWaveDelay(wave.delayAfterWave));
                }
            }
            else
            {
                Debug.Log("Level Complete!");
                OnWaveCompleted?.Invoke();
            }
        }
    }

    private void SpawnBatch()
    {
        int remaining = wave.totalEnemies - enemiesSpawned;
        int spawnCount = Mathf.Min(wave.batchSize, remaining);

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Vector3 offset = Random.insideUnitSphere * 3f;
        offset.y = 0;

        Vector3 spawnPos = spawnPoint.position + offset;

        EnemyBase prefab =
            wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];

        EnemyBase enemy = Instantiate(
            prefab,
            spawnPos,
            Quaternion.identity
        );

        enemy.Initialize(playerTarget);

        enemiesSpawned++;
        enemiesAlive++;
        NotifyEnemyCountChanged();
    }

    private void HandleEnemyKilled(EnemyBase enemy)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        NotifyEnemyCountChanged();

        if (enemiesAlive == 0 && waitingForWaveEnd)
        {
            waitingForWaveEnd = false;
        }
    }

    private IEnumerator RunWaveDelay(float delay)
    {
        int secondsLeft = Mathf.RoundToInt(delay);

        OnWaveDelayStarted?.Invoke(secondsLeft);
        OnWaveDelayUpdated?.Invoke(secondsLeft);

        while (secondsLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            secondsLeft--;
            OnWaveDelayUpdated?.Invoke(secondsLeft);
        }
    }

    private void NotifyEnemyCountChanged()
    {
        OnEnemyCountChanged?.Invoke(enemiesAlive, enemiesSpawned, wave.totalEnemies);
    }

    private void HandleLevelComplete()
    {
        int levelIndex = LevelManager.Instance.CurrentLevelIndex;

        SaveSystem.UnlockNextLevel(levelIndex);

        levelCompleteUI.SetActive(true);

        Time.timeScale = 0f;
    }
}