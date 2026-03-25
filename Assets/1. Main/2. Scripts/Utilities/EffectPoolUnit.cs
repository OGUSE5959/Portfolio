using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPoolUnit : MonoBehaviour
{
    [SerializeField] string _effectName;
    public string EffectName { get { return _effectName; } }    
    [SerializeField]
    float _delay = 0.5f;
    float _inactiveTime;
    FXAutoFalse _autoFalse;
    public FXAutoFalse AutoFalse { get { if (!_autoFalse) _autoFalse =GetComponent<FXAutoFalse>(); return _autoFalse; } }
    public bool IsReady
    {
        get
        {
            if (!gameObject.activeSelf)
            {
                if (Time.time > _inactiveTime + _delay)
                    return true;
            }
            return false;
        }
    }
    public void SetEffectPool(string effectName)
    {
        _effectName = effectName;
        transform.SetParent(EffectPool.Instance.transform);
        transform.localPosition = Vector3.zero;
        // transform.localScale = Vector3.one;
    }
    void OnDisable()
    {
        _inactiveTime = Time.time;
        EffectPool.Instance.InsertEffect(_effectName, this);
    }
    // Start is called before the first frame update
    void Start()
    {

    }
}
