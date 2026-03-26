using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text logObject;
        [SerializeField] private Slider healthSlider;
        public static UIManager Instance { get; private set; }

        void Awake()
        {
            // if (Instance && Instance != this)
            // {
            //     Destroy(gameObject);
            //     return;
            // }
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }

        public void UpdatePlayerHealth(float health)
        {

            healthSlider.value = health;
        }

    }
}