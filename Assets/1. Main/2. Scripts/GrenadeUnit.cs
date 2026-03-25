using System.Collections;
using System.Collections.Generic;
using System.Linq;  
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class GrenadeUnit : SyncedMonoBehaviour, IThrowable
{
    enum Motion
    {
        Idle,

        PinOut,
        PinIn,
        Thrown,

        Max
    }
    public ThrowableType ID => ThrowableType.Grenade;
    Dictionary<Motion, int> _hashTable = new Dictionary<Motion, int>();
    bool _isThrow = false;
    Grenade _master;
    Animator _anim;
    int _preHash = -1;
    Rigidbody _rb;
    [SerializeField] float _effectRadius;
    Dictionary<IDamagable, List<HitParts>> _masterAndpartList = new Dictionary<IDamagable, List<HitParts>>();
    List<IDamagable> _indicatorTargetList = new List<IDamagable>();
    ThrowableRanger _ranger;
    [SerializeField] Transform _pin;
    [SerializeField] float _waitTime = 5f;
    float _timer;
    Coroutine _coroutine_Boom;

    public bool IsMe => _master.Master.IsMe;
    public float EffectRadius => _effectRadius;
    public float DangerRadius => _effectRadius / 3f;
    public float TimeValue => _timer / _waitTime;
    public float LeftTimeVal => 1f - TimeValue;
    public float LeftTime => _waitTime - _timer;

    public void Initialize(Grenade master, float effectRadius = -1f, float waitTime = -1f)
    {
        _master = master;
        _effectRadius = effectRadius < 0f ? _effectRadius : effectRadius; 
        _waitTime = waitTime < 0f ? _waitTime : waitTime;

        _ranger = GetComponentInChildren<ThrowableRanger>(true);
        if (PV.IsMine) _ranger.Initialize(this, _effectRadius);
        else _ranger.gameObject.SetActive(false);
    }
    public void CancelBoom()
    {
        if (_coroutine_Boom != null)
        {
            StopCoroutine(_coroutine_Boom);
            _coroutine_Boom = null;
        }
    }
    void PlayAnim(Motion motion, bool isBlend = true)
    {
        int hash = _hashTable[motion];
        if (_preHash != -1)
            _anim.ResetTrigger(hash);
        if(isBlend) _anim.SetTrigger(hash);
        else _anim.Play(hash);
        _preHash = hash;
    }
    public void SetKinematic(bool value)
    {
        if(!_rb.isKinematic) _rb.velocity = Vector3.zero;
        _rb.isKinematic = value;
        _rb.useGravity = !value;
    }
    public void OnThrown(Vector3 dir, float force)
    {
        _isThrow = true;
        if(transform.parent != null) transform.SetParent(null);
        // if (_coroutine_Boom == null) _coroutine_Boom = StartCoroutine(Coroutine_Boom());
        _pin.gameObject.SetActive(false);
        SyncedSetActive(true);
        PlayAnim(Motion.Thrown);
        _rb.AddForce(dir * force, ForceMode.Impulse);
        UpdateShowIndiTarget();
    }
    public void PinOut()
    {
        PlayAnim(Motion.PinOut);
        CancelBoom();
        _coroutine_Boom = StartCoroutine(Coroutine_Boom());
    }
    public void PinIn()
    {
        PlayAnim(Motion.PinIn);
        CancelBoom();
    }
    public void OnDetectParts(HitParts parts)
    {
        IDamagable dmgMaster = parts.Master;
        if (!_masterAndpartList.ContainsKey(dmgMaster))
        {
            List<HitParts> partsList = new List<HitParts>();
            partsList.Add(parts);
            _masterAndpartList.Add(dmgMaster, partsList);
            UpdateShowIndiTarget();
        }
        else _masterAndpartList[dmgMaster].Add(parts);
    }
    public void OnReleaseParts(HitParts parts)
    {
        if (_masterAndpartList.ContainsKey(parts.Master))
            _masterAndpartList[parts.Master].Remove(parts);
    }
    void Boom()
    {
        _master.SyncedPlayOnSpot(transform.position);
        if (_master.CurrUnit != null && _master.CurrUnit == this)
        {
            _master.CurrUnit = null;
            _master.ResetHold(true, 1.5f);
        }
            
        Vector3 tp = transform.position;
        EffectPool.Instance.SyncedCE_MeFirst("Grenade_Boom", transform.position);
        CameraManager.Instance.SyncedNoticShake(tp, _effectRadius * 3f, 1.5f, 0.5f);
        foreach (var pair in _masterAndpartList)
        {
            IDamagable key = pair.Key;  // 사실상 맞는 애들 다 PlayerCtrl인데 설계 잘못한거 같기도 하고..
            foreach (HitParts parts in pair.Value)
            {
                float dist = Vector3.Distance(parts.transform.position, tp);
                Ray ray = new Ray(tp, Utility.GetNormalizedDir(parts.transform.position, tp));
                bool isBlocked = Physics.Raycast(ray, /*out RaycastHit hit,*/ dist, 1 << LayerMask.NameToLayer("Map"));

                if (isBlocked) continue;
                PlayerController myPlayer = _master.Master;
                if (key.gameObject == myPlayer.gameObject)
                {
                    float absDmg = myPlayer.Health.HP * 0.99f;
                    key.SetHit(myPlayer, absDmg);
                    myPlayer.MoveCtrl.AddForce(-myPlayer.CamCtrl.CameraTarget.forward * 20f);
                }
                else
                {
                    float maxDamage = 100f;
                    float exponent = 2f;
                    float normalizedDistance = Mathf.Clamp01(dist / _effectRadius);
                    float damage = maxDamage * Mathf.Pow(1 - normalizedDistance, exponent);
                    parts.SetHit(_master.Master, damage);
                }                              
                if (parts.IsDie) _master.OnKill(parts.Master);
                break;
            }

            if (!_indicatorTargetList.Contains(key)) continue;

            if (key.PV && key.PV.IsMine) PlayerUI.Instance.RemoveThrowIndi(this);
            else PlayerUI.Instance.SyncedRemoveThrowIndi(key, this);
            _indicatorTargetList.Remove(key);
        }
            
        SetKinematic(true);
        SyncedSetActive(false);
        _master.Pool.Set(this);
    }
    void UpdateShowIndiTarget()
    {
        if (!_isThrow) return;
        foreach (IDamagable key in _masterAndpartList.Keys)
            if(!_indicatorTargetList.Contains(key))
            {
                if (!key.PV) continue;
                _indicatorTargetList.Add(key);  // key.PV.IsMine Or _master.Master.IsMe
                if (key.PV.IsMine) PlayerUI.Instance.AddThrowIndi(this);
                else PlayerUI.Instance.SyncedAddThrowIndi(key, this);
            }
    }

    IEnumerator Coroutine_Boom()
    {
        _timer = 0f;
        while (true)
        {
            _timer += Time.deltaTime;
            if(_timer >= _waitTime)
            {
                _timer = 0f;
                _coroutine_Boom = null;
                Boom();
                yield break;
            }
            yield return null;
        }
    }

    private void OnEnable()
    {
        if (!_master || !_master.PV 
            || !_master.PV.IsMine) return;
        // CancelBoom();

        _pin.gameObject.SetActive(true);    // 가능하면 이런 동기화는 줄이자!
        _isThrow = false;
        _masterAndpartList.Clear();
        PlayAnim(Motion.Idle);
    }
    private void OnDisable()
    {
        CancelBoom();
        StopAllCoroutines();
    }
    protected override void Awake()
    {
        base.Awake();
        for(int i = 0; i < (int)Motion.Max; i++)
        {
            Motion motion = (Motion)i;
            int hash = Animator.StringToHash(motion.ToString());
            _hashTable.Add(motion, hash);
        }
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _rb.excludeLayers = ~(1 << LayerMask.NameToLayer("Map"));
    }
}
