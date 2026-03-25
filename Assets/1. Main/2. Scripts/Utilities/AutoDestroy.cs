using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] float _timer;

    IEnumerator Coroutine_Destroy(float time)
    {
        yield return Utility.GetWaitForSeconds(time);
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Coroutine_Destroy(_timer));
    }
}
