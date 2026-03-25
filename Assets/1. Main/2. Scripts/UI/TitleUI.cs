using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : SingletonMonoBehaviour<TitleUI>
{
    [SerializeField] GameObject _weaponToggle;    
    [SerializeField] GameObject _motionToggle;
    Button[] _weaponBtns;
    Button[] _motionBtns;
    [Space]
    [SerializeField] TitleModel _model;

    public GameObject WeaponToggle => _weaponToggle;
    public GameObject MotionToggle => _motionToggle;

    // Start is called before the first frame update
    void Start()
    {
        _weaponBtns = _weaponToggle.GetComponentsInChildren<Button>();
        _motionBtns = _motionToggle.GetComponentsInChildren<Button>();
        if (!_model) _model = FindAnyObjectByType<TitleModel>();

        for(int i = 0; i < _weaponBtns.Length; i++)
        {
            // if (i >= (int)WeaponID.Max) break;
            var wb = _weaponBtns[i];
            int a = i;
            wb.onClick.AddListener(() =>
            {
                // Debug.Log(a);
                foreach (Button btn in _weaponBtns)
                    btn.GetComponent<Image>().color = Color.white;
                wb.GetComponent<Image>().color = Color.yellow;
                _model.SetWeapon(a);    // 이게 그냥 i로 하면 참조에 의한 호출??
            });
            wb.GetComponentInChildren<Text>().text = ((WeaponID)i).ToString();
        }
        for(int i = 0; i <= _motionBtns.Length; i++)
        {
            if(i >= (int)TitleModelAnimationController.Motion.Max) break;
            var mb = _motionBtns[i];
            int a = i;
            mb.onClick.AddListener(() =>
            {
                foreach(Button btn in _motionBtns)
                    btn.GetComponent<Image>().color = Color.white;
                mb.GetComponent<Image>().color = Color.yellow;
                _model.SetMotion(a);
            });
            mb.GetComponentInChildren<Text>().text = ((TitleModelAnimationController.Motion)i).ToString();
        }
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
