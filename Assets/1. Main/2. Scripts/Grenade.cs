using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;

[RequireComponent(typeof(GrenadeAnimCtrl))]
public class Grenade : Weapon
{
    GrenadeAnimCtrl _animCtrl;
    bool _isPinOut = false;
    [Space]
    [SerializeField] Transform _shape;
    [SerializeField] GrenadeUnit _currUnit;
    GameObjectPool<GrenadeUnit> _pool = new GameObjectPool<GrenadeUnit>();
    Rigidbody _shapeRigid;
    Vector3 _startPos;
    Quaternion _startRot;
    [Space]
    [SerializeField] float _boomWait;
    [SerializeField] float _throwPower;
    [Space]
    [SerializeField] LineRenderer _lr;
    [SerializeField] Material[] _lrMats;
    [SerializeField] GameObject _colSpot;
    [Space]
    [SerializeField] [Range(0, 100)] int _linePoints = 25;
    [SerializeField] [Range(0.0f, 0.25f)] float _timeBetweenPoints = 0.1f;
    Vector3 _predicHitPos;
    [Space]
    [SerializeField] public GameObject _cookingUI;
    [SerializeField] Image _timerFill;
    [SerializeField] Text _timerLabel;
    [SerializeField] Slider _waitToWaitSlider;
    Coroutine _coroutine_WaitToHold;
    [Space]
    [SerializeField] AudioClip[] _boomSFXs;
    [SerializeField] AS3DUnit _audioUnitPrefab;
    GameObjectPool<AS3DUnit> _audioPool = new GameObjectPool<AS3DUnit>();

    #region Getter & Setter
    public GrenadeAnimCtrl.Motion GetMotion => _animCtrl.GetMotion;
    public bool IsDraw => GetMotion == GrenadeAnimCtrl.Motion.Draw;
    public bool IsIdle => GetMotion == GrenadeAnimCtrl.Motion.Idle;
    public GameObjectPool<GrenadeUnit> Pool => _pool;
    public GrenadeUnit CurrUnit { get { return _currUnit; } set { _currUnit = value; } }
    public bool IsPinOut => _isPinOut;// GetMotion == GrenadeAnimCtrl.Motion.PinOut;
    #endregion
    #region
    void AnimEvent_SetIdle()
    {
        _animCtrl.Play(GrenadeAnimCtrl.Motion.Idle);
    }
    void AnimEvent_Throw()
    {
        _pv.RPC("RPC_Throw", RpcTarget.All);
    }
    /*void AnimEvent_Boom()
    {
        _pv.RPC("RPC_Boom", RpcTarget.All);
    }*/
    [PunRPC] void RPC_Throw()
    {
        //_rigid.isKinematic = false;
        //_rigid.AddForce(_master.transform.forward * _throwPower, ForceMode.Impulse);
    }
    [PunRPC] void RPC_Boom()
    {
        Debug.Log("Boom!!");
    }
    #endregion

