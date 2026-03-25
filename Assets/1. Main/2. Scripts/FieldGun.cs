using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FieldGun : FieldWeapon
{
    GunItemData _gunItemData;
    public override ItemData Data => _gunItemData;  // 사실 _gunItemData든 _itemData든 상관 없다
    public GunItemData GunData { get { if (!_gunItemData) MoveState(_itemData); return _gunItemData;}}
    [SerializeField] GunStatus _gunStatus;
    public GunStatus Status => _gunStatus;

    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        MoveState(data);
    }
    void MoveState(ItemData data)
    {
        _gunItemData = data as GunItemData;
        _gunStatus = _gunItemData.baseGunState;
        if (_gunStatus.aimFOV <= 0) _gunStatus.aimFOV = 50f;
    }
    /*public override void OnThrown(Vector3 dir, float force)
    {
        base.OnThrown(dir, force);
    }*/
    public void OnThrown(Vector3 dir, float force, GunStatus state)
    {
        base.OnThrown(dir, force);
        _pv.RPC("RPC_ReadState", RpcTarget.All, (byte)state.fireModeIndex, (byte)state.burstRepeat, (byte)state.burstCounter
            , state.fireRate, state.fireTimer, (byte)state.magazineSize, (byte)state.magazine , state.aimSpeed, state.aimFOV
            , state.recoilPitchMax, state.recoilPitchMin, state.recoilYawMax, state.recoilYawMin);
        // _gunStatus = state;
    }
    [PunRPC] void RPC_ReadState(byte fireModeIndex, byte burstRepeat, byte burstCounter
                , float fireRate, float fireTimer, byte magazineSize, byte magazine, float aimSpeed, float aimFOV
                , float recoilPitchMax, float recoilPitchMin, float recoilYawMax, float recoilYawMin)
    {
        _gunStatus.fireModeIndex = fireModeIndex;

        _gunStatus.burstRepeat = burstRepeat;
        _gunStatus.burstCounter = burstCounter;

        _gunStatus.fireRate = fireRate;
        _gunStatus.fireTimer = fireTimer;

        _gunStatus.magazineSize = magazineSize;
        _gunStatus.magazine = magazine;

        _gunStatus.aimSpeed = aimSpeed;
        _gunStatus.aimFOV = aimFOV;

        _gunStatus.recoilPitchMax = recoilPitchMax;
        _gunStatus.recoilPitchMin = recoilPitchMin;
        _gunStatus.recoilYawMax = recoilYawMax;
        _gunStatus.recoilYawMin = recoilYawMin;
    }
    [PunRPC] protected override void RPC_Reset()
    {
        base.RPC_Reset();
        _gunStatus = _gunItemData.baseGunState;
        if (_gunStatus.aimFOV == 0) _gunStatus.aimFOV = 50f;
    }

}
