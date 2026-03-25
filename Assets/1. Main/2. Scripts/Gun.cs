using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;



[RequireComponent(typeof(GunAnimCtrl))]
public class Gun : Weapon
{
    const float CamFireRotOffset = 1f;
    public enum SFXType
    {
        Reload,
        RelaodNoAmmo,
        Fire,

        Fire_Silence,

        Max
    }
    CameraManager _cm;
    CrossHair _crossHair;
    [Space]
    [SerializeField] protected GunAnimCtrl _animCtrl;
    CinemachineVirtualCamera _aimVC;
    [SerializeField] Transform _shapeTr;    // shape는 모양이라는 뜻이지만 IK 타겟과 총 발사 Tr이 들어있다
    Quaternion _baseShapeLocalRot;
    Quaternion _currShapeLocalRot;
    [Space]
    [SerializeField] Transform _aimTr;
    [SerializeField] protected Transform _muzzleFireTr;   Vector3 _muzzleStartPos;
    [SerializeField] Transform _muzzleEffectTr;
    [SerializeField] Transform _cartridgeCaseTr;
    protected LayerMask _bulletLayer;
    protected LayerMask _blockLayer;
    protected bool _isAim = false;
    protected bool _isBlocked;
    [Space]
    [Tooltip("장전, 전체장전, 발사, 소음발사")]
    [SerializeField] AudioClip[] _soundEffects = new AudioClip[(int)SFXType.Max];
    [SerializeField] GunStatus _status;
    protected Camera _mainCam;
    Vector3 _tpsCamLerpPos;

    Vector3 _hitPos;

    #region Getter & Setter
    Inventory Inven => Inventory.Instance;
    public Transform AimTr { get { if (_aimTr) return _aimTr; return _aimVC.transform; } }
    public virtual bool CanReload => Magazine < MagazineSize && Inventory.Instance.AmmoCount(AmmoType) > 0;
    public virtual bool CanAim => /*!IsDraw && */!IsAim && !IsReloading && !IsRun; //&& !_isBlocked
    // public override bool IsRun => base.IsRun && _animCtrl.GetBool(GunAnimCtrl.Motion.Run);
    public bool IsAim { get { return _isAim; } set { _animCtrl.SetBool("IsAim", _isAim = value); } }
    public GunAnimCtrl.Motion GetMotion { get { return _animCtrl.GetMotion; } }
    public AmmoType AmmoType => _status.ammoType;
    #region Animation States
    public bool IsIdle => GetMotion == GunAnimCtrl.Motion.Idle;
    public virtual bool IsDraw => GetMotion == GunAnimCtrl.Motion.Draw;
    public virtual bool IsReloading => GetMotion == GunAnimCtrl.Motion.Reload || GetMotion == GunAnimCtrl.Motion.ReloadNoAmmo;
    public bool CanMove
    {
        get
        {
            if (GetMotion == GunAnimCtrl.Motion.Reload || GetMotion == GunAnimCtrl.Motion.ReloadNoAmmo
                || GetMotion == GunAnimCtrl.Motion.Fire) return false; return true;
        }
    }
    public bool IsFire => GetMotion == GunAnimCtrl.Motion.Fire;
    #endregion
    #region Status
    public GunStatus Status => _status;
    int FireModeIndex { get => _status.fireModeIndex; set => _status.fireModeIndex = value; }
    List<FireMode> AbleFireModes { get => _status.ableFireModes; set => _status.ableFireModes = value; }
    public FireMode FireMode => AbleFireModes[FireModeIndex];
    protected int BurstRepeat { get => _status.burstRepeat; set => _status.burstRepeat = value; }
    protected int BurstCounter { get => _status.burstCounter; set => _status.burstCounter = value; }
    protected float FireRate { get => _status.fireRate; set => _status.fireRate = value; }
    protected float FireTimer { get => _status.fireTimer; set => _status.fireTimer = value; }
    protected string FireEffectName { get => _status.fireEffectName; set => _status.fireEffectName = value; }
    public int MagazineSize { get => _status.magazineSize; set => _status.magazineSize = value; }
    public int Magazine { get => _status.magazine; set => _status.magazine = value; }
    protected float AimSpeed { get => _status.aimSpeed; set => _status.aimSpeed = value; }
    protected float AimFOV { get => _status.aimFOV; set => _status.aimFOV = value; }
    protected float RecoilPitchMax { get => _status.recoilPitchMax; set => _status.recoilPitchMax = value; }
    protected float RecoilPitchMin { get => _status.recoilPitchMin; set => _status.recoilPitchMin = value; }
    protected float RecoilYawMax { get => _status.recoilYawMax; set => _status.recoilYawMax = value; }
    protected float RecoilYawMin { get => _status.recoilYawMin; set => _status.recoilYawMin = value; }
    #endregion
    #endregion    

