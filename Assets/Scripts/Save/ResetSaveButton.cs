using UnityEngine;

namespace Save
{
    public class ResetSaveButton : MonoBehaviour
    {
        public void ResetSave()
        {
            SaveSystem.ResetSave();
        }
    }
}