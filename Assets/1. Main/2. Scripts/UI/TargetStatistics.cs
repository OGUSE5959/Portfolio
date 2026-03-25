using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TargetStatistics : MonoBehaviour
{
    // int _fireCount;
    [SerializeField] ShootingTarget _target;
    Vector3 _targetStartPos;
    [SerializeField] bool _isGoing;
    [SerializeField] bool _isSideMoving = false;
    Coroutine _coroutine_SideMove;
    Coroutine _coroutine_ResetX;
    [Space]
    [SerializeField] Canvas _statView;
    [SerializeField] Text _accurate;    
    int _hitCount;
    float _accuracy;
    /*[SerializeField] Image[] _distBtnImgs;
    [SerializeField] Image _moveBtmImg;*/
    [Space]
    [SerializeField] Transform _distMarker;
    [SerializeField] HitButton[] _distBtns;
    [SerializeField] GameObject _moveToggle;
    [SerializeField] HitButton _moveBtn;
    public void AddHitAvg(float dist)
    {
        float acc = 0f;
        if (_hitCount >= 1)
            acc = _accuracy * _hitCount;
        acc += dist;
        _hitCount++;
        _accuracy = acc / _hitCount;
        SetAccurate(100f - _accuracy * 400f);
    }
    public void ResetStats()
    {
        _hitCount = 0;
        _accuracy = 0f;
        _target.ResetHitSpots();
        SetAccurate("-");
    }
    public bool SetDistance(float distFromStart)
    {
        if (_isGoing) return false;
        if (Vector3.Distance(_target.transform.position, _targetStartPos) == distFromStart) return false;
        float dist = Mathf.Abs(distFromStart - _target.transform.position.z - _targetStartPos.z);
        float speed = 25f;
        float time = dist / speed;
        // Debug.Log(time);
        SetTargetGo(time);
        _target.transform.DOMoveZ(_targetStartPos.z + distFromStart, time);
        return true;
    }
    void SetTargetGo(float time)
    {
        _isGoing = true;
        Invoke("SetTargetStop", time);
        foreach (HitButton btn in _distBtns)
            btn.SetColor(Color.gray);
    }
    void SetTargetStop()
    {
        _isGoing = false;
        foreach (HitButton btn in _distBtns)
           if(btn.Color != Color.red) btn.SetColor(Color.white);
    }
    public void SetSideMove()
    {        
        bool value = _isSideMoving = !_isSideMoving;
        if (value)
        {
            if (_coroutine_ResetX != null) StopCoroutine(_coroutine_ResetX);
            _coroutine_ResetX = null;
            _coroutine_SideMove = StartCoroutine(Coroutine_SideMove());
        }
        else
        {
            if(_coroutine_SideMove != null) StopCoroutine(_coroutine_SideMove);
            _coroutine_SideMove = null;
            _coroutine_ResetX = StartCoroutine(Coroutine_ResetX());
        }
        _moveToggle.SetActive(value);
    }

    public void SetAccurate(float accurate)
        => _accurate.text = accurate.ToString("0.0") + "%";
    public void SetAccurate(string txt) => _accurate.text = txt;

    IEnumerator Coroutine_SideMove()
    {
        _target.transform.DOKill();
        while (true)
        {
            _target.transform.DOMoveX(_targetStartPos.x + 4f, 3f);
            yield return Utility.GetWaitForSeconds(3f);
            _target.transform.DOMoveX(_targetStartPos.x, 3f);
            yield return Utility.GetWaitForSeconds(3f);
        }
    }
    IEnumerator Coroutine_ResetX()
    {
        _target.transform.DOKill();
        _target.transform.DOMoveX(_targetStartPos.x, 1f);
        yield return Utility.GetWaitForSeconds(1f);
        _coroutine_ResetX = null;
    }

    // Start is called before the first frame update
    private void Start()
    {
        _target.Initialize(this);
        _targetStartPos = _target.transform.position;
        // StartCoroutine(Coroutine_SideMove());

        for(int i = 1; i <= 4; i++)
        {
            float dist = 0f;
            if (i == 1) dist = 1f;
            else dist = 5 * i;
            HitButton btn = _distBtns[i - 1];
            btn.OnClick.AddListener(() =>
            {
                if (!SetDistance(dist)) return;
                btn.SetColor(Color.red);
                /*if (!_distMarker.gameObject.activeSelf)
                    _distMarker.gameObject.SetActive(true);
                _distMarker.SetParent(btn.transform);
                _distMarker.localPosition = Vector3.zero;*/
                
            });
        }
        _moveBtn.OnClick.AddListener(() => SetSideMove());
    }
    /*private void Update()
    {
        
    }*/
}
