using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenu : Menu
{
    [SerializeField] Button _generalBtn;
    [SerializeField] Button _customBtn;
    [SerializeField] Button _goBackBtn;
    [Space]
    [SerializeField] GameObject _normalWnd;
    [SerializeField] GameObject _customWnd;

    public override void Initialize()
    {
        base.Initialize();
        _menuType = MenuType.Play;

        _generalBtn.onClick.AddListener(() => _mm.OpenMenu(MenuType.GeneralMode));
        _customBtn.onClick.AddListener(() => _mm.OpenMenu(MenuType.CustomMode));
        _goBackBtn.onClick.AddListener(() => _mm.OpenMenu(MenuType.Title));
    }
}
