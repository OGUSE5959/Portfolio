using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldConsumable : FieldItem
{
    protected ConsumableItemData _consumableItemData;
    public override ItemData Data => base.Data;
    public ConsumeType ConsumeType => _consumableItemData.ConsumeType;

    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        _consumableItemData = (ConsumableItemData)data;
    }
}
