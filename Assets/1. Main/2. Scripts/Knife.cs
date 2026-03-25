using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Knife : MeleeWeapon
{
    KnifeAnimCtrl _animCtrl;
    AttackArea _attackArea;
    #region Animation Events
    void AnimEvent_SetIdle()
    {
        _animCtrl.Play(KnifeAnimCtrl.Motion.Idle);
    }
    void AnimEvent_Mow()
    {
        if (!_master.IsMe) return;
        /* foreach (var id in _attackArea.UnitList)
             id.SetHit(damage);*/
       Transform camTr = _master.CamCtrl.CameraTarget;
        Ray ray = new Ray(camTr.position, camTr.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << LayerMask.NameToLayer("OtherPlayer")))
        {
            if (hit.collider.gameObject.TryGetComponent<HitParts>(out HitParts parts))
            {
                // Debug.Log(parts.Master.gameObject.name, parts.Master.gameObject);
                parts.SetHit(_master, 30f, parts.tag);
                // Debug.Log("Hit!!" + _damage, hit.collider);
                SyncedMakeBleed(hit.point, Utility.GetNormalizedDir(hit.point, hit.collider.transform.position));
            }
        }
        Debug.DrawRay(ray.origin, ray.direction, Color.cyan, 2f);
    }
    #endregion
    #region Getter & Setter
    public KnifeAnimCtrl.Motion GetMotion => _animCtrl.GetMotion;
    public bool IsIdle => GetMotion == KnifeAnimCtrl.Motion.Idle || _animCtrl.IsState(KnifeAnimCtrl.Motion.Idle);
    public bool IsAttack => _animCtrl.IsState(KnifeAnimCtrl.Motion.Mow) || _animCtrl.IsState(KnifeAnimCtrl.Motion.Mow_1);
    #endregion

    public override void Initialize(PlayerController master)
    {
        base.Initialize(master);
    }
    public override void OnMouse0Down()
    {
        base.OnMouse0Down();
        if (!IsAttack)
        {
            _animCtrl.Play(KnifeAnimCtrl.Motion.Mow);
            _master.AnimCtrl.SetTrigger(AnimTrigger.Mow);
        }
    }
    public override void OnMouse1Down()
    {
        base.OnMouse1Down();
        if (!IsAttack)
        {
            _animCtrl.Play(KnifeAnimCtrl.Motion.Mow_1);
            _master.AnimCtrl.SetTrigger(AnimTrigger.Mow_1);
        }
    }   

    protected override void Awake()
    {
        base.Awake();
        _animCtrl = GetComponent<KnifeAnimCtrl>();
        _attackArea = GetComponentInChildren<AttackArea>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        _animCtrl.Play(KnifeAnimCtrl.Motion.Draw);
    }
    /*private void LateUpdate()
    {
        if (_pv.IsMine)
        {
            Transform ct = _master.CamCtrl.CameraTarget;
            _master.CamCtrl.CameraTarget.rotation
                = _master.transform.rotation; //Quaternion.Euler(ct.eulerAngles.x, _master.transform.eulerAngles.y, ct.eulerAngles.z);
        }
    }*/
}
