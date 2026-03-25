using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView), typeof(PhotonAnimatorView), typeof(AudioSource))]
public class Weapon : SyncedMonoBehaviour
{
    [SerializeField] WeaponID _id;

    protected GameManager _gm;
    protected PlayerController _master;
    public bool CanSync => _master && _master.IsMe;
    protected float _damage;

    protected AudioSource _audioSFX;
    [Space]
    public Transform rightHandTarget;
    public Transform leftHandTarget;

    public WeaponID ID => _id;
    public PlayerController Master => _master;
    public Transform Root => _master.WeaponCtrl.WeaponRoot;
    public bool IsMove => _master.MoveCtrl.IsMove;
    public virtual bool IsRun => _master.MoveCtrl.IsSprint;
    public float Damage => _damage;

    public virtual void Initialize(PlayerController master)
    {
        _master = master;
        if (!_master) _master = GetComponentInParent<PlayerController>();
        if (!_pv) _pv = GetComponent<PhotonView>();

        if (!TryGetComponent<AudioSource>(out _audioSFX))
            _audioSFX = gameObject.AddComponent<AudioSource>();
        _audioSFX.playOnAwake = false;
        _audioSFX.spatialBlend = 1f;
        _audioSFX.outputAudioMixerGroup = AudioManager.Instance.Group_SFX;
    }
    public virtual void SetUp(WeaponItemData data)
    {
        _damage = data.damage;
    }
    public virtual void ResetAll() { }
    public virtual void OnHold() { }
    public virtual void Action(bool lockInput = false)
    {
        if (!_pv.IsMine) return;
    }

    public virtual void OnB() { }
    public virtual void OnR() { }
    public virtual void OnMouse0Down() { }
    public virtual void OnMouse0() { }
    public virtual void OnMouse0Up() { }
    public virtual void OnMouse1Down() { }
    public virtual void OnMouse1() { }
    public virtual void OnMouse1Up() { }

    protected void PlaySFX(AudioClip clip)
    {
        // Debug.Log((_audioSFX == null) + ", " + clip == null);
        // _audioSFX.clip = clip;
        if (clip != null) _audioSFX.PlayOneShot(clip);
    }
    public virtual void OnKill(PlayerController victim)
    {
        GameManager.Instance.OnPlayerKill(_master, victim);
        PlayerUI.Instance.OnKill();
    }
    public virtual void OnKill(IDamagable victim)
    {
        GameManager.Instance.OnPlayerKill(_master, victim);
        PlayerUI.Instance.OnKill();
    }
    protected void SyncedMakeBleed(Vector3 point, Vector3 dir)
    {
        EffectPool.Instance.CreateEffect("BImp_SoftBody", point).transform.forward = dir;
        _pv.RPC("RPC_MakeBleed", RpcTarget.Others, point, dir);
    }
    [PunRPC] protected void RPC_MakeBleed(Vector3 point, Vector3 dir)
        => EffectPool.Instance.CreateEffect("BImp_SoftBody", point).transform.forward = dir;

    protected virtual void OnEnable()
    {
        if (_master)
            _master.AnimCtrl.IKControl.SetHandTargets(rightHandTarget, leftHandTarget);
    }
    protected virtual void OnDisable()
    {
        if (_master)
            _master.AnimCtrl.IKControl.SetHandTargets();
    }
    protected override void Awake()
    {
        base.Awake();
        // _pv = GetComponent<PhotonView>();
        _gm = GameManager.Instance;
        _master = GetComponentInParent<PlayerController>();
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }
}
