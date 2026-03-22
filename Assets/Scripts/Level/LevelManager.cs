using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        public int CurrentLevelIndex { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadLevel(int levelIndex, SceneAsset levelScene)
        {
            CurrentLevelIndex = levelIndex;

            SceneManager.LoadScene(levelScene.name);
        }
    
        public void ReturnToLevelSelect()
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }
}