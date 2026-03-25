using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun Item Data", menuName = "Scriptable Object/Gun Item Data", order = int.MaxValue)]
public class GunItemData : WeaponItemData
{
    public override WeaponType WeaponType => WeaponType.Gun;
    [Space]
    [Header("Base Gun Informations")]
    public GunStatus baseGunState;
}
