using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectWeapon : MonoBehaviour
{
    GameManager _gm;
    PlayerWeaponController _weaponCtrl;

    [SerializeField] FieldWeapon[] _weapons;
    [SerializeField] FieldWeapon[] _subWeapons;
    [SerializeField] SelectWeaponButton _buttonPrefab;
    SelectWeaponButton[] _buttons;
    [SerializeField] Transform _layout;
    [SerializeField] Transform _marker;
    public void Initialize()
    {
        _gm = GameManager.Instance;
        gameObject.SetActive(true);
        _weaponCtrl = _gm.MyPlayer.WeaponCtrl;
        _buttons = GetComponentsInChildren<SelectWeaponButton>(); //new SelectWeaponButton[_weapons.Length];
        Dictionary<WeaponID, SelectWeaponButton> btnTable = new Dictionary<WeaponID, SelectWeaponButton>();
        foreach (SelectWeaponButton btn in _buttons)
            btnTable.Add(btn.ID, btn);

        for (int i = 0; i < _weapons.Length; i++)
        {
            FieldWeapon weapon = _weapons[i];
            if (!btnTable.TryGetValue(weapon.WeaponID, out SelectWeaponButton btn)) continue;
            btn.Initialize(this, weapon);
        }
        SetSubWeapons();
    }
    void SetSubWeapons()
    {
        foreach (FieldWeapon weapon in _subWeapons)
            _gm.MyPlayer.WeaponCtrl.TrySetWeaponItem(weapon);
    }
    public void Select(Transform tr)
    {
        _marker.SetParent(tr);
        _marker.localPosition = Vector3.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        // _gm = GameManager.Instance;
        gameObject.SetActive(false);
    }
}
