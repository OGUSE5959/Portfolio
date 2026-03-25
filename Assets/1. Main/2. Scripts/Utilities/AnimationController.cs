using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    [SerializeField] protected Animator _animator;
    public Animator Animator { get { return _animator; } }
    int _prehash = -1;

    public virtual void Initialize() => _animator = GetComponent<Animator>();
    public void SetTrigger(string name)
    {
        _animator.SetTrigger(name);
    }
    public void SetTrigger(int hash)
    {
        _animator.SetTrigger(hash);
    }
    public bool IsState(int hash) => _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == hash;
    public bool GetBool(string name)
    {
        return _animator.GetBool(name);
    }
    public bool GetBool(int hash)
    {
        return _animator.GetBool(hash);
    }
    public void SetBool(string name, bool value)
    {
        _animator.SetBool(name, value);
    }
    public void SetBool(int hash, bool value)
    {
        _animator.SetBool(hash, value);
    }
    public void SetFloat(string name, float value)
    {
        _animator.SetFloat(name, value);
    }
    public void SetFloat(int hash, float value)
    {
        _animator.SetFloat(hash, value);
    }
    public void SetInteger(string name, int value)
    {
        _animator.SetInteger(name, value);
    }
    public void SetInteger(int hash, int value)
    {
        _animator.SetInteger(hash, value);
    }

    public void Play(int hash, bool isBlend)
    {
        if (_prehash != -1)
            _animator.ResetTrigger(hash);
        if(isBlend)
            _animator.SetTrigger(hash);
        else
            _animator.Play(hash);
        _prehash = hash;
    }
    protected virtual void Awake()
    {
        Initialize();
    }
}
