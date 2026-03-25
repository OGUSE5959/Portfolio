using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InteractableManager : SingletonMonoBehaviour<InteractableManager>
{
    List<Action> _regenList = new List<Action>();
    IInteractable[] _interactables;
    Dictionary<int, IInteractable> _interactTable = new Dictionary<int, IInteractable>();
    public Dictionary<int, IInteractable> InteractableTable => _interactTable;

    //PhotonView _pv;
    //public PhotonView PV => _pv;

    PlayerController _player;
    public PlayerController Player => _player;

    public void AddRegetCallbacks(Action action) => _regenList.Add(action);
    public void Regen()
    {
        foreach (Action action in _regenList)
            action();
    }
    public IInteractable GetInteractable(int id) => _interactTable[id];
    public void SetInteractable(IInteractable it)
    {
        if (!_interactTable.ContainsKey(it.PV.ViewID))
            _interactTable.Add(it.PV.ViewID, it);
    }
    public int InteractableCount() => _interactTable.Count;

    protected override void OnAwake()
    {
        base.OnAwake();
        // _pv = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    protected override void OnStart()
    {
        base.OnStart();
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
