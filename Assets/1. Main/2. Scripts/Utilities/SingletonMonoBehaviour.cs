using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    private static T m_instance;
    public static T Instance {  get { return m_instance; } }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }

    private void Awake()
    {
        if(m_instance == null)
        {
            m_instance = (T)this;
        }
        OnAwake();
    }
    private void Start()
    {
        OnStart();
    }
}
