using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class HitButton : MonoBehaviour, IDamagable
{
    Button _button;
    Image _img;

    public Color Color => _img.color;
    public Button.ButtonClickedEvent OnClick => _button.onClick;
    bool IDamagable.IsDie => false;

    PhotonView IDamagable.PV => throw new System.NotImplementedException();

    float IDamagable.Health => throw new System.NotImplementedException();

    void IDamagable.SetHit(IAttackable attacker, float damage) { }

    void IDamagable.SetHit(IAttackable attacker, float damage, string message) { }

    void IDamagable.SetHit(IAttackable attacker, float damage, Vector3 hitSpot)
    {
        _button.OnPointerDown(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        _button.OnPointerUp(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        _button.onClick.Invoke();
        if (GameManager.Instance.MyPlayer.WeaponCtrl.CurrentWeapon.TryGetComponent<Gun>(out Gun gun))
            if (gun.Magazine <= gun.MagazineSize)
                gun.Magazine++;
        // Debug.Log("UI TOUCH");
    }

    public void SetColor(Color color) => _img.color = color;

    // Start is called before the first frame update
    void Awake()
    {
        _button = GetComponent<Button>();
        _img = GetComponent<Image>();
    }
}
