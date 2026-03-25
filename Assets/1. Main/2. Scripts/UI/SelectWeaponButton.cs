using System.Collections;


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectWeaponButton : MonoBehaviour
{
    [SerializeField] WeaponID _weaponID;
    SelectWeapon _master;
    Button _btn;
    [Space]
    [SerializeField] Image _icon;
    [SerializeField] Text _name;
    public Button.ButtonClickedEvent OnClick => _btn.onClick;

    public WeaponID ID => _weaponID;

    public void Initialize(SelectWeapon master, FieldWeapon weapon)
    {
        // if(!_weaponCtrl) _weaponCtrl = GameManager.Instance.MyPlayer.WeaponCtrl;
        _master = master;
        _icon.sprite = weapon.WeaponData.icon;
        _name.text = weapon.WeaponData.name;
        OnClick.AddListener(() =>
        {
            GameManager.Instance.MyPlayer.WeaponCtrl.ResetWeaponItem(WeaponUsage.Main);
            GameManager.Instance.MyPlayer.WeaponCtrl.TrySetWeaponItem(weapon);
            _master.Select(transform);
        });
    }

    // Start is called before the first frame update
    void Awake()
    {
        _btn = GetComponent<Button>();
    }
}
