using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using Photon.Pun;

public enum AnimTrigger
{
    Jump,
    Punch,
    Victory,
    Throw,
    Mow,
    Mow_1,

    Max
}
public enum AnimFloat
{
    Speed,
    HorizontalMove,
    VerticalMove,
    HorizontalAngle,
    VerticalAngle,
    TurnOffset,

    Max
}
public enum AnimBool
{
    IsRun,
    IsCrouch,
    IsGrounded,
    IsWeaponForm,

    Max
}

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimationController : MonoBehaviour
{
    #region Player Components
    PlayerController _me;
    PlayerInputController _input;
    PlayerMoveController _move;
    PlayerCameraController _camCtrl;
    PlayerWeaponController _weaponCtrl;
    #endregion

    IKControl _ikControl;
    public IKControl IKControl => _ikControl;

    [SerializeField] Animator _animator;
    Dictionary<AnimTrigger, int> _triggerHashTable = new Dictionary<AnimTrigger, int>();    // 트리거형 파라미터 해쉬
    Dictionary<AnimFloat, int> _floatHashTable = new Dictionary<AnimFloat, int>();          // 실수형 파라미터 해쉬
    Dictionary<AnimBool, int> _boolHashTable = new Dictionary<AnimBool, int>();             // 불형 파라미터 해쉬

    StringBuilder _sb = new StringBuilder();
    AudioSource _speaker;                       // PlayerCtrl 클래스 중 아무나 가지고 있어도 된다
    [Space]
    [SerializeField] float _baseVolume = 0.8f;  // 발소리가 생각보다 커서 넣은 값
    [SerializeField] AudioClip[] _stepClips;
    [SerializeField] AudioClip _landClip;

    #region AnimBool Parameter
    bool _isRun;
    bool _grounded;
    bool _weaponForm;

    // 많이 쓸 수도 있는 불값(입력과는 또 다른 값)
    public bool IsRun { get { return _isRun; } set { SetBool(AnimBool.IsRun, _isRun = value); } }   // 뛰고 있는가?
    public bool IsGrounded { get { return _grounded; } set { SetBool(AnimBool.IsGrounded, _grounded = value); } }   // 지면인가?
    public bool IsWeaponForm { get { return _weaponForm; } set { SetBool(AnimBool.IsWeaponForm, _weaponForm = value); } }   // 무기 자세인가?
    #endregion

    public void AnimEvent_FootStep()
    {
        if (_me.IsMe)
            _me.PV.RPC("RPC_FootStep", RpcTarget.All, Random.Range(0, _stepClips.Length));
    }
    public void AnimEvent_Land()
    {
        if (_me.IsMe)
            _me.PV.RPC("RPC_Land", RpcTarget.All);
    }

    [PunRPC] void RPC_FootStep(int index)
    {
        _speaker.volume = _baseVolume / (_me.MoveCtrl.Crouch ? 6f : 1f);
        _speaker.PlayOneShot(_stepClips[index]);
    }
    [PunRPC] void RPC_Land() => _speaker.PlayOneShot(_landClip);

    void Initialize()
    {
        #region Components Allocation
        _me = GetComponent<PlayerController>();
        if (!TryGetComponent<AudioSource>(out _speaker))
            _speaker = gameObject.AddComponent<AudioSource>();
        _speaker.volume = 0.4f;
        _speaker.spatialBlend = 1f;
        _speaker.outputAudioMixerGroup = AudioManager.Instance.Group_SFX;

        _input = _me.Inputter;
        _move = _me.MoveCtrl;
        _camCtrl = _me.CamCtrl;
        _weaponCtrl = _me.WeaponCtrl;

        _ikControl = GetComponent<IKControl>();
        _animator = GetComponent<Animator>();
        #endregion

        // enum의 문자 그대로 해쉬 저장 => 파라미터의 이름과 같아야 한다
        for (int i = 0; i < (int)AnimTrigger.Max; i++) 
        {
            AnimTrigger type = (AnimTrigger)i;
            _sb.Clear();
            _sb.Append(type);
            int hash = Animator.StringToHash(_sb.ToString());
            _triggerHashTable.Add(type, hash);
        }
        for (int i = 0; i <(int)AnimFloat.Max; i++)
        {
            AnimFloat type = (AnimFloat)i;
            _sb.Clear();
            _sb.Append(type);
            int hash = Animator.StringToHash(_sb.ToString());
            _floatHashTable.Add(type, hash);
        }
        for (int i = 0; i < (int)AnimBool.Max; i++)
        {
            AnimBool type = (AnimBool)i;
            _sb.Clear();
            _sb.Append(type);
            int hash = Animator.StringToHash(_sb.ToString());
            _boolHashTable.Add(type, hash);
        }
    }
    public void SetTrigger(AnimTrigger type)
    {
        if (!_me.IsMe) return;
        int hash = _triggerHashTable[type];
        _animator.SetTrigger(hash);
    }
    public void SetFloat(AnimFloat type, float value)
    {
        if (!_me.IsMe) return;
        int hash = _floatHashTable[type];
        _animator.SetFloat(hash, value);
    }
    public void SetBool(AnimBool type, bool value)
    {
        if (!_me.IsMe) return;
        int hash = _boolHashTable[type];
        _animator.SetBool(hash, value);
    }    
    void Awake()
    {
        Initialize();
    }
}