    #region Animation Events
    void AnimEvent_SetIdle()
    {
        if (!_pv.IsMine) return;

        if (BurstCounter < BurstRepeat)
        {
            Invoke("InvokeBurst", 1f / FireRate * 2f / 3f);
            /*BurstCounter++;
            _animCtrl.Play(GunAnimationController.Motion.Fire);*/
        }
        // SyncedPlayAnim(GunAnimCtrl.Motion.Idle, false);  // 문제 발생!
        _animCtrl.Play(GunAnimCtrl.Motion.Idle, false);
    }
    void InvokeBurst(float term)
    {
        BurstCounter++;
        Invoke("SyncedScanFire", term);
        if (BurstCounter >= BurstRepeat)
            FireTimer = - 1f / FireRate;
        // _animCtrl.Play(GunAnimCtrl.Motion.Fire);
    }
    void AnimEvent_Fire()
    {
        if (!_pv.IsMine) return;
        if (Magazine < 1) return;

        if (_pv.IsMine)
        {
            // _pv.RPC("RPC_Fire", RpcTarget.All);
            _pv.RPC("RPC_ScanFire", RpcTarget.All);
            _master.UI.SetMagazineTxt(--Magazine);
        }
    }
    void AnimEvent_ReloadStart() => AimOut();
    void SyncedPlayAnim(GunAnimCtrl.Motion motion, bool isBlend = true) // 써도 되나?
        => _pv.RPC("RPC_PlayAnim", RpcTarget.All, (int)motion, isBlend);

