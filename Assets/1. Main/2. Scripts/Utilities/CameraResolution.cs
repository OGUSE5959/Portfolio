using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraResolution : MonoBehaviour
{
    [SerializeField]
    Camera _mainCamera;
    [SerializeField]
    float _fixedWidth;
    [SerializeField]
    float _fixedHeight;
    float _fixedRatio;

    float _currentWidth;
    float _currentHeight;
    float _currentRatio;

    void Awake()
    {
        _mainCamera = Camera.main;

        _fixedRatio = _fixedWidth / _fixedHeight;
        _currentWidth = Screen.width;
        _currentHeight = Screen.height;
        _currentRatio = _currentWidth / _currentHeight;

        if(_currentRatio < _fixedRatio)
        {
            Debug.Log(0);
            var h = _currentRatio / _fixedRatio;
            var y = (1 - h) / 2f;
            _mainCamera.rect = new Rect(0, y, 1, h);
        }
        else if (_fixedRatio < _currentRatio)
        {
            Debug.Log(1);
            var w = _fixedRatio / _currentRatio;
            var x = (1 - w) / 2f;
            _mainCamera.rect = new Rect(x, 0, w, 1);
        }
    }
    /*private void Update()
    {
        _fixedRatio = _fixedWidth / _fixedHeight;
        _currentWidth = Screen.width;
        _currentHeight = Screen.height;
        _currentRatio = _currentWidth / _currentHeight;

        if (_currentRatio < _fixedRatio)
        {
            var h = _currentRatio / _fixedRatio;
            var y = (1 - h) / 2f;
            _mainCamera.rect = new Rect(0, y, 1, h);
        }
        else if (_fixedRatio < _currentRatio)
        {
            var w = _fixedRatio / _currentRatio;
            var x = (1 - w) / 2f;
            _mainCamera.rect = new Rect(x, 0, w, 1);
        }
    }*/
}
