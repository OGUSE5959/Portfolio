using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FieldWeapon : FieldItem
{
    WeaponItemData _weaponItem;
    public override ItemData Data { get { return _weaponItem == null ? _itemData : _weaponItem; } }
    public WeaponItemData WeaponData { get {
            if (!_weaponItem)
            {
                _weaponItem = _itemData as WeaponItemData;
                Debug.Log(_weaponItem);
            }
            return _weaponItem; } }

    public WeaponUsage Usage => WeaponData.usage;   // _weaponItemData가 할당이 안됐을수도 있으니 getter를 쓰자
    public WeaponID WeaponID => WeaponData.weaponID;

    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        _weaponItem = data as WeaponItemData;
        if (PV.IsMine && ListUnit && Usage != WeaponUsage.Throw) 
            ListUnit.HideCount();
    }

    public override void OnPicked(IInteractor interactor)
    {
        // Debug.Log("OnPicked");
        // base.OnPicked(interactor); 전략 패턴의 한계 ㅆㅂ..
        if (!interactor.PV.IsMine) return;
        Inventory.Instance.MoveToEquip(this);
        // 이러면 IInteractor가 아깝긴 한데 줍는건 사람밖에 없어서..
        /*if (interactor.gameObject.TryGetComponent<PlayerWeaponController>(out PlayerWeaponController player))
        {
            if (player.TrySetWeaponItem(this))
            {
                // player.NearInteractList.Remove(this);
                SyncedDisable();
            }
            else // if (player.Me.IsMe)
                PlayerUI.Instance.SwapCanceled(Usage);
        }*/
    }

    /*[PunRPC] protected override void RPC_Reset()
    {
        base.RPC_Reset();

    }*/

    protected override void Awake()
    {
        base.Awake();
        /*if (_item.GetType() == typeof(WeaponItem))
            _weapon = (WeaponItem)_item;*/
        if(_itemData.ItemType != ItemType.Weapon) { Debug.LogError("FieldWeapon's ItemType must be weaponType", this); return; }
        _weaponItem = (WeaponItemData)_itemData;
        _dropRoll = 90f;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        if (!_collider) _collider = GetComponent<Collider>();
        if (!_rigid) _rigid = GetComponent<Rigidbody>();
        _thickness = Mathf.Abs(_collider.bounds.size.x / 4f);
    }
}
