using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractDetector : MonoBehaviour
{
    IInteractor _master;
    SphereCollider _collider;

    public void SetInteractRange(Vector3 center, float radius)
    {
        if(!_collider) _collider = GetComponent<SphereCollider>();
        _collider.center = center;
        _collider.radius = radius;
    }

    private void Awake()
    {
        _master = GetComponentInParent<IInteractor>();
        if (!_collider) _collider = GetComponent<SphereCollider>();
    }
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_master.PV.IsMine) return;
        if(other.TryGetComponent<IInteractable>(out IInteractable it))
        {
            if (it.InteractType == InteractType.Item)
                _master.OnDetect(it);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!_master.PV.IsMine || !_master.DetectContinuous) return;
        if (other.TryGetComponent<IInteractable>(out IInteractable it))
        {
            if (it.InteractType == InteractType.Item)
                _master.OnDetect(it);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!_master.PV.IsMine) return;
        if (other.TryGetComponent<IInteractable>(out IInteractable it))
        {
            if (it.InteractType == InteractType.Item)
                _master.OnRelease(it);
        }
    }
}
