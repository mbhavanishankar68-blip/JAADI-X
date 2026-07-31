using UnityEngine;

namespace JaadiX.Core
{
    public class BootstrapManager : Singleton<BootstrapManager>
    {
        protected override void Awake()
        {
            base.Awake();

            Debug.Log("=================================");
            Debug.Log(" JAADI X Engine Booting...");
            Debug.Log("=================================");

            InitializeEngine();
        }

        private void InitializeEngine()
        {
            Debug.Log("Initializing Core Systems...");

            CreateSceneLoader();

            Debug.Log("Core Systems Initialized Successfully.");
        }

        private void CreateSceneLoader()
        {
            GameObject sceneLoaderObject = new GameObject("SceneLoader");
            sceneLoaderObject.AddComponent<SceneLoader>();

            DontDestroyOnLoad(sceneLoaderObject);

            Debug.Log("SceneLoader Initialized.");
        }
    }
}