using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class RoomListItem : MonoBehaviour
{
    RoomInfo _roomInfo;

    [SerializeField] Button _it;
    [SerializeField] Text _roomName;
    [SerializeField] Text _mode;

    public void SetUp(RoomInfo roomInfo, Transform parent)
    {
        if (roomInfo == null) return;

        _roomInfo = roomInfo;
        _roomName.text = roomInfo.Name;

        transform.SetParent(parent);
        // Debug.Log("RoomListItem " + _roomInfo.MaxPlayers + ", " + roomInfo.CustomProperties.ContainsKey("GameMode"));
        if (_roomInfo.CustomProperties.TryGetValue("GameMode", out object mode))
        {
            Debug.Log("RoomListItem");
            switch ((GameMode)mode)
            {
                case GameMode.Rounds_1vs1:
                    _mode.text = "라운드 매치 1vs1"; break;
                case GameMode.Rounds_Team:
                    _mode.text = "라운드 매치 팀전"; break;
                case GameMode.DeathMatch_Solo:
                    _mode.text = "데스매치 솔로"; break;
                case GameMode.DeathMatch_Team:
                    _mode.text = "데스매치 팀전"; break;
            }
        }
    }
    public void OnClick()
    {
        Launcher.Instance.JoinRoom(_roomInfo);
    }

    // Start is called before the first frame update
    void Start()
    {
        /*_it = GetComponent<Button>();
        _roomName = GetComponentInChildren<Text>();*/        
        _it.onClick.AddListener(() => OnClick());
    }
}
