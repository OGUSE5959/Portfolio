#define Weapon

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Item Data", menuName = "Scriptable Object/Weapon Item Data", order = int.MaxValue)]
public class WeaponItemData : ItemData
{    
    public override ItemType ItemType => ItemType.Weapon;   
    public virtual WeaponType WeaponType { get; }
    [Space]
    [Header("Weapon Informations")]
    // public WeaponType weaponType;
    public WeaponUsage usage;
    public WeaponID weaponID;
    [Space]
    public Sprite weaponShape;  // 인벤토리에서 보일 Sprite
    public float damage;
}
public enum WeaponType
{
    Gun,
    Throw,
    Melee,

    Max
}
public enum WeaponID
{
    None = -1,
    
    M4,
    ScarL,
    AK,

    AWM,
    BarrettM82,

    M1014,
    Shotgun2,

    RPG_7,

    Tec,
    Glock,
    Shorty,

    Grenade,
    Flash,

    Knife,
    Machete,

    Max
}
