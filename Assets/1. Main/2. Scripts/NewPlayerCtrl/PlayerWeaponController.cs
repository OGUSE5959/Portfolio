using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using HashTable = ExitGames.Client.Photon.Hashtable;
using Unity.VisualScripting;

public enum WeaponUsage
{
    None = -1,
    Main,
    Sub,
    Throw,
    Melee,

    Max
}

[RequireComponent(typeof(PlayerInputController))]
public class PlayerWeaponController : MonoBehaviourPunCallbacks
{
    PlayerController _me;
    PlayerMoveController _moveCtrl;
    PlayerInputController _input;
    PlayerCameraController _camCtrl;
    PlayerAnimationController _animCtrl;
    PlayerInteractController _interact;
    PhotonView _pv;

    Vector3 _weaponStartPos;
    Quaternion _baseWeaponRootRot;
    Quaternion _currWeaponRootRot;
    [SerializeField] bool _isBlocked;

    [SerializeField] Transform _weaponRoot;
    [SerializeField] Transform _weaponCase;
    [SerializeField] Weapon[] _weaponObjs;  // 사실 기능은 밑의 리스트로 다 할 수 있긴 한데 재활용 용
    Dictionary<WeaponID, Weapon> _weaponList    // 아이템이 아닌 실제 플래이어에 내장된 무기를 무기 별 ID를 키로 저장
        = new Dictionary<WeaponID, Weapon>();  
    [SerializeField] FieldWeapon[] _weaponItems // 수집한 무기 필드 아이템
        = new FieldWeapon[(int)WeaponUsage.Max];    
    WeaponUsage _currWeapon = WeaponUsage.None;     // 현제 들고있는 무기 슬롯 종류
    WeaponID _lastActiveWeapon = WeaponID.None;     // 마지막으로 활성화됐던 플레이어 고유 무기
    FieldWeapon[] _fieldWeaponPrefabs;
    List<FieldWeapon> _usedWeapons = new List<FieldWeapon>();

    [Space]
    [SerializeField] SyncedMonoBehaviour[] _helmets;
    [SerializeField] SyncedMonoBehaviour[] _vests;
    FieldArmor _helmetItem;
    FieldArmor _vestItem;
    List<FieldArmor> _usedArmors = new List<FieldArmor>();
    /*ArmorLevel _currHelmet = ArmorLevel.None;
    ArmorLevel _currVest = ArmorLevel.None;*/

    public PlayerController Me => _me;
    public bool IsGameOver => !_me.IsGameOver;
    PlayerUI UI => _me.UI;
    Inventory Inven => UI.Inven;
    Transform CameraTarget => _camCtrl.CameraTarget;
    public Transform WeaponRoot => _weaponRoot;     // 무기루트는 플레이어 입력에 따른 카마라와의 절대적인 회전을 받을 트렌스폼
    public Transform WeaponCase => _weaponCase;     // 무기케이스는 절대 회전에 상대적으로 적용되기 위한 트렌스폼
    public Weapon CurrentWeapon { get
        {
            if (_currWeapon == WeaponUsage.None) return null;
            FieldWeapon item = _weaponItems[(int)_currWeapon];
            if (!item) return null;
            return _weaponList[item.WeaponID];
        } }
    public Weapon CW => CurrentWeapon;
    public bool CanInput => _me.IsMe && CW && !GameManager.Instance.InputLock;
    public bool IsBlocked { get { return _isBlocked; } set { _isBlocked = value; } }
    public FieldArmor CurrHelmet => _helmetItem;
    public FieldArmor CurrVest => _vestItem;

