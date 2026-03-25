using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LerpFill : MonoBehaviour
{
    [SerializeField] Image _me;
    [SerializeField] Image _target;
    [Space]
    [SerializeField] float _lerpSpeed;

    // Start is called before the first frame update
    void Start()
    {
        if (!_me) _me = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_target.fillAmount != _me.fillAmount)
        {
            _me.fillAmount = Mathf.Lerp(_me.fillAmount, _target.fillAmount, _lerpSpeed * Time.deltaTime);
        }
    }
}
