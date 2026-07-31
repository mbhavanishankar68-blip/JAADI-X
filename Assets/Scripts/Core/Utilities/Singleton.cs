using UnityEngine;

namespace JaadiX.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();

                    if (instance == null)
                    {
                        Debug.LogError($"{typeof(T).Name} does not exist in the scene.");
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else if (instance != this)
            {
                Debug.LogWarning($"Duplicate {typeof(T).Name} detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
    }
}