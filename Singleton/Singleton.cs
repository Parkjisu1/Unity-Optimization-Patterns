using UnityEngine;

/// <summary>
/// Generic Singleton base class for MonoBehaviour managers.
/// Provides lazy initialization and DontDestroyOnLoad persistence.
/// </summary>
/// <typeparam name="T">The MonoBehaviour type to make a singleton.</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T));
            }

            if (_instance == null)
            {
                GameObject _gameObject = new GameObject();
                _gameObject.name = typeof(T).Name;
                _instance = _gameObject.AddComponent<T>();
            }

            return _instance;
        }

        set
        {
            _instance = null;
        }
    }
}
