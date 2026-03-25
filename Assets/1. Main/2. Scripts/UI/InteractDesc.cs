using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractDesc : MonoBehaviour
{
    [SerializeField] Text _interactKey;
    [SerializeField] Text _interactUsage;

    public void TurnOn(string key, string usage)
    {
        _interactKey.text = key;
        _interactUsage.text = usage;
        gameObject.SetActive(true);
    }
    public void TurnOff()
    {
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