    public override void Initialize(PlayerController master)
    {
        base.Initialize(master);
        _animCtrl = GetComponent<GrenadeAnimCtrl>();

        _lr = GetComponent<LineRenderer>();
        _shapeRigid = _shape.GetComponent<Rigidbody>();
        _shapeRigid.excludeLayers = ~(1 << LayerMask.NameToLayer("Map"));
        // _shapeRigid.includeLayers = 1 << LayerMask.NameToLayer("Map");
        _startPos = _shape.localPosition;
        _startRot = _shape.localRotation;

        _cookingUI.SetActive(false);
        _pool.CreatePool(2, () =>
        {
            GrenadeUnit unit = PhotonNetwork.Instantiate("PhotonPrefabs/GrenadeUnit", _shape.position, _shape.rotation)
            .GetComponent<GrenadeUnit>(); //Instantiate(_unitPrefab);
            if (GameManager.Instance) GameManager.Instance.RegistPvInst(unit.PV);
            unit.Initialize(this, unit.EffectRadius, 5f);
            unit.SyncedSetActive(false);
            return unit;
        });
        _audioPool.CreatePool(2, () =>
        {
            AS3DUnit unit = Instantiate(_audioUnitPrefab);
            unit.Initialize(_audioPool);
            unit.gameObject.SetActive(false);
            return unit;
        });
    }
    public override void ResetAll()
    {
        base.ResetAll();
        foreach(GrenadeUnit unit in _pool.Pool)
            unit.SyncedDisable();
    }
    public void SyncedPlayOnSpot(Vector3 spot)
    {
        byte index = (byte)Random.Range(0, _boomSFXs.Length);
        RPC_PlayOnSpot(index, spot);
        _pv.RPC("RPC_PlayOnSpot", RpcTarget.Others, index, spot);
    }
    public override void OnMouse0Down()
    {
        base.OnMouse0();
        if (!_currUnit) return;

        _isPinOut = true;
        _currUnit.PinOut();
    }
    public override void OnMouse0Up()
    {
        base.OnMouse0();
        if (IsPinOut)
        {
            Throw();
            WaitToHold(1f);
            // _animCtrl.Play(GrenadeAnimCtrl.Motion.Throw);
        }
    }
    public override void OnMouse1Down()
    {
        base.OnMouse1Down();
        if (IsPinOut)
        {
            _isPinOut = false;
            _currUnit.PinIn();
            // _lr.enabled = false;
        }
    }
    void DrawProjection()
    {
        if(!_lr.enabled) _lr.enabled = true;
        _lr.material = _isPinOut ? _lrMats[1] : _lrMats[0];
        _lr.positionCount = Mathf.CeilToInt(_linePoints / _timeBetweenPoints) + 1;
        _colSpot.SetActive(false);
        Vector3 startPosition = _shape.position;
        Vector3 startVelocity = 10f * Camera.main.transform.forward / _shapeRigid.mass;
        int i = 0;
        _lr.SetPosition(i, startPosition);
        Vector3 lastPoint = Vector3.zero;
        for(float time = 0f; time < _linePoints; time += _timeBetweenPoints)
        {
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * Mathf.Pow(time, 2));
            if(lastPoint != Vector3.zero)
            {
                Ray ray = new Ray(lastPoint, Utility.GetNormalizedDir(point, lastPoint));
                if (Physics.Raycast(ray, out RaycastHit hit
                    , Vector3.Distance(lastPoint, point), 1 << LayerMask.NameToLayer("Map")))
                {
                    _predicHitPos = hit.point;
                    _lr.SetPosition(i, _predicHitPos);
                    _lr.positionCount = i;
                    _colSpot.transform.position = _predicHitPos;
                    _colSpot.gameObject.SetActive(true);
                    return;
                }
            }
           
            lastPoint = point;
            _lr.SetPosition(i++, point);
        }
    }
    public void ResetHold(bool holdNew = false, float holdWait = 0.5f)
    {
        _isPinOut = false;
        if(_currUnit)
        {
            _currUnit.SyncedSetActive(false);
            _pool.Set(_currUnit);
            _currUnit = null;
        }
        CancelHold();
        _waitToWaitSlider.gameObject.SetActive(false);
        if(holdNew) WaitToHold(holdWait);
    }
    void HoldNew()
    {
        // if (_currUnit || !Inventory.Instance.GetThrowable(ThrowableType.Grenade)) return;
        GrenadeUnit unit = _pool.Get();
        /*if(unit.gameObject.activeSelf)
        {
            HoldNew();
            return;
        }*/
        _currUnit = unit;
        _currUnit.transform.SetParent(transform);
        _currUnit.SetKinematic(false);

        _currUnit.transform.localPosition = _startPos;
        _currUnit.transform.localRotation = _startRot;
        _currUnit.SyncedSetActive(true);
    }
    void Throw()
    {
        if (!_currUnit)
        {
            _isPinOut = false;
            return;
        }
        _cookingUI.SetActive(false);
        _isPinOut = false;
        _currUnit.transform.position = _shape.position;      
        _currUnit.SetKinematic(false);
        _currUnit.OnThrown(Camera.main.transform.forward, 10f);
        _currUnit = null;
    }
    public void WaitToHold(float wait = 2.5f)
    {
        if (_cookingUI.activeSelf) _cookingUI.SetActive(false);
        CancelHold();
        _coroutine_WaitToHold = StartCoroutine(Coroutine_WaitToHold(wait));
    }
    void CancelHold()
    {
        if (_coroutine_WaitToHold != null)
        {
            StopCoroutine(_coroutine_WaitToHold);
            _coroutine_WaitToHold = null;
        }
        if (_currUnit) _currUnit = null;
    }

    IEnumerator Coroutine_WaitToHold(float wait)
    {
        yield return Utility.GetWaitForSeconds(0.3f);
        _waitToWaitSlider.transform.localScale = Vector3.one;
        _waitToWaitSlider.gameObject.SetActive(true);
        float timer = 0f;
        while (true)
        {
            float deltaTime = Time.deltaTime;
            timer += deltaTime;
            if(timer >= wait)
            {
                HoldNew();
                _waitToWaitSlider.transform.DOScale(new Vector3(1.5f, 0f, 1f), 0.3f);
                yield return Utility.GetWaitForSeconds(0.3f);
                _waitToWaitSlider.gameObject.SetActive(false);
                yield break;
            }
            // UI
            _waitToWaitSlider.value = timer / wait;
            yield return null;
        }
    }

    [PunRPC] void RPC_PlayOnSpot(byte clipIndex, Vector3 spot)
    {
        AudioClip clip = _boomSFXs[clipIndex];
        AudioManager.Instance.PlayOnSpot(clip, spot);
    }

    protected override void Start()
    {
        base.Start();
        if (!_pv.IsMine) _lr.enabled = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_predicHitPos, 0.5f);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        if (!CanSync) return;

        //if (!_animCtrl)
        //{
        //    _animCtrl = GetComponent<GrenadeAnimCtrl>();
        //    _animCtrl.Initialize();
        //}
        if (!_currUnit) WaitToHold(0.3f);
        _animCtrl.Play(GrenadeAnimCtrl.Motion.Draw);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (!CanSync) return;
        if (_currUnit) ResetHold();
    }
    private void Update()
    {
        if (!CanSync) return;
        if (_currUnit && _currUnit)
        {
            _currUnit.transform.localPosition = _startPos; //SyncedSetPosition(_startPos, true);
            _currUnit.transform.localRotation = _startRot; //SyncedSetRotation(_startRot, true);
            if(_isPinOut && _currUnit.TimeValue >= 0.1f)
            {
                float timeVal = _currUnit.TimeValue;
                float leftVal = _currUnit.LeftTimeVal;
                if (!_cookingUI.activeSelf) _cookingUI.SetActive(true);

                _timerFill.fillAmount = leftVal;
                _timerLabel.text = _currUnit.LeftTime.ToString("0.0");
                _timerLabel.color = new Color(timeVal, leftVal, leftVal);
            } else if(_cookingUI.activeSelf) _cookingUI.SetActive(false);
        }
        DrawProjection();
    }
}
