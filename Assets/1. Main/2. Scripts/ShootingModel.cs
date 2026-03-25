using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShootingModel : MonoBehaviour, IDamagable
{
    ShootingModelAnimCtrl _animCtrl;
    [SerializeField] Transform _hipTr;
    [Space]
    [SerializeField] GameObject _uiWnd;
    [SerializeField] Slider _timerSlider;
    [SerializeField] Slider _damageSlider;
    [SerializeField] Text _deathTxt;
    [SerializeField] Transform _deathMark;
    float _timeLimit = 6f;
    Coroutine _coroutine_SetTImer;
    bool _isDie = false;
    float _hp, _hpMax = 100f;
    int _deathCombo = 0;
    // float _damageCombo = 0f;

    public ShootingModelAnimCtrl.Motion GetMotion => _animCtrl.GetMotion;

    void AnimEvent_OnEndPosing() => ControlDummy.Instance.EndPosing();

    bool IDamagable.IsDie => _isDie;
    PhotonView IDamagable.PV => null;
    float IDamagable.Health => _hp;
    void IDamagable.SetHit(IAttackable attacker, float damage) { OnDamaged(damage); }
    void IDamagable.SetHit(IAttackable attacker, float damage, string message)
    {
        OnDamaged(damage);
    }
    void IDamagable.SetHit(IAttackable attacker, float damage, Vector3 hitSpot) { }

    void OnDamaged(float damage)
    {        
        if((_hp -= damage) <= 0)
        {
            // _isDie = true;
            // StartCoroutine(Coroutine_SetRebirth());
            PlayerUI.Instance.OnKill();
            _hp += _hpMax;
            _deathTxt.text = "x " + ++_deathCombo + " Kill !!";
            OnDeath();
        }
        _damageSlider.value = 1 - _hp / _hpMax;
        if (_coroutine_SetTImer != null) StopCoroutine(_coroutine_SetTImer);
        _coroutine_SetTImer = StartCoroutine(Coroutine_SetTimer());
        if(!_uiWnd.activeSelf) _uiWnd.SetActive(true);
    }
    void OnDeath()
    {
        _deathTxt.transform.DOKill();
        _deathTxt.transform.localScale = Vector3.one * 1.2f;
        _deathTxt.transform.DOScale(Vector3.one, 0.3f);
        _deathMark.transform.DOKill();
        _deathMark.localScale = Vector3.one;
        _deathMark.DOScale(Vector3.zero, 0.5f);
    }
    void ResetCombo()
    {
        _damageSlider.value = 0f;
        _deathTxt.text = string.Empty;
        _deathCombo = 0;
        _uiWnd.SetActive(false);
    }

    public void SetAnim(ShootingModelAnimCtrl.Motion motion, bool isBlend) => _animCtrl.Play(motion, isBlend);

    IEnumerator Coroutine_SetTimer()
    {
        float timer = 0f;
        _timerSlider.value = 1f;
        while(true)
        {
            timer += Time.deltaTime;
            if(timer >= _timeLimit)
            {
                _coroutine_SetTImer = null;
                ResetCombo();
                yield break;
            }
            _timerSlider.value = 1 - timer / _timeLimit;
            yield return null;
        }
    }
    IEnumerator Coroutine_SetRebirth()
    {
        yield return null;
        _isDie = false;
    }
    private void Awake()
    {
        _animCtrl = GetComponent<ShootingModelAnimCtrl>();
        Collider[] cols = _hipTr.GetComponentsInChildren<Collider>();
        foreach (Collider col in cols)
        {
            col.gameObject.layer = LayerMask.NameToLayer("OtherPlayer");
            HitParts parts = col.gameObject.AddComponent<HitParts>();
            parts.Initialize(this);
        }
        _hp = _hpMax;
    }    
    // void Update() { }
}
