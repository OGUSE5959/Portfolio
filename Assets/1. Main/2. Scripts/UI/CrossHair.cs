// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossHair : SingletonMonoBehaviour<CrossHair>
{
    [SerializeField] Transform _imgTr;
    List<Image> _crossHair = new List<Image>();
    [SerializeField] Image _dot;
    [SerializeField] Image[] _verties;  // 위, 아래 순
    [SerializeField] Image[] _horizies; // 좌, 우 순서
    int i;
    float _currAlpha = 1f;

    public List<Image> Hair => _crossHair;

    public void ResetAll()
    {
        SetBothDist(60f);
        SetAlpha(1f);
        SetPosition(Utility.ScreenCenter);
        SetScale(Vector3.one);
        SetColor(Color.white);
    }
    public void SetBothDist(float dist)
    {
        SetVerticalDist(dist);
        SetHorizontalDist(dist);
    }
    public void SetVerticalDist(float dist)
    {
        _verties[0].transform.position = new Vector3(0f, dist / 2f, 0f);
        _verties[1].transform.position = new Vector3(0f, dist / -2f, 0f);
    }
    public void SetHorizontalDist(float dist)
    {
        _horizies[0].transform.position = new Vector3(dist / -2f, 0f, 0f);
        _horizies[1].transform.position = new Vector3(dist / 2f, 0f, 0f);
    }
    public void SetAlpha(float alpha)
    {
        foreach (Image item in _crossHair)
        {
            Color newColor = item.color;
            newColor.a = alpha;
            item.color = newColor;
        }
    }
    public void SetPosition(Vector3 screenPos) => _imgTr.position = screenPos;
    public void SetScale(Vector3 scale) => _imgTr.localScale = scale;
    public void SetRGB(Color color) => SetRGB(color.r, color.b, color.g);
    public void SetRGB(float r, float g, float b) => SetColor(new Color(r, g, b, 1f));
    public void SetColor(Color color)
    {
        foreach (Image img in _crossHair)
            img.color = color;
    }
    #region Lerp Functions
    public void ResetLerp(float lerp)
    {
        SetBothDistLerp(60f, lerp);
        SetAlphaLerp(1f, lerp);
        SetPositionLerp(Utility.ScreenCenter, lerp);
        SetScaleLerp(Vector3.one, lerp);
        SetColorLerp(Color.white, lerp);
    }
    public void SetBothDistLerp(float dist, float lerp)
    {
        SetVerticalDistLerp(dist, lerp);
        SetHorizontalDistLerp(dist, lerp);
    }
    public void SetVerticalDistLerp(float dist, float lerp)
    {
        Vector3 vec0 = _verties[0].transform.localPosition;
        Vector3 vec1 = _verties[1].transform.localPosition;

        float y0 = Mathf.Lerp(vec0.y, dist / -2f, lerp);
        float y1 = Mathf.Lerp(vec1.y, dist / 2f, lerp);

        vec0.y = y0;
        vec1.y = y1;

        _verties[0].transform.localPosition = vec0;
        _verties[1].transform.localPosition = vec1;
    }
    public void SetHorizontalDistLerp(float dist, float lerp)
    {
        Vector3 vec0 = _horizies[0].transform.localPosition;
        Vector3 vec1 = _horizies[1].transform.localPosition;

        float x0 = Mathf.Lerp(vec0.x, dist / 2f, lerp);
        float x1 = Mathf.Lerp(vec1.x, dist / -2f, lerp);

        vec0.x = x0;
        vec1.x = x1;

        _horizies[0].transform.localPosition = vec0;
        _horizies[1].transform.localPosition = vec1;
    }
    public void SetAlphaLerp(float alpha, float lerp)
    {
        _currAlpha = Mathf.Lerp(_currAlpha, alpha, lerp);
        foreach (var image in _crossHair)
        {
            Color col = image.color;
            col.a = _currAlpha; // Mathf.Lerp(col.a, alpha, lerp);
            image.color = col;
            // Debug.Log(alpha);
        }
    }
    public void SetPositionLerp(Vector3 pos, float lerp)
    {
        Vector3 newPos = Vector3.Lerp(transform.position, pos, lerp);
        _imgTr.position = newPos;
    }
    public void SetScaleLerp(Vector3 scale, float lerp)
    {
        Vector3 newScale = Vector3.Lerp(transform.localScale, scale, lerp);
        _imgTr.localScale = newScale;
    }
    public void SetColorLerp(Color color, float lerp)
    {
        Color sample = _crossHair[0].color;
        Color newColor = Color.Lerp(sample, color, lerp);
        SetColor(newColor);
    }
    #endregion

    protected override void OnAwake()
    {
        base.OnAwake();

        _crossHair.Add(_dot);
        foreach (Image img in _verties)
            _crossHair.Add(img);
        foreach (Image img in _horizies)
            _crossHair.Add(img);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    /*void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnAttack();
    }*/
}
