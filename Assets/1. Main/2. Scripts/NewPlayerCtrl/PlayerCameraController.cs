using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PlayerController))]
public class PlayerCameraController : MonoBehaviour
{
    CameraManager _cm;
    PlayerController _me;
    PlayerInputController _input;

    Camera _mainCam;
    CinemachineBrain _brain;
    [Space]
    [Header("   Camera")]
    [Space]
    [SerializeField] Transform _spineTarget;
    [SerializeField] Transform _headTr;
    [SerializeField] Transform _cameraCase;
    [SerializeField] Transform _cameraTarget;
    Quaternion _leanRot;
    Quaternion _targetRot;
    [SerializeField] Quaternion _lookRot;
    Quaternion _recoilRot;
    Vector3 _camStartPos;
    Vector3 _weaponCamDiffer;
    [Space]
    [SerializeField] float _look_Horizontal_Sensitivity = 90;
    [SerializeField] float _look_Vertical_Sensitivity = 90;
    float _look_Scope_Sensitivity = 1f;
    [Space]
    [Tooltip("Negative to Look up")]
    [SerializeField] float _verticalTopClamp = -40f;
    [Tooltip("Positive to Look up")]
    [SerializeField] float _verticalBottomClamp = 70f;
    [SerializeField] float _crouchVertiBtmClamp = 50f;
    [Space]
    [SerializeField] float _leanAngleAbs = 30f;
    float _cameraPitch;
    public float _pitchOffset;
    float _cameraYaw;
    float _spineRoll;
    [SerializeField] float _lerpRotSpeed = 10f;
    public Transform CameraTarget => _cameraTarget; // 사실 FPS타겟이라는 이름이 더 정확함
    Vector2 _currentRot;

    public bool IsFPS => _cm.IsFPS;
    public bool IsGameOver => _me.IsGameOver;
    // bool IsMenuOn => PlayerUI.Instance.Menu.gameObject.activeSelf;
    public Camera MainCam { get { return _mainCam; } }
    public Vector2 PitchYaw { get { return _currentRot; } set { _cameraPitch = value.x; _cameraYaw = value.y; } }
    public Quaternion LookRot => _lookRot;
    public float HorizontalSensitivity => _look_Horizontal_Sensitivity;
    public float VerticalSensitivity => _look_Vertical_Sensitivity;

    Transform WeaponRoot => _me.WeaponCtrl.WeaponRoot;
    float MouseX => _input.MouseX;
    float MouseY => _input.MouseY;
    bool IsLeanInputLeft => _input.Actions.Lean_Left.IsPressed();
    bool IsLeanInputRight => _input.Actions.Lean_Right.IsPressed();

