using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeEx : MonoBehaviour
{
    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {

    }
    private void Update()
    {
        Vector3 targetPos = new Vector3(10f, 0f, 0f);
        transform.position = Vector3.Slerp(transform.position, targetPos, 1);
    }
}
