using AT.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Inventory : SingletonMonoBehaviour<Inventory>
{
    PlayerController _master;
    [SerializeField] VerticalLayoutGroup _fieldLayout;

    // [Space]
    [SerializeField] VerticalLayoutGroup _bagLayout;
    [SerializeField] Transform _dummy_Field;
    [SerializeField] Transform _dummy_Bag;
    Dictionary<AmmoType, List<FieldAmmo>> _ammoList = new Dictionary<AmmoType, List<FieldAmmo>>();
    Dictionary<ThrowableType, FieldWeapon> _throwableList = new Dictionary<ThrowableType, FieldWeapon>(); 
    
    List<FieldAmmo> _usedAmmos = new List<FieldAmmo>();

    [Space]
    [SerializeField] Image[] _weaponShapes = new Image[(int)WeaponUsage.Max];
    [SerializeField] ArmorSlot[] _armorSlots = new ArmorSlot[(int)ArmorType.Max];

    bool CanSync => _master.IsMe;
    bool HasInven => GameManager.Instance && GameManager.Instance.HasInven;
    public PlayerController Master => _master;
    public VerticalLayoutGroup FieldGrid => _fieldLayout;
    public VerticalLayoutGroup BagGrid => _bagLayout;
    public Transform FieldTr => _dummy_Field.transform;
    public Transform BagTr => _dummy_Bag.transform;
    public Dictionary<AmmoType, List<FieldAmmo>> AmmoList => _ammoList;
    public ArmorSlot[] ArmorSlots => _armorSlots;

    public void Initialize(PlayerController master)
    {
        _master = master;
    }
    public void ResetAll()
    {
        /*foreach(ItemListUnit unit in FieldGrid.GetComponentsInChildren<ItemListUnit>())
                unit.SyncedDisable();*/   // 가방 안에 있는 아이템들은 각자 용도별 초기화
        /*foreach (ItemListUnit unit in BagGrid.GetComponentsInChildren<ItemListUnit>())
        {
            PutDownOnField(unit, false); //unit.SyncedDisable();// HideFieldItem(unit.FieldBody);
            unit.FieldBody.SyncedReset();
        }*/
        foreach (var pair in _ammoList)
        {
            /*foreach (FieldAmmo ammo in pair.Value)
                ammo.SyncedReset();*/
            pair.Value.Clear();
        }
        foreach (FieldAmmo ammo in _usedAmmos)
            ammo.SyncedReset();

        foreach (ArmorSlot slot in _armorSlots)
            slot.ResetAll();
        foreach (Image img in _weaponShapes)
            img.color = Color.clear; //sprite = null;
        // field group, slots
    }
    #region Item Moves
    public void AddFieldItem(FieldItem item)
    {
        if (!CanSync || !item) return;
        item.SetInField();
        item.SetUIActive(true);
    }
    public void HideFieldItem(FieldItem item)
    {
        if (!CanSync) return;
        // item.ListUnit.transform.SetParent(_fieldLayout.transform);
        item.SetUIActive(false);
    }
    public void MoveToBag(ItemListUnit item)
    {
        FieldItem data = item.FieldBody;
        item.gameObject.SetActive(true);
        // Debug.Log(data.GetType());
        switch (data.ItemType)
        {
            case ItemType.Consumable:
                if (data.IsConvertibleTo(typeof(FieldConsumable), true))
                {
                    var consume = data.ConvertTo<FieldConsumable>();
                    if (consume.IsConvertibleTo(typeof(FieldAmmo), true))
                    {
                        var ammo = data.ConvertTo<FieldAmmo>();
                        AddAmmo(ammo);
                    }
                    else if (consume.IsConvertibleTo(typeof(FieldHeal), true))
                    {
                        var heal = data.ConvertTo<FieldHeal>();

                    }
                }
                item.FieldBody.SetInBag();  
                break;
            /*case ItemType.Weapon:
                if (item.FieldBody.TryGetComponent<FieldWeapon>(out FieldWeapon weapon))
                    weapon.OnPicked(_master.Interacter);
                break;*/
        }       
    }
    public void MoveToBag(FieldItem item)
    {
        // Debug.Log(data.GetType());
        switch (item.ItemType)
        {
            /*case ItemType.Weapon:
                if (_master.WeaponCtrl.TrySetWeaponItem(item))
                {
                    // player.NearInteractList.Remove(this);
                    item.SyncedDisable();
                }
                else // if (player.Me.IsMe)
                    PlayerUI.Instance.SwapCanceled(item.Usage);
                break;*/
            case ItemType.Consumable:
                if (item.IsConvertibleTo(typeof(FieldConsumable), true))
                {
                    FieldConsumable consume = item.ConvertTo<FieldConsumable>();
                    if (consume.IsConvertibleTo(typeof(FieldAmmo), true))
                    {
                        FieldAmmo ammo = item.ConvertTo<FieldAmmo>();
                        AddAmmo(ammo);
                    }
                    else if (consume.IsConvertibleTo(typeof(FieldHeal), true))
                    {
                        FieldHeal heal = item.ConvertTo<FieldHeal>();

                    }
                }
                item.SetInBag();
                break;
                /*case ItemType.Weapon:
                    if (item.FieldBody.TryGetComponent<FieldWeapon>(out FieldWeapon weapon))
                        weapon.OnPicked(_master.Interacter);
                    break;*/
        }
    }
    public void MoveToEquip(FieldWeapon weapon)
    {
        // Debug.Log("MoveToEquip " + weapon.WeaponData.WeaponType);
        /*if (weapon.WeaponData.usage == WeaponUsage.Throw)
        {
            AddThrowable(weapon);           
            return;
        }*/
        if (_master.WeaponCtrl.TrySetWeaponItem(weapon))
            weapon.SyncedDisableWithUI();
        else
        {
            string usage = "";
            switch(weapon.Usage)
            {
                case WeaponUsage.Main:
                    usage = "주"; break;
                case WeaponUsage.Sub:
                    usage = "보조"; break;
                case WeaponUsage.Throw:
                    usage = "투척"; break;
                case WeaponUsage.Melee:
                    usage = "근접"; break;
            }
            Notificator.Instance.Notice(usage + " 무기가 이미 있습니다! (T 를 눌러 버리기)");
            /*_master.WeaponCtrl.ThrowWeapon();
            _master.WeaponCtrl.SetWeaponItem(weapon);
            weapon.SyncedDisableWithUI();*/
        }
        // else // if (player.Me.IsMe) PlayerUI.Instance.SwapCanceled(weapon.Usage);
    }
    public void MoveToEquip(FieldArmor armor)
    {
        _master.WeaponCtrl.SetArmor(armor);
        armor.SyncedDisableWithUI();
    }
    public void PutDownOnField(ItemListUnit item, bool l_throw = true)
    {
        if (!CanSync) return;
        if (item.FieldBody.ItemType == ItemType.Consumable
            && item.FieldBody.TryGetComponent<FieldAmmo>(out FieldAmmo ammo))
            RemoveAmmo(ammo);
        // Destroy(item.gameObject);
        item.SyncedSetActive(false);
        item.transform.SetParent(null);

        // Debug.Log("Drop Item", item.gameObject);
        if(l_throw) _master.Interacter.ThrowItem(item.FieldBody);
    }
    public void OnMouseDown1(ItemListUnit unit)
    {
        FieldItem item = unit.FieldBody;
        switch(item.ItemType)
        {
            case ItemType.Weapon:
                FieldWeapon weapon = item as FieldWeapon;
                if (weapon.WeaponData.usage == WeaponUsage.Throw)
                    _master.WeaponCtrl.TrySetWeaponItem(weapon);
                break;
        }
    }
    #endregion
    #region Ammo Management
    public void AddAmmo(FieldAmmo ammo)
    {
        if (!CanSync) return;
        if (!_ammoList.TryGetValue(ammo.AmmoType, out var list))
            return;

        if (list.Count == 0) list.Add(ammo);
        else
        {
            FieldAmmo last = list.Last();
            if (last.Count < 200)
            {
                last.Count += ammo.Count;
                if (last.Count > 200)
                {
                    ammo.Count = last.Count - 200;
                    last.Count = 200;
                    list.Add(ammo);
                }
                else RemoveAmmo(ammo);
            }
        }
        PlayerUI.Instance.UpdateTotalAmmo();

        // Debug.Log(ammo+" "+ _ammoList[ammo.AmmoType].Count);
    }
    public int GetAmmo(AmmoType ammoType, int amount)
    {
        if (!HasInven) return amount;
        if (!_ammoList.TryGetValue(ammoType, out var list))
            return 0;

        if (list == null || list.Count == 0) return 0;       
        else
        {
            int i = 0;
            while(list.Count > 0)
            {
                if (i++ >= 5959) return i;
                var ammo = list.Last();
                if (ammo.Count >= amount)
                {
                    ammo.Count -= amount;
                    if(ammo.Count == 0) RemoveAmmo(ammo);
                    // Debug.Log(amount);
                    return amount;
                }
                else
                {
                    RemoveAmmo(ammo);
                    int left = amount - ammo.Count;
                    int temp = ammo.Count;
                    if (list.Count == 0)
                    {
                        // Debug.Log(temp);
                        return temp;
                    }
                    var ammo1 = list.Last();                    

                    if (ammo1.Count >= left)
                    {
                        ammo1.Count -= left;
                        // Debug.Log(amount);
                        return amount;
                    }
                    RemoveAmmo(ammo1);
                    return ammo1.Count;
                }
            }
            return 0;           
        }
    }
    public int AmmoCount(AmmoType ammoType)
    {
        if (!HasInven) return 999;
        if(!_ammoList.TryGetValue(ammoType, out var list))
            return 5959;

        int total = 0;
        foreach(var ammo in list)
            total += ammo.Count;
        return total;
    }
    public void DiscountAmmo(AmmoType ammoType, int discount)
    {
        var list = _ammoList[ammoType];
        if (list.Count == 0) return;
        FieldAmmo ammo = list.First();
        ammo.Count -= discount;
        if (ammo.Count <= 0) RemoveAmmo(ammo);
    }
    public void RemoveAmmo(FieldAmmo ammo)
    {
        if (!CanSync) return;

        var list = _ammoList[ammo.AmmoType];
        if (list.Contains(ammo)) list.Remove(ammo);
        ammo.ListUnit.SyncedSetActive(false);
        if(!_usedAmmos.Contains(ammo)) _usedAmmos.Add(ammo);

        PlayerUI.Instance.UpdateTotalAmmo();
    }
    #endregion
    #region Throwable Management
    public void AddThrowable(FieldWeapon fieldWeapon)
    {
         Debug.Log("AT");
        var data = fieldWeapon.WeaponData;
        if (data.usage != WeaponUsage.Throw) return;
        ThrowableType type = (ThrowableType)((int)data.weaponID - (int)WeaponID.Grenade);
        if (!_throwableList.TryGetValue(type, out FieldWeapon origin))
        {
            // Debug.Log("&&&&&&&&&&&&&&&&&");
            _throwableList[type] = fieldWeapon;
            fieldWeapon.SetInBag();
            return;
        }

        origin.Count += fieldWeapon.Count;
        Debug.Log("AddThrowable " + origin.Count, origin.ListUnit);
        fieldWeapon.SyncedDisableWithUI();
    }
    // 차라리 이렇게 하나의 투척물을 얻어오며 총 남은 수를 갱신하는걸 PlayerUI에서 만드는게 좋을듯
    public bool GetThrowable(ThrowableType type/*, int count = 1*/) 
    {
        if (!_throwableList.TryGetValue(type, out FieldWeapon item)
            || item.Count == 0) return false;
        if (--item.Count <= 0)
        {
            item.SyncedDisableWithUI();
            _throwableList[type] = null;
        }
        return true;
     }
    public int ThrowableCount(ThrowableType type)
    {
        if (!_throwableList.TryGetValue(type, out FieldWeapon item)) return 0;
        return item.Count;
    }
    #endregion
    public void SetWeaponShape(WeaponUsage usage, Sprite sprite)
    {
        Image img = _weaponShapes[(int)usage];
        img.sprite = sprite;
        img.gameObject.SetActive(img.sprite != null);
    }
    public void ResetArmorSlot(ArmorType armor)
    {
        ArmorSlot slot = _armorSlots[(int)armor];
       slot.ResetAll();
    }
    public void SetArmorSlot(FieldArmor armor)
    {
        ArmorSlot slot = _armorSlots[(int)armor.ArmorType];
        slot.SetIcon(armor.Data.icon);
        slot.SetDurability(armor.Durability);
    }

    // Start is called before the first frame update
    protected override void OnAwake()
    {
        base.OnAwake();
        for(int i = 0; i < (int)AmmoType.Max; i++)
        {
            var list= new List<FieldAmmo>();
            _ammoList.Add((AmmoType)i, list);
        }
        for (int i = 0; i < (int)WeaponUsage.Max; i++)
            SetWeaponShape((WeaponUsage)i, null);
        for (int i = 0; i < (int)ArmorType.Max; i++)
        {
            ArmorType type = (ArmorType)i;
            ArmorSlot slot = ArmorSlots[i];
            slot._armorType = type;
            slot.ResetAll();
        }
    }
    protected override void OnStart()
    {
        base.OnStart();
        // _master = GameManager.Instance.Player;
    }
}
