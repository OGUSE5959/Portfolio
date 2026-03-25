using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConsumeType
{
    Ammo,
    Heal,

    Max
}

public class ConsumableItemData : ItemData
{
    public override ItemType ItemType => ItemType.Consumable;
    public virtual ConsumeType ConsumeType { get; set; }
}
