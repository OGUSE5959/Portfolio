using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AttackMarker : MonoBehaviour
{
    [SerializeField] Transform _attack;
    [Space]
    [SerializeField] Transform _killTr;
    [SerializeField] Image[] _killImg;
    Color[] _killStartColor;
    Coroutine _coroutine_Attack;
    Coroutine _coroutine_Kill;

    void SetKillColor(Color color)
    {
        foreach(Image img in _killImg)
            img.color = color;
    }
    void ResetKillColor()
    {
        for (int i = 0; i < _killStartColor.Length; i++)
            _killImg[i].color = _killStartColor[i];
    }
    public void SetKillAlpha(float  alpha)
    {
        foreach(Image img in _killImg)
        {
            Color newColor = img.color;
            newColor.a = alpha;
            img.color = newColor;
        }
    }
    public void OnAttack()
    {
        if (_coroutine_Attack != null)
            StopCoroutine(_coroutine_Attack);
        _coroutine_Attack = StartCoroutine(Coroutine_Attack());
    }
    public void OnKill()
    {
        if (_coroutine_Kill != null)
            StopCoroutine(_coroutine_Kill);
        _coroutine_Kill = StartCoroutine(Coroutine_Kill());
    }

    IEnumerator Coroutine_Attack()
    {
        _attack.DOKill();
        _attack.localScale = Vector3.zero;
        _attack.gameObject.SetActive(true);
        _attack.DOScale(Vector3.one, 0.1f);
        yield return Utility.GetWaitForSeconds(0.1f);
        _attack.DOScale(Vector3.zero, 0.25f);
        yield return Utility.GetWaitForSeconds(0.25f);
        _attack.gameObject.SetActive(false);
        yield break;
    }
    IEnumerator Coroutine_Kill()
    {
        ResetKillColor();
        _killTr.DOKill();
        _killTr.localScale = Vector3.zero;
        _killTr.gameObject.SetActive(true);

        _killTr.DOScale(Vector3.one * 1.5f, 0.1f);
        float duration = 0.5f;
        float timer = 0f;
        while(true)
        {
            timer += Time.deltaTime;
            float alpha = 1f - timer / (duration * 2);
            SetKillAlpha(alpha);
            if(timer > duration)
            {
                SetKillAlpha(0.5f);
                break;
            }
            yield return null;
        }
        _killTr.DOScale(Vector3.zero, 0.2f);
        yield return Utility.GetWaitForSeconds(0.2f);
        _attack.gameObject.SetActive(false);
        // yield break;
    }
    // Start is called before the first frame update
    void Start()
    {
        _killImg = _killTr.GetComponentsInChildren<Image>(true);
        _killStartColor = new Color[_killImg.Length];
        for(int i = 0; i < _killImg.Length; i++)
            _killStartColor[i] = _killImg[i].color;
    }
}
