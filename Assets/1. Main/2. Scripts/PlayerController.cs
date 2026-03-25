using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Photon.Pun;

[RequireComponent(typeof(PlayerInputController), typeof(PlayerMoveController), typeof(PlayerCameraController))]
[RequireComponent(typeof(PlayerAnimationController), typeof(PlayerInteractController), typeof(PlayerWeaponController))]
[RequireComponent(typeof(PlayerHealthController))]
public partial class PlayerController : SyncedMonoBehaviour, IAttackable
{
    PlayerManager _pm;
    GameManager _gm;
    CharacterController _charCtrl;
    PlayerUI _ui;
    [SerializeField] int _spawnIndex;
    [SerializeField] int _winCount;
    [SerializeField] SkinnedMeshRenderer[] _skins;
    int _deathCount = 0;
    List<Action> _onEnabledCallbacks = new List<Action>();
    List<Action> _onRespawnedCallbacks = new List<Action>();

    public IAttackable Entity => this;
    public string Name => PhotonNetwork.LocalPlayer.NickName;
    public float Damage => 0f;
    public int SpawnIndex => _spawnIndex;
    public Transform SpawnPoint => GameManager.Instance.SpawnPoint;// GameManager.Instance.SpawnPoints[IsMasterClient ? 0 : 1];
    public bool Offline => PhotonNetwork.OfflineMode;
    public bool IsSingle => PhotonNetwork.PlayerList.Length == 1;
    public bool IsMe => _pv.IsMine || Offline || IsSingle;
    public PlayerUI UI => _ui;
    public bool IsGameOver => _gm.IsGameOver; //_gameOver;
    
    #region Controllers
    PlayerInputController _input;
    PlayerMoveController _moveCtrl;
    PlayerCameraController _camCtrl;
    PlayerAnimationController _animCtrl;
    PlayerInteractController _interact;
    PlayerWeaponController _weaponCtrl;
    PlayerHealthController _health;

    public PlayerInputController Inputter => _input;
    public PlayerMoveController MoveCtrl => _moveCtrl;
    public PlayerCameraController CamCtrl => _camCtrl;
    public PlayerAnimationController AnimCtrl => _animCtrl;
    public PlayerInteractController Interacter => _interact;
    public PlayerWeaponController WeaponCtrl => _weaponCtrl;
    public PlayerHealthController Health => _health;
    #endregion
    public void SetPM(PlayerManager pm) => _pm = pm;
    void Initialize()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _pv = GetComponent<PhotonView>();
        // _pm = PlayerManager.Instance;    // PM이 2개라 이런식은 안될수도
        _gm = GameManager.Instance;
        if (_pv.IsMine)
        {
            foreach(var skin in _skins)
                skin.gameObject.layer = LayerMask.NameToLayer("PlayerCulling");
            if (IsMe)
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                        _pv.RPC("SetSpawnIndex", RpcTarget.All, i);

            gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
         
            /*_gm.SetMy(this);
            _gm.AddPlayerList(PhotonNetwork.LocalPlayer.UserId, PV.ViewID);*/
        }
        else
        {
            // gameObject.layer = LayerMask.NameToLayer("OtherPlayer");
            // _gm.SetAsOther(this);
        }
        // 이런 방법까진 쓰기 싫었는데 어떤 무기들은 Start쯤에 초기화 완료되서 1프레임 쉰다..
        StartCoroutine(Coroutine_InitGM_NextFrame(1)); 
        // InitWithGM();

        _input = GetComponent<PlayerInputController>();
        _moveCtrl = GetComponent<PlayerMoveController>();
        _camCtrl = GetComponent<PlayerCameraController>();
        _animCtrl = GetComponent<PlayerAnimationController>();
        _interact = GetComponent<PlayerInteractController>();
        _weaponCtrl = GetComponent<PlayerWeaponController>();
        _health = GetComponent<PlayerHealthController>();

