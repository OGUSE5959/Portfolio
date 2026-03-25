using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmorSlot : Slot
{
    public ArmorType _armorType;
    [SerializeField] Image _shapeIcon;
    // [SerializeField] Image _durabilityFill;
    [SerializeField] Slider _durability;

    public override void OnClick()
    {
        base.OnClick();
        GameManager.Instance.MyPlayer.WeaponCtrl.PutOffArmor(_armorType);
    }

    public void ResetAll()
    {
        SetIcon(null);
        SetDurability(0f);
    }
    public void SetIcon(Sprite icon)
    {
        _shapeIcon.sprite = icon;
        _shapeIcon.gameObject.SetActive(icon != null);
    }
    public void SetDurability(float value)
    {
        // Debug.Log(value);
        _durability.value = value;
        _durability.gameObject.SetActive(value > 0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
