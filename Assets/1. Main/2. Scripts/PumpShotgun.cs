using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PumpShotgun : Shotgun
{
    [Space]
    [SerializeField] bool _isFilled;
    [SerializeField] Text _pumpCheck;

    void AnimEvent_Pumped()
    {
        SetPumped(true);
    }
    void AnimEvent_AddBuckshot()
    {
        int ammoCnt = Inventory.Instance.AmmoCount(AmmoType);
        if (ammoCnt <= 0) _animCtrl.Play(GunAnimCtrl.Motion.Idle, false);
        PlayerUI.Instance.SetMagazineTxt(++Magazine);
        PlayerUI.Instance.SetTotalAmmoTxt(ammoCnt);
    }
    void SetPumped(bool value)
    {
        if(value)
        {
            _isFilled = true;
            _pumpCheck.text = "¹÷¼¦ Ã¤¿öÁü!!";
            _pumpCheck.color = Color.yellow;
        }
        else
        {
            _isFilled = false;
            _pumpCheck.text = "¹÷¼¦ ºñ¿öÁü";
            _pumpCheck.color = Color.white;
        }
    }
    protected override void SyncedScanFire()
    {
        if (!_isFilled) return;
        base.SyncedScanFire();
        SetPumped(false);
    }
    protected override bool FireCondition()
    {
        if (Inventory.Instance.gameObject.activeSelf
            || GameMenu.Instance.gameObject.activeSelf
            || !_isFilled || _isBlocked) return false;

        var mouse0 = _master.Inputter.ActionMaps.Weapon.Mouse0;
        bool getMouseDown = mouse0.WasPressedThisFrame();
        bool fireCoolDown = FireTimer >= 1 / FireRate;
        bool isRun = _master.MoveCtrl.IsSprint;

        if (Magazine > 0 && !isRun)
        {
            switch (FireMode)
            {
                case FireMode.Single:
                    if (fireCoolDown)
                    {
                        if (getMouseDown)
                        {
                            FireTimer = 0f;
                            return true;
                        }
                    }
                    return false;
                case FireMode.SemiAuto: return getMouseDown;
            }
        }
        return false;
    }
}
