using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoadingMenu : Menu
{
    [SerializeField] Text _loadingLabel;
    Coroutine _coroutine_Label;

    public override void Initialize()
    {
        base.Initialize();
        _menuType = MenuType.Loading;
    }

    IEnumerator Coroutine_Label()
    {
        string str = "·Îµùñé";
        while (true)
        {
            _loadingLabel.text = str;
            yield return Utility.GetWaitForSeconds(0.5f);
            _loadingLabel.text +=  ".";
            yield return Utility.GetWaitForSeconds(0.5f);
            _loadingLabel.text += ".";
            yield return Utility.GetWaitForSeconds(0.5f);
            _loadingLabel.text += ".";
            yield return Utility.GetWaitForSeconds(0.5f);
        }
    }

    protected new void OnEnable()
    {
        _coroutine_Label = StartCoroutine(Coroutine_Label());
    }
    protected new void OnDisable()
    {
        StopAllCoroutines();
        _coroutine_Label = null;
    }
    /*void Start()
    {
        
    }*/
}
