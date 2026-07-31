using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaadiX.Core
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        public void LoadScene(string sceneName)
        {
            Debug.Log($"Loading Scene : {sceneName}");

            SceneManager.LoadScene(sceneName);
        }
    }
}