#define NotWeapon

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class FieldItem : SyncedMonoBehaviour, IInteractable
{
    /*protected Item _item;
    public virtual Item Item => _item;*/

    [SerializeField] protected ItemData _itemData;  // 파생 클래스의 데이타를 여기에 넣을까 말까?
    public virtual ItemData StartData => _itemData;
    public virtual ItemData Data => _itemData;
    public string Name => Data.itemName;
    public string Purpose => " 줍기";
    public InteractType InteractType => InteractType.Item;

    [SerializeField] public ItemType ItemType => _itemData.ItemType;
    // public Type classType => GetType();
    protected string _itemName;
    protected int _count;
    protected Sprite _icon;
    protected ItemListUnit _listUnit;

    [Space]
    [SerializeField] protected Collider _collider;
    [SerializeField] protected Rigidbody _rigid;
    Vector3 _startPos, _startDir;

    Material[] _materials;
    protected float _thickness = 0.1f;
    protected float _dropRoll;

    public int Count { get { /*ListUnit.UpdateCount();*/ return _count; } set { SyncedSetCount(value); } }
    public ItemListUnit ListUnit { get { return _listUnit; } }
    Material[] IInteractable.Materials => _materials;
    public Inventory Inven => Inventory.Instance;

    /*void IInteractable.OnInteracted(IInteractor interactor)
    {
        OnInteracted(interactor);
    }*/

    public virtual void Initialize(ItemData data)
    {
        // _itemType = data.ItemType;
        _itemName = data.itemName;
        _count = data.count;
        _icon = data.icon;
        if (IsMasterClient)
        {
            _listUnit = ItemListUnitPool.Instance.CreateUnit(this);
            _pv.RPC("RPC_SetListUnit", RpcTarget.Others, _listUnit.PV.ViewID);
            _listUnit.SyncedDisable();
        }
    }   
    public virtual void OnInteracted(IInteractor interactor)
    {
        Debug.Log("OnInteracted With Item : " + gameObject.name, interactor.gameObject);
        OnPicked(interactor);
    }
    public virtual void OnPicked(IInteractor interactor)
    {
        Debug.Log("OnPicked");
        if (interactor.PV.IsMine) // return;
        {
            // Debug.Log("MoveToBag");
            Inventory.Instance.MoveToBag(this);
            SyncedDisable();
        }
    }
    public virtual void OnThrown(Vector3 dir, float force)
    {
        _pv.RPC("RPC_OnThrown", RpcTarget.All, dir, force);
    }
    void SyncedSetCount(int count)
    {
        RPC_SetCount(count);
        _pv.RPC("RPC_SetCount", RpcTarget.Others, count);
    }
    public virtual void SyncedReset()
    {        
        SyncedEnable();
        if (_listUnit) _listUnit.SyncedDisable();
        _pv.RPC("RPC_Reset", RpcTarget.All);
    }
    public void SyncedDisableWithUI()
    {
        SyncedDisable();
        if (ListUnit)  ListUnit.SyncedDisable();
    }

    [PunRPC] protected virtual void RPC_Reset()
    {
        transform.position = _startPos;
        transform.forward = _startDir;
        transform.rotation 
            = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, _dropRoll));
    }
    [PunRPC] protected void RPC_SetListUnit(int viewID)
    {
        if (!IsMasterClient)
        {
            _listUnit = PhotonView.Find(viewID).GetComponent<ItemListUnit>();
                // ItemListUnitPool.Instance.UnitList[viewID];
            _listUnit.Initialize(this);
        }
    }
    [PunRPC] protected void RPC_SetCount(int count)
    {
        _count = count; 
        if (ListUnit) ListUnit.UpdateCount();
    }
    public void SetUIActive(bool value)
    {
        if (ListUnit != null) 
            ListUnit.gameObject.SetActive(value);
        // _pv.RPC("RPC_SetUIActive", RpcTarget.All, value);
    }
    public void SetInField()
    {
        if (ListUnit)
            ListUnit.transform.SetParent(Inven.FieldTr);// _pv.RPC("RPC_SetInField", RpcTarget.All);
    }
    public void SetInBag()
    {
        ListUnit.transform.SetParent(Inven.BagTr);// _pv.RPC("RPC_SetInBag", RpcTarget.All);
        ListUnit.gameObject.SetActive(true);
        ListUnit.HideToOthers();
    }
    [PunRPC] protected virtual void RPC_OnThrown(Vector3 dir, float force)
    {
        if (!PV.IsMine) return;
        SyncedEnable();
        _rigid.useGravity = true;
        _rigid.isKinematic = false;
        _rigid.AddForce(dir * force, ForceMode.Impulse);
    }
    [PunRPC] protected virtual void RPC_SetUIActive(bool value)
    {
        if (ListUnit)
            ListUnit.SyncedSetActive(value);
        else Debug.LogWarning("No ListUnit!!");
    }
    [PunRPC] protected virtual void RPC_SetInField()
    {
        if (ListUnit)
            ListUnit.transform.SetParent(Inventory.Instance.FieldGrid.transform);
        else Debug.LogWarning("No ListUnit!!");
    }
    [PunRPC] protected virtual void RPC_SetInBag()
    {
        if (ListUnit)
            ListUnit.transform.SetParent(Inventory.Instance.BagGrid.transform);
        else Debug.LogWarning("No ListUnit!!");
    }

    protected virtual void OnDestroy()
    {
        if (ListUnit)
        {
            Destroy(ListUnit);
        }
    }
    protected virtual void OnEnable()
    {
        // if(_listUnit && IsMasterClient) _listUnit.SyncedSetActive(true);
    }
    protected virtual void OnDisable()
    {
        // if (_listUnit) _listUnit.SyncedDisable();
    }

    protected override void Awake()
    {
        base.Awake();

        var renderers = GetComponentsInChildren<Renderer>();
        // Debug.Log(0, InteractableManager.Instance);
        _collider = GetComponent<Collider>();
        _rigid = GetComponent<Rigidbody>();
        _rigid.excludeLayers = ~(1 << LayerMask.NameToLayer("Map") | 1 << LayerMask.NameToLayer("DetectArea"));
        if (!PV.IsMine)
        {
            _collider.isTrigger = true;
            _rigid.useGravity = false;
            _rigid.isKinematic = true;
        }

        _startPos = transform.position;
        _startDir = transform.forward;
        // _materials = 
        // _itemData = Instantiate(_itemData);
        // Initialize(StartData);
    }
    void Start()
    {
        InteractableManager im = InteractableManager.Instance;
        if (im) im.SetInteractable(this);
        Initialize(StartData);
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
    private void OnCollisionEnter(Collision collision)
    {
        if (!PV.IsMine) return;
        if (!_rigid || _rigid.isKinematic) return;
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Debug.Log(_thickness);
            Vector3 newPos = transform.position; newPos.y = 0;
            transform.position = newPos;
            transform.position += Vector3.up * _thickness;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, _dropRoll));

            _rigid.useGravity = false;
            _rigid.isKinematic = true;
        }
    }
    /*private void OnTriggerExit(Collider other)
    {

    }*/
}
