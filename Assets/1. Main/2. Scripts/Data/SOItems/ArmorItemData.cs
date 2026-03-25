using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmorType
{
    None = -1,

    Helmet,
    Vest,

    Max
}
public enum ArmorLevel
{
    None = -1,

    Lv_1,
    Lv_2,
    Lv_3,

    Max
}
[CreateAssetMenu(fileName = "Armor Item Data", menuName = "Scriptable Object/Armor Item Data", order = int.MaxValue)]
public class ArmorItemData : ItemData
{
    public override ItemType ItemType => ItemType.Armor;
    public ArmorType armorType;
    public ArmorLevel armorLevel;

    [Space]
    [Header("Armor Data")]
    public float defense;
    public float durability;
}
