using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Flashbang : Weapon
{
    [Space]
    [SerializeField] Transform _shape;
    [SerializeField] FlashbangUnit _currUnit;
    GameObjectPool<FlashbangUnit> _pool = new GameObjectPool<FlashbangUnit>();
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

    public GameObjectPool<FlashbangUnit> Pool => _pool;

    public override void Initialize(PlayerController master)
    {
        base.Initialize(master);

        _lr = GetComponent<LineRenderer>();
        // _shapeRigid.includeLayers = 1 << LayerMask.NameToLayer("Map");
        _startPos = _shape.localPosition;
        _startRot = _shape.localRotation;

        _pool.CreatePool(2, () =>
        {
            FlashbangUnit unit = PhotonNetwork.Instantiate("PhotonPrefabs/FlashbangUnit"
                , _shape.position, _shape.rotation).GetComponent<FlashbangUnit>();
            if (GameManager.Instance) GameManager.Instance.RegistPvInst(unit.PV);
            unit.Initialize(this);
            unit.SyncedSetActive(false);
            return unit;
        });
    }
    public void ResetHold(float holdWait = 0.5f)
    {
        if (_currUnit)
        {
            _currUnit.SyncedSetActive(false);
            _pool.Set(_currUnit);
            _currUnit = null;
        }
        Invoke("HoldNew", holdWait);
    }
    void HoldNew()
    {
        _currUnit = _pool.Get();
        _currUnit.transform.SetParent(transform);
        _currUnit.SetKinematic(false);

        _currUnit.transform.localPosition = _startPos;
        _currUnit.transform.localRotation = _startRot;
        _currUnit.gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