    void Initialize()
    {
        _me = GetComponent<PlayerController>();
        _pv = GetComponent<PhotonView>();
        _moveCtrl = _me.MoveCtrl;
        _input = _me.Inputter;
        _camCtrl = _me.CamCtrl;
        _animCtrl = _me.AnimCtrl;
        _interact = _me.Interacter;

        _weaponObjs = GetComponentsInChildren<Weapon>(true);
        foreach (var weapon in _weaponObjs)
        {
            weapon.Initialize(_me);
            // weapon.gameObject.SetActive(true);
            _weaponList.Add(weapon.ID, weapon);
        }
        _fieldWeaponPrefabs = Resources.LoadAll<FieldWeapon>("Prefabs/FieldItems/Weapons");
        _weaponStartPos = _weaponRoot.localPosition;
        _baseWeaponRootRot = _currWeaponRootRot = WeaponRoot.localRotation;
        if (_me.IsMe)
        {
            foreach (var helmet in _helmets)
            {
                var objs = helmet.GetComponentsInChildren<Transform>(true);
                foreach (Transform obj in objs)
                    obj.gameObject.layer = LayerMask.NameToLayer("PlayerCulling");
            }
        }
        if (!_me.IsMe) return;
        /*_input.RegistAction(PlayerInputType.Item1, InputTiming.Started
            , input =>{ if (_me.IsMe) EquipWeapon(0);});
        _input.RegistAction(PlayerInputType.Item2, InputTiming.Started
            , input => { if (_me.IsMe) EquipWeapon(1); });
        _input.RegistAction(PlayerInputType.Item3, InputTiming.Started
            , input => { if (_me.IsMe) EquipWeapon(2); });
        _input.RegistAction(PlayerInputType.Item4, InputTiming.Started
            , input => { if (_me.IsMe) EquipWeapon(3); });*/
        var playerInput = _input.Actions;
        playerInput.Item1.started += input => { if (_me.IsMe) EquipWeapon(0); };
        playerInput.Item2.started += input => { if (_me.IsMe) EquipWeapon(1); };
        playerInput.Item3.started += input => { if (_me.IsMe) EquipWeapon(2); };
        playerInput.Item4.started += input => { if (_me.IsMe) EquipWeapon(3); };
        playerInput.ThrowItem.started += input => { if (_me.IsMe && CW && !GameManager.Instance.HasLog) ThrowWeapon(); };       

        // bool canSync = _me.IsMine && CW;     Value타입이라 불가 ((CW는 CurrentWeapon
        var weaponInput = _input.Weapon;
        
        weaponInput.B.started += input => { if (CanInput) CW.OnB(); };
        weaponInput.R.started += input => { if (CanInput) CW.OnR(); };
       
        weaponInput.Mouse0.started += input => 
            { if (CanInput) CW.OnMouse0Down(); };
        weaponInput.Mouse0.performed += input => 
            { if (CanInput) CW.OnMouse0(); };
        weaponInput.Mouse0.canceled += input => 
            { if (CanInput) CW.OnMouse0Up(); };

        weaponInput.Mouse1.started += input => 
            { if (CanInput) CW.OnMouse1Down(); };
        weaponInput.Mouse1.performed += input => 
            { if (CanInput) CW.OnMouse1(); };
        weaponInput.Mouse1.canceled += input => 
            { if (CanInput) CW.OnMouse1Up(); };        
    }
    public void ResetAll()  // PV가 Mine일 때만 실행됨
    {
        CameraManager.Instance.VC_FPS.m_Lens.FieldOfView    // FOV는 사실상 무기때문에만 바뀌니까 여기서 초기화
            = CameraManager.Instance.VC_TPS.m_Lens.FieldOfView = 60f;
        ResetWeaponItem();
        ResetArmorItem();

        UI.ResetAll();
    }

