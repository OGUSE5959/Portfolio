using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using WebSocketSharp;

public class TitleMenu : Menu
{
    [SerializeField] InputField _nickName;
    [Space]
    [SerializeField] Button _play;
    [SerializeField] Button _setting;
    [SerializeField] Button _quitGame;
    [Space]
    [SerializeField] GameMenu _optionWnd;
    [SerializeField] ExitAsk _exitAsk;

    public Player LocalPlayer => PhotonNetwork.LocalPlayer;

    public override void Initialize()
    {
        base.Initialize();
        _menuType = MenuType.Title;

        if(LocalPlayer != null && !LocalPlayer.NickName.IsNullOrEmpty())
            _nickName.text = LocalPlayer.NickName;
        _play.onClick.AddListener(() =>
        {
            if (!CheckNickName(_nickName.text))
                return;
            PhotonNetwork.NickName = _nickName.text;
            _mm.OpenMenu(MenuType.CustomMode);
        });
        _setting.onClick.AddListener(()=>_optionWnd.gameObject.SetActive(!_optionWnd.gameObject.activeSelf));
        _quitGame.onClick.AddListener(() =>
        _exitAsk.gameObject.SetActive(!_exitAsk.gameObject.activeSelf));
        _optionWnd.Initialize();
    }
    bool CheckNickName(string nickName)
    {
        if (string.IsNullOrEmpty(nickName))
        {
            Notificator.Instance.Notice("먼저 닉네임을 입력해주세요 !!");
            return false;
        }
        foreach (var player in PhotonNetwork.PlayerList)
            if (player != LocalPlayer && player.NickName.Equals(nickName))
            {
                Notificator.Instance.Notice("같은 닉네임이 존재합니다 !!");
                return false;
            }
        return true;
    }
    public new void OnEnable()
    {
        if(_optionWnd.gameObject.activeSelf)
            _optionWnd.gameObject.SetActive(false);
        if (_exitAsk.gameObject.activeSelf)
            _exitAsk.gameObject.SetActive(false);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (_exitAsk.gameObject.activeSelf)
                _exitAsk.gameObject.SetActive(false);
            else if (_optionWnd.gameObject.activeSelf)
                _optionWnd.gameObject.SetActive(false);
            else _exitAsk.gameObject.SetActive(true);
        }
    }
}
