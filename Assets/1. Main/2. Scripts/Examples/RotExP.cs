using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotExP : MonoBehaviour
{
    [SerializeField] Transform _child;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(50, 50, 50) * Time.deltaTime);
        _child.rotation = Utility.QI;
    }
}
