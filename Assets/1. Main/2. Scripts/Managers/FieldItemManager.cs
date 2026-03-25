using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldItemManager : SingletonMonoBehaviour<FieldItemManager>
{
    FieldItem[] _startItems;
    Dictionary<int, FieldItem> _existItemTable = new Dictionary<int, FieldItem>();

    protected override void OnStart()
    {
        _startItems = FindObjectsByType<FieldItem>(FindObjectsSortMode.None);
        foreach (var item in _startItems)
            if (!_existItemTable.ContainsKey(item.PV.ViewID))
                _existItemTable.Add(item.PV.ViewID, item);
    }
    /*void Update()
    {
        
    }*/
}