        _charCtrl = GetComponent<CharacterController>();
    }
    void InitWithGM()
    {
        if (_pv.IsMine)
        {
            _gm.SetMy(this);
            _gm.AddPlayerList(PhotonNetwork.LocalPlayer.UserId, PV.ViewID);
            switch(_gm.GameMode)
            {
                case GameMode.DeathMatch_Solo:
                    var dmSolo = _gm as GameManager_DeathMatch_Solo;
                    AddEnabledCallbacks(dmSolo.RPC_SetMyPlayerWeapon); break;
            }
        }
        else _gm.SetAsOther(this);
    }
    public void Respawn()   // 게임의 시작 부분에서 호출되는 가장 빠른 RPC인데
                            // 양 측의 모든 플레이어가 생성되도 PV가 없는 경우가 있어서
                            // 특별히 RPC호출 전 PV검사
    {
        if (_pv)
        {
            _pv.RPC("RPC_Respawn", RpcTarget.All);
            return;
        }
        else Debug.Log("PV가 아직 생성되지 않음.");
    }
    public void Respawn(float invoke) => Invoke("Respawn", invoke);
    public void Respawn(Vector3 pos, Vector3 forword) => _pv.RPC("RPC_Respawn", RpcTarget.All, pos, forword);
    public void Respawn(Vector3 pos, Vector3 forword, float wait) => StartCoroutine(Coroutine_WaitRespawn(pos, forword, wait));
    public void AddEnabledCallbacks(Action action)
    {
        if(!_onEnabledCallbacks.Contains(action))
            _onEnabledCallbacks.Add(action);
    }
    public void AddRespanedCallbacks(Action action)
    {
        if(!_onRespawnedCallbacks.Contains(action))
            _onRespawnedCallbacks.Add(action);
    }
    public void OnKill(PlayerController victim)
    {

    }
    public void SetWin() => _pv.RPC("RPC_SetWin", RpcTarget.All);
    public void SetLose() => _pv.RPC("RPC_SetLose", RpcTarget.All);
    public void SetVictory() => _pv.RPC("RPC_SetVictory", RpcTarget.All);
    public void SetDefeat() => _pv.RPC("RPC_SetDefeat", RpcTarget.All);
    [PunRPC] void RPC_Respawn()
    {
        // Debug.Log("RPC_Respawn");
        _health.ResetHealth();
        transform.position = SpawnPoint.position;
        transform.forward = SpawnPoint.forward;
        if (IsMe)
        {
            CameraManager.Instance.SetTarget(_camCtrl.CameraTarget);
            if(_deathCount > 0) _weaponCtrl.ResetAll();
            SyncedEnable();
            foreach (Action action in _onRespawnedCallbacks)
                action?.Invoke();
        }
        _deathCount++;
    }
    [PunRPC] void RPC_Respawn(Vector3 pos, Vector3 forword)
    {        
        _health.ResetHealth();
        transform.position = pos;
        transform.forward = forword;        
        if (IsMe)
        {
            CameraManager.Instance.SetTarget(_camCtrl.CameraTarget);
            if (_deathCount > 0) _weaponCtrl.ResetAll();
            SyncedEnable();
            foreach (Action action in _onRespawnedCallbacks)
                action?.Invoke();
        }
        _deathCount++;
    }
    [PunRPC] void RPC_SetWin()
    {
        _winCount++;
        if (!IsMe) return;
        Invoke("Respawn", 5f);  // 승리 개수가 목표치 이상이여도 SetVictor에서 Invoke캔슬     
    }
    [PunRPC] void RPC_SetLose()
    {
        if (!IsMe) return;
        UI.SetHp(0f, 1f);

        // CameraManager.Instance.SetTarget(_gm.DeathView);
        Invoke("Respawn", 5f);
    }
    [PunRPC] void RPC_SetVictory()
    {
        if (IsInvoking("Respawn"))
            CancelInvoke("Respawn");
    }
    [PunRPC] void RPC_SetDefeat()
    {
        if (IsInvoking("Respawn"))
            CancelInvoke("Respawn");
    }

    IEnumerator Coroutine_WaitRespawn(Vector3 pos, Vector3 forword, float wait)
    {
        yield return Utility.GetWaitForSeconds(wait);
        _pv.RPC("RPC_Respawn", RpcTarget.All, pos, forword);
    }

    private void OnEnable()
    {
        foreach (Action action in _onEnabledCallbacks)
            action?.Invoke();
    }

    protected new void Awake()
    {
        Initialize();
        /*if (PV.IsMine)
        {
            _gm.AddPlayerList(PhotonNetwork.LocalPlayer.UserId, PV.ViewID);
            _gm.SetMy(this);
        }
        else
            _gm.SetEnemy(this);*/
    }
    void Start()
    {
        _ui = PlayerUI.Instance;
        if (IsMe)
        {
            _ui.Initialize(this);
        }
    }
    [PunRPC] void SetSpawnIndex(int index) => _spawnIndex = index;
    [PunRPC] void Tlqkf() { Debug.Log("Fuck Photon!!"); }

    IEnumerator Coroutine_InitGM_NextFrame(int frame)
    {
        for(int i = 0; i < frame; i++) 
            yield return null;

        if(_pv.IsMine)
        {
            _gm.SetMy(this);
            _gm.AddPlayerList(PhotonNetwork.LocalPlayer.UserId, PV.ViewID);
            switch (_gm.GameMode)
            {
                case GameMode.DeathMatch_Solo:
                    var dmSolo = _gm as GameManager_DeathMatch_Solo;
                    /*AddEnabledCallbacks(dmSolo.RPC_SetMyPlayerWeapon);*/ break;
            }
        }
        else _gm.SetAsOther(this);
    }
    void Update()
    {
        if (!IsMe) return;

        if (_health.Health >= 0 && transform.position.y <= -59.59f)
            _health.SetHit(this, 100f);
        /*if (Input.GetKeyDown(KeyCode.K))
        {
            // PV.RPC("Tlqkf", RpcTarget.All);
            _animCtrl.SetTrigger(AnimTrigger.Mow);
        }*/
    }
    private void OnDestroy()
    {
        // if (UI) Destroy(UI.gameObject);
    }
}
