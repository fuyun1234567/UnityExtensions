using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonMono<T>:MonoBehaviour where T : class
{
    protected static T _instance;
    public static T GetInstance
    {
        get
        {
            if (_instance == null)
            {
#if UNITY_EDITOR
                if (FindObjectsOfType(typeof(T)).Length>1)
                {
                    Debug.LogError("Singleton must be unique");
                }
#endif
                _instance = FindObjectOfType(typeof(T)) as T;
                
            }
            if (_instance == null)
            {
                GameObject go = new GameObject(typeof(T).ToString());
                _instance = go.AddComponent(typeof(T)) as T;
            }
            return _instance;
        }
    }
}