    #region Weapon Functions
    public void ResetWeaponPos() => _weaponRoot.localPosition = _weaponStartPos;
    public void SetWeaponRootRecoil(float pitchRecoil, float yawRecoil, float multiOffset)
    {
        Vector3 euler = _currWeaponRootRot.eulerAngles;
        _currWeaponRootRot = Quaternion.Euler(euler.x + pitchRecoil * multiOffset, euler.y + yawRecoil * multiOffset, euler.z);
    }
    void SetWeaponRootRotation()
    {
        _currWeaponRootRot = Quaternion.Lerp(_currWeaponRootRot, _baseWeaponRootRot, 10f * Time.deltaTime);
        _weaponRoot.localRotation = _currWeaponRootRot;
    }
    FieldWeapon GetWeaponItem(WeaponUsage usage)
    {
        if (usage == WeaponUsage.None 
            || (int)usage < 0 || (int)usage >= _weaponItems.Length)
            return null;
        return _weaponItems[(int)usage];
    }
    public FieldWeapon GetCurrentWeapon() => _weaponItems[(int)_currWeapon];
    public bool TryGetCurrentWeapon(out FieldWeapon weapon)
    {
        weapon = GetCurrentWeapon();
        if (!weapon) return false;
        return true;
    }
    public void SetWeaponItem(FieldWeapon item)
    {
        if (GetWeaponItem(item.Usage)) return;

        _weaponItems[(int)item.Usage] = item;
        var data = item.WeaponData;
        var weapon = _weaponList[item.WeaponID]; //_weaponObjs[(int)item.WeaponID];
        switch (data.WeaponType)
        {
            case WeaponType.Gun:
                if (item.TryGetComponent<FieldGun>(out FieldGun gun))
                    ((Gun)weapon).SetUp(gun);
                break;
            case WeaponType.Throw:
                weapon.SetUp(item.WeaponData);
                break;
            case WeaponType.Melee:
                weapon.SetUp(item.WeaponData);
                break;
        }

        if (!_usedWeapons.Contains(item)) _usedWeapons.Add(item);
        UI.SetWeaponIcon(data.usage, data.icon);
        Inven.SetWeaponShape(data.usage, data.weaponShape);
        weapon.OnHold();
        // _pv.RPC("RPC_SetWeaponItem", RpcTarget.All, item.PV.ViewID);
    }

