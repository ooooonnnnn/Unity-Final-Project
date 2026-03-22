using TMPro;
using UnityEngine;

namespace Wave
{
    public class WaveDelayUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI delayText;
        [SerializeField] private GameObject panel;

        private void Start()
        {
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            EnemySpawner.OnWaveDelayStarted += Show;
            EnemySpawner.OnWaveDelayUpdated += UpdateTimer;
        }
    
        private void Show(float time)
        {
            panel.SetActive(true);
            delayText.text = $"Next wave in: {(int)time}";
        }

        private void UpdateTimer(float time)
        {
            int seconds = Mathf.RoundToInt(time);
            delayText.text = $"Next wave in: {seconds}";

            if (seconds <= 0)
            {
                panel.SetActive(false);
            }
        }
    }
}