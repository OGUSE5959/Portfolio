using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    [SerializeField] bool _detectContinuous;
    List<IDamagable> _unitList = new List<IDamagable>();
    public List<IDamagable> UnitList => _unitList;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<IDamagable>(out IDamagable id))
        {
            Debug.Log(id.gameObject.name);
            if (!_unitList.Contains(id))
                _unitList.Add(id);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!_detectContinuous) return;
        if (other.TryGetComponent<IDamagable>(out IDamagable id))
        {
            if (!_unitList.Contains(id))
                _unitList.Add(id);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IDamagable>(out IDamagable id))
        {
            if (_unitList.Contains(id))
                _unitList.Remove(id);
        }
    }
}
