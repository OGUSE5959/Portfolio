using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerMoveController : MonoBehaviour
{
    PlayerController _me;
    PlayerInputController _input;
    PlayerWeaponController _weaponCtrl;
    PlayerAnimationController _animCtrl;

    Vector3 _inputVec;
    bool _move;
    bool _sprint;
    bool _crouch;

    CharacterController _charCtrl;
    float _standHeight;
    float _crouchHeight;

    Vector3 _animVec;
    [Space]
    [Header("   Movement")]
    [Space]
    [SerializeField] float _moveSpeed = 3f;
    [SerializeField] float _sprintSpeed = 6f;
    [SerializeField] float _speedLerp = 10f;
    [SerializeField] float _speedOffset = 0.1f;
    float _finalSpeed;

    [Space]
    [SerializeField] bool _grounded;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] Vector3 _groundCheckOffset;
    [SerializeField] float _groundCheckRadius;
    [SerializeField] float _jumpHeight = 1.2f;
    [SerializeField] float _gravity = -8.9f;
    [SerializeField] float _terminalVelocity = -53f;
    [SerializeField] float _verticalVelocity;
    Vector3 _currentForce = Vector3.zero;
    Vector3 _targetForce = Vector3.zero;
    // float _forceLerpSpeed = 1f;

    float _animAirLerpX;
    float _animAirLerpZ;

    float _footStepTimer;
    float _footStepCool = 0.6f;

    public bool IsGameOver => _me.IsGameOver;
    public bool IsMove => _move;
    public float HorizontalAxis => _animVec.x;
    public float VerticalAxis => _animVec.z;
    public bool IsSprint => _sprint && _move /*&& Grounded*/;
    public bool Crouch
    {
        get { return _crouch; }    // 얘랑 Grounded랑 Is가 앖에 안붙은 이유 : 초반엔 아무렇게 했고 파라미터 전달 값 위주라
        set
        {
            if (value)
            {       
                _charCtrl.height = _crouchHeight;
                _charCtrl.center = Vector3.up * _crouchHeight / 2;
            }
            else
            {
                if (!TryStand())
                {
                    _me.UI.PoseCanceled();
                    return;
                }
                _charCtrl.height = _standHeight;
                _charCtrl.center = Vector3.up * _standHeight / 2;
            }
            _me.UI.SetPose(!value);
            _animCtrl.SetBool(AnimBool.IsCrouch, _crouch = value);
        }
    }
    public float Speed => _finalSpeed;
    public float MaxSpeed => _sprintSpeed;
    public bool Grounded => _grounded;

    void Initialize()
    {
        _me = GetComponent<PlayerController>();
        _weaponCtrl = GetComponent<PlayerWeaponController>();
        _animCtrl = _me.AnimCtrl;

        _charCtrl = GetComponent<CharacterController>();
        _standHeight = _charCtrl.height;
        _crouchHeight = _standHeight * 2f / 3f;

        _input = _me.Inputter;
        if (!_me.IsMe) return;
        _input.Actions.Move.started += input => { if (_me.IsMe) _inputVec = input.ReadValue<Vector3>(); };
        _input.Actions.Move.performed += input =>
        {
            if (!_me.IsMe) return;
            _inputVec = input.ReadValue<Vector3>();
        };
        _input.Actions.Move.canceled += input => { if (!_me.IsMe) return; _inputVec = Vector3.zero; _footStepTimer = 0f; };
        _input.Actions.Sprint.started += input =>
        {
            if (!_me.IsMe) return;
            if (Crouch) Crouch = false;
            if (!Crouch) _sprint = true;
        };
        _input.Actions.Sprint.canceled += input =>
        {
            if (!_me.IsMe) return;
            // if (Crouch) Crouch = false;
            _sprint = false;
        };
        _input.Actions.Crouch.started += input => { if (_me.IsMe && !IsSprint && Grounded) Crouch = !Crouch; };
        _input.Actions.Jump.started += input =>
        {
            if (!_me.IsMe) return;
            if (Crouch) Crouch = false;
            Jump();
        };
        /* _input.RegistAction(PlayerInputType.Move, InputTiming.Started
            , input => { if (_me.IsMe) _inputVec = input.ReadValue<Vector3>(); });
        _input.RegistAction(PlayerInputType.Move, InputTiming.Performed
            , input => { if (_me.IsMe) _inputVec = input.ReadValue<Vector3>(); });
        _input.RegistAction(PlayerInputType.Move, InputTiming.Canceled
            , input => { if (_me.IsMe) _inputVec = Vector3.zero; _footStepTimer = 0f; });
        _input.RegistAction(PlayerInputType.Sprint, InputTiming.Started
            , input => {
                if (!_me.IsMe) return;
                if (Crouch) Crouch = false;
                _sprint = true;
            });
        _input.RegistAction(PlayerInputType.Sprint, InputTiming.Canceled
            , input => {
                if (!_me.IsMe) return;
                _sprint = false;
            });
        _input.RegistAction(PlayerInputType.Crouch, InputTiming.Started
           , input => { if (_me.IsMe && !IsSprint && Grounded) Crouch = !Crouch; });
        _input.RegistAction(PlayerInputType.Jump, InputTiming.Started
           , input => {
               if (!_me.IsMe) return;
               if (Crouch) Crouch = false;
               Jump();
           });*/
    }
    public void AddForce(Vector3 force)
    {
        _targetForce += force;
    }
    void Move()
    {
        float deltaTime = Time.deltaTime;
        // 애니메이션 파라미터용
        _animVec = Vector3.Lerp(_animVec, _inputVec, _speedLerp * deltaTime);
        // 편하게 지역변수에 입력값 할당
        float horizontal = _inputVec.x;
        float vertical = _inputVec.z;
        _move = horizontal != 0 || vertical != 0;
        float targetSpeed = 0f;
        if (_move)  // 움직이는 중이면 타겟 스피드 조정
            targetSpeed = IsSprint ? _sprintSpeed : _moveSpeed;
        targetSpeed *= _weaponCtrl.CW == null ? 1.05f : 0.88f;
        // 최종 속도가 타겟 속도보다 특정 이상 차이나면 보정
        if (_finalSpeed < targetSpeed - _speedOffset
            || _finalSpeed > targetSpeed + _speedOffset)
        {
            _finalSpeed = Mathf.Lerp(_finalSpeed, targetSpeed, _speedLerp * deltaTime);
            _finalSpeed = Mathf.Round(_finalSpeed * 1000) / 1000;   // 깔끔하게 소수 3째 반올림
        }
        else _finalSpeed = targetSpeed; // 무의미한 차이면 바로 설정
        _finalSpeed = Mathf.Lerp(_finalSpeed,   // 지면에 접지 여부에 따라 속도를 부드럽게 조정
            Grounded ? targetSpeed : targetSpeed * 0.6f, _speedLerp * deltaTime);
        // 플레이어의 앞과 오른쪽에 입력값 적용된 방향
        Vector3 targetDir = transform.forward * vertical + transform.right * horizontal;
        Vector3 moveVec = (targetDir.normalized * _finalSpeed   // 입력방향 + 수직 속도 + 힘을 더해서 적용
                        + Vector3.up * _verticalVelocity + _currentForce) * deltaTime;
        _charCtrl.Move(moveVec);

        if(IsMove && Grounded)
        {
            _footStepTimer += Time.deltaTime;
            if (_footStepTimer >= _footStepCool / (IsSprint ? 2f : 1f))
            {
                _animCtrl.AnimEvent_FootStep();
                _footStepTimer = 0f;
            }
        }
        else  _footStepTimer = 0f;
    }
    void CheckGround()
    {
        _grounded = Physics.CheckSphere(transform.position + _groundCheckOffset, _groundCheckRadius, _groundLayer);
        _animCtrl.IsGrounded = _grounded;
    }
    void Jump()
    {
        if (_grounded)
        {
            _animCtrl.SetTrigger(AnimTrigger.Jump);
            _verticalVelocity = Mathf.Sqrt(2f * _jumpHeight * -_gravity);
            _grounded = false;
            _animCtrl.IsGrounded = _grounded;

            _animAirLerpX = HorizontalAxis;
            _animAirLerpZ = VerticalAxis;
        }
    }
    void Gravity()
    {
        float deltaTime = Time.deltaTime;
        if (_grounded)
        {
            if (_verticalVelocity >= 0)
                _verticalVelocity -= deltaTime;// * (_verticalVelocity > 0f ? 1f : 10f);
            else if (_verticalVelocity < -1)
                _verticalVelocity += deltaTime * (-1 - _verticalVelocity) * 10;
        }
        else if (_verticalVelocity < _terminalVelocity)
            _verticalVelocity = _terminalVelocity;
        else
            _verticalVelocity += _gravity * deltaTime;
    }
    void SetLocomotion()
    {
        if(IsGameOver)
        {
            _animCtrl.SetFloat(AnimFloat.HorizontalMove, 0f);
            _animCtrl.SetFloat(AnimFloat.VerticalMove, 0f);
            _animCtrl.SetFloat(AnimFloat.Speed, 0f); return;
        }
        if (!_me.MoveCtrl.Grounded)
        {
            _animAirLerpX = Mathf.Lerp(HorizontalAxis, 0f, 30f * Time.deltaTime);
            _animAirLerpZ = Mathf.Lerp(VerticalAxis, 0f, 30f * Time.deltaTime);
            _animCtrl.SetFloat(AnimFloat.HorizontalMove, _animAirLerpX);
            _animCtrl.SetFloat(AnimFloat.VerticalMove, _animAirLerpZ);
        }
        else
        {
            _animCtrl.SetFloat(AnimFloat.HorizontalMove, HorizontalAxis);
            _animCtrl.SetFloat(AnimFloat.VerticalMove, VerticalAxis);
        }

        _animCtrl.SetFloat(AnimFloat.Speed, Speed / MaxSpeed);       
    }
    bool TryStand()
    {
        Ray ray = new Ray(transform.position + Vector3.up * _crouchHeight, Vector3.up);
        float length = _standHeight - _crouchHeight;
        Debug.DrawRay(ray.origin, Vector3.up * length, Color.cyan, 4f);
        if (Physics.Raycast(ray, out RaycastHit hit,length, 1 << LayerMask.NameToLayer("Map")))
            return false;
        return true;
    }

    private void OnDisable()
    {
        if (_me.IsMe) _currentForce = _targetForce = Vector3.zero;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + _groundCheckOffset, _groundCheckRadius);
    }
    private void Start()
    {
        Initialize();
    }
    private void Update()
    {
        if (!_me.IsMe || IsGameOver) return;
        Move();
        CheckGround();
        Gravity();
        _currentForce = Vector3.Lerp(_currentForce, _targetForce, 10 * Time.deltaTime);
        _targetForce = Vector3.Lerp(_targetForce, Vector3.zero, (Grounded ? 10f : 1f) * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        if (!_me.IsMe) return;
        SetLocomotion();
    }
}