    void Initialize()
    {
        _cm = CameraManager.Instance;
        _me = GetComponent<PlayerController>();
        // bool canSync = _me.IsMe;
        _input = _me.Inputter;       
        
        if (_me.IsMe) 
            CameraManager.Instance.SetTarget(_cameraTarget);

        _mainCam = Camera.main;
        _brain = _mainCam.GetComponent<CinemachineBrain>();
        _camStartPos = _cameraTarget.localPosition;
        _weaponCamDiffer = _cameraCase.position - _me.WeaponCtrl.WeaponRoot.position;

        _look_Horizontal_Sensitivity = PlayerOption.GetFloatOption(FloatOptionType.Sensitivity_Horizontal);
        _look_Vertical_Sensitivity = PlayerOption.GetFloatOption(FloatOptionType.Sensitivity_Vertical);
        _look_Scope_Sensitivity = PlayerOption.GetFloatOption(FloatOptionType.Sensitivity_Scope);
        PlayerOption.AddFloatValueChangedCallback(FloatOptionType.Sensitivity_Horizontal, call => _look_Horizontal_Sensitivity = call);
        PlayerOption.AddFloatValueChangedCallback(FloatOptionType.Sensitivity_Vertical, call => _look_Vertical_Sensitivity = call);
        PlayerOption.AddFloatValueChangedCallback(FloatOptionType.Sensitivity_Scope, call => _look_Scope_Sensitivity = call);

        if (!_me.IsMe) return;
        _input.Actions.Lean_Left.started += input => { if (_me.IsMe) OnLeanLeft(); };
        _input.Actions.Lean_Left.canceled += input => { if (_me.IsMe) OffLeanLeft(); };
        _input.Actions.Lean_Right.started += input => { if (_me.IsMe) OnLeanRight(); };
        _input.Actions.Lean_Right.canceled += input => { if (_me.IsMe) OffLeanRight(); };
        _input.Actions.ChangeView.started += input =>
        {
            if (!_me.IsMe) return; _cm.SetView(!IsFPS); 
            PlayerUI.Instance.SetMeMarkerActive(!IsFPS);
        };
    }
    public void SetHorizontalSensitivity(float  sensitivity) => _look_Horizontal_Sensitivity = sensitivity;
    public void SetVierticalSensitivity(float sensitivity) => _look_Vertical_Sensitivity = sensitivity;
    public void SetScopeSensitivity(float sensitivity) => _look_Scope_Sensitivity = sensitivity;
    public bool RayCastFront(out RaycastHit hitInfo, float maxDist = 1000f, int layerMask = 1)  // 기본적으로 MainCam 기준
    {
        Ray ray = new Ray(_mainCam.transform.position, _mainCam.transform.forward); //new Ray(CameraTarget.position, CameraTarget.forward);
        return RayCastFront(ray, out hitInfo, maxDist, layerMask);
    }
    public bool RayCastFront(Ray ray, out RaycastHit hitInfo, float maxDist = 1000f, int layerMask = 1)  // 기본적으로 MainCam 기준
    {
        if (Physics.Raycast(ray, out hitInfo, maxDist, layerMask))
            return true;
        return false;
    }
    void LookAround()
    {
        float inputLock = GameManager.Instance.InputLock ? 0f : 1f; // 특정 상황에 대한 제약
        // FOV에 따라 반동이 세게 느껴지기에 기본 60으로 나눠줌
        float scopeOffset = _cm.CurrentFOV < 60f ? _look_Scope_Sensitivity * _cm.CurrentFOV / 60f : 1f;
        float deltaTIme = Time.deltaTime;
        // 감도가 적용된 입력 값 두개를 각각 저장
        _cameraPitch -= MouseY * _look_Vertical_Sensitivity 
            * scopeOffset * deltaTIme * inputLock;
        _cameraPitch = Mathf.Clamp(_cameraPitch, _verticalTopClamp, _me.MoveCtrl.Crouch ? _crouchVertiBtmClamp : _verticalBottomClamp);
        _cameraYaw += MouseX * _look_Horizontal_Sensitivity 
            * scopeOffset * deltaTIme * inputLock;
        _cameraYaw = Utility.NormalizeAngle(_cameraYaw);    // 좌우를 구별하기 쉬운 -180~180 범위로 맞춥니다
        transform.rotation = Quaternion.Euler(new Vector3(0f, _cameraYaw, 0f));     //  Y축은 바로 적용시키기

        float recoilLerp = 5f;      // 반동 회전을 잡기 위한 Linear보정
        float lookLerp = 30f;       // 타겟 회전을 따라가는 Linear보정
        _recoilRot = Quaternion.Lerp(_recoilRot, Utility.QI, recoilLerp * deltaTIme);   // 지금은 안쓰는 쿼터니언입니다
        // 마우스 입력과 반동이 적용된 목표 회전
        _targetRot = Quaternion.Euler(new Vector3(_cameraPitch, _cameraYaw,0f /*+ _spineRoll*/) + _recoilRot.eulerAngles);    
        _lookRot = Quaternion.Lerp(_lookRot, _targetRot, lookLerp * deltaTIme);  // 부드럽게 따라갈 최종 회전

        if (IsFPS) _headTr.rotation = _targetRot;   // 1인칭이면 머리 리그에 타겟회전 적용
        _cameraTarget.rotation = _lookRot;          // 카메라와 무기 루트는 부드럽게 적용
        WeaponRoot.rotation = _lookRot;

        _leanRot = Quaternion.Lerp(_leanRot, Quaternion.Euler(new Vector3(0f, _cameraYaw, _spineRoll)), lookLerp * deltaTIme);
        //_me.WeaponCtrl.WeaponRoot.rotation = _lookRot;
        _currentRot = Vector2.Lerp(_currentRot, new Vector2(_cameraPitch, _cameraYaw), _lerpRotSpeed * deltaTIme);

        if (!_me.WeaponCtrl.CW)
            if (RayCastFront(out RaycastHit hit, 1000f
                , 1 << LayerMask.NameToLayer("Map") /*| 1 << LayerMask.NameToLayer("Interactable")*/))
                _cm.SetTPSTarget(CameraTarget.position, Utility.GetNormalizedDir(hit.point, _mainCam.transform.position));
            else _cm.SetTPSTarget(CameraTarget.position, CameraTarget.forward);

        _me.AnimCtrl.SetFloat(AnimFloat.VerticalAngle, _cameraPitch);

        if (!IsFPS) PlayerUI.Instance.SetMeMarkerPos(_mainCam.WorldToScreenPoint(_headTr.position + _headTr.up * 0.3f));
    }
    void LookAroundByAnim()
    {
        if (GameManager.Instance.InputLock) return;
        float deltaTIme = Time.deltaTime;
        _cameraPitch -= MouseY * _look_Vertical_Sensitivity * deltaTIme;
        _cameraPitch = Mathf.Clamp(_cameraPitch, _verticalTopClamp, _verticalBottomClamp);

        _cameraYaw += MouseX * _look_Horizontal_Sensitivity * deltaTIme;
        _cameraYaw = Utility.NormalizeAngle(_cameraYaw);

        transform.rotation = Quaternion.Euler(new Vector3(0f, _cameraYaw, 0f));

        _cameraTarget.rotation = Quaternion.Euler(new Vector3(_cameraTarget.eulerAngles.x, _cameraTarget.eulerAngles.y, 0f));
        // _cameraCase.rotation = Quaternion.Euler(new Vector3(_cameraCase.eulerAngles.x, _cameraCase.eulerAngles.y, 0f));
        /*_cameraCase.rotation = Quaternion.Euler(new Vector3(_cameraCase.eulerAngles.x, _cameraCase.eulerAngles.y, 0f));
        _cameraTarget.localRotation = Quaternion.Lerp(_cameraTarget.localRotation, Quaternion.Euler(_pitchOffset, 0f, 0f), 10f * Time.deltaTime);
        _me.WeaponCtrl.WeaponRoot.forward = _cameraTarget.forward;*/
        _currentRot = Vector2.Lerp(_currentRot, new Vector2(_cameraPitch, _cameraYaw), _lerpRotSpeed * deltaTIme);
        // SetFloat(AnimFloat.HorizontalAngle, angle.y);
        _me.AnimCtrl.SetFloat(AnimFloat.VerticalAngle, PitchYaw.x);
        if (!_me.WeaponCtrl.CW) ResetCamTargetPosLerp();
    }
    public void ResetCamTargetPosLerp()
           => _cameraTarget.localPosition = Vector3.Lerp(_cameraTarget.localPosition, _camStartPos, 10f * Time.deltaTime);

