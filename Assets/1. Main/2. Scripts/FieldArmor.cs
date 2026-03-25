using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FieldArmor : FieldItem
{
    ArmorItemData _armorItemData;
    public override ItemData Data => _armorItemData;
    public ArmorItemData ArmorData => _armorItemData;
    public ArmorType ArmorType => _armorItemData.armorType;
    public ArmorLevel ArmorLevel => _armorItemData.armorLevel;
    public float Defense => ArmorData.defense;
    public float Durability;

    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        _armorItemData = data as ArmorItemData;
        Durability = ArmorData.durability;
    }
    public override void OnPicked(IInteractor interactor)
    {
        if (!interactor.PV.IsMine) return;
        Inventory.Instance.MoveToEquip(this);
        /*if (interactor.gameObject.TryGetComponent<PlayerWeaponController>(out PlayerWeaponController player))
        {
            if (player.photonView.IsMine)   // 아닌 경우가 있을진 모르겠다
                player.SetArmor(this);
            SyncedDisable();
        }*/
    }
    [PunRPC] protected override void RPC_Reset()
    {
        base.RPC_Reset();
        Durability = ArmorData.durability;
    }
}
