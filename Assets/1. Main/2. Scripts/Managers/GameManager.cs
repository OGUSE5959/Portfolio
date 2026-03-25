using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PhotonHashTable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using DG.Tweening;

public class GameManager : SingletonMonoBehaviourPunCallbacks<GameManager>
{
    protected PhotonView _pv;

    protected Dictionary<string, int> _playerAndViewTable = new Dictionary<string, int>();
    protected Dictionary<int, string> _viewAndPlayerTable = new Dictionary<int, string>();
    Dictionary<int, PlayerController> _playerViewTable = new Dictionary<int, PlayerController>();

    protected GameMenu _menu;
    protected KillLog _killLog;
    protected GameLog _log;
    protected List<PhotonView> _instPvTracker = new List<PhotonView>();
    protected List<PhotonView> _otherPvTracker = new List<PhotonView>();
    [Space]
    [SerializeField] protected Transform[] _spawnPoints = new Transform[2];
    [SerializeField] protected Transform _deathView;
    [SerializeField] protected Transform _victoryView;
    [SerializeField] protected PlayerController _myPlayer;
    [Space]
    [SerializeField] protected float _timeLimit;
    protected float _timer;
    [SerializeField] protected int _goalCounts;
    [SerializeField] protected float _syncedTerm = 1f;
    protected Coroutine _coroutine_Timer;
    protected Coroutine _coroutine_CloneTimer;

    protected bool _isInvinc = false;
    protected bool _isGameOver = false;
    protected bool _hasInven = true;
    [Space]
    [SerializeField] protected GameObject _frameworkBG;
    [SerializeField] Text _fwTxts;
    [SerializeField] protected string[] _frameworkStrs =
    {
        "플레이어들의 참가를 기다리는 중...",
        "동기화 오브젝트를 안전하게 삭제하는 중...",
        "방을 떠나는 중...",
        "상대방이 완전히 나갈 때 까지 기다리는 중...",
        "<color=Color.red>Connection Lost!</color>: 인터넷 연결 실패 "
    };

    public virtual GameMode GameMode => GameMode.None;
    public GameMenu Menu => _menu;
    public bool IsMasterClient => PhotonNetwork.IsMasterClient;
    public Dictionary<string, int> PlayerAndViewTable => _playerAndViewTable;
    public Dictionary<int, string> ViewAndPlayerTable => _viewAndPlayerTable;
    public Transform[] SpawnPoints => _spawnPoints;
    public virtual Transform SpawnPoint => null;
    public Transform DeathView => _deathView;
    public PlayerController MyPlayer => _myPlayer;
    public PlayerActionMaps Inputter => _myPlayer.Inputter.ActionMaps;
    public PlayerActionMaps ActionMap => _myPlayer.Inputter.ActionMaps;
    Inventory Inven => Inventory.Instance;
    public virtual bool InputLock
        => GameMenu.Instance && GameMenu.Instance.gameObject.activeSelf;
        // || Inventory.Instance.gameObject.activeSelf;
    public bool IsInvinc => _isInvinc;
    public bool IsGameOver => _isGameOver;
    public bool HasInven => _hasInven;
    public bool HasLog => !_hasInven;
    public PhotonHashTable CustomProperties  // 원하는 시점에 적용하도록 getter만 씀
        => PhotonNetwork.LocalPlayer.CustomProperties;

