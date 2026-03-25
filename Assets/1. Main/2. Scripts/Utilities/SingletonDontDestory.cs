using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonDontDestory<T> : MonoBehaviour where T : SingletonDontDestory<T>
{
    private static T m_instance;
    public static T Instance { get { return m_instance; } }

    protected virtual void OnAwake() { m_instance = (T)this; }
    protected virtual void OnStart() { }

    private void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }

        DontDestroyOnLoad(this);
        OnAwake();
    }
    private void Start()
    {
        OnStart();
    }
}