    protected virtual void SyncedScanFire()
    {
        if (!_master.IsMe || Magazine < 1) return;
        // 좀 많이 짜치긴 한데.. 지향사격의 한계 때문에 결국 눈에서부터 발사
        Transform mcTr = _mainCam.transform;
        bool diffLot = Vector3.Dot(mcTr.forward, _aimTr.forward) <= 0.866f; //if (Vector3.Dot(mcTr.forward, _aimTr.forward) <= 0.866f) return;
        SyncedCreateMuzzleEffect(FireEffectName);
        Vector3 startPos = mcTr.position; // + mcTr.forward * Vector2.Distance(new Vector2(mcTr.position.y, mcTr.position.z), new Vector2(_muzzleFireTr.position.y, _muzzleFireTr.position.z));
        Ray aimBased = new Ray(_aimTr.position, mcTr.forward/*_aimTr.forward*/);
        Ray camBased = new Ray(startPos, mcTr.forward);
        if (_master.CamCtrl.RayCastFront(diffLot || (IsAim && _cm.IsFPS) ? aimBased : camBased, out RaycastHit hit, 1000f, _bulletLayer))
        {
            if (((_muzzleFireTr.position.z > hit.point.z && _muzzleFireTr.forward.z > 0)
                || (_muzzleFireTr.position.z < hit.point.z && _muzzleFireTr.forward.z < 0))     // 캠 캐스트 타겟이 너무 가까움 
                && Physics.Raycast(_muzzleFireTr.position, _muzzleFireTr.forward, out RaycastHit hit1, 10f, _bulletLayer))
                hit = hit1;
            else if (Physics.Raycast(_muzzleFireTr.position, Utility.GetNormalizedDir(hit.point, _muzzleFireTr.position)
                , out RaycastHit hit2, 10f, _bulletLayer)) hit = hit2; // 캠 캐스트 타겟이 총구에선 가로막힘
            Debug.DrawRay(camBased.origin, Utility.GetDirection(_hitPos = hit.point, camBased.origin), Color.yellow, 2f);
            HitSomething(hit);
        }
        else MissedShot();

        float pitchRecoil = Random.Range(RecoilPitchMin, RecoilPitchMax) / (IsAim ? 2f : 1f);
        float yawRecoil = Random.Range(RecoilYawMin, RecoilYawMax) / (IsAim ? 1.5f : 1f);
        bool isMove = _master.MoveCtrl.IsMove; // bool sprint = _master.Sprint;

        _master.CamCtrl.SetChangeRotationRecoil(pitchRecoil, yawRecoil, 1f * (isMove ? 1.2f : 1f));
        SetShapeRecoil(pitchRecoil, yawRecoil / 2f, 1.5f * (isMove ? 1.2f : 1f));
        CreateCartridgeCase();
        SyncedPlaySFX(SFXType.Fire);

        _master.UI.SetMagazineTxt(--Magazine);
        if (BurstCounter < BurstRepeat)
            InvokeBurst(1f / FireRate / 2f);
    }
    protected virtual void HitSomething(RaycastHit hit)
    {
        Vector3 effectDir = Utility.GetNormalizedDir(hit.point, hit.transform.position);
        GameObject obj = hit.collider.gameObject;
        if(obj.TryGetComponent<IDamagable>(out IDamagable dmg) )
        {
            if (obj.TryGetComponent<HitParts>(out HitParts parts))
            {
                // Debug.Log(parts.Master, gameObject);
                parts.SetHit(_master, _damage/*, obj.tag*/);
                SyncedMakeBleed(hit.point, effectDir);
                if (parts.IsDie && parts.Master.transform
                    .TryGetComponent<PlayerController>(out PlayerController victim))
                {
                    // parts.Master.gameObject.SetActive(false);
                    OnKill(victim);
                }
                // else PlayerUI.Instance.OnAttack();
            }
            else
            {
                // Debug.Log(dmg.gameObject.name);
                dmg.SetHit(_master, _damage, hit.point);
            } PlayerUI.Instance.OnAttack();
        }       
        else _pv.RPC("RPC_CreateEffect", RpcTarget.All, "BImp_Wood", hit.point);
        SyncedCreateBulletEffect(_muzzleFireTr.position, hit.point);
    }
    protected virtual void MissedShot() => SyncedCreateBulletEffect(_muzzleFireTr.position, _mainCam.transform.position + _mainCam.transform.forward * 100f, 0.5f);
    [PunRPC] protected void RPC_CreateEffect(string effectName, Vector3 position)
    {
        var effect = EffectPool.Instance.CreateEffect("BImp_Wood", position);
        effect.transform.forward = -transform.forward;
    }
    [PunRPC] protected void RPC_PlayAnim(int animIndex, bool isBlend = true) => _animCtrl.Play((GunAnimCtrl.Motion)animIndex, isBlend);
    #endregion
    #region Init & Set
    public override void Initialize(PlayerController master)
    {
        base.Initialize(master);

        // 이런 변수는 꼭 마스터의 초기화로 할당 될 필요는 없지만
        // 문제는 총이 기본적으로 비활성화라서 Awake(), Start()의미가 없음 + 이 Inst는 빨리 호출됨
        _cm = CameraManager.Instance;
        // _cm._onFPSCallbacks.Add(() => _master.CamCtrl.PitchYaw = _master.CamCtrl.PitchYaw + new Vector2(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y));
        _crossHair = CrossHair.Instance;
        if (AbleFireModes.Count == 0)
            AbleFireModes.Add(FireMode.Single);
        BurstCounter = BurstRepeat;

        _muzzleStartPos = _muzzleFireTr.position;
        _mainCam = Camera.main; //master.CamCtrl.MainCam;
    }
    public void SetUp(GunStatus state)
    {
        _status = state;

        if (AbleFireModes.Count == 0)
            AbleFireModes.Add(FireMode.Single);
        BurstCounter = BurstRepeat;
        FireTimer = 1f / FireRate;
    }
    public void SetUp(GunItemData data)
    {
        _damage = data.damage;
        _status = data.baseGunState;

        if (AbleFireModes.Count == 0)
            AbleFireModes.Add(FireMode.Single);
        BurstCounter = BurstRepeat;
        FireTimer = 1f / FireRate;
    }
    public void SetUp(FieldGun gun)
    {
        _damage = gun.GunData.damage;
        SetUp(gun.Status);
    }
    public override void ResetAll()
    {
        base.ResetAll();
        // _animCtrl.SetBool("IsAim", _isAim = false);
        // _aimVC.gameObject.SetActive(false);
    }
    public override void OnHold()
    {
        /*if (GameManager.Instance.HasInven && Notificator.Instance) Notificator.Instance.ToolTip
                  ("탄창: <color=#" + AmmoItemData.GetColor(AmmoType, out string info) + ">" + info + "</color> 총알");*/
        // if (Notificator.Instance) Notificator.Instance.ToolTip("발사 모드 : " + _status.GetFireModeName(FireMode));
    }
    #endregion
    #region Input Funtions
    public override void OnB()
    {
        base.OnB();
        ChangeFireMode();
    }
    public override void OnR()
    {
        base.OnR();
        OnReload();
    }
    public override void OnMouse0Down()
    {
        base.OnMouse0Down();
        /*if(FireMode == FireMode.Single && FireTimer >= 1f / FireRate)
        {
            _animCtrl.Play(GunAnimationController.Motion.Fire);
            FireTimer = 0f;
        }
        else if(FireMode == FireMode.SemiAuto)
            _animCtrl.Play(GunAnimationController.Motion.Fire);
        else if(FireMode == FireMode.Burst && BurstCounter == BurstRepeat)
            _animCtrl.Play(GunAnimationController.Motion.Fire);*/
    }
    public override void OnMouse0()
    {
        base.OnMouse0();
        /*if(FireMode == FireMode.Auto && FireTimer >= 1f / FireRate)
        {
            _animCtrl.Play(GunAnimationController.Motion.Fire);
            FireTimer = 0f;
        }*/
    }
    public override void OnMouse0Up()
    {
        base.OnMouse0Up();        
    }
    public override void OnMouse1Down()
    {
        base.OnMouse1Down();
        AimIn();
    }
    public override void OnMouse1()
    {
        base.OnMouse1();
        // Aiming();
        
    }
    public override void OnMouse1Up()
    {
        base.OnMouse1Up();
        AimOut();
    }
    #endregion

