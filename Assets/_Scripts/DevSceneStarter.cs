using UnityEngine;
using UnityEngine.SceneManagement;

public class DevSceneStarter : MonoBehaviour
{
    [Header("Dev Options")]
    public bool devMode = true;
    public string targetScene = "HomeScreen";

    void Start()
    {
#if UNITY_EDITOR
        if (devMode)
        {
            Debug.Log("[DEV] Loading dev target scene: " + targetScene);
            SceneManager.LoadScene(targetScene);
        }
#endif
    }
}