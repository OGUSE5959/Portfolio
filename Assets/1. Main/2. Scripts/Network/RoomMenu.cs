using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RoomMenu : Menu
{
    bool _isInst = false;
    [SerializeField] Text _roomName;
    [Space]
    [SerializeField] Transform _playerListContent;
    [SerializeField] PlayerListItem _itemPrefab;
    GameObjectPool<PlayerListItem> _pool = new GameObjectPool<PlayerListItem>();
    List<PlayerListItem> _activeItems = new List<PlayerListItem>();
    // [SerializeField] Transform _poolTr;
    [Space]
    [SerializeField] Button _leaveBtn;
    [SerializeField] Button _startBtn;
    [SerializeField] Image _startBtnImg;
    [SerializeField] Color _startColor;
    [Space]
    [SerializeField] Button _observeBtn;
    [SerializeField] Text _observeLabel;
    bool _isObserver = false;

    List<PlayerListItem> ActiveItems { get { return
            _playerListContent.GetComponentsInChildren<PlayerListItem>().ToList<PlayerListItem>();
            //_activeItems.RemoveAll(item => item == null || !item.gameObject.activeSelf); return _activeItems; 
        } }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + "Entered Room");
        CreateItem(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer != PhotonNetwork.LocalPlayer)
            InsertItem(otherPlayer);
    }

    void CreateItem(Player player)
    {
        if (player == null) return;
        var item = _pool.Get();
        /*if (item == null || item.gameObject == null) {
            CreateItem(player, playerListContent);
            return; }*/
        if(item == null || item.gameObject == null)
            CreateItem(player);
        item.SetUp(player, _playerListContent);
        item.gameObject.SetActive(true);
        ActiveItems.Add(item);
    }
    void CreateItems(Player[] players)
    {
        if(players == null) return;
        for (int i = 0; i < players.Length; i++)
            CreateItem(players[i]);
    }
    void InsertItemsAll()
    {
        foreach (PlayerListItem item in ActiveItems)
        {
            item.gameObject.SetActive(false);
            _pool.Set(item);
        }
        // ActiveItems.Clear();
    }
    public void InsertItem(Player player)
    {
        if (ActiveItems == null) return;
        foreach (PlayerListItem item in ActiveItems)
        {
            if(item.Player != player) continue;
            InsertItem(item);
            // ActiveItems.Remove(item);
        }
    }
    public void InsertItem(PlayerListItem item)
    {
        if (item.gameObject.activeSelf) item.gameObject.SetActive(false);
        _pool.Set(item);
    }
    public void UpdateItemList()
    {
        InsertItemsAll();
        var players = PhotonNetwork.PlayerList;
        // Debug.Log("UpdateItemList " + players == null);
        CreateItems(players);
        SetStartButton(players);
    }
    public override void Initialize()
    {
        base.Initialize();
        _menuType = MenuType.Room;
        _isInst = true;
        _startColor = _startBtnImg.color;

        _pool.CreatePool(2, () =>
        {
            PlayerListItem item = Instantiate(_itemPrefab, _playerListContent);
            item.Initialize(this);
            item.gameObject.SetActive(false);
            return item;
        });
        Launcher.Instance.AddJoinedRoomCallback(()=>
        {
            UpdateItemList();
            Debug.Log("This Client Entered Room");            
        });
        Launcher.Instance.AddPlayerEnteredRoomCallback((player) =>
        {
            CreateItem(player);
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == PhotonNetwork.CurrentRoom.MaxPlayers)
                PhotonNetwork.CurrentRoom.IsVisible = false;
            Debug.Log("Player " + player.NickName + " Entered Room");
            SetStartButton(PhotonNetwork.PlayerList);
        });
        Launcher.Instance.AddPlayerLeftRoomCallback((player) =>
        {   
            InsertItem(player);

            Debug.Log("Player " + player.NickName + " Left Room");
            SetStartButton(PhotonNetwork.PlayerList);
        });
        Launcher.Instance.AddLeftRoomCallback(()=>SetIsObserver(false));

        AddOnClick(_leaveBtn, () => Launcher.Instance.LeaveRoom());
        AddOnClick(_startBtn, () =>
        {            
            if (PhotonNetwork.IsMasterClient) Launcher.Instance.StartGame();
        });
        AddOnClick(_observeBtn,()=> SetIsObserver(!_isObserver));
    }
    void SetIsObserver(bool isObserver) => _observeLabel.text 
        = (RoomManager.Instance.isObserver = _isObserver = _isObserver = isObserver) ? "관전" : "비관전";
    void SetStartButton(Photon.Realtime.Player[] players)
    {
        int startPlayerCount = 0;
        switch(CustomSheet.Instance.Mode)
        {
            case GameMode.None:
            case GameMode.Training:
                startPlayerCount = 1; break;
            default: startPlayerCount = 2; break;
        }
        bool over = players.Length >= startPlayerCount;
        _startBtn.gameObject.SetActive(
#if UNITY_EDITOR
             true
#else
            over
#endif
             );
        _startBtnImg.color = PhotonNetwork.IsMasterClient && over ? Color.green : Color.gray;
    }

    private void OnDestroy()
    {
        _pool.Clear();
    }
    protected new void OnEnable()
    {        
        if (_isInst && PhotonNetwork.InRoom)
        {           
            _roomName.text = PhotonNetwork.CurrentRoom.Name;
            UpdateItemList();
        }
        // if (PhotonNetwork.InRoom) UpdateItemList();
    }
    /*protected new void OnDisable()
    {
        SetIsObserver(false);
    }*/
    private void Start()
    {
        /*if (PhotonNetwork.InRoom)
            UpdateItemList();*/
    }
}