    #region Actions
    public override void Action(bool lockInput = false)
    {
        base.Action(lockInput);

        FireLogic();
        Aiming();
        SetBlocked();

        SetShapeRotation();
    }
    void AimIn()
    {
        if (GetMotion == GunAnimCtrl.Motion.Draw) return;
        if (CanAim) IsAim = true;
    }
    void AimOut() => IsAim = false;
    void Aiming()   // 이 함수에서 처리하는 기능(크로스헤어, FOV etc)이 엄청 많은데 그 중에서도 달리는지
                    // , 에임 중인지, 1인칭인지 등을 판별해야해서 복잡하다..
    {
        _animCtrl.SetBool("IsRun", IsRun);
        Transform ct = _master.CamCtrl.CameraTarget;
        float standardLerp = 10f * Time.deltaTime;
        bool redCH = _isBlocked;
        if (redCH) _crossHair.SetColorLerp(Color.red, standardLerp);
        else _crossHair.SetColorLerp(Color.white, standardLerp);
        if (_cm.IsFPS)
        {
            _crossHair.SetPosition(Utility.ScreenCenter);
            Vector3 newVec = _mainCam.WorldToScreenPoint(_muzzleFireTr.position)
            - Utility.ScreenCenter;
            newVec.z = 0f;
            float dist = Mathf.Sqrt(newVec.sqrMagnitude) / 4f;
            _crossHair.SetBothDistLerp(dist, 10f * Time.deltaTime);
        }
        else
        {
            Vector3 targetPos, dir = Vector3.zero;
            Ray ray = new Ray(_muzzleFireTr.position, _muzzleFireTr.forward);
            Ray mcRay = new Ray(_mainCam.transform.position, _mainCam.transform.forward);
            // _cm.SetTPSTarget(ct.position, ct.forward);
            if (_master.CamCtrl.RayCastFront(out RaycastHit hit, 1000f, 1 << LayerMask.NameToLayer("Map") | 1 << LayerMask.NameToLayer("OtherPlayer"))
                /*Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << LayerMask.NameToLayer("Map") | 1 << LayerMask.NameToLayer("OtherPlayer"))*/)
            {
                _hitPos = hit.point;
                dir = Utility.GetNormalizedDir(hit.point, _mainCam.transform.position);
                targetPos = _mainCam.WorldToScreenPoint(hit.point);

                /*if (IsIdle && !_isBlocked)
                    _cm.SetTPSTarget(Root.position, IsRun ? ct.forward : dir);
                else _cm.SetTPSTarget(Root.position, ct.forward);*/
                // Debug.DrawRay(_mainCam.transform.position, _mainCam.transform.forward * 100f, Color.red, 1f);
            }            
            else
            {
                targetPos = _mainCam.WorldToScreenPoint(ray.origin + ray.direction * 100f);
                // _cm.SetTPSTarget(Root.position, ct.forward);
            }

            _tpsCamLerpPos = Vector3.Lerp(_tpsCamLerpPos, targetPos, 20f * Time.deltaTime);
            _crossHair.SetBothDistLerp(IsAim ? 20f : 40f, 10f * Time.deltaTime);
        }

        if (IsRun)     // 아예 플레이어한테 New Input System을 주고 뛸 때(started)에 함수를 넣을까 생각중
        {
            _master.CamCtrl.ResetCamTargetPosLerp();
            _crossHair.SetAlphaLerp(0f, standardLerp);
            _cm.VC_FPS.m_Lens.FieldOfView = Mathf.Lerp(_cm.VC_FPS.m_Lens.FieldOfView, 60f, standardLerp);
            _cm.VC_TPS.m_Lens.FieldOfView = Mathf.Lerp(_cm.VC_TPS.m_Lens.FieldOfView, 60f, standardLerp);
            if (_isAim)
            {
                AimOut();
            }
            else
            {

            }
        }
        else
        {
            if (IsAim)
            {
                ct.position = Vector3.Lerp(ct.position, AimTr.position, 30f * AimSpeed * Time.deltaTime);

                if (_cm.IsFPS)
                {
                    _crossHair.SetAlphaLerp(0f, standardLerp);
                    _cm.VC_FPS.m_Lens.FieldOfView = Mathf.Lerp(_cm.VC_FPS.m_Lens.FieldOfView, AimFOV, standardLerp);
                }
                else
                    _cm.VC_TPS.m_Lens.FieldOfView = Mathf.Lerp(_cm.VC_TPS.m_Lens.FieldOfView, 40f, standardLerp);
            }
            else
            {
                _master.CamCtrl.ResetCamTargetPosLerp();
                _cm.VC_FPS.m_Lens.FieldOfView = Mathf.Lerp(_cm.VC_FPS.m_Lens.FieldOfView, 60f, 10f * standardLerp);
                _cm.VC_TPS.m_Lens.FieldOfView = Mathf.Lerp(_cm.VC_TPS.m_Lens.FieldOfView, 60f, 10f * standardLerp);
                if (!IsRun) _crossHair.SetAlphaLerp(1f, standardLerp);
            }
        }
        if (_master.Inputter.Weapon.Mouse1.IsPressed() && !_gm.InputLock)
            AimIn();
    }
    protected virtual void OnReload()
    {
        if (!_pv.IsMine || IsReloading || _master.WeaponCtrl.AmmoCount(AmmoType) == 0) return;
        if (Magazine < MagazineSize)
        {
            if (Magazine != 0/*_master.GetLeftAmmo(_ammoType) > 0*/)
            {
                _animCtrl.Play(GunAnimCtrl.Motion.Reload);
                _pv.RPC("RPC_PlaySFX", RpcTarget.All, (int)SFXType.Reload);
            }
            else
            {
                _animCtrl.Play(GunAnimCtrl.Motion.ReloadNoAmmo);
                _pv.RPC("RPC_PlaySFX", RpcTarget.All, (int)SFXType.RelaodNoAmmo);
            }
        }
    }
    public virtual void ResetAmmo()
    {
        if (!_pv.IsMine) return;
        int ammo = _master.WeaponCtrl.GetAmmo(AmmoType, MagazineSize - Magazine);
        Magazine += ammo;
        // Inven.DiscountAmmo(AmmoType, ammo);

        _master.UI.SetMagazineTxt(Magazine);
        _master.UI.SetTotalAmmo(AmmoType);
    }
    protected virtual bool FireCondition()
    {
        if (_gm.InputLock || IsReloading || _isBlocked
            || _animCtrl.GetBool(GunAnimCtrl.Motion.Run)) return false;

        var mouse0 = _master.Inputter.ActionMaps.Weapon.Mouse0;
        bool getMouseDown = mouse0.WasPressedThisFrame();
        bool getMouse = mouse0.IsPressed();
        bool fireCoolDown = FireTimer >= 1f / FireRate;
        bool isRun = _master.MoveCtrl.IsSprint;

        if (Magazine > 0 && !isRun)
        {
            switch (FireMode)
            {
                case FireMode.Single:
                    if (fireCoolDown)
                    {
                        if (getMouseDown)
                        {
                            FireTimer = 0f;
                            return true;
                        }
                    }
                    return false;
                case FireMode.Burst:
                    if (BurstCounter >= BurstRepeat && !IsFire
                        && fireCoolDown && getMouseDown)
                    {
                        BurstCounter = 0;
                        BurstCounter++;
                        return true;
                    }
                    return false;
                case FireMode.SemiAuto: return getMouseDown;
                case FireMode.Auto:
                    if (fireCoolDown)
                    {
                        if (getMouse)
                        {
                            FireTimer = 0f;
                            return true;
                        }
                    }
                    return false;
            }
        }
        return false;
    }
    void FireLogic()
    {
        if (FireRate > 0 && FireTimer < 1 / FireRate)
            FireTimer += Time.deltaTime;

        if (_master.Inputter.ActionMaps.Weapon.Mouse0.WasPressedThisFrame() && Magazine <= 0)
            Notificator.Instance.ToolTip("탄창이 비었습니다, 탄약이 있다면 R키를 눌러 장전해주세요."); //Debug.Log("No Ammo");
        if (FireCondition())
        {
            if (Magazine <= 0) OnReload();
            else SyncedScanFire(); //_animCtrl.Play(GunAnimCtrl.Motion.Fire);
        }
    }
    void ChangeFireMode()
    {
        if (AbleFireModes.Count > 1 && IsIdle)
        {
            if (++FireModeIndex >= AbleFireModes.Count)
                FireModeIndex = 0;
            if (Notificator.Instance) Notificator.Instance
                    .ToolTip("발사 모드 : " + _status.GetFireModeName(FireMode));
        }
        _master.WeaponCtrl.SetFireModeIcon(FireMode, BurstRepeat);        
    }
    public void SetShapeRecoil(float pitchRecoil, float yawRecoil, float multiOffset)
    {
        Vector3 euler = _currShapeLocalRot.eulerAngles;
        _currShapeLocalRot = Quaternion.Euler(Utility.NormalizeAngle(euler.x + pitchRecoil * multiOffset)
            , Utility.NormalizeAngle(euler.y + yawRecoil * multiOffset),Utility.NormalizeAngle(euler.z));
    }
    void SetShapeRotation()
    {
        float diff = Mathf.Sqrt((_currShapeLocalRot.eulerAngles - _baseShapeLocalRot.eulerAngles).sqrMagnitude);
        if (diff < 0.001f) return;
        _currShapeLocalRot = Quaternion.Lerp(_currShapeLocalRot, _baseShapeLocalRot, 10f * Time.deltaTime);
        _shapeTr.localRotation = _currShapeLocalRot;
    }
    void SetBlocked()
    {
        var ctrl = _master.WeaponCtrl;
        Vector3 startPos = ctrl.WeaponRoot.position;
        Vector3 dir = Utility.GetNormalizedDir(_muzzleFireTr.position, startPos);

        Ray ray = new Ray(startPos, dir);
        float distToMuzzle = Vector3.Distance(startPos, _muzzleFireTr.position);

        _isBlocked = Physics.Raycast(ray, out RaycastHit hit, distToMuzzle, _blockLayer);
        ctrl.SetBlockRotation(distToMuzzle, hit.distance, _isBlocked);

        Debug.DrawRay(startPos, dir * distToMuzzle, Color.yellow);
    }
    #endregion

