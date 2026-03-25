using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableRanger : MonoBehaviour
{
    [SerializeField] IThrowable _master;
    SphereCollider _collider;
    Rigidbody _rigid;
    bool CanSync => _master != null && _master.IsMe;

    public void Initialize(IThrowable master, float radius)
    {
        _master = master;
        _collider = GetComponent<SphereCollider>();
        _rigid = GetComponent<Rigidbody>();

        _collider.excludeLayers = _rigid.excludeLayers
            = LayerMask.NameToLayer("Map");
        _collider.radius = radius / 2f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CanSync) return;
        if(other.TryGetComponent<HitParts>(out HitParts parts))
            _master.OnDetectParts(parts);
    }
    private void OnTriggerExit(Collider other)
    {
        if (!CanSync) return;
        if (other.TryGetComponent<HitParts>(out HitParts parts))
            _master.OnReleaseParts(parts);
    }
}
