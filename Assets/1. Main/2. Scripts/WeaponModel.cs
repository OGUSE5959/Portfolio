using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponModel : MonoBehaviour, IWatchable
{
    [SerializeField] Vector3 _watchOffset;
    public Vector3 WatchOffset => _watchOffset;
    public Vector3 WatchPoint => transform.position + WatchOffset;
    TitleUI _titleUI;

    void IWatchable.OnWatched()
    {
        _titleUI.WeaponToggle.SetActive(true);
    }
    void IWatchable.OnQuited()
    {
        _titleUI.WeaponToggle.SetActive(false);
    }

    /*private void OnMouseDown()
{
   Debug.Log(0);
}*/

    // Start is called before the first frame update
    void Start()
    {
        _titleUI = TitleUI.Instance;
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
