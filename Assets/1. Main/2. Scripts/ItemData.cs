
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Consumable,
    Weapon,
    Armor,

    Max
}

public class ItemData : ScriptableObject
{
    public virtual ItemType ItemType { get; }
    // public Type classType => GetType();
    // [Space]
    [Header("Basic Item Data")]
    public string itemName;
#if Weapon
    [HideInInspector]
#endif
    public int count;
    public Sprite icon;
}
