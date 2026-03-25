using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

public class TeamMatchRoomMenu : Menu
{
    PhotonView _pv;
    CustomModeMenu _custom;
    [SerializeField] ChatterBox _chat;
    [SerializeField] Text _modeLabel;
    [SerializeField] Text _roomName;
    [Space]
    [SerializeField] Transform _playerListContent_A;
    [SerializeField] Transform _playerListContent_B;
    [SerializeField] GameObject _itemPrefab;
    // [SerializeField] Transform _poolTr;
    [Space]
    [SerializeField] Button _leaveBtn;
    [SerializeField] Button _swapBtn;
    [SerializeField] Button _startBtn;
    [SerializeField] Image _startBtnImg;
    Color _startColor;
    PhotonHashTable playerSetting = new PhotonHashTable();// PhotonNetwork.LocalPlayer.CustomProperties;

    public string Team { 
        get {
            if (!playerSetting.ContainsKey("Team")) return "NullData"; 
            return playerSetting["Team"].ToString(); }
        set {
            if (!playerSetting.ContainsKey("Team"))
                playerSetting.Add("Team", "Null");
            playerSetting["Team"] = value; 
            PhotonNetwork.SetPlayerCustomProperties(playerSetting); }}
    public int MaxPlayers => PhotonNetwork.CurrentRoom.MaxPlayers;

    public override void Initialize()
    {
        base.Initialize();
        _menuType = MenuType.TeamMatchRoom;

        _custom = FindAnyObjectByType<CustomModeMenu>(FindObjectsInactive.Include);

        _startBtn.onClick.AddListener(() => StartGame());
        _startBtnImg = _startBtn.GetComponent<Image>();
        _startColor = _startBtnImg.color;
        _startBtn.interactable = false;

        PunInit();
    }
    void PunInit()
    {
        Launcher.Instance.AddJoinedRoomCallback(() =>
        {
            if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object mode)
                || ((GameMode)mode != GameMode.DeathMatch_Team && (GameMode)mode != GameMode.Rounds_Team)) return;

            _mm.OpenMenu(_menuType);
            Team = GetTeamMember(true) < Mathf.CeilToInt(MaxPlayers / 2f) ? "A" : "B";
            Debug.Log(playerSetting.ContainsKey("Team"));

            _modeLabel.text = _custom.Mode.ToString();
            _roomName.text = "방 이름 : " + PhotonNetwork.CurrentRoom.Name;
            Debug.Log("This Client Entered Team Match Room");

        });
        Launcher.Instance.AddPlayerEnteredRoomCallback((player) =>
        {
            if (player == PhotonNetwork.LocalPlayer) return;
            // SyncedUpdateTeamList();  
        });
        Launcher.Instance.AddPlayerLeftRoomCallback((player) =>
        {
            // SyncedUpdateTeamList();
        });

        AddOnClick(_leaveBtn, () => Launcher.Instance.LeaveRoom());
        AddOnClick(_swapBtn, () => Swap());
        AddOnClick(_startBtn, () => Launcher.Instance.StartGame());
    }

    int GetTeamMember(bool isA)
    {
        var players = PhotonNetwork.PlayerList;
        int count = 0;
        for(int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (isA && player.CustomProperties.TryGetValue("Team", out object team) 
                && team.Equals("A")) count++;
            else if (!isA && player.CustomProperties.TryGetValue("Team", out team)
                && team.Equals("B")) count++;
        }
        return count;
    }
    void Swap()
    {
        if (Team.Equals("A") && GetTeamMember(false) < 5) Team = "B";
        else if(GetTeamMember(true) < 5) Team = "A";
    }
    void SyncedUpdateTeamList()
    {
        if (!gameObject.activeSelf || PhotonNetwork.InRoom 
            && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object mode)
            && ((GameMode)mode == GameMode.DeathMatch_Team || (GameMode)mode == GameMode.Rounds_Team))
            _pv.RPC("RPC_OnTeamListUpdated", RpcTarget.All);
    }

    [PunRPC] void RPC_OnTeamListUpdated()
    {
        DestroyList();
        var players = PhotonNetwork.PlayerList;
        
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            // Debug.Log(player.NickName);
            var item = Instantiate(_itemPrefab, transform).GetComponent<PlayerListItem>();
            // Debug.Log(player.CustomProperties.ContainsKey("Team"));
            // Debug.Log(player == PhotonNetwork.LocalPlayer);
            // 팀 정보 프로퍼티 설정 함수 다음에 호출되어도 Player.SetCustomProperties는 꼭 1프레임 안에 끝나지 않음..
            Transform tr = /*TeamCount(true) < 5*/player.CustomProperties["Team"].Equals("A") ? _playerListContent_A : _playerListContent_B;
            item.SetUp(player, tr);
            item.gameObject.SetActive(true);
        }
        SetStartButton(PhotonNetwork.PlayerList);
    }
    void DestroyList()
    {
        foreach (Transform tr in _playerListContent_A.GetComponentsInChildren<Transform>())
        {
            if (tr == _playerListContent_A) continue;
            Destroy(tr.gameObject);
        }
        foreach (Transform tr in _playerListContent_B.GetComponentsInChildren<Transform>())
        {
            if (tr == _playerListContent_B) continue;
            Destroy(tr.gameObject);
        }
    }
    void SetStartButton(Player[] players)
    {
        /*bool canStart = Mathf.Abs(GetTeamMember(true) - GetTeamMember(false)) < 2
            && PhotonNetwork.IsMasterClient;*/
        _startBtnImg.color = PhotonNetwork.IsMasterClient ? _startColor : Color.gray;
        _startBtn.interactable = PhotonNetwork.IsMasterClient;
    }
    void StartGame()
    {
        if(Mathf.Abs(GetTeamMember(true) - GetTeamMember(false)) > 1)
        {
            // Notificator.Instance.Notice("플레이어 수 불균형");
            _chat.SyncedAnnounce("플레이어 수 불균형");
            return;
        }
        _lc.StartGame();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashTable changedProps)
    {
        if (gameObject.activeSelf && changedProps.ContainsKey("Team") && _mm.CurrentMenu == MenuType.TeamMatchRoom)
            SyncedUpdateTeamList();
    }

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        // PunInit();
    }
}
