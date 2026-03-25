using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleModel : MonoBehaviour, IWatchable
{
    TitleModelAnimationController _animCtrl;
    [SerializeField] Transform _watchTr;
    TitleUI _titleUI;
    [SerializeField] WeaponModel[] _weaponModels;

    public Vector3 WatchOffset => _watchTr.position - transform.position;
    public Vector3 WatchPoint => transform.position + WatchOffset;
    public TitleModelAnimationController.Motion GetMotion => _animCtrl.GetMotion;

    void IWatchable.OnQuited()
    {
        _titleUI.MotionToggle.SetActive(false);
    }
    void IWatchable.OnWatched()
    {
        _titleUI.MotionToggle.SetActive(true);
    }

    public void SetWeapon(WeaponID weaponType) => SetWeapon((int)weaponType);
    public void SetWeapon(int index)
    {
        foreach (var weapon in _weaponModels) 
            if(weapon.gameObject.activeSelf)
                weapon.gameObject.SetActive(false);
        _weaponModels[index].gameObject.SetActive(true);
    }
    public void SetMotion(TitleModelAnimationController.Motion motion) => _animCtrl.Play(motion);
    public void SetMotion(int index) => SetMotion((TitleModelAnimationController.Motion)index);

    // Start is called before the first frame update
    void Start()
    {
        _animCtrl = GetComponent<TitleModelAnimationController>();
        _titleUI = TitleUI.Instance;
        _weaponModels = GetComponentsInChildren<WeaponModel>(true);  
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
