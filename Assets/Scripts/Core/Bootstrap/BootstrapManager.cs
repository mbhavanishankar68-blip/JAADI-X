using UnityEngine;
using UnityEngine.SceneManagement;

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

            // Listen for scenes finishing loading
            SceneManager.sceneLoaded += OnSceneLoaded;

            InitializeEngine();
        }


        private void InitializeEngine()
        {
            if (gameConfig == null)
            {
                Debug.LogError(
                    "GameConfig is not assigned to BootstrapManager!"
                );

                return;
            }


            Debug.Log(
                $"Starting {gameConfig.GameName}"
            );

            Debug.Log(
                $"Version: {gameConfig.Version}"
            );


            Application.targetFrameRate =
                gameConfig.TargetFrameRate;

            QualitySettings.vSyncCount =
                gameConfig.EnableVSync ? 1 : 0;


            InitializeServices();


            Debug.Log(
                "Core Systems Initialized Successfully."
            );
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
            GameObject sceneLoaderObject =
                new GameObject("SceneLoader");

            sceneLoaderObject.AddComponent<SceneLoader>();

            DontDestroyOnLoad(sceneLoaderObject);


            Debug.Log(
                "SceneLoader Initialized."
            );
        }


        //==================================================
        // SCENE LOADED
        //==================================================

        private void OnSceneLoaded(
            Scene scene,
            LoadSceneMode mode
        )
        {
            Debug.Log(
                "Scene Loaded: " + scene.name
            );


            // We only want to initialize gameplay
            // after the Gameplay scene exists.

            if (scene.name == "Gameplay")
            {
                InitializeGameplayManager();
            }
        }


        //==================================================
        // INITIALIZE GAMEPLAY MANAGER
        //==================================================

        private void InitializeGameplayManager()
        {
            GameManager gameManager =
                FindFirstObjectByType<GameManager>();


            if (gameManager == null)
            {
                Debug.LogError(
                    "GameManager was not found in Gameplay scene!"
                );

                return;
            }


            Debug.Log(
                "Gameplay scene ready."
            );


            gameManager.InitializeGameplay();
        }


        //==================================================
        // CLEANUP
        //==================================================

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}