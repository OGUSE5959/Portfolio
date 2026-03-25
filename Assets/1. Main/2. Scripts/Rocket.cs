using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : SyncedMonoBehaviour
{
    RPG_7 _master;
    Rigidbody _rigid;
    [Space]
    [SerializeField] LayerMask _crashLayers;
    [SerializeField] float _speed;
    [SerializeField] float _effectRadius = 4f;
    Vector3 _dir;
    [Space]
    [SerializeField] float _maxDist;
    Vector3 _launchPos;
    [SerializeField] float _lifeTime;
    float _timer;
    public void Initialize(RPG_7 master)
    {
        _master = master;
        _rigid = GetComponent<Rigidbody>();
        _rigid.excludeLayers = ~_crashLayers;
    }
    public void Launch(Vector3 startPos, Vector3 dir)
    {
        SyncedEnable();
        transform.position = _launchPos = startPos;
        transform.forward = _dir = dir;
        _timer = 0;
    }
    void InsertIntoPool()
    {
        SyncedDisable();
        _master.Pool.Set(this);
    }
    public void Explode()
    {
        if (!_master || !_master.Master) return;
        _master.SyncedPlayOnSpot(transform.position);
        // Debug.Log("Explode", gameObject);
        Collider[] cols;
        List<IDamagable> hitMasters = new List<IDamagable>();
        if ((cols = Physics.OverlapSphere(transform.position, _effectRadius
            , 1 << LayerMask.NameToLayer("OtherPlayer") | 1 << LayerMask.NameToLayer("LocalPlayer"))).Length > 0)
            foreach (Collider col in cols)
                if (col.TryGetComponent<HitParts>(out HitParts parts))
                    if (!hitMasters.Contains(parts.Master)) hitMasters.Add(parts.Master);                
        foreach(IDamagable hitter in  hitMasters)
        {
            if (hitter.gameObject == _master.Master.gameObject)
            {
                if (hitter.gameObject.TryGetComponent<PlayerMoveController>(out PlayerMoveController moveCtrl))
                    moveCtrl.AddForce(Utility.GetNormalizedDir(moveCtrl.transform.position, transform.position) * 17.5f);
                hitter.SetHit(_master.Master, _master.Damage / 5f);
            }
            else hitter.SetHit(_master.Master, _master.Damage);
            if (hitter.IsDie)
            {
                _master.OnKill(hitter);
                PlayerUI.Instance.OnKill();
            } else PlayerUI.Instance.OnAttack();
        }
        CameraManager.Instance.SyncedNoticShake(transform.position, _effectRadius * 7f, 1f, 0.5f);
        EffectPool.Instance.SyncedCE_MeFirst("RPG7_Explosion", transform.position);
        InsertIntoPool();
    }
    private void Update()
    {
        if (!_pv.IsMine) return;
        float deltaTime = Time.deltaTime;
        transform.position += _dir * _speed * deltaTime;
        _timer += deltaTime;
        if( _timer >= _lifeTime
            || Vector3.Distance(_launchPos, transform.position) >= _maxDist)
            Explode();
    }
    private void OnTriggerEnter(Collider other)
    {
        Explode();
    }
}
