using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FieldAmmo : FieldConsumable
{
    AmmoItemData _ammoItemData;
    public override ItemData Data { get { if (!_ammoItemData) return _itemData; return _ammoItemData; } }
    public AmmoType AmmoType => _ammoItemData.ammoType;

    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        _ammoItemData = (AmmoItemData)_consumableItemData;
    }
    [PunRPC] protected override void RPC_Reset()
    {
        base.RPC_Reset();
        Count = _ammoItemData.count;
    }
    [PunRPC] protected override void RPC_OnThrown(Vector3 dir, float force)
    {
        base.RPC_OnThrown(dir, force);
        Inventory.Instance.RemoveAmmo(this);    // 인벤토리 함수 안에 마스터의 PV.IsMine 조건이 있지만 조심!!
    }
}
