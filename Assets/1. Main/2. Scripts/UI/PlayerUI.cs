using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using System.Linq;

// 여러 가지의 UI클래스들은 거의 다 싱글턴이지만 그 싱글턴들을 초기화 시켜주고 접근하기 편하게 해줌 + 동기화
public class PlayerUI : SingletonMonoBehaviour<PlayerUI>
{
    PhotonView _pv;     // 플레이어UI의 PV.IsMine은 꼭 로컬플레이어와 같지 않을수도 있다!!
    StringBuilder _sb = new StringBuilder();
    GameManager _gm;
    [SerializeField] PlayerController _master;
    PlayerActionMaps _actionMaps;
    [Space]
    [SerializeField] Image _hpFill;
    [SerializeField] Text _hpLabel;
    [Space]
    [SerializeField] Image[] _poseImg = new Image[2];
    // [SerializeField] Sprite[] _poseIcons = new Sprite[2];
    Coroutine _coroutine_PoseCanceled;
    [Space]
    [SerializeField] WeaponIcon[] _weaponIconList = new WeaponIcon[(int)WeaponUsage.Max];
    [SerializeField] Text _magazine;
    [SerializeField] Text _totalAmmo;
    [SerializeField] Text _ammoType;
    // [SerializeField] Image _posture;
    [Space]
    [SerializeField] AttackMarker _attackMarker;
    [SerializeField] Transform _hitIndiPrefab;
    [SerializeField] ThrowableIndicator _throwIndiPrefab;
    [SerializeField] Transform _IndicatorsParent;
    GameObjectPool<Transform> _hitIndiPool = new GameObjectPool<Transform>();
    GameObjectPool<ThrowableIndicator> _throwIndiPool = new GameObjectPool<ThrowableIndicator>();
    Dictionary<IThrowable, ThrowableIndicator> _throwIndiList = new Dictionary<IThrowable, ThrowableIndicator>();
    [Space]
    [SerializeField] RectTransform _meMarker;
    [SerializeField] Minimap _minimap;
    [Space]
    [SerializeField] InteractDesc _interactDesc;
    [SerializeField] Inventory _inventory;

    public PlayerController Master => _master;
    public PlayerActionMaps.UIActions Inputter => _actionMaps.UI;
    bool CanSync => _master && _master.IsMe;
    public WeaponIcon[] WeaponIconList => _weaponIconList;
    public GameObjectPool<ThrowableIndicator> ThrowIndiPool => _throwIndiPool;
    public InteractDesc Interact => _interactDesc;
    public Inventory Inven => _inventory;

    void Allocation()
    {
        _pv = GetComponent<PhotonView>();
        // 겜 시작 시 비활성화되있어야 하지만 Awake에서 초기화 되는 친구들을 Awake에서만 켜주고 Start에서 꺼주니 잘 되더라!!
        if (!_inventory) _inventory = FindAnyObjectByType<Inventory>(FindObjectsInactive.Include);
        if (!_inventory.gameObject.activeSelf)
            _inventory.gameObject.SetActive(true);

        _hitIndiPool.CreatePool(2, () =>
        {
            Transform img = Instantiate(_hitIndiPrefab, _IndicatorsParent);
            img.gameObject.SetActive(false);
            return img;
        });
        _throwIndiPool.CreatePool(2, () =>
        {
            ThrowableIndicator indi = Instantiate(_throwIndiPrefab, _IndicatorsParent);
            indi.Initialize(this);
            indi.gameObject.SetActive(false);
            return indi;
        });
    }
    public void Initialize(PlayerController master)
    {
        _gm = GameManager.Instance;
        _master = master;
        Inven.Initialize(master);
        _minimap.Initialize(_master.transform);
        CreateInput();
    }
    void CreateInput()
    {
        _actionMaps = new PlayerActionMaps();
        _actionMaps.Enable();
    }
    public void SetHp(float hp, float hpMax)
    {
        // float value = hp / hpMax;
        _hpFill.fillAmount = hp / hpMax;
        if (hp < 0.1f && hp > 0) hp = 0.1f;
        _hpLabel.text = hp.ToString("0.0");
    }
    public void SetPose(bool stand)
    {
        int index = stand ? 0 : 1;
        int turnOff = Mathf.Abs(index - 1);
        _poseImg[turnOff].gameObject.SetActive(false);
        _poseImg[index].gameObject.SetActive(true);
    }
    public void PoseCanceled() 
    {
        if (_coroutine_PoseCanceled != null) StopCoroutine(_coroutine_PoseCanceled);
        _coroutine_PoseCanceled = StartCoroutine(Coroutine_PoseCanceled(0.75f));
    }
    public void ResetAll()
    {
        Inven.ResetAll();
        foreach(WeaponIcon wi in _weaponIconList)
            wi.SetIcon(null);
    }

