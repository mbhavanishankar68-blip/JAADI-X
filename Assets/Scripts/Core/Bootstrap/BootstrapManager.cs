using UnityEngine;

namespace JaadiX.Core
{
    public class BootstrapManager : Singleton<BootstrapManager>
    {
        [Header("Configuration")]
        [SerializeField]
        private GameConfig gameConfig;

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
            if (gameConfig == null)
            {
                Debug.LogError("GameConfig is not assigned to BootstrapManager!");
                return;
            }

            Debug.Log($"Starting {gameConfig.GameName}");
            Debug.Log($"Version: {gameConfig.Version}");

            Application.targetFrameRate = gameConfig.TargetFrameRate;
            QualitySettings.vSyncCount = gameConfig.EnableVSync ? 1 : 0;

            InitializeServices();

            Debug.Log("Core Systems Initialized Successfully.");
        }

        private void InitializeServices()
        {
            CreateSceneLoader();

            // Future services
            // CreateAudioManager();
            // CreateInputManager();
            // CreateMatchManager();
            // CreateNetworkManager();
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