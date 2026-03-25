using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;

public class Menu : MonoBehaviourPunCallbacks
{
    protected Launcher _lc;
    protected MenuManager _mm;
    [SerializeField] protected MenuType _menuType;
    public MenuType MenuType => _menuType;

    protected bool _isOpen;
    public bool IsOpen => _isOpen;

    public virtual void Initialize()
    {
        _lc = Launcher.Instance;
        _mm = MenuManager.Instance;
        // Debug.Log("Initialize");
    }
    public void Open() => gameObject.SetActive(_isOpen = true);
    public void Close() => gameObject.SetActive(_isOpen = false);

    public virtual void AddOnClick(Button button, UnityAction action)
    {
        button.onClick.AddListener(action);
    }

    /*protected void Awake()
    {

    }
    protected void Start()
    {
        
    }*/
}
