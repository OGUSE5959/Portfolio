using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum GameMode
{
    None = -1,

    Training,
    DeathMatch_Solo,
    DeathMatch_Team,
    Rounds_1vs1,
    Rounds_Team,
    
    Max
}
public class Launcher : MonoBehaviourPunCallbacks
{
    static Launcher _instance;
    public static Launcher Instance => _instance;
    List<RoomInfo> _rooms = new List<RoomInfo>();
    [SerializeField] MenuManager _mm;
    [SerializeField] GameMode _currGameMode;

    List<Action<DisconnectCause>> _onDisconnectCallbacks = new List<Action<DisconnectCause>>();
    List<Action> _onJoinedRoomCallbacks = new List<Action>();
    List<Action<short, string>> _onJoinRoomFailedCallbacks = new List<Action<short, string>>();
    List<Action<List<RoomInfo>>> _onRoomListUpdateCallbacks = new List<Action<List<RoomInfo>>>();
    List<Action> _onLeftRoomCallbacks = new List<Action>();
    List<Action<Player>> _onPlayerEnteredRoomCallbacks = new List<Action<Player>> ();
    List<Action<Player>> _onPlayerLeftRoomCallbacks = new List<Action<Player>>();
    List<Action> _onCreatedRoomCallbacks = new List<Action>();
    List<Action<short, string>> _onCreatedRoomFailedCallbacks = new List<Action<short, string>>();

    public Player Master => PhotonNetwork.MasterClient;
    public List<RoomInfo> Rooms => _rooms;

    #region Add Callbacks
    public void AddDisconnectCallback(Action<DisconnectCause> callback)
    {
        if (!_onDisconnectCallbacks.Contains(callback))
            _onDisconnectCallbacks.Add(callback);
    }
    public void AddJoinedRoomCallback(Action callback)
    {
        if(!_onJoinedRoomCallbacks.Contains(callback))
            _onJoinedRoomCallbacks.Add(callback);
    }
    public void AddJoinRoomFailedCallback(Action<short, string> callback)
    {
        if (!_onJoinRoomFailedCallbacks.Contains(callback))
            _onJoinRoomFailedCallbacks.Add(callback);
    }
    public void AddRoomListUpdateCallback(Action<List<RoomInfo>> callback)
    {
        if(!_onRoomListUpdateCallbacks.Contains (callback))
            _onRoomListUpdateCallbacks.Add (callback);
    }
    public void AddLeftRoomCallback(Action callback)
    {
        if(!_onLeftRoomCallbacks.Contains (callback)) 
            _onLeftRoomCallbacks.Add(callback);
    }
    public void AddPlayerEnteredRoomCallback(Action<Player> callback)
    {
        if(!_onPlayerEnteredRoomCallbacks.Contains(callback))
            _onPlayerEnteredRoomCallbacks.Add(callback);
    }
    public void AddPlayerLeftRoomCallback(Action<Player> callback)
    {
        if (!_onPlayerLeftRoomCallbacks.Contains(callback))
            _onPlayerLeftRoomCallbacks.Add(callback);
    }
    public void AddCreatedRoomCallback(Action callback)
    {
        if (!_onCreatedRoomCallbacks.Contains(callback))
            _onCreatedRoomCallbacks.Add(callback);
    }
    public void AddCreatedRoomFailedCallback(Action<short, string> callback)
    {
        if (!_onCreatedRoomFailedCallbacks.Contains(callback))
            _onCreatedRoomFailedCallbacks.Add(callback);
    }
    #endregion

