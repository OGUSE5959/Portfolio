using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SingletonMonoBehaviourPunCallbacks<T> : MonoBehaviourPunCallbacks where T : SingletonMonoBehaviourPunCallbacks<T>
{
    private static T m_instance;
    public static T Instance { get { return m_instance; } }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }

    private void Awake()
    {
        if (m_instance == null)
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
