using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;
using PhotonHashTable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon.StructWrapping;

public class GameManager_DeathMatch_Solo : GameManager
{
    const string Kill = "Kill";
    const string Death = "Death";

    int _playerCount = 0;
    int _conpletedClient = 0;
    [Space]
    [SerializeField] int _myKill;
    [SerializeField] Text _goalKillLabel;
    [SerializeField] DeathMatchUI _matchUI;
    PlayerController[] _playerCtrls;
    [SerializeField] WeaponItemTemp[] _weaponTemps;
    [SerializeField] FieldGun[] _gunTemps;
    [Space]
    [SerializeField] bool _isFinalTime = false;
    // List<string> _bestPlayers = new List<string>();
    CameraManager _cm;
    Dictionary<int, Transform> _watchingViews = new Dictionary<int, Transform>();
    int _watchingIndex = 0;
    [SerializeField] bool _isWatchingMode;
    [SerializeField] GameObject _watchingWnd;
    [SerializeField] Text _currPlayerName;

    public override Transform SpawnPoint
    {
        get
        {           
            var list = _spawnPoints.ToList<Transform>();
            // Transform closest = Utility.GetClosestOne<Transform>(Enemy.transform.position, list);
            return list[Random.Range(0, list.Count)];
        }
    }

    public override void OnLeftRoom()
    {
        LoadSceneManager.Instance.LoadSceneAsync(SceneType.Title);
    }

