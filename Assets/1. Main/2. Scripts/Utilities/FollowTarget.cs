using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] Transform _followTarget;
    [Space]
    [SerializeField] bool _x;
    [SerializeField] bool _y;
    [SerializeField] bool _z;

    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/

    // Update is called once per frame
    void Update()
    {
        Vector3 myPos = transform.position;
        Vector3 targetPos = _followTarget.position;
        Vector3 newPos = new Vector3(_x ? targetPos.x : myPos.x, _y ? targetPos.y : myPos.y, _z ? targetPos.z : myPos.z);
        transform.position = newPos;
    }
}
