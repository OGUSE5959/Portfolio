using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitAsk : MonoBehaviour
{
    [SerializeField] Button _x;
    [SerializeField] Button _exitBtn;
    void Start()
    {
        _x.onClick.AddListener(() => gameObject.SetActive(false));
        _exitBtn.onClick.AddListener(() => 
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false
#else
        Application.Quit()
#endif
        );
    }
}
