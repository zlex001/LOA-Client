using UnityEngine;


public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
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

    // 可以根据需要添加释放资源的方法
    // public static void ReleaseInstance() { ... }
}
