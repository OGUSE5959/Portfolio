using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetFailMenu : Menu
{
    [Space]
    [SerializeField] Text _causeTxt;
    [SerializeField] Text _stateTxt;
    [SerializeField] Button _exitBtn;

    Coroutine _coroutine_TryConnection;

    public override void Initialize()
    {
        base.Initialize();
        _menuType = MenuType.NetworkFailed;
        _lc.AddDisconnectCallback(cause => OnDisconnectedAction(cause));
        _exitBtn.onClick.AddListener(()=>Application.Quit());
    }
    public void OnDisconnectedAction(DisconnectCause cause)
    {
        _causeTxt.text = "<color=red>Connection Lost</color>!: 인터넷 연결 실패\n" +
            "<color=yellow>" + "cause : " + cause.ToString() + "</color>";
        if(_coroutine_TryConnection != null)
        {
            StopCoroutine(_coroutine_TryConnection);
            _coroutine_TryConnection = null;
        }
        _coroutine_TryConnection = StartCoroutine(Coroutine_TryConnection());
    }

    IEnumerator Coroutine_TryConnection()
    {
        PhotonNetwork.ConnectUsingSettings();
        _stateTxt.text = "서버와의 연결을 시도하는 중...";
        yield return new WaitUntil(()=>PhotonNetwork.IsConnected);
        _stateTxt.text = "로비에 들어갈 준비를 하는 중...";
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        _stateTxt.text = "로비에 진입 하는 중...";
        PhotonNetwork.JoinLobby();
        yield return new WaitUntil(() => PhotonNetwork.InLobby);
        _stateTxt.text = "재접속 성공!";
        yield return Utility.GetWaitForSeconds(0.5f);   
        _mm.OpenMenu(MenuType.Title);
    }
}
