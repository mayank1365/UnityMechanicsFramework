using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// A generic implementation of the Singleton pattern for MonoBehaviours.
    /// Provides global access and ensures only one instance exists.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class.</typeparam>
    public abstract class MonoSingletonGeneric<T> : MonoBehaviour where T : MonoSingletonGeneric<T>
    {
        private static T instance;
        private static readonly object lockObject = new object();

        public static T Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<T>();

                        if (instance == null)
                        {
                            GameObject singleton = new GameObject(typeof(T).Name);
                            instance = singleton.AddComponent<T>();
                            DontDestroyOnLoad(singleton);
                        }
                    }
                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
