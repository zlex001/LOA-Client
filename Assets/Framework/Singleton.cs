using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Generic singleton base class for MonoBehaviour.
    /// Automatically creates instance on first access and persists across scenes.
    /// Handles application quit gracefully to prevent instance recreation during teardown.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Returns true if the singleton instance exists and application is not quitting.
        /// Use this to check before accessing Instance in cleanup code.
        /// </summary>
        public static bool IsAlive => _instance != null && !_applicationIsQuitting;

        public static T Instance
        {
            get
            {
                // During application quit, return existing instance without creating new one
                if (_applicationIsQuitting)
                {
                    return _instance;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject(typeof(T).Name);
                        _instance = singleton.AddComponent<T>();
                        DontDestroyOnLoad(singleton);
                    }
                    return _instance;
                }
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
