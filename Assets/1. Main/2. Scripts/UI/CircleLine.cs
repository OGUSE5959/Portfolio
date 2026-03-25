using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleLine : MonoBehaviour
{
    [SerializeField] int _segments;
    [SerializeField] float _radiusX;
    [SerializeField] float _radiusY;
    LineRenderer _lr;
 
    // Start is called before the first frame update
    void Start()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = _segments + 1;
        _lr.useWorldSpace = false;
        CreatePoints();
    }
    void CreatePoints()
    {
        float x, y, z = 0f;
        float angle = 20f;
        for (int i = 0; i < _segments + 1; i++)
        {
            x = Mathf.Cos(angle * Mathf.Deg2Rad) * _radiusX;
            y = Mathf.Sin(angle * Mathf.Deg2Rad) * _radiusX;

            _lr.SetPosition(i, new Vector3(x, y, z));
            angle = Utility.NormalizeAngle(angle + (360f / _segments));
        }
    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    float x, y, z = 0f;
    //    float angle = 20f;
    //    for (int i = 0; i < _segments + 1; i++)
    //    {
    //        x = Mathf.Cos(angle * Mathf.Deg2Rad) * _radiusX;
    //        y = Mathf.Sin(angle * Mathf.Deg2Rad) * _radiusX;

    //        Gizmos.DrawWireSphere(new Vector3(x, y, z), 1f);
    //        angle = Utility.NormalizeAngle(angle + (360f / _segments));
    //    }
    //}
}
