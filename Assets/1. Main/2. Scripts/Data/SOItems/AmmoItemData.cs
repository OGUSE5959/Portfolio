using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AmmoType
{
    None = -1,

    _9_0,
    _5_56,
    _7_62,
    _300_Magnum,
    Buckshot,
    Slug,

    Max
}

[CreateAssetMenu(fileName = "Ammo Item Data", menuName = "Scriptable Object/Ammo Item Data", order = int.MaxValue)]
public class AmmoItemData : ConsumableItemData
{
    public override ConsumeType ConsumeType => ConsumeType.Ammo;
    public AmmoType ammoType;
    // 종류마다 특유의 색깔을 가져오는 메서드
    public static string GetColor(AmmoType ammoType, out string info)
    {
        info = string.Empty;
        switch (ammoType)
        {
            case AmmoType._9_0:
                info = "노란색"; return Utility.ToRGBHex(Color.yellow);
            case AmmoType._5_56:
                info = "연두색"; return "ADFF2F";
            case AmmoType._7_62: 
                info = "주황색"; return "FFA500";
            case AmmoType._300_Magnum:
                info = "회색"; return Utility.ToRGBHex(Color.gray);
            case AmmoType.Buckshot:
                info = "벅샷(산탄)"; return Utility.ToRGBHex(Color.red);
        }
        return "FFFFFF";
    }
}
