using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashbangUnit : SyncedMonoBehaviour, IThrowable
{
    public ThrowableType ID => ThrowableType.FlashBang;
    Flashbang _master;
    ThrowableRanger _ranger;
    bool _isThrown = false;
    Rigidbody _rb;
    [SerializeField] float _effectRadius;
    [SerializeField] float _waitTime = 5f;
    Dictionary<IDamagable, List<HitParts>> _masterAndpartList = new Dictionary<IDamagable, List<HitParts>>();
    float _timer;
    Coroutine _coroutine_Bang;

    public bool IsMe => _master.Master.IsMe;
    public float EffectRadius => _effectRadius;
    public float DangerRadius => 0f;

    public void Initialize(Flashbang master, float effectRadius = -1f, float waitTime = -1f)
    {
        _master = master;
        _effectRadius = effectRadius < 0f ? _effectRadius : effectRadius;
        _waitTime = waitTime < 0f ? _waitTime : waitTime;

        /*_ranger = GetComponentInChildren<ThrowableRanger>(true);
        _ranger.Initialize(this, _effectRadius);*/
    }
    public void SetKinematic(bool value)
    {
        if (!_rb.isKinematic) _rb.velocity = Vector3.zero;
        _rb.isKinematic = value;
        _rb.useGravity = !value;
    }
    public void CancelBang()
    {
        if (_coroutine_Bang != null)
        {
            StopCoroutine(_coroutine_Bang);
            _coroutine_Bang = null;
        }
    }
    public void OnThrown(Vector3 dir, float force)
    {
        _coroutine_Bang = StartCoroutine(Coroutine_Bang(_waitTime));
        _isThrown = true;
        if (transform.parent != null) transform.SetParent(null);
        // if (_coroutine_Boom == null) _coroutine_Boom = StartCoroutine(Coroutine_Boom());
        SyncedSetActive(true);
        _rb.AddForce(dir * force, ForceMode.Impulse);
        UpdateShowIndiTarget();
    }
    void UpdateShowIndiTarget()
    {
        if (!_isThrown) return;
        foreach (IDamagable key in _masterAndpartList.Keys)
            if (key.PV.IsMine && key.gameObject
               .TryGetComponent<PlayerController>(out PlayerController plyCtrl))
                plyCtrl.UI.AddThrowIndi(this);
    }
    public void OnDetectParts(HitParts parts)
    {
        Debug.Log("OnDetectParts");
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
    void Bang()
    {
        Debug.Log("Bang");

        foreach (var pair in _masterAndpartList)
        {
            IDamagable key = pair.Key;
            if (key.PV.IsMine && key.gameObject
                .TryGetComponent<PlayerController>(out PlayerController plyCtrl))
                plyCtrl.UI.RemoveThrowIndi(this);
        }
        gameObject.SetActive(false);
        SetKinematic(true);
        _master.Pool.Set(this);
        _masterAndpartList.Clear();
    }

    IEnumerator Coroutine_Bang(float wait)
    {
        yield return Utility.GetWaitForSeconds(wait);
        Bang();
    }

    private void OnDisable()
    {
        CancelBang();
        StopAllCoroutines();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
}
