using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralModeMenu : Menu
{
    [SerializeField] Button _1VS1;
    [SerializeField] Button _deathMatch;
    [SerializeField] Button _goBack;

    public override void Initialize()
    {
        base.Initialize();
        _menuType = MenuType.GeneralMode;

        // _1VS1.onClick.AddListener(() =>);
        _goBack.onClick.AddListener(() => _mm.OpenMenu(MenuType.Play));
    }
}