    #region Weapon UI
    public void SetWeaponIcon(WeaponUsage usage, Sprite sprite)
    {
        WeaponIcon icon = _weaponIconList[(int)usage];
        // icon.gameObject.SetActive(sprite != null);
        icon.SetIcon(sprite);
    }
    public void SelectWeapon(WeaponUsage usage/*, bool on*/)
    {
        for (int i = 0; i < (int)WeaponUsage.Max; i++)
        {
            WeaponIcon icon = _weaponIconList[i];
            if (icon.Sprite == null || (WeaponUsage)i == usage) continue;
            icon.SetStow();
        }
        WeaponIcon select = _weaponIconList[(int)usage];
        select.SetHold();
    }
    public void DeselectAll()
    {
        for (int i = 0; i < (int)WeaponUsage.Max; i++)
        {
            WeaponIcon icon = _weaponIconList[i];
            icon.SetStow();
        }
    }
    public void SetMagazineTxt(string text = "") => _magazine.text = text;
    /*{
        if (_magazine)  // 안넣으면 런타임 종료시 에러.. 굳이?
            _magazine.text = text;
    }*/
    public void SetMagazineTxt(int magazine)
    {
        _sb.Clear();
        if(magazine == -1)
        {
            SetMagazineTxt("");
            return;
        }
        _sb.Append("<color=#" + (magazine == 0 ? "ff0000" : "ffffff") + ">");
        _sb.Append(magazine);
        _sb.Append("</color>");
        SetMagazineTxt(_sb.ToString());
    }
    public void SetAmmoInfoAll(int magazine, AmmoType type)
    {
        SetMagazineTxt(magazine);
        if(type == AmmoType.None)
        {
            _ammoType.text = _totalAmmo.text = string.Empty;
            return;
        }
        _sb.Clear();
        switch (type)
        {
            case AmmoType._9_0: _sb.Append("9.0mm탄"); break;
            case AmmoType._7_62: _sb.Append("7.62mm탄"); break;
            case AmmoType._5_56: _sb.Append("5.56mm탄"); break;
            case AmmoType._300_Magnum: _sb.Append("300Magnum탄"); break;
            case AmmoType.Buckshot: _sb.Append("벅샷탄"); break;
        }
        _sb.Append(" ");
        _sb.Append("<color=#" + AmmoItemData.GetColor(type, out string info) 
            + ">(" + info +")</color>");
        _ammoType.text = _sb.ToString();
        SetTotalAmmo(type);
        _sb.Clear();
    }
    public void SetTotalAmmo(AmmoType type)
        => SetTotalAmmoTxt(Inven.AmmoCount(type));
    public void SetTotalAmmoTxt(string text = "") => _totalAmmo.text = text;
    public void SetTotalAmmoTxt(int totalAmmo)
    {
        _sb.Clear();
        _sb.Append(" / ");
        _sb.Append(totalAmmo);
        SetTotalAmmoTxt(_sb.ToString());
    }
    public void UpdateTotalAmmo()
    {
        for (int i = 0; i < _weaponIconList.Length; i++)
        {
            WeaponIcon icon = _weaponIconList[i];
            if (!icon.Selected) continue;

            var weaponCtrl = _master.WeaponCtrl;
            if (weaponCtrl.CurrentWeapon)
            {
                if (weaponCtrl.CurrentWeapon.TryGetComponent<Gun>(out Gun gun))
                    SetTotalAmmoTxt(weaponCtrl.AmmoCount(gun.AmmoType)/*Inven.AmmoCount(gun.AmmoType)*/);
            }
        }
    }
    public void SwapCanceled(WeaponUsage usage)
        => _weaponIconList[(int)usage].SwapCanceled();
    #endregion
    #region Indicators
    public void OnAttack() => _attackMarker.OnAttack();
    public void OnKill() => _attackMarker.OnKill();
    public void SyncedRegistHitIndi(int victimID, Vector3 from, float duration = 2f)
    {
        if (_master.PV.ViewID == victimID) StartCoroutine(Coroutine_RegistHitIndi(from, duration));
        else _pv.RPC("RPC_RegistHitIndi", RpcTarget.Others, victimID, from, duration);
    }
    public void AddThrowIndi(IThrowable tr)
    {
        if (_throwIndiList.ContainsKey(tr)) return;

        ThrowableIndicator indi = _throwIndiPool.Get();
        indi.SetUp(tr.ID, tr);
        indi.gameObject.SetActive(true);
        _throwIndiList.Add(tr, indi);
    }
    public void RemoveThrowIndi(IThrowable tr)
    {
        if(_throwIndiList.ContainsKey(tr))
        {
            ThrowableIndicator indi = _throwIndiList[tr];
            indi.End();
            _throwIndiList.Remove(tr);
        }    
    }
    public void SyncedAddThrowIndi(IDamagable other, IThrowable tr)
        => _pv.RPC("RPC_AddThrowIndi", _gm.GetPlayerByPV(other.PV),  tr.PV.ViewID);
    public void SyncedRemoveThrowIndi(IDamagable other, IThrowable tr)
        => _pv.RPC("RPC_RemoveThrowIndi", _gm.GetPlayerByPV(other.PV), tr.PV.ViewID);
    #endregion