    #region Own Functions
    public void SetGameMode(GameMode mode) => _currGameMode = mode; 
    public void CreateRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName)) return;
        PhotonNetwork.CreateRoom(roomName);
        _mm.OpenMenu(MenuType.Loading);
    }
    public void CreateRoom(string roomName, RoomOptions options)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Notificator.Instance.Notice("¹æ ÀÌ¸§À» ÀÔ·ÂÇØÁÖ¼¼¿ä");
            return;
        }
        PhotonNetwork.CreateRoom(roomName, options);
        // Debug.Log(options.CustomRoomProperties["GameMode"]);
        _mm.OpenMenu(MenuType.Loading);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        _mm.OpenMenu(MenuType.Loading);
    }
    public void JoinRoom(RoomInfo roomInfo)
    {
        PhotonNetwork.JoinRoom(roomInfo.Name);
        _mm.OpenMenu(MenuType.Loading);
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        _mm.OpenMenu(MenuType.Loading);
    }
    void LoadLevel(SceneType level) => PhotonNetwork.LoadLevel((int)level);
    public void StartGame()
    {
        _mm.OpenMenu(MenuType.Loading);
        GameMode mode = (GameMode)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"];
        switch(mode)
        {
            case GameMode.Training:
                LoadLevel(SceneType.Training); break;
            case GameMode.Rounds_1vs1:
                LoadLevel(SceneType.Round_1vs1); break;
            case GameMode.Rounds_Team:
                Debug.Log("TeamRounds"); 
                LoadLevel(SceneType.Round_Team); break;
            case GameMode.DeathMatch_Solo:
                LoadLevel(SceneType.DeathMatch_Solo); break;
            case GameMode.DeathMatch_Team:
                Debug.Log("DeathMatch");
                LoadLevel(SceneType.DeathMatch_Team); break;
            default: /*PhotonNetwork.LoadLevel(2);*/ Debug.Log("Can't Load Level !!"); break;
        }
       
    }

    #endregion
    #region Overrided Functions
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Master");
        PhotonNetwork.JoinLobby(/*TypedLobby.Default*/);
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        _mm.OpenMenu(MenuType.NetworkFailed);
        foreach (var callback in _onDisconnectCallbacks)
            callback?.Invoke(cause);
        if (Notificator.Instance)
            Notificator.Instance.Notice("Connection Lost! <color=Color.white>³×Æ®¿öÅ· ½ÇÆÐ</color> " +
                "<color=Color.yellow> cause => " + cause.ToString() + "</color>");
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        if (_mm.CurrentMenu != MenuType.CustomMode)
            _mm.OpenMenu(MenuType.Title);
    }
    public override void OnJoinedRoom()
    {
        foreach(var callback in _onJoinedRoomCallbacks)
            callback?.Invoke();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        foreach (var callback in _onJoinRoomFailedCallbacks)
            callback?.Invoke(returnCode, message);
    }
    public override void OnCreatedRoom()
    {
        foreach (var callback in _onCreatedRoomCallbacks)
            callback?.Invoke();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // Debug.LogAssertion("OnCreateRoomFailed");
        foreach (var callback in _onCreatedRoomFailedCallbacks)
            callback?.Invoke(returnCode, message);
    }
    public override void OnLeftRoom()
    {
        foreach (var callback in _onLeftRoomCallbacks)
            callback?.Invoke();
        _mm.OpenMenu(MenuType.CustomMode);   // Title    ? Or Play?

        /*ChatterBox cb = FindAnyObjectByType<ChatterBox>();
        cb.SyncedAnnounce(PhotonNetwork.LocalPlayer.NickName + " ´ÔÀÌ ³ª°¡¼Ì½À´Ï´Ù", RpcTarget.Others);
        cb.ResetChat();*/
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        _rooms = roomList;
        foreach (var callback in _onRoomListUpdateCallbacks)
            callback?.Invoke(roomList);
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        // base.OnRoomPropertiesUpdate(propertiesThatChanged);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        foreach(var callback in _onPlayerEnteredRoomCallbacks)
            callback?.Invoke(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach (var callback in _onPlayerLeftRoomCallbacks)
            callback?.Invoke(otherPlayer);
        // Debug.Log("Master Client : " + Master.NickName);
    }
    #endregion

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        // Debug.Log(Mathf.Log10(0.5f));
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        if (!_mm) _mm = MenuManager.Instance;
        Debug.Log("Connecting To Master");
        if (PhotonNetwork.InRoom)
        {
            _mm.OpenMenu(MenuType.Room);

        }
        else if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            _mm.OpenMenu(MenuType.Loading);
        }
        else
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                Debug.Log("Connected To Master");
                PhotonNetwork.JoinLobby(/*TypedLobby.Default*/);
            }
        }
        /*string str = "¾¾¹ß»õ³¢";
        str = str.Replace("¾¾¹ß»õ³¢", "****");
        Debug.Log(str);*/
    }
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var rl = FindObjectsOfType<RoomManager>();
            Debug.Log("Room Count : " + rl.Length);
        }
    }*/
    private void OnDestroy()
    {
        _onPlayerEnteredRoomCallbacks.Clear();
        _onPlayerLeftRoomCallbacks.Clear();
        _onCreatedRoomCallbacks.Clear();
    }
}
