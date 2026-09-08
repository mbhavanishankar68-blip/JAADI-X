using UnityEngine;

namespace JaadiX.Gameplay
{
    public class PocketManager : MonoBehaviour
    {
        [Header("Pocket Settings")]
        [SerializeField] private bool isEnabled = true;

        public bool IsEnabled => isEnabled;

        private void Awake()
        {
            Debug.Log($"{gameObject.name} Ready");
        }
    }
}