    public void SetMeMarkerActive(bool active) => _meMarker.gameObject.SetActive(active);
    public void SetMeMarkerPos(Vector3 pos) => _meMarker.position = pos;
    public void Inventory()
    {        
        bool active = _inventory.gameObject.activeSelf;
        if(active)
        {
            _inventory.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            _inventory.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public void AddFieldItem(FieldItem item) => _inventory.AddFieldItem(item);
    public void HideFieldItem(FieldItem item) => _inventory.HideFieldItem(item);

    [PunRPC] void RPC_RegistHitIndi(int victimID, Vector3 from, float duration = 2f)
    {
        if (_master == null) return;
        if (_master.PV.ViewID == victimID)
            StartCoroutine(Coroutine_RegistHitIndi(from, duration));
    }
    [PunRPC] void RPC_AddThrowIndi(int throwableID)
    {
        PhotonView pv = PhotonView.Find(throwableID);
        if(pv && pv.TryGetComponent<IThrowable>(out IThrowable tr))
            AddThrowIndi(tr);
    }
    [PunRPC] void RPC_RemoveThrowIndi(int throwableID)
    {
        foreach(IThrowable tr in _throwIndiList.Keys.ToList())
            if(tr.PV.ViewID == throwableID)
                RemoveThrowIndi(tr);
    }

    public static float GetAngle(Vector3 from, Vector3 to)
    {
        to.y = from.y = 0f;
        Vector3 v = to - from;
        return Mathf.Atan2(v.z, v.x)/* * Mathf.Rad2Deg*/;
    }
    #region ====== Coroutines ======
    IEnumerator Coroutine_PoseCanceled(float duration = 1.5f)
    {
        float timer = 0f;
        bool goRed = true;
        while (true)
        {
            float deltaTime = Time.deltaTime;
            timer += deltaTime;
            if (timer >= duration)
            {
                _poseImg[0].color = _poseImg[1].color = Color.white;
                _coroutine_PoseCanceled = null;
                yield break;
            }
            float half = duration / 2f;
            if (goRed && timer >= half)
                goRed = false;
            float offset_BG = goRed ? 1f - timer / half : (timer - half) / half;
            Color newColor = new Color(1f, offset_BG, offset_BG);
            _poseImg[0].color = _poseImg[1].color = newColor;
           yield return null;
        }
    }
    IEnumerator Coroutine_RegistHitIndi(Vector3 point, float duration = 2f)
    {
        float timer = 0f;
        float caseAngle = 0f;

        Transform indi = _hitIndiPool.Get();
        Image icon = indi.GetComponentInChildren<Image>();
        icon.color = Color.red;
        indi.gameObject.SetActive(true);
        while(true)
        {
            float deltaTime = Time.deltaTime;

            Transform cenTr = _master.transform;
            Vector3 from = cenTr.forward;
            Vector3 target = Utility.GetNormalizedDir(point, cenTr.position);
            from.y = target.y = 0f;
            caseAngle = Vector3.SignedAngle(target, from, Vector3.up);
            caseAngle = (caseAngle < 0) ? caseAngle + 360f : caseAngle;
            indi.transform.rotation = Quaternion.Euler(0, 0, caseAngle);

            Color newColor = icon.color;
            newColor.a = 1f - timer/ duration;
            icon.color = newColor;

            if ((timer += deltaTime) >= duration)
            {
                indi.gameObject.SetActive(false);
                _hitIndiPool.Set(indi);
                yield break;
            }
            yield return null;
        }
    }
    #endregion
    protected override void OnAwake()
    {
        base.OnAwake();
        Allocation();       
    }
    protected override void OnStart()
    {
        base.OnStart();
        foreach (WeaponIcon icon in _weaponIconList)
            icon.SetIcon(null);

        DeselectAll();
        SetMagazineTxt("");
        SetTotalAmmoTxt("");

        if (_inventory.gameObject.activeSelf)
            _inventory.gameObject.SetActive(false); 
    }

    private void Update()
    {
        if (!_master || !_master.IsMe) return;
        /*if (Inputter.Menu.WasPressedThisFrame())
            if (Inven.gameObject.activeSelf) Inventory();
            else GameMenu();*/

        bool tabDown = Inputter.Tab.WasPressedThisFrame();
        bool tabUp = Inputter.Tab.WasReleasedThisFrame();
        bool isOver = _gm.IsGameOver;
        if (!isOver)
        {
            if (!_gm.HasLog && tabDown)
                Inventory();
            else
            {
                // if (tabDown || tabUp) GameLog(tabDown);
            }
        }
        else
        {
            if (Inven.gameObject.activeSelf)
                Inventory();
            //if (tabDown || tabUp) GameLog(tabDown);
        }
        // if (Input.GetKeyDown(KeyCode.K)) PoseCanceled();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if(_actionMaps != null)
            _actionMaps.Dispose();
    }
}