    // pv
    public void SetAimSpeed(float speed)
    {
        _brain.m_DefaultBlend.m_Time = speed;
    }
    public void SetSelfCotrolRecoil(Vector3 addRot)
    {
        _recoilRot.eulerAngles += addRot;
    }
    public void SetChangeRotationRecoil(float pitch, float yaw, float multiOffset = 1f)
    {
        _cameraPitch += pitch * multiOffset;
        _cameraYaw += yaw * multiOffset;
    }

    public void OnLean(bool isLeft)
    {
        _spineRoll = isLeft ? _leanAngleAbs : -_leanAngleAbs;
    }
    public void OffLean()
    {
        _spineRoll = 0f;
    }
    void OnLeanLeft()
    {
        OnLean(true);
    }
    void OffLeanLeft()
    {
        if (!IsLeanInputRight)
            OffLean();
        else OnLean(false);
    }
    void OnLeanRight()
    {
        OnLean(false);
    }
    void OffLeanRight()
    {
        if (!IsLeanInputLeft)
            OffLean();
        else OnLean(true);
    }
    public void SyncedSetCamTr(Vector3 position, Vector3 forward) => _me.PV.RPC("RPC_SetCamTr", RpcTarget.All, position, forward);
    [PunRPC] void RPC_SetCamTr(Vector3 position, Vector3 forward)
    {
        _cameraTarget.position = position;
        _cameraTarget.forward = forward;
    }
    [PunRPC] void RPC_SetWriteRotations(Quaternion target, Quaternion look)
    {
        _targetRot = target;
        _lookRot = look;        
    }
    [PunRPC] void RPC_SetWeaponRootPosition(Vector3 position)
    {
        if (!_me) _me = GetComponent<PlayerController>();
        _me.WeaponCtrl.WeaponRoot.position = position;
    }

    private void Start()
    {
        Initialize();
    }
    private void OnAnimatorMove()
    {
        if (!_me.IsMe || IsGameOver) return;
        // _headTr.rotation = _targetRot;
        /*_cameraTarget.rotation = WeaponRoot.rotation = _lookRot;
        _cameraTarget.rotation = WeaponRoot.rotation = Quaternion.Euler(new Vector3(_cameraTarget.eulerAngles.x, _cameraTarget.eulerAngles.y, 0f));*/
    }
    /*private void Update()
    {
        if (!_me.IsMe || IsGameOver) return;

        *//*if (IsFPS)
            _headTr.rotation = _targetRot;
        _cameraTarget.rotation = _lookRot;
        WeaponRoot.rotation = _lookRot;*//*
    }*/
    private void LateUpdate()
    {
        // if (IsGameOver) return;

        if (_me.IsMe)
        {
            // _me.PV.RPC("RPC_SetWeaponRootPosition", RpcTarget.Others, WeaponRoot.position);
            /*if (GameMenu.Instance.gameObject.activeSelf || Inventory.Instance.gameObject.activeSelf)
                _cameraTarget.rotation = _lookRot;
            else*/ LookAround();
        }
        else
        {
            float y = transform.eulerAngles.y;
            _headTr.rotation = Quaternion.Euler(_headTr.eulerAngles.x, y, 0f);
            WeaponRoot.rotation = Quaternion.Euler(WeaponRoot.eulerAngles.x, y, 0f);
        }
        /*else
        {
            Quaternion rot = CameraTarget.rotation;
            rot.z = 0f;
            CameraTarget.rotation = _headTr.rotation = WeaponRoot.rotation = rot;
        }*/
        /*else
        {
            if (!_me) _me = GetComponent<PlayerController>();
            _headTr.rotation = _targetRot;
            // CameraTarget.rotation = _me.WeaponCtrl.WeaponRoot.rotation = _lookRot;
        }      */
    }
}