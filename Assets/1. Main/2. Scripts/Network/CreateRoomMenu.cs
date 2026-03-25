using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class CreateRoomMenu : Menu
{
    CustomSheet _custom;
    // _menuType = MenuType.CreateRoom;
    [SerializeField] InputField _roomNameInputField;
    [SerializeField] Button _createBtn;
    [SerializeField] Button _exitBtn;

    public override void Initialize()
    {
        base.Initialize();

        _custom = FindAnyObjectByType<CustomSheet>(FindObjectsInactive.Include);
        _menuType = MenuType.CreateRoom;
        _createBtn.onClick.AddListener(() => {
            var hashTable = new PhotonHashTable();
            hashTable.Add("GameMode", (int)_custom.Mode);
            RoomOptions option = new RoomOptions()
            {
                MaxPlayers = (_custom.Mode == GameMode.DeathMatch_Solo) ? 5 : 3 //_custom.PersonNumber * 2
                , IsVisible = _custom.IsPublic
                , PublishUserId = true
                , CustomRoomProperties = hashTable
            };
            option.CustomRoomPropertiesForLobby = new string[] { "GameMode" };
            Launcher.Instance.CreateRoom(_roomNameInputField.text, option);
        });
        _exitBtn.onClick.AddListener(() => MenuManager.Instance.OpenMenu(MenuType.CustomMode));
    }

    /*public override void OnRoomPropertiesUpdate(PhotonHashTable propertiesThatChanged)
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object mode)
                || ((GameMode)mode != GameMode.DeathMatch_Team && (GameMode)mode != GameMode.Rounds_Team)) return;
        _mm.OpenMenu(MenuType.TeamMatchRoom);
    }*/
}
