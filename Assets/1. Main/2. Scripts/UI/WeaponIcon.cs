using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WeaponIcon : MonoBehaviour
{
    [SerializeField] Image _icon;
    public Sprite Sprite => _icon.sprite;
    [SerializeField] public Text _simpleDesc;
    [Space]
    [SerializeField] GameObject _fireModeObj;
    [SerializeField] Image[] _fireModeIcons;
    [SerializeField] bool _selected =false;

    Coroutine _coroutine_SwapCanceled;

    public bool Selected => _selected;

    public void SetIcon(Sprite icon)
    {
        _icon.sprite = icon;
        SetStow();
    }
    public void SetIconAlpha(float alpha) => _icon.color = new Color(1f, 1f, 1f, alpha);
    public void SetStow(float alpha = 120f / 256f)
    {
        if (!_icon.sprite) { SetIconAlpha(0f); return; }
        _selected = false;
        _icon.color = new Color(1f, 1f, 1f, alpha);
        _icon.rectTransform.DOScale(0.7f, 0.5f);
    }
    public void SetHold(float alpha = 200f / 256f)
    {
        if (!_icon.sprite) { SetIconAlpha(0f); return; }
        _selected = true;
        _icon.color = new Color(1f, 1f, 1f, alpha);
        _icon.rectTransform.DOScale(1f, 0.5f);
    }
    public void SetFireMode(FireMode fireMode, int burstRepeat = 3)
    {
        foreach (Image img in _fireModeIcons)
            if (!img) return;   // 런타임 종료 시 GunArm에서 이미지가 Destroy됐는데도 호출해서 끊기
            else img.gameObject.SetActive(false);
        if (fireMode == FireMode.None)
        {
            _fireModeObj.SetActive(false);
            return;
        }

        if (!_fireModeObj.activeSelf) _fireModeObj.SetActive(true);
        switch (fireMode)
        {
            case FireMode.Single:
            case FireMode.SemiAuto:
                _fireModeIcons[0].gameObject.SetActive(true);
                break;
            case FireMode.Burst:
                for(int i = 0; i < burstRepeat; i++)
                    _fireModeIcons[i].gameObject.SetActive(true);
                break;
            case FireMode.Auto:
                foreach (Image img in _fireModeIcons)
                    img.gameObject.SetActive(true);
                break;
        }
    }
    public void SwapCanceled()
    {
        if (_coroutine_SwapCanceled != null)
            StopCoroutine(_coroutine_SwapCanceled);
        _coroutine_SwapCanceled = StartCoroutine(Coroutine_SwapCanceled());
    }

    IEnumerator Coroutine_SwapCanceled()
    {
        bool isRed = false;
        while(true)
        {
            Color newColor = _icon.color;
            if (!isRed)
            {
                newColor.g -= 10f * Time.deltaTime;
                newColor.b -= 10f * Time.deltaTime;
                _icon.color = newColor;
                if (newColor.g <= 0f)
                {
                    newColor.g = newColor.b = 0f;
                    _icon.color = newColor;
                    isRed = true;
                }
                yield return null;
            }

            newColor.g += 5f * Time.deltaTime;
            newColor.b += 5f * Time.deltaTime;
            _icon.color = newColor;
            if (newColor.b >= 1f)
            {
                newColor.g = newColor.b = 1f;
                _icon.color = newColor;
                yield break;
            }
            yield return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
