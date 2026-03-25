using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldHeal : FieldConsumable
{
    HealItemData _healItemData;
    public override ItemData Data => _healItemData;
    // public AmmoType AmmoType => _healItemData.he;

    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        _healItemData = (HealItemData)_consumableItemData;
    }
}
