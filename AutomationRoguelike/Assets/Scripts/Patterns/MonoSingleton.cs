using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static bool _isExiting = false;
    private static T _instance;

    /// <summary>
    /// Will return null if the application is exiting. When requesting Instance in OnDestroy, always perform null checks.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_isExiting)
            {
                return null;
            }

            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = "[S] " + typeof(T).ToString();
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// When override awake, make sure to run base.Awake(), or inherit from LateAwake().
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            LateAwake();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Override this instead of Awake to make sure this class awake is called.
    /// </summary>
    protected virtual void LateAwake()
    {
    }

    private void OnApplicationQuit()
    {
        _isExiting = true;
    }
}