    #region ====== SFX
    AudioClip GetSFX(SFXType type) => GetSFX((int)type);
    AudioClip GetSFX(int index) => _soundEffects[index];
    public void PlaySFX(SFXType type) => PlaySFX(GetSFX(type));
    void PlaySFX(int index) => PlaySFX(GetSFX(index));
    protected void SyncedPlaySFX(SFXType type)
    {
        PlaySFX((int)type);
        _pv.RPC("RPC_PlaySFX", RpcTarget.Others, (int)type);
    }
    [PunRPC]
    protected virtual void SyncedCreateEffect(string effectName, Vector3 pos, Vector3 forward)
    {
        EffectPool.Instance.CreateEffect(effectName, pos).transform.forward = forward;
        _pv.RPC("RPC_CreateEffect", RpcTarget.Others, effectName, pos, forward);
    }
    protected virtual void SyncedCreateMuzzleEffect(string fireEffectName)
    {
        if (string.IsNullOrEmpty(fireEffectName)) return;
        float randAbs = 15f;
        float xRot = Random.Range(-randAbs, randAbs);
        RPC_CreateMuzzleEffect(fireEffectName, Quaternion.Euler(new Vector3(xRot, -90f, 0f)));
        _pv.RPC("RPC_CreateMuzzleEffect", RpcTarget.Others, fireEffectName, Quaternion.Euler(new Vector3(xRot, -90f, 0f)));
    }
    protected virtual void SyncedCreateBulletEffect(Vector3 start, Vector3 end, float duration = 0.1f)
    {
        RPC_CreateBulletEffect(start, end, duration);
        _pv.RPC("RPC_CreateBulletEffect", RpcTarget.Others, start, end, duration);    
        // GunEffectManager.Instance.SyncedCreateBullet(start, end, duration);
    }
    protected virtual void CreateCartridgeCase() 
        => GunEffectManager.Instance.SyncedCreateCartridgeCase(_cartridgeCaseTr.position, _cartridgeCaseTr.forward);
    [PunRPC] protected void RPC_PlaySFX(int index) => PlaySFX(index);
    [PunRPC] protected void RPC_CreateEffect(string effectName, Vector3 pos, Vector3 forward) => EffectPool.Instance.CreateEffect(effectName, pos).transform.forward = forward;
    [PunRPC] protected void RPC_CreateMuzzleEffect(string fireEffectName, Quaternion randRot)
    {
        var flash = EffectPool.Instance.CreateEffect(fireEffectName, _muzzleEffectTr.position);
        flash.transform.SetParent(_muzzleEffectTr);
        flash.transform.forward = -_muzzleEffectTr.right;
        flash.transform.localRotation = randRot;
    }
    [PunRPC] protected void RPC_CreateBulletEffect(Vector3 start, Vector3 end, float duration = 0.1f) => GunEffectManager.Instance.CreateBullet(start, end, duration);
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_hitPos, 0.1f);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        // CreateInput();
        BurstCounter = BurstRepeat;
        if (CanSync && _master.UI)
        {
            _cm.VC_TPS.m_Lens.FieldOfView = 60f;
            _master.CamCtrl._pitchOffset -= 14.6f;
            _master.UI.SetAmmoInfoAll(Magazine, AmmoType);
            _master.WeaponCtrl.SetFireModeIcon(FireMode, BurstRepeat);
            if (Notificator.Instance) Notificator.Instance.ToolTip("발사 모드 : " + _status.GetFireModeName(FireMode));
        }
        if (!_animCtrl)
        {
            _animCtrl = GetComponent<GunAnimCtrl>();
            _animCtrl.Initialize();
        }
        _animCtrl.Play(GunAnimCtrl.Motion.Draw);
        if (_animCtrl.Animator)
            _animCtrl.SetFloat("AimSpeed", 1 / AimSpeed);
        else Debug.LogWarning("Assign GunAnimCtrl's Animator in Inspector First!");
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (!CanSync || !_master.UI || _master.gameObject == null) return;
        _cm.VC_TPS.m_Lens.FieldOfView = 60f;
        _master.UI.SetAmmoInfoAll(-1, AmmoType.None);
        _master.WeaponCtrl.SetFireModeIcon(FireMode.None);
        _master.WeaponCtrl.SetBlockRotation(0, 0, false);
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _bulletLayer = 1 << LayerMask.NameToLayer("Map") | 1 << LayerMask.NameToLayer("OtherPlayer") | 1 << LayerMask.NameToLayer("Interactable");
        _baseShapeLocalRot = _currShapeLocalRot = _shapeTr.localRotation;
        _aimVC = GetComponentInChildren<CinemachineVirtualCamera>(true);        
        if (_blockLayer == 0) _blockLayer = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Map");
        // _master.CamCtrl.SetAimSpeed(AimSpeed);
    }
    /*private void LateUpdate()
    {
        Debug.Log("1 " + transform.position);
    }*/
    /*private void OnAnimatorMove()
    {
        if (_fired)
            Debug.Log("1 " + transform.localPosition);
    }*/
}
