using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AWM : Gun
{
    [SerializeField] bool _isAmmoInChamber = true;

    public override bool IsReloading => GetMotion == GunAnimCtrl.Motion.ReloadNoAmmo;

    void AnimEvent_SetChamber()
    {
        _isAmmoInChamber = true;
        CreateCartridgeCase();
    }
    void AnimEvent_TrySetChamber()
    {
        if (GetMotion == GunAnimCtrl.Motion.Reload) return;
        if (!_isAmmoInChamber)
        {
            BoltAction();
            return;
        }
    }

    void BoltAction() => _animCtrl.Play(GunAnimCtrl.Motion.Reload);
    protected override void OnReload()
    {
        if (!_pv.IsMine || IsReloading || _master.WeaponCtrl.AmmoCount(AmmoType) == 0) return;
        if (Magazine < MagazineSize)
        {
            _animCtrl.Play(GunAnimCtrl.Motion.ReloadNoAmmo);
            SyncedPlaySFX(SFXType.RelaodNoAmmo);
        }
    }
    protected override bool FireCondition()
    {
        if (!_isAmmoInChamber) return false;
        return base.FireCondition();
    }
    protected override void CreateCartridgeCase()
    {
        if(!_isAmmoInChamber) { return; }
        base.CreateCartridgeCase();
    }
    protected override void SyncedScanFire()
    {
        if (!_isAmmoInChamber)
        {
            BoltAction();
            return;
        }
        _isAmmoInChamber = false;
        base.SyncedScanFire();
        if (Magazine != 0) BoltAction();
    }
}
