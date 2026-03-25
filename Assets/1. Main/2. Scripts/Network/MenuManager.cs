using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuType
{
    None = -1,

    Loading,
    // MatchMaking
    Title,
    
    Play,
    GeneralMode,
    CustomMode,
    FindRoom,
    CreateRoom,    
    Room,

    SigleRoom,
    TeamMatchRoom,     

    Error,    
    NetworkFailed,

    Max
}

public class MenuManager : SingletonMonoBehaviour<MenuManager>
{
    [SerializeField] Menu[] _menus;
    Dictionary<MenuType, Menu> _menuTable = new Dictionary<MenuType, Menu>();
    [SerializeField] MenuType _currentMenu = MenuType.None;
    public MenuType CurrentMenu => _currentMenu;

    Dictionary<MenuType, MenuType> _rollbackList = new Dictionary<MenuType, MenuType>
    {
        { MenuType.CustomMode, MenuType.Title },
        { MenuType.CreateRoom, MenuType.CustomMode },
        { MenuType.FindRoom, MenuType.CustomMode }
        // { MenuType.Room, MenuType.CustomMode }
    }; 

    public void Initialize()
    {
        foreach (Menu menu in _menus)
        {
            if (!menu.IsOpen) menu.Open();
            menu.Initialize();
        }
        /*foreach (Menu menu in _menus)
            if (menu.IsOpen) menu.Close();*/
    }
    public void OpenMenu(MenuType type)
    {
        foreach (Menu menu in _menus) 
            if(menu.IsOpen) menu.Close();
        if(type == MenuType.None) return;
        _menuTable[type].Open();
        _currentMenu = type;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
    }
    protected override void OnStart()
    {
        base.OnStart();
        Initialize();   // 꼭 딕셔너리 할당보다 먼저(메뉴타입 초기화 때문에)

        for (int i = 0; i < _menus.Length; i++)
        {
            Menu menu = _menus[i];
            MenuType type = menu.MenuType;
            if (!_menuTable.ContainsKey(type))
            {
                _menuTable.Add(type, menu);
            }
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) 
            if(_rollbackList.TryGetValue(_currentMenu, out MenuType rollback))
                OpenMenu(rollback);
    }
}
