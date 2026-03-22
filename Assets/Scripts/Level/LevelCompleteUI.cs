using UnityEngine;

namespace Level
{
    public class LevelCompleteUI : MonoBehaviour
    {
        public void ReturnToLevelSelect()
        {
            LevelManager.Instance.ReturnToLevelSelect();
        }
    }
}