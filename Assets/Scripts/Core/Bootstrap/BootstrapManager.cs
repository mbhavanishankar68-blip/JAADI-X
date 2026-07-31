using UnityEngine;

namespace JaadiX.Core
{
    public class BootstrapManager : MonoBehaviour
    {
        #region Unity Callbacks

        private void Awake()
        {
            Debug.Log("=================================");
            Debug.Log(" JAADI X Engine Booting...");
            Debug.Log("=================================");

            InitializeEngine();
        }

        #endregion

        #region Initialization

        private void InitializeEngine()
        {
            Debug.Log("Initializing Core Systems...");

            Debug.Log("Core Systems Initialized Successfully.");
        }

        #endregion
    }
}