using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Error : Menu
{
    [SerializeField] Text _state;
    [SerializeField] Text _desc;
    [SerializeField] Button _leaveBtn;

    void CreateRoomFailed(short errorCode, string messege)
    {
        _mm.OpenMenu(_menuType);
        _state.text = "방 생성에 실패했습니다";
        _desc.text = "같은 이름을 가진 이름이 이미 있을 수도 있습니다 " +
            "\n에러코드: " + errorCode + ", 메시지: " + messege;
    }
    void JoinRoomFailed(short errorCode, string messege)
    {
        _mm.OpenMenu(_menuType);
        _state.text = "방 진입에 실패했습니다";
        _desc.text = "방 목록 업데이트가 늦었거나 유효하지 않은 방일 수 있습니다 " +
            "\n에러코드: " + errorCode + ", 메시지: " + messege;
    }

    public override void Initialize()
    {
        base.Initialize();

        _menuType = MenuType.Error;

        _lc.AddCreatedRoomFailedCallback((e, m) => CreateRoomFailed(e, m));
        _lc.AddJoinRoomFailedCallback((e, m) => JoinRoomFailed(e, m));

        AddOnClick(_leaveBtn, () => _mm.OpenMenu(MenuType.Title));
        AddOnClick(_leaveBtn, () => Launcher.Instance.OnJoinedLobby());
    }
}
