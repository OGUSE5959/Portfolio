using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireMode
{
    None = -1,

    Single,
    Burst,
    SemiAuto,
    Auto,

    Max
}

[System.Serializable]
public struct GunStatus
{
    static string[] FireModeName = { "단발", "점사", "반자동", "자동" };
    public string GetFireModeName(FireMode fireMode) => FireModeName[(int)fireMode];
    // public AmmoType ammoType;
    // public FireMode currFireMode;
    public AmmoType ammoType;   // 탄약 종류는 바뀔 일이 없지만 ItemData의 Status를 편집하는 김에 편하게 하려고

    public int fireModeIndex;
    public List<FireMode> ableFireModes;// = new List<FireMode>();
    [Space]
    public int burstRepeat;
    public int burstCounter;
    [Space]
    public float fireRate;
    public float fireTimer;
    [Space]
    public int magazineSize;
    public int magazine;
    [Space]
    public float aimSpeed;
    public float aimFOV;
    const float recoilMaxAbs = 30f;
    [Range(recoilMaxAbs, -recoilMaxAbs)] public float recoilPitchMax;
    [Range(-recoilMaxAbs, recoilMaxAbs)] public float recoilPitchMin;
    [Range(-recoilMaxAbs, recoilMaxAbs)] public float recoilYawMax;
    [Range(recoilMaxAbs, -recoilMaxAbs)] public float recoilYawMin;
    [Space]
    public string fireEffectName;
}
