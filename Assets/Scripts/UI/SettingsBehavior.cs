using UnityEngine;

public class SettingsBehavior : MonoBehaviour
{
    public void ToggleSettingsMenu()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    
    public void ToggleFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
}