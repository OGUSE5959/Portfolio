using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindRoomMenu : Menu
{
    [SerializeField] Transform _roomListContent;
    [SerializeField] GameObject _roomListPrefab;
    [Space]
    [SerializeField] Transform _itemPoolTr;
    GameObjectPool<RoomListItem> _itemPool = new GameObjectPool<RoomListItem>();
    [SerializeField] List<RoomListItem> _itemList = new List<RoomListItem>();
    [Space]
    [SerializeField] Button _updateBtn;
    [SerializeField] Button _backBtn;

    public override void Initialize()
    {
        base.Initialize();
        _menuType = MenuType.FindRoom;

        _itemPool.CreatePool(2, () =>
        {
            var obj = Instantiate(_roomListPrefab);
            var item = obj.GetComponent<RoomListItem>();
            item.transform.SetParent(_itemPoolTr);
            item.gameObject.SetActive(false);
            return item;
        });

        Launcher.Instance.AddRoomListUpdateCallback(AddCallback_OnRoomListUpdate);
        AddOnClick(_updateBtn, () => UpdateListItem());
        AddOnClick(_backBtn, () => _mm.OpenMenu(MenuType.CustomMode));
    }
    void UpdateListItem(List<RoomInfo> roomInfos)
    {
        // Debug.Log("UpdateRoom " + roomInfos.Count);
        if (roomInfos.Count == 0) return;
        foreach (var item in _itemList)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(_itemPoolTr);
            _itemPool.Set(item);
        }
        _itemList.RemoveAll(a => !a.gameObject.activeSelf);
        for (int i = 0; i < roomInfos.Count; i++)
        {
            RoomInfo info = roomInfos[i];
            if (info == null || info.PlayerCount == 0) continue;
            var item = _itemPool.Get();
            item.SetUp(info, _roomListContent);
            item.gameObject.SetActive(true);
            _itemList.Add(item);
        }
    }
    void UpdateListItem() => UpdateListItem(Launcher.Instance.Rooms);
    public void AddCallback_OnRoomListUpdate(List<RoomInfo> roomInfos)
    {
        if(roomInfos.Count == 0 && Launcher.Instance.Rooms.Count > 0)
        {
            UpdateListItem(Launcher.Instance.Rooms);
            return;
        }
        UpdateListItem(roomInfos);
    }

    protected new void OnEnable()
    {
        if (PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
            AddCallback_OnRoomListUpdate(Launcher.Instance.Rooms);
    }
    private void Start()
    {
//        Launcher.Instance.AddRoomListUpdateCallback(OnRoomListUpdate);
    }
}
