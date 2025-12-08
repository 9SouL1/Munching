using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectSceneLoader : MonoBehaviour
{
    // Name of the scene to load
    public string sceneToLoad = "level_1";

    private void OnMouseDown()
    {
        // Check if this GameObject is named "1"
        if (gameObject.name == "1")
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}