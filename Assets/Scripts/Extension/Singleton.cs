using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != (T)(object)this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = (T)(object)this;
    }
    protected virtual void OnDestroy()
    {
        if (_instance == (T)(object)this)
            _instance = null;
    }
    public static T GetInstance(bool isForceGet = false)
    {
        if (_instance == null)
        {
            _instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (_instance == null)
            {
                Debug.LogWarning($"没有找到 {typeof(T).Name} 的实例！");
                if (isForceGet) // 不安全补救策略，新建实例并获取
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }
            }
        }
        return _instance;
    }
}