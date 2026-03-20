using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerTest : MonoBehaviour
{
    [SerializeField] private Scene nextScene;

    public void ChangeScene() => SceneManager.LoadScene(nextScene.name);
}
