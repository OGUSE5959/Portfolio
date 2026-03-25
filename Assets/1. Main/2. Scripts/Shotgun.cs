using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    [Space]
    [SerializeField] AudioClip _shellIn;

    public override bool CanAim => !IsAim && !IsRun;

    void AnimEvent_ReloadOneShell()
    {
        if (Magazine >= MagazineSize || Inventory.Instance.AmmoCount(AmmoType) <= 0)
        {
            _animCtrl.Play(GunAnimCtrl.Motion.Idle);
            return;
        }
        PlaySFX(_shellIn); 
        Inventory.Instance.GetAmmo(AmmoType, 1);
        PlayerUI.Instance.SetAmmoInfoAll(++Magazine, AmmoType);
    }
    void AnimEvent_AfterOneShell()
    {
        if (CanReload)
            _animCtrl.Play(GunAnimCtrl.Motion.Reload);
        else _animCtrl.Play(GunAnimCtrl.Motion.Idle);
    }
    void AnimEvent_StartReloadOne() => _animCtrl.Play(GunAnimCtrl.Motion.Reload);

    protected override bool FireCondition()
    {
        if (Inventory.Instance.gameObject.activeSelf
            || GameMenu.Instance.gameObject.activeSelf || _isBlocked) return false;

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
    protected override void OnReload()
    {
        if (CanReload)
            _animCtrl.Play(GunAnimCtrl.Motion.ReloadNoAmmo);
    }

    protected override void HitSomething(RaycastHit hit)
    {
        // base.HitSomething(hit);
        BuckShot(hit);
    }
    protected override void MissedShot()
    {
        BuckShot(_mainCam.transform.position + _mainCam.transform.forward * 100f);
    }
    protected virtual Ray[] GetBuckRays(Vector3 start, Vector3 dir, int count, float spread/*, float dist = 100f*/)
    {
        if (count <= 0) return null;
        Ray[] rays = new Ray[count];
        spread = IsAim ? spread / 2f : spread;
        for (int i = 0; i < count; i++)
        {

            float angle = Random.Range(0f, spread);
            float azimuth = Random.Range(0f, 360f);

            Quaternion rotation = Quaternion.AngleAxis(angle, Random.onUnitSphere);
            Vector3 pelletDir = (rotation * dir).normalized;
            // Debug.Log(pelletDir);
            Ray ray = new Ray();
            ray.origin = start;
            ray.direction = pelletDir;

            rays[i] = ray;

        }
        return rays;
    }

    protected virtual void BuckShot(RaycastHit hit) => BuckShot(hit.point);
    protected virtual void BuckShot(Vector3 point)
    {
        Ray[] rays = GetBuckRays(_muzzleFireTr.position, Utility.GetNormalizedDir(point, _muzzleFireTr.position), 8, 5f);
        float lastDist = 10f;
        foreach (Ray ray in rays)
        {
            if (Physics.Raycast(ray, out RaycastHit hit1, 100f, _bulletLayer))
            {
                base.HitSomething(hit1);
                lastDist = Vector3.Distance(_muzzleFireTr.position, hit1.point);
            }
            else SyncedCreateBulletEffect(ray.origin, ray.direction * 100f, 2f);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
            // Debug.Log(ray.direction);
        }
        // Debug.Log("BuckShot");
    }

    public override void Action(bool lockInput = false)
    {
        base.Action(lockInput);
        if (IsReloading && IsRun)
            _animCtrl.SetBool("IsRun", true);
    }
}
