using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowableIndicator : MonoBehaviour
{
    PlayerUI _master;
    Camera _mainCam;
    [SerializeField] ThrowableType _throwType;
    [SerializeField] Sprite[] _throwIcon;
    [SerializeField] Image[] _imgs = new Image[2];
    [Space]
    [SerializeField] RectTransform _dirCase;
    [SerializeField] Transform _dirImg;
    IThrowable _target;
    float _caseAngle;
    Vector3 posDir;
    PlayerController Center => _master.Master;

    public void Initialize(PlayerUI playerUI)
    {
        _master = playerUI;
        // _center = playerUI.Master;
        _mainCam = Camera.main;
    }
    public void SetUp(ThrowableType type, IThrowable target)
    {
        _throwType = type;
        _target = target;
    }
    public void End()
    {
        gameObject.SetActive(false);
        _master.ThrowIndiPool.Set(this);
        SetColor(Color.white);
    }
    public void SetColor(Color color)
    {
        foreach (Image img in _imgs)
            img.color = color;
    }
    public void SetAlpha(float alpha)
    {
        foreach (Image img in _imgs)
        {
            Color newColor = img.color;
            newColor.a = alpha;
            img.color = newColor;
        }
    }
    public void SetColorLerp(Color color, float lerp)
    {
        foreach (Image img in _imgs)
        {
            Color newColor = img.color;
            newColor = Color.Lerp(newColor, color, lerp);
            img.color = newColor;
        }
    }
    public void SetAlphaLerp(float alpha, float lerp)
    {
        foreach (Image img in _imgs)
        {
            Color newColor = img.color;
            newColor.a = Mathf.Lerp(newColor.a, alpha, lerp);
            img.color = newColor;
        }
    }
    // Start is called before the first frame update
    /*void Awake()
    {
        _imgs[0] = GetComponent<Image>();
        _imgs[1] = _dirImg.GetComponent<Image>();
    }*/

    void RotationByVertical()
    {
        Transform CenTr =  Center.transform;
        Vector3 from =  CenTr.forward;
        Vector3 target = Utility.GetNormalizedDir(_target.transform.position, _master.Master.transform.position);
        from.y = target.y = 0f;
        _caseAngle = Vector3.SignedAngle(target, from, Vector3.up);
        _caseAngle = (_caseAngle < 0) ? _caseAngle + 360f : _caseAngle ;
        _dirCase.rotation = Quaternion.Euler(0, 0, _caseAngle);
    }
    void RotationByScreenPos()
    {
        Transform CenTr = _mainCam.transform;
        Vector3 from = Utility.ScreenCenter;
        Vector3 target = Utility.GetNormalizedDir(
            _mainCam.WorldToScreenPoint(_target.transform.position), from/*_master.Master.transform.position*/);
        from.z = target.z = 0f;
        _caseAngle = Vector3.SignedAngle(target, Utility.ScreenCenter, -Vector3.forward);
        _caseAngle = (_caseAngle < 0) ? _caseAngle + 360f : _caseAngle;
        //_dirCase.rotation = Quaternion.Euler(0, 0, _caseAngle);
        // posDir = Utility.GetNormalizedDir(target, from);
        // Debug.Log(_mainCam.WorldToScreenPoint(_target.transform.position));
    }
    void SubDetails()
    {
        float baseLerp = 10f * Time.deltaTime;
        float dist = Vector3.Distance(Center.transform.position, _target.transform.position);
        if (dist <= _target.EffectRadius)
            if (dist <= _target.DangerRadius) SetColorLerp(Color.red, baseLerp);
            else SetColorLerp(Color.white, baseLerp);
        else SetColorLerp(Color.clear, baseLerp);

        Vector3 dir = Utility.GetNormalizedDir(Utility.ScreenCenter, transform.position);
        float cenAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _dirImg.rotation = Quaternion.Euler(0, 0, cenAngle);
    }
    private void LateUpdate()
    {
        // _dirCase.rotation = Quaternion.Euler(0, 0, tanAngle);
        RotationByVertical();
        // RotationByScreenPos();
        SubDetails();
        _dirCase.rotation = Quaternion.Euler(0, 0, _caseAngle);
    }
    // Update is called once per frame
    /*void Update()
    {
        Transform CenTr = _mainCam.transform;// Center.transform;
        Vector3 from = Utility.ScreenCenter; //_mainCam.WorldToScreenPoint(_master.Master.transform.position);// CenTr.forward;
        Vector3 target = Utility.GetNormalizedDir(
            _mainCam.WorldToScreenPoint(_target.transform.position), from*//*_master.Master.transform.position*//*);
        from.z = target.z = 0f;
        posDir = Utility.GetNormalizedDir(target, from);
        tanAngle = Vector3.SignedAngle(target, Utility.ScreenCenter, -Vector3.forward); 
        tanAngle = (tanAngle < 0) ? tanAngle + 360f : tanAngle;
        _dirCase.rotation = Quaternion.Euler(0, 0, tanAngle);
        Debug.Log(_mainCam.WorldToScreenPoint(_target.transform.position));

        float baseLerp = 10f * Time.deltaTime;
        float dist = Vector3.Distance(CenTr.position, _target.transform.position);
        if (dist <= _target.EffectRadius)
            if (dist <= _target.DangerRadius) SetColorLerp(Color.red, baseLerp);
            else SetColorLerp(Color.white, baseLerp);
        else SetColorLerp(Color.clear, baseLerp);

        Vector3 dir = Utility.GetNormalizedDir(Utility.ScreenCenter, transform.position);
        float cenAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _dirImg.rotation = Quaternion.Euler(0, 0, cenAngle);

        *//*float dot = Vector3.Dot(from, target) * (2f / Mathf.PI);
        bool isRight = Vector3.Cross(from, target).y >= 0f;
        float angle = isRight ? dot : -(Vector3.Dot(target, from));
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        *//*transform.position = Utility.ScreenCenter
            + offset * 100f * (isRight ? 1f : -1f);*//* // 외적으로 왼쪽인걸 구하면 내적의 값을 반대로 뒤집고 offset * -1*//*
    }*/
}