    void OnCW()
    {
        CurrentWeapon.Action();

        bool wpFormUsage = true;//_currWeapon == WeaponUsage.Main || _currWeapon == WeaponUsage.Sub;
        if ((_moveCtrl.IsSprint || !wpFormUsage) && _animCtrl.IsWeaponForm)
            _animCtrl.IsWeaponForm = false;     // 달리는 중인데 무기 자세이면 바꾸기
        else if (!_moveCtrl.IsSprint && !_animCtrl.IsWeaponForm && wpFormUsage)
            _animCtrl.IsWeaponForm = true;      // 달릴지 않는데 무기 자세이면 바꾸기
    }
    void NoCW()
    {
        _camCtrl.ResetCamTargetPosLerp();
        CameraManager.Instance.VC_TPS.m_Lens.FieldOfView = Mathf.Lerp(CameraManager.Instance.VC_TPS.m_Lens.FieldOfView, 60f, 10f * Time.deltaTime);
        if (CameraManager.Instance.IsFPS)
            CrossHair.Instance.ResetLerp(10f * Time.deltaTime);
        else {
            Vector3 targetPos = Vector3.zero;
            Ray ray = new Ray(CameraTarget.position, CameraTarget.forward);
            float dist = 0f; //, scaleOffset = 1f;

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << LayerMask.NameToLayer("Map") | 1 << LayerMask.NameToLayer("OtherPlayer")))
            {
                targetPos = Camera.main.WorldToScreenPoint(hit.point);
                dist = Vector3.Distance(ray.origin, hit.point);
            }
            else
            {
                targetPos = Camera.main.WorldToScreenPoint(ray.origin + ray.direction * 100f);
                dist = 100f;
            }
            // CrossHair.Instance.SetPosition(targetPos/*, 10f * Time.deltaTime*/);
            // CameraManager.Instance.SetTPSTarget(WeaponRoot.position, _camCtrl.CameraTarget.forward);
        }
        // if (Input.GetMouseButtonDown(0)) _me.AnimCtrl.SetTrigger(AnimTrigger.Punch); 
    }
    public bool TrySetWeaponItem(FieldWeapon weapon)
    {
        if (!GetWeaponItem(weapon.Usage))   // 이미 장착한 무기가 없으면
        {
            SetWeaponItem(weapon);
            return true;
        }
        Debug.Log("Already have weapon of " + weapon.Usage + " Slot");
        PlayerUI.Instance.SwapCanceled(weapon.Usage);
        return false;
    } // 아직 해당 칸의 무기가 없으면 _weaponItems[(int)item.Usage]에
      // item할당 return true, 이미 있으면 경고 후 return false
    void EquipWeapon(int index) => EquipWeapon((WeaponUsage)index);
    public void EquipWeapon(WeaponUsage usageIndex)
    {
        var gunItem = GetWeaponItem(usageIndex);
        if (!gunItem)
        {
            if (usageIndex == WeaponUsage.None)
            {
                if (_lastActiveWeapon != WeaponID.None)
                    /*_weaponObjs[(int)_lastActiveWeapon]*/
                    _weaponList[_lastActiveWeapon].SyncedSetActive(false);
                _currWeapon = WeaponUsage.None;
                _lastActiveWeapon = WeaponID.None;
                _animCtrl.IsWeaponForm = false;
            }
            return;
        }
        if (_lastActiveWeapon != WeaponID.None)
            /*_weaponObjs[(int)_lastActiveWeapon]*/
            _weaponList[_lastActiveWeapon].SyncedSetActive(false);
        if (gunItem.Usage == _currWeapon)
        {
            _currWeapon = WeaponUsage.None;
            _lastActiveWeapon = WeaponID.None;
            _animCtrl.IsWeaponForm = false;
            if (_me.IsMe) PlayerUI.Instance.DeselectAll();
            return;
        }
        _currWeapon = usageIndex;
        /*_weaponObjs[(int)gunItem.WeaponID]*/
        _weaponList[gunItem.WeaponID].SyncedSetActive(true);
        _lastActiveWeapon = gunItem.WeaponID;
        _animCtrl.IsWeaponForm = true;

        UI.SelectWeapon(_currWeapon);
        // _me.PV.RPC("RPC_EquipWeapon", RpcTarget.All, (int)usageIndex);return;
    } // 해당 usage인덱스의 아이템이 있으면 아이템의ID에 맞춰
      // _weaponList[item.WeaponID].SyncedEnable()
    public void SycedEquipNone() => _pv.RPC("RPC_EquipNone", RpcTarget.All);
    void ChangeWeaponRoot()
    {
        for (int i = 0; i < _weaponItems.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipWeapon(i); break;
            }
        }
    }
    public void ThrowWeapon()
    {
        if (!_me.IsMe/* || _currWeapon == WeaponUsage.None*/) return;
        var item = GetWeaponItem(_currWeapon);
        var weaponObj = _weaponList[item.WeaponID]; //_weaponObjs[(int)item.WeaponID];

        weaponObj.ResetAll();
        EquipWeapon(_currWeapon);
        // weapon = null;   ※ValueType!!
        _weaponItems[(int)item.Usage] = null;

        item.SyncedSetPosition(_camCtrl.CameraTarget.position);
        item.SyncedEnable();
        if (item.TryGetComponent<FieldGun>(out FieldGun gun))
        {
            Gun gunObj = (Gun)weaponObj;    // OnThrown을 통일하는 방법도 고려
            gun.OnThrown((transform.forward + Vector3.up * 0.1f).normalized, 5f, gunObj.Status);
        }
        else _interact.ThrowItem(item, (CameraTarget.forward + Vector3.up * 0.1f).normalized, 5f);        

        UI.SetWeaponIcon(item.Usage, null);
        UI.Inven.SetWeaponShape(item.Usage, null);

        //_pv.RPC("RPC_ThrowWeapon", RpcTarget.All);       
    }   // 인자값 없음 _currentWeapon에 맞는(장착 중인) FieldItem이 있으면 말 그대로 던짐
    public void AddAmmo(FieldAmmo ammo)
    {
        Inven.AddAmmo(ammo);
        // _pv.RPC("RPC_AddAmmo", RpcTarget.All, ammo.AmmoType, ammo.Count);
    }

    [PunRPC] public void RPC_SetWeaponItem(int viewID)
    {
        var item = /*InteractableManager.Instance.GetInteractable*/PhotonView.Find(viewID)
            .gameObject.GetComponent<FieldWeapon>();
        if (GetWeaponItem(item.Usage)) return;

        _weaponItems[(int)item.Usage] = item;
        var data = item.WeaponData;
        var weapon = _weaponList[item.WeaponID]; //_weaponObjs[(int)item.WeaponID];
        switch (data.WeaponType)
        {
            case WeaponType.Gun:
                if (item.TryGetComponent<FieldGun>(out FieldGun gun))
                    ((Gun)weapon).SetUp(gun);
                break;
            case WeaponType.Throw:
                weapon.SetUp(item.WeaponData);
                break;
            case WeaponType.Melee:
                weapon.SetUp(item.WeaponData);
                break;
        }

        if (_pv.IsMine)
        {
            UI.SetWeaponIcon(data.usage, data.icon);
            Inven.SetWeaponShape(data.usage, data.weaponShape);
        }
    }
    [PunRPC] void RPC_EquipWeapon(int index)
    {
        WeaponUsage usageIndex = (WeaponUsage)index;
        var gunItem = GetWeaponItem(usageIndex);
        if (!gunItem)
        {
            if (usageIndex == WeaponUsage.None)
            {
                if (_lastActiveWeapon != WeaponID.None)
                    /*_weaponObjs[(int)_lastActiveWeapon]*/
                    _weaponList[_lastActiveWeapon].gameObject.SetActive(false);
                _currWeapon = WeaponUsage.None;
                _lastActiveWeapon = WeaponID.None;
                _animCtrl.IsWeaponForm = false;
            }
            return;
        }
        if (_lastActiveWeapon != WeaponID.None)
            /*_weaponObjs[(int)_lastActiveWeapon]*/
            _weaponList[_lastActiveWeapon].gameObject.SetActive(false);
        if (gunItem.Usage == _currWeapon)
        {
            _currWeapon = WeaponUsage.None;
            _lastActiveWeapon = WeaponID.None;
            _animCtrl.IsWeaponForm = false;
            if (_me.IsMe) PlayerUI.Instance.DeselectAll();
            return;
        }
        _currWeapon = usageIndex;
        /*_weaponObjs[(int)gunItem.WeaponID]*/
        _weaponList[gunItem.WeaponID].gameObject.SetActive(true);
        _lastActiveWeapon = gunItem.WeaponID;
        _animCtrl.IsWeaponForm = true;

        if (_me.IsMe)
        {
            UI.SelectWeapon(_currWeapon);
        }
    }
    [PunRPC] void RPC_EquipNone()
    {
        if (_me.IsMe)
            EquipWeapon(WeaponUsage.None);
    }
    [PunRPC] void RPC_ThrowWeapon()
    {       
        var item = GetWeaponItem(_currWeapon);
        var weaponObj = _weaponList[item.WeaponID]; //_weaponObjs[(int)item.WeaponID];

        weaponObj.ResetAll();
        EquipWeapon(_currWeapon);
        // weapon = null;   ※ValueType!!
        _weaponItems[(int)item.Usage] = null;

        if (_pv.IsMine)
        {
            item.SyncedSetPosition(_camCtrl.CameraTarget.position);
            if (item.TryGetComponent<FieldGun>(out FieldGun gun))
            {
                Gun gunObj = (Gun)weaponObj;
                gun.OnThrown((transform.forward + Vector3.up * 0.1f).normalized, 5f, gunObj.Status);
            }
            else _interact.ThrowItem(item, (CameraTarget.forward + Vector3.up * 0.1f).normalized, 5f);

            UI.SetWeaponIcon(item.Usage, null);
            UI.Inven.SetWeaponShape(item.Usage, null);
        }
    }
    public int GetAmmo(AmmoType type, int give) => Inven.GetAmmo(type, give);
    // public int GetAmmo(AmmoType type, int give) => UI.Inven.GetAmmo(type, give);
    public int AmmoCount(AmmoType type) => UI.Inven.AmmoCount(type); //_ammoList[type]; 
    public void SetFireModeIcon(FireMode fireMode, int burstRepeat = 3)
        => UI.WeaponIconList[(int)_currWeapon].SetFireMode(fireMode, burstRepeat);
    public void SetBlockRotation(float gunLength, float distToBlock, bool blocked)
    {
        IsBlocked = blocked;
        if (!IsBlocked)
        {
            _weaponCase.localRotation = Quaternion.Lerp(_weaponCase.localRotation, Quaternion.identity, 10f * Time.deltaTime);
            return;
        }
        float cos = distToBlock / gunLength;
        float angle = Mathf.Acos(cos) * Mathf.Rad2Deg * (_camCtrl.PitchYaw.x > 0 ? 1f : -1f);
        _weaponCase.localRotation = Quaternion.Lerp(_weaponCase.localRotation, Quaternion.Euler(new Vector3(angle, 0f, 0f)), 10f * Time.deltaTime);
    }
    public void SetBlockRotation()
    {
        IsBlocked = false;
        _weaponRoot.localRotation = Quaternion.identity;
    }
    public void ResetWeaponItem()
    {
        SycedEquipNone();

        for (int i = 0; i < _weaponItems.Length; i++)
            ResetWeaponItem((WeaponUsage)i);
        // foreach (FieldWeapon item in _usedWeapons) item.SyncedReset();
    }
    public void ResetWeaponItem(WeaponUsage usage)
    {
        SycedEquipNone();
        int index = (int)usage;
        var item = _weaponItems[index];
        if (!item) return;
        if (_usedWeapons.Contains(item)) _usedWeapons.Remove(item);
        // item.SyncedReset();
        _weaponItems[index] = null;
        CameraManager.Instance.VC_FPS.m_Lens.FieldOfView
            = CameraManager.Instance.VC_TPS.m_Lens.FieldOfView  = 60f;
    }
    #endregion
    #region Armor Functions
    public void SetArmor(FieldArmor item)
    {
        if (!item) return;
        if (item.ArmorType == ArmorType.Helmet)
        {
            if (_helmetItem)
            {
                _helmets[(int)_helmetItem.ArmorLevel].SyncedDisable();
                _interact.ThrowItem(_helmetItem);
                _helmetItem = null;
            }
            _helmets[(int)item.ArmorLevel].SyncedSetActive(true);
            _helmetItem = item;
        }
        else if (item.ArmorType == ArmorType.Vest)
        {
            if (_vestItem)
            {
                _vests[(int)_vestItem.ArmorLevel].SyncedDisable();
                _interact.ThrowItem(_vestItem);
                _vestItem = null;
            }
            _vests[(int)item.ArmorLevel].SyncedSetActive(true);
            _vestItem = item;
        }

        if(!_usedArmors.Contains(item)) _usedArmors.Add(item);
        item.SyncedDisable();
        Inven.SetArmorSlot(item);
        _pv.RPC("RPC_SetArmor", RpcTarget.Others, item.PV.ViewID);  // 보호구는 무기와 다르게 세팅이 곧 장착이다
    }
    public void SetArmorDamaged(ArmorType type, float damage)
    {
        _pv.RPC("RPC_SetArmorDamaged", RpcTarget.All, (int)type, damage);
        if(!_me.IsSingle) return;
        if (type == ArmorType.Helmet)
        {
            if (!CurrHelmet) return;
            CurrHelmet.Durability -= damage;
            if (CurrHelmet.Durability <= 0)
            {
                Inven.ResetArmorSlot(ArmorType.Helmet);
                PutOffArmor(ArmorType.Helmet, true);  
                return;
            }
            Inven.ArmorSlots[(int)ArmorType.Helmet].SetDurability(CurrHelmet.Durability / CurrHelmet.ArmorData.durability);
        }
        else if (type == ArmorType.Vest)
        {
            if (!CurrVest) return;
            CurrVest.Durability -= damage;
            if (CurrVest.Durability <= 0)
            {
                Inven.ResetArmorSlot(ArmorType.Vest);
                PutOffArmor(ArmorType.Vest);
                // CurrVest.SyncedDestroy();
                return;
            }
            Inven.ArmorSlots[(int)ArmorType.Vest].SetDurability(CurrVest.Durability / CurrVest.ArmorData.durability);
        }
    }
    public void PutOffArmor(ArmorType type, bool isBroken = false)
    {
        // 갑옷 아이템을 버리는 것 => 해당 타입의 갑옷 아이템이 있어야 함!!
        if (type == ArmorType.Helmet)
        {
            if (!CurrHelmet) return;

            _helmets[(int)CurrHelmet.ArmorLevel].SyncedDisable();
            if (!isBroken) _interact.ThrowItem(CurrHelmet);
            else _helmetItem.SyncedDisableWithUI();
            _helmetItem = null;
        }
        else if (type == ArmorType.Vest)
        {
            if (!CurrVest) return;

            _vests[(int)CurrVest.ArmorLevel].SyncedDisable();
            if (!isBroken) _interact.ThrowItem(CurrVest);
            else _vestItem.SyncedDisableWithUI();
            _vestItem = null;
        }

        Inven.ResetArmorSlot(type);
        _pv.RPC("RPC_SetArmorNull", RpcTarget.Others, (byte)type);
    }
    /*void OnAromorBroken(ArmorType type)
    {
        FieldArmor item = type == ArmorType.Helmet ? _helmetItem : _vestItem;
        if (item == null) return;

        _pv.RPC("RPC_SetArmorNull", RpcTarget.Others, (byte)type);
    }*/
    public void ResetArmorItem()
    {
        if (CurrHelmet)
        {
            _helmets[(int)CurrHelmet.ArmorLevel].SyncedDisable();
            if (_usedArmors.Contains(_helmetItem)) _usedArmors.Remove(_helmetItem);
            _helmetItem = null;
            Inven.ResetArmorSlot(ArmorType.Helmet);
        }
        if (CurrVest)
        {
            _vests[(int)CurrVest.ArmorLevel].SyncedDisable();
            if (_usedArmors.Contains(_vestItem)) _usedArmors.Remove(_vestItem);
            _vestItem = null;
            Inven.ResetArmorSlot(ArmorType.Vest);
        }
        /*foreach (FieldArmor armor in _usedArmors)
            armor.SyncedReset();*/
    }
    // 이 RPC들은 이름은 같지만 사실 상 클론들을 위한 용도
    [PunRPC] void RPC_SetArmor(int viewID)  
        // 보호구는 맞는 클론들을 위해 PV가 Mine이 아니여도 장착해야겠다..
        // 결과만을 동기화하는게 항상 좋지만은 않다는걸 보여주는 예시
    {
        FieldArmor item = /*InteractableManager.Instance.GetInteractable*/PhotonView.Find(viewID).gameObject.GetComponent<FieldArmor>();
        if (!item) return;
        if (item.ArmorType == ArmorType.Helmet)
            _helmetItem = item;
        else if (item.ArmorType == ArmorType.Vest)
            _vestItem = item;
    }
    [PunRPC] void RPC_SetArmorDamaged(int index, float damage)
    {
        if(!_me.IsMe) return;
        ArmorType type = (ArmorType)index;  
        if (type == ArmorType.Helmet)
        {
            if (!CurrHelmet) return;
            CurrHelmet.Durability -= damage;
            // Debug.Log(CurrHelmet.Durability + ", " + damage);
            if (CurrHelmet.Durability <= 0)
            {
                if (_me.IsMe)
                    Inven.ResetArmorSlot(ArmorType.Helmet);
                PutOffArmor(ArmorType.Helmet, true);  // 벗기 먼저!!
                return;
            }
            Inven.ArmorSlots[(int)ArmorType.Helmet].SetDurability(CurrHelmet.Durability / CurrHelmet.ArmorData.durability);
        }
        else if (type == ArmorType.Vest)
        {
            if (!CurrVest) return;
            CurrVest.Durability -= damage;
            if (CurrVest.Durability <= 0)
            {
                if (_me.IsMe)
                    Inven.ResetArmorSlot(ArmorType.Vest);
                PutOffArmor(ArmorType.Vest, true);
                return;
            }
            Inven.ArmorSlots[(int)ArmorType.Vest].SetDurability(CurrVest.Durability / CurrVest.ArmorData.durability);
        }
    }
    [PunRPC] void RPC_SetArmorNull(byte index)
    {
        ArmorType type = (ArmorType)index;
        if (type == ArmorType.Helmet) _helmetItem = null;
        else if (type == ArmorType.Vest) _vestItem = null;
    }
    #endregion

    private void Start()
    {
        Initialize();
        foreach (var weapon in _weaponObjs)
            weapon.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (!_me.IsMe) return;
        
        if (CurrentWeapon) OnCW();
        else NoCW();
    }
}
