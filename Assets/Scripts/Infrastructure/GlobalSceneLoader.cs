using UnityEngine;
using UnityEngine.SceneManagement;

public static class GlobalSceneLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadGlobalScene()
    {
        if (!SceneManager.GetSceneByName("GlobalSystems").isLoaded)
        {
            Debug.Log("→ GlobalSystems scene is loading...");
            SceneManager.LoadScene("GlobalSystems", LoadSceneMode.Additive);
        }
        
    }
}
