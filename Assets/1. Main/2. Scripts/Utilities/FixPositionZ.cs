using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixPositionZ : MonoBehaviour
{
    float _z;
    void Awake()
    {
        _z = transform.position.z;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 newPos = transform.position;
        newPos.z = _z;
        transform.position = newPos;
    }
}
