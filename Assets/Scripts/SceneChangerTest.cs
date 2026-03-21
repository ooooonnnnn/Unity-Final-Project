using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerTest : MonoBehaviour
{
    [SerializeField] private SceneAsset nextScene;

    public void ChangeScene() => SceneManager.LoadScene(nextScene.name);
}