    public void RegistPvInst(PhotonView pv)
    {
        if (!pv.IsMine) return;
        _instPvTracker.Add(pv);
        // _pv.RPC("RPC_SetPvInst", RpcTarget.Others, pv.ViewID);
    }
    public void SetProperties(float timeLimit, int goalPoints)
    {
        Debug.Log("SetProperties");
        timeLimit = timeLimit <= 0f ? 90f : timeLimit;
        goalPoints = goalPoints <= 0 ? 3 : goalPoints;

        RPC_SetProperties(timeLimit, (byte)goalPoints);
        _pv.RPC("RPC_SetProperties", RpcTarget.Others, timeLimit, (byte)goalPoints);
    }
    public Player GetPlayerByPV(PhotonView pv) => GetPlayerByPV(pv.ViewID);
    public Player GetPlayerByPV(int pv)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
            if (player.UserId == _viewAndPlayerTable[pv])
                return player;
        return null;
    }
    public virtual void SetMy(PlayerController player)
    {
        _playerViewTable.Add(player.PV.ViewID, player);
        _myPlayer = player;
    }
    public virtual void SetAsOther(PlayerController clone) => _playerViewTable.Add(clone.PV.ViewID, clone);
    public virtual void AddPlayerList(string playerID, int viewID)
    {
        
        // if (!IsMasterClient) return;
            _pv.RPC("RPC_AddPlayerList", RpcTarget.All, playerID, viewID); // or RpcTarget.MasterClient
    }
    public void SetMenu()
    {
        bool active = _menu.gameObject.activeSelf;
        if (active)
        {
            _menu.gameObject.SetActive(false);
            if (!IsGameOver) Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            _menu.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public virtual void OnPlayerDead(PlayerController player) { }
    //OnPlayerKill PV가 Mine일 때 호출하는게 좋을들? 왜냐면 KDA는 커스텀 프로퍼티를 건드릴거라
    public virtual void OnPlayerKill(PlayerController me, PlayerController victim) { }
    public virtual void OnPlayerKill(IAttackable me, IDamagable victim) { }
    public virtual void OnPlayerKill(int myID, int victimID) { }

    public string GetNickName(string playerID)
    {
        foreach(var player in PhotonNetwork.PlayerList) 
            if(player.UserId.Equals(playerID))
                return player.NickName;
        return null;
    }
    public string GetNickName(int viewID) => GetNickName(_viewAndPlayerTable[viewID]);
    public string GetNickNameNotMe(bool isMaster, string myID)
    {
        string masterID = PhotonNetwork.MasterClient.UserId;
        if (isMaster) return GetNickName(masterID);

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.UserId.Equals(masterID) && !player.UserId.Equals(myID))
                return player.NickName;
        }
        return string.Empty;
    }
    public void SetTimer()  // 마스터 클라일때만 실행됨
    {
        // _pv.RPC("RPC_SetCloneTimer", RpcTarget.Others);
        if (!IsMasterClient) return;
        if (_coroutine_Timer != null) StopCoroutine(_coroutine_Timer);
        _coroutine_Timer = StartCoroutine(Coroutine_SetTimer());
    }
    protected virtual void SetLeftTime(float left) { /*_timer = _timeLimit - left;*/ }
    protected void SpawnPlayer(int viewID, Transform spawnPoint, float wait = 0f)
    {
        _playerViewTable[viewID].Respawn(spawnPoint.position, spawnPoint.forward, wait);
    }
    public void SetFwBgActive(bool value, bool sync = false)
    {
        if(!sync) _frameworkBG.SetActive(value);
        else SyncedSetFwBgActive(value);
    }
    public void SetFwTxt(int idx) => _fwTxts.text = _frameworkStrs[idx];
    public void SetFwTxt(string txt) => _fwTxts.text = txt;
    public void AddFwTxt(string txt) => _fwTxts.text += txt;
    public virtual void GoToTitle() { }
    protected void SyncedSetFwBgActive(bool value)
    {
        _frameworkBG.SetActive(value);
        _pv.RPC("RPC_SetFwBgActive", RpcTarget.Others, value);
    }
    public void SyncedSetMyPlayTr(Transform tr) => _pv.RPC("RPC_SetMyPlayTr", RpcTarget.All, tr.position, tr.forward);
    public void SyncedSetInvinc(bool value) => _pv.RPC("RPC_SetInvinc", RpcTarget.All, value);
    protected void SyncedSetLeftTime(bool calculRag = false)
    {
        float left = _timeLimit - _timer;
        SyncedSetLeftTime(left, calculRag);
    }
    protected void SyncedSetLeftTime(float left, bool calculRag = false)
    {
        SetLeftTime(left);
        _pv.RPC("RPC_SetLeftTime", RpcTarget.Others, left, calculRag);
    }
    public void SyncedStopTimer()   // 타이머를 실제로 돌리는건 마스터클라밖에 없지만 최대한 한 곳에서 호출하기 위해..
    {
        if (IsMasterClient)
        {
            if (_coroutine_Timer != null)
            {
                StopCoroutine(_coroutine_Timer);
                _coroutine_Timer = null;
            }
            _pv.RPC("RPC_StopCloneTimer", RpcTarget.Others);
            SyncedSetLeftTime();
        }
        else
        {
            if (_coroutine_CloneTimer != null)
            {
                StopCoroutine(_coroutine_CloneTimer);
                _coroutine_CloneTimer = null;
            }
            _pv.RPC("RPC_StopTimer", RpcTarget.MasterClient);
        }
    }
    protected virtual void SetDraw() => _pv.RPC("RPC_SetDraw", RpcTarget.All);
    public virtual void SetVictory() { }
    public virtual void SetDefeat() { }
    protected void SyncedSetGameOver(bool value) => _pv.RPC("RPC_SetGameOver", RpcTarget.All, value);

    [PunRPC] protected virtual void RPC_SetPvInst(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if(pv != null && !_instPvTracker.Contains(pv))
            _instPvTracker.Add(pv);
    }
    [PunRPC] protected virtual void RPC_SetProperties(float timeLimit, byte goatPoints)
    {
        _timeLimit = timeLimit;
        _goalCounts = goatPoints;
    }
    [PunRPC] protected virtual void RPC_AddPlayerList(string playerID, int viewID)
    {
        // Debug.Log(playerID + ", " + viewID);
        _playerAndViewTable.Add(playerID, viewID);
        _viewAndPlayerTable.Add(viewID, playerID);
    }
    [PunRPC] protected void RPC_SetFwBgActive(bool value) => _frameworkBG.SetActive(value);
    [PunRPC] protected virtual void RPC_SetInvinc(bool value) => _isInvinc = value;
    [PunRPC] protected virtual void RPC_SetLeftTime(float origin, bool calculRag, PhotonMessageInfo info)
    {
        float offset = 0f;
        if (calculRag)
        {
            float rag = (float)(PhotonNetwork.Time - info.SentServerTime);
            offset = calculRag ? rag : 0f;
            if (rag >= 1f) Notificator.Instance.Notice("(※핑 1000ms 이상 경고) 타이머 지연 시간 : " + rag + "초", Color.magenta);
            // Debug.Log("RPC_SetLeftTime " + offset);
        }
        SetLeftTime(_timeLimit - _timer - offset);
    }
    [PunRPC] protected void RPC_SetCloneTimer(float timeLimit, bool calculRag, PhotonMessageInfo info)
    {
        RPC_StopCloneTimer();
        if(calculRag)
        {
            float rag = (float)(PhotonNetwork.Time - info.SentServerTime);
            _timer = calculRag ? rag : 0f;
            if (rag >= 1f) Notificator.Instance.Notice("(※핑 1000ms 이상 경고) 타이머 지연 시간 : " + rag + "초", Color.magenta);
        }
        _coroutine_CloneTimer = StartCoroutine(Coroutine_SetCloneTimer(timeLimit));
    }
    [PunRPC] protected void RPC_StopCloneTimer()
    {
        _timer = 0f;
        if ( _coroutine_CloneTimer != null) StopCoroutine(_coroutine_CloneTimer);
        _coroutine_CloneTimer = null;
    }
    [PunRPC] protected virtual void RPC_StopTimer()
    {
        if (_coroutine_Timer != null) StopCoroutine(_coroutine_Timer);
        _coroutine_Timer = null;
        if(_coroutine_CloneTimer != null) StopCoroutine(_coroutine_CloneTimer);
        _coroutine_CloneTimer = null;
    }
    [PunRPC] protected void RPC_SetMyPlayTr(Vector3 pos, Vector3 dir)
    {
        MyPlayer.transform.position = pos;
        MyPlayer.transform.forward = dir;
    }
    [PunRPC] protected virtual void RPC_SetDraw(){ }
    [PunRPC] protected void RPC_SetGameOver(bool value) => _isGameOver = value;
    [PunRPC] protected void RPC_SetDefeat(float waitTime) => Invoke("SetDefeat", waitTime);
    [PunRPC] protected void RPC_SetVictory(float waitTime) => Invoke("SetVictory", waitTime);
    [PunRPC] protected void RPC_SetVictoryView()
    {
        // Debug.Log("RPC_SetVictoryView");
        CameraManager.Instance.SetTarget(_victoryView);
        Camera.main.cullingMask = ~0;
    }
    [PunRPC] protected void RPC_SetVVDOMove(Vector3 pos, float duration) => _victoryView.transform.DOMove(pos, duration);

    protected virtual IEnumerator Coroutine_SetCloneTimer(float timeLimit)
    {
        _timeLimit = timeLimit;
        while (true)
        {
            float deltaTime = Time.deltaTime;
            _timer += deltaTime;
            SetLeftTime(_timeLimit - _timer);
            if (_timer >= _timeLimit)
            {
                yield break;
            }
            yield return null;
        }
    }
    protected virtual IEnumerator Coroutine_SetTimer()
    {
        _pv.RPC("RPC_SetCloneTimer", RpcTarget.Others, _timeLimit, true);
        yield return null;
    }
    protected virtual IEnumerator Coroutine_SetDraw(float waitTime)
    {
        if (!IsMasterClient) yield break;
        yield return Utility.GetWaitForSeconds(waitTime);
    }
    protected virtual IEnumerator Coroutine_Victory() { yield return null; }
    protected virtual IEnumerator Coroutine_Defeat() { yield return null; }
    protected override void OnAwake()
    {
        base.OnAwake();
        _pv = GetComponent<PhotonView>();

        if (!_killLog) _killLog = FindAnyObjectByType<KillLog>(FindObjectsInactive.Include);
        if (!_killLog.gameObject.activeSelf)
            _killLog.gameObject.SetActive(true);
        if (!_log) _log = FindAnyObjectByType<GameLog>(FindObjectsInactive.Include);
        if (!_log.gameObject.activeSelf)
            _log.gameObject.SetActive(true);
        if (!_menu) _menu = FindAnyObjectByType<GameMenu>(FindObjectsInactive.Include);
        if (!_menu.gameObject.activeSelf)
            _menu.gameObject.SetActive(true);
        _menu.Initialize();
        // Debug.Log(_frameworkStrs.Length);
    }
    protected override void OnStart()
    {
        base.OnStart();
        // _killLog.gameObject.SetActive(false);
        _log.gameObject.SetActive(false);
        _menu.gameObject.SetActive(false);
    }
    protected virtual void Update()
    {
        if (!_myPlayer || !_myPlayer.IsMe || !_myPlayer.Inputter) return;
        bool tabDown = Inputter.UI.Tab.WasPressedThisFrame();
        bool tabUp = Inputter.UI.Tab.WasReleasedThisFrame();
        bool escDown = Inputter.UI.Menu.WasPressedThisFrame();
        if (!IsGameOver)
        {
            if (!HasInven)
            {
                if (tabDown || tabUp) GameLog.Instance.gameObject.SetActive(tabDown);
            }
        }
        else
        {

        }
        if (escDown)
        {
            if (Inven.gameObject.activeSelf) _myPlayer.UI.Inventory();
            else SetMenu();
        }
    }
}
