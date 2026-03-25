using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ControlDummy : SingletonMonoBehaviour<ControlDummy>
{
    private enum BtnType
    {
        Distance,
        Pose,
        Rotation,

        Max
    }
    private enum DirType
    {
        Front,
        Back,
        Left,
        Right,

        Max
    }

    [SerializeField] Transform _stage;
    Vector3 _stageStartPos;
    [SerializeField] ShootingModel _dummy;
    [Space]
    [SerializeField] HitButton[] _distBtns;
    [SerializeField] HitButton[] _posBtns;
    [SerializeField] HitButton[] _rotBtns;
    [Space]
    [SerializeField] Transform[] _markers = new Transform[(int)BtnType.Max];
    [Space]
    [SerializeField] HitButton _moveBtn;
    [SerializeField] GameObject _moveToggle;

    bool _isGoing;
    bool _isSideMoving = false;
    Coroutine _coroutine_SideMove;
    Coroutine _coroutine_ResetZ;

    bool _isPosing = false;
    bool _isRotating = false;

    void SetBtnsColor(BtnType type, Color color, bool isAbs = false)
    {
        HitButton[] btns = null;
        switch (type)
        {
            case BtnType.Distance: btns = _distBtns; break;
            case BtnType.Pose: btns = _posBtns; break;
            case BtnType.Rotation: btns = _rotBtns; break;
        }
        foreach (HitButton btn in btns)
            if (isAbs) btn.SetColor(color);
            else if (btn.Color != Color.red) btn.SetColor(color);
    }
    void SetMarker(BtnType type, Transform tr)
    {
        Transform marker = _markers[(int)type];
        if (!marker.gameObject.activeSelf) 
            marker.gameObject.SetActive(true);
        marker.SetParent(tr);
        marker.localPosition = Vector3.zero;
    }

    public bool SetDistance(float distFromStart)
    {
        if (_isGoing) return false;
        if (Vector3.Distance(_stage.transform.position, _stageStartPos) == distFromStart) return false;     
        float time = 1f;
        SetTargetGo(time);
        // _stage.transform.DOKill();
        _stage.transform.DOMoveX(_stageStartPos.x + distFromStart, time);
        return true;
    }
    void SetTargetGo(float time)
    {
        _isGoing = true;
        Invoke("SetTargetStop", time);
        SetBtnsColor(BtnType.Distance, Color.gray, true);
    }
    void SetTargetStop()
    {
        _isGoing = false;
        SetBtnsColor(BtnType.Distance, Color.white);
    }

    bool SetDummyPos(ShootingModelAnimCtrl.Motion motion, bool isBlend = true)
    {       
        if (_isPosing) return false;
        _dummy.SetAnim(motion, isBlend);
        StartPosing();
        return true;
    }
    void StartPosing()
    {
        // Debug.Log("StartPosing");
        _isPosing = true;
        SetBtnsColor(BtnType.Pose, Color.gray, true);
    }
    public void EndPosing()
    {
        // Debug.Log("EndPosing");
        _isPosing = false;
        SetBtnsColor(BtnType.Pose, Color.white);
    }

    bool SetRotation(DirType dir)
    {
        if(_isRotating) return false;
        float angle = 0f;
        switch(dir)
        {
            case DirType.Front: break;
            case DirType.Back: angle = 180f; break;
            case DirType.Left: angle = 90f; break;
            case DirType.Right: angle = -90f; break;
        }
        StartRotate(angle);
        return true;
    }
    void StartRotate(float angle, float duration = 0.5f)
    {
        _isRotating = true;
        _stage.DORotate(new Vector3(0f, angle, 0f), duration);
        Invoke("EndRotate", duration);
        SetBtnsColor(BtnType.Rotation, Color.gray, true);
    }
    void EndRotate()
    {
        _isRotating = false;
        SetBtnsColor(BtnType.Rotation, Color.white);
    }

    public void SetSideMove()
    {
        bool value = _isSideMoving = !_isSideMoving;
        if (value)
        {
            if (_coroutine_ResetZ != null) StopCoroutine(_coroutine_ResetZ);
            _coroutine_ResetZ = null;
            _coroutine_SideMove = StartCoroutine(Coroutine_SideMove());
        }
        else
        {
            if (_coroutine_SideMove != null) StopCoroutine(_coroutine_SideMove);
            _coroutine_SideMove = null;
            _coroutine_ResetZ = StartCoroutine(Coroutine_ResetZ());
        }
        _moveToggle.SetActive(value);
    }

    IEnumerator Coroutine_SideMove()
    {
        _stage.transform.DOKill();
        while (true)
        {
            _stage.transform.DOMoveZ(_stageStartPos.z - 6f, 2.5f);
            yield return Utility.GetWaitForSeconds(3f);
            _stage.transform.DOMoveZ(_stageStartPos.z, 2.5f);
            yield return Utility.GetWaitForSeconds(3f);
        }
    }
    IEnumerator Coroutine_ResetZ()
    {
        _stage.transform.DOKill();
        _stage.transform.DOMoveZ(_stageStartPos.z, 1f);
        yield return Utility.GetWaitForSeconds(1f);
        _coroutine_ResetZ = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        _stageStartPos = _stage.position;
        // Set Distance Buttons
        for (int i = 1; i <= _distBtns.Length; i++)
        {
            float dist = 0f;
            if (i == 1) dist = 1f;
            else dist = 5 * i;
            HitButton btn = _distBtns[i - 1];
            btn.OnClick.AddListener(() =>
            {
                if (!SetDistance(dist)) return;
                btn.SetColor(Color.red);
                // SetMarker(BtnType.Distance, btn.transform);
            });
        }
        // Set Pose Buttons
        for(int i = 0; i < _posBtns.Length; i++)
        {
            var motion = (ShootingModelAnimCtrl.Motion)i;
            HitButton btn = _posBtns[i];
            btn.OnClick.AddListener(() =>
            {
                if (!SetDummyPos(motion)) return;
                btn.SetColor(Color.red);
                // SetMarker(BtnType.Pose, btn.transform);
            });
        }
        // Set Rot Buttons
        for(int i = 0; i < _rotBtns.Length; i++)
        {
            DirType dir = (DirType)i;
            HitButton btn = _rotBtns[i];
            btn.OnClick.AddListener(() =>
            {
                if(!SetRotation(dir)) return;
                btn.SetColor(Color.red);
                // SetMarker(BtnType.Rotation, btn.transform);
            });
        }
        _moveBtn.OnClick.AddListener(() => SetSideMove());
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
