using Camera;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameObject spawnPoint;

        //[SerializeField] private InputManager inputManager;
        [SerializeField] private CharacterComponents selectedCharacter;
        [SerializeField] private UIManager uiManager;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            selectedCharacter.OnHealthChanged += UpdateHealth;
            selectedCharacter.OnPlayerDied += PlayerLost;
            EnemySpawner.OnWaveCompleted += PlayerWon;
        }

        private void UpdateHealth(float health)
        {
            uiManager.UpdatePlayerHealth(health / CharacterComponents.MAX_HEALTH);
        }

        private void OnDisable()
        {
            selectedCharacter.OnHealthChanged -= UpdateHealth;
            selectedCharacter.OnPlayerDied -= PlayerLost;
            EnemySpawner.OnWaveCompleted -= PlayerWon;
        }

        private void PlayerWon()
        {
          //  SceneManager.LoadScene("LevelSelect");
        }


        private void PlayerLost()
        {
          // SceneManager.LoadScene("MainMenu");
        }
    }
}