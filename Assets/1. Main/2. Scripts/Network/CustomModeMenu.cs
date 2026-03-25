using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashTable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CustomModeMenu : Menu
{
    bool _isInst = false;
    [Space]
    [SerializeField] Text _gameModeLabel;
    [SerializeField] Transform _tweenTarget;
    [SerializeField] Button _roundsMode;
    [SerializeField] Button _deathMode;
    [SerializeField] CustomSheet _customSheet;
    [Space]
    [SerializeField] Button _findRoom;
    [SerializeField] Button _trainning;
    [SerializeField] Button _createRoom;
    [SerializeField] Button _goBack;

    public GameMode Mode { get { return _customSheet.Mode; } }
    float BtnStartY => _tweenTarget.localPosition.y;

    public override void Initialize()
    {
        base.Initialize();
        _menuType = MenuType.CustomMode;
        _customSheet.Initialize();  // 속성 조절 UI오브젝트 초기화

        _lc.AddJoinedRoomCallback(() => // lc == Launcher.Instance, 런처에게 룸 입장 콜백 등록
        {
            GameMode mode = GameMode.None;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object md))
                mode = (GameMode)md;
            if (mode == GameMode.DeathMatch_Team
                || mode == GameMode.Rounds_Team)
            {
                // _mm.OpenMenu(MenuType.TeamMatchRoom);
            }
            else
            {
                _mm.OpenMenu(MenuType.Room);
            }

            ChatterBox cb = FindAnyObjectByType<ChatterBox>();
            if(cb) cb.SyncedAnnounce(PhotonNetwork.NickName + " 님이 들어오셨습니다", RpcTarget.All);
            // Debug.Log("Master Client : " + PhotonNetwork.MasterClient.NickName);
        });

        _roundsMode.onClick.AddListener(() =>
        {
            GameMode mode = GameMode.Rounds_1vs1;   // 라운드 모드의 기본형은 1ㄷ1
            if (Mode == mode) return;
            _gameModeLabel.text = "게임 모드 : 라운드매치";
            _lc.SetGameMode(mode);
            _customSheet.SetUp(MatchType.Rounds);

            ColorBlock colorBlock = _roundsMode.colors;
            colorBlock.normalColor = colorBlock.selectedColor = Color.cyan;
            _roundsMode.colors = colorBlock;
            colorBlock.normalColor = colorBlock.selectedColor = Color.white;
            _deathMode.colors = colorBlock;

            _roundsMode.transform.DOKill();
            _deathMode.transform.DOKill();
            _roundsMode.transform.DOLocalMoveY(BtnStartY + 20f, 0.5f);
            // if (_deathMode.transform.localPosition.y >= BtnStartY)
                _deathMode.transform.DOLocalMoveY(BtnStartY, 0.5f);
        });
        _deathMode.onClick.AddListener(() =>
        {
            GameMode mode = GameMode.DeathMatch_Solo;   // 데스매치의 기본형은 개인전
            if (Mode == mode) return;
            _gameModeLabel.text = "게임 모드 : 데스매치";
            _lc.SetGameMode(mode);
            _customSheet.SetUp(MatchType.DeathMatch);

            ColorBlock colorBlock = _deathMode.colors;
            colorBlock.normalColor = colorBlock.selectedColor = Color.yellow;
            _deathMode.colors = colorBlock;
            colorBlock.normalColor = colorBlock.selectedColor = Color.white;
            _roundsMode.colors = colorBlock;
            _deathMode.transform.DOKill();
            _roundsMode.transform.DOKill();
            _deathMode.transform.DOLocalMoveY(BtnStartY + 20f, 0.5f);
            // if (_roundsMode.transform.localPosition.y >= BtnStartY)
                _roundsMode.transform.DOLocalMoveY(BtnStartY, 0.5f);
        });

        _findRoom.onClick.AddListener(()=>_mm.OpenMenu(MenuType.FindRoom));
        _createRoom.onClick.AddListener(() =>
        {
            if(Mode == GameMode.None)
            {
                Notificator.Instance.Notice("게임 모드가 선택되지 않았습니다!");
                return;
            }
            RoomManager.Instance.SetProperties(_customSheet.TimeLimit, _customSheet.GoalCount);
            _mm.OpenMenu(MenuType.CreateRoom);
        });
        _trainning.onClick.AddListener(() =>
        {
            _customSheet.SetUp(MatchType.Train);
            // _mm.OpenMenu(MenuType.CreateRoom);
            var hashTable = new PhotonHashTable();
            hashTable.Add("GameMode", (int)GameMode.Training);
            RoomOptions option = new RoomOptions()
            {
                CustomRoomProperties = hashTable,
                IsVisible = false
            };
            option.CustomRoomPropertiesForLobby = new string[] { "GameMode" };
            Launcher.Instance.CreateRoom("My Trainning Room, ID: " + PhotonNetwork.LocalPlayer.UserId, option);
        });
        _goBack.onClick.AddListener(() => _mm.OpenMenu(MenuType.Title));

        _roundsMode.onClick?.Invoke();
    }
    public void ResetMode()
    {
        _gameModeLabel.text = "게임 모드 : None";
        _customSheet.ResetAll();

        ColorBlock colorBlock = _roundsMode.colors;
        colorBlock.normalColor = colorBlock.selectedColor = Color.white;
        _roundsMode.colors = _deathMode.colors = colorBlock;

        if (_deathMode.transform.localPosition.y >= BtnStartY)
            _deathMode.transform.DOLocalMoveY(BtnStartY, 0.5f);
        if (_roundsMode.transform.localPosition.y >= BtnStartY)
            _roundsMode.transform.DOLocalMoveY(BtnStartY, 0.5f);
    }
    protected new void OnEnable()
    {
        // _customSheet.ResetAll();
        if (_isInst)
        {
            // if(Mode != GameMode.None) ResetMode();
            // _customSheet.SetGameMode(GameMode.Rounds_1vs1);
            // _roundsMode.onClick?.Invoke();
        }
        else
        {
            _isInst = true;
            // _roundsMode.onClick?.Invoke();
        }
    }
}
