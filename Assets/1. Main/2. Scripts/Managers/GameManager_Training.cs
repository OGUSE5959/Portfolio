using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_Training : GameManager
{
    [SerializeField] SelectWeapon _selectUI;

    public override bool InputLock => base.InputLock || _selectUI.gameObject.activeSelf;

    public override void OnLeftRoom() => LoadSceneManager.Instance.LoadSceneAsync(SceneType.Title);
    public override void SetMy(PlayerController player)
    {
        base.SetMy(player);
        _selectUI.Initialize();
        _frameworkBG.SetActive(false);
    }
    public override void GoToTitle()
    {
        base.GoToTitle();
        _frameworkBG.SetActive(true);
        SetFwTxt(2); 
        LoadSceneManager.Instance.LoadSceneAsync(SceneType.Title);
        // Destroy(RoomManager.Instance.gameObject);
    }
    public void SetSelectUI()
    {
        bool active = _selectUI.gameObject.activeSelf;
        _selectUI.gameObject.SetActive(!active);
        Cursor.lockState = !active ? CursorLockMode.None : CursorLockMode.Locked;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _hasInven = false;
    }
    // Update is called once per frame
    protected override void Update()
    {
        // base.Update(); 
        if (!_myPlayer) return;
        bool escDown = Inputter.UI.Menu.WasPressedThisFrame();
        bool tabDown = Inputter.UI.Tab.WasPressedThisFrame();
        if (escDown)
        {
            if (_selectUI.gameObject.activeSelf) SetSelectUI(); 
            else SetMenu();
        }
        else if (tabDown) SetSelectUI();
    }
}