    public override void AddPlayerList(string playerID, int viewID) // PV가Mine일 때만
    {
        base.AddPlayerList(playerID, viewID);   // 이 인자값들은 모든 클라의 GM에게 공유될 ID
        if (++_playerCount >= PhotonNetwork.PlayerList.Length)
            if (IsMasterClient) RPC_TryStartGame();
            else _pv.RPC("RPC_TryStartGame", RpcTarget.MasterClient);
        Debug.Log("Call AddPlayerList cnt : " + _playerCount);
    }
    public void AddPlayerList()
    {
        if (++_playerCount >= PhotonNetwork.PlayerList.Length)
            if (IsMasterClient) RPC_TryStartGame();
            else _pv.RPC("RPC_TryStartGame", RpcTarget.MasterClient);
        Debug.Log("Call AddPlayerList cnt : " + _playerCount);
    }
    public override void SetAsOther(PlayerController clone)
    {
        base.SetAsOther(clone);
        AddPlayerList(); // PV가Mine인 객체만 올리면 안되니깐 다른 애들도 카운트 세주기
    }
    public override void SetMy(PlayerController player)
    {
        base.SetMy(player);
        /*MyPlayer.AddRespanedCallbacks(() =>
        {
            Debug.Log("RespanedCallback");
            RPC_SetMyPlayerWeapon();
        });*/
        if (player.IsSingle)
        {
            _frameworkBG.SetActive(false);
            player.gameObject.SetActive(true);
            RPC_SetMyPlayerWeapon(); // _pv.RPC("RPC_SetMyPlayerWeapon", RpcTarget.All);
            _pv.RPC("RPC_InitKDA", RpcTarget.All);// RPC_InitKDA();

            return;
        }
    }
    List<PhotonView> GetServivers()
    {
        List<PhotonView> playerPVs = new List<PhotonView>();
        List<PhotonView> servivers = new List<PhotonView>();
        foreach(int viewID in _viewAndPlayerTable.Keys)
            playerPVs.Add(PhotonView.Find(viewID));
        foreach (PhotonView player in playerPVs)
            if (player.gameObject.activeSelf) servivers.Add(player);
        return servivers;
    }
    public override void OnPlayerDead(PlayerController player)
    // OnPlayerKill과 다른점 : 승패 처리용, OnPlayerKill는 KDA, 킬로그 업데이트용
    // 보통 OnPlayerDead가 승패를 좌우하는 이유 : 죽지 않은애가 승으로 치면 되고 죽인 경우 사용한 무기를 통해 피해자를 알아내야됨
    {
        // if (!IsMasterClient) return;
        base.OnPlayerDead(player);
        if(player == MyPlayer)
        {
            PhotonHashTable cp = CustomProperties;
            if (!cp.ContainsKey("Death")) cp.Add("Death", 0);

            int death = (int)cp["Death"]; death += 1;
            cp["Death"] = death;
            PhotonNetwork.SetPlayerCustomProperties(cp);                
        }
        else 
        {
            // 죽인 쪽에서 로그? 죽은 쪽에서 로그? 고민
            // _pv.RPC("RPC_LogKill", RpcTarget.All, );           
        }
        if (!_isFinalTime)
        {
            if (player.IsMe)
                player.Respawn(3f);
        }
        else /*if(!_isWatchingMode)*/
        {
            if (player.IsMe)
            {
                _cm.SetTarget(DeathView);
                Cursor.lockState = CursorLockMode.None;
                var list = GetServivers();
                if (list.Count == 1) SyncedNoticVictory(list[0].ViewID);
            }
            // if (IsMasterClient) { }
        }
    }
    public override void OnPlayerKill(PlayerController me, PlayerController victim)
    {
        // Debug.Log("OnPlayerKill");
        if(me.gameObject == victim.gameObject)
        {
            Notificator.Instance.Notice("자살하셨습니다!");
            return;
        }
        SetKill();
        if (_myKill >= _goalCounts /*&& IsMasterClient*/)
            SetVictory();
        /*_pv.RPC("RPC_LogKill", RpcTarget.All
            , PhotonNetwork.LocalPlayer.NickName, GetNickName(_viewAndPlayerTable[victim.PV.ViewID]), null);*/
    }
    public override void OnPlayerKill(IAttackable me, IDamagable victim)
    {
        base.OnPlayerKill(me, victim);
        if (me.gameObject == victim.gameObject)
        {
            Notificator.Instance.Notice("자살하셨습니다!");
            return;
        }
        SetKill();
        if (_myKill >= _goalCounts /*&& IsMasterClient*/)
            SetVictory();
    }
    void SyncedSetIsFinalTime(bool value)
    {
        RPC_SetIfFinalTime(value);
        _pv.RPC("RPC_SetIfFinalTime", RpcTarget.Others, value);
    }
    void SyncedCheckIfFinals(string[] ids)
    {
        RPC_CheckIfFinals(ids);
        _pv.RPC("RPC_CheckIfFinals", RpcTarget.Others, ids);
    }
    void SyncedCheckAndShareWV(string[] ids)
    {
        RPC_CheckAndShareWV(ids);
        _pv.RPC("RPC_CheckAndShareWV", RpcTarget.Others, ids);
    }
    public void ChangeWatchingView(bool isLeft)
    {
        // Debug.Log(isLeft + ", " + _watchingViews.Count);
        if (_watchingViews.Count <= 1) return;
        if(isLeft)
        {
            if (_watchingIndex == 0)
                _watchingIndex = _watchingViews.Count - 1;
            var views = _watchingViews.Values;
        }
        else
        {
            if (_watchingIndex == _watchingViews.Count - 1)
                _watchingIndex = 0;
            var views = _watchingViews.Values;
        }
        var element = _watchingViews.ElementAt(_watchingIndex);
        _cm.SetTarget(element.Value);
        _currPlayerName.text = GetNickName(element.Key) + " 님의 시점";
    }
    public override void SetVictory()
    {
        Debug.Log("SetVictory");
        _isGameOver = true;
        SyncedSetGameOver(true);
        SyncedStopTimer();

        MyPlayer.SetVictory();
        StartCoroutine(Coroutine_Victory());
        _pv.RPC("RPC_SetDefeat", RpcTarget.Others, 0f);
        _pv.RPC("RPC_SetWinnerName", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
    }
    void SyncedNoticVictory(int viewID)
    {
        RPC_NoticVictory(viewID);
        _pv.RPC("RPC_NoticVictory", RpcTarget.Others, viewID);
    }
    public override void SetDefeat()
    {
        _isGameOver = true;
        MyPlayer.SetDefeat();
        StartCoroutine(Coroutine_Defeat());
    }
    void SetKill(bool suicide = false)
    {
        PhotonHashTable cp = CustomProperties;
        if(!cp.ContainsKey(Kill)) cp.Add(Kill, _myKill);
        cp[Kill] = suicide ? --_myKill : ++_myKill; 
        PhotonNetwork.SetPlayerCustomProperties(cp);

        _matchUI.SetKill(_myKill, _goalCounts);
    }
    void SetDeath() { }
    protected override void SetLeftTime(float left)
    {
        base.SetLeftTime(left);
        _matchUI.SetTimer(left.ToString("0.0"));
    }
    void FinalFight(List<string> bestPlayers)
    {
        SyncedSetIsFinalTime(true);
        string[] arr = bestPlayers.ToArray();
        SyncedCheckIfFinals(arr);
        SyncedCheckAndShareWV(arr);
        for (int i = 0; i < _playerCtrls.Length; i++)
        {
            PlayerController player = _playerCtrls[i];
            if (!bestPlayers.Contains(ViewAndPlayerTable[player.PV.ViewID])) continue;
            SpawnPlayer(_playerCtrls[i].PV.ViewID, _spawnPoints[i]);
        }
    }
    public override void GoToTitle()
    {
        base.GoToTitle();
        Notificator.Instance.Notice(_isGameOver ? "타이틀 화면으로 돌아갑니다" 
            : "중도퇴장: 타이틀 화면으로 돌아갑니다", _isGameOver ? Color.cyan : Color.red); ;
        // Destroy(RoomManager.Instance.gameObject);
        _frameworkBG.SetActive(true);
        var cp = CustomProperties;
        cp.Clear(); PhotonNetwork.SetPlayerCustomProperties(cp);
        SetFwTxt(2);
        LoadSceneManager.Instance.LoadSceneAsync(SceneType.Title);
    }
    public void OnMasterLeave()
    {
        _pv.RPC("RPC_NoticeMasterLeave", RpcTarget.All);
        GoToTitle();
    }
    [PunRPC] protected override void RPC_SetProperties(float timeLimit, byte goalPoints)
    {
        base.RPC_SetProperties(timeLimit, goalPoints);
        _goalKillLabel.text = "목표 : " + goalPoints.ToString() + " 킬";
    }
    [PunRPC] protected override void RPC_AddPlayerList(string playerID, int viewID)
    {
        base.RPC_AddPlayerList(playerID, viewID);
        var properties = CustomProperties;  // value Type
        /*if (!properties.ContainsKey("PlayerEnterCount"))
            properties.Add("PlayerEnterCount", ++_playerCount);
        else  properties["PlayerEnterCount"] = ++_playerCount;*/
        PhotonNetwork.SetPlayerCustomProperties(properties);
    }
    [PunRPC] public void RPC_SetMyPlayerWeapon()
    {
        // MyPlayer.WeaponCtrl.TrySetWeaponItem(_weaponTemps[0].Temps[1]);
        for (int i = 0; i < (int)WeaponUsage.Max; i++)
        {
            if (_weaponTemps[i].Temps == null || _weaponTemps[i].Temps.Length == 0) continue;
            FieldWeapon weapon = _weaponTemps[i].Temps[Random.Range(0, _weaponTemps[i].Temps.Length)];
            MyPlayer.WeaponCtrl.TrySetWeaponItem(weapon);
        }
    }
    [PunRPC] void RPC_AddMyPlayerCallback() => MyPlayer.AddRespanedCallbacks(RPC_SetMyPlayerWeapon);
    [PunRPC] void RPC_TryStartGame()
    {
        // if (!MyPlayer) return;
        if (PhotonNetwork.PlayerList.Length == 1)
            MyPlayer.Respawn();
        else if (++_conpletedClient == PhotonNetwork.PlayerList.Length)
        {
            _playerCtrls = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            for (int i = 0; i < _playerCtrls.Length; i++)
                SpawnPlayer(_playerCtrls[i].PV.ViewID, _spawnPoints[i]);

            _pv.RPC("RPC_SetMyPlayerWeapon", RpcTarget.All);
            _pv.RPC("RPC_AddMyPlayerCallback", RpcTarget.All);
            _pv.RPC("RPC_InitKDA", RpcTarget.All);

            Debug.Log("StartGame");
            SetTimer();           
        }
        // _framework.SetWinCounts(0, 0);
        SyncedSetFwBgActive(false);
        Debug.Log("TryStartGame " + _conpletedClient);
    }
    [PunRPC] void RPC_InitKDA()
    {
        PhotonHashTable cp = CustomProperties;
        if (!cp.ContainsKey(Kill))
            cp.Add(Kill, 0);
        if (!cp.ContainsKey(Death))
            cp.Add(Death, 0);
        PhotonNetwork.SetPlayerCustomProperties(cp);
    }
    [PunRPC] void RPC_LogKill(string killer, string victime, Sprite icon = null)
    {
        KillLog.Instance.CreateLog(killer, victime, icon);
    }
    // 마스터 클라 플레이어가 실행시켜 줄거고 어차피 모두 동시에 사용하니까 PlayerUI에 그냥 접근해도 된다
    // [PunRPC] void RPC_SetLeftTime(float time) => SetLeftTime(time);        
    [PunRPC] void RPC_UpdateLogList()
    {
        if (!GameLog.Instance) return;
        GameLog.Instance.UpdateList();
    }
    [PunRPC] void RPC_SetIfFinalTime(bool value)
    {
        if (_isFinalTime = value)
            _matchUI.SetFinalBanner();
    }
    [PunRPC] void RPC_CheckIfFinals(string[] ids)
    {
        if (!ids.Contains<string>(PhotonNetwork.LocalPlayer.UserId))
        {
            _myPlayer.SyncedDisable();
            CameraManager.Instance.SetTarget(DeathView);
            _watchingWnd.SetActive(_isWatchingMode = true);
            Cursor.lockState = CursorLockMode.None;
            // ChangeWatchingView(true);
        }
        else
        {
            Debug.Log("The Finals");
            _myPlayer.Health.SyncedSetHP(100f, 100f);
        }
    }
    [PunRPC] void RPC_CheckAndShareWV(string[] ids)
    {
        if (!_myPlayer.gameObject.activeSelf/*!ids.Contains<string>(PhotonNetwork.LocalPlayer.UserId)*/) return;
        RPC_SetWatchingView(_myPlayer.PV.ViewID);
        _pv.RPC("RPC_SetWatchingView", RpcTarget.Others, _myPlayer.PV.ViewID);
    }
    [PunRPC] void RPC_SetWatchingView(int viewID)
    {
        Debug.Log("RPC_SetWatchingView");
        if (_watchingViews.ContainsKey(viewID)) return;
        PhotonView pv = PhotonView.Find(viewID);
        if (pv.gameObject.TryGetComponent<PlayerCameraController>(out PlayerCameraController camCtrl))
            _watchingViews.Add(viewID, camCtrl.CameraTarget);
        Debug.Log("RPC_SetWatchingView No Return " + viewID);
    }
    [PunRPC] void RPC_NoticVictory(int viewID)
    {
        if (_myPlayer.PV.ViewID == viewID)
            SetVictory();
    }
    [PunRPC] void RPC_TryVictory(string playerID)
    {
        Debug.Log(playerID);
        if (playerID.Equals(PhotonNetwork.LocalPlayer.UserId))
            SetVictory();
    }
    [PunRPC] void RPC_SetWinnerName(string winnerName) => _matchUI.SetWinenrName(winnerName);
    [PunRPC]
    void RPC_NoticeMasterLeave()
    {
        PhotonHashTable cp = CustomProperties;
        if (cp.ContainsKey(Kill)) cp[Kill] = 0;
        if (cp.ContainsKey(Death)) cp[Death] = 0;
        PhotonNetwork.SetPlayerCustomProperties(cp);
        _frameworkBG.SetActive(true);
        SetFwTxt("매치가 끝나서 나갑니다");
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashTable changedProps)
    {
        if (!IsMasterClient) return;
        /*if (changedProps.TryGetValue<int>("Kill", out int kill))
            Debug.Log("Kill : " + kill);
        if (changedProps.TryGetValue<int>("Death", out int death))
            Debug.Log("Death : " + death);*/

        // GameLog.Instance.UpdateList();
         _pv.RPC("RPC_UpdateLogList", RpcTarget.All);
    }

    IEnumerator Coroutine_RestAndSetWeapon(int rest = 10)
    {
        for(int i = 0; i < rest; i++)
            yield return null;
        MyPlayer.WeaponCtrl.TrySetWeaponItem(_gunTemps[0]);
    }
    protected override IEnumerator Coroutine_SetTimer()
    {
        // base.Coroutine_SetTimer();
        _pv.RPC("RPC_SetCloneTimer", RpcTarget.Others, _timeLimit, false);
        _pv.RPC("RPC_SetLeftTime", RpcTarget.Others, _timeLimit, true);
        _timer = 0f;
        float seconTimer = 0f;
        while (true)
        {
            float deltaTime = Time.deltaTime;
            _timer +=  deltaTime;
            seconTimer += deltaTime;
            // PlayerUI.Instance.SetTimer((_timeLimit - _timer).ToString("0.0"));
            SetLeftTime(_timeLimit - _timer);
            if (_timer >= _timeLimit)
            {
                // SyncedSetInvinc(true);
                int bestKill = -1;
                Player bestPlayer = null;
                List<string> bestPlayers = new List<string>();
                foreach(Player player in PhotonNetwork.PlayerList)  // 일단 무지성으로 가려내기
                    if(player.CustomProperties.TryGetValue("Kill", out int kill))
                        if(kill > bestKill)
                        {
                            bestKill = kill;
                            bestPlayer = player;
                        }
                foreach (Player player in PhotonNetwork.PlayerList) // 원래 킬 수가 겹쳤는지 확인
                    if (player.CustomProperties.TryGetValue("Kill", out int kill))
                    {
                        if (kill == bestKill)
                            bestPlayers.Add(player.UserId);
                        // if (bestPlayer != null) bestPlayer = null;
                    }
                if (bestPlayer != null && bestPlayers.Count == 1)
                {
                    Debug.Log(bestPlayer.NickName + ", " + bestPlayer.UserId);
                    if (bestPlayer == PhotonNetwork.LocalPlayer)
                        SetVictory();
                    else _pv.RPC("RPC_TryVictory", RpcTarget.Others, bestPlayer.UserId);
                }
                else FinalFight(bestPlayers);
                yield break;
                // Invoke("SetTimer", 5f);
            }
            if(seconTimer >= 10f)
            {
                _pv.RPC("RPC_SetLeftTime", RpcTarget.Others, _timeLimit - _timer, true);
                seconTimer = 0f;
            }
            yield return null;
        }
    }
    protected override IEnumerator Coroutine_Victory()
    {
        SyncedStopTimer();
        _myPlayer.SyncedSetPosition(new Vector3(8f, 0f, -1f));
        _myPlayer.SyncedSetDirection(Vector3.left);
        _pv.RPC("RPC_SetVictoryView", RpcTarget.All);

        Cursor.lockState = CursorLockMode.None;
        /* if (_myPlayer.IsInvoking("Respawn"))
             CancelInvoke("Respawn");*/
        yield return Utility.GetWaitForSeconds(3f);

        _myPlayer.SyncedEnable();
        _myPlayer.WeaponCtrl.SycedEquipNone();

        _myPlayer.SyncedSetDirection(Vector3.right);
        _myPlayer.SyncedEnable();
        _myPlayer.AnimCtrl.SetTrigger(AnimTrigger.Victory);

        _matchUI.SetVictoryOrDefeat(true);
        yield return Utility.GetWaitForSeconds(1.5f);
        _pv.RPC("RPC_SetVVDOMove", RpcTarget.All, new Vector3(5.15f, 1.5f, -1), 1f);
    }
    protected override IEnumerator Coroutine_Defeat()
    {
        Cursor.lockState = CursorLockMode.None;
        yield return Utility.GetWaitForSeconds(3f);

        _myPlayer.SyncedDisable();
        _matchUI.SetVictoryOrDefeat(false);
    }
    protected override void OnAwake()
    {
        base.OnAwake();
        _timeLimit = 20f;
        _hasInven = false;
        for (int i = 0; i < _gunTemps.Length; i++)
        {
            _gunTemps[i] = Instantiate(_gunTemps[i], transform);
            _gunTemps[i].gameObject.SetActive(false);
        }
    }
    protected override void OnStart()
    {
        base.OnStart();
        _cm = CameraManager.Instance;
        // GameLog.Instance.UpdateList();
        /*for (int i = 0; i < _gunTemps.Length; i++)
        {
            _gunTemps[i] = Instantiate(_gunTemps[i], transform);
            _gunTemps[i].gameObject.SetActive(false);
        }*/
    }
    protected override void Update()
    {
        base.Update();
        // if (Input.GetKeyDown(KeyCode.K)) _matchUI.SetFinalBanner();
        if (_isWatchingMode)
        {
            if (Input.GetKeyDown(KeyCode.A))
                ChangeWatchingView(true);
            else if (Input.GetKeyDown(KeyCode.D))
                ChangeWatchingView(false);
        }
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
        {
            _matchUI.SetVictoryOrDefeat(Input.GetKey(KeyCode.LeftShift));
            /*PhotonHashTable cp = CustomProperties;
            if (!cp.ContainsKey("Kill")) cp.Add("Kill", 1);
            Debug.Log(cp["Kill"] == null);
            int kill = (int)cp["Kill"] + 1; cp["Kill"] = kill;
            PhotonNetwork.SetPlayerCustomProperties(cp);*/
        }
#endif
    }
}

[System.Serializable]
public class WeaponItemTemp
{
    [SerializeField] FieldWeapon[] _temps;
    public FieldWeapon[] Temps => _temps;
}
