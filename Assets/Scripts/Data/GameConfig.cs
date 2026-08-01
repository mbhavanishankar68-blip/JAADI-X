using UnityEngine;

namespace JaadiX.Core
{
    [CreateAssetMenu(
        fileName = "GameConfig",
        menuName = "JAADI X/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Information")]

        public string GameName = "JAADI X";

        public string Version = "0.0.1";

        [Header("Performance")]

        public int TargetFrameRate = 60;

        public bool EnableVSync = false;

        [Header("Debug")]

        public bool EnableDebugLogs = true;
    }
}