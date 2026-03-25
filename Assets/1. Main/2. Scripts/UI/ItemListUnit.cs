using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Unity.VisualScripting;

public class ItemListUnit : SyncedMonoBehaviour, IPointerClickHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    Inventory _inven;
    [SerializeField] FieldItem _fieldBody;
    public FieldItem FieldBody => _fieldBody;
    public ItemData Data => _fieldBody.Data;
    public bool IsOnField => transform.parent == _inven.FieldTr;// _fieldBody.gameObject.activeSelf;
    [SerializeField] Image _icon;
    [SerializeField] Text _name;
    [SerializeField] int _count;
    [SerializeField] Text _countLabel;

    #region MouseEvents
    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsOnField)
            FieldBody.OnPicked(_inven.Master.Interacter);
        else
        {
            switch(eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    _inven.PutDownOnField(this); break;
                case PointerEventData.InputButton.Right:
                    _inven.OnMouseDown1(this); break;
            }
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        // Debug.Log("Drag");
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("Enter");
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("Exit");
    }
    #endregion

    public void Initialize(FieldItem item)
    {
        _fieldBody = item;
        ItemData data = item.StartData;

        _icon.sprite = data.icon;
        _name.text = data.name;
        _count = data.count;
        _countLabel.text = /*item.ItemType == ItemType.Weapon ? "" :*/ data.count.ToString();
    }
    public void HideCount() => _countLabel.text = string.Empty;
    /*public void Initialize(ItemData data)
    {
        _icon.sprite = data.icon;
        _name.text = data.name;
        _count = data.count;
        _countLabel.text = data.count.ToString();
    }*/
    public void Add(int add)
    {
        _count += add;
        _countLabel.text = _count.ToString();
    }
    public void Discount(int discount)
    {
        if(discount > _count) { discount = _count; Debug.LogWarning("Discount is more than whole count of Item"); }
        _count -= discount;
        _countLabel.text = _count.ToString();
    }
    public void UpdateCount()      // 굳이 RPC?
    {
        RPC_UpdateCount();
        // _pv.RPC("RPC_UpdateCount", RpcTarget.Others);
    }
    public void HideToOthers() => _pv.RPC("RPC_Disable", RpcTarget.Others);     // 종속된 필드 아이템을 가방에 넣어 비활성화 되고 다른 클라에서만 안보이게
    [PunRPC] void RPC_UpdateCount()
    {
        _count = _fieldBody.Count;
        _icon.sprite = Data.icon;
        _name.text = Data.name;
        _countLabel.text = Data.ItemType == ItemType.Weapon ? "" : _count.ToString();
    }

    public void InsertIntoPool()
    {
        ItemListUnitPool pool = ItemListUnitPool.Instance;
        if (pool)
        {
            pool.SetUnit(this);
            // transform.SetParent(pool.transform);
        }
    }
    protected override void Awake()
    {
        base.Awake();
        ItemListUnitPool.Instance.AddUnit(this);
        if(_pv.IsMine) SyncedDisable();
    }
    // Start is called before the first frame update
    void Start()
    {
        _inven = Inventory.Instance;
    }
    private void OnDisable()
    {
        ItemListUnitPool pool = ItemListUnitPool.Instance;
        if (pool)
        {
            // pool.SetUnit(this);
            // transform.SetParent(pool.transform);
        }
    }
    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
