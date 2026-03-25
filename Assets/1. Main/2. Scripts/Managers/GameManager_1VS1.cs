using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager_1VS1 : GameManager
{
    bool _sharedPvInst = false;
    int _myWin = 0;
    int _enemyWin = 0;

    int _currRound;
    [Space]
    [SerializeField] protected RoundWork _framework;
    [Space]
    [SerializeField] PlayerController _enemy;
    bool _otherLeftCompletely = false;
    bool _isMatchOver;
    bool _isVictorier;

    Notificator _notice;
    [SerializeField] FieldArmor[] _helmets;
    bool _isObserver;

    Camera _mainCam;
    Transform _masterPlayerTr;
    Transform _otherPlayerTr;

    public PlayerController Enemy => _enemy;
    public override Transform SpawnPoint => _spawnPoints[IsMasterClient ? 0 : 1];
    public override bool InputLock => base.InputLock || Inventory.Instance.gameObject.activeSelf;
    public bool IsObserver => _isObserver;
    public override void OnLeftRoom()
    {
        // _pv.RPC("RPC_SetVictory", RpcTarget.Others, 0f);
        // LoadSceneManager.Instance.LoadSceneAsync(SceneType.Title);
    }

    void SharePvInsts()
    {
        List<PhotonView> list = _instPvTracker;
        list.RemoveAll(x => !x.IsMine);
        int[] arr = new int[list.Count];
        for (int i = 0; i < list.Count; i++)
            arr[i] = list[i].ViewID;
        _pv.RPC("RPC_SharePvInsts", RpcTarget.Others, arr);
    }
    public override void SetAsOther(PlayerController clone)
    {
        base.SetAsOther(clone);
        SetEnemy(clone);
    }
    public override void SetMy(PlayerController player)
    {
        base.SetMy(player);
        if(player.IsSingle)
        {
            player.gameObject.SetActive(true);
            _frameworkBG.SetActive(false);
            return;
        }
        if (Enemy != null) _pv.RPC("RPC_TryStartGame", RpcTarget.Others);
    }
    public void SetEnemy(PlayerController player)
    {
        _enemy = player;
        if (MyPlayer != null)
            _pv.RPC("RPC_TryStartGame", RpcTarget.Others);
    }
    /*void TryStartGame()     // Other클라 View들한테 적과 내가 할당 되었는지 물어보는 용도
    => _pv.RPC("RPC_TryStartGame", RpcTarget.Others);*/
    public override void OnPlayerDead(PlayerController deadOne)
    {
        if (_isObserver)
        {
            bool masterDead = _viewAndPlayerTable[deadOne.PV.ViewID].Equals(PhotonNetwork.MasterClient.UserId);
            if (masterDead) Debug.Log(GetNickNameNotMe(false, PhotonNetwork.LocalPlayer.UserId) + "님 라운드 승리!");
            else Debug.Log(GetNickNameNotMe(true, PhotonNetwork.LocalPlayer.UserId) + "님 라운드 승리!");
            _framework.Ob_SetWinOrLose(!masterDead);
        }
        if (!IsMasterClient) return;

        SyncedStopTimer();
        SyncedSetInvinc(true);


        bool amIDead = deadOne == _myPlayer;

        if (amIDead) SetLose();
        else SetWin();
        _pv.RPC("RPC_SetWinOrLoseRound", RpcTarget.Others, amIDead);
        

        MyPlayer.Respawn(5f);
        Enemy.Respawn(5f);
        SyncedSetReset(5f);
    }
    void SetWin()
    {
        if (IsMasterClient) SyncedSetLeftTime();
        
        _myWin++;
        _framework.SetWinOrLose(true);
        _framework.SetWinCounts(_myWin, _enemyWin);
        if (_myWin >= _goalCounts)
        {
            SyncedStopTimer();
            SetVictory();
        }
    }
    void SetLose()
    {
        if (IsMasterClient) SyncedSetLeftTime();

        _enemyWin++;
        _framework.SetWinOrLose(false);
        _framework.SetWinCounts(_myWin, _enemyWin);
    }
    void SyncedSetWinCounts(int my, int enemy) => _pv.RPC("RPC_SetWinCountsUI", RpcTarget.All, my, enemy);
    public override void SetVictory()
    {
        if(_isObserver)
        {
            return;
        }
        _isMatchOver = _isVictorier = true;
        SyncedSetGameOver(true);
        SyncedStopTimer();
        MyPlayer.SetVictory();
        Enemy.SyncedDisable();

        StartCoroutine(Coroutine_Victory());
        _pv.RPC("RPC_SetDefeat",  RpcTarget.Others, MyPlayer.PV.ViewID, 0f);
    }
    public override void SetDefeat()
    {
        /*if (_isObserver)
        {
            return;
        }*/
        _isMatchOver = true;
        _isVictorier = false;
        MyPlayer.SetDefeat();
        StartCoroutine(Coroutine_Defeat());
    }
    void SyncedSetReset(float waitTime)=>_pv.RPC("RPC_SetReset", RpcTarget.All, waitTime);
    void SetReset()
    {
        if (IsGameOver) return;
        if (IsMasterClient)
            SetTimer();
        SyncedSetInvinc(false);
    }
    void SyncedSetRound(int round)
    {
        RPC_SetRound((byte)round);
        _pv.RPC("RPC_SetRound", RpcTarget.Others, (byte)round);
    }
    void SyncedSetRound(int round, int goal)
    {
        RPC_SetRound((byte)round, (byte)goal);
        _pv.RPC("RPC_SetRound", RpcTarget.All, (byte)round, (byte)goal);
    }
    protected override void SetLeftTime(float left)
    {
        base.SetLeftTime(left);
        _framework.SetTimer(left.ToString("0.0"));
    }
    public override void GoToTitle()
    {
        base.GoToTitle();
        if(PhotonNetwork.PlayerList.Length == 1)
            _notice.Notice("매치를 떠납니다", Color.cyan);
        else if (/*!_sharedPvInst ||*/ !MyPlayer || (PhotonNetwork.PlayerList.Length > 1 && !Enemy))
            _notice.Notice("나갈 준비가 되지 않았습니다(플레이어 대기중)");
        else if (!_isMatchOver) _notice.Notice("중도 퇴장: 매치를 떠납니다(결과가 안 났으면 내 패배).");
        else _notice.Notice("매치를 떠납니다", Color.cyan);
        _pv.RPC("OnOtherLeftRoom", RpcTarget.Others);
        StartCoroutine(Coroutine_SafeLeaveRoom());
    }

    [PunRPC] void RPC_SharePvInsts(int[] pvs)
    {
        _sharedPvInst = true;
        for (int i = 0; i < pvs.Length; i++)
            _otherPvTracker.Add(PhotonView.Find(pvs[i]));
    }
    [PunRPC] protected override void RPC_SetDraw()
    {
        _framework.SetDraw();
        // if (IsMasterClient)
        StartCoroutine(Coroutine_SetDraw(4f));
        // if (IsMasterClient)
        Invoke("SetTimer", 5f);
    }
    [PunRPC] void RPC_TryStartGame()
    {
        if (IsObserver && PhotonNetwork.PlayerList.Count() == 3)
        {
            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (PlayerController player in players)
                if (_viewAndPlayerTable[player.PV.ViewID].Equals(PhotonNetwork.MasterClient.UserId))
                {
                    player.GetComponentInChildren<SkinnedMeshRenderer>().material.color = Color.gray;
                    _framework.masterNickname.GetComponentInChildren<Text>().text 
                        = GetNickNameNotMe(true, PhotonNetwork.LocalPlayer.UserId);
                    _framework.masterNickname.gameObject.SetActive(true);
                    _masterPlayerTr = player.transform;
                }
                else
                {
                    player.GetComponentInChildren<SkinnedMeshRenderer>().material.color = Color.white;
                    _framework.otherNickname.GetComponentInChildren<Text>().text
                        = GetNickNameNotMe(false, PhotonNetwork.LocalPlayer.UserId);
                    _framework.otherNickname.gameObject.SetActive(true);
                    _otherPlayerTr = player.transform;
                }
            return;
        }
        if (IsMasterClient && PhotonNetwork.PlayerList.Length == 1)
        {
            MyPlayer.Respawn();
            // _frameworkBG.SetActive(false);            
        }
        if (!MyPlayer || !Enemy) return;
        Debug.Log("StartGame");
        if (IsMasterClient)
        {
            SetTimer();
            MyPlayer.Respawn();
            Enemy.Respawn();
            SyncedSetRound(0, _goalCounts);
            SyncedTurnOnHelmet();
            _pv.RPC("RPC_SetPlayerCallback", RpcTarget.All);
        }
        SyncedSetWinCounts(0, 0);
        SyncedSetFwBgActive(false);
        //_pv.RPC("RPC_HideLoading", RpcTarget.All);
    }
    [PunRPC] void RPC_SetWinOrLoseRound(bool win)
    {
        if (win) SetWin();
        else SetLose();
    }
    // 관전자가 승리 수 공유를 못받게 RoundWork클래스의 함수에서 자를지 아님 이 호출을 못하게 할지 고민인데
    // 지금으로서는 관전자를 알 수 없기 때문에 UI에서 자르는게 맞을듯
    [PunRPC] void RPC_SetWinCountsUI(int my, int enemy) => _framework.SetWinCounts(my, enemy);
    [PunRPC] void RPC_SetWinOrLoseUI(bool win) => _framework.SetWinOrLose(win);    
    [PunRPC] void RPC_SetReset(float waitTime) => Invoke("SetReset", waitTime);
    [PunRPC] void RPC_HideLoading() => _frameworkBG.gameObject.SetActive(false);
    [PunRPC] void RPC_SetPlayerCallback()
    {
        if(MyPlayer == null) return;
        MyPlayer.AddRespanedCallbacks(() => { if (IsMasterClient) ItemListUnitPool.Instance.ResetAll(); });
    }

    [PunRPC] void RPC_SetRound(byte round) => _framework.SetRoundCount(round);
    [PunRPC] void RPC_SetRound(byte round, byte goal) => _framework.SetRoundCount(round, goal);
    [PunRPC] void OnOtherLeftRoom()
    {
        if(!_isMatchOver)
            Notificator.Instance.Notice("중도 퇴장: 상대방이 매치를 떠났습니다(결과가 안 났으면 내 승리).");
        else Notificator.Instance.Notice(
            (_isVictorier ? "패자" : "승자") + "가 매치를 떠났습니다", Color.cyan);
        StartCoroutine(Coroutine_WaitTilOtherLeft());
    }
    [PunRPC] void SetIfOtherLeft(bool value) => _otherLeftCompletely = value;
    void SyncedTurnOnHelmet()
    {
        byte index = (byte)Random.Range(0, _helmets.Length);
        RPC_TurnOnHelmet(index);
        _pv.RPC("RPC_TurnOnHelmet", RpcTarget.Others, index);
    }
    [PunRPC] void RPC_TurnOnHelmet(byte index) => _helmets[index].gameObject.SetActive(true);
    [PunRPC] protected void RPC_SetDefeat(string victorier, float waitTime)
    {
        if (_isObserver)
        {
            if (victorier.Equals(PhotonNetwork.MasterClient.UserId))
            {
                Debug.Log(GetNickNameNotMe(true, PhotonNetwork.LocalPlayer.UserId) + "님 매치 승리!");
                _framework.Ob_SetVictoryOrDefeat(true);
            }
            else
            {
                Debug.Log(GetNickNameNotMe(false, PhotonNetwork.LocalPlayer.UserId) + "님 매치 승리!");
                _framework.Ob_SetVictoryOrDefeat(false);
            }
            return;
        }
        Invoke("SetDefeat", waitTime);
    }
    [PunRPC] protected void RPC_SetDefeat(int victorier, float waitTime)
        => RPC_SetDefeat(_viewAndPlayerTable[victorier], waitTime);
    #region Coroutines
    protected override IEnumerator Coroutine_SetTimer()
    {
        _pv.RPC("RPC_SetCloneTimer", RpcTarget.Others, _timeLimit, true);
        _pv.RPC("RPC_SetLeftTime", RpcTarget.Others, _timeLimit, true);
        SyncedSetRound(++_currRound);
        SyncedSetInvinc(false);
        _timer = 0f;
        float syncedTimer = 0f;
        while (true)
        {
            float deltaTime = Time.deltaTime;
            _timer += deltaTime;
            syncedTimer += deltaTime;
            SetLeftTime(_timeLimit - _timer);
            /*if(syncedTimer > _syncedTerm)     // 1초마다라도 성능이..
            {
                _pv.RPC("RPC_SetLeftTime", RpcTarget.All, _timeLimit - _timer);
                syncedTimer = 0f;
            }*/
            if (_timer >= _timeLimit)
            {
                if(_isMatchOver) yield break;
                _pv.RPC("RPC_StopCloneTimer", RpcTarget.Others);
                SyncedSetInvinc(true);
                SyncedSetLeftTime(0f, false);
                // if (!PhotonNetwork.IsMasterClient || Enemy == null) yield break;
                var player1 = _myPlayer;    // 승패는 내 클라 플레이어던 말던 상관없는걸 강조하기 위해..
                var player2 = _enemy;
                if (player1.Health.HP < player2.Health.HP)
                {
                    SetLose(); // 마스터 클라 하나에서만 호출하는 RPC메서드라 과감하게 실행
                    _pv.RPC("RPC_SetWinOrLoseRound", RpcTarget.Others, true);
                }
                else if (player1.Health.HP > player2.Health.HP)
                {
                    SetWin();
                    _pv.RPC("RPC_SetWinOrLoseRound", RpcTarget.Others, false);
                }
                else
                {
                    Debug.Log("Draw");
                    SetDraw();                    
                    /*player1.Respawn(5f);
                    player2.Respawn(5f);*/
                    yield break;
                }
                Invoke("SetTimer", 5f);
                player1.Respawn(5f);
                player2.Respawn(5f);
                yield break;
            }
            yield return null;
        }
    }
    protected override IEnumerator Coroutine_SetDraw(float waitTime)
    {
        if (_isObserver) yield break;
        base.Coroutine_SetDraw(waitTime);
        MyPlayer.Respawn(waitTime);
        Enemy.Respawn(waitTime); yield break;
    }
    protected override IEnumerator Coroutine_Victory()
    {
        SyncedStopTimer();
        _pv.RPC("RPC_SetVictoryView", RpcTarget.All);

        Cursor.lockState = CursorLockMode.None;
        /* if (_myPlayer.IsInvoking("Respawn"))
             CancelInvoke("Respawn");*/
        yield return Utility.GetWaitForSeconds(3f);

        _myPlayer.SyncedEnable();       
        _myPlayer.WeaponCtrl.SycedEquipNone();

        _myPlayer.SyncedSetPosition(/*_myPlayer.SpawnPoint.position*/Vector3.zero);
        _myPlayer.SyncedSetDirection(Vector3.right);
        _myPlayer.SyncedEnable();
        _myPlayer.AnimCtrl.SetTrigger(AnimTrigger.Victory);
          
        _framework.SetVictoryOrDefeat(true);
        yield return Utility.GetWaitForSeconds(1.5f);
        _pv.RPC("RPC_SetVVDOMove", RpcTarget.All, new Vector3(1.3f, 1.8f, 0f), 1f);
    }   // 내 클라 캐릭터가 이겼을 때
    protected override IEnumerator Coroutine_Defeat()  // 마찬가지로 내 클라 캐릭터가 졌을 때
    {
        Cursor.lockState = CursorLockMode.None;
        yield return Utility.GetWaitForSeconds(3f);

        _myPlayer.SyncedDisable();
        _framework.SetVictoryOrDefeat(false);
    }
    IEnumerator Coroutine_SafeLeaveRoom()
    {
        _frameworkBG.SetActive(true);
        // PhotonView[] pvs = FindObjectsOfType<PhotonView>();
        string str1 = "동기화 오브젝트를 제거하는 중";
        SetFwTxt(str1);
        int i = 0;
        foreach (PhotonView pv in _instPvTracker)
        {
            if (pv == null) continue;
            // pv.enabled = false;
            pv.SafeDestroy();
            // if (pv.IsMine)
                yield return new WaitUntil(() => pv == null || pv.gameObject == null);
            SetFwTxt(str1 + "\n(" + ++i + "/" + _instPvTracker.Count + ")완료");
        } _instPvTracker.Clear();
        string str2 = "상대방 동기화 오브젝트 검토 중...";
        SetFwTxt(str2); i = 0;
        foreach (PhotonView pv in _otherPvTracker)
        {
            if (pv == null) continue;
            yield return new WaitUntil(() => pv == null || pv.gameObject == null);
            SetFwTxt(str2 + "\n(" + ++i + "/" + _otherPvTracker.Count + ")완료");
        } _otherPvTracker.Clear();
        _pv.RPC("SetIfOtherLeft", RpcTarget.Others, true);
        yield return null;
        if (PhotonNetwork.PlayerList.Length > 1 && Enemy)
        {
            AddFwTxt("\n상대방의 응답을 기다리는 중...");
            yield return new WaitUntil(() => _otherLeftCompletely == true);
        }
        LoadSceneManager.Instance.LoadSceneAsync(SceneType.Title);
    }
    IEnumerator Coroutine_WaitTilOtherLeft()
    {
        _frameworkBG.SetActive(true);
        // SetFwTxt("상대방이 동기화 오브젝트를 제거하는 것을 기다리는 중...");
        // yield return new WaitUntil(() => _otherLeftCompletely == true);
        Debug.Log("Wait..");
        int i = 0;
        string str2 = "상대방 동기화 오브젝트 검토 중...";
        foreach (PhotonView pv in _otherPvTracker)
        {
            if (pv == null) continue;
            yield return new WaitUntil(() => pv == null || pv.gameObject == null);
            SetFwTxt(str2 + "\n(" + ++i + "/" + _otherPvTracker.Count + ")완료");
        } _otherPvTracker.Clear();
        string str1 = "나가기 전 동기화 오브젝트 제거 중...";
        SetFwTxt(str1); i = 0;
        foreach (PhotonView pv in _instPvTracker)
        {
            SetFwTxt(str1 + "\n(" + i + "/" + _instPvTracker.Count + ")완료");
            i++;
            if (pv == null) continue;
            pv.SafeDestroy();
            // if (pv.IsMine)
            yield return new WaitUntil(() => pv == null || pv.gameObject == null);        
        } _instPvTracker.Clear();
        Debug.Log("End Cleaning");
        // Destroy(RoomManager.Instance);
        _pv.RPC("SetIfOtherLeft", RpcTarget.Others, true);
        AddFwTxt("\n상대방의 응답을 기다리는 중...");
        yield return null;
        yield return new WaitUntil(() => _otherLeftCompletely == true);
        LoadSceneManager.Instance.LoadSceneAsync(SceneType.Title);
    }
    #endregion
    protected override void OnAwake()
    {
        base.OnAwake();
        _mainCam = Camera.main;
        _framework.gm = this;
        _notice = Notificator.Instance;
        if (!RoomManager.Instance.isObserver)
        {
            _isObserver = false;
            _hasInven = true;
            GameObject im = Instantiate<GameObject>(Resources.Load<GameObject>("PhotonPrefabs/InteractableManager"), transform);//PhotonNetwork.Instantiate("PhotonPrefabs/InteractableManager", Vector3.zero, Quaternion.identity);
                                                                                                                                // im.transform.SetParent(transform);
            _frameworkBG.SetActive(true);
        }
        else
        {
            _isObserver = true;
            var observer = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Observer"));
            observer.transform.position = _deathView.transform.position;
            _framework.SetAsObserver();
            _frameworkBG.SetActive(false);
        }
    }
    protected override void OnStart()
    {
        base.OnStart();
        SharePvInsts();
    }
    protected override void Update()
    {
        base.Update();
        /*if (Enemy && _coroutine_Timer == null)
            SetTimer();*/
        if(IsObserver && _masterPlayerTr != null && _otherPlayerTr != null)
        {
            _framework.masterNickname.position = _mainCam.WorldToScreenPoint(_masterPlayerTr.position + Vector3.up * 1.5f);
            _framework.otherNickname.position = _mainCam.WorldToScreenPoint(_otherPlayerTr.position + Vector3.up * 1.5f);
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.J))
            _framework.SetDraw();
        else if (Input.GetKeyDown(KeyCode.K))
            _framework.SetWinOrLose(Input.GetKey(KeyCode.LeftShift));
        else if (Input.GetKeyDown(KeyCode.L))
            _framework.SetVictoryOrDefeat(Input.GetKey(KeyCode.LeftShift));
#endif
    }
}
