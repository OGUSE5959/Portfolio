using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealItemData : ConsumableItemData
{
    public override ConsumeType ConsumeType => ConsumeType.Heal;
    public float healAmount;
}
