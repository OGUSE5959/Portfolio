using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : SyncedMonoBehaviour
{
    PhotonTransformView _ptv;
    PhotonRigidbodyView _prv;
    Vector3 _syncedVelocity;

    [SerializeField] AmmoType _type;
    public AmmoType BulletType => _type; 
    BulletPoolManager _bpm;
    int _shooterID;
    Rigidbody _rigid;
    [SerializeField] float _speed;
    [SerializeField] float _damage;
    [Space]
    [SerializeField] LayerMask _hitLayer;
    [SerializeField] float _lifeTime;
    float _lifeCounter;
    [SerializeField] float _maxDist;
    Vector3 _startPos;

    public void Initialize(int shooterID, float damage = 10f)
    {
        ResetBullet();
        if(PV.IsMine) PV.RPC("RPC_Initialize", RpcTarget.All, shooterID, transform.position, damage);
    }
    public void ResetBullet()
    {
        if (PV.IsMine)
            PV.RPC("RPC_ResetBullet", RpcTarget.All);
    }
    public void InsertIntoPool()
    {
        SyncedDisable();
        _bpm.SetBullet(_type, this);
    }
    [PunRPC] void RPC_ResetBullet()
    {
        _lifeCounter = 0f;
        if (!_rigid.isKinematic)
            _rigid.velocity = Vector3.zero;
    }
    [PunRPC] void RPC_Initialize(int shooterID, Vector3 startPos, float damage)
    {
        _shooterID = shooterID;
        _startPos = startPos;
        _damage = damage;
    }
    [PunRPC] void RPC_CreateEffect(string effectName, Vector3 position)
    {
        var effect = EffectPool.Instance.CreateEffect("BImp_Wood", position);
        effect.transform.forward = -transform.forward;
    }

    protected override void Awake()
    {
        base.Awake();
        _ptv = GetComponent<PhotonTransformView>();
        _prv = GetComponent<PhotonRigidbodyView>();

        _rigid = GetComponent<Rigidbody>();
        if (_hitLayer == 0)
            _hitLayer = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Map") | 1 << LayerMask.NameToLayer("Player");
        _rigid.includeLayers = _hitLayer;
        _rigid.excludeLayers = ~_hitLayer;   
    }
    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        
    }

    void Start()
    {
        _bpm = BulletPoolManager.Instance;
    }
    void Update()
    {
        if(_pv.IsMine)
        {
            float deltaTime = Time.deltaTime;

            _lifeCounter += deltaTime;
            if (_lifeCounter >= _lifeTime)
                InsertIntoPool();
            float dist = Vector3.Distance(transform.position, _startPos);
            if (dist >= _maxDist)
                InsertIntoPool();

            // transform.position += transform.forward * _speed * deltaTime;
            _rigid.AddForce(transform.forward * _speed, ForceMode.Force);
        }
        /*else
        {
            _rigid.velocity = _syncedVelocity;
        }*/
    }

    private void OnTriggerEnter(Collider other)
    {
        var contect = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        if (_pv.IsMine)
        {
            if(other.gameObject.TryGetComponent<IDamagable>(out IDamagable id) && id.PV.ViewID != _shooterID)
            {
                Debug.Log(id.gameObject.name, other);
                // id.SetHit(_sh, _damage, other.tag);
                _pv.RPC("RPC_CreateEffect", RpcTarget.All, "BImp_SoftBody", contect);
                SyncedSetActive(false);
            }
            /*if (other.gameObject.TryGetComponent<PlayerController>(out PlayerController player)
                    && player.PV.ViewID != _shooter.PV.ViewID)
            {
                // Debug.Log(other.name, other);
                player.Health.SetHit(_damage);
                _pv.RPC("RPC_CreateEffect", RpcTarget.All, "BImp_SoftBody", contect);
                SyncedSetActive(false);
                *//* if (player.Health.IsDie)    // 죽을 때 또맞아서 버그걸림 => 수정 필요
                    _shooter.SetWin();*//*
            }*/
            else
                _pv.RPC("RPC_CreateEffect", RpcTarget.All, "BImp_Wood", contect);
        }
        
        // Debug.Log(other.gameObject.name);
        // gameObject.SetActive(false);
    }
